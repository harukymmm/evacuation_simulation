import asyncio
import json
import os
import random
import re
import math
from datetime import datetime
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

    lines = ["あなたは災害に遭遇してしまい、今すぐに避難しなければならない状態です。"]
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

    lines.append("候補避難所:")
    for idx, shelter in enumerate(shelters):
        capacity = shelter.get("current_capacity", 0)
        max_capacity = shelter.get("max_capacity", 0)
        position = shelter.get("position", {})
        lines.append(
            f"- index {idx}: id={shelter.get('id')} "
            f"残り受け入れ可能人数={capacity} "
            f"pos=({position.get('x', 0):.2f}, {position.get('z', 0):.2f})"
        )

    lines.append("以下のJSON形式で回答してください。")
    lines.append(
        '{"selected_shelter_id": "id", "reasoning": "短い説明", '
        '"confidence": 0.0-1.0, "desired_speed": (opt) m/s}'
    )
    lines.append(
        "desired_speedは体力や距離などの状況に応じて設定してください。急いで避難している状況を考慮してください"
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
) -> None:
    try:
        sanitized_id = _sanitize_filename(evacuee_id)
        timestamp = datetime.utcnow().isoformat() + "Z"

        # JSON Lines形式のログ
        json_filename = LOG_DIR / f"{sanitized_id}.log"
        log_entry = {
            "timestamp": timestamp,
            "request_id": request_id,
            "source": source,
            "input": input_snapshot,
            "output": output_snapshot,
        }
        with json_filename.open("a", encoding="utf-8") as fp:
            fp.write(json.dumps(log_entry, ensure_ascii=False))
            fp.write("\n")
    except Exception as exc:  # pragma: no cover
        print(f"[LLM SERVER] failed to write log: {exc}")


async def process_payload(payload: Dict[str, Any]) -> Dict[str, Any]:
    request_id = payload.get("request_id", f"req-{random.randint(0, 1_000_000)}")
    agent_input = _build_agent_input(payload)
    decision = await call_openai(payload, agent_input)
    evacuee = payload.get("evacuee", {})
    input_snapshot = {
        "agent_input": agent_input.model_dump() if agent_input else None,
        "shelter_candidates": payload.get("shelter_candidates"),
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
    )

    return response_payload


async def handler(websocket: websockets.WebSocketServerProtocol) -> None:
    async for message in websocket:
        try:
            payload = json.loads(message)
            response = await process_payload(payload)
            await websocket.send(json.dumps(response))
        except json.JSONDecodeError:
            await websocket.send(json.dumps({"error": "invalid_json"}))
        except Exception as exc:  # pragma: no cover
            await websocket.send(json.dumps({"error": str(exc)}))


async def main() -> None:
    print(f"[LLM SERVER] starting on ws://{SERVER_HOST}:{SERVER_PORT}")
    async with websockets.serve(handler, SERVER_HOST, SERVER_PORT):
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())
