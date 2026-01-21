# ルールベースエージェント仕様書

## 概要

ルールベースエージェントは、LLMエージェントとの比較実験（実験1）のためのベースラインとして実装された意思決定システムである。事前定義されたルールに基づいて避難行動を選択する。

**Level 3では、ペルソナのmental_stateに基づく認知バイアスを反映し、「人間的な不合理性」を再現する。**

## 目的

Level 3では、LLMエージェントと有意義な比較検証を行うため、ルールベースエージェントにも「人間らしい行動の揺らぎ」を導入している。

| 観点 | Level 1-2 | Level 3（現行） | LLMエージェント |
|------|-----------|----------------|----------------|
| 行動選択 | 合理的、画一的 | mental_stateで分化 | 人間らしい、多様 |
| 避難完了率 | 95-100% | 80-90%（mental_stateで変動） | 79% |
| FOLLOW発生率 | 0% | 15-25% | 15% |
| 目的 | 最適解ベースライン | 人間的不合理性を持つベースライン | 人間行動の再現 |

---

## 実装レベル

### Level 1

基本的な避難判断のみ対応。

**対応アクション**: EVACUATE, STAY

**判断ルール**:
- 震度6以上 OR 警報受信 → EVACUATE
- 震度5以下 AND 警報なし → STAY（最大60秒後に再評価）

### Level 2

家族対応を含む拡張判断に対応。

**対応アクション**: EVACUATE, STAY, SEARCH_FAMILY, CONTACT

**非対応アクション**: FOLLOW, TALK

### Level 3（現行実装）

ペルソナのmental_stateに基づく認知バイアスと人間的行動を反映。

**対応アクション**: EVACUATE, STAY, SEARCH_FAMILY, FOLLOW

**非対応アクション**: CONTACT, TALK（明示的な会話行動は非対応、ただし情報伝播機能で同等の情報取得が可能）

**追加機能**:
- mental_stateによる行動分化（7タイプ）
- FOLLOW行動（同調バイアス）
- 確率的要素の導入
- 避難先選択の多様化（避難所＋津波避難場所）
- 避難先ロック機能（一度選択した避難先を到達まで維持、振動防止）
- 準備時間の導入
- 年齢層による行動修飾
- 有効最大待機時間の動的計算
- 情報伝播（Pseudo-TALK）: 近接する避難中エージェントから危機情報を受け取る

---

## 行動選択アルゴリズム

### Level 3 アルゴリズム（現行）

ペルソナの`mental_state`を解析し、行動タイプに応じた判断を行う。

```
1. 初期化
   - ペルソナのmental_stateから行動タイプを判定（7タイプ）
   - age_groupから年齢修飾子を適用
   - 準備時間を計算（基本10-30秒 + 高齢者追加20-60秒 + mental_state修飾）
   - 有効最大待機時間を計算（基本600秒 × mental_state倍率）

2. 評価間隔チェック
   - ReevaluationInterval（デフォルト10秒）未満の場合は前回の判断を継続

3. 準備時間待機（初回のみ）
   - 準備が完了するまでSTAYを維持

4. 優先度0: 焦燥・目的外行動の家族探索
   - mental_state="焦燥・目的外行動" AND シーン内家族あり
   → SEARCH_FAMILY（津波警報より優先）

5. 優先度1: 津波警報受信時（全ペルソナに確率的判定を適用）
   - 同調バイアス系（社交的・周囲配慮型、不安傾向・サポート希求型）:
     - 周囲30m以内に避難中エージェントがいれば → FOLLOW
     - FOLLOWできない場合は周囲の状況を確認
     - 周囲に避難中が多い（evacuatingCount > stayingCount） → EVACUATE（確率判定スキップ）
     - 周囲が待機多数（stayingCount >= 2） → STAY
     - 周囲に人がいない場合、不安傾向は「動けない」
   - 全ペルソナ共通: 確率的判定（GetEvacuationProbability()で計算）
     - mental_state別の基本確率（下表参照）
     - 警報受信回数ごとに+10%（最大+30%）
     - 時間経過: 経過時間/1800秒 × 20%（最大）
     - 確率を満たせばEVACUATE、満たさなければSTAY継続

6. 優先度2: 同調バイアス判定（警報未受信時）
   - 社交的・周囲配慮型: 周囲30m以内に避難中3人以上 → FOLLOW
   - 不安傾向・サポート希求型: 周囲30m以内に避難中1人以上 → FOLLOW
   - 周囲が待機多数（stayingCount > evacuatingCount AND stayingCount >= 2） → STAY継続

7. 優先度3: 家族対応
   - シーン内未合流家族 AND 距離500m以内 → SEARCH_FAMILY

8. 優先度4: 避難先選択（避難所 + 津波避難場所）
   - 冷静系: 最寄りの避難先
   - 慎重・人混み恐怖: 最寄り回避（混むと予測して2番目に近い場所）
   - 焦燥系: 高台優先（海抜が高い場所）

9. デフォルト: STAY
   - 有効最大待機時間（_effectiveMaxStayDuration）を超過した場合のみEVACUATE
```

