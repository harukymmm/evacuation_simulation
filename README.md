# LLM駆動エージェント・ベース・モデル（ABM）避難シミュレーション

Unity上で動作する3D避難シミュレーションシステム。大規模言語モデル（LLM）をエージェントの意思決定エンジンとして活用し、人間らしい避難行動を再現する。東日本大震災を想定した福島県いわき市平豊間地区・平薄磯地区の実環境を対象としている。

## 目次

- [システム概要](#システム概要)
- [アーキテクチャ](#アーキテクチャ)
- [プロジェクト構造](#プロジェクト構造)
- [セットアップ](#セットアップ)
- [主要コンポーネント](#主要コンポーネント)
- [シミュレーションの仕組み](#シミュレーションの仕組み)
- [設定ファイル](#設定ファイル)
- [理想的な入出力](#理想的な入出力)
- [ドキュメント](#ドキュメント)

---

## システム概要

### 技術スタック

| カテゴリ | 技術 |
|---------|------|
| ゲームエンジン | Unity 2022.3+ (C#) |
| 3D都市モデル | PLATEAU SDK for Unity (国土交通省) |
| LLM | OpenAI API (GPT-4o-mini) |
| 通信 | WebSocket (双方向リアルタイム) |
| サーバー | Python 3.10+ (asyncio) |
| データ検証 | Pydantic |
| 経路探索 | Unity NavMesh |

### 主要な特徴

- **LLM意思決定**: 避難者が状況に応じて自律的に避難行動を判断
- **実環境再現**: PLATEAUデータによるいわき市の3D再現（建物、道路、地形、災害リスク区域）
- **大規模シミュレーション**: 最大500体の異なるペルソナを同時実行可能
- **社会的相互作用**: エージェント間の会話、家族連絡、追従行動
- **フォールバック機構**: LLM不在時は自動でヒューリスティック（最短距離）に切替

---

## アーキテクチャ

```
┌─────────────────────────────────────────────────────────────┐
│                      Unity (C#)                              │
├─────────────────────────────────────────────────────────────┤
│  ShelterEnvManager (環境全体制御)                            │
│       ↓                                                      │
│  ┌─────────┐  ┌───────────┐  ┌─────────┐  ┌────────────┐  │
│  │ Evacuee │  │ Shelter   │  │AlertMgr │  │ContextProv │  │
│  │ (避難者) │  │ (避難所)   │  │ (警報)   │  │ (環境情報)  │  │
│  └────┬────┘  └───────────┘  └─────────┘  └────────────┘  │
│       │                                                      │
│       ↓  WebSocket JSON                                      │
│  LLMDecisionClient ←→ RuleBasedDecisionMaker                │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ↓ WebSocket
┌───────────────────────┴─────────────────────────────────────┐
│                   Python LLM サーバー                        │
├─────────────────────────────────────────────────────────────┤
│  server.py                                                   │
│    ├─ プロンプト生成 (ペルソナ + 環境 + 記憶)                 │
│    ├─ OpenAI API 呼び出し                                   │
│    └─ ヒューリスティック (フォールバック)                     │
│                                                              │
│  MemoryManager / MemorySummarizer (長期記憶管理)            │
└─────────────────────────────────────────────────────────────┘
```

---

## プロジェクト構造

```
evacuation_simulation/
├── Assets/
│   ├── Scripts/                    # C#スクリプト
│   │   ├── LLM/                   # LLM通信
│   │   │   ├── LLMDecisionClient.cs       # WebSocketクライアント
│   │   │   └── LLMActionMessages.cs       # リクエスト/レスポンス定義
│   │   ├── Evacuee.cs              # 避難者エージェント (メイン)
│   │   ├── Shelter.cs              # 避難所
│   │   ├── ShelterEnvManager.cs    # シミュレーション環境管理
│   │   ├── EnvironmentalContextProvider.cs  # 空間情報提供
│   │   ├── AlertManager.cs         # 警報・放送管理
│   │   ├── RuleBasedDecisionMaker.cs # ルールベース比較用
│   │   ├── PersonaData.cs          # ペルソナ属性
│   │   ├── FamilyData.cs           # 家族関係
│   │   ├── ExperimentConfig.cs     # 実験設定
│   │   ├── BuildingCategorizer.cs  # 建物分類
│   │   └── SpawnLocationManager.cs # スポーン位置管理
│   ├── Config/                      # 設定ファイル
│   │   ├── personas.csv            # 基本ペルソナ
│   │   ├── families.csv            # 家族グループ
│   │   ├── fukushima_evacuation_personas_500.csv  # 500人分ペルソナ
│   │   ├── scenario.csv            # シナリオ設定
│   │   └── BuildingSpatialIndex.asset  # 建物空間インデックス
│   ├── Scenes/                      # Unityシーン
│   │   ├── Iwaki/                  # いわき市
│   │   └── namie/                  # 浪江町
│   └── Editor/                      # エディタ拡張
├── llm_server/                      # Pythonサーバー
│   ├── server.py                   # メインサーバー
│   ├── models.py                   # Pydanticモデル
│   ├── persona_maker.py            # ペルソナ生成
│   ├── memory_manager.py           # 長期記憶管理
│   ├── memory_summarizer.py        # 記憶要約
│   ├── makeCSV.py                  # CSV生成
│   ├── requirements.txt            # Python依存
│   └── .env                        # 環境変数 (APIキー)
├── Docs/                            # ドキュメント
│   ├── LLM_API_Specification.md    # API仕様書
│   └── ImplementationPlan.md       # 実装計画
├── Logs/                            # 実行ログ
│   └── llm_decisions/              # LLM意思決定ログ
└── results/                         # シミュレーション結果
```

---

## セットアップ

### 必要環境

- Unity 2022.3以降
- Python 3.10以降
- OpenAI API Key（オプション、なくても動作可）

### サーバー起動

```bash
cd llm_server
cp .env.example .env  # APIキーを設定
pip install -r requirements.txt
python server.py
```

### 環境変数 (.env)

```
OPENAI_API_KEY=sk-...
OPENAI_MODEL=gpt-4o-mini
LLM_SERVER_HOST=127.0.0.1
LLM_SERVER_PORT=8765
```

APIキー未設定時はヒューリスティック（最短距離避難所選択）で動作。

---

## 主要コンポーネント

### Unity側 (C#)

| ファイル | 役割 |
|---------|------|
| `Evacuee.cs` | 避難者エージェント本体。LLM/ルールベースで行動決定、NavMeshで移動、体力・ストレス管理 |
| `ShelterEnvManager.cs` | シミュレーション全体制御。避難者スポーン、時間管理、イベント発火 |
| `LLMDecisionClient.cs` | WebSocketクライアント。Python サーバーとの非同期通信 |
| `LLMActionMessages.cs` | JSON リクエスト/レスポンスのデータ構造定義 |
| `EnvironmentalContextProvider.cs` | グリッドベースの空間インデックスで周辺環境情報を高速提供 |
| `AlertManager.cs` | 防災行政無線、Jアラートの管理 |
| `RuleBasedDecisionMaker.cs` | 比較実験用のルールベースエージェント |
| `Shelter.cs` | 避難所。収容人数、容量管理 |
| `PersonaData.cs` | ペルソナデータ構造（年齢、速度、心理状態等） |
| `FamilyData.cs` | 家族関係データ |

### Python側

| ファイル | 役割 |
|---------|------|
| `server.py` | WebSocketサーバー。プロンプト生成、LLM呼び出し、レスポンス返却 |
| `models.py` | Pydanticベースのデータ検証モデル |
| `memory_manager.py` | 長期記憶の管理 |
| `memory_summarizer.py` | 行動履歴・会話履歴の要約 |
| `persona_maker.py` | 人口統計に基づくペルソナ生成 |
| `makeCSV.py` | CSV生成ツール |

---

## シミュレーションの仕組み

### 行動タイプ

避難者が取りうる行動（`LLMActionMessages.cs`で定義）:

| ActionType | 説明 |
|------------|------|
| `EVACUATE` | 指定避難所へ移動 |
| `STAY` | その場で待機 |
| `SEARCH_FAMILY` | 家族を探す |
| `CONTACT` | 家族に連絡を試みる |
| `FOLLOW` | 周囲の人について行く |
| `TALK` | 周辺の避難者と会話 |

### 階層的意思決定

LLMは3層構造で判断:

1. **長期目標**: 「家族全員で◯◯避難所に到達する」
2. **中期計画**: 「まず◯◯小学校で子供を迎える」
3. **即時判断**: 現在の状況に基づく具体的行動

### 災害シナリオ

`scenario.csv`で時系列イベントを定義。本震、余震、津波警報、停電、サイレン等のイベントを時刻指定で発火させる。

### 環境コンテキスト

避難者に提供される情報:

- 周辺建物（名前、位置、用途、構造）
- 災害リスク（津波浸水想定区域、土砂災害警戒区域）
- 現在地の標高
- 最寄りの道路情報

---

## 設定ファイル

### ペルソナCSV (`personas.csv`)

避難者の属性（年齢、移動速度、心理状態、自宅位置、土地勘、過去の災害経験等）を定義。

### 家族CSV (`families.csv`)

家族グループの構成を定義。複数の避難者間の家族関係を設定。

### シナリオCSV (`scenario.csv`)

時間経過に伴うイベント、アラート発信タイミングを定義。

---

## 理想的な入出力

### 入力（Unity → LLM）の要点

- 避難者のペルソナ（年齢、身体能力、性格）、対象避難者の位置
- 利用可能な避難所（距離、収容率、安全度）
- 環境情報（危険要因、混雑状況）
- 記憶（地域知識、過去の行動）
- 利用可能な避難所（位置、収容人数、タグ情報）
- 必要に応じて危険度・混雑状況などの追加コンテキスト

### 出力（LLM → Unity）の要点

- 行動タイプ（EVACUATE/STAY/SEARCH_FAMILY/CONTACT/FOLLOW/TALK）
- 行動対象（避難所ID、家族名、追従対象ID、会話相手ID等）
- 長期目標・中期計画（階層的意思決定）
- 移動速度（SLOW/NORMAL/FAST/RUN）
- 判断理由と確信度

---

## 出力と結果

- `Logs/llm_decisions/`: LLM意思決定ログ (JSON形式)
- `results/`: 避難完了率、避難時間、避難所混雑度推移 (CSV形式)

---

## ドキュメント

| ファイル | 内容 |
|---------|------|
| `Docs/LLM_API_Specification.md` | LLM API仕様書 |
| `Docs/ImplementationPlan.md` | 実装計画書 |
| `Docs/persona_specification.md` | ペルソナ仕様 |
