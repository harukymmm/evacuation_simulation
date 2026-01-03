# 🎯 現実的な避難行動の実装プラン

現在の実装は「避難所選択→移動」のみですが、これを拡張して4つの行動を追加します。

---

## 📋 実装プラン概要

### **追加する行動タイプ**

1. **STAY（待機・自宅待機）** - その場に留まる
2. **SEARCH_FAMILY（家族探索）** - 特定の場所/人を探す
3. **CONTACT（連絡）** - 家族・知人に連絡を試みる
4. **FOLLOW（同調行動）** - 近くの避難者について行く
5. **EVACUATE（既存）** - 避難所に向かう

---

## 🏗️ 実装の全体構造

### **レイヤー1: データ構造の拡張**

```
LLMActionMessages.cs に追加:
├─ ActionType enum (STAY, SEARCH_FAMILY, CONTACT, FOLLOW, EVACUATE)
├─ LLMEvacDecisionResponse の拡張
│   ├─ action_type: string
│   ├─ target_location: Vector3Payload (SEARCH_FAMILY用)
│   ├─ target_evacuee_id: string (FOLLOW用)
│   ├─ contact_target: string (CONTACT用)
│   └─ stay_duration: float (STAY用)
└─ FamilyMemberPayload (家族情報)
```

### **レイヤー2: Unity側の実行ロジック**

```
Evacuee.cs に追加:
├─ 新しい状態変数
│   ├─ CurrentAction: ActionType
│   ├─ ActionStartTime: float
│   ├─ FamilyMembers: List<FamilyMember>
│   └─ FollowTarget: Evacuee
├─ 行動実行メソッド
│   ├─ ExecuteStayAction()
│   ├─ ExecuteSearchAction()
│   ├─ ExecuteContactAction()
│   ├─ ExecuteFollowAction()
│   └─ ApplyLLMDecision() の拡張
└─ 行動完了判定
    └─ CheckActionCompletion()
```

### **レイヤー3: サーバー側のプロンプト拡張**

```
server.py の変更:
├─ プロンプトに行動選択肢を追加
├─ 家族情報のコンテキスト追加
├─ 周辺の避難者情報の追加
└─ JSON形式の拡張
```

---

## 📝 詳細実装プラン

### **1. 待機行動（STAY）**

#### **目的**

- 自宅待機を選択
- 情報収集のための一時待機
- 余震が収まるのを待つ

#### **Unity側実装**

```csharp
// Evacuee.cs に追加

[Header("Stay Action")]
private float _stayStartTime;
private float _stayDuration;
private Vector3 _stayPosition;

private void ExecuteStayAction(float duration)
{
    _stayStartTime = Time.time;
    _stayDuration = duration;
    _stayPosition = transform.position;
    
    // NavMeshAgentを停止
    if (NavAgent != null)
    {
        NavAgent.isStopped = true;
        NavAgent.ResetPath();
    }
    
    Debug.Log($"[Evacuee] {gameObject.name}: 待機行動を開始（{duration}秒間）");
}

private bool IsStayActionComplete()
{
    return Time.time - _stayStartTime >= _stayDuration;
}
```

#### **LLMプロンプト例**

```
あなたは災害に遭遇しました。以下の行動から選択してください：

1. 避難所に向かう (EVACUATE)
2. その場で待機する (STAY)
3. 家族を探す (SEARCH_FAMILY)
4. 家族に連絡する (CONTACT)
5. 周囲の人について行く (FOLLOW)

【待機を選ぶべき状況】
- 建物が安全で、immediate危険がない
- 津波到達まで時間がある
- 家族の帰宅を待つ必要がある
- 情報収集が必要

JSON形式で回答:
{
  "action_type": "STAY",
  "stay_duration": 300.0,  // 秒単位
  "reasoning": "建物が安全で家族を待つ必要があるため"
}
```

---