### Level 1-2 アルゴリズム（旧）

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

## mental_stateと行動の対応表（Level 3）

ペルソナの`mental_state`に応じて、行動パターンが分化する。

| mental_state | 人数 | 基本避難確率 | 避難開始 | FOLLOW | 避難先選択 | 特徴 |
|--------------|------|------------|---------|--------|----------|------|
| 楽観的・自己信頼型 | 125 | 15% | 遅延（確率的） | なし | 最寄り | 正常性バイアス |
| 不安傾向・サポート希求型 | 20 | 40% | 周囲に依存 | 1人以上で発動 | 追従先と同じ | 不安で動けない |
| 社交的・周囲配慮型 | 80 | 50% | 周囲に依存 | 3人以上で発動 | 追従先と同じ | 同調バイアス |
| 慎重・人混み恐怖 | 75 | 70% | 比較的早い | なし | 最寄り回避 | 混むと予測して2番目に近い場所を選択 |
| 焦燥・目的外行動 | 50 | 80% | 早い | なし | 高台優先 | 家族優先 |
| 冷静・分析的 | 75 | 85% | 分析後に避難 | なし | 最寄り | 分析的判断 |
| 冷静・合理的 | 75 | 90% | 即座 | なし | 最寄り | 合理的判断 |

### mental_state判定メソッド

```csharp
// 正常性バイアス（楽観的・自己信頼型）
private bool HasNormalcyBias()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return false;
    return _persona.mental_state.Contains("楽観") ||
           _persona.mental_state.Contains("自己信頼");
}

// 同調バイアス（社交的・周囲配慮型）
private bool HasConformityBias()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return false;
    return _persona.mental_state.Contains("社交的") ||
           _persona.mental_state.Contains("周囲配慮");
}

// 強い同調バイアス（不安傾向・サポート希求型）
private bool HasStrongConformityBias()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return false;
    return _persona.mental_state.Contains("不安傾向") ||
           _persona.mental_state.Contains("サポート希求");
}

// 混雑回避（慎重・人混み恐怖）
private bool HasCrowdAvoidance()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return false;
    return _persona.mental_state.Contains("慎重") ||
           _persona.mental_state.Contains("人混み恐怖");
}

// 家族優先（焦燥・目的外行動）
private bool HasFamilyPriority()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return false;
    return _persona.mental_state.Contains("焦燥") ||
           _persona.mental_state.Contains("目的外行動");
}

// 合理的判断（冷静系）
private bool IsRational()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.mental_state)) return true; // デフォルトは冷静
    return _persona.mental_state.Contains("冷静");
}

// 高齢者判定
private bool IsElderly()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.age_group)) return false;
    return _persona.age_group.Contains("70") ||
           _persona.age_group.Contains("80");
}

// 若年層判定（子供/10代）
private bool IsYoung()
{
    if (_persona == null || string.IsNullOrEmpty(_persona.age_group)) return false;
    return _persona.age_group.Contains("10") ||
           _persona.age_group.ToLower().Contains("child");
}
```

---

## ルール詳細

### EVACUATE条件

以下のいずれかを満たす場合、最寄りの避難所に向かう:

