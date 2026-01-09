# 評価機能仕様書

## 概要

本シミュレーションでは、避難者エージェントの行動と避難結果を評価・記録するための複数のログシステムを実装している。これらのシステムにより、実験結果の分析やLLMエージェントとルールベースエージェントの比較が可能となる。

## ログディレクトリ構造

すべてのログは `Logs/` ディレクトリ（プロジェクトルート直下）に統一して出力される。SimulationMetricsによる評価ログとLLMサーバーの意思決定ログは、同一の実験セッションディレクトリに統合して出力される。

```
Logs/
├── experiment_results/           # 実験結果（SimulationMetrics + LLMサーバー）
│   └── YYYYMMDD_HHMMSS/         # 実験セッションごとのディレクトリ
│       ├── episode_X_actions.csv # エピソードごとの行動ログ
│       ├── episode_X_agents.csv  # エピソードごとのエージェント別メトリクス
│       ├── episode_X_summary.csv # エピソードサマリ
│       └── llm_decisions/        # LLMサーバーの意思決定ログ（同セッション）
│           └── {evacuee_id}.txt  # 避難者ごとの詳細ログ
├── YYYY_MM_DD-HH_MM_SS/          # EnvManagerのセッションログ
│   └── EvaRatesPerSec_Episode_X.csv  # 避難率の時系列データ
└── building_stats/               # 建物属性の統計情報
```

**ログ統合の仕組み:**

- `SimulationMetrics`が実験ID（`YYYYMMDD_HHMMSS`形式）を生成
- 避難者（`Evacuee`）がLLMリクエスト時に`experiment_id`フィールドで実験IDをサーバーに送信
- LLMサーバーは受信した実験IDに対応するディレクトリにログを出力

---

## 評価コンポーネント

### 1. SimulationMetrics

シミュレーションの評価指標を計測・記録する主要コンポーネント。

**ファイル:** `Assets/Scripts/SimulationMetrics.cs`

#### 設定パラメータ

| パラメータ | 型 | デフォルト値 | 説明 |
|---|---|---|---|
| `EnableMetrics` | bool | true | 指標記録の有効/無効 |
| `OutputDirectory` | string | "experiment_results" | 出力先サブディレクトリ名 |
| `EvacuationTimeLimit` | float | 1800 | 避難完了判定の制限時間（秒） |

#### ライフサイクル

1. **Awake**: EnvManagerのイベントハンドラに登録、実験IDを生成
2. **OnEpisodeStart**: 各種メトリクスを初期化
3. **シミュレーション中**: `RecordAction`/`RecordEvacuation`で行動・避難完了を記録
4. **OnEpisodeEnd**: ログファイルを出力

#### 出力ファイル

##### episode_X_actions.csv（行動ログ）

避難者の各行動を時系列で記録。

| カラム | 型 | 説明 |
|---|---|---|
| timestamp | float | シミュレーション開始からの経過時間（秒） |
| agent_id | string | 避難者ID |
| action_type | string | 行動タイプ（EVACUATE, STAY, SEARCH_FAMILY, CONTACT, FOLLOW, TALK） |
| target_shelter | string | 避難先（EVACUATEの場合） |
| reasoning | string | LLMの判断理由 |
| confidence | float | 確信度（0.0-1.0） |
| primary_goal | string | 長期目標（階層的意思決定使用時） |
| plan_steps | string | 中期計画のステップ（パイプ区切り） |
| goal_updated | bool | 長期目標更新フラグ |
| plan_updated | bool | 中期計画更新フラグ |

##### episode_X_agents.csv（エージェント別メトリクス）

各避難者の行動特性と結果を記録。

| カラム | 型 | 説明 |
|---|---|---|
| agent_id | string | 避難者ID |
| persona_name | string | ペルソナ名 |
| mental_state | string | 心理状態（バイアス条件） |
| first_evacuate_time | float | 最初のEVACUATE時刻（-1は未避難） |
| total_stay_duration | float | STAY継続時間の合計（秒） |
| follow_count | int | FOLLOW行動の選択回数 |
| talk_count | int | TALK行動の選択回数 |
| contact_count | int | CONTACT行動の選択回数 |
| search_family_count | int | SEARCH_FAMILY行動の選択回数 |
| goal_update_count | int | 長期目標の更新回数 |
| plan_update_count | int | 中期計画の更新回数 |
| evacuation_completed | bool | 避難完了したか |
| evacuation_time | float | 避難完了時間（-1は未完了） |
| final_shelter | string | 到達した避難所名 |

##### episode_X_summary.csv（エピソードサマリ）

エピソード全体の統計を記録。

**基本指標:**
| 指標 | 説明 |
|---|---|
| episode_id | エピソード番号 |
| agent_type | エージェントタイプ（LLM or RuleBased） |
| evacuation_rate | 避難完了率（0.0-1.0） |
| average_evacuation_time | 平均避難時間（秒） |
| median_evacuation_time | 中央値避難時間（秒） |
| max_evacuation_time | 最大避難時間（秒） |
| total_agents | 総エージェント数 |
| evacuated_agents | 避難完了エージェント数 |

**行動分布（action_type, ratio）:**
各行動タイプの選択比率

**避難所分布（shelter, count）:**
避難所ごとの選択回数

---

### 2. LLMサーバーログ

LLMの意思決定プロセスを詳細に記録。

