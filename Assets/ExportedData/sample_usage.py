"""
Building Data Exporter の出力データを活用するサンプルスクリプト

必要なライブラリ:
pip install pandas chromadb sentence-transformers
"""

import json
import pandas as pd
from pathlib import Path
from typing import List, Dict


class BuildingDataAnalyzer:
    """建物データの分析クラス"""

    def __init__(self, json_path: str):
        """
        Args:
            json_path: JSONファイルのパス
        """
        with open(json_path, "r", encoding="utf-8") as f:
            self.data = json.load(f)

        self.buildings = self.data["buildings"]
        self.df = pd.DataFrame(self.buildings)

        print(f"読み込み完了: {len(self.buildings)}件の建物データ")

    def get_buildings_in_radius(
        self, center_x: float, center_z: float, radius: float
    ) -> List[Dict]:
        """
        指定座標から半径内の建物を取得

        Args:
            center_x: 中心座標X
            center_z: 中心座標Z
            radius: 検索半径

        Returns:
            半径内の建物リスト
        """
        results = []
        for building in self.buildings:
            if building.get("bounds_center"):
                bx = building["bounds_center"]["x"]
                bz = building["bounds_center"]["z"]
                distance = ((bx - center_x) ** 2 + (bz - center_z) ** 2) ** 0.5

                if distance <= radius:
                    building_copy = building.copy()
                    building_copy["distance"] = distance
                    results.append(building_copy)

        # 距離でソート
        results.sort(key=lambda b: b["distance"])
        return results

    def get_buildings_by_usage(self, usage_keyword: str) -> List[Dict]:
        """
        用途で建物を検索

        Args:
            usage_keyword: 用途のキーワード（例: "住宅", "商業"）

        Returns:
            該当する建物リスト
        """
        return [b for b in self.buildings if usage_keyword in b.get("usage", "")]

    def analyze_statistics(self):
        """統計情報を表示"""
        print("\n=== 建物統計 ===")
        print(f"総建物数: {len(self.buildings)}件")

        # 用途別の集計
        if "usage" in self.df.columns:
            print("\n【用途別の建物数】")
            usage_counts = self.df["usage"].value_counts()
            for usage, count in usage_counts.head(10).items():
                print(f"  {usage}: {count}件")

        # 高さの統計
        if "measured_height" in self.df.columns:
            heights = self.df["measured_height"][self.df["measured_height"] > 0]
            if not heights.empty:
                print(f"\n【建物高さ】")
                print(f"  平均: {heights.mean():.2f}m")
                print(f"  最小: {heights.min():.2f}m")
                print(f"  最大: {heights.max():.2f}m")

        # 階数の統計
        if "storeys_above_ground" in self.df.columns:
            floors = self.df["storeys_above_ground"][
                self.df["storeys_above_ground"] > 0
            ]
            if not floors.empty:
                print(f"\n【地上階数】")
                print(f"  平均: {floors.mean():.2f}階")
                print(f"  最小: {int(floors.min())}階")
                print(f"  最大: {int(floors.max())}階")

    def export_filtered_csv(self, filter_func, output_path: str):
        """
        フィルタリングした結果をCSVで出力

        Args:
            filter_func: フィルタ関数
            output_path: 出力先パス
        """
        filtered = [b for b in self.buildings if filter_func(b)]
        df_filtered = pd.DataFrame(filtered)
        df_filtered.to_csv(output_path, index=False, encoding="utf-8-sig")
        print(f"フィルタリング結果を出力: {output_path} ({len(filtered)}件)")


