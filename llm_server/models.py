from __future__ import annotations

from typing import List, Optional, Tuple, Literal

from pydantic import BaseModel, Field, field_validator


class TemporalContext(BaseModel):
    elapsed_time: float = Field(description="秒単位の経過時間")
    time_limit: Optional[float] = Field(
        default=None, description="意図的に設定された制限時間（未設定ならNone）"
    )


class SelfState(BaseModel):
    position: Tuple[float, float, float]
    velocity: Tuple[float, float, float]
    energy_level: float = Field(ge=0.0, le=1.0)
    energy_label: Literal["high", "medium", "low"]
    stress_level: float = Field(ge=0.0, le=1.0)
    stress_label: Literal["calm", "alert", "panicked"]
    stress_reason: Optional[str]
    current_goal: Optional[str]
    stamina: float = Field(ge=0.0, le=1.0)
    injuries: List[str] = []
    injury_notes: Optional[str]

    @staticmethod
    def _vector3_to_tuple(value: object) -> Tuple[float, float, float]:
        if isinstance(value, dict):
            return (
                float(value.get("x", 0.0)),
                float(value.get("y", 0.0)),
                float(value.get("z", 0.0)),
            )
        if isinstance(value, (list, tuple)) and len(value) == 3:
            return float(value[0]), float(value[1]), float(value[2])
        raise TypeError("Vector3 must be dict or 3-tuple/list")

    @field_validator("position", "velocity", mode="before")
    @classmethod
    def convert_vector3(cls, value: object) -> Tuple[float, float, float]:
        return cls._vector3_to_tuple(value)


class AgentInput(BaseModel):
    self_state: SelfState
    temporal_context: TemporalContext
