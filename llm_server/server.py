import asyncio
import json
import os
import random
from typing import Any, Dict, List, Optional

import websockets
from dotenv import load_dotenv

try:
    from openai import AsyncOpenAI
except ImportError:  # pragma: no cover
    AsyncOpenAI = None  # type: ignore


load_dotenv()

OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
OPENAI_MODEL = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
SERVER_HOST = os.getenv("LLM_SERVER_HOST", "127.0.0.1")
SERVER_PORT = int(os.getenv("LLM_SERVER_PORT", "8765"))


def _create_client() -> Optional[AsyncOpenAI]:
    if OPENAI_API_KEY and AsyncOpenAI is not None:
        return AsyncOpenAI(api_key=OPENAI_API_KEY)
    return None


OPENAI_CLIENT = _create_client()


def build_prompt(payload: Dict[str, Any]) -> str:
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

    lines.append("候補避難所:")
    for idx, shelter in enumerate(shelters):
        capacity = shelter.get("current_capacity", 0)
        max_capacity = shelter.get("max_capacity", 0)
        position = shelter.get("position", {})
        lines.append(
            f"- index {idx}: id={shelter.get('id')} "
            f"capacity={capacity}/{max_capacity} "
            f"pos=({position.get('x', 0):.2f}, {position.get('z', 0):.2f})"
        )

    lines.append("以下のJSON形式で回答してください。")
    lines.append(
        '{"selected_shelter_id": "id", "reasoning": "短い説明", "confidence": 0.0-1.0}'
    )

    return "\n".join(lines)


def _position_tuple(info: Dict[str, Any]) -> tuple[float, float, float]:
    pos = info.get("position", {}) if info else {}
    return (
        float(pos.get("x", 0.0)),
        float(pos.get("y", 0.0)),
        float(pos.get("z", 0.0)),
    )


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


async def call_openai(payload: Dict[str, Any]) -> Optional[Dict[str, Any]]:
    if OPENAI_CLIENT is None:
        return None

    prompt = build_prompt(payload)
    try:
        response = await OPENAI_CLIENT.responses.create(
            model=OPENAI_MODEL,
            input=prompt,
        )
        content = response.output[0].content[0].text  # type: ignore[attr-defined]
        decision = json.loads(content)
        if "selected_shelter_id" in decision:
            return decision
    except Exception as exc:  # pragma: no cover
        print(f"[LLM SERVER] OpenAI呼び出しで例外: {exc}")
    return None


async def process_payload(payload: Dict[str, Any]) -> Dict[str, Any]:
    request_id = payload.get("request_id", f"req-{random.randint(0, 1_000_000)}")
    decision = await call_openai(payload)
    evacuee = payload.get("evacuee", {})

    if decision is None:
        selected_id = heuristic_selection(payload)
        reasoning = "Fallback heuristic: closest shelter with spare capacity."
        confidence = 0.5
    else:
        selected_id = decision.get("selected_shelter_id") or heuristic_selection(
            payload
        )
        reasoning = decision.get("reasoning", "LLM decision")
        confidence = float(decision.get("confidence", 0.5))

    if selected_id is None and payload.get("shelter_candidates"):
        selected_id = payload["shelter_candidates"][0].get("id")

    return {
        "request_id": request_id,
        "evacuee_id": evacuee.get("id"),
        "selected_shelter_id": selected_id,
        "reasoning": reasoning,
        "confidence": confidence,
    }


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
