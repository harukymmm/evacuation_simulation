# ルールベースエージェント仕様書

## 概要

ルールベースエージェントは、LLMエージェントとの比較実験（実験1）のためのベースラインとして実装された意思決定システムである。事前定義されたルールに基づいて合理的かつ効率的な避難行動を選択する。

## 目的

LLMエージェントが「人間らしい行動」を再現するのに対し、ルールベースエージェントは「合理的な最適行動」を選択する。これにより、以下の比較分析が可能となる:

| 観点 | LLMエージェント | ルールベースエージェント |
|------|----------------|----------------------|
| 行動選択 | 人間らしい、多様 | 合理的、効率的、画一的 |
| 避難完了率 | 85-95% | 95-100% |
| 行動分布 | 6種全て使用 | 主にEVACUATE |
| 目的 | 人間行動の再現 | 最適解のベースライン |

---

## 実装レベル

### Level 1（現在実装済み）

基本的な避難判断のみ対応。

**対応アクション**: EVACUATE, STAY

**判断ルール**:
- 震度6以上 OR 警報受信 → EVACUATE
- 震度5以下 AND 警報なし → STAY（最大60秒後に再評価）

### Level 2（拡張実装）

家族対応を含む拡張判断に対応。

**対応アクション**: EVACUATE, STAY, SEARCH_FAMILY, CONTACT

**非対応アクション**: FOLLOW, TALK（効率的な避難には不要）

---

## 行動選択アルゴリズム

ルールベースエージェントは以下の優先度順でアクションを決定する:

```
優先度1: 津波警報時のみ緊急避難
  Jアラート OR 行政無線 OR 消防団呼びかけ → EVACUATE
  ※地震（震度）だけでは動かない

優先度2: 家族対応（津波警報がない場合）
  シーン内未合流家族 AND 距離500m以内 → SEARCH_FAMILY（子供優先）
  シーン外家族 AND 未連絡 → CONTACT（1回限り）

優先度3: 待機
  津波警報なし → STAY
  ※待機時間上限（600秒=10分）を超えた場合のみEVACUATE

デフォルト: STAY
```

---

## ルール詳細

### EVACUATE条件

以下のいずれかを満たす場合、最寄りの避難所に向かう:

1. Jアラートを受信
2. 行政無線の避難指示を聞いた
3. 消防団の呼びかけを聞いた
4. 待機時間が上限（デフォルト: 600秒=10分）を超過

**注意**: 地震（震度）だけではEVACUATEを選択しない。津波警報が出たタイミングのみ避難行動を開始する。

### STAY条件

以下の条件をすべて満たす場合、その場で待機:

1. 津波警報（Jアラート/行政無線/消防団呼びかけ）を受信していない
2. 待機時間が上限以内

### SEARCH_FAMILY条件（Level 2）

以下の条件をすべて満たす場合、家族を探しに行く:

1. シーン内に未合流の家族が存在する
2. 家族との距離が500m以内
3. 優先順位: 子供（息子/娘）> 配偶者 > その他

```csharp
bool ShouldSearchFamily(out FamilyMember target)
{
    // シーン内に未合流の家族がいるか
    var unreunitedFamily = _familyData.members
        .Where(m => m.exists_in_scene && !m.is_reunited)
        .ToList();

    if (unreunitedFamily.Count == 0) return false;

    // 距離が500m以内で、子供を優先
    target = unreunitedFamily
        .Where(m => Vector3.Distance(pos, m.search_position) < 500f)
        .OrderBy(m => m.relationship.Contains("息子") ||
                      m.relationship.Contains("娘") ? 0 : 1)
        .FirstOrDefault();

    return target != null;
}
```

### CONTACT条件（Level 2）

以下の条件をすべて満たす場合、家族に連絡を取る:

1. シーン外の家族が存在する
2. まだ連絡を取っていない（1回限り）

```csharp
bool ShouldContact(out FamilyMember target)
{
    if (_hasContactedFamily) return false;  // 1回限り

    // シーン外の家族を対象
    target = _familyData.members
        .Where(m => !m.exists_in_scene)
        .FirstOrDefault();

    return target != null;
}
```

---

## 設定パラメータ

**ファイル**: `Assets/Scripts/RuleBasedDecisionMaker.cs`

| パラメータ | 型 | デフォルト値 | 説明 |
|---|---|---|---|
| `MaxStayDuration` | float | 600 | 待機から避難に切り替える最大時間（秒） |
| `DangerousSeismicIntensity` | int | 6 | 危険と判定する震度の閾値 |
| `ReevaluationInterval` | float | 10 | ルール再評価の間隔（秒） |

---

## エージェントタイプの切り替え

Unity Inspectorから `ExperimentConfig` コンポーネントで切り替え可能。

**ファイル**: `Assets/Scripts/ExperimentConfig.cs`

