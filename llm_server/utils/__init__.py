"""
ユーティリティモジュール

server.pyから分離した共通機能を提供する。
"""
from .descriptions import (
    get_speed_description,
    get_distance_description,
    get_elevation_description,
    get_capacity_description,
)

__all__ = [
    "get_speed_description",
    "get_distance_description",
    "get_elevation_description",
    "get_capacity_description",
]
