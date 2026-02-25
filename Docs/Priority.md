# 入出力拡張の優先度決定

## 入力／出力フローを構成する要素の分解

ImplementationPlan.md（特に7章）で定義されている構造体を、実装タスク単位に落とすと次のように整理できます。

### 入力サイド（AgentInputツリー）

1. **SelfState**（位置・速度・体力・ストレス・現在目標など）
2. **EnvironmentPerception**
   - `VisibleArea`（視界 radius、見えているエージェント／建物／ランドマーク）
   - `obstacles / hazards / nearby_locations / shelters_in_range / exit_routes`
   - 天候・時間帯・混雑度など環境ステート
3. **SocialContext**
   - 家族／知人の状態、周囲の群衆行動、受信メッセージ、権威からの指示など
4. **MemoryContext**（working/episodic/semantic/procedural）
5. **TemporalContext**（経過時間、切迫度、災害予測）

### 出力サイド（HierarchicalDecision）

1. **StrategicIntent**（primary/secondary goals, priority, constraints…）
2. **TacticalPlan**（行動シーケンス、waypoints、contingency plans…）
3. **ImmediateAction**
   - `MovementAction`（target_position, desired_speed, route_preferences…）
   - `InteractionAction`
   - `EmotionalResponse`
   - `ExecutionParameters`
4. メタ情報（reasoning, confidence, alternatives 等）

## 重要度 × 取り掛かりやすさ

| 要素                                                        | 重要度 | 実装しやすさ | 理由                                                                                                   |
| --------------------------------------------------------- | --- | ------ | ---------------------------------------------------------------------------------------------------- |
| **SelfState**                                             | 最優先 | 高      | Unity側の既存情報（位置、速度、体力パラメタ）を束ねるだけで効果が大きい。すべての意思決定の前提。                                                  |
| **MovementAction（ImmediateAction内部）**                     | 最優先 | 中      | 実行フェーズでNavMesh制御・速度設定などに直結。出力フォーマットを先に固めると、Unity制御に渡しやすい。                                            |
| EnvironmentPerception（特に `hazards` / `shelters_in_range`） | 高   | 中      | LLM判断の主要な入力。まずは視界情報だけを簡易構造で渡す実装から始めれば段階的に拡張できる。                                                      |
| StrategicIntent / TacticalPlan                            | 高   | 低〜中    | 階層的構造の核だが情報量が多く、最初は`primary_goal`やシンプルな`tactical_plan`だけをサポートする形で着手するのが現実的。                          |
| SocialContext / MemoryContext                             | 中   | 低      | ペルソナ・記憶システムとの連携が必要なため、基盤（SelfState, MovementAction, EnvironmentPerception）ができてから段階的に追加する方が効率的。       |
| TemporalContext                                           | 中   | 高      | `elapsed_time`や`time_pressure`など算出が容易なため、SelfStateとセットで実装しやすい。                                       |
| InteractionAction / EmotionalResponse                     | 中   | 中      | 仕様は計画に示されているが、行動多様性を検証する段階で重要。入力／移動の土台が固まった後に追加するのがよい。                                               |
| Function Calling Schema全体                                 | 中   | 中      | HierarchicalDecisionのフィールドが固まった時点でまとめてJSON Schema化。先にSelfState／MovementActionを定義しておけばスキーマのベースも決めやすい。 |
| InputCompressor                                           | 中   | 低〜中    | AgentInputの各フィールドが整った後で実装しないと、優先度付けやフィルタリングのルールが定まらない。                                               |

## 推奨する着手順序（実装観点）

1. **SelfState（＋TemporalContext）を定義するデータクラス／Pydanticモデル**  
   - Python側の`agents/input.py`のようなモジュールで、`@dataclass`／`BaseModel`を使って定義  
   - Unityや既存シミュレーションコードから取得できる値を詰めるだけで済むため、短期間で実装可能  
   - 後続のInputCompressor・LLMプロンプトでも最も頻繁に使う

2. **ImmediateAction.MovementActionのデータ構造**  
   - `agents/decision.py`などに`MovementAction`, `RoutePreferences`のクラスを作成  
   - NavMeshに渡すための必須フィールド（目的地、速度、経路指向）だけ先に実装し、その他は後から追加  
   - これで「LLM出力 → Unity制御」の最小経路が確立する

3. **EnvironmentPerceptionの最小版**  
   - `visible_area.radius`, `hazards`, `shelters_in_range`など必須フィールドのみ先に定義  
   - Unity → Pythonのデータ受け渡し部分でテストしやすい小さな構造から始める

4. **HierarchicalDecisionのシェル（StrategicIntent / TacticalPlan / ImmediateActionを含む）**  
   - 最小フィールド（`primary_goal`, `movement`）だけで構造体を作成し、徐々にsub-fieldsを拡張  
   - これが決まればFunction Calling Schemaも書き始められる

5. **Function Calling Schema（JSON Schema）**  
   - 上記データクラスのフィールドが固まった段階で、`schemas/decision_schema.json`のようなファイルを作り、LLMに渡す  
   - LangChain／OpenAI Function Callingの定義にも流用できる

6. **InputCompressor（SelfStateやPerceptionの要約・ペルソナフィルタ）**  
   - AgentInput全体が整った後に実装  
   - `input_compressor.py`などで「重要情報抽出→自然言語プロンプト化」の関数を用意

この順序なら、重要度が高く且つ技術的に着手しやすい部分（SelfState／MovementAction）から進めつつ、最小構成の入出力パイプラインを短期間で構築できます。その後、EnvironmentPerceptionやStrategic/Tacticalの詳細を段階的に拡張し、最後にInputCompressorやFunction Calling Schemaで全体を統合すると計画書の方針に沿った実装が進められます。