```csharp
public enum AgentType
{
    LLM,        // LLMエージェント
    RuleBased   // ルールベースエージェント
}

[Tooltip("使用するエージェントタイプ")]
public AgentType SelectedAgentType = AgentType.LLM;
```

### 切り替え確認方法

1. **Unity Inspector**で `ExperimentConfig.SelectedAgentType` を変更
2. **コンソールログ**で `[RuleBased]` プレフィックスを確認
3. **行動分布**でEVACUATEが大多数（90%以上）であることを確認

---

## 内部状態

RuleBasedDecisionMakerは以下の内部状態を保持する:

| フィールド | 型 | 説明 |
|---|---|---|
| `_stayStartTime` | float | STAY開始時刻 |
| `_lastEvaluationTime` | float | 最後のルール評価時刻 |
| `_hasHeardBroadcast` | bool | 行政無線を聞いたか |
| `_hasReceivedJAlert` | bool | Jアラートを受信したか |
| `_hasHeardFireTruck` | bool | 消防団の呼びかけを聞いたか |
| `_hasContactedFamily` | bool | 家族に連絡を取ったか（Level 2） |

---

## 情報受信メソッド

外部からの情報を受信するためのメソッド:

```csharp
// 行政無線を聞いた
public void OnHeardBroadcast()

// Jアラートを受信
public void OnReceivedJAlert()

// 消防団の呼びかけを聞いた
public void OnHeardFireTruck()
```

これらのメソッドが呼ばれると、次回の評価でEVACUATEが選択される。

---

## 避難所選択ロジック

EVACUATEが選択された場合、最寄りの避難所を選択する:

1. 利用可能な全避難所を走査
2. 収容可能人数（`currentCapacity > 0`）をチェック
3. 現在位置からの距離を計算
4. 最も近い避難所を選択

```csharp
private (GameObject shelter, float distance) FindNearestShelter()
{
    GameObject nearestShelter = null;
    float nearestDistance = float.MaxValue;
    Vector3 currentPos = _evacuee.transform.position;

    foreach (var shelterObj in _envManager.Shelters)
    {
        var shelter = shelterObj.GetComponent<Shelter>();
        if (shelter == null || shelter.currentCapacity <= 0) continue;

        float distance = Vector3.Distance(currentPos, pointTransform.position);
        if (distance < nearestDistance)
        {
            nearestDistance = distance;
            nearestShelter = pointTransform.gameObject;
        }
    }

    return (nearestShelter, nearestDistance);
}
```

---

## ライフサイクル

### 初期化

```csharp
public void Initialize(Evacuee evacuee, EnvManager envManager, NavMeshAgent navAgent)
// Level 2では追加パラメータ
public void Initialize(Evacuee evacuee, EnvManager envManager,
    NavMeshAgent navAgent, PersonaData persona, FamilyData familyData)
```

### 意思決定

```csharp
public (ActionType action, GameObject targetShelter, string reasoning) MakeDecision()
```

### リセット（エピソード開始時）

```csharp
public void Reset()
{
    _stayStartTime = -1f;
    _lastEvaluationTime = 0f;
    _hasHeardBroadcast = false;
    _hasReceivedJAlert = false;
    _hasHeardFireTruck = false;
    _hasContactedFamily = false;  // Level 2
}
```

---

## 検証方法

### 1. 行動分布の確認

- EVACUATE が大多数（90%以上）
- SEARCH_FAMILY/CONTACT は条件付きで発生
- TALK/FOLLOW は選択されない

### 2. 避難完了率の確認

- LLMより高い完了率（95%以上を期待）
- 平均避難時間が短い

### 3. メトリクス記録

- `SimulationMetrics.RecordAction()` でログ出力
- `episode_X_summary.csv` の `agent_type` が `RuleBased` であることを確認
- CSVエクスポートでLLMと比較

---

## 関連コンポーネント

- [ExperimentConfig](../../Assets/Scripts/ExperimentConfig.cs): エージェントタイプ切り替え
- [Evacuee](../../Assets/Scripts/Evacuee.cs): 避難者エージェント
- [EnvManager](../../Assets/Scripts/ShelterEnvManager.cs): 環境管理
- [SimulationMetrics](../../Assets/Scripts/SimulationMetrics.cs): 評価ログ
- [FamilyData](../../Assets/Scripts/FamilyData.cs): 家族情報（Level 2）
- [PersonaData](../../Assets/Scripts/LLM/PersonaData.cs): ペルソナ情報（Level 2）

---

## 実験での使用

### 実験1: 基本性能比較

1. `ExperimentConfig.SelectedAgentType = RuleBased` に設定
2. `ExperimentConfig.NumberOfTrials = 10` に設定
3. シミュレーションを実行
4. `Logs/experiment_results/` の結果をLLMエージェントと比較

### 期待される結果