### **2. 家族探索（SEARCH_FAMILY）**

#### **目的**

- 家族の居場所に移動
- 学校や職場に迎えに行く

#### **Unity側実装**

```csharp
// Evacuee.cs に追加

[Serializable]
public class FamilyMember
{
    public string name;              // 名前
    public string relation;          // 続柄（妻、子供、親など）
    public string likely_location;   // 想定される場所
    public Vector3 search_position;  // 探す場所の座標
    public bool found;               // 発見済みフラグ
}

[Header("Family Search")]
public List<FamilyMember> FamilyMembers = new List<FamilyMember>();
private FamilyMember _currentSearchTarget;

private void ExecuteSearchAction(Vector3 targetPosition, string targetName)
{
    _currentSearchTarget = FamilyMembers.Find(f => f.name == targetName);
    
    if (NavAgent != null)
    {
        NavAgent.isStopped = false;
        NavAgent.SetDestination(targetPosition);
    }
    
    Debug.Log($"[Evacuee] {gameObject.name}: {targetName}を探すため{targetPosition}に向かいます");
}

// 到着判定
private bool IsSearchLocationReached()
{
    if (_currentSearchTarget == null) return false;
    
    float distance = Vector3.Distance(transform.position, _currentSearchTarget.search_position);
    return distance < 5f; // 5m以内
}

// 家族発見シミュレーション（簡易版）
private void OnTriggerEnter(Collider other)
{
    if (_currentSearchTarget != null && other.CompareTag("FamilyMember"))
    {
        // 家族を発見した処理
        _currentSearchTarget.found = true;
        Debug.Log($"[Evacuee] {gameObject.name}: {_currentSearchTarget.name}を発見しました");
        
        // 次の行動をLLMに問い合わせ（家族と一緒に避難）
        RequestLLMDecision();
    }
}
```

#### **LLMリクエストに家族情報を追加**

```csharp
// LLMActionMessages.cs に追加

[Serializable]
public class FamilyMemberPayload
{
    public string name;
    public string relation;
    public string likely_location;
    public Vector3Payload search_position;
    public bool found;
    public float distance_meters;
}

// LLMEvacDecisionRequest に追加
public FamilyMemberPayload[] family_members;
```

#### **LLMプロンプト例**

```
【家族情報】
- 妻（田中 花子）: 自宅にいる可能性が高い（距離: 500m、徒歩7分）
- 息子（田中 太郎）: 小学校にいる（距離: 1.2km、徒歩15分）

【選択肢】
1. すぐに避難所に向かう
2. 妻を探しに自宅に戻る (SEARCH_FAMILY)
3. 息子を迎えに小学校に行く (SEARCH_FAMILY)

JSON形式:
{
  "action_type": "SEARCH_FAMILY",
  "target_family_member": "田中 太郎",
  "target_location": {"x": 1234.5, "y": 0, "z": 5678.9},
  "reasoning": "息子の安全を確認する必要がある"
}
```

---

### **3. 連絡行動（CONTACT）**

#### **目的**

- 家族に電話/メール
- 安否確認
- 現実的には通信遅延・不通をシミュレート

#### **Unity側実装**