1. Jアラートを受信
2. 行政無線の避難指示を聞いた
3. 消防団の呼びかけを聞いた
4. 近くの避難中エージェントから情報を受け取った（情報伝播）
5. 有効最大待機時間（_effectiveMaxStayDuration）を超過

**注意**: 地震（震度）だけではEVACUATEを選択しない。津波警報が出たタイミングのみ避難行動を開始する。

### STAY条件

以下の条件をすべて満たす場合、その場で待機:

1. 津波警報（Jアラート/行政無線/消防団呼びかけ/情報伝播）を受信していない
2. 有効最大待機時間以内

### SEARCH_FAMILY条件（Level 3）

以下の条件をすべて満たす場合、家族を探しに行く:

1. シーン内に未合流の家族が存在する（`exists_in_scene && agent_id > 0 && !is_reunited`）
2. 家族との距離が500m以内
3. 優先順位: 子供（息子/娘/子供/子ども）> その他

```csharp
private bool ShouldSearchFamily(out FamilyMember target)
{
    target = null;
    if (_familyData == null || _familyData.members == null) return false;

    var unreunitedFamily = _familyData.members
        .Where(m => m.exists_in_scene && m.agent_id > 0 && !m.is_reunited)
        .ToList();

    if (unreunitedFamily.Count == 0) return false;

    Vector3 pos = _evacuee.transform.position;

    // 距離閾値以内、子供を優先
    target = unreunitedFamily
        .Where(m => Vector3.Distance(pos, m.search_position) < SearchFamilyDistanceThreshold)
        .OrderBy(m => IsChild(m.relation) ? 0 : 1)
        .FirstOrDefault();

    return target != null;
}

private bool IsChild(string relation)
{
    if (string.IsNullOrEmpty(relation)) return false;
    return relation.Contains("息子") || relation.Contains("娘") ||
           relation.Contains("子供") || relation.Contains("子ども");
}
```

### FOLLOW条件（Level 3）

同調バイアスを持つペルソナが、周囲の避難行動に追従する。

**対象mental_state**:
- 社交的・周囲配慮型: 周囲30m以内に避難中エージェントが3人以上
- 不安傾向・サポート希求型: 周囲30m以内に避難中エージェントが1人以上

```csharp
private (ActionType action, GameObject target, string reasoning, FamilyMember familyTarget) TryConformityFollow()
{
    var nearbyEvacuees = GetNearbyEvacuatingAgents();

    int threshold = HasStrongConformityBias() ? StrongConformityThreshold : ConformityThreshold;
    if (nearbyEvacuees.Count < threshold)
    {
        // FOLLOWできない場合
        if (HasStrongConformityBias())
        {
            // 不安傾向は不安で動けない
            return (ActionType.STAY, null, "周囲に避難中の人がいないので不安で動けない", null);
        }
        return (ActionType.STAY, null, null, null); // 次の優先度へ
    }

    // 避難中のエージェントから追従対象を選択
    var followTarget = nearbyEvacuees
        .Where(e => !WouldCreateFollowCycle(e))  // 循環防止
        .OrderBy(e => Vector3.Distance(_evacuee.transform.position, e.transform.position))
        .FirstOrDefault();

    if (followTarget == null)
    {
        return (ActionType.STAY, null, null, null);
    }

    _followTargetEvacuee = followTarget;
    string targetName = followTarget.PersonaName ?? followTarget.EvacueeId;
    return (ActionType.FOLLOW, followTarget.Target,
        $"周囲の人（{targetName}）について行く", null);
}
```

**循環防止**: FOLLOWチェーンが循環しないよう、追従対象の選択時に検証を行う（2段階まで確認）。

```csharp
private bool WouldCreateFollowCycle(Evacuee target)
{
    // 相手が自分をFOLLOWしている場合は循環
    if (target.CurrentAction == ActionType.FOLLOW)
    {
        // 直接自分を追従しているか確認
        var targetFollowTarget = target.FollowTarget;
        if (targetFollowTarget == _evacuee)
        {
            return true;
        }

        // 間接的に自分を追従しているか確認（2段階まで）
        if (targetFollowTarget != null && targetFollowTarget.CurrentAction == ActionType.FOLLOW)
        {
            var indirectTarget = targetFollowTarget.FollowTarget;
            if (indirectTarget == _evacuee)
            {
                return true;
            }
        }
    }

    return false;
}
```

