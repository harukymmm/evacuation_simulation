# Building Data Exporter - 使い方

## 概要

Unity上のPLATEAU建物オブジェクトから属性情報と位置座標を抽出し、JSON/CSV形式で出力するツールです。

## 実行方法

1. Unityエディタで対象のシーンを開く
2. メニューバーから `Tools > PLATEAU > Export Building Data` を選択
3. エクスポート設定ウィンドウが開く
4. 必要に応じて設定を変更
   - **出力ディレクトリ**: ファイルの保存先（デフォルト: `Assets/ExportedData`）
   - **ファイル名プレフィックス**: 出力ファイル名の接頭辞
   - **JSON形式で出力**: JSON形式での出力ON/OFF
   - **CSV形式で出力**: CSV形式での出力ON/OFF
   - **詳細属性を含める**: 詳細な属性情報を含めるかどうか
5. `建物データを出力` ボタンをクリック
6. 処理完了後、出力先フォルダが自動的に開く

## 出力ファイル形式

### JSON形式

```json
{
  "export_datetime": "2025-12-12 15:30:45",
  "total_count": 150,
  "buildings": [
    {
      "object_name": "bldg_a52084a1-c53f-4a1d-afc8-e1295c2438c5",
      "gml_id": "bldg_a52084a1-c53f-4a1d-afc8-e1295c2438c5",
      "city_object_type": "Building",
      "bounds_center": {
        "x": 284.276,
        "y": 5.2,
        "z": -310.852
      },
      "bounds_min": {
        "x": 276.102,
        "y": 0.0,
        "z": -318.341
      },
      "bounds_max": {
        "x": 292.450,
        "y": 10.4,
        "z": -303.362
      },
      "usage": "住宅",
      "measured_height": 10.4,
      "storeys_above_ground": 3,
      "year_of_construction": 2010,
      "city": "福島県いわき市",
      "prefecture": "福島県"
    }
  ]
}
```

### CSV形式

カンマ区切りのテキストファイル。Excel等で開けます。

**含まれる列:**

- `object_name`: オブジェクト名
- `gml_id`: GML ID
- `city_object_type`: 地物種別
- `bounds_center_x/y/z`: 境界ボックス中心座標
- `bounds_min_x/y/z`: 境界ボックス最小座標
- `bounds_max_x/y/z`: 境界ボックス最大座標
- `bounds_size_x/y/z`: 境界ボックスサイズ
- `usage`: 建物用途
- `measured_height`: 建物高さ
- `storeys_above_ground`: 地上階数
- `year_of_construction`: 建築年
- `total_floor_area`: 床総面積
- `building_structure_type`: 構造種別
- `land_use_type`: 土地利用種別
- その他多数の属性

## 活用例

### 1. RAGシステムへの入力データとして

出力されたJSONファイルをベクトルデータベース（Chroma, FAISS等）に読み込み、セマンティック検索に利用。

```python
import json
import chromadb
from sentence_transformers import SentenceTransformer

# JSONファイルを読み込み
with open('building_data_20251212_153045.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# ベクトル化してChromaDBに格納
model = SentenceTransformer('paraphrase-multilingual-mpnet-base-v2')
collection = chroma_client.create_collection("buildings")

for building in data['buildings']:
    text = f"{building['usage']} {building['storeys_above_ground']}階建て {building['object_name']}"
    embedding = model.encode(text)
    collection.add(
        embeddings=[embedding],
        documents=[text],
        metadatas=building,
        ids=[building['object_name']]
    )
```

### 2. 空間インデックスの構築

位置座標を使って空間分割データ構造を構築。

### 3. データ分析

CSVファイルをPandas等で読み込んで統計分析。

```python
import pandas as pd

df = pd.read_csv('building_data_20251212_153045.csv')
print(df.groupby('usage')['measured_height'].mean())
```

### 4. GISツールとの連携

座標データをQGIS等のGISツールにインポート。

## 注意事項

- シーンに`PLATEAUCityObjectGroup`コンポーネントを持つオブジェクトが必要です
- 大規模なシーン（数千～数万の建物）では処理に時間がかかる場合があります
- 属性情報が欠落している建物は「不明」または0として記録されます

## トラブルシューティング

**Q: 「シーン内にPLATEAU建物オブジェクトが見つかりませんでした」と表示される**
A: シーンにPLATEAUデータがインポートされているか確認してください。

**Q: 一部の建物で属性情報が取得できない**
A: PLATEAU SDKのバージョンやデータ形式によっては、一部の属性が取得できない場合があります。

**Q: 出力ファイルが大きすぎる**
A: 「詳細属性を含める」をOFFにすると、ファイルサイズを削減できます。

## 更新履歴

- 2025-12-12: 初版作成