```csharp
// Evacuee.cs に追加

[Header("Contact Action")]
private bool _isContactingFamily;
private float _contactStartTime;
private float _contactAttemptDuration = 30f; // 連絡試行時間
private string _contactTarget;
private bool _contactSuccessful;

private void ExecuteContactAction(string targetName)
{
    _isContactingFamily = true;
    _contactStartTime = Time.time;
    _contactTarget = targetName;
    
    // 連絡中は立ち止まる
    if (NavAgent != null)
    {
        NavAgent.isStopped = true;
    }
    
    // 連絡成功率をシミュレート（災害時は低い）
    float successRate = 0.3f; // 30%の確率で成功
    _contactSuccessful = UnityEngine.Random.value < successRate;
    
    Debug.Log($"[Evacuee] {gameObject.name}: {targetName}に連絡を試みています...");
}

private bool IsContactComplete()
{
    if (!_isContactingFamily) return false;
    
    float elapsed = Time.time - _contactStartTime;
    if (elapsed >= _contactAttemptDuration)
    {
        _isContactingFamily = false;
        
        if (_contactSuccessful)
        {
            Debug.Log($"[Evacuee] {gameObject.name}: {_contactTarget}との連絡に成功");
            // 家族の位置情報を更新（連絡で判明）
            UpdateFamilyLocation(_contactTarget);
        }
        else
        {
            Debug.Log($"[Evacuee] {gameObject.name}: {_contactTarget}との連絡に失敗（回線混雑）");
        }
        
        // 次の行動をLLMに問い合わせ
        RequestLLMDecision();
        return true;
    }
    
    return false;
}
```

#### **LLMプロンプト例**

```
【状況】
- 家族の安否が不明
- 携帯電話は繋がりにくい状態
- 津波到達まで15分

【選択肢】
1. すぐに避難所に向かう
2. 家族に連絡を試みる (CONTACT) - 30秒程度
3. 家族を探しに行く

JSON形式:
{
  "action_type": "CONTACT",
  "contact_target": "妻（田中 花子）",
  "reasoning": "安否確認後、状況に応じて行動を決定"
}
```

---

### **4. 同調行動（FOLLOW）**

#### **目的**

- 周囲の人の動きに従う
- 避難方向が分からない時
- 群集心理のシミュレート

#### **Unity側実装**

```csharp
// Evacuee.cs に追加

[Header("Follow Action")]
private Evacuee _followTarget;
private float _followDistance = 2f; // 追従距離
private float _followCheckInterval = 1f;
private float _lastFollowCheck;

private void ExecuteFollowAction(string targetEvacueeId)
{
    // 対象の避難者を探す
    GameObject targetObj = GameObject.Find(targetEvacueeId);
    if (targetObj != null)
    {
        _followTarget = targetObj.GetComponent<Evacuee>();
        Debug.Log($"[Evacuee] {gameObject.name}: {targetEvacueeId}について行きます");
    }
    else
    {
        Debug.LogWarning($"[Evacuee] {gameObject.name}: 追従対象が見つかりません");
        // フォールバック: 最寄りの避難所に向かう
        SelectNearestShelter();
    }
}

private void UpdateFollowAction()
{
    if (_followTarget == null) return;
    
    // 定期的に追従先の位置を更新
    if (Time.time - _lastFollowCheck >= _followCheckInterval)
    {
        _lastFollowCheck = Time.time;
        
        Vector3 targetPos = _followTarget.transform.position;
        float distance = Vector3.Distance(transform.position, targetPos);
        
        // 一定距離以上離れたら追いかける
        if (distance > _followDistance * 2)
        {
            if (NavAgent != null)
            {
                NavAgent.SetDestination(targetPos);
            }
        }
        // 近すぎる場合は止まる
        else if (distance < _followDistance)
        {
            if (NavAgent != null)
            {
                NavAgent.isStopped = true;
            }
        }
    }
    
    // 追従対象が避難所に到達したら、自分も同じ避難所に向かう
    if (_followTarget.isEvacuating)
    {
        Target = _followTarget.Target;
        _followTarget = null; // 追従解除
    }
}
```

#### **周辺避難者情報の取得**

