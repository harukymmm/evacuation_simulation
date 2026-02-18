# llm_server/prompts/vehicle_prompts.py
"""
車両用LLMプロンプト構築モジュール

車両避難シミュレーションにおけるLLM意思決定のための
システムプロンプトとユーザープロンプトを構築します。
"""

from typing import Any, Dict, List, Tuple


def build_vehicle_system_prompt() -> str:
    """
    車両用システムプロンプトを構築

    Returns:
        システムプロンプト文字列
    """
    lines = []

    # 基本ロール定義
    lines.append("あなたは災害発生時に車で避難しようとしている運転手です。")
    lines.append("乗客の安全を最優先に、状況に応じた判断を行ってください。")
    lines.append("")

    # 人間らしい判断の強調
    lines.append("【重要】人間らしい判断をしてください：")
    lines.append("- 渋滞時は無理に進まず、状況を見極めることがある")
    lines.append("- 家族の安否が気になり、避難より迎えに行きたくなることがある")
    lines.append("- 燃料が心配で、遠くの避難所は避けたくなる")
    lines.append("- 車を置いて逃げる判断は心理的に難しい")
    lines.append("- 「まだ大丈夫」と危険を過小評価することがある")
    lines.append("")

    # 選択可能な行動
    lines.append("【選択可能な行動】")
    lines.append("- DRIVE_TO_SHELTER: 避難所へ向かって走行する")
    lines.append("- WAIT_IN_CAR: 車内で待機する（渋滞回避、情報収集）")
    lines.append("- PICKUP_FAMILY: 家族を迎えに行く")
    lines.append("- CONTACT: 電話で家族に連絡を取る")
    lines.append("- PARK_AND_WALK: 車を駐車して徒歩で避難する")
    lines.append("")

    # 出力形式
    lines.append("【出力形式】")
    lines.append("以下のJSON形式で回答してください：")
    lines.append("")
    lines.append("```json")
    lines.append("{")
    lines.append('  "action_type": "DRIVE_TO_SHELTER",')
    lines.append('  "reasoning": "判断理由を2-3文で説明",')
    lines.append('  "confidence": 0.8,')
    lines.append('  "selected_shelter_id": "避難所名"  // DRIVE_TO_SHELTERの場合')
    lines.append("}")
    lines.append("```")
    lines.append("")

    # 行動別の追加フィールド
    lines.append("【行動別の追加フィールド】")
    lines.append("- DRIVE_TO_SHELTER: selected_shelter_id(必須), desired_speed(SLOW/NORMAL/FAST)")
    lines.append("- WAIT_IN_CAR: wait_reason, max_wait_time_sec, engine_state(ON/OFF)")
    lines.append("- PICKUP_FAMILY: target_family_member, pickup_location, after_pickup_shelter")
    lines.append("- CONTACT: contact_target, contact_message, should_stop(true/false)")
    lines.append("- PARK_AND_WALK: walking_destination, abandon_vehicle(true/false)")

    return "\n".join(lines)


def build_vehicle_user_prompt(payload: Dict[str, Any]) -> str:
    """
    車両用ユーザープロンプトを構築

    Args:
        payload: 車両の状態情報を含む辞書

    Returns:
        ユーザープロンプト文字列
    """
    sections = []

    # A. 車両情報
    sections.append(_build_vehicle_info_section(payload))

    # B. 現在の状況
    sections.append(_build_current_situation_section(payload))

    # C. 交通状況
    sections.append(_build_traffic_context_section(payload))

    # D. 避難所候補
    sections.append(_build_shelter_candidates_section(payload))

    # E. 家族情報（存在する場合）
    family_section = _build_family_info_section(payload)
    if family_section:
        sections.append(family_section)

    # F. 運転者のペルソナ
    sections.append(_build_driver_persona_section(payload))

    # G. 行動履歴（存在する場合）
    history_section = _build_action_history_section(payload)
    if history_section:
        sections.append(history_section)

    return "\n\n".join(sections)


def build_vehicle_prompts(payload: Dict[str, Any]) -> Tuple[str, str]:
    """
    車両用のシステムプロンプトとユーザープロンプトを構築

    Args:
        payload: 車両の状態情報を含む辞書

    Returns:
        (システムプロンプト, ユーザープロンプト) のタプル
    """
    system_prompt = build_vehicle_system_prompt()
    user_prompt = build_vehicle_user_prompt(payload)
    return system_prompt, user_prompt


# ============================================================
# セクション構築ヘルパー関数
# ============================================================


