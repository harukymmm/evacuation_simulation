"""
設定モジュール

サーバー全体で使用する定数・設定値を集約。
マジックナンバーを排除し、設定変更を容易にする。
"""
from __future__ import annotations


class ElevationConfig:
    """標高に関する設定"""
    # 標高閾値（メートル）
    HIGH_GROUND = 30      # 高台と判定する標高
    MODERATE_HEIGHT = 20  # やや高い場所と判定する標高
    SLIGHT_HEIGHT = 10    # 少し高い場所と判定する標高


class DistanceConfig:
    """距離に関する設定"""
    # 距離閾値（メートル）
    VERY_CLOSE = 200       # すぐ近く
    SHORT_WALK = 500       # 歩いて数分
    TEN_MINUTE_WALK = 1000     # 歩いて10分
    TWENTY_MINUTE_WALK = 2000  # 歩いて20分
    THIRTY_MINUTE_WALK = 3000  # 歩いて30分


class CapacityConfig:
    """収容能力に関する設定"""
    # 収容人数閾値
    VERY_LARGE = 500   # 非常に広い
    LARGE = 100        # 広い
    SMALL = 30         # 小さな


class SpeedConfig:
    """移動速度に関する設定"""
    # 速度倍率閾値
    VERY_FAST = 2.0    # 極めて自信がある
    FAST = 1.5         # 十分な自信がある
    SLIGHTLY_FAST = 1.2  # 自信がある
    NORMAL = 1.0       # 通常
    SLIGHTLY_SLOW = 0.8  # 少し不安がある
    SLOW = 0.5         # 自信がない


class MemoryConfig:
    """メモリ/RAGに関する設定"""
    EMBEDDING_MODEL = "text-embedding-3-small"
    EMBEDDING_DIMENSIONS = 1536
    SIMILARITY_THRESHOLD = 0.75
    DEFAULT_TOP_K = 3


class LLMConfig:
    """LLM API呼び出しに関する設定"""
    MAX_RETRIES = 3
    BASE_DELAY_SECONDS = 1.0
    TIMEOUT_SECONDS = 30.0


class ConversationConfig:
    """会話に関する設定"""
    MAX_CONVERSATION_HISTORY = 5
    MAX_CONTACT_HISTORY = 5
    MAX_ACTION_HISTORY = 3