**ファイル:** `llm_server/server.py`

#### 出力形式

各避難者ごとに `{evacuee_id}.txt` ファイルが作成され、以下の形式で追記される:

```
================================================================================
{
  "timestamp": "2026-01-09 23:30:45",
  "request_id": "req-123456",
  "source": "llm",
  "input": { ... },
  "output": {
    "action_type": "EVACUATE",
    "reasoning": "...",
    "confidence": 0.9,
    "selected_shelter_id": "豊間中学校",
    "desired_speed": "NORMAL"
  }
}

--------------------------------------------------------------------------------
[PROMPT]
--------------------------------------------------------------------------------
【あなたのペルソナ】
名前: 佐藤 健太
...
```

---

### 3. EnvManagerログ

避難率の時系列変化を記録。

**ファイル:** `Assets/Scripts/ShelterEnvManager.cs`

#### 出力ファイル

##### EvaRatesPerSec_Episode_X.csv

| カラム | 説明 |
|---|---|
| Time | 経過時間（秒） |
| EvacuationRate | その時点の避難完了率 |

---

## 行動タイプ

評価対象となる避難者の行動タイプ（`LLM.ActionType`）:

| 行動タイプ | 説明 |
|---|---|
| `EVACUATE` | 避難所に向かう |
| `STAY` | その場で待機する |
| `SEARCH_FAMILY` | 家族を探しに行く |
| `CONTACT` | 家族に連絡を取る |
| `FOLLOW` | 周囲の避難者について行く |
| `TALK` | 周辺の避難者と会話する |

---

## 記録フロー

### 行動記録

```
[Evacuee]
    │
    ├─(LLM意思決定)→ LLMサーバー → llm_decisions/{id}.txt
    │
    └─(行動実行)→ SimulationMetrics.RecordAction()
                        │
                        ├→ _actionLogs に追加
                        ├→ _actionCounts を更新
                        └→ _agentMetrics を更新
```

### 避難完了記録

```
[避難者がShelter/TsunamiEvacuationAreaに到達]
    │
    └→ RecordEvacuationToMetrics()
            │
            └→ SimulationMetrics.RecordEvacuation()
                    │
                    ├→ _evacuationRecords に追加
                    └→ _agentMetrics を更新
```

### エピソード終了時

```
[EnvManager.OnEndEpisode]
    │
    ├→ SimulationMetrics.OnEpisodeEnd()
    │       │
    │       ├→ CollectEvacuationRecords()  # 未避難者の記録
    │       ├→ FinalizeAgentMetrics()      # STAY継続時間の確定
    │       ├→ GenerateEpisodeSummary()
    │       ├→ SaveEpisodeLogs()           # actions.csv
    │       ├→ SaveEpisodeSummary()        # summary.csv
    │       └→ SaveAgentMetrics()          # agents.csv
    │
    └→ ShelterManagementAgent.OnEndEpisode()
            │
            └→ (AlwaysActivateAllShelters=falseの場合のみ)
                    Utils.SaveResultCSV() → ActionLog_Episode_X.csv
```

---

## 評価指標の定義

### 避難完了率（Evacuation Rate）

```
避難完了率 = 避難完了者数 / 総避難者数
```

避難完了の判定:
- 避難所（Shelter）または津波避難地域（TsunamiEvacuationArea）に到達
- 制限時間内（デフォルト1800秒=30分）に到達した場合のみ `completedInTime = true`

### 避難時間

シミュレーション開始から避難所到達までの経過時間（秒）。

### 行動分布

各行動タイプの選択回数の比率。LLMエージェントの意思決定傾向を分析するために使用。

---

## 自動生成

`SimulationMetrics`コンポーネントはシーン内に存在しない場合、`EnvManager`によって自動生成される:

```csharp
private void EnsureSimulationMetrics()
{
    var existing = FindFirstObjectByType<SimulationMetrics>();
    if (existing == null)
    {
        GameObject metricsObj = new GameObject("SimulationMetrics");
        metricsObj.transform.SetParent(transform);
        metricsObj.AddComponent<SimulationMetrics>();
    }
}
```

---

## 使用例

### 実験結果の分析

1. シミュレーション実行後、`Logs/experiment_results/YYYYMMDD_HHMMSS/` を確認
2. `episode_X_summary.csv` で全体の避難率・時間を確認
3. `episode_X_agents.csv` でペルソナ（心理状態）ごとの行動傾向を分析
4. `episode_X_actions.csv` で時系列の行動パターンを確認

### LLM意思決定の分析

1. `Logs/llm_decisions/YYYY_MM_DD-HH_MM_SS/` を確認
2. 各避難者のログファイルでプロンプトと応答を確認
3. `reasoning` フィールドでLLMの判断根拠を分析

---

## 関連コンポーネント

- [EnvManager](Assets/Scripts/ShelterEnvManager.cs): 環境管理、エピソード制御
- [Evacuee](Assets/Scripts/Evacuee.cs): 避難者エージェント
- [Shelter](Assets/Scripts/Shelter.cs): 避難所コンポーネント
- [TsunamiEvacuationArea](Assets/Scripts/TsunamiEvacuationArea.cs): 津波避難地域
- [LLMDecisionClient](Assets/Scripts/LLM/LLMDecisionClient.cs): LLMサーバー通信
- [server.py](llm_server/server.py): LLMサーバー
