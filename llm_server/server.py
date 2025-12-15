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


load_dotenv()

OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
SERVER_HOST = os.getenv("LLM_SERVER_HOST", "127.0.0.1")
SERVER_PORT = int(os.getenv("LLM_SERVER_PORT", "8765"))

PROJECT_ROOT = Path(__file__).resolve().parents[1]
LOG_DIR = PROJECT_ROOT / "Logs" / "llm_decisions"
LOG_DIR.mkdir(parents=True, exist_ok=True)


def _create_client() -> Optional[AsyncOpenAI]:
    if OPENAI_API_KEY and AsyncOpenAI is not None:
        return AsyncOpenAI(api_key=OPENAI_API_KEY)
    return None


OPENAI_CLIENT = _create_client()


def build_prompt(payload: Dict[str, Any], agent_input: Optional[AgentInput]) -> str:
    shelters = payload.get("shelter_candidates", [])
    evacuee = payload.get("evacuee", {})
    persona = payload.get("persona")

    lines = []

    # ペルソナ情報がある場合は、それを最初に追加
    if persona:
        lines.append("【あなたのペルソナ】")
        lines.append(f"名前: {persona.get('name', '不明')}")
        lines.append(f"役割: {persona.get('role', '不明')}")
        lines.append(f"年齢層: {persona.get('age_group', '不明')}")
        lines.append(f"心理状態: {persona.get('mental_state', '不明')}")
        lines.append(f"優先事項: {persona.get('priority', '不明')}")
        lines.append("")
        lines.append(persona.get("system_prompt_context", ""))
        lines.append("")

    lines.append(
        "あなたは災害に遭遇してしまい、今すぐに避難しなければならない状態です。"
    )
    lines.append(
        "現在、避難所の候補があなたの近くにあります。あなたはどの避難所に向かうべきですか？"
    )
    lines.append("対象避難者:")
    lines.append(
        f"- id={evacuee.get('id')} pos=({evacuee.get('position', {}).get('x', 0):.2f}, "
        f"{evacuee.get('position', {}).get('z', 0):.2f})"
    )

    if agent_input is not None:
        self_state = agent_input.self_state
        temporal = agent_input.temporal_context
        lines.append("自身の状態:")
        lines.append(
            f"- 体力={self_state.energy_label}({self_state.energy_level:.2f}) "
            f"ストレス={self_state.stress_label}({self_state.stress_level:.2f})"
        )
        if self_state.stress_reason:
            lines.append(f"- ストレス要因: {self_state.stress_reason}")
        if self_state.current_goal:
            lines.append(f"- 現在の目標: {self_state.current_goal}")
        if temporal.time_limit is not None:
            remaining = max(temporal.time_limit - temporal.elapsed_time, 0.0)
            lines.append(
                f"- 時間制約: 残り{remaining:.1f}秒 / 総量 {temporal.time_limit:.1f}秒"
            )

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
            if floors > 0:
                details.append(f"{floors}階建て")
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
        lines.append("")

    lines.append("候補避難所:")
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

        # 基本情報の表示（表示名をメインに）
        shelter_info = f"- 候補{idx}: {display_name}"

        # 説明がある場合は追加
        if description:
            shelter_info += f"（{description}）"

        # 収容人数を追加
        shelter_info += f" 残り受け入れ可能人数={capacity}人"

        # 距離と徒歩時間を追加
        if distance_m > 0:
            shelter_info += f" / 距離=約{distance_m:.0f}m（徒歩{walking_time:.0f}分）"

        lines.append(shelter_info)

    # ペルソナ情報に基づく追加指示
    if persona:
        speed_multiplier = persona.get("speed_multiplier", 1.0)
        stairs_usage = persona.get("stairs_usage", "allowed")
        if stairs_usage == "forbidden":
            lines.append(
                "重要: あなたは階段を使用できません。階段を含む経路は選択しないでください。"
            )
        if speed_multiplier < 1.0:
            lines.append(
                f"注意: あなたの移動速度は通常の{speed_multiplier:.1f}倍です。無理をしない範囲で避難してください。"
            )
        elif speed_multiplier > 1.0:
            lines.append(
                f"あなたは通常より{speed_multiplier:.1f}倍速く移動できます。状況に応じて速度を調整してください。"
            )

    lines.append("")
    lines.append("【重要】以下のJSON形式で回答してください。")
    lines.append(
        '{"selected_shelter_id": "避難所の名前", "reasoning": "短い説明", '
        '"confidence": 0.0-1.0, "desired_speed": (opt) m/s}'
    )
    lines.append("")
    lines.append("注意事項:")
    lines.append(
        "- selected_shelter_idには必ず上記に表示されている避難所の名前を正確に指定してください"
    )
    lines.append(
        "  例: 「豊間小学校」「いわき市役所」など（「候補1」「候補2」は使用不可）"
    )
    lines.append(
        "- desired_speedは体力や距離、あなたのペルソナの特性などの状況に応じて設定してください（急いで避難している状況を考慮）"
    )

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


async def call_openai(
    payload: Dict[str, Any], agent_input: Optional[AgentInput]
) -> Optional[Dict[str, Any]]:
    if OPENAI_CLIENT is None:
        return None

    prompt = build_prompt(payload, agent_input)
    try:
        response = await OPENAI_CLIENT.responses.create(
            model=OPENAI_MODEL,
            input=prompt,
        )
        content = response.output[0].content[0].text  # type: ignore[attr-defined]
        decision = _safe_load_json(content)
        if "selected_shelter_id" in decision:
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
        selected_id = heuristic_selection(payload)
        reasoning = "Fallback heuristic: closest shelter with spare capacity."
        confidence = 0.5
        source = "heuristic"
        desired_speed = None
    else:
        selected_id = decision.get("selected_shelter_id") or heuristic_selection(
            payload
        )
        reasoning = decision.get("reasoning", "LLM decision")
        confidence = float(decision.get("confidence", 0.5))
        source = "llm"
        desired_speed = _extract_float(decision.get("desired_speed"))

    if selected_id is None and payload.get("shelter_candidates"):
        selected_id = payload["shelter_candidates"][0].get("id")

    response_payload = {
        "request_id": request_id,
        "evacuee_id": evacuee.get("id"),
        "selected_shelter_id": selected_id,
        "reasoning": reasoning,
        "confidence": confidence,
        "desired_speed": desired_speed,
    }

    output_snapshot = {
        "selected_shelter_id": selected_id,
        "reasoning": reasoning,
        "confidence": confidence,
    }
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


async def _handle_single_message(
    websocket: websockets.WebSocketServerProtocol, message: str
) -> None:
    """
    1つのメッセージ処理を独立したタスクとして実行するヘルパー。
    これにより、同一WebSocket上でも複数のリクエストを並列に処理できる。
    """
    try:
        payload = json.loads(message)
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
    print(f"[LLM SERVER] starting on ws://{SERVER_HOST}:{SERVER_PORT}")
    async with websockets.serve(handler, SERVER_HOST, SERVER_PORT):
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
