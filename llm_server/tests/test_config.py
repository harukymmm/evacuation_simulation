"""
設定モジュールのテスト

config.py の設定値が適切に定義されていることを検証する。
"""
import pytest
import sys
import os

# テスト対象のモジュールをインポートできるようにパスを追加
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config import (
    ElevationConfig,
    DistanceConfig,
    CapacityConfig,
    SpeedConfig,
    MemoryConfig,
    ConversationConfig,
)


class TestElevationConfig:
    """標高設定のテスト"""

    def test_high_ground_value(self):
        """高台の閾値が正しいこと"""
        assert ElevationConfig.HIGH_GROUND == 30

    def test_moderate_height_value(self):
        """やや高い場所の閾値が正しいこと"""
        assert ElevationConfig.MODERATE_HEIGHT == 20

    def test_slight_height_value(self):
        """少し高い場所の閾値が正しいこと"""
        assert ElevationConfig.SLIGHT_HEIGHT == 10

    def test_ordering(self):
        """閾値が正しい順序であること"""
        assert ElevationConfig.HIGH_GROUND > ElevationConfig.MODERATE_HEIGHT
        assert ElevationConfig.MODERATE_HEIGHT > ElevationConfig.SLIGHT_HEIGHT


class TestDistanceConfig:
    """距離設定のテスト"""

    def test_distance_values(self):
        """各距離閾値が正しいこと"""
        assert DistanceConfig.VERY_CLOSE == 200
        assert DistanceConfig.SHORT_WALK == 500
        assert DistanceConfig.TEN_MINUTE_WALK == 1000
        assert DistanceConfig.TWENTY_MINUTE_WALK == 2000
        assert DistanceConfig.THIRTY_MINUTE_WALK == 3000

    def test_ordering(self):
        """閾値が昇順であること"""
        values = [
            DistanceConfig.VERY_CLOSE,
            DistanceConfig.SHORT_WALK,
            DistanceConfig.TEN_MINUTE_WALK,
            DistanceConfig.TWENTY_MINUTE_WALK,
            DistanceConfig.THIRTY_MINUTE_WALK,
        ]
        assert values == sorted(values)


class TestCapacityConfig:
    """収容能力設定のテスト"""

    def test_capacity_values(self):
        """各収容能力閾値が正しいこと"""
        assert CapacityConfig.VERY_LARGE == 500
        assert CapacityConfig.LARGE == 100
        assert CapacityConfig.SMALL == 30


class TestSpeedConfig:
    """速度設定のテスト"""

    def test_normal_speed(self):
        """通常速度が1.0であること"""
        assert SpeedConfig.NORMAL == 1.0

    def test_fast_speeds(self):
        """速い速度の閾値が正しいこと"""
        assert SpeedConfig.SLIGHTLY_FAST > SpeedConfig.NORMAL
        assert SpeedConfig.FAST > SpeedConfig.SLIGHTLY_FAST
        assert SpeedConfig.VERY_FAST > SpeedConfig.FAST

    def test_slow_speeds(self):
        """遅い速度の閾値が正しいこと"""
        assert SpeedConfig.SLIGHTLY_SLOW < SpeedConfig.NORMAL
        assert SpeedConfig.SLOW < SpeedConfig.SLIGHTLY_SLOW


class TestMemoryConfig:
    """メモリ/RAG設定のテスト"""

    def test_embedding_model(self):
        """埋め込みモデルが設定されていること"""
        assert MemoryConfig.EMBEDDING_MODEL == "text-embedding-3-small"

    def test_embedding_dimensions(self):
        """埋め込み次元が正しいこと"""
        assert MemoryConfig.EMBEDDING_DIMENSIONS == 1536

    def test_similarity_threshold(self):
        """類似度閾値が0-1の範囲であること"""
        assert 0 <= MemoryConfig.SIMILARITY_THRESHOLD <= 1

    def test_default_top_k(self):
        """デフォルトのtop_kが正の整数であること"""
        assert MemoryConfig.DEFAULT_TOP_K > 0


class TestConversationConfig:
    """会話設定のテスト"""

    def test_history_limits(self):
        """履歴制限が正の整数であること"""
        assert ConversationConfig.MAX_CONVERSATION_HISTORY > 0
        assert ConversationConfig.MAX_CONTACT_HISTORY > 0
        assert ConversationConfig.MAX_ACTION_HISTORY > 0


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
