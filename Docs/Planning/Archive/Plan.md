# 入出力拡張ロードマップ（優先度順）

本ドキュメントは`Docs/priority.md`で整理された優先度に従い、既存コード（主に`Assets/Evacuee.cs`・`Assets/ShelterEnvManager.cs`・`Assets/Scripts/LLM/LLMActionMessages.cs`・`llm_server/server.py`）を踏まえて段階的な実装仕様をまとめたものです。Unity → Python → LLM → Unityの往復フローを崩さずに、SelfState/MovementActionなどのデータ構造を拡張していきます。

---

## 0. 現状の入出力パイプライン整理

- Unity側では`Evacuee`が`EnvManager`配下でスポーンし、`ShelterManagementAgent`のアクション完了イベントに合わせて`MoveToNearestShelter()`または`LLMDecisionClient`経由の`RequestEvacueeDecisionAsync()`を呼び出す。
- リクエストメッセージは`LLMActionMessages.cs`で定義された`LLMEvacDecisionRequest`が使用され、`llm_server/server.py`でWebSocket受信 → OpenAI呼び出し（またはヒューリスティック）→ `selected_shelter_id`返却という単純なJSONフォーマット。
- Unityはレスポンスから`selected_shelter_id`のみを読み取り`NavMeshAgent.SetDestination()`するため、SelfStateやHierarchicalDecisionといった概念は未実装。

---

## 1. SelfState (+ TemporalContext) 定義(実装済み)

### ゴール（SelfState/Temporal）

- エージェント自身の状態（位置/速度/スタミナ/ストレス/現在目標等）と、経過時間/切迫度などのTemporalContextを**Python側でPydantic BaseModel**として定義し、Unityから送信されるリクエストに含める。

### 既存コードの参照ポイント

- `Assets/Evacuee.cs`: 位置 (`transform.position`)、速度 (`NavMeshAgent.velocity`)、現在ターゲット (`Target`) を直接取得可能。
- `EnvManager`：`currentTimeSec`・`MaxSeconds`・`currentEpisodeId`がTemporalContextのソース。
- `LLMActionMessages.cs`: `LLMEvacDecisionRequest`にSelfStateを追加する拡張ポイント。

### 仕様（SelfState/Temporal）

1. **Pythonモデル (`llm_server/agents/input.py`)**

   ```python
   from pydantic import BaseModel, Field
   from typing import List, Optional, Tuple, Literal

   class TemporalContext(BaseModel):
       elapsed_time: float = Field(description="秒単位の経過時間")
       time_limit: Optional[float] = Field(
           default=None, description="意図的に設定された制限時間"
       )

   class SelfState(BaseModel):
       position: Tuple[float, float, float]
       velocity: Tuple[float, float, float]
       energy_level: float = Field(ge=0, le=1)
       energy_label: Literal["high", "medium", "low"]
       stress_level: float = Field(ge=0, le=1)
       stress_label: Literal["calm", "alert", "panicked"]
       stress_reason: Optional[str]
       current_goal: Optional[str]
       stamina: float = Field(ge=0, le=1)
       injuries: List[str] = []
       injury_notes: Optional[str]

   class AgentInput(BaseModel):
       self_state: SelfState
       temporal_context: TemporalContext
   ```

   - エネルギー・ストレス・スタミナは当面`Evacuee`のプレースホルダ値でよい（例：1.0で初期化し、後続でペルソナ連携）。

2. **Unity → Pythonのデータ収集**

   - `Evacuee.cs`に`BuildSelfStatePayload()`を追加。
     - `Vector3Payload`を流用し、`velocity`用に新たに`Vector3Payload velocity;`を`LLMActionMessages.cs`へ追加。
     - `current_goal`は`Target?.transform.parent?.name`を採用しつつ、視覚的に分かりやすい自然言語の`GoalLabel`を設定できる`[SerializeField] string GoalLabelOverride`を用意。
     - エネルギー/ストレス/スタミナは`[Header("Status")]`セクションで数値スライダに加え、`EnergyLabel`／`StressLabel`列挙と`[TextArea] string StressReason`、`[TextArea] string InjuryNotes`を保持。ペルソナやイベント（例：火災検知時にStressLabelを"panicked"へ変更）から自動更新するメソッドをコメントで示す。
   - TemporalContextは`EnvManager`参照が必要なため、`Evacuee`に`_env`が既にある点を利用して`_env.currentTimeSec`を共有する。さらに`EnvManager`側に`EnableTemporalOverrides`と`ManualTimeLimitSeconds`を追加し、余震警報や避難目安時間を明示的にセットしたケースでのみ`time_limit`を送信する（無効時は`None`のまま）。