---

## 設定パラメータ

**ファイル**: `Assets/Scripts/RuleBasedDecisionMaker.cs`

### 基本パラメータ

| パラメータ | 型 | デフォルト値 | 説明 |
|---|---|---|---|
| `MaxStayDuration` | float | 600 | 待機から避難に切り替える最大時間（秒）。実際の最大待機時間はmental_stateで変動 |
| `DangerousSeismicIntensity` | int | 6 | 危険と判定する震度の閾値（現在未使用：津波警報のみで判定） |
| `ReevaluationInterval` | float | 10 | ルール再評価の間隔（秒） |
| `SearchFamilyDistanceThreshold` | float | 500 | 家族探索の距離閾値（メートル） |

### Level 3 追加パラメータ

| パラメータ | 型 | デフォルト値 | 説明 |
|---|---|---|---|
| `ConformityCheckRadius` | float | 30 | 同調バイアス判定の半径（メートル） |
| `ConformityThreshold` | int | 3 | 社交的タイプのFOLLOW発動閾値（人数） |
| `StrongConformityThreshold` | int | 1 | 不安傾向タイプのFOLLOW発動閾値（人数） |
| `InformationDiffusionProbability` | float | 0.3 | 情報伝播の確率（0.0〜1.0） |
| `ProximityRadius` | float | 5 | 情報伝播が発生する距離（メートル） |

### 内部計算パラメータ

以下のパラメータは実装内でハードコードされている:

| パラメータ | 値 | 説明 |
|---|---|---|
| 基本準備時間 | 10-30秒 | ランダムで決定 |
| 高齢者追加準備時間 | 20-60秒 | 70代/80代の場合に追加 |
| 楽観的タイプ準備時間倍率 | 1.5倍 | 「まだ大丈夫」と準備が遅い |
| 焦燥系準備時間倍率 | 0.5倍 | 焦って準備が早い |
| 楽観的タイプ最大待機時間倍率 | 1.5〜2.0倍 | 正常性バイアスで長く待機 |
| 焦燥系最大待機時間倍率 | 0.5倍 | 早く避難を開始 |
| 避難確率増加量（警報ごと） | +10%（最大+30%） | 警報を受けるたびに上昇 |
| 避難確率増加量（時間経過） | 最大+20% | 1800秒で最大に |

### mental_state別 基本避難確率

| mental_state | 基本確率 | 説明 |
|---|---|---|
| 楽観的・自己信頼型 | 15% | 正常性バイアスで最も低い |
| 不安傾向・サポート希求型 | 40% | 不安で動けないことも |
| 社交的・周囲配慮型 | 50% | 周囲の状況を見て判断 |
| 慎重・人混み恐怖 | 70% | 比較的早く避難 |
| 焦燥・目的外行動 | 80% | 焦って行動 |
| 冷静・分析的 | 85% | 分析後に避難 |
| 冷静・合理的 | 90% | 合理的に即避難 |
| ペルソナなし（デフォルト） | 90% | 冷静系と同等 |

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

### 基本状態

| フィールド | 型 | 説明 |
|---|---|---|
| `_stayStartTime` | float | STAY開始時刻 |
| `_lastEvaluationTime` | float | 最後のルール評価時刻 |
| `_hasHeardBroadcast` | bool | 行政無線を聞いたか |
| `_hasReceivedJAlert` | bool | Jアラートを受信したか |
| `_hasHeardFireTruck` | bool | 消防団の呼びかけを聞いたか |
| `_hasContactedFamily` | bool | 家族に連絡を取ったか（Level 3では未使用） |

### Level 3 追加状態

