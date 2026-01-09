import asyncio
import json
import os
import random
import re
import math
from datetime import datetime, timezone, timedelta
from pathlib import Path
from typing import Any, Dict, List, Optional

import websockets
from dotenv import load_dotenv
from pydantic import ValidationError

try:
    from openai import AsyncOpenAI
except ImportError:  # pragma: no cover
    AsyncOpenAI = None  # type: ignore

from models import AgentInput
from memory_summarizer import get_recent_actions, get_recent_conversations, get_recent_contacts
from memory_manager import initialize_memory_manager, get_memory_manager


load_dotenv()

OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
SERVER_HOST = os.getenv("LLM_SERVER_HOST", "127.0.0.1")
SERVER_PORT = int(os.getenv("LLM_SERVER_PORT", "8765"))

PROJECT_ROOT = Path(__file__).resolve().parents[1]
LOG_BASE_DIR = PROJECT_ROOT / "Logs" / "llm_decisions"

# サーバー起動時のタイムスタンプでディレクトリを作成
_SESSION_TIMESTAMP = datetime.now().strftime("%Y_%m_%d-%H_%M_%S")
LOG_DIR = LOG_BASE_DIR / _SESSION_TIMESTAMP
LOG_DIR.mkdir(parents=True, exist_ok=True)


def _create_client() -> Optional[AsyncOpenAI]:
    if OPENAI_API_KEY and AsyncOpenAI is not None:
        return AsyncOpenAI(api_key=OPENAI_API_KEY)
    return None


OPENAI_CLIENT = _create_client()

# 長期記憶（RAG）の初期化状態を追跡
_memory_initialized = False

# シナリオコンテキストのキャッシュ
_scenario_context: Optional[Dict[str, Any]] = None


def load_scenario_context() -> Optional[Dict[str, Any]]:
    """
    scenario_context.jsonを読み込む（キャッシュ付き）
    """
    global _scenario_context
    if _scenario_context is not None:
        return _scenario_context

    context_path = Path(__file__).parent / "data" / "scenario_context.json"
    if context_path.exists():
        try:
            with open(context_path, 'r', encoding='utf-8') as f:
                _scenario_context = json.load(f)
            print(f"[LLM SERVER] シナリオコンテキスト読み込み完了: {context_path}")
        except Exception as e:
            print(f"[LLM SERVER] シナリオコンテキスト読み込みエラー: {e}")
            _scenario_context = None
    else:
        print(f"[LLM SERVER] シナリオコンテキストファイルが見つかりません: {context_path}")
    return _scenario_context


def _get_speed_description(speed_multiplier: float) -> str:
    """
    速度倍率を「自信」に関する表現に変換する

    Args:
        speed_multiplier: 速度倍率（1.0が通常速度）

    Returns:
        自信に関する表現（例: "徒歩での移動に自信があります"、"徒歩での移動には自信がありません"）
    """
    if speed_multiplier > 1.0:
        # 速い場合（自信がある）
        if speed_multiplier <= 1.2:
            return "徒歩での移動に自信があります"
        elif speed_multiplier <= 1.5:
            return "徒歩での移動に十分な自信があります"
        elif speed_multiplier <= 2.0:
            return "徒歩での移動に非常に自信があります"
        else:
            return "徒歩での移動に極めて自信があります"
    elif speed_multiplier < 1.0:
        # 遅い場合（自信がない）
        if speed_multiplier >= 0.8:
            return "徒歩での移動には少し不安があります"
        elif speed_multiplier >= 0.5:
            return "徒歩での移動には自信がありません"
        else:
            return "徒歩での移動には全く自信がありません"
    else:
        # 1.0の場合（通常速度）- この場合は何も表示しない
        return None