3. **メッセージフォーマット拡張**

   - `LLMEvacDecisionRequest`へ`public SelfStatePayload self_state; public TemporalContextPayload temporal;`を追加。
   - JSON変換を壊さないよう、`JsonUtility`がシリアライズできる`[Serializable]`な小クラスを`LLMActionMessages.cs`に定義。
   - `BuildEvacDecisionRequest()`で新規フィールドを埋め、`llm_server/process_payload`（既存）では`AgentInput`バリデーションを通す。

4. **テレメトリ/検証**

   - Unity Editor上で`UseLLMDecision`を有効化した状態でPlayし、`LLMDecisionClient`ログにSelfState情報がJSONとして含まれることを確認。
   - Python側では`AgentInput.model_validate_json(message)`で例外が出ないことを単体テスト化（`llm_server/tests/test_agent_input.py`を新設）。

---

## 2. ImmediateAction.MovementAction データ構造

### ゴール（MovementAction）

- LLMが返す行動を`MovementAction`として構造化し、`NavMeshAgent`制御に必要な最小フィールド（目的地、期待速度、経路嗜好）を扱えるようにする。

### 既存コード参照

- `Evacuee.MoveToNearestShelter()`は目的地と速度設定箇所。
- `NavMeshAgent.speed`はペルソナ実装（`Docs/SimplifiedImplementationPlan.md` Phase1）とも連携予定。
- `llm_server/server.py`では現在`sselected_shelter_id`のみ返却。

### 仕様（MovementAction）

1. **データ定義**

   - `LLMActionMessages.cs`に`[Serializable] public class MovementActionPayload { public string target_shelter_id; public Vector3Payload target_position; public float desired_speed; public string route_preference; }`を追加。
   - `LLMEvacDecisionResponse`へ`public MovementActionPayload movement;`を追加（互換性維持のため旧フィールドも残す）。

2. **Unity実装**

   - `Evacuee.ApplyLLMDecision()`で`response.movement`が存在する場合を優先し、`desired_speed`が0より大きければ`NavAgent.speed`を一時的に上書き。
   - `route_preference`は`NavMeshAgent.areaMask`や`avoidancePriority`に将来使うため`switch`で`"safe"`なら既存ルート、`"fast"`なら`NavMeshAgent.acceleration`を引き上げるなど簡易実装を記述。

3. **LLMサーバ更新**

   - `process_payload()`にて`movement_action = {"target_shelter_id": selected_id, "target_position": ..., "desired_speed": 1.5, "route_preference": "fast"}`といった最低限の値を構築。
   - 後続のOpenAIレスポンス活用では`movement`フィールドを信頼し、降格時にヒューリスティックで埋める。

4. **テスト**

   - Unityエディタで`MovementAction`のJSONをログ出力し、`desired_speed`変更が反映されるか確認。

---

## 3. EnvironmentPerception（最小版）

### 目的（Perception）

- LLM推論の入力に環境知覚の要点だけを入れ、意思決定のコンテキストを増やす。優先度表の通り`visible_area.radius`と`hazards`・`shelters_in_range`を最小構成で実装。

### 仕様（Perception）

1. **Unity側データ収集**

   - `Evacuee`に`public float VisionRadius = 30f;`を追加。
   - `EnvManager.Shelters`をイテレートして、`Vector3.Distance`が`VisionRadius`以下のものを`shelters_in_range`に格納。
   - `hazards`は当面`EnvManager`に`[SerializeField] private List<GameObject> Hazards;`を追加して手動設定、`position`+`severity`（0-1）を送る。

2. **Pythonモデル**

   ```python
   class Hazard(BaseModel):
       id: str
       position: Tuple[float, float, float]
       severity: float = Field(ge=0, le=1)

   class ShelterSummary(BaseModel):
       id: str
       position: Tuple[float, float, float]
       current_capacity: int
       max_capacity: int

   class EnvironmentPerception(BaseModel):
       visible_radius: float
       shelters_in_range: List[ShelterSummary]
       hazards: List[Hazard]
   ```

   - `AgentInput`を拡張して`perception: EnvironmentPerception`を含める。

3. **送信フロー**

   - `LLMEvacDecisionRequest`に`EnvironmentPerceptionPayload`を追加。
   - `llm_server`で`AgentInput`→`build_prompt`に活用（例えば危険度の高いシェルターへの誘導を避けるロジック）。

---

## 4. HierarchicalDecision シェル

### ゴール（Hierarchy）

- LLM出力を`StrategicIntent`→`TacticalPlan`→`ImmediateAction`（MovementActionを含む）の階層構造に落とし、Function Calling Schemaの基盤を整える。

### 仕様（Hierarchy）

