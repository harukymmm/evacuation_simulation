# LLM-Agents避難シミュレーション

## サーバー起動と環境変数

`llm_server/` フォルダにサンプル実装を追加しました。`.env` に `OPENAI_API_KEY` と `OPENAI_MODEL` を記述すると自動で読み込まれます。

```bash
cd llm_server
cp .env.example .env  # APIキーを設定
pip install -r requirements.txt
python server.py
```

APIキーが未設定の場合は安全にヒューリスティックで応答します。

## LLM連携で避難者が行動する流れ

### 1. Unity側で入力を収集

- `ShelterManagementAgent` は全避難所を開放した状態で `OnDidActioned` を発火。
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

この流れで、ML-Agentsの入出力を保ったまま LLM の判断結果を即座に行動へ反映できるようになっています。

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