| フィールド | 型 | 説明 |
|---|---|---|
| `_persona` | PersonaData | ペルソナ情報 |
| `_decisionRandom` | System.Random | 確率的判断用の乱数生成器（エージェントIDベースのシード） |
| `_alertReceivedCount` | int | 警報受信回数（避難確率計算に使用） |
| `_preparationTime` | float | 計算された準備時間（秒） |
| `_preparationComplete` | bool | 準備が完了したか |
| `_preparationStartTime` | float | 準備開始時刻 |
| `_effectiveMaxStayDuration` | float | 有効な最大待機時間（mental_stateで変動） |
| `_followTargetEvacuee` | Evacuee | FOLLOW行動の追従対象 |
| `_lockedDestination` | GameObject | ロック中の避難先（一度選択した避難先を維持） |
| `_hasHeardFromNearbyAgent` | bool | 近くのエージェントから情報を受け取ったか |
| `_informationSourceName` | string | 情報伝播元のエージェント名 |

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

// 近くの避難者から情報を受け取った（Level 3: 情報伝播）
public void OnHeardFromNearbyAgent(string sourceName)
```

これらのメソッドが呼ばれると、`_alertReceivedCount`がインクリメントされ、次回の評価でEVACUATEが選択される可能性が高まる。

---

## 情報伝播（Pseudo-TALK）機能（Level 3）

### 目的

LLMエージェントがTALK行動で「会話により危機情報を得る」機能を持つため、ルールベースエージェントにも同等の情報伝播メカニズムを導入する。これにより、「LLMが優れているのは単に情報を得られたから」という結論を防ぎ、意思決定能力の公平な比較を可能にする。

### SIRモデル的情報伝播

情報伝播はSIR（Susceptible-Infected-Recovered）モデルの考え方に基づく:

| 状態 | 説明 |
|------|------|
| S (Susceptible) | 警報未受信エージェント（情報を受け取り得る状態） |
| I (Infected) | 警報受信済みエージェント（情報源として機能） |
| R (Recovered) | この実装では不要（一度受信したら維持） |

### 伝播条件

```
伝播元: 避難中（EVACUATE/FOLLOW）かつ警報受信済みのエージェント
伝播先: 待機中（STAY）かつ警報未受信のエージェント
距離条件: ProximityRadius（デフォルト: 5m）以内
確率: InformationDiffusionProbability（デフォルト: 0.3）
```

### 実装

```csharp
// RuleBasedDecisionMaker.cs
public void OnHeardFromNearbyAgent(string sourceName)
{
    if (_hasHeardFromNearbyAgent) return; // 既に受信済み

    _hasHeardFromNearbyAgent = true;
    _informationSourceName = sourceName;
    _alertReceivedCount++;

    Debug.Log($"[RuleBased] {_evacuee.gameObject.name}: {sourceName}から避難情報を受け取った（情報伝播）");
}

public bool CanSpreadInformation()
{
    bool hasAlert = _hasHeardBroadcast || _hasReceivedJAlert || _hasHeardFireTruck || _hasHeardFromNearbyAgent;
    if (!hasAlert) return false;

    var currentAction = _evacuee.CurrentAction;
    return currentAction == ActionType.EVACUATE || currentAction == ActionType.FOLLOW;
}
```

### パラメータ

| パラメータ | 型 | デフォルト値 | 説明 |
|-----------|-----|-------------|------|
| `InformationDiffusionProbability` | float | 0.3 | 情報伝播の確率（0.0〜1.0） |
| `ProximityRadius` | float | 5.0 | 情報伝播が発生する距離（メートル） |

### 期待される効果

| 指標 | 情報伝播なし | 情報伝播あり |
|-----|-------------|-------------|
| 警報受信経路 | 行政無線/Jアラートのみ | + 近接伝播 |
| 情報到達率 | スピーカー範囲・スマホ保有に依存 | 段階的に拡大 |
| 避難開始の波及 | 一斉（警報同時受信） | カスケード（波及的） |
| LLMとの比較 | 情報取得能力で不利 | 同等の情報取得機会 |

### LLMエージェントとの互換性

情報伝播の伝播元は、ルールベースエージェントだけでなくLLMエージェントも対象となる。避難中のLLMエージェントの近くを通過した場合も、同様の確率で情報を受け取る。

---

## 避難先選択ロジック（Level 3）

Level 3では、避難所（Shelter）と津波避難場所（TsunamiEvacuationArea）の両方を選択肢に含め、mental_stateに応じて選択戦略が異なる。

### 避難先ロック機能

一度選択した避難先は、以下の条件を満たす限り維持される（避難先振動の防止）:

- 避難先がまだ有効である（避難所の場合は収容可能、津波避難場所は常に有効）

避難所が満員になった場合のみ、新しい避難先を再選択する。

```csharp
private (GameObject destination, float distance) SelectDestination()
{
    Vector3 pos = _evacuee.transform.position;

    // ロック中の避難先があり、まだ有効であれば継続
    if (_lockedDestination != null && IsDestinationStillValid(_lockedDestination))
    {
        float dist = Vector3.Distance(pos, _lockedDestination.transform.position);
        return (_lockedDestination, dist);
    }

    // ロック解除（無効になった場合）
    _lockedDestination = null;

    // 新規選択（mental_stateベースのロジック）
    // ...

    // ロック設定
    _lockedDestination = result.destination;

    return result;
}

