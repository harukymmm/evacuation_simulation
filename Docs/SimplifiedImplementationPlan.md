# 浪江町避難シミュレーション 簡易実装プラン

## 既存システムの理解

### 現在の構成

```text
Unity (既存)
├── Evacuee.cs          : NavMeshで最短距離の避難所に移動
├── Shelter.cs          : 収容人数管理
├── ShelterEnvManager.cs: 強化学習環境（ML-Agents）
└── ShelterManagementAgent.cs: 強化学習エージェント
```

**現状の動作**：

- 避難者は最短距離の避難所に向かう（単純なルールベース）
- 収容人数が満杯の場合、次の避難所を探す
- 強化学習エージェントが避難所の選択を学習

---

## 提案：3段階の段階的実装

### ✅ Phase 1: 既存システムの拡張（実現可能）

既存の`Evacuee.cs`を拡張し、**簡単なペルソナシステム**を追加

#### 1.1 ペルソナクラスの追加

```csharp
[System.Serializable]
public class EvacueePersona
{
    // 基本属性
    public int age;                      // 年齢
    public float physicalAbility;        // 身体能力 (0-1)
    public float riskAversion;           // リスク回避傾向 (0-1)
    public float normalcyBias;           // 正常性バイアス (0-1)
    public float socialInfluence;        // 同調傾向 (0-1)
    
    // 移動速度の計算
    public float GetMovementSpeed()
    {
        float baseSpeed = 1.5f;  // m/s
        
        // 年齢による減速
        if (age > 65) baseSpeed *= 0.7f;
        else if (age < 20) baseSpeed *= 1.2f;
        
        // 身体能力による調整
        baseSpeed *= physicalAbility;
        
        return baseSpeed;
    }
    
    // 避難所選択の優先度を計算
    public float CalculateShelterScore(Shelter shelter, float distance)
    {
        float score = 0f;
        
        // リスク回避傾向が高い人は、収容人数に余裕がある避難所を好む
        float capacityRatio = (float)shelter.currentCapacity / shelter.MaxCapacity;
        score += riskAversion * capacityRatio * 10f;
        
        // 距離による減点（全員共通だが、リスク回避傾向が高い人はより重視しない）
        score -= (1f - riskAversion * 0.5f) * distance / 100f;
        
        return score;
    }
}
```

#### 1.2 Evacueeクラスの拡張

```csharp
public class Evacuee : MonoBehaviour {
    
    [Header("Persona")]
    public EvacueePersona persona;  // 追加
    
    [Header("Decision Making")]
    public float decisionDelay = 0f;  // 意思決定の遅延（正常性バイアスに影響）
    private float startTime;
    
    void Awake() {
        // ペルソナをランダム生成（または外部から設定）
        if (persona == null) {
            persona = GenerateRandomPersona();
        }
        
        // 正常性バイアスが高い人は避難開始が遅れる
        decisionDelay = persona.normalcyBias * 10f;  // 最大10秒遅延
        startTime = Time.time;
        
        // NavMeshAgentの速度をペルソナに応じて設定
        NavAgent = GetComponent<NavMeshAgent>();
        NavAgent.speed = persona.GetMovementSpeed();
        
        // 以下、既存コード...
    }
    
    private EvacueePersona GenerateRandomPersona()
    {
        return new EvacueePersona
        {
            age = Random.Range(10, 90),
            physicalAbility = Random.Range(0.5f, 1.0f),
            riskAversion = Random.Range(0.3f, 1.0f),
            normalcyBias = Random.Range(0.2f, 0.8f),
            socialInfluence = Random.Range(0.3f, 0.9f)
        };
    }
    
    // 避難所検索ロジックを拡張（ペルソナベース）
    private List<GameObject> SearchShelters(List<string> excludeTowerUUIDs = null) {
        // 既存コード...
        
        // ペルソナに基づいてソート
        sortedTowerPoints.Sort((a, b) => {
            Shelter shelterA = a.transform.parent.GetComponent<Shelter>();
            Shelter shelterB = b.transform.parent.GetComponent<Shelter>();
            float distA = Vector3.Distance(a.transform.position, transform.position);
            float distB = Vector3.Distance(b.transform.position, transform.position);
            
            float scoreA = persona.CalculateShelterScore(shelterA, distA);
            float scoreB = persona.CalculateShelterScore(shelterB, distB);
            
            return scoreB.CompareTo(scoreA);  // 降順
        });
        
        return sortedTowerPoints;
    }
}
```