def _build_vehicle_info_section(payload: Dict[str, Any]) -> str:
    """【A. 車両情報】セクションを構築"""
    lines = ["【A. 車両情報】"]

    vehicle = payload.get("vehicle", {})
    vehicle_state = payload.get("vehicle_state", {})

    # 車種
    vehicle_type = vehicle.get("vehicle_type", "普通車")
    vehicle_type_ja = _translate_vehicle_type(vehicle_type)
    lines.append(f"- 車種: {vehicle_type_ja}")

    # 乗車人数
    passenger_count = vehicle.get("passenger_count", 1)
    lines.append(f"- 乗車人数: {passenger_count}人")

    # 燃料
    fuel_level = vehicle_state.get("fuel_level", 1.0)
    fuel_percent = int(fuel_level * 100)
    fuel_status = _get_fuel_status(fuel_level)
    lines.append(f"- 燃料: {fuel_percent}%（{fuel_status}）")

    return "\n".join(lines)


def _build_current_situation_section(payload: Dict[str, Any]) -> str:
    """【B. 現在の状況】セクションを構築"""
    lines = ["【B. 現在の状況】"]

    vehicle_state = payload.get("vehicle_state", {})

    # 現在速度
    current_speed = vehicle_state.get("current_speed_kmh", 0)
    lines.append(f"- 現在の速度: {current_speed:.0f} km/h")

    # 現在の道路
    current_road = vehicle_state.get("current_road_name", "")
    if current_road:
        lines.append(f"- 現在の道路: {current_road}")

    # 車両状態
    state = vehicle_state.get("vehicle_state", "Driving")
    state_ja = _translate_vehicle_state(state)
    lines.append(f"- 状態: {state_ja}")

    # 現在の目標
    current_goal = vehicle_state.get("current_goal", "")
    if current_goal:
        lines.append(f"- 現在の目的地: {current_goal}")

    # ストレスレベル
    stress_level = vehicle_state.get("stress_level", 0)
    if stress_level > 0.3:
        stress_status = _get_stress_status(stress_level)
        lines.append(f"- ストレス状態: {stress_status}")

    return "\n".join(lines)


def _build_traffic_context_section(payload: Dict[str, Any]) -> str:
    """【C. 交通状況】セクションを構築"""
    lines = ["【C. 交通状況】"]

    traffic = payload.get("traffic_context", {})

    # 全体的なサービスレベル
    los = traffic.get("overall_level_of_service", "A")
    los_description = _get_los_description(los)
    lines.append(f"- 全体的な道路状況: {los_description}")

    # 渋滞道路数
    congested_count = traffic.get("congested_road_count", 0)
    if congested_count > 0:
        lines.append(f"- 渋滞している道路: {congested_count}箇所")

    # 周辺道路の詳細
    nearby_roads = traffic.get("nearby_roads", [])
    if nearby_roads:
        lines.append("- 周辺道路の状況:")
        for road in nearby_roads[:3]:  # 最大3件
            road_name = road.get("road_name", "不明")
            congestion = road.get("congestion_level", 0)
            avg_speed = road.get("average_speed_kmh", 0)
            congestion_status = _get_congestion_status(congestion)
            lines.append(f"  - {road_name}: {congestion_status}（平均{avg_speed:.0f}km/h）")

    return "\n".join(lines)


def _build_shelter_candidates_section(payload: Dict[str, Any]) -> str:
    """【D. 避難所候補】セクションを構築"""
    lines = ["【D. 避難所候補】"]

    shelters = payload.get("shelter_candidates", [])

    if not shelters:
        lines.append("（避難所情報がありません）")
        return "\n".join(lines)

    # 距離順にソート
    sorted_shelters = sorted(shelters, key=lambda x: x.get("driving_distance_km", float("inf")))

    for i, shelter in enumerate(sorted_shelters[:5], 1):  # 最大5件
        name = shelter.get("display_name", shelter.get("id", "不明"))
        distance = shelter.get("driving_distance_km", 0)
        time = shelter.get("driving_time_minutes", 0)
        current_cap = shelter.get("current_capacity", 0)
        max_cap = shelter.get("max_capacity", 0)
        parking = shelter.get("available_parking_spots", 0)

        # 収容状況
        if max_cap > 0:
            usage = current_cap / max_cap
            if usage >= 0.9:
                cap_status = "ほぼ満員"
            elif usage >= 0.7:
                cap_status = "混雑"
            else:
                cap_status = "余裕あり"
        else:
            cap_status = "不明"

        lines.append(
            f"{i}. {name}: 距離{distance:.1f}km, "
            f"所要時間{time:.0f}分, "
            f"収容状況: {cap_status}, "
            f"駐車場{parking}台空き"
        )

    return "\n".join(lines)