private bool IsDestinationStillValid(GameObject destination)
{
    if (destination == null) return false;

    var shelter = destination.GetComponentInParent<Shelter>();
    if (shelter != null)
    {
        return shelter.currentCapacity > 0;
    }

    // 津波避難場所は常に有効（収容制限なし）
    return true;
}
```

### 避難先候補の取得

```csharp
private List<(GameObject obj, float elevation, int capacity, bool isShelter)> GetAllEvacuationDestinations()
{
    var destinations = new List<(GameObject, float, int, bool)>();

    // 避難所（Shelter）
    if (_envManager?.Shelters != null)
    {
        foreach (var shelterObj in _envManager.Shelters)
        {
            if (shelterObj == null) continue;

            var shelter = shelterObj.GetComponent<Shelter>();
            if (shelter == null || shelter.currentCapacity <= 0) continue;

            var point = shelterObj.transform.childCount > 0
                ? shelterObj.transform.GetChild(0).gameObject
                : shelterObj;
            destinations.Add((point, shelter.GetElevation(), shelter.currentCapacity, true));
        }
    }

    // 津波避難場所（TsunamiEvacuationArea）
    // 津波避難場所は収容制限がない（高台なので無制限に受け入れ可能）
    if (_envManager?.TsunamiEvacuationAreas != null)
    {
        foreach (var areaObj in _envManager.TsunamiEvacuationAreas)
        {
            if (areaObj == null) continue;

            var area = areaObj.GetComponent<TsunamiEvacuationArea>();
            if (area == null) continue;

            // 津波避難場所は収容制限なしのため、capacityは十分大きな値を設定
            destinations.Add((areaObj, area.elevationMeters, int.MaxValue, false));
        }
    }

    return destinations;
}
```

### 選択戦略

| mental_state | 選択戦略 | 効果 |
|--------------|---------|------|
| 冷静系 | 最寄り優先 | 距離が最も近い避難先を選択 |
| 慎重・人混み恐怖 | 最寄り回避 | 「近くは混む」と予測し、2番目に近い場所を選択 |
| 焦燥・目的外行動 | 高台優先 | 海抜が高い場所を優先（神社・高台等） |

```csharp
// 最寄り回避（慎重・人混み恐怖向け）
// 人混み恐怖の人は「近くの避難所は混む」と予測し、あえて少し遠い場所を選ぶ
private (GameObject destination, float distance) FindLeastCrowdedDestination()
{
    var destinations = GetAllEvacuationDestinations();
    if (destinations.Count == 0) return (null, float.MaxValue);

    Vector3 pos = _evacuee.transform.position;

    // 距離順にソート
    var sortedByDistance = destinations
        .OrderBy(d => Vector3.Distance(pos, d.obj.transform.position))
        .ToList();

    // 最寄りを避けて2番目以降を選択（2箇所以上ある場合）
    // 1箇所しかない場合は仕方なく最寄りを選択
    var selected = sortedByDistance.Count > 1 ? sortedByDistance[1] : sortedByDistance[0];

    return (selected.obj, Vector3.Distance(pos, selected.obj.transform.position));
}

