"""
説明関数のテスト

utils/descriptions.py の各関数が正しい日本語表現を返すことを検証する。
"""
import pytest
import sys
import os

# テスト対象のモジュールをインポートできるようにパスを追加
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from utils.descriptions import (
    get_speed_description,
    get_distance_description,
    get_elevation_description,
    get_capacity_description,
)


class TestSpeedDescription:
    """速度説明のテスト"""

    def test_normal_speed_returns_none(self):
        """通常速度（1.0）の場合はNoneを返す"""
        result = get_speed_description(1.0)
        assert result is None

    def test_slightly_fast(self):
        """少し速い場合"""
        result = get_speed_description(1.1)
        assert result == "徒歩での移動に自信があります"

    def test_fast(self):
        """速い場合"""
        result = get_speed_description(1.4)
        assert result == "徒歩での移動に十分な自信があります"

    def test_very_fast(self):
        """非常に速い場合"""
        result = get_speed_description(1.8)
        assert result == "徒歩での移動に非常に自信があります"

    def test_extremely_fast(self):
        """極めて速い場合"""
        result = get_speed_description(2.5)
        assert result == "徒歩での移動に極めて自信があります"

    def test_slightly_slow(self):
        """少し遅い場合"""
        result = get_speed_description(0.9)
        assert result == "徒歩での移動には少し不安があります"

    def test_slow(self):
        """遅い場合"""
        result = get_speed_description(0.6)
        assert result == "徒歩での移動には自信がありません"

    def test_very_slow(self):
        """非常に遅い場合"""
        result = get_speed_description(0.3)
        assert result == "徒歩での移動には全く自信がありません"


class TestDistanceDescription:
    """距離説明のテスト"""

    def test_very_close(self):
        """すぐ近くの場合"""
        result = get_distance_description(150, 2)
        assert result == "すぐ近く"

    def test_short_walk(self):
        """歩いて数分の場合"""
        result = get_distance_description(400, 5)
        assert result == "歩いて数分"

    def test_ten_minute_walk(self):
        """10分くらいの場合"""
        result = get_distance_description(800, 10)
        assert result == "歩いて10分くらい"

    def test_twenty_minute_walk(self):
        """20分くらいの場合"""
        result = get_distance_description(1500, 18)
        assert result == "歩いて20分くらい"

    def test_thirty_minute_walk(self):
        """30分くらいの場合"""
        result = get_distance_description(2500, 30)
        assert result == "歩いて30分くらい"

    def test_far_away(self):
        """かなり遠い場合"""
        result = get_distance_description(4000, 50)
        assert "かなり遠い" in result
        assert "50分" in result


class TestElevationDescription:
    """標高説明のテスト"""

    def test_high_ground(self):
        """高台の場合"""
        result = get_elevation_description(35)
        assert result == "高台"

    def test_moderate_height(self):
        """やや高い場所の場合"""
        result = get_elevation_description(25)
        assert result == "やや高い場所"

    def test_slight_height(self):
        """少し高い場所の場合"""
        result = get_elevation_description(15)
        assert result == "少し高い場所"

    def test_low_ground(self):
        """低地の場合"""
        result = get_elevation_description(5)
        assert result == "低地"

    def test_boundary_30(self):
        """境界値30mの場合"""
        result = get_elevation_description(30)
        assert result == "高台"

    def test_boundary_20(self):
        """境界値20mの場合"""
        result = get_elevation_description(20)
        assert result == "やや高い場所"

    def test_boundary_10(self):
        """境界値10mの場合"""
        result = get_elevation_description(10)
        assert result == "少し高い場所"


class TestCapacityDescription:
    """収容能力説明のテスト"""

    def test_int_max_value(self):
        """int.MaxValueの場合（広い場所）"""
        result = get_capacity_description(2147483647, 0)
        assert result == "広い場所"

    def test_ratio_high(self):
        """残り8割以上の場合"""
        result = get_capacity_description(80, 100)
        assert result == "まだ余裕がある"

    def test_ratio_medium(self):
        """残り半分くらいの場合"""
        result = get_capacity_description(60, 100)
        assert result == "半分くらい埋まっている"

    def test_ratio_low(self):
        """残り少ない場合"""
        result = get_capacity_description(25, 100)
        assert result == "かなり混雑している"

    def test_ratio_very_low(self):
        """ほぼ満員の場合"""
        result = get_capacity_description(10, 100)
        assert result == "ほぼ満員"

    def test_no_max_capacity_large(self):
        """max_capacity無しで残りが多い場合"""
        result = get_capacity_description(600, 0)
        assert result == "まだ余裕がある"

    def test_no_max_capacity_medium(self):
        """max_capacity無しで残りが中程度の場合"""
        result = get_capacity_description(150, 0)
        assert result == "それなりに受け入れ可能"

    def test_no_max_capacity_low(self):
        """max_capacity無しで残りが少ない場合"""
        result = get_capacity_description(40, 0)
        assert result == "あまり余裕がない"

    def test_no_max_capacity_very_low(self):
        """max_capacity無しでほぼ満員の場合"""
        result = get_capacity_description(20, 0)
        assert result == "ほぼ満員"


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
