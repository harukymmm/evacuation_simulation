# LLMエージェント記憶管理システム仕様書

## 1. 概要

本ドキュメントは、避難シミュレーションにおけるLLMエージェントの記憶管理システムの設計仕様を定義する。

### 1.1 記憶システムの目的
- エージェントに連続的な行動の一貫性を持たせる
- 過去の経験や地域知識を意思決定に反映させる
- 現実的な避難行動パターンの再現（経験に基づく判断等）

### 1.2 記憶の分類

本システムでは記憶を2種類に分類する：

| 記憶タイプ | 説明 | 保持期間 | 管理方式 |
|-----------|------|----------|----------|
| 短期記憶 | シミュレーション中の行動履歴・会話履歴 | シミュレーション実行中 | 直近N件保持 |
| 長期記憶 | 地域知識・ペルソナ情報・過去の災害経験 | 永続（JSON） | ベクトル検索（RAG） |

### 1.3 関連ファイル一覧

| ファイル | 説明 |
|---------|------|
| `llm_server/memory_summarizer.py` | 短期記憶管理モジュール |
| `llm_server/memory_manager.py` | 長期記憶管理モジュール（RAG） |
| `llm_server/data/memories.json` | 長期記憶データ（本番用） |
| `llm_server/data/memories_sample.json` | 長期記憶データ（サンプル） |
| `llm_server/server.py` | 記憶統合・プロンプト生成 |
| `Assets/Scripts/LLM/LLMActionMessages.cs` | 行動履歴データ構造（C#） |
| `Assets/Scripts/Evacuee.cs` | 行動履歴の記録処理（C#） |

---

## 2. 短期記憶（行動履歴）

### 2.1 概要

短期記憶は、シミュレーション実行中のエージェントの行動履歴を管理する。過去の行動との一貫性を保つため、直近の履歴をプロンプトに含める。

### 2.2 設計方針

- **LLM APIを使用しない**: レート制限を回避するため、要約処理にLLM APIを使用せず、直近N件をそのまま保持
- **件数制限**: プロンプトサイズとコストを抑えるため、履歴件数を制限

### 2.3 保持件数

| 履歴タイプ | 保持件数 | 定義場所 |
|-----------|----------|----------|
| 行動履歴 | 直近3件 | `memory_summarizer.py: MAX_RECENT_ACTIONS` |
| 会話履歴 | 直近5件 | `memory_summarizer.py: MAX_RECENT_CONVERSATIONS` |
| 連絡履歴 | 直近5件 | `memory_summarizer.py: MAX_RECENT_CONTACTS` |

### 2.4 データ構造

#### 2.4.1 行動履歴エントリ（ActionHistoryEntry）

```csharp
// LLMActionMessages.cs
public class ActionHistoryEntry
{
    public float timestamp;      // 行動時刻（シミュレーション内秒数）
    public string action_type;   // 行動タイプ（EVACUATE, STAY, etc.）
    public string target;        // 対象（避難所名、家族名など）
    public string reasoning;     // 判断理由
    public string result;        // 結果（completed, failed, interrupted）
}
```

#### 2.4.2 行動履歴ペイロード（ActionHistoryPayload）

```csharp
// LLMActionMessages.cs
public class ActionHistoryPayload
{
    public ActionHistoryEntry[] recent_actions;  // 直近の行動（最大5件）
    public string summarized_history;            // 要約済み過去履歴（現在未使用）
    public int total_action_count;               // 総行動回数
}
```

### 2.5 Unity側の処理フロー

1. **行動記録**: `Evacuee.AddActionToHistory()` で行動を履歴に追加
2. **件数制限**: `MAX_ACTION_HISTORY`（5件）を超えたら古いものから削除
3. **ペイロード構築**: `Evacuee.BuildActionHistoryPayload()` でリクエスト用データを構築
4. **サーバー送信**: WebSocket経由でサーバーに送信

### 2.6 サーバー側の処理

```python
# server.py: build_user_prompt()
action_history = payload.get("action_history")
if action_history:
    all_actions = action_history.get("recent_actions", [])
    recent_actions = get_recent_actions(all_actions, max_count=3)  # 直近3件のみ
```