def _build_family_info_section(payload: Dict[str, Any]) -> str:
    """【E. 家族情報】セクションを構築"""
    family_members = payload.get("family_members", [])

    if not family_members:
        return ""

    lines = ["【E. 家族情報】"]

    for member in family_members:
        name = member.get("name", "不明")
        relation = member.get("relation", "")
        location = member.get("likely_location", "不明")
        distance = member.get("distance_meters", 0)
        has_phone = member.get("has_phone", False)
        is_in_vehicle = member.get("is_in_vehicle", False)
        is_safe = member.get("is_safe", False)

        if is_in_vehicle:
            lines.append(f"- {name}（{relation}）: 車内にいます ✓")
        elif is_safe:
            lines.append(f"- {name}（{relation}）: 安全確認済み ✓")
        else:
            phone_status = "連絡可能" if has_phone else "連絡不可"
            lines.append(
                f"- {name}（{relation}）: {location}にいる可能性（{distance:.0f}m先、{phone_status}）"
            )

    return "\n".join(lines)


def _build_driver_persona_section(payload: Dict[str, Any]) -> str:
    """【F. 運転者のペルソナ】セクションを構築

    歩行者と同じペルソナ形式（PersonaPayload）を使用。
    """
    lines = ["【F. 運転者のペルソナ】"]

    persona = payload.get("persona", {})

    # 基本情報
    if name := persona.get("name"):
        lines.append(f"名前: {name}")

    if role := persona.get("role"):
        lines.append(f"役割: {role}")

    if age_group := persona.get("age_group"):
        age_group_ja = _translate_age_group(age_group)
        lines.append(f"年齢層: {age_group_ja}")

    # 心理状態
    if mental_state := persona.get("mental_state"):
        mental_state_ja = _translate_mental_state(mental_state)
        lines.append(f"心理状態: {mental_state_ja}")

    # 優先事項
    if priority := persona.get("priority"):
        lines.append(f"優先事項: {priority}")

    # 身体状態
    if physical_condition := persona.get("physical_condition"):
        lines.append(f"身体状態: {physical_condition}")

    # 生活背景情報
    background_info = []

    if home_location := persona.get("home_location_category"):
        location_ja = _translate_home_location(home_location)
        background_info.append(f"自宅: {location_ja}")

    if local_knowledge := persona.get("local_knowledge_level"):
        knowledge_ja = _translate_local_knowledge(local_knowledge)
        background_info.append(f"土地勘: {knowledge_ja}")

    if past_disaster := persona.get("past_disaster_experience"):
        background_info.append(f"災害経験: {past_disaster}")

    if background_info:
        lines.append("- " + "、".join(background_info))

    # 追加のコンテキスト
    if context := persona.get("system_prompt_context"):
        lines.append(f"- {context}")

    return "\n".join(lines)


def _translate_age_group(age_group: str) -> str:
    """年齢層を日本語に変換"""
    translations = {
        "child": "子ども",
        "teen": "10代",
        "young_adult": "若年層",
        "adult": "成人",
        "middle_aged": "中年",
        "senior": "高齢者",
        "elderly": "高齢者",
    }
    return translations.get(age_group, age_group)


def _translate_mental_state(mental_state: str) -> str:
    """心理状態を日本語に変換"""
    translations = {
        "calm": "落ち着いている",
        "neutral": "普通",
        "anxious": "不安",
        "panic": "パニック",
        "stressed": "ストレス状態",
        "confused": "混乱",
    }
    return translations.get(mental_state, mental_state)


def _translate_home_location(location: str) -> str:
    """自宅位置カテゴリを日本語に変換"""
    translations = {
        "coastal": "沿岸部",
        "hill": "丘陵地",
        "center": "中心部",
        "inland": "内陸部",
    }
    return translations.get(location, location)


def _translate_local_knowledge(knowledge: str) -> str:
    """土地勘レベルを日本語に変換"""
    translations = {
        "newcomer": "新参者（土地勘なし）",
        "resident": "住民（ある程度の土地勘）",
        "native": "地元民（十分な土地勘）",
    }
    return translations.get(knowledge, knowledge)


def _build_action_history_section(payload: Dict[str, Any]) -> str:
    """【G. 行動履歴】セクションを構築"""
    history = payload.get("action_history", [])
    current_action = payload.get("current_action", "")

    if not history and not current_action:
        return ""

    lines = ["【G. 最近の行動】"]

    if current_action:
        action_ja = _translate_action_type(current_action)
        lines.append(f"- 現在の行動: {action_ja}")

    if history:
        lines.append("- 行動履歴:")
        for entry in history[-5:]:  # 最新5件
            lines.append(f"  - {entry}")

    return "\n".join(lines)