class BuildingRAGSystem:
    """建物データを使ったRAGシステムの簡易実装"""

    def __init__(self, json_path: str):
        """
        Args:
            json_path: JSONファイルのパス
        """
        try:
            import chromadb
            from sentence_transformers import SentenceTransformer
        except ImportError:
            print("エラー: chromadbとsentence-transformersが必要です")
            print("インストール: pip install chromadb sentence-transformers")
            raise

        with open(json_path, "r", encoding="utf-8") as f:
            self.data = json.load(f)

        self.buildings = self.data["buildings"]
        self.model = SentenceTransformer("paraphrase-multilingual-mpnet-base-v2")

        # ChromaDBの初期化
        self.client = chromadb.Client()
        self.collection = self.client.create_collection("buildings")

        # データを登録
        self._build_index()

    def _build_index(self):
        """ベクトルインデックスを構築"""
        print("ベクトルインデックスを構築中...")

        documents = []
        metadatas = []
        ids = []

        for i, building in enumerate(self.buildings):
            # テキスト化
            text = self._building_to_text(building)
            documents.append(text)

            # メタデータ
            metadata = {
                "object_name": building.get("object_name", ""),
                "usage": building.get("usage", "不明"),
                "height": building.get("measured_height", 0),
                "floors": building.get("storeys_above_ground", 0),
                "x": building.get("bounds_center", {}).get("x", 0),
                "y": building.get("bounds_center", {}).get("y", 0),
                "z": building.get("bounds_center", {}).get("z", 0),
            }
            metadatas.append(metadata)
            ids.append(str(i))

        # 埋め込みベクトルを生成
        embeddings = self.model.encode(documents).tolist()

        # ChromaDBに登録
        self.collection.add(
            embeddings=embeddings, documents=documents, metadatas=metadatas, ids=ids
        )

        print(f"インデックス構築完了: {len(documents)}件")

    def _building_to_text(self, building: Dict) -> str:
        """建物情報をテキスト化"""
        usage = building.get("usage", "不明")
        floors = building.get("storeys_above_ground", 0)
        height = building.get("measured_height", 0)
        structure = building.get("building_structure_type", "")

        text = f"{usage}"
        if floors > 0:
            text += f" {floors}階建て"
        if height > 0:
            text += f" 高さ{height:.1f}m"
        if structure:
            text += f" {structure}"

        return text

    def search(self, query: str, n_results: int = 5) -> List[Dict]:
        """
        セマンティック検索

        Args:
            query: 検索クエリ（例: "高い建物", "病院", "住宅"）
            n_results: 取得する結果数

        Returns:
            検索結果のリスト
        """
        query_embedding = self.model.encode([query]).tolist()

        results = self.collection.query(
            query_embeddings=query_embedding, n_results=n_results
        )

        return [
            {"document": doc, "metadata": meta, "distance": dist}
            for doc, meta, dist in zip(
                results["documents"][0],
                results["metadatas"][0],
                results["distances"][0],
            )
        ]


# ==================== 使用例 ====================


def example_basic_analysis():
    """基本的な分析の例"""
    # JSONファイルのパスを指定
    json_path = "building_data_20251212_153045.json"  # 実際のファイル名に変更

    if not Path(json_path).exists():
        print(f"エラー: {json_path} が見つかりません")
        return

    analyzer = BuildingDataAnalyzer(json_path)

    # 統計情報を表示
    analyzer.analyze_statistics()

    # 特定座標から半径100m以内の建物を検索
    print("\n=== 座標(0, 0)から半径100m以内の建物 ===")
    nearby = analyzer.get_buildings_in_radius(0, 0, 100)
    for building in nearby[:5]:  # 上位5件
        print(
            f"  {building['object_name']}: {building['distance']:.2f}m, {building['usage']}"
        )

    # 住宅建物のみを抽出してCSV出力
    analyzer.export_filtered_csv(
        lambda b: "住宅" in b.get("usage", ""), "residential_buildings.csv"
    )


def example_rag_search():
    """RAGシステムの例"""
    json_path = "building_data_20251212_153045.json"  # 実際のファイル名に変更

    if not Path(json_path).exists():
        print(f"エラー: {json_path} が見つかりません")
        return

    try:
        rag = BuildingRAGSystem(json_path)

        # セマンティック検索の例
        queries = ["高い建物", "病院や医療施設", "古い建物", "大きな建物"]

        for query in queries:
            print(f"\n=== 検索: '{query}' ===")
            results = rag.search(query, n_results=3)
            for i, result in enumerate(results, 1):
                meta = result["metadata"]
                print(
                    f"{i}. {meta['usage']} ({meta['floors']}階, {meta['height']:.1f}m)"
                )
                print(f"   位置: ({meta['x']:.1f}, {meta['y']:.1f}, {meta['z']:.1f})")
                print(f"   類似度: {1 - result['distance']:.3f}")

    except ImportError as e:
        print("RAG機能を使用するには、以下をインストールしてください:")
        print("pip install chromadb sentence-transformers")


if __name__ == "__main__":
    print("Building Data Analyzer - サンプルスクリプト\n")

    # 基本的な分析
    print("=" * 60)
    print("【例1】基本的なデータ分析")
    print("=" * 60)
    example_basic_analysis()

    # RAGシステム（オプション）
    print("\n" + "=" * 60)
    print("【例2】RAGシステムによるセマンティック検索")
    print("=" * 60)
    example_rag_search()
