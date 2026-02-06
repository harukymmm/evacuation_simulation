"""
説明文生成ユーティリティ

数値データを人間が理解しやすい日本語表現に変換する関数群。
LLMへのプロンプト構築時に使用。
"""
from __future__ import annotations

from typing import Optional
import sys
import os

# 親ディレクトリをパスに追加してconfigをインポート可能にする
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config import (
    ElevationConfig,
    DistanceConfig,
    CapacityConfig,
    SpeedConfig,
)


def get_speed_description(speed_multiplier: float) -> Optional[str]:
    """
    速度倍率を「自信」に関する表現に変換する

    Args:
        speed_multiplier: 速度倍率（1.0が通常速度）

    Returns:
        自信に関する表現、1.0の場合はNone
    """
    if speed_multiplier > SpeedConfig.NORMAL:
        # 速い場合（自信がある）
        if speed_multiplier <= SpeedConfig.SLIGHTLY_FAST:
            return "徒歩での移動に自信があります"
        elif speed_multiplier <= SpeedConfig.FAST:
            return "徒歩での移動に十分な自信があります"
        elif speed_multiplier <= SpeedConfig.VERY_FAST:
            return "徒歩での移動に非常に自信があります"
        else:
            return "徒歩での移動に極めて自信があります"
    elif speed_multiplier < SpeedConfig.NORMAL:
        # 遅い場合（自信がない）
        if speed_multiplier >= SpeedConfig.SLIGHTLY_SLOW:
            return "徒歩での移動には少し不安があります"
        elif speed_multiplier >= SpeedConfig.SLOW:
            return "徒歩での移動には自信がありません"
        else:
            return "徒歩での移動には全く自信がありません"
    else:
        # 1.0の場合（通常速度）- この場合は何も表示しない
        return None


def get_distance_description(distance_m: float, walking_time: float) -> str:
    """
    距離を感覚的な表現に変換する

    Args:
        distance_m: 距離（メートル）
        walking_time: 歩行時間（分）

    Returns:
        距離の日本語表現
    """
    if distance_m <= DistanceConfig.VERY_CLOSE:
        return "すぐ近く"
    elif distance_m <= DistanceConfig.SHORT_WALK:
        return "歩いて数分"
    elif distance_m <= DistanceConfig.TEN_MINUTE_WALK:
        return "歩いて10分くらい"
    elif distance_m <= DistanceConfig.TWENTY_MINUTE_WALK:
        return "歩いて20分くらい"
    elif distance_m <= DistanceConfig.THIRTY_MINUTE_WALK:
        return "歩いて30分くらい"
    else:
        return f"かなり遠い（歩いて{int(walking_time)}分以上）"


def get_elevation_description(elevation_m: float) -> str:
    """
    海抜を感覚的な表現に変換する

    Args:
        elevation_m: 海抜（メートル）

    Returns:
        標高の日本語表現
    """
    if elevation_m >= ElevationConfig.HIGH_GROUND:
        return "高台"
    elif elevation_m >= ElevationConfig.MODERATE_HEIGHT:
        return "やや高い場所"
    elif elevation_m >= ElevationConfig.SLIGHT_HEIGHT:
        return "少し高い場所"
    else:
        return "低地"


def get_capacity_description(current_capacity: int, max_capacity: int) -> str:
    """
    収容状況を感覚的な表現に変換する

    Args:
        current_capacity: 残り収容可能人数
        max_capacity: 最大収容人数

    Returns:
        収容状況の日本語表現
    """
    if current_capacity >= 2147483647:  # int.MaxValue
        return "広い場所"

    if max_capacity > 0:
        ratio = current_capacity / max_capacity
        if ratio >= 0.8:
            return "まだ余裕がある"
        elif ratio >= 0.5:
            return "半分くらい埋まっている"
        elif ratio >= 0.2:
            return "かなり混雑している"
        else:
            return "ほぼ満員"

    # max_capacityがない場合は残り人数で判断
    if current_capacity >= CapacityConfig.VERY_LARGE:
        return "まだ余裕がある"
    elif current_capacity >= CapacityConfig.LARGE:
        return "それなりに受け入れ可能"
    elif current_capacity >= CapacityConfig.SMALL:
        return "あまり余裕がない"
    else:
        return "ほぼ満員"