# ============================================================
# ユーティリティ関数
# ============================================================


def _translate_vehicle_type(vehicle_type: str) -> str:
    """車両タイプを日本語に変換"""
    translations = {
        "Sedan": "セダン",
        "SUV": "SUV",
        "Minivan": "ミニバン",
        "Truck": "トラック",
    }
    return translations.get(vehicle_type, vehicle_type)


def _translate_vehicle_state(state: str) -> str:
    """車両状態を日本語に変換"""
    translations = {
        "Idle": "停止中",
        "Driving": "走行中",
        "Waiting": "待機中",
        "Parking": "駐車中",
        "Evacuated": "避難完了",
    }
    return translations.get(state, state)


def _translate_action_type(action_type: str) -> str:
    """行動タイプを日本語に変換"""
    translations = {
        "DRIVE_TO_SHELTER": "避難所へ走行中",
        "WAIT_IN_CAR": "車内で待機中",
        "PICKUP_FAMILY": "家族を迎えに行く",
        "CONTACT": "電話連絡中",
        "PARK_AND_WALK": "徒歩避難に切り替え",
    }
    return translations.get(action_type, action_type)


def _get_fuel_status(fuel_level: float) -> str:
    """燃料レベルから状態を取得"""
    if fuel_level >= 0.7:
        return "十分"
    elif fuel_level >= 0.4:
        return "普通"
    elif fuel_level >= 0.2:
        return "少なめ"
    else:
        return "要給油"


def _get_stress_status(stress_level: float) -> str:
    """ストレスレベルから状態を取得"""
    if stress_level >= 0.8:
        return "非常に高い"
    elif stress_level >= 0.6:
        return "高い"
    elif stress_level >= 0.4:
        return "やや高い"
    else:
        return "普通"


def _get_los_description(los: str) -> str:
    """LOS（サービスレベル）の説明を取得"""
    descriptions = {
        "A": "自由流（スムーズ）",
        "B": "安定流（良好）",
        "C": "安定流（普通）",
        "D": "不安定流（やや混雑）",
        "E": "限界流（混雑）",
        "F": "強制流（渋滞）",
    }
    return descriptions.get(los, f"レベル{los}")


def _get_congestion_status(congestion: float) -> str:
    """混雑度から状態を取得"""
    if congestion >= 0.8:
        return "渋滞"
    elif congestion >= 0.6:
        return "混雑"
    elif congestion >= 0.4:
        return "やや混雑"
    else:
        return "スムーズ"


# ============================================================
# ヒューリスティック選択（LLM失敗時のフォールバック）
# ============================================================


def heuristic_vehicle_selection(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    ヒューリスティックによる車両の行動選択（LLM失敗時のフォールバック）

    Args:
        payload: 車両の状態情報

    Returns:
        行動決定の辞書
    """
    shelters = payload.get("shelter_candidates", [])
    vehicle_state = payload.get("vehicle_state", {})
    traffic = payload.get("traffic_context", {})

    # デフォルト: 最寄りの避難所へ
    if not shelters:
        return {
            "action_type": "DRIVE_TO_SHELTER",
            "reasoning": "避難所情報がないため、現在の目的地へ向かいます",
            "confidence": 0.3,
        }

    # 距離と収容状況を考慮して避難所を選択
    best_shelter = None
    best_score = float("inf")

    for shelter in shelters:
        distance = shelter.get("driving_distance_km", float("inf"))
        current_cap = shelter.get("current_capacity", 0)
        max_cap = shelter.get("max_capacity", 1)
        parking = shelter.get("available_parking_spots", 0)

        # スコア計算（低いほど良い）
        usage_ratio = current_cap / max_cap if max_cap > 0 else 1.0
        score = distance * (1 + usage_ratio)

        # 駐車場がない場合はペナルティ
        if parking == 0:
            score *= 1.5

        if score < best_score:
            best_score = score
            best_shelter = shelter

    if best_shelter:
        return {
            "action_type": "DRIVE_TO_SHELTER",
            "selected_shelter_id": best_shelter.get("display_name", best_shelter.get("id")),
            "desired_speed": "NORMAL",
            "reasoning": f"最寄りで収容余裕のある{best_shelter.get('display_name')}へ向かいます",
            "confidence": 0.6,
        }

    return {
        "action_type": "DRIVE_TO_SHELTER",
        "reasoning": "利用可能な避難所へ向かいます",
        "confidence": 0.4,
    }