### 2.7 プロンプト出力例

```
【直近の行動履歴】
これまでに5回の行動判断を行っています。直近3件:

- 120秒: 避難（豊間小学校） - 津波警報を受けて高台の避難所に向かうことを決定...
- 90秒: 連絡（妻） - 家族の安否を確認するためメールを送信
- 60秒: 待機 - 揺れが収まるまで様子を見ることを決定

※ 過去の行動と一貫性を持ちつつ、現在の状況に適した判断をしてください。
```

---

## 3. 長期記憶（RAG）

### 3.1 概要

長期記憶は、エージェントが持つ永続的な知識や経験を管理する。ベクトル検索（RAG: Retrieval-Augmented Generation）により、現在の状況に関連する記憶を取得してプロンプトに含める。

### 3.2 記憶タイプ

| memory_type | 説明 | アクセス制御 |
|-------------|------|-------------|
| `regional_knowledge` | 地域知識（避難所、危険区域等） | 全エージェント共有（agent_id: null） |
| `persona_knowledge` | 自己認識（役割、身体状態等） | エージェント個別（agent_id: N） |
| `disaster_experience` | 過去の災害経験 | エージェント個別（agent_id: N） |

### 3.3 データ形式（memories.json）

```json
[
  {
    "memory_type": "regional_knowledge",
    "agent_id": null,
    "content": "豊間小学校は海抜約20mに位置し、津波避難ビルとして指定されています。",
    "metadata": {"location": "豊間小学校", "elevation": 20}
  },
  {
    "memory_type": "persona_knowledge",
    "agent_id": 1,
    "content": "私は施設管理者として働いています。緊急時には他者の誘導を優先します。",
    "metadata": {"role": "facility_manager", "priority": "guide_others"}
  },
  {
    "memory_type": "disaster_experience",
    "agent_id": 1,
    "content": "2011年の震災では、津波が想定より早く到達しました。",
    "metadata": {"event": "2011_tsunami", "lesson": "early_evacuation"}
  }
]
```

### 3.4 ベクトル検索の仕組み

#### 3.4.1 使用技術

| 項目 | 値 |
|------|-----|
| Embedding API | OpenAI text-embedding-3-small |
| ベクトル次元数 | 1536 |
| 類似度計算 | コサイン類似度 |
| 実装 | numpy + JSON（軽量構成） |

#### 3.4.2 検索パラメータ

| パラメータ | デフォルト値 | 説明 |
|-----------|-------------|------|
| `top_k` | 3 | 返す最大件数 |
| `threshold` | 0.75 | 類似度閾値（0.0-1.0） |

### 3.5 アクセス制御

```python
# memory_manager.py: search()
# agent_idでフィルタ（共通知識 or 特定エージェント）
mem_agent_id = mem.get('agent_id')
if agent_id is not None:
    # agent_idがNull（共通知識）または一致するもののみ
    if mem_agent_id is not None and mem_agent_id != agent_id:
        continue
```

- `agent_id: null` → 全エージェントがアクセス可能（地域知識）
- `agent_id: N` → エージェントNのみアクセス可能（個人記憶）

### 3.6 初期化フロー

1. サーバー起動時に`initialize_memory_manager()`を呼び出し
2. `memories.json`（なければ`memories_sample.json`）を読み込み
3. 全記憶をEmbedding APIでベクトル化
4. numpy配列としてメモリに保持

### 3.7 検索フロー

1. リクエスト受信時に`scenario_id`と`persona`からクエリを構築
2. クエリをベクトル化
3. コサイン類似度で全記憶と比較
4. `agent_id`でフィルタリング
5. 閾値以上の記憶を類似度順に`top_k`件返す

### 3.8 プロンプト出力例

```
【あなたの記憶・知識（長期記憶）】
以下は現在の状況に関連する、あなたが持っている知識や経験です:

[地域知識] 豊間小学校は海抜約20mに位置し、津波避難ビルとして指定されています。
[過去の経験] 2011年の震災では、津波が想定より早く到達しました。「まだ大丈夫」という油断が命取りになることを学びました。
[自己認識] 私は施設管理者として働いています。緊急時には他者の誘導を優先します。

※ これらの知識や経験を活かして判断してください。
```