// 高台優先（焦燥・目的外行動向け）
private (GameObject destination, float distance) FindHighElevationDestination()
{
    var destinations = GetAllEvacuationDestinations();
    if (destinations.Count == 0) return (null, float.MaxValue);

    Vector3 pos = _evacuee.transform.position;

    // 海抜が高い場所を優先
    var highest = destinations
        .OrderByDescending(d => d.elevation)
        .ThenBy(d => Vector3.Distance(pos, d.obj.transform.position))
        .FirstOrDefault();

    return (highest.obj, Vector3.Distance(pos, highest.obj.transform.position));
}
```

---

## ライフサイクル

### 初期化

```csharp
public void Initialize(Evacuee evacuee, EnvManager envManager, NavMeshAgent navAgent)
// Level 2/3では拡張初期化メソッドを追加で呼び出す
public void InitializeExtended(FamilyData familyData)
```

Initialize内で以下が実行される:

- 基本参照の保存（evacuee, envManager, navAgent）
- 仮の乱数生成器の初期化

**注意**: ペルソナは`Initialize()`時点では取得しない（遅延読み込み）。これは`Evacuee.Awake()`で`Initialize()`が呼ばれる時点では、まだ`SetEvacueeId()`が呼ばれておらずペルソナが設定されていないため。

### ペルソナ遅延読み込み

```csharp
private void EnsurePersonaLoaded()
```

`MakeDecisionExtended()`の冒頭で呼び出され、以下を実行:

- ペルソナの取得（`_evacuee.GetPersona()`）
- 再現可能な乱数生成器の再初期化（エージェントIDベースのシード）
- 準備時間の計算
- 有効最大待機時間の計算

この遅延初期化により、`ShelterEnvManager`が`SetEvacueeId()`でペルソナを設定した後に、正しくペルソナ情報を取得できる。

### 意思決定

```csharp
// Level 1-2（後方互換性）
public (ActionType action, GameObject targetShelter, string reasoning) MakeDecision()