def build_prompt(payload: Dict[str, Any], agent_input: Optional[AgentInput]) -> str:
    shelters = payload.get("shelter_candidates", [])
    evacuee = payload.get("evacuee", {})
    persona = payload.get("persona")
    scenario_id = payload.get("scenario_id", "mild_quake")

    lines = []

    # ペルソナ情報がある場合は、それを最初に追加
    if persona:
        lines.append("【あなたのペルソナ】")
        lines.append(f"名前: {persona.get('name', '不明')}")
        lines.append(f"役割: {persona.get('role', '不明')}")
        lines.append(f"年齢層: {persona.get('age_group', '不明')}")
        lines.append(f"心理状態: {persona.get('mental_state', '不明')}")
        lines.append(f"優先事項: {persona.get('priority', '不明')}")

        # 拡張フィールド: 生活背景情報
        home_category = persona.get('home_location_category')
        home_elevation = persona.get('home_elevation', 0)
        home_structure = persona.get('home_structure')
        if home_category or home_elevation or home_structure:
            home_category_ja = {
                "coastal": "海岸沿い",
                "hill": "高台",
                "center": "地区中心部"
            }.get(home_category, home_category or "不明")
            structure_ja = {
                "wooden_1story": "木造平屋",
                "wooden_2story": "木造2階建て",
                "rc_2story": "鉄筋コンクリート2階建て",
                "rc_3story": "鉄筋コンクリート3階建て"
            }.get(home_structure, home_structure or "")
            home_desc = f"自宅: {home_category_ja}、海抜{home_elevation}m"
            if structure_ja:
                home_desc += f"、{structure_ja}"
            lines.append(home_desc)

        residence_years = persona.get('residence_years', 0)
        if residence_years > 0:
            lines.append(f"居住歴: この地域に{residence_years}年")

        local_knowledge = persona.get('local_knowledge_level')
        if local_knowledge:
            knowledge_ja = {
                "newcomer": "引っ越してきたばかりで土地勘がない",
                "resident": "数年住んでおり、ある程度土地勘がある",
                "native": "地元出身で土地勘に詳しい"
            }.get(local_knowledge, local_knowledge)
            lines.append(f"土地勘: {knowledge_ja}")

        current_reason = persona.get('current_location_reason')
        if current_reason:
            lines.append(f"現在ここにいる理由: {current_reason}")

        # 家族の状況をfamily_membersから動的生成
        family_members = payload.get("family_members", [])
        if family_members and len(family_members) > 0:
            family_descriptions = []
            for member in family_members:
                relation = member.get("relation", "")
                location = member.get("likely_location", "")
                if relation and location:
                    family_descriptions.append(f"{relation}が{location}にいる")
            if family_descriptions:
                lines.append(f"家族の状況: {' / '.join(family_descriptions)}")
        elif not family_members:
            lines.append("家族の状況: 一人暮らし")

        past_disaster = persona.get('past_disaster_experience')
        if past_disaster:
            lines.append(f"過去の災害経験: {past_disaster}")

        physical_condition = persona.get('physical_condition')
        if physical_condition and physical_condition != "健康":
            lines.append(f"身体状態: {physical_condition}")

        lines.append("")
        lines.append(persona.get("system_prompt_context", ""))
        lines.append("")

    lines.append("【状況】")
    # シナリオIDに応じて状況説明を切り替える
    if scenario_id == "shindo_2":
        lines.append("地震が発生しました。震度は2で、少し揺れたかなという程度です。")
        lines.append("地震の震度: 2")
        lines.append("")
        lines.append("あなたは今、この状況でどう行動するべきか考えています。")
        lines.append("避難するか、それとも別の行動を取るか...")
        lines.append(
            "状況を判断するために一度留まって状況の確認に努めるのも一つの手です。"
        )
    elif scenario_id == "shindo_6":
        lines.append(
            "強い地震が発生しました。震度は6で、室内の物が大きく揺れ、一部は棚から落ちています。"
        )
        lines.append("地震の震度: 6")
        lines.append("建物自体は大きく損壊していませんが、余震の可能性もあります。")
        lines.append("")
        lines.append("あなたは今、自分と家族の安全を最優先に行動する必要があります。")
        lines.append(
            "すぐに避難するか、しばらく建物内で安全を確認しつつ様子を見るかを考えてください。"
        )
    elif scenario_id == "shindo_7_tsunami":
        lines.append(
            "非常に強い地震が発生しました。震度は7クラスで、沿岸部では津波警報が発表されています。"
        )
        lines.append("地震の震度: 7 / 津波警報: 発令中")
        lines.append(
            "あなたのいる地域にも津波が到達する可能性があり、早めの高台への避難が推奨されています。"
        )
        lines.append("")
        lines.append(
            "周囲の状況や家族の所在を考えつつも、津波到達までの時間には限りがあります。"
        )
        lines.append("どこに向かうか、誰を優先するかを慎重かつ素早く判断してください。")
    else:
        # 未知のシナリオIDの場合はデフォルトの軽い地震として扱う
        lines.append(
            "地震が発生しました。揺れはそれほど大きくありませんが、状況を注意深く見守る必要があります。"
        )
        lines.append("地震の震度: 2（デフォルト）")

    # シナリオコンテキストから地域情報を追加
    scenario_context = load_scenario_context()
    if scenario_context:
        # 地域情報
        region = scenario_context.get("region", {})
        if region:
            lines.append("")
            lines.append("【地域情報】")
            lines.append(f"現在地: {region.get('name', '不明')}")
            if region.get('description'):
                lines.append(f"地域の特徴: {region.get('description')}")
            geography = region.get('geography', {})
            if geography.get('elevation_range'):
                lines.append(f"標高範囲: {geography.get('elevation_range')}")

        # 避難の一般ルールと避難所情報の統合
        evac_rules = scenario_context.get("evacuation_rules", {})
        if evac_rules:
            lines.append("")
            lines.append("【避難の一般ルールと避難所】")

            # 一般原則
            principles = evac_rules.get("general_principles", [])
            if principles:
                lines.append("《避難の原則》")
                for principle in principles[:5]:  # 上位5つに制限
                    lines.append(f"・{principle}")

            # 時間制限
            time_limits = evac_rules.get("time_limits", {})
            if time_limits:
                if time_limits.get("tsunami_arrival_estimate"):
                    lines.append(f"※ 津波到達予想: {time_limits['tsunami_arrival_estimate']}")
                if time_limits.get("critical_note"):
                    lines.append(f"※ 重要: {time_limits['critical_note']}")

            # 指定避難所
            designated_shelters = evac_rules.get("designated_shelters", [])
            if designated_shelters:
                lines.append("")
                lines.append("《指定避難所》")
                for shelter in designated_shelters:
                    shelter_line = f"・{shelter.get('name', '不明')}"
                    if shelter.get('elevation_m'):
                        shelter_line += f"（海抜{shelter['elevation_m']}m）"
                    features = shelter.get('features', [])
                    if features:
                        shelter_line += f" - {', '.join(features[:3])}"
                    lines.append(shelter_line)

        # 既知の危険箇所
        hazards = scenario_context.get("known_hazards", [])
        if hazards:
            lines.append("")
            lines.append("《注意すべきエリア》")
            for hazard in hazards[:3]:  # 上位3つに制限
                lines.append(f"・{hazard.get('area', '不明')}: {hazard.get('risk', '')}")

    # 現在の災害状況（environment_stateから動的に）
    environment_state = payload.get("environment_state")
    if environment_state:
        lines.append("")
        lines.append("【現在の災害状況】")
        phase_display = environment_state.get("disaster_phase_display")
        if phase_display:
            lines.append(f"フェーズ: {phase_display}")

        seismic = environment_state.get("seismic_intensity", 0)
        if seismic > 0:
            lines.append(f"震度: {seismic}")

        tsunami_height = environment_state.get("tsunami_height", 0)
        if tsunami_height > 0:
            lines.append(f"津波予想高さ: {tsunami_height}m")

        is_power_on = environment_state.get("is_power_on", True)
        if not is_power_on:
            lines.append("※ 現在停電中")

        is_radio_working = environment_state.get("is_radio_working", True)
        if not is_radio_working:
            lines.append("※ 防災無線が故障中")

        rumble_intensity = environment_state.get("rumble_intensity", 0)
        if rumble_intensity > 0.5:
            lines.append("※ 強い地鳴りが聞こえる")

    lines.append("")
    lines.append("【あなたの現在位置】")
    lines.append(
        f"座標: ({evacuee.get('position', {}).get('x', 0):.2f}, "
        f"{evacuee.get('position', {}).get('z', 0):.2f})"
    )

    # 階層的意思決定の説明を追加
    lines.append("")
    lines.append("【階層的意思決定について】")
    lines.append("あなたの意思決定は、以下の3つの階層で構成されます：")
    lines.append("")
    lines.append(
        "1. 長期目標（Long-term Goal）: 避難所到達、家族合流、安全確保などの長期目標"
    )
    lines.append("   - 災害発生直後や状況が大きく変化した際に更新")
    lines.append("   - Primary Goal: 主要目標")
    lines.append("   - Secondary Goals: 副次目標")
    lines.append("   - Constraints: 制約条件")
    lines.append("")
    lines.append("2. 中期計画（Mid-term Plan）: 長期目標を達成するための具体的な手順")
    lines.append("   - 数分ごと、または新しい情報を受信した際に更新")
    lines.append("   - Steps: 手順のリスト")
    lines.append("   - Contingency: 緊急時の代替案")
    lines.append("")
    lines.append(
        "3. 即時判断（Immediate Decision）: 現在のタイムステップで実行する具体的な行動"
    )
    lines.append("   - 毎タイムステップ（数秒ごと）に更新")
    lines.append("   - 行動タイプ: EVACUATE, STAY, SEARCH_FAMILY, CONTACT, FOLLOW")
    lines.append("")

    # 現在の長期目標と中期計画を表示（存在する場合）
    current_goal = payload.get("current_long_term_goal")
    current_plan = payload.get("current_mid_term_plan")

    if current_goal:
        lines.append("【現在の長期目標】")
        primary_goal = current_goal.get("primary_goal", "未設定")
        if primary_goal and primary_goal != "未設定":
            lines.append(f"主要目標: {primary_goal}")
        else:
            lines.append("主要目標: 未設定")

        secondary_goals = current_goal.get("secondary_goals")
        if secondary_goals and len(secondary_goals) > 0:
            lines.append(f"副次目標: {', '.join(secondary_goals)}")

        constraints = current_goal.get("constraints")
        if constraints and len(constraints) > 0:
            lines.append(f"制約条件: {', '.join(constraints)}")
        lines.append("")

    if current_plan:
        lines.append("【現在の中期計画】")
        steps = current_plan.get("steps")
        if steps and len(steps) > 0:
            for idx, step in enumerate(steps, 1):
                lines.append(f"{idx}. {step}")
        else:
            lines.append("手順: 未設定")

        contingency = current_plan.get("contingency")
        if contingency:
            lines.append(f"緊急時の代替案: {contingency}")
        lines.append("")

    # 更新判断の指示
    if current_goal or current_plan:
        lines.append("【更新判断】")
        lines.append(
            "以下の場合、長期目標を更新してください（should_update_goal = true）："
        )
        lines.append("- 災害発生直後")
        lines.append(
            "- 状況が大きく変化した（例: 津波到達時刻が早まった、家族の位置が判明した）"
        )
        lines.append("")
        lines.append(
            "以下の場合、中期計画を更新してください（should_update_plan = true）："
        )
        lines.append("- 新しい情報を受信した（例: 避難所が満員、道路が通行止め）")
        lines.append("- 数分経過した")
        lines.append("- 現在の計画が実行不可能になった")
        lines.append("")

    if agent_input is not None:
        self_state = agent_input.self_state
        temporal = agent_input.temporal_context
        lines.append("")
        lines.append("【あなたの状態】")

        # 体力（stamina）情報の追加
        stamina = getattr(self_state, 'stamina', 1.0) if self_state else 1.0
        stamina_label = getattr(self_state, 'stamina_label', None)
        if stamina_label is None:
            if stamina >= 0.7:
                stamina_label = "十分"
            elif stamina >= 0.5:
                stamina_label = "普通"
            elif stamina >= 0.3:
                stamina_label = "低下"
            else:
                stamina_label = "疲労"

        lines.append(
            f"体力(スタミナ): {stamina_label}({stamina:.0%}), "
            f"エネルギー: {self_state.energy_label}({self_state.energy_level:.2f}), "
            f"ストレス: {self_state.stress_label}({self_state.stress_level:.2f})"
        )

        # 現在の速度選択と利用可能な選択肢
        current_speed = getattr(self_state, 'current_speed_choice', 'NORMAL')
        available_speeds = getattr(self_state, 'available_speed_choices', None)
        if available_speeds is None:
            # デフォルトの選択肢を体力に基づいて生成
            available_speeds = ["ゆっくり", "普通"]
            if stamina >= 0.3:
                available_speeds.append("急ぎ足")
            if stamina >= 0.5:
                available_speeds.append("走る")

        speed_names = {"SLOW": "ゆっくり", "NORMAL": "普通", "FAST": "急ぎ足", "RUN": "走る"}
        current_speed_display = speed_names.get(current_speed, current_speed)
        lines.append(f"現在の移動速度: {current_speed_display}")
        lines.append(f"選択可能な速度: {', '.join(available_speeds)}")

        if self_state.stress_reason:
            lines.append(f"ストレス要因: {self_state.stress_reason}")
        if self_state.current_goal:
            lines.append(f"現在の目標: {self_state.current_goal}")
        if temporal.time_limit is not None:
            remaining = max(temporal.time_limit - temporal.elapsed_time, 0.0)
            lines.append(
                f"時間制約: 残り{remaining:.1f}秒 / 総量 {temporal.time_limit:.1f}秒"
            )

    # 家族情報を追加
    family_members = payload.get("family_members", [])
    if family_members and len(family_members) > 0:
        lines.append("")
        lines.append("【あなたの家族情報】")
        for member in family_members:
            name = member.get("name", "不明")
            relation = member.get("relation", "不明")
            location = member.get("likely_location", "不明")
            distance = member.get("distance_meters", 0)
            has_phone = member.get("has_phone", False)

            contact_status = "連絡可能" if has_phone else "連絡手段なし"
            member_info = f"- {relation}: {name}（{location}にいる可能性、距離: 約{distance:.0f}m、{contact_status}）"
            lines.append(member_info)

        lines.append("")
        lines.append("※ 家族の安全も考慮して行動を選択してください。")

    # 直近の家族とのやり取り（メールの返信など）があれば追加
    last_contact_message = payload.get("last_contact_message")
    if last_contact_message:
        lines.append("")
        lines.append("【直近の家族からの返信】")
        lines.append("前回、家族から返ってきたメッセージの内容:")
        lines.append(last_contact_message)

    # 会話履歴を追加（TALK行動用）- 直近5件のみ
    conversation_history = payload.get("conversation_history")
    if conversation_history:
        all_conversations = conversation_history.get("recent_conversations", [])
        total_count = conversation_history.get("total_conversation_count", 0)

        # 直近5件のみ取得
        recent_conversations = get_recent_conversations(all_conversations, max_count=5)

        if recent_conversations and len(recent_conversations) > 0:
            lines.append("")
            lines.append("【直近の会話履歴】")
            lines.append(f"これまでに{total_count}回の会話を行っています。直近5件:")
            lines.append("")
            for conv in recent_conversations:
                partner_name = conv.get("partner_name", "不明")
                topic = conv.get("topic", "一般")
                my_message = conv.get("my_message", "")
                partner_response = conv.get("partner_response", "")
                i_initiated = conv.get("i_initiated", True)

                topic_desc = {
                    "ShelterInfo": "避難所情報",
                    "CurrentSituation": "現状確認",
                    "ActionAdvice": "行動相談",
                    "SafetyConfirm": "安全確認",
                    "General": "一般",
                }.get(topic, topic)

                lines.append(f"会話相手: {partner_name}（話題: {topic_desc}）")
                if i_initiated:
                    lines.append(f"自分: {my_message}")
                    lines.append(f"{partner_name}: {partner_response}")
                else:
                    lines.append(f"{partner_name}: {my_message}")
                    lines.append(f"自分: {partner_response}")
                lines.append("")

            lines.append("※ 以前の会話内容も踏まえて次の行動を判断してください。")

    # 行動履歴を追加（短期記憶）- 直近3件のみ
    action_history = payload.get("action_history")
    if action_history:
        all_actions = action_history.get("recent_actions", [])
        total_action_count = action_history.get("total_action_count", 0)

        # 直近3件のみ取得
        recent_actions = get_recent_actions(all_actions, max_count=3)

        if recent_actions and len(recent_actions) > 0:
            lines.append("")
            lines.append("【直近の行動履歴】")
            lines.append(f"これまでに{total_action_count}回の行動判断を行っています。直近3件:")
            lines.append("")

            for action in recent_actions:
                timestamp = action.get("timestamp", 0)
                action_type = action.get("action_type", "不明")
                target = action.get("target", "")
                reasoning = action.get("reasoning", "")
                result = action.get("result", "completed")

                action_desc = {
                    "EVACUATE": "避難",
                    "STAY": "待機",
                    "SEARCH_FAMILY": "家族探索",
                    "CONTACT": "連絡",
                    "FOLLOW": "追従",
                    "TALK": "会話",
                }.get(action_type, action_type)

                line = f"- {timestamp:.0f}秒: {action_desc}"
                if target:
                    line += f"（{target}）"
                if reasoning:
                    # 長い理由は省略
                    short_reasoning = reasoning[:50] + "..." if len(reasoning) > 50 else reasoning
                    line += f" - {short_reasoning}"
                if result != "completed":
                    line += f" [結果: {result}]"

                lines.append(line)

            lines.append("")
            lines.append("※ 過去の行動と一貫性を持ちつつ、現在の状況に適した判断をしてください。")

    # 長期記憶（RAG検索結果）を追加
    long_term_memories = payload.get("long_term_memories", [])
    if long_term_memories and len(long_term_memories) > 0:
        lines.append("")
        lines.append("【あなたの記憶・知識（長期記憶）】")
        lines.append("以下は現在の状況に関連する、あなたが持っている知識や経験です:")
        lines.append("")

        for mem in long_term_memories:
            memory_type = mem.get("memory_type", "unknown")
            content = mem.get("content", "")

            type_label = {
                "regional_knowledge": "地域知識",
                "persona_knowledge": "自己認識",
                "disaster_experience": "過去の経験",
            }.get(memory_type, "記憶")

            lines.append(f"[{type_label}] {content}")

        lines.append("")
        lines.append("※ これらの知識や経験を活かして判断してください。")

    # 周辺避難者情報を追加（FOLLOW行動用）
    nearby_evacuees = payload.get("nearby_evacuees", [])
    nearby_evacuees_count = payload.get("nearby_evacuees_count", 0)
    nearby_evacuees_density = payload.get("nearby_evacuees_density", 0.0)

    if nearby_evacuees_count > 0:
        lines.append("")
        lines.append("【周辺の避難者】")

        # 総人数と混雑度の情報を追加
        if nearby_evacuees_count > 0:
            density_desc = ""
            if nearby_evacuees_density < 1.0:
                density_desc = "（周囲は比較的空いています）"
            elif nearby_evacuees_density < 3.0:
                density_desc = "（周囲に人がいます）"
            elif nearby_evacuees_density < 5.0:
                density_desc = "（周囲は混雑しています）"
            else:
                density_desc = "（周囲は非常に混雑しています）"

            lines.append(
                f"半径30m以内に {nearby_evacuees_count}人の避難者がいます{density_desc}。"
            )
            lines.append("")
            lines.append("近くの避難者の詳細情報（距離順、上位5人）:")

        for idx, evacuee in enumerate(nearby_evacuees, 1):
            evacuee_id = evacuee.get("id", "不明")
            distance = evacuee.get("distance_meters", 0)
            action = evacuee.get("current_action", "不明")
            target_shelter = evacuee.get("target_shelter_id", "")

            action_desc = {
                "EVACUATE": "避難所に移動中",
                "STAY": "その場で待機中",
                "SEARCH_FAMILY": "家族を探し中",
                "CONTACT": "連絡中",
                "FOLLOW": "他の人について行っている",
                "TALK": "会話中",
            }.get(action, action)

            # 避難者の名前・役割・年齢層があれば表示
            evacuee_name = evacuee.get("name", "")
            evacuee_role = evacuee.get("role", "")
            evacuee_age = evacuee.get("age_group", "")

            # 名前と役割があればそれを表示、なければIDを表示
            if evacuee_name:
                name_display = f"{evacuee_name}（{evacuee_role}）" if evacuee_role else evacuee_name
                evacuee_info = (
                    f"{idx}. {name_display} [ID: {evacuee_id}] - 距離: 約{distance:.0f}m、行動: {action_desc}"
                )
            else:
                evacuee_info = (
                    f"{idx}. {evacuee_id} - 距離: 約{distance:.0f}m、行動: {action_desc}"
                )
            if target_shelter:
                evacuee_info += f"（目標: {target_shelter}）"
            lines.append(evacuee_info)

        lines.append("")
        lines.append(
            "※ 避難所の場所が分からない場合や、周囲の人の動きに従いたい場合は、FOLLOW行動を選択できます。"
        )
        lines.append(
            "※ 周囲の避難者に話しかけて情報を得たい場合は、TALK行動を選択できます。"
        )

    # 防災行政無線の放送情報を追加
    has_heard_broadcast = payload.get("has_heard_broadcast", False)
    last_broadcast_message = payload.get("last_broadcast_message", "")
    if has_heard_broadcast and last_broadcast_message:
        lines.append("")
        lines.append("【防災行政無線の放送】")
        lines.append("あなたは屋外スピーカーからの放送を聞きました:")
        lines.append(last_broadcast_message)
        lines.append("")
        lines.append("※ この放送内容を踏まえて行動を判断してください。")

    # Jアラート（スマホの緊急速報）情報を追加
    has_received_j_alert = payload.get("has_received_j_alert", False)
    last_j_alert_message = payload.get("last_j_alert_message", "")
    if has_received_j_alert and last_j_alert_message:
        lines.append("")
        lines.append("【スマホの緊急速報（Jアラート）】")
        lines.append("あなたのスマートフォンに、次のような警報が届きました:")
        lines.append(last_j_alert_message)
        lines.append("")
        lines.append("※ この緊急速報を踏まえて行動を判断してください。")

    # 災害フェーズ情報を追加（DisasterEventManagerから）
    environment_state = payload.get("environment_state")
    if environment_state:
        disaster_phase = environment_state.get("disaster_phase", "")
        disaster_phase_display = environment_state.get("disaster_phase_display", "")

        if disaster_phase_display:
            lines.append("")
            lines.append("【現在の災害フェーズ】")
            lines.append(f"フェーズ: {disaster_phase_display}")

            # フェーズに応じた説明を追加
            phase_descriptions = {
                "Shaking": "現在、強い揺れが続いています。安全な場所で身を守ってください。",
                "InfoGap": "停電により情報が入りにくい状況です。周囲の様子を注意深く観察してください。",
                "WarningIssued": "津波警報が発表されています。高台への避難を検討してください。",
                "PreArrival": "津波が接近しています。直ちに高台へ避難してください。",
                "FirstWave": "津波の第1波が到達しています。絶対に海に近づかないでください。",
                "MainWave": "津波の本波が到達しています。最大限の注意が必要です。",
            }
            if disaster_phase in phase_descriptions:
                lines.append(phase_descriptions[disaster_phase])

            # 環境状態の詳細
            seismic_intensity = environment_state.get("seismic_intensity", 0)
            if seismic_intensity > 0:
                lines.append(f"震度: {seismic_intensity}")

            tsunami_height = environment_state.get("tsunami_height", 0)
            if tsunami_height > 0:
                lines.append(f"津波予想高さ: {tsunami_height}m")

            is_power_on = environment_state.get("is_power_on", True)
            if not is_power_on:
                lines.append("※ 現在、停電中です。テレビ等の情報が得られません。")

            is_radio_working = environment_state.get("is_radio_working", True)
            if not is_radio_working:
                lines.append("※ 防災行政無線が故障しており、放送が聞こえない可能性があります。")

    # 聴覚情報を追加（知覚状態から）
    perception_state = payload.get("perception_state")
    if perception_state:
        audio_info = []

        # 地鳴り
        if perception_state.get("has_heard_rumble", False):
            rumble_intensity = perception_state.get("rumble_intensity", 0)
            if rumble_intensity > 0.7:
                audio_info.append("非常に強い地鳴りが聞こえます。津波が近づいている可能性があります。")
            elif rumble_intensity > 0.3:
                audio_info.append("地鳴りのような音が聞こえます。")

        # サイレン
        if perception_state.get("has_heard_siren", False):
            audio_info.append("津波警報のサイレンが鳴り響いています。")

        # 消防団の呼びかけ
        if perception_state.get("has_heard_fire_truck", False):
            fire_truck_msg = perception_state.get("last_fire_truck_message", "")
            if fire_truck_msg:
                audio_info.append(f"消防団の呼びかけが聞こえました: 「{fire_truck_msg}」")
            else:
                audio_info.append("消防団の呼びかけが聞こえました。")

        if audio_info:
            lines.append("")
            lines.append("【聞こえた音・声】")
            for info in audio_info:
                lines.append(f"- {info}")
            lines.append("")
            lines.append("※ これらの音や声を踏まえて、避難の緊急性を判断してください。")

    # 環境コンテキスト（周辺建物情報）を追加
    env_context = payload.get("environmental_context")

    # デバッグログ: environmental_contextの有無を確認
    if env_context is None:
        print("[LLM SERVER] WARNING: environmental_context is None")
    elif not env_context.get("nearby_buildings"):
        print(
            f"[LLM SERVER] WARNING: environmental_context exists but nearby_buildings is empty or None: {env_context}"
        )
    else:
        print(
            f"[LLM SERVER] INFO: environmental_context received with {len(env_context.get('nearby_buildings', []))} buildings"
        )

    if env_context and env_context.get("nearby_buildings"):
        lines.append("")
        lines.append("【周辺環境の情報】")
        lines.append(
            f"検索範囲: 半径{env_context['search_radius']:.0f}m以内に"
            f"{env_context['total_buildings_in_area']}件の建物があります"
        )
        lines.append("")
        lines.append("近くの建物（距離順、上位10件）:")

        for idx, building in enumerate(env_context["nearby_buildings"], 1):
            usage = building.get("usage", "不明")
            distance = building.get("distance", 0)
            height = building.get("height", 0)
            floors = building.get("floors", 0)
            major_usage = building.get("major_usage", "")
            structure_type = building.get("structure_type", "")

            lines.append(f"{idx}. {usage} (距離: {distance:.1f}m)")

            details = []
            # 9999は「不明」を意味するため、1階建てとして扱う
            display_floors = 1 if floors == 9999 else floors
            if display_floors > 0:
                details.append(f"{display_floors}階建て")
            if height > 0:
                details.append(f"高さ{height:.1f}m")
            if structure_type:
                details.append(f"{structure_type}")
            if major_usage:
                details.append(f"用途: {major_usage}")

            if details:
                lines.append(f"   {' / '.join(details)}")

        lines.append("")
        lines.append("※ 周辺環境も考慮して避難所を選択してください。")
        lines.append(
            "   例: 近くに公共施設（官公庁、学校など）があれば、"
            "そこが避難所として指定されている可能性が高い"
        )

    # 環境認識情報を追加（避難者が感じ取れる・知っている情報として表現）
    if env_context:
        lines.append("")
        lines.append("【今いる場所の様子】")

        terrain_perceptions = []
        location_awareness = []

        # 標高に基づく環境認識（数値ではなく感覚的な表現）
        elevation = env_context.get("current_elevation", -1)
        if elevation >= 0:
            if elevation < 5:
                terrain_perceptions.append("海や川がすぐ近くに見える低い場所だ")
                terrain_perceptions.append("津波が来たらすぐに水に浸かりそうな気がする")
            elif elevation < 15:
                terrain_perceptions.append("それほど高くない場所にいる")
                terrain_perceptions.append("海からの距離を考えると、津波の時は不安だ")
            elif elevation < 30:
                terrain_perceptions.append("少し高い場所にいるようだ")
            else:
                terrain_perceptions.append("かなり高い場所にいる")
                terrain_perceptions.append("ここなら津波の心配は少なそうだ")

        # 津波リスク区域の認識（地元民としての土地勘）
        if env_context.get("is_in_tsunami_zone"):
            location_awareness.append("この辺りは昔から「津波が来たら危ない場所」と言われている")
            depth = env_context.get("tsunami_estimated_depth", 0)
            if depth >= 5:
                location_awareness.append("大津波が来れば、建物の2階でも危ないかもしれない")
            elif depth >= 2:
                location_awareness.append("津波が来れば1階は水に浸かるだろう")

        # 土砂災害リスクの認識（地震による二次災害の判断）
        if env_context.get("is_in_special_landslide_zone"):
            landslide_type = env_context.get("landslide_type", "")
            # 地形の視覚的認識
            terrain_perceptions.append("すぐ近くに急な崖や斜面が見える")
            # 地震後の二次災害への認識
            location_awareness.append("さっきの地震で地盤が緩んでいるかもしれない")
            location_awareness.append("余震が来たら崩れる危険がある。ここは早く離れた方がいい")
            if "急傾斜" in landslide_type:
                location_awareness.append("この崖は地震の揺れで崩れやすくなっているはずだ")
                terrain_perceptions.append("崖の上から小石がパラパラ落ちてきている気がする")
            elif "土石流" in landslide_type:
                location_awareness.append("山の上から土砂が流れてくるかもしれない")
                location_awareness.append("この谷筋は危険だ。横方向に逃げた方がいい")
            elif "地すべり" in landslide_type:
                location_awareness.append("この斜面全体が動き出すかもしれない")
                location_awareness.append("地面にひび割れがないか注意しながら進もう")
        elif env_context.get("is_in_landslide_zone"):
            landslide_type = env_context.get("landslide_type", "")
            terrain_perceptions.append("近くに斜面や崖がある")
            location_awareness.append("地震の後は土砂崩れが起きやすい。注意が必要だ")
            if "急傾斜" in landslide_type:
                location_awareness.append("あの崖沿いは通らない方が安全だろう")
            elif "土石流" in landslide_type:
                location_awareness.append("沢沿いの道は避けた方がいいかもしれない")
            elif "地すべり" in landslide_type:
                location_awareness.append("斜面の様子に気をつけながら進もう")

        # 土地利用の認識（見てわかる情報）
        land_use = env_context.get("current_land_use", "")
        if land_use:
            if "住宅" in land_use:
                terrain_perceptions.append("住宅が立ち並ぶ場所だ")
            elif "商業" in land_use:
                terrain_perceptions.append("お店や建物が多い場所だ")
            elif "田" in land_use or "畑" in land_use or "農" in land_use:
                terrain_perceptions.append("田畑が広がる開けた場所だ")
            elif "森林" in land_use or "山林" in land_use:
                terrain_perceptions.append("木々に囲まれた場所だ")

        # 道路の認識（見てわかる情報）
        road_type = env_context.get("nearest_road_type", "")
        if road_type:
            if "国道" in road_type or "高速" in road_type:
                terrain_perceptions.append("大きな道路が近くにある")
            elif "県道" in road_type or "都道府県道" in road_type:
                terrain_perceptions.append("県道沿いにいる")

        # 環境認識を出力
        if terrain_perceptions:
            for perception in terrain_perceptions:
                lines.append(f"- {perception}")

        # 危険認識を出力（土地勘に基づく）
        if location_awareness:
            lines.append("")
            lines.append("【この場所について知っていること】")
            for awareness in location_awareness:
                lines.append(f"- {awareness}")

    lines.append("")

    lines.append("")
    lines.append("【近くの避難先情報】")
    for idx, shelter in enumerate(shelters, 1):
        # 基本情報
        shelter_id = shelter.get("id", "不明")
        display_name = shelter.get("display_name", shelter_id)
        description = shelter.get("description", "")
        capacity = shelter.get("current_capacity", 0)
        max_capacity = shelter.get("max_capacity", 0)

        # 距離情報
        distance_m = shelter.get("distance_meters", 0)
        walking_time = shelter.get("walking_time_minutes", 0)

        # 避難先タイプと海抜情報
        destination_type = shelter.get("destination_type", "shelter")
        elevation_meters = shelter.get("elevation_meters", 0)

        # 避難先タイプに応じた表示
        if destination_type == "tsunami_evacuation_area":
            type_label = "【津波避難地域】"
            # 収容制限なし（int.MaxValueが送られてくる）
            if capacity >= 2147483647:
                capacity_info = "収容制限なし"
            else:
                capacity_info = f"残り受け入れ可能人数: {capacity}人"
            # 海抜情報
            elevation_info = f"海抜: {elevation_meters:.0f}m, " if elevation_meters > 0 else ""
        else:
            type_label = "【避難所】"
            capacity_info = f"残り受け入れ可能人数: {capacity}人"
            elevation_info = ""

        # 基本情報の表示（タイプラベル + 表示名）
        shelter_info = f"{idx}. {type_label} {display_name}"

        # 説明がある場合は追加
        if description:
            shelter_info += f"（{description}）"

        # 海抜・収容人数を追加
        shelter_info += f" - {elevation_info}{capacity_info}"

        # 距離と徒歩時間を追加
        if distance_m > 0:
            shelter_info += f", 距離: 約{distance_m:.0f}m（徒歩約{walking_time:.0f}分）"

        lines.append(shelter_info)

    # ペルソナ情報に基づく追加指示
    if persona:
        speed_multiplier = persona.get("speed_multiplier", 1.0)
        speed_desc = _get_speed_description(speed_multiplier)
        if speed_desc is not None:
            if speed_multiplier < 1.0:
                lines.append(
                    f"注意: {speed_desc}。無理をしない範囲で避難してください。"
                )
            elif speed_multiplier > 1.0:
                lines.append(f"{speed_desc}。状況に応じて速度を調整してください。")

    lines.append("")
    lines.append("【行動選択】")
    lines.append("以下の行動から1つ選択してください:")
    lines.append("")
    lines.append("1. EVACUATE - 避難所に向かう")
    lines.append("   避難所に移動します。")
    lines.append("")
    lines.append("2. STAY - その場で待機する")
    lines.append("   避難所に移動する必要がない場合、その場で待機します。")
    lines.append("")
    lines.append("3. SEARCH_FAMILY - 家族を探しに行く")
    lines.append("   家族がいそうな場所（自宅・学校・職場など）に向かいます。")
    lines.append("")
    lines.append("4. CONTACT - 家族に連絡を取る（メールなど）")
    lines.append("   電話やメールで家族に安否確認を行います。")
    lines.append("")
    lines.append("5. FOLLOW - 周囲の避難者について行く")
    lines.append(
        "   避難所の場所が分からない場合や、周囲の人の動きに従いたい場合に選択します。"
    )
    lines.append("   周辺の避難者情報を参考に、誰について行くかを決定してください。")
    lines.append("")
    lines.append("6. TALK - 周辺の避難者と会話する")
    lines.append(
        "   近くにいる避難者に話しかけて、避難所の情報や現状について尋ねます。"
    )
    lines.append(
        "   話しかけた相手は、自分の状況に基づいて返答します（相手のLLMが返答を生成）。"
    )
    lines.append(
        "   会話の話題: ShelterInfo（避難所情報）、CurrentSituation（現状確認）、"
    )
    lines.append(
        "              ActionAdvice（行動相談）、SafetyConfirm（安全確認）、General（一般）"
    )
    lines.append("")
    lines.append("【重要】以下のJSON形式で回答してください。")
    lines.append("")
    lines.append("■ EVACUATE を選択する場合:")
    lines.append(
        '{"action_type": "EVACUATE", "selected_shelter_id": "避難所の名前", '
        '"reasoning": "短い説明", "confidence": 0.0-1.0, "desired_speed": "ゆっくり/普通/急ぎ足/走る のいずれか"}'
    )
    lines.append("")
    lines.append("■ STAY を選択する場合:")
    lines.append(
        '{"action_type": "STAY", "reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("■ SEARCH_FAMILY を選択する場合:")
    lines.append(
        '{"action_type": "SEARCH_FAMILY", '
        '"target_family_member": "探しに行く家族の名前または続柄（例: 息子）", '
        '"reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("■ CONTACT を選択する場合:")
    lines.append(
        '{"action_type": "CONTACT", '
        '"contact_target": "連絡する家族の名前または続柄（例: 妻）", '
        '"contact_message": "家族に送る短いメール本文（口語的な日本語）", '
        '"reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("■ FOLLOW を選択する場合:")
    lines.append(
        '{"action_type": "FOLLOW", '
        '"target_evacuee_id": "周辺避難者のID（上記の【周辺の避難者】に表示されているID）", '
        '"reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("■ TALK を選択する場合:")
    lines.append(
        '{"action_type": "TALK", '
        '"talk_target_id": "周辺避難者のID（上記の【周辺の避難者】に表示されているID）", '
        '"talk_topic": "ShelterInfo/CurrentSituation/ActionAdvice/SafetyConfirm/General のいずれか", '
        '"talk_message": "話しかける内容（口語的な日本語）", '
        '"reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("■ 階層的意思決定を含む場合（推奨）:")
    lines.append(
        '{"action_type": "EVACUATE", '
        '"selected_shelter_id": "避難所の名前", '
        '"long_term_goal": {'
        '  "primary_goal": "家族全員で○○避難所に到達する", '
        '  "secondary_goals": ["途中で怪我をしない", "できるだけ早く到達する"], '
        '  "constraints": ["高齢の母を連れているため、階段の多い経路は避ける"]'
        "}, "
        '"mid_term_plan": {'
        '  "steps": ["まず自宅から○○小学校に移動し、子供を迎える", "次に○○交差点を経由して△△避難所に向かう"], '
        '  "contingency": "もし津波が早く来そうなら、子供を迎えずに最寄りの高台に避難する"'
        "}, "
        '"should_update_goal": true, '
        '"should_update_plan": true, '
        '"reasoning": "短い説明", "confidence": 0.0-1.0}'
    )
    lines.append("")
    lines.append("注意事項:")
    lines.append(
        "- action_type は必ず「EVACUATE」「STAY」「SEARCH_FAMILY」「CONTACT」「FOLLOW」「TALK」のいずれかを指定してください"
    )
    lines.append(
        "- EVACUATE の場合、selected_shelter_idには必ず上記に表示されている避難所の名前を正確に指定"
    )
    lines.append(
        "  例: 「豊間小学校」「いわき市役所」など（「1」「2」などの番号は使用不可）"
    )
    lines.append(
        "- CONTACT を選ぶ場合、contact_message には実際に家族に送るつもりの短いメール本文を書いてください"
    )
    lines.append(
        "- TALK を選ぶ場合、talk_message には実際に相手に話しかけるつもりの内容を書いてください"
    )
    lines.append("- desired_speedは体力状態に応じて選択してください:")
    lines.append("  - ゆっくり: 体力を回復しながらゆっくり歩く")
    lines.append("  - 普通: 通常の歩行速度")
    lines.append("  - 急ぎ足: 早歩き（体力を消費、体力30%以上で選択可）")
    lines.append("  - 走る: 全力で走る（体力を大きく消費、体力50%以上で選択可）")

    return "\n".join(lines)


def _position_tuple(info: Dict[str, Any]) -> tuple[float, float, float]:
    pos = info.get("position", {}) if info else {}
    return (
        float(pos.get("x", 0.0)),
        float(pos.get("y", 0.0)),
        float(pos.get("z", 0.0)),
    )


def _extract_float(value: Any) -> Optional[float]:
    if value is None:
        return None
    try:
        number = float(value)
        if math.isnan(number) or math.isinf(number):
            return None
        return number
    except (TypeError, ValueError):
        return None


def _normalize_speed_choice(value: Any) -> Optional[str]:
    """
    速度選択肢を正規化する（日本語/英語両方対応）

    Args:
        value: LLMからのdesired_speed値（"ゆっくり", "SLOW", "急ぎ足", "FAST"など）

    Returns:
        正規化された速度選択肢（"SLOW", "NORMAL", "FAST", "RUN"）またはNone
    """
    if value is None:
        return "NORMAL"

    value_str = str(value).strip()
    if not value_str:
        return "NORMAL"

    # 日本語名と英語名のマッピング（大文字小文字を区別しない）
    speed_map = {
        "ゆっくり": "SLOW",
        "普通": "NORMAL",
        "急ぎ足": "FAST",
        "走る": "RUN",
        "slow": "SLOW",
        "normal": "NORMAL",
        "fast": "FAST",
        "run": "RUN",
    }

    normalized = speed_map.get(value_str.lower())
    if normalized:
        return normalized

    # 完全一致しない場合、大文字に変換して確認
    upper_value = value_str.upper()
    if upper_value in ["SLOW", "NORMAL", "FAST", "RUN"]:
        return upper_value

    return "NORMAL"


def heuristic_selection(payload: Dict[str, Any]) -> Optional[str]:
    shelters = payload.get("shelter_candidates", [])
    evacuee = payload.get("evacuee", {})
    if not shelters or not evacuee:
        return None

    ex, ey, ez = _position_tuple(evacuee)
    best_id = None
    best_distance = float("inf")

    for entry in shelters:
        capacity = entry.get("current_capacity", 0)
        if capacity <= 0:
            continue
        sx, sy, sz = _position_tuple(entry)
        distance = (ex - sx) ** 2 + (ez - sz) ** 2 + (ey - sy) ** 2
        if distance < best_distance:
            best_distance = distance
            best_id = entry.get("id")

    if best_id is None and shelters:
        best_id = random.choice(shelters).get("id")

    return best_id


def _normalize_temporal(raw: Dict[str, Any]) -> Dict[str, Any]:
    normalized = dict(raw)
    has_limit = normalized.pop("has_time_limit", None)
    if has_limit is not None and not has_limit:
        normalized["time_limit"] = None
    return normalized


def _build_agent_input(payload: Dict[str, Any]) -> Optional[AgentInput]:
    self_state = payload.get("self_state")
    temporal = payload.get("temporal_context") or payload.get("temporal")
    if self_state is None or temporal is None:
        return None
    try:
        return AgentInput.model_validate(
            {
                "self_state": self_state,
                "temporal_context": _normalize_temporal(temporal),
            }
        )
    except ValidationError as exc:  # pragma: no cover - logging only
        print(f"[LLM SERVER] AgentInput validation failed: {exc}")
        return None


async def _call_openai_with_retry(
    prompt: str, max_retries: int = 3, base_delay: float = 1.0
) -> Optional[str]:
    """
    Exponential backoffによるリトライ機能付きでOpenAI APIを呼び出す。

    Args:
        prompt: LLMに送信するプロンプト
        max_retries: 最大リトライ回数
        base_delay: 初回リトライまでの待機時間（秒）

    Returns:
        LLMの応答テキスト、または失敗時はNone
    """
    from openai import RateLimitError, APIError, APIConnectionError

    last_exception = None
    for attempt in range(max_retries + 1):
        try:
            response = await OPENAI_CLIENT.chat.completions.create(
                model=OPENAI_MODEL,
                messages=[{"role": "user", "content": prompt}],
                response_format={"type": "json_object"},
            )
            return response.choices[0].message.content
        except RateLimitError as e:
            last_exception = e
            if attempt < max_retries:
                delay = base_delay * (2**attempt)  # exponential backoff
                print(
                    f"[LLM SERVER] RateLimitError, retrying in {delay:.1f}s (attempt {attempt + 1}/{max_retries})"
                )
                await asyncio.sleep(delay)
            else:
                print(f"[LLM SERVER] RateLimitError, max retries exceeded: {e}")
        except (APIError, APIConnectionError) as e:
            last_exception = e
            if attempt < max_retries:
                delay = base_delay * (2**attempt)
                print(
                    f"[LLM SERVER] API error, retrying in {delay:.1f}s (attempt {attempt + 1}/{max_retries}): {e}"
                )
                await asyncio.sleep(delay)
            else:
                print(f"[LLM SERVER] API error, max retries exceeded: {e}")
        except Exception as e:
            # その他の例外はリトライしない
            print(f"[LLM SERVER] Unexpected error: {e}")
            return None

    return None


async def call_openai(
    payload: Dict[str, Any], agent_input: Optional[AgentInput]
) -> Optional[Dict[str, Any]]:
    if OPENAI_CLIENT is None:
        return None

    prompt = build_prompt(payload, agent_input)
    try:
        content = await _call_openai_with_retry(prompt)
        if content is None:
            return None
        decision = _safe_load_json(content)

        # action_typeが存在するか、または従来の形式（selected_shelter_idのみ）か確認
        action_type = decision.get("action_type", "EVACUATE")

        # EVACUATEの場合はselected_shelter_idが必須
        if action_type == "EVACUATE":
            if "selected_shelter_id" in decision:
                return decision
            else:
                print(
                    f"[LLM SERVER] EVACUATE行動だがselected_shelter_idがありません: {decision}"
                )
                return None

        # STAY, SEARCH_FAMILY, CONTACT, FOLLOW の場合はaction_typeがあればOK
        if "action_type" in decision:
            action_type = decision.get("action_type")
            # FOLLOWの場合はtarget_evacuee_idが必要
            if action_type == "FOLLOW":
                if "target_evacuee_id" not in decision:
                    print(
                        f"[LLM SERVER] FOLLOW行動だがtarget_evacuee_idがありません: {decision}"
                    )
                    return None

            # TALKの場合はtalk_target_idとtalk_messageが必要
            if action_type == "TALK":
                if "talk_target_id" not in decision:
                    print(
                        f"[LLM SERVER] TALK行動だがtalk_target_idがありません: {decision}"
                    )
                    return None
                if "talk_message" not in decision:
                    print(
                        f"[LLM SERVER] TALK行動だがtalk_messageがありません: {decision}"
                    )
                    return None

            # 長期目標と中期計画の検証
            if decision.get("should_update_goal"):
                goal = decision.get("long_term_goal")
                if goal and not goal.get("primary_goal"):
                    print(
                        f"[LLM SERVER] WARNING: should_update_goal=true but long_term_goal.primary_goal is missing"
                    )

            if decision.get("should_update_plan"):
                plan = decision.get("mid_term_plan")
                if plan and not plan.get("steps"):
                    print(
                        f"[LLM SERVER] WARNING: should_update_plan=true but mid_term_plan.steps is missing"
                    )

            return decision

        # 従来の形式（action_typeなし、selected_shelter_idあり）もサポート
        if "selected_shelter_id" in decision:
            decision["action_type"] = "EVACUATE"  # デフォルトで追加
            return decision

    except Exception as exc:  # pragma: no cover
        print(f"[LLM SERVER] OpenAI呼び出しで例外: {exc}")
    return None


def _safe_load_json(text: str) -> Dict[str, Any]:
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        match = re.search(r"\{.*\}", text, re.DOTALL)
        if match:
            return json.loads(match.group(0))
        raise


def _sanitize_filename(name: Optional[str]) -> str:
    if not name:
        return "unknown"
    safe = re.sub(r"[^a-zA-Z0-9_.-]", "_", name)
    return safe or "unknown"


def _log_decision(
    evacuee_id: Optional[str],
    request_id: str,
    source: str,
    input_snapshot: Dict[str, Any],
    output_snapshot: Dict[str, Any],
    prompt: Optional[str] = None,
) -> None:
    try:
        sanitized_id = _sanitize_filename(evacuee_id)

        # 日本時間（JST = UTC+9）で秒までのタイムスタンプを生成
        jst = timezone(timedelta(hours=9))
        timestamp_jst = datetime.now(jst).strftime("%Y-%m-%d %H:%M:%S")

        # 読みやすい形式のログファイル（.txt）
        log_filename = LOG_DIR / f"{sanitized_id}.txt"

        # promptを除いたメタデータ
        log_meta = {
            "timestamp": timestamp_jst,
            "request_id": request_id,
            "source": source,
            "input": input_snapshot,
            "output": output_snapshot,
        }

        with log_filename.open("a", encoding="utf-8") as fp:
            # 区切り線
            fp.write("=" * 80 + "\n")

            # メタデータをJSON形式で出力
            fp.write(json.dumps(log_meta, ensure_ascii=False, indent=2))
            fp.write("\n\n")

            # プロンプトを別セクションとして出力（改行をそのまま表示）
            if prompt:
                fp.write("-" * 80 + "\n")
                fp.write("[PROMPT]\n")
                fp.write("-" * 80 + "\n")
                fp.write(prompt)
                fp.write("\n\n")
    except Exception as exc:  # pragma: no cover
        print(f"[LLM SERVER] failed to write log: {exc}")


async def process_payload(payload: Dict[str, Any]) -> Dict[str, Any]:
    request_id = payload.get("request_id", f"req-{random.randint(0, 1_000_000)}")

    # 長期記憶（RAG）検索
    memory_manager = get_memory_manager()
    if memory_manager and memory_manager.is_initialized:
        try:
            # 現在の状況からクエリを構築
            evacuee = payload.get("evacuee", {})
            persona = payload.get("persona", {})
            scenario_id = payload.get("scenario_id", "")
            agent_id = evacuee.get("id")

            # agent_idを整数に変換（可能な場合）
            agent_id_int = None
            if agent_id is not None:
                try:
                    # "evacuee_1" のような形式からIDを抽出
                    if isinstance(agent_id, str) and "_" in agent_id:
                        agent_id_int = int(agent_id.split("_")[-1])
                    elif isinstance(agent_id, (int, float)):
                        agent_id_int = int(agent_id)
                except (ValueError, IndexError):
                    pass

            # 検索クエリを構築
            query_parts = []
            if scenario_id == "shindo_7_tsunami":
                query_parts.append("津波 避難 地震 高台")
            elif scenario_id == "shindo_6":
                query_parts.append("地震 避難 揺れ")
            else:
                query_parts.append("地震 避難")

            # ペルソナ情報があれば追加
            if persona:
                role = persona.get("role", "")
                if role:
                    query_parts.append(role)

            query = " ".join(query_parts)

            # 長期記憶を検索
            memories = await memory_manager.search(
                query=query,
                agent_id=agent_id_int,
                top_k=3,
                threshold=0.7
            )

            if memories:
                payload["long_term_memories"] = memories
                print(f"[LLM SERVER] 長期記憶を{len(memories)}件取得しました")

        except Exception as e:
            print(f"[LLM SERVER] 長期記憶検索でエラー: {e}")

    # デバッグログ: 受信したペイロードのキーを確認
    print(f"[LLM SERVER] Received payload keys: {list(payload.keys())}")
    if "environmental_context" in payload:
        env_ctx = payload["environmental_context"]
        if env_ctx:
            print(
                f"[LLM SERVER] environmental_context type: {type(env_ctx)}, keys: {list(env_ctx.keys()) if isinstance(env_ctx, dict) else 'N/A'}"
            )
        else:
            print(
                f"[LLM SERVER] environmental_context is present but value is: {env_ctx}"
            )
    else:
        print("[LLM SERVER] environmental_context NOT in payload")

    agent_input = _build_agent_input(payload)

    # LLMに与えるプロンプトを生成（ログに記録するため）
    prompt = build_prompt(payload, agent_input)

    decision = await call_openai(payload, agent_input)
    evacuee = payload.get("evacuee", {})

    # input_snapshotは最小限に（shelter_candidatesとpersonaはpromptに含まれているため省略）
    input_snapshot = {
        "agent_input": agent_input.model_dump() if agent_input else None,
    }

    if decision is None:
        # フォールバック: LLMが失敗した場合は最寄りの避難所を選択
        selected_id = heuristic_selection(payload)
        reasoning = "Fallback heuristic: closest shelter with spare capacity."
        confidence = 0.5
        source = "heuristic"
        desired_speed = None
        action_type = "EVACUATE"
    else:
        # LLMの決定を取得
        action_type = decision.get("action_type", "EVACUATE")
        reasoning = decision.get("reasoning", "LLM decision")
        confidence = float(decision.get("confidence", 0.5))
        source = "llm"
        desired_speed = _normalize_speed_choice(decision.get("desired_speed"))

        # EVACUATEの場合はselected_shelter_idが必要
        if action_type == "EVACUATE":
            selected_id = decision.get("selected_shelter_id") or heuristic_selection(
                payload
            )
        else:
            # STAY等の場合はselected_shelter_idは不要
            selected_id = decision.get("selected_shelter_id")

    # EVACUATEだがselected_idがない場合のフォールバック
    if (
        action_type == "EVACUATE"
        and selected_id is None
        and payload.get("shelter_candidates")
    ):
        selected_id = payload["shelter_candidates"][0].get("id")

    response_payload = {
        "request_id": request_id,
        "evacuee_id": evacuee.get("id"),
        "action_type": action_type,
        "selected_shelter_id": selected_id,
        "reasoning": reasoning,
        "confidence": confidence,
        "desired_speed": desired_speed,
    }

    # TALKの場合は追加フィールドを含める
    if decision and action_type == "TALK":
        response_payload["talk_target_id"] = decision.get("talk_target_id")
        response_payload["talk_topic"] = decision.get("talk_topic", "General")
        response_payload["talk_message"] = decision.get("talk_message")

    # FOLLOWの場合は追加フィールドを含める
    if decision and action_type == "FOLLOW":
        response_payload["target_evacuee_id"] = decision.get("target_evacuee_id")

    # 注: 行動履歴の要約（LLM API呼び出し）は削除
    # レート制限を回避するため、直近3件の履歴をそのまま使用する方式に変更

    output_snapshot = {
        "action_type": action_type,
        "reasoning": reasoning,
        "confidence": confidence,
    }
    if selected_id is not None:
        output_snapshot["selected_shelter_id"] = selected_id
    if desired_speed is not None:
        output_snapshot["desired_speed"] = desired_speed
    # llm_raw_responseの記録は現在不要なため無効化
    # if decision is not None:
    #     output_snapshot["llm_raw_response"] = decision

    _log_decision(
        evacuee_id=evacuee.get("id"),
        request_id=request_id,
        source=source,
        input_snapshot=input_snapshot,
        output_snapshot=output_snapshot,
        prompt=prompt,  # プロンプトをログに追加
    )

    return response_payload


def build_conversation_response_prompt(payload: Dict[str, Any]) -> str:
    """
    会話応答生成用のプロンプトを構築する。
    話しかけられた避難者が、相手にどう返答するかを決定するためのプロンプト。
    """
    lines = []

    persona = payload.get("persona", {})
    environment = payload.get("environment", {})
    incoming = payload.get("incoming_conversation", {})
    current_action = payload.get("current_action", "不明")
    current_target = payload.get("current_target", "")

    # 話しかけられた側の情報
    lines.append("あなたは避難者です。他の避難者から話しかけられました。")
    lines.append("")
    lines.append("【あなたの情報】")
    lines.append(f"名前: {persona.get('name', '不明')}")
    lines.append(f"役割: {persona.get('role', '不明')}")
    lines.append(f"年齢層: {persona.get('age_group', '不明')}")

    # 現在の行動
    action_desc = {
        "EVACUATE": "避難所に向かっている",
        "STAY": "その場で待機している",
        "SEARCH_FAMILY": "家族を探している",
        "CONTACT": "家族に連絡を取っている",
        "FOLLOW": "他の避難者について行っている",
        "TALK": "他の避難者と会話中",
    }.get(current_action, current_action)

    lines.append(f"現在の行動: {action_desc}")
    if current_target:
        lines.append(f"目標避難所: {current_target}")
    lines.append("")

    # 環境情報
    scenario_id = environment.get("scenario_id", "")
    if scenario_id == "shindo_7_tsunami":
        lines.append("【状況】")
        lines.append("震度7クラスの地震が発生し、津波警報が発令中です。")
        lines.append("")

    # 話しかけてきた人の情報
    lines.append("【話しかけてきた人】")
    lines.append(f"名前: {incoming.get('initiator_name', '不明')}")
    lines.append(f"メッセージ: 「{incoming.get('message', '')}」")

    topic = incoming.get("topic", "General")
    topic_desc = {
        "ShelterInfo": "避難所情報",
        "CurrentSituation": "現状確認",
        "ActionAdvice": "行動相談",
        "SafetyConfirm": "安全確認",
        "General": "一般",
    }.get(topic, topic)
    lines.append(f"話題: {topic_desc}")
    lines.append("")

    # 指示
    lines.append("あなたの立場で、この人にどう返答しますか？")
    lines.append("自分の知っている情報を適切に共有してください。")
    lines.append("ただし、自分も避難中で時間がない場合は、簡潔に答えてください。")
    lines.append("")
    lines.append("【重要】以下のJSON形式で回答してください:")
    lines.append('{')
    lines.append('  "response_message": "返答内容（口語的な日本語）",')
    lines.append('  "willing_to_share": true または false（情報を共有する意思があるか）,')
    lines.append('  "reasoning": "この返答をした理由"')
    lines.append('}')

    return "\n".join(lines)


async def process_conversation_response(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    会話応答生成リクエストを処理する。
    話しかけられた避難者のLLMが返答を生成する。
    """
    request_id = payload.get("request_id", f"conv-{random.randint(0, 1_000_000)}")

    if OPENAI_CLIENT is None:
        # LLMが利用できない場合はデフォルト応答
        return {
            "request_id": request_id,
            "response_message": "すみません、今は急いでいるので...",
            "willing_to_share": False,
            "reasoning": "LLM unavailable - default response",
        }

    prompt = build_conversation_response_prompt(payload)

    try:
        content = await _call_openai_with_retry(prompt)
        if content is None:
            return {
                "request_id": request_id,
                "response_message": "すみません、今は急いでいるので...",
                "willing_to_share": False,
                "reasoning": "LLM call failed - default response",
            }

        response = _safe_load_json(content)

        return {
            "request_id": request_id,
            "response_message": response.get("response_message", "..."),
            "willing_to_share": response.get("willing_to_share", True),
            "reasoning": response.get("reasoning", ""),
        }

    except Exception as exc:
        print(f"[LLM SERVER] 会話応答生成で例外: {exc}")
        return {
            "request_id": request_id,
            "response_message": "すみません、今は急いでいるので...",
            "willing_to_share": False,
            "reasoning": f"Exception: {exc}",
        }


def build_family_contact_response_prompt(payload: Dict[str, Any]) -> str:
    """
    家族連絡応答生成用のプロンプトを構築する。
    連絡を受けた家族メンバーが、送信者にどう返答するかを決定するためのプロンプト。
    """
    lines = []

    persona = payload.get("persona", {})
    environment = payload.get("environment", {})
    incoming = payload.get("incoming_contact", {})
    current_action = payload.get("current_action", "不明")
    current_target = payload.get("current_target", "")
    family_relationship = payload.get("family_relationship", "")

    # 連絡を受けた家族の情報
    lines.append("あなたは家族から緊急連絡（メール）を受けました。")
    lines.append("")
    lines.append("【あなたの情報】")
    lines.append(f"名前: {persona.get('name', '不明')}")
    lines.append(f"役割: {persona.get('role', '不明')}")
    lines.append(f"年齢層: {persona.get('age_group', '不明')}")
    if family_relationship:
        lines.append(f"家族関係: {family_relationship}")

    # 現在の行動
    action_desc = {
        "EVACUATE": "避難所に向かっている",
        "STAY": "その場で待機している",
        "SEARCH_FAMILY": "家族を探している",
        "CONTACT": "家族に連絡を取っている",
        "FOLLOW": "他の避難者について行っている",
        "TALK": "他の避難者と会話中",
    }.get(current_action, current_action)

    lines.append(f"現在の行動: {action_desc}")
    if current_target:
        lines.append(f"目標避難所: {current_target}")
    lines.append("")

    # 環境情報
    scenario_id = environment.get("scenario_id", "")
    if scenario_id == "shindo_7_tsunami":
        lines.append("【状況】")
        lines.append("震度7クラスの地震が発生し、津波警報が発令中です。")
        lines.append("")

    # 連絡してきた家族の情報
    lines.append("【連絡してきた家族】")
    lines.append(f"名前: {incoming.get('sender_name', '不明')}")
    if incoming.get("sender_relation"):
        lines.append(f"続柄: {incoming.get('sender_relation')}")
    lines.append(f"メッセージ: 「{incoming.get('message', '')}」")
    if incoming.get("sender_location"):
        lines.append(f"現在位置: {incoming.get('sender_location')}")
    if incoming.get("sender_action"):
        sender_action_desc = {
            "EVACUATE": "避難所に向かっている",
            "STAY": "その場で待機している",
            "SEARCH_FAMILY": "家族を探している",
            "CONTACT": "連絡を取っている",
            "FOLLOW": "他の人について行っている",
            "TALK": "他の人と話している",
        }.get(incoming.get("sender_action"), incoming.get("sender_action"))
        lines.append(f"相手の行動: {sender_action_desc}")
    lines.append("")

    # 指示
    lines.append("あなたはこの家族にメールで返信します。")
    lines.append("以下の点を含めて、自然な返信を作成してください：")
    lines.append("- あなたの現在の状況（無事かどうか、怪我はないか）")
    lines.append("- あなたの現在位置")
    lines.append("- 今後の行動予定（どこに避難するか、どう行動するか）")
    lines.append("")
    lines.append("【重要】以下のJSON形式で回答してください:")
    lines.append("{")
    lines.append('  "response_message": "返信内容（メールの本文、口語的な日本語）",')
    lines.append('  "current_status": "無事/軽傷/重傷など現在の状況",')
    lines.append('  "current_location": "現在位置の説明",')
    lines.append('  "planned_action": "今後の行動予定",')
    lines.append('  "reasoning": "この返信をした理由"')
    lines.append("}")

    return "\n".join(lines)


async def process_family_contact_response(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    家族連絡応答生成リクエストを処理する。
    連絡を受けた家族のLLMが返答を生成する。
    """
    request_id = payload.get("request_id", f"fam-{random.randint(0, 1_000_000)}")

    if OPENAI_CLIENT is None:
        # LLMが利用できない場合はデフォルト応答
        return {
            "request_id": request_id,
            "response_message": "無事だよ。今は自宅付近にいる。様子を見ている。",
            "current_status": "無事",
            "current_location": "自宅付近",
            "planned_action": "様子を見て避難する",
            "reasoning": "LLM unavailable - default response",
        }

    prompt = build_family_contact_response_prompt(payload)

    try:
        content = await _call_openai_with_retry(prompt)
        if content is None:
            return {
                "request_id": request_id,
                "response_message": "無事だよ。今は自宅付近にいる。様子を見ている。",
                "current_status": "無事",
                "current_location": "自宅付近",
                "planned_action": "様子を見て避難する",
                "reasoning": "LLM call failed - default response",
            }

        response = _safe_load_json(content)

        return {
            "request_id": request_id,
            "response_message": response.get("response_message", "無事だよ。"),
            "current_status": response.get("current_status", "不明"),
            "current_location": response.get("current_location", "不明"),
            "planned_action": response.get("planned_action", ""),
            "reasoning": response.get("reasoning", ""),
        }

    except Exception as exc:
        print(f"[LLM SERVER] 家族連絡応答生成で例外: {exc}")
        return {
            "request_id": request_id,
            "response_message": "無事だよ。今は自宅付近にいる。",
            "current_status": "無事",
            "current_location": "自宅付近",
            "planned_action": "様子を見て避難する",
            "reasoning": f"Exception: {exc}",
        }


async def _handle_single_message(
    websocket: websockets.WebSocketServerProtocol, message: str
) -> None:
    """
    1つのメッセージ処理を独立したタスクとして実行するヘルパー。
    これにより、同一WebSocket上でも複数のリクエストを並列に処理できる。
    """
    try:
        payload = json.loads(message)

        # request_typeによって処理を分岐
        request_type = payload.get("request_type", "evac_decision")

        if request_type == "conversation_response":
            # 会話応答生成リクエスト
            response = await process_conversation_response(payload)
        elif request_type == "family_contact_response":
            # 家族連絡応答生成リクエスト
            response = await process_family_contact_response(payload)
        else:
            # 通常の避難意思決定リクエスト
            response = await process_payload(payload)

        await websocket.send(json.dumps(response))
    except json.JSONDecodeError:
        await websocket.send(json.dumps({"error": "invalid_json"}))
    except Exception as exc:  # pragma: no cover
        await websocket.send(json.dumps({"error": str(exc)}))


async def handler(websocket: websockets.WebSocketServerProtocol) -> None:
    # 各受信メッセージごとに独立したタスクを起動し、並列に処理する
    async for message in websocket:
        # fire-and-forget タスクとして起動（エラーは各タスク内でハンドリング）
        asyncio.create_task(_handle_single_message(websocket, message))


async def main() -> None:
    global _memory_initialized

    print(f"[LLM SERVER] starting on ws://{SERVER_HOST}:{SERVER_PORT}")

    # 長期記憶（RAG）の初期化
    if OPENAI_CLIENT and not _memory_initialized:
        print("[LLM SERVER] 長期記憶（RAG）を初期化中...")
        memory_manager = await initialize_memory_manager(OPENAI_CLIENT)
        if memory_manager and memory_manager.is_initialized:
            _memory_initialized = True
            stats = memory_manager.get_statistics()
            print(f"[LLM SERVER] 長期記憶初期化完了: {stats}")
        else:
            print("[LLM SERVER] 長期記憶の初期化に失敗しました（RAG機能は無効）")

    # ping_timeout を延長（デフォルト20秒 → 120秒）
    # LLM API呼び出しが長時間かかる場合の接続切断を防ぐ
    async with websockets.serve(
        handler,
        SERVER_HOST,
        SERVER_PORT,
        ping_timeout=120,
        ping_interval=30,
    ):
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