| 指標 | LLMエージェント | ルールベースエージェント |
|------|----------------|----------------------|
| 避難完了率 | 85-95% | 95-100% |
| 平均避難時間 | 長い | 短い |
| 行動多様性 | 高い | 低い |
| EVACUATE比率 | 50-70% | 90%以上 |

---

## Future Work

現在の実装では、ルールベースエージェントは「合理的な最適行動」を目指しており、ペルソナ情報（個人特性）は使用していない。以下は将来的な拡張候補である。

### 1. ペルソナ情報の反映

LLMエージェントでは`PersonaData`が以下のように利用されている：

| フィールド | LLMでの利用 | ルールベースでの対応案 |
|-----------|------------|---------------------|
| `speed_multiplier` | 移動速度の調整 | ✅ 既にNavAgent.speedで反映済み |
| `has_smartphone` | CONTACT可否判定 | ✅ 実装済み（ShouldContact判定に利用可能） |
| `mental_state` | LLMプロンプトでの性格反映 | 🔲 未対応（下記参照） |
| `age_group` | LLMの判断材料 | 🔲 避難開始遅延の確率調整に使用可能 |
| `physical_condition` | 移動能力の制約 | 🔲 移動速度・経路選択に反映可能 |
| `past_disaster_experience` | 過去経験による判断 | 🔲 避難開始閾値の調整に使用可能 |

#### mental_stateによる行動変化（実装案）

```csharp
// 正常性バイアスを持つエージェントは避難開始を遅延
if (_persona?.mental_state == "NormalcyBias")
{
    MaxStayDuration *= 1.5f;  // 待機時間を1.5倍に
}

// 同調バイアスを持つエージェントはFOLLOW傾向
if (_persona?.mental_state == "ConformityBias")
{
    // 周囲の避難者が多い場合にEVACUATEを選択しやすく
}
```

### 2. 未対応アクション

現在のLevel 2では効率性を優先してFOLLOW/TALKを非対応としているが、より人間らしい行動を再現する場合は以下の実装が考えられる。

#### FOLLOW（同調行動）

```
条件: 周囲10m以内に避難中のエージェントが3人以上
効果: 最寄りではなく、彼らと同じ避難所を選択
```

#### TALK（情報交換）

```
条件: 近くに停止中のエージェントがいる AND 避難開始前
効果: 一定時間の会話後、相手の情報を考慮して判断
```

### 3. 先行研究で実装されているルールベースの要素

避難シミュレーションの先行研究では、以下のような要素がルールベースで実装されることがある：

| 要素 | 現状 | 対応案 |
|------|------|--------|
| 群衆回避 | ❌ 未対応 | NavMeshAgentの障害物回避に依存 |
| 避難所の混雑回避 | ❌ 未対応 | `currentCapacity`を考慮した避難所選択 |
| 危険地帯（浸水域）回避 | ❌ 未対応 | 津波浸水予測区域を迂回する経路選択 |
| 持ち物準備時間 | ❌ 未対応 | 避難開始前に確率的な遅延を追加 |
| 近所への声掛け | ❌ 未対応 | 消防団ペルソナがTALKを選択 |
| 車避難 | ❌ 未対応 | ペルソナ属性（高齢者・遠方居住）で車移動を選択 |
| 垂直避難（自宅2階への退避） | ❌ 未対応 | 高層建物が近い場合のEVACUATE先変更 |

### 4. 認知バイアスの明示的実装（実験2対応）

実験2-2（認知バイアスの影響検証）に対応するため、以下のバイアスタイプを明示的に制御する機能が必要：

```csharp
public enum BiasType
{
    None,           // ベースライン（合理的判断）
    NormalcyBias,   // 正常性バイアス（避難開始遅延）
    ConformityBias, // 同調バイアス（周囲に追従）
    Combined        // 複合バイアス
}
```

**実装優先度**: 高（実験2-2の実行に必須）

### 5. 情報条件による行動変化（実験3対応）

現在の津波警報受信（Jアラート/行政無線/消防団）に加え、情報の詳細度による行動変化を実装：

| 情報レベル | 現状の対応 | 追加実装案 |
|-----------|----------|-----------|
| 最小限（地震のみ） | ✅ STAY | - |
| 標準（津波警報） | ✅ EVACUATE | - |
| 詳細（混雑情報） | ❌ 未対応 | 混雑避難所を避ける選択 |
| 拡張（二次災害） | ❌ 未対応 | 危険地域からの迅速避難 |

---

### 実装優先度まとめ

| 機能 | 優先度 | 理由 |
|------|--------|------|
| 認知バイアス制御 | 高 | 実験2-2に必須 |
| 混雑回避ロジック | 中 | より現実的な避難所選択 |
| ペルソナ属性反映 | 中 | 行動多様性の向上 |
| FOLLOW/TALK対応 | 低 | 効率的避難には不要だが人間らしさ向上 |
| 車避難・垂直避難 | 低 | シナリオ拡張時に検討 |