1. **Python側モデル `llm_server/agents/decision.py`**

   ```python
   class StrategicIntent(BaseModel):
       primary_goal: str
       secondary_goals: List[str] = []
       constraints: List[str] = []

   class TacticalPlan(BaseModel):
       steps: List[str]
       contingency: Optional[str]

   class ImmediateAction(BaseModel):
       movement: MovementAction
       interaction: Optional[str]
       emotional_response: Optional[str]

   class HierarchicalDecision(BaseModel):
       intent: StrategicIntent
       plan: TacticalPlan
       action: ImmediateAction
       reasoning: str
       confidence: float
   ```

2. **LLMレスポンス**

   - `llm_server`のOpenAIプロンプトをFunction Calling（あるいはJSONモード）で`HierarchicalDecision`に合わせる。
   - ヒューリスティックFallback時も`HierarchicalDecision`を構築して返す。

3. **Unity適用**

   - `LLMEvacDecisionResponse`を`HierarchicalDecisionPayload decision;`へ置き換え（従来フィールドは`[Obsolete]`コメントで残す）。
   - `Evacuee`は`decision.action.movement`を参照しつつ、`decision.intent.primary_goal`をHUD表示など後続拡張に活用できるよう`public string LastGoal` を保持。

---

## 5. Function Calling Schema

### 目的（Schema）

- 上記モデルをOpenAI Function Callingで利用できるよう、JSON Schemaを提供する。

### 仕様（Schema）

- `schemas/hierarchical_decision.schema.json`を新規作成。
- スキーマ例:

  ```json
  {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "title": "HierarchicalDecision",
    "type": "object",
    "required": ["intent", "plan", "action", "reasoning", "confidence"],
    "properties": {
      "intent": { "$ref": "#/$defs/StrategicIntent" },
      "plan": { "$ref": "#/$defs/TacticalPlan" },
      "action": { "$ref": "#/$defs/ImmediateAction" },
      "reasoning": { "type": "string", "maxLength": 512 },
      "confidence": { "type": "number", "minimum": 0, "maximum": 1 }
    },
    "$defs": {
      "...": {}
    }
  }
  ```

- `llm_server/server.py`で`OPENAI_CLIENT.responses.create`の引数に`response_format={"type":"json_schema","json_schema":schema_dict}`を指定。

---

## 6. InputCompressor

### ゴール（Compressor）

- LLMへ渡す前に重要情報だけを抽出し、トークンコストとノイズを抑える。

### 仕様（Compressor）

- `llm_server/agents/input_compressor.py`を新設。

  ```python
  def compress(agent_input: AgentInput) -> dict:
      return {
          "self_state": {
              "position": agent_input.self_state.position,
              "energy_level": agent_input.self_state.energy_level,
              "stress_level": agent_input.self_state.stress_level,
          },
          "perception": {
              "shelters_in_range": sorted(
                  agent_input.perception.shelters_in_range,
                  key=lambda s: s.current_capacity,
                  reverse=True
              )[:3],
              "nearest_hazard": _nearest_hazard(agent_input),
          },
          "temporal_context": agent_input.temporal_context.model_dump(),
      }
  ```

- `process_payload()`でPydanticバリデーション→`compress()`→`build_prompt()`というフローに変更。
- 後からSocialContext/Memoriesを追加する際も`compress()`を拡張する形にする。

---

## 7. 推奨実装順序と依存関係

1. SelfState + TemporalContext（Unity送信 → Pythonモデル）
2. MovementAction（Unityレスポンス処理 + Python構築）
3. EnvironmentPerception最小版（Unity収集 → Pythonモデル）
4. HierarchicalDecisionシェル（モデル定義 → LLMレスポンス反映）
5. Function Calling Schema（JSON Schema化 + OpenAIレスポンス設定）
6. InputCompressor（AgentInput全体が揃った後に追加）

各ステップでUnityの`Evacuee`とPythonサーバの両方を更新し、`LLMDecisionClient`の送受信インターフェースを崩さないよう慎重にバージョン管理する。

---

## 8. テストと検証フロー

- **Unity Play Modeテスト**：SelfStateやMovementActionのJSONがInspectorログに正しく出力されるかを確認。`EnvManager`の`currentEpisodeId`を進めつつ、タイムアウトケースでも問題なく再送信されるか監視。
- **Pythonユニットテスト**：`pytest`で`AgentInput`バリデーション・`compress()`のトリミングロジック・ヒューリスティックfallbackを検証。
- **統合テスト**：ローカルで`llm_server/server.py`を起動し、Unityから5体以上の避難者で同時にLLMリクエストを送信してボトルネックを洗う（`_pendingRequests`のキー衝突などを確認）。

---

この仕様に沿って実装を進めれば、SelfState → MovementAction → EnvironmentPerception → HierarchicalDecision という優先ステップを短期間で具現化でき、最終的なFunction Calling SchemaおよびInputCompressorの足場が整います。