**この段階の出力**：

- 避難完了率の時系列変化（年齢層別、リスク回避傾向別）
- 避難所ごとの収容状況
- エージェントごとの避難完了時間
- ペルソナと避難時間の相関分析

---

### 🔄 Phase 2: Python連携（オプション）

複雑な意思決定が必要な場合のみ、Pythonサーバーと連携

#### 2.1 軽量なWebSocket通信

```csharp
// Unity側
public class LLMDecisionClient : MonoBehaviour
{
    private WebSocket ws;
    
    async void Start()
    {
        ws = new WebSocket("ws://localhost:8765");
        await ws.Connect();
    }
    
    // LLMに意思決定を問い合わせ（同期的に待つ）
    public async Task<string> RequestDecision(EvacueeState state)
    {
        string json = JsonUtility.ToJson(state);
        ws.Send(json);
        
        // レスポンスを待つ
        string response = await ReceiveResponse();
        return response;
    }
}
```

```python
# Python側（簡易版）
import asyncio
import websockets
import json

class SimpleEvacuationLLM:
    def __init__(self, llm_model="gpt-4"):
        self.llm = ChatOpenAI(model=llm_model, temperature=0.7)
    
    def decide_action(self, state: dict) -> dict:
        """LLMで意思決定"""
        prompt = f"""
あなたは{state['age']}歳の避難者です。
現在地から避難所までの距離：
- 避難所A: {state['shelter_a_distance']}m (収容率: {state['shelter_a_capacity']}%)
- 避難所B: {state['shelter_b_distance']}m (収容率: {state['shelter_b_capacity']}%)

どちらの避難所に向かいますか？AまたはBで答えてください。
"""
        
        response = self.llm.invoke(prompt)
        return {"decision": response.content}

async def handler(websocket):
    llm = SimpleEvacuationLLM()
    async for message in websocket:
        state = json.loads(message)
        decision = llm.decide_action(state)
        await websocket.send(json.dumps(decision))

async def main():
    async with websockets.serve(handler, "localhost", 8765):
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())
```

**この段階の出力**：

- Phase 1の出力に加えて
- LLMの推論ログ（なぜその避難所を選んだか）
- 意思決定の多様性指標（Shannon Entropy）

---

### 🚀 Phase 3: 高度な機能（時間があれば）

- RAGによる地域知識の活用
- エージェント間のコミュニケーション
- 創発的行動の検出

---

## 実装の優先順位

### 🟢 最優先（11月中に実装可能）

1. **EvacueePersonaクラスの実装**
   - 5つの基本パラメータ（年齢、身体能力、リスク回避、正常性バイアス、同調傾向）
   - ペルソナに基づく移動速度の調整
   - ペルソナに基づく避難所選択

2. **データ収集システムの拡張**
   - ペルソナデータの記録
   - 避難完了時間の記録
   - 経路データの記録

3. **基本的な可視化**
   - ペルソナ別の避難完了時間分布
   - 避難所の収容率の時系列変化

### 🟡 中優先（12月上旬）

4. **Python連携の実装**
   - WebSocketサーバーの構築
   - 簡易的なLLM意思決定
   - Unity-Python間のデータ転送

5. **多様性指標の計算**
   - Shannon Entropy
   - 経路の多様性

### 🔴 低優先（時間があれば）

6. **高度な機能**
   - RAG
   - エージェント間通信
   - 創発パターン検出

---

## 期待される出力

### 1. CSVファイル（既存機能を拡張）

#### `evacuee_details.csv`

```csv
EvacueeID,Age,PhysicalAbility,RiskAversion,NormalcyBias,TargetShelter,EvacuationTime,PathLength
0,75,0.6,0.8,0.5,ShelterA,45.2,250.5
1,32,0.9,0.4,0.3,ShelterB,23.1,180.2
2,68,0.7,0.9,0.7,ShelterA,52.8,245.1
...
```