```csharp
// LLMリクエストに周辺避難者情報を追加

[Serializable]
public class NearbyEvacueePayload
{
    public string id;
    public Vector3Payload position;
    public float distance;
    public string current_action; // "moving", "staying", "searching"
    public string direction; // "north", "shelter", etc.
}

// LLMEvacDecisionRequest に追加
public NearbyEvacueePayload[] nearby_evacuees;

// 実装
private List<NearbyEvacueePayload> GetNearbyEvacuees(float radius)
{
    var nearbyList = new List<NearbyEvacueePayload>();
    
    Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
    foreach (var col in colliders)
    {
        Evacuee other = col.GetComponent<Evacuee>();
        if (other != null && other != this)
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            nearbyList.Add(new NearbyEvacueePayload
            {
                id = other.gameObject.name,
                position = new Vector3Payload(other.transform.position),
                distance = distance,
                current_action = other.CurrentAction.ToString()
            });
        }
    }
    
    return nearbyList.OrderBy(e => e.distance).Take(5).ToList();
}
```

#### **LLMプロンプト例**

```
【周辺の避難者】
1. Evacuee3 - 距離: 10m、行動: 避難所に移動中
2. Evacuee5 - 距離: 15m、行動: 避難所に移動中
3. Evacuee7 - 距離: 20m、行動: その場で待機中

【あなたの状況】
- 避難所の場所が分からない
- 周囲の人が避難所に向かっているようだ

【選択肢】
1. 自分で判断して避難所を選ぶ
2. 近くの避難者について行く (FOLLOW)

JSON形式:
{
  "action_type": "FOLLOW",
  "target_evacuee_id": "Evacuee3",
  "reasoning": "避難所の場所が分からないため、移動中の人について行く"
}
```

---

## 🔄 行動の状態遷移

```
[開始]
  ↓
[LLM意思決定] ← ←←←←←←←←←←←←←
  ↓           ↑               ↑
[行動選択]     ↑               ↑
  ├→ STAY → [待機] ──────────┘
  ├→ SEARCH → [移動] → [到着判定] → [家族発見?] ─┘
  ├→ CONTACT → [連絡試行] → [成功/失敗] ────────┘
  ├→ FOLLOW → [追従] → [対象が避難完了] ────────┘
  └→ EVACUATE → [避難所へ移動] → [到達] → [終了]
```

---

## 📊 実装の優先順位

### **フェーズ1: 基本行動の実装** (1-2週間)

1. ✅ データ構造の拡張（ActionType, レスポンス形式）
2. ✅ STAY行動の実装
3. ✅ サーバー側プロンプトの拡張

### **フェーズ2: 家族関連の実装** (2-3週間)

4. ✅ 家族情報の管理
5. ✅ SEARCH_FAMILY行動
6. ✅ CONTACT行動

### **フェーズ3: 社会的行動の実装** (1-2週間)

7. ✅ 周辺避難者の検出
8. ✅ FOLLOW行動

### **フェーズ4: 統合とテスト** (1週間)

9. ✅ 全行動の統合
10. ✅ LLMプロンプトの最適化
11. ✅ 各行動のバランス調整

---

## ⚠️ 実装上の注意点

### **1. LLMの制約**

- 行動選択が複雑になるため、プロンプトの明確化が重要
- ペルソナ（高齢者、親、子供など）によって行動傾向を変える必要がある

### **2. パフォーマンス**

- 周辺避難者の検索は重い処理
- 更新頻度を調整（1秒に1回程度）

### **3. 現実性のバランス**

- 連絡の成功率を災害時の実データに基づいて設定
- 家族探索と避難のジレンマをリアルに表現

---

## 🎮 テストシナリオ例

### **シナリオ1: 家族優先**

- ペルソナ: 30代男性、妻と子供あり
- 期待行動: CONTACT → SEARCH_FAMILY → EVACUATE

### **シナリオ2: 高齢者**

- ペルソナ: 70代女性、一人暮らし
- 期待行動: STAY → FOLLOW → EVACUATE

### **シナリオ3: 施設管理者**

- ペルソナ: 責任感が強い
- 期待行動: STAY（他者を誘導） → EVACUATE（最後に）

---

この実装により、避難行動の多様性が大幅に向上し、より現実的なシミュレーションが可能になります。Agent modeに切り替えていただければ、順次実装を進めることができます。
