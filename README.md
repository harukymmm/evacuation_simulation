# LLM-Agents避難シミュレーション

Unity + LLMを組み合わせた避難シミュレーションシステムです。LLMエージェントが避難者のペルソナや環境情報を考慮して、自律的に避難行動を決定します。

## 目次

- [プロジェクト構造](#プロジェクト構造)
- [セットアップ](#セットアップ)
- [主要コンポーネント](#主要コンポーネント)
- [設定ファイル](#設定ファイル)
- [LLM連携の流れ](#llm連携で避難者が行動する流れ)
- [ドキュメント](#ドキュメント)

## プロジェクト構造

```
evacuation_simulation/
├── Assets/
│   ├── Scripts/           # C#スクリプト
│   │   ├── LLM/           # LLM通信関連
│   │   │   ├── LLMDecisionClient.cs    # WebSocket通信クライアント
│   │   │   └── LLMActionMessages.cs    # リクエスト/レスポンス定義
│   │   ├── Evacuee.cs                  # 避難者エージェント
│   │   ├── Shelter.cs                  # 避難所
│   │   ├── ShelterEnvManager.cs        # 避難所環境マネージャー
│   │   ├── EnvironmentalContextProvider.cs  # 環境コンテキスト提供
│   │   ├── AlertManager.cs             # アラートシステム
│   │   ├── EmergencyBroadcastSpeaker.cs # 緊急放送スピーカー
│   │   ├── PersonaData.cs              # ペルソナデータ構造
│   │   ├── FamilyData.cs               # 家族データ構造
│   │   ├── BuildingCategorizer.cs      # 建物カテゴライザ
│   │   └── SpawnLocationManager.cs     # スポーン位置管理
│   ├── Config/            # 設定ファイル
│   │   ├── personas.csv                # ペルソナ設定
│   │   ├── families.csv                # 家族設定
│   │   └── fukushima_evacuation_personas_500.csv  # 福島避難ペルソナ
│   └── Editor/            # Unityエディタ拡張
│       ├── BuildingAttributeAnalyzer.cs    # 建物属性分析
│       ├── BuildingDataExporter.cs         # 建物データエクスポート
│       └── ShelterInfoEditor.cs            # 避難所情報エディタ
├── llm_server/            # Pythonサーバー
│   ├── server.py          # WebSocketサーバー（LLM/ヒューリスティック）
│   ├── models.py          # データモデル
│   ├── persona_maker.py   # ペルソナ生成ツール
│   ├── makeCSV.py         # CSV作成ツール
│   └── .env.example       # 環境変数テンプレート
├── Docs/                  # ドキュメント
└── results/               # 実行結果出力
```

## セットアップ

### 必要環境

- Unity 2022.3以降
- Python 3.10以降
- OpenAI API Key（オプション）

### サーバー起動と環境変数

`llm_server/` フォルダにサンプル実装を追加しました。`.env` に `OPENAI_API_KEY` と `OPENAI_MODEL` を記述すると自動で読み込まれます。

```bash
cd llm_server
cp .env.example .env  # APIキーを設定
pip install -r requirements.txt
python server.py
```

APIキーが未設定の場合は安全にヒューリスティックで応答します。

## 主要コンポーネント

### Unity側

| コンポーネント | 説明 |
|---------------|------|
| `Evacuee` | 避難者エージェント。ペルソナに基づいてLLMに問い合わせ、NavMeshで移動 |
| `Shelter` | 避難所。収容人数、安全度などの属性を持つ |
| `ShelterEnvManager` | シミュレーション全体の環境管理 |
| `EnvironmentalContextProvider` | 周辺建物・危険情報などの環境コンテキストを提供 |
| `AlertManager` | 警報システムの管理 |
| `EmergencyBroadcastSpeaker` | 緊急放送スピーカー |
| `PersonaData` / `FamilyData` | 避難者の属性・家族構成データ |

### Python側

| コンポーネント | 説明 |
|---------------|------|
| `server.py` | WebSocketサーバー。LLMまたはヒューリスティックで避難判断を返す |
| `persona_maker.py` | ペルソナCSVを生成するツール |
| `makeCSV.py` | 各種CSVデータを作成 |

## LLM連携で避難者が行動する流れ

### 1. Unity側で入力を収集

- `ShelterEnvManager` がシミュレーション開始時に全避難所を開放。
- 各 `Evacuee` がトリガーを受け取ると、自身の位置 (`evacuee`) と全避難所 (`shelter_candidates`) を `LLMEvacDecisionRequest` にまとめます。

### 2. Pythonサーバーに送信

- `LLMDecisionClient`（`Assets/Scripts/LLM/`）が WebSocket で JSON を送信。  
- サーバー側（`llm_server/server.py`）は `.env` の `OPENAI_API_KEY` があれば OpenAI を呼び、無ければ「最も近い空き避難所」をヒューリスティックで計算。

### 3. LLM/ヒューリスティックで選択

- サーバーは `{"selected_shelter_id": "...", "reasoning": "...", "confidence": ...}` を返します。
- 必要に応じて `movement_speed`, `route_preferences` などの拡張情報を含めることも可能です。

### 4. Unityで行動に反映

- `Evacuee` が `selected_shelter_id` に対応する建物を検索し、`NavMeshAgent.SetDestination()` で個別に移動を開始します。
- レスポンスに該当建物が見つからない／タイムアウトした場合は、自前の最短距離ロジックにフォールバック。

### 5. フォールバック

- LLMレスポンスが得られない場合でも、`MoveToNearestShelter()` によって即座に最寄り避難所へ向かうため、シミュレーションは止まりません。

### 6. APIキー管理

- `.env` に `OPENAI_API_KEY` を設定すれば自動読込。
- `.gitignore` に `.env` と `llm_server/.env`, `llm_server/.venv` を追加済みで、キーはリポジトリに残りません。

この流れで、LLMの判断結果を即座に避難行動へ反映できるようになっています。

## 設定ファイル

### ペルソナ設定 (`Assets/Config/personas.csv`)

避難者の属性を定義するCSVファイル。年齢、身体能力、性格特性などを設定できます。

### 家族設定 (`Assets/Config/families.csv`)

家族グループの構成を定義。家族単位での避難行動をシミュレートする際に使用します。

### 福島避難ペルソナ (`Assets/Config/fukushima_evacuation_personas_500.csv`)

福島の避難シナリオ用に作成された500人分のペルソナデータ。

## ドキュメント

`Docs/` フォルダに詳細なドキュメントがあります：

| ファイル | 内容 |
|---------|------|
| `LLM_API_Specification.md` | LLM APIの仕様書 |
| `ImplementationPlan.md` | 実装計画書 |
| `Plan.md` | プロジェクト計画 |
| `Priority.md` | 優先度設定 |
| `Proposal.md` | プロジェクト提案書 |

## 理想的な入出力

### 入力（Unity → LLM）の要点

✅ 避難者のペルソナ（年齢、身体能力、性格）、対象避難者の位置  
✅ 利用可能な避難所（距離、収容率、安全度）  
✅ 環境情報（危険要因、混雑状況）  
✅ 記憶（地域知識、過去の行動）  
✅ 利用可能な避難所（位置、収容人数、タグ情報）  
✅ 必要に応じて危険度・混雑状況などの追加コンテキスト

### 出力（LLM → Unity）の要点

✅ 避難者が向かうべき避難所ID  
✅ 任意で移動速度や経路嗜好などの補足情報  
✅ 判断理由と確信度（ログ用）