// Level 3（拡張版）
public (ActionType action, GameObject targetShelter, string reasoning, FamilyMember familyTarget) MakeDecisionExtended()
```

### リセット（エピソード開始時）

```csharp
public void Reset()
{
    // 基本状態
    _stayStartTime = -1f;
    _lastEvaluationTime = 0f;
    _hasHeardBroadcast = false;
    _hasReceivedJAlert = false;
    _hasHeardFireTruck = false;
    _hasContactedFamily = false;
    _alertReceivedCount = 0;
    _preparationComplete = false;
    _preparationStartTime = -1f;
    _followTargetEvacuee = null;

    // 避難先ロックのリセット
    _lockedDestination = null;

    // 情報伝播フラグのリセット
    _hasHeardFromNearbyAgent = false;
    _informationSourceName = null;

    // 再計算
    if (_decisionRandom != null)
    {
        CalculatePreparationTime();
        _effectiveMaxStayDuration = CalculateEffectiveMaxStayDuration();
    }
}
```

---

## 検証方法

### Level 3 検証項目

#### 1. mental_state別の行動分布

- 楽観的・自己信頼型: STAY比率が高い、避難開始が遅延
- 社交的・周囲配慮型: FOLLOW発生率 > 0
- 冷静系: EVACUATE比率が高い

#### 2. 避難完了率の確認

- Level 3: 80-90%（mental_stateで変動）
- 楽観的・自己信頼型: 70-80%
- 冷静系: 95%以上

#### 3. FOLLOW発生率

- 全体: 15-25%
- 社交的・周囲配慮型: 30-50%
- 不安傾向・サポート希求型: 50-70%

#### 4. メトリクス記録

- `SimulationMetrics.RecordAction()` でログ出力
- `episode_X_summary.csv` の `agent_type` が `RuleBased` であることを確認
- mental_state別のグループ分析

---

## 関連コンポーネント

- [ExperimentConfig](../../Assets/Scripts/ExperimentConfig.cs): エージェントタイプ切り替え
- [Evacuee](../../Assets/Scripts/Evacuee.cs): 避難者エージェント
- [EnvManager](../../Assets/Scripts/ShelterEnvManager.cs): 環境管理
- [SimulationMetrics](../../Assets/Scripts/SimulationMetrics.cs): 評価ログ
- [FamilyData](../../Assets/Scripts/FamilyData.cs): 家族情報
- [PersonaData](../../Assets/Scripts/LLM/PersonaData.cs): ペルソナ情報

---

## 実験での使用

### 実験1: 基本性能比較

1. `ExperimentConfig.SelectedAgentType = RuleBased` に設定
2. `ExperimentConfig.NumberOfTrials = 10` に設定
3. シミュレーションを実行
4. `Logs/experiment_results/` の結果をLLMエージェントと比較

### 期待される結果（Level 3）

| 指標 | LLMエージェント | ルールベースLevel 3 | ルールベースLevel 1-2 |
|------|----------------|-------------------|---------------------|
| 避難完了率 | 79% | 80-90% | 95-100% |
| FOLLOW発生率 | 15% | 15-25% | 0% |
| 避難開始時間 | 60-180秒 | 30-150秒 | 21秒（一斉） |
| 行動多様性 | 高い | 中程度 | 低い |

---

## Future Work

Level 3で多くの機能が実装済みとなった。以下は将来的な拡張候補である。

### Level 3 実装状況

| フィールド | LLMでの利用 | Level 3対応状況 |
|-----------|------------|----------------|
| `speed_multiplier` | 移動速度の調整 | ✅ NavAgent.speedで反映済み |
| `has_smartphone` | CONTACT可否判定 | ⚠️ CONTACTアクション自体が非対応 |
| `mental_state` | LLMプロンプトでの性格反映 | ✅ 7タイプで行動分化 |
| `age_group` | LLMの判断材料 | ✅ 準備時間に反映 |
| `physical_condition` | 移動能力の制約 | 🔲 未対応 |
| `past_disaster_experience` | 過去経験による判断 | 🔲 未対応 |

### アクション対応状況

| アクション | Level 1 | Level 2 | Level 3 |
|-----------|---------|---------|---------|
| EVACUATE | ✅ | ✅ | ✅ |
| STAY | ✅ | ✅ | ✅ |
| SEARCH_FAMILY | - | ✅ | ✅ |
| CONTACT | - | ✅ | ❌ 削除 |
| FOLLOW | - | - | ✅ |
| TALK | - | - | ⚠️ Pseudo-TALK（情報伝播）として実装 |

### 残りの拡張候補

#### 1. CONTACTアクションの復活

Level 3では簡略化のためCONTACTアクションが削除されている。シーン外家族への連絡機能が必要な場合は再実装を検討。

#### 2. 先行研究で実装されているルールベースの要素

| 要素 | 現状 | 対応案 |
|------|------|--------|
| 群衆回避 | ❌ 未対応 | NavMeshAgentの障害物回避に依存 |
| 避難所の混雑回避 | ⚠️ 最寄り回避で代替 | 混雑情報を知るのは非現実的なため、「近くは混む」と予測して2番目を選択 |
| 危険地帯（浸水域）回避 | ❌ 未対応 | 津波浸水予測区域を迂回する経路選択 |
| 持ち物準備時間 | ✅ Level 3で実装 | 年齢・mental_stateで変動 |
| 近所への声掛け | ❌ 未対応 | 消防団ペルソナがTALKを選択 |
| 車避難 | ❌ 未対応 | ペルソナ属性（高齢者・遠方居住）で車移動を選択 |
| 垂直避難（自宅2階への退避） | ❌ 未対応 | 高層建物が近い場合のEVACUATE先変更 |

#### 3. physical_conditionの反映

```csharp
// 身体状態による移動制約
if (_persona?.physical_condition?.Contains("杖") == true)
{
    // 移動速度を低下
    // 階段の多いルートを避ける
}
```

#### 4. past_disaster_experienceの反映

```csharp
// 過去の災害経験による避難開始閾値の調整
if (_persona?.past_disaster_experience?.Contains("津波") == true)
{
    // 正常性バイアスの影響を軽減
    // 避難開始が早くなる
}
```

---

### 実装優先度まとめ

| 機能 | 優先度 | 理由 |
|------|--------|------|
| physical_condition反映 | 中 | より現実的な移動制約 |
| past_disaster_experience反映 | 中 | 避難経験による行動変化 |
| CONTACTアクション復活 | 低 | シーン外家族対応が必要な場合 |
| 車避難・垂直避難 | 低 | シナリオ拡張時に検討 |
