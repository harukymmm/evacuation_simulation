# 家族情報システム（Family System）

## 概要

各避難者に家族情報を設定し、スポーン位置と探索先を自動的に決定するシステムです。

---

## ファイル構成

```
Assets/
├── Config/
│   ├── personas.csv          # ペルソナ情報（既存）
│   └── families.csv          # 家族情報（新規）
├── Scripts/
│   ├── PersonaData.cs        # ペルソナ管理（既存）
│   ├── FamilyData.cs         # 家族情報管理（新規）
│   ├── BuildingCategorizer.cs # 建物分類（新規）
│   ├── SpawnLocationManager.cs # スポーン位置管理（新規）
│   ├── EnvironmentalContextProvider.cs # 建物情報（拡張）
│   └── Evacuee.cs            # 避難者（拡張）
```

---

## families.csv のフォーマット

### カラム定義

| カラム名 | 型 | 説明 | 例 |
|---------|---|------|---|
| `agent_id` | int | 避難者のID（owner） | 3 |
| `relation` | string | 続柄 | "妻", "息子", "self" |
| `target_agent_id` | int | 家族メンバーのagent_id（-1=NPC） | -1, 3 |
| `target_name` | string | 家族メンバーの名前 | "山本 美咲" |
| `spawn_category` | string | スポーンする建物カテゴリ | "School", "Office" |
| `has_phone` | bool | 連絡手段の有無 | true, false |
| `exists_in_scene` | bool | シーン内に実在するか | true, false |

### relation（続柄）の種類

- `self`: 本人の設定（スポーンカテゴリを指定）
- `妻`, `夫`: 配偶者
- `息子`, `娘`: 子供
- `父`, `母`: 親
- `なし`: 家族なし（単身者）

### spawn_category（建物カテゴリ）

| カテゴリ | 説明 | PLATEAU判定キーワード |
|---------|------|---------------------|
| `Residential` | 住宅 | "住宅", "共同住宅" |
| `School` | 学校 | "学校", "文教" |
| `Office` | オフィス・職場 | その他の業務施設 |
| `Commercial` | 商業施設 | "商業", "店舗" |
| `PublicFacility` | 公共施設 | "官公庁", "公共公益" |
| `Hospital` | 病院 | "病院", "医療" |
| `Other` | その他 | - |

---

## 使用例

### 例1: 父親（山本 剛）- 子供を探している

```csv
3,self,-1,山本 剛,Office,true,true
3,妻,-1,山本 美咲,Residential,true,false
3,息子,-1,山本 健,School,false,false
```

**動作：**

1. 山本 剛は**Office（職場）**にスポーンされる
2. 妻（山本 美咲）は**Residential（住宅）**のどこかに設定される
3. 息子（山本 健）は**School（学校）**のどこかに設定される
4. LLMは家族情報を受け取り、SEARCH_FAMILY行動を選択可能

**LLMが受け取る情報：**

```json
{
  "family_members": [
    {
      "name": "山本 美咲",
      "relation": "妻",
      "likely_location": "住宅（bldg_xxxx）",
      "distance_meters": 520,
      "has_phone": true
    },
    {
      "name": "山本 健",
      "relation": "息子",
      "likely_location": "学校（いわき市立豊間小学校）",
      "distance_meters": 1350,
      "has_phone": false
    }
  ]
}
```

---

### 例2: 高齢者（鈴木 誠）- 独居

```csv
4,self,-1,鈴木 誠,Residential,true,true
```

**動作：**

1. 鈴木 誠は**Residential（住宅）**にスポーンされる
2. 家族メンバーなし
3. LLMは家族情報なしで判断

---

### 例3: 学生（小林 愛）- 母が自宅

```csv
5,self,-1,小林 愛,School,true,true
5,母,-1,小林 由美,Residential,true,false
```

**動作：**

1. 小林 愛は**School（学校）**にスポーンされる
2. 母（小林 由美）は**Residential（住宅）**に設定される
3. 学校から自宅への移動を検討可能

---

## 座標の自動決定

### 建物カテゴリから座標への変換

```
agent_id=3 の場合:
  spawn_category=School
    ↓
  SpawnLocationManager.GetRandomSpawnPosition(School)
    ↓
  シーン内の全建物から "学校" を抽出
    - bldg_abc123 (usage="学校", position=(3800, 0, -1500))
    - bldg_def456 (majorUsage="文教施設", position=(4200, 0, -1800))
    ↓
  ランダムに1つ選択
    → (3800, 0, -1500) いわき市立豊間小学校
    ↓
  この座標に避難者をスポーン
```

---

## システムの流れ

### 1. シーン起動時

```
[EnvironmentalContextProvider]
  ↓ 全建物のPLATEAU属性を読み込み
[SpawnLocationManager]
  ↓ 建物をカテゴリ別に分類
  ↓ Residential: 600件, School: 20件, Office: 150件...
[FamilyManager]
  ↓ families.csvを読み込み
[EnvManager]
  ↓ 避難者をスポーン
  ↓ 各避難者のagent_idから家族情報を取得
  ↓ spawn_categoryに応じた建物にスポーン
  ↓ 家族メンバーの探索先座標も決定
```

### 2. LLM決定時

```
[Evacuee]
  ↓ BuildEvacDecisionRequest()
  ↓ 家族情報をペイロードに含める
[LLMサーバー]
  ↓ プロンプトに家族情報を追加
  ↓ "妻: 山本 美咲（住宅、520m、連絡可能）"
  ↓ LLMが判断
  ↓ action_type="SEARCH_FAMILY" など
[Evacuee]
  ↓ ExecuteSearchAction(target_location)
```

---

## カスタマイズ方法

### 新しい避難者を追加する場合

1. `personas.csv` に新しい行を追加
2. `families.csv` に対応する行を追加
3. 自動的にスポーン位置が決定される

**例：**

```csv
# personas.csv
6,高橋 明,教師,40s_Male,1.0,allowed,責任感,生徒の安全確認,"..."

# families.csv
6,self,-1,高橋 明,School,true,true
6,妻,-1,高橋 恵子,Hospital,true,false
```

→ 高橋 明は学校にスポーン、妻は病院に設定される

---

## 注意事項

1. **EnvironmentalContextProvider は必須**
   - 建物情報の読み込みに使用
   - 自動的に生成されます

2. **SpawnLocationManager は必須**
   - カテゴリ別の建物管理
   - 自動的に生成されます

3. **初期化順序**
   - EnvironmentalContextProvider → SpawnLocationManager → FamilyManager → スポーン

4. **カテゴリに建物がない場合**
   - デフォルトのスポーン位置が使用されます
   - 警告ログが出力されます

---

## デバッグ方法

### ログで確認できる情報

```
[SpawnLocationManager] 建物カテゴリ分類完了:
  - 住宅: 600件
  - 学校: 20件
  - 職場・オフィス: 150件
  ...

[EnvManager] Agent 3: 職場・オフィスにスポーン - bldg_abc123

[Evacuee] Evacuee3(Clone): 家族情報設定 - 2人の家族

[Evacuee] Evacuee3(Clone): 家族情報をLLMリクエストに含めます（2人）
```

---

## 今後の拡張

- ✅ SEARCH_FAMILY行動の実装
- ✅ CONTACT行動の実装
- ✅ 家族発見のトリガー
- ✅ 実在する家族メンバーとの合流

このシステムにより、各避難者は自動的に適切な場所にスポーンされ、家族情報を持った状態でLLMによる意思決定が行われます。