---

## 4. 実装詳細

### 4.1 memory_summarizer.py

短期記憶管理のための軽量モジュール。

```python
# 保持する履歴の最大件数
MAX_RECENT_ACTIONS = 3          # 直近の行動履歴
MAX_RECENT_CONVERSATIONS = 5    # 直近の会話履歴
MAX_RECENT_CONTACTS = 5         # 直近の連絡履歴

def get_recent_actions(action_history, max_count=MAX_RECENT_ACTIONS):
    """直近の行動履歴を取得"""
    return action_history[-max_count:] if action_history else []

def get_recent_conversations(conversation_history, max_count=MAX_RECENT_CONVERSATIONS):
    """直近の会話履歴を取得"""
    return conversation_history[-max_count:] if conversation_history else []

def get_recent_contacts(contact_history, max_count=MAX_RECENT_CONTACTS):
    """直近の連絡履歴を取得"""
    return contact_history[-max_count:] if contact_history else []
```

### 4.2 memory_manager.py

長期記憶管理（RAG）のためのモジュール。

```python
class MemoryManager:
    async def load_memories(self, json_path: str) -> bool:
        """JSONファイルから記憶を読み込み、ベクトル化"""

    async def search(
        self,
        query: str,
        agent_id: Optional[int] = None,
        memory_types: Optional[List[str]] = None,
        top_k: int = 3,
        threshold: float = 0.75
    ) -> List[Dict[str, Any]]:
        """関連する記憶を検索"""

    def get_statistics(self) -> Dict[str, Any]:
        """記憶の統計情報を取得"""

# グローバル関数
async def initialize_memory_manager(openai_client, memories_path=None):
    """メモリマネージャーを初期化"""

def get_memory_manager():
    """グローバルなMemoryManagerインスタンスを取得"""
```

### 4.3 server.py での統合

```python
# 起動時の初期化
async def main():
    memory_manager = await initialize_memory_manager(OPENAI_CLIENT)

# リクエスト処理時の検索
async def process_payload(payload):
    memory_manager = get_memory_manager()
    if memory_manager and memory_manager.is_initialized:
        memories = await memory_manager.search(
            query=query,
            agent_id=agent_id_int,
            top_k=3,
            threshold=0.7
        )
        if memories:
            payload["long_term_memories"] = memories
```

---

## 5. 設定とチューニング

### 5.1 短期記憶の件数調整

`memory_summarizer.py`の定数を変更：

```python
MAX_RECENT_ACTIONS = 3          # 行動履歴件数
MAX_RECENT_CONVERSATIONS = 5    # 会話履歴件数
MAX_RECENT_CONTACTS = 5         # 連絡履歴件数
```

### 5.2 長期記憶の検索パラメータ調整

`memory_manager.py`の定数を変更：

```python
SIMILARITY_THRESHOLD = 0.75     # 類似度閾値
DEFAULT_TOP_K = 3               # 返す最大件数
```

または`search()`呼び出し時に指定：

```python
memories = await memory_manager.search(
    query=query,
    agent_id=agent_id,
    top_k=5,          # 最大5件
    threshold=0.6     # 閾値を下げる
)
```

---

## 6. 今後の拡張可能性

### 6.1 短期記憶
- 重要度に基づく選択的保持
- 行動パターンの統計的分析

### 6.2 長期記憶
- 大規模データ対応（FAISS、Elasticsearch等）
- 動的な記憶追加（シミュレーション中の学習）
- 記憶の時間減衰

---

## 7. 依存関係

### 7.1 Python パッケージ

```
# requirements.txt
openai>=1.35.0    # Embedding API
numpy>=1.24.0     # ベクトル計算
```

### 7.2 外部サービス

| サービス | 用途 | 必須/任意 |
|---------|------|----------|
| OpenAI API | Embedding生成 | 長期記憶使用時は必須 |