#### `shelters_capacity.csv`

```csv
Time,ShelterA_Capacity,ShelterA_Current,ShelterB_Capacity,ShelterB_Current
0.0,100,0,80,0
5.0,100,12,80,8
10.0,100,45,80,32
...
```

#### `evacuation_summary.csv`

```csv
Episode,TotalEvacuees,EvacuationRate,AverageTime,MaxTime,Diversity(Entropy)
0,100,0.95,42.5,85.2,1.23
1,100,0.98,38.2,78.5,1.45
...
```

### 2. 可視化（Matplotlibで生成）

#### グラフ1: 避難完了率の時系列変化

```python
import matplotlib.pyplot as plt

# 時間 vs 避難完了率
plt.plot(time, evacuation_rate)
plt.xlabel('Time (seconds)')
plt.ylabel('Evacuation Rate')
plt.title('Evacuation Progress Over Time')
plt.savefig('evacuation_progress.png')
```

#### グラフ2: ペルソナと避難時間の相関

```python
# 年齢 vs 避難時間
plt.scatter(ages, evacuation_times, c=risk_aversion, cmap='coolwarm')
plt.xlabel('Age')
plt.ylabel('Evacuation Time (seconds)')
plt.colorbar(label='Risk Aversion')
plt.title('Age vs Evacuation Time (colored by Risk Aversion)')
plt.savefig('age_vs_time.png')
```

#### グラフ3: 避難所選択の分布

```python
# 避難所ごとの選択数（ペルソナ別）
# リスク回避傾向で色分け
```

### 3. Unityでのリアルタイム表示

- 各避難者の上に年齢・リスク回避傾向を表示
- 色で身体能力を可視化（赤=低、緑=高）
- 避難所の収容状況をリアルタイム表示

### 4. 最終レポート（テキスト）

```text
=== シミュレーション結果サマリー ===

総避難者数: 100人
避難完了率: 95%
平均避難時間: 42.5秒
最大避難時間: 85.2秒

ペルソナ別統計:
- 高齢者（65歳以上）: 平均55.3秒
- 若年層（20-40歳）: 平均32.1秒
- リスク回避高（0.7以上）: 平均38.2秒
- リスク回避低（0.5以下）: 平均48.9秒

行動の多様性:
- 避難所選択のエントロピー: 1.23
- 経路の多様性: 0.78

ボトルネック:
- 避難所Aが15秒時点で満杯
- 10名が避難所Bへ迂回（平均12秒の遅延）
```

---

## 実装スケジュール（現実的）

### Week 1-2（11月中旬）

- [x] EvacueePersonaクラスの実装
- [x] 既存Evacueeクラスの拡張
- [x] ペルソナランダム生成
- [x] 基本的なログ出力

### Week 3（11月下旬）

- [ ] データ収集システムの拡張
- [ ] CSV出力の実装
- [ ] 基本的な可視化スクリプト

### Week 4（12月上旬）

- [ ] 簡易的なPython連携（オプション）
- [ ] 多様性指標の計算
- [ ] 結果の分析

### Week 5-6（12月中旬）

- [ ] 実験の実施
- [ ] データの分析
- [ ] 論文執筆

---

## まとめ

### 🎯 現実的なアプローチ

1. **Phase 1だけでも十分な貢献**：
   - ペルソナベースの行動多様性
   - 既存システムへの最小限の変更
   - 即座に実装可能

2. **段階的な拡張**：
   - Phase 1が動いたらPhase 2へ
   - 必要に応じてPhase 3へ

3. **明確な出力**：
   - CSV形式のデータ
   - 可視化グラフ
   - 統計サマリー

### ⚠️ 避けるべきこと

- 一気に複雑なLLMシステムを構築
- Unity-Python連携の複雑な実装
- 階層的意思決定などの過度な抽象化

### ✅ フォーカスすべきこと

- 既存コードへの最小限の変更
- 実装可能な範囲でのペルソナシステム
- 明確な評価指標とデータ収集
