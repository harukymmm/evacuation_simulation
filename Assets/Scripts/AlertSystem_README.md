# 防災行政無線とJアラートシステムの実装詳細

## 実装概要

防災行政無線（屋外スピーカー）とスマホのJアラート（緊急速報）をシミュレーションに組み込み、避難者がこれらの情報を受信したかどうかを LLM の判断に反映するシステムです。

---

## システム構成

### **1. 防災行政無線（EmergencyBroadcastSpeaker）**

**役割：** シーン内の特定位置にスピーカーを配置し、可聴範囲内の避難者に放送を届ける

**主要コンポーネント：**

- `EmergencyBroadcastSpeaker.cs` - スピーカー1台分の設定と状態管理

**設定項目：**

- 位置（`Transform.position`）
- 可聴半径（`audibleRadiusMeters`）
- 放送内容（`broadcastMessage`）
- 放送タイミング（`broadcastStartTimeSeconds`、`broadcastDurationSeconds`）

**可聴範囲の判定方法：**

- **単純な3D距離判定**を使用
- 避難者の位置とスピーカーの位置の距離を計算
- `距離 <= audibleRadiusMeters` なら「聞こえた」と判定
- 建物による遮蔽は考慮しない（シンプルな実装）

**判定タイミング：**

- `AlertManager` が `Update()` で **1秒ごとに** チェック
- 放送中（`IsBroadcasting()` が `true`）の間、継続的に判定

**なぜこの方法が「一番簡単かつ問題なく動作する」か：**

1. **実装が単純** - `Vector3.Distance()` だけで判定できる
2. **バグが入りにくい** - 複雑な物理計算や遮蔽判定がない
3. **パフォーマンスが良い** - 数百人の避難者 × 数個のスピーカーでも十分軽い
4. **拡張しやすい** - 後から遮蔽判定を追加する場合も、距離判定の上に重ねるだけ

---

### **2. Jアラート（AlertManager）**

**役割：** スマホ保有者に一斉に緊急速報を送信

**主要コンポーネント：**

- `AlertManager.cs` - Jアラートの発信タイミングと内容を管理

**設定項目：**

- 発信時刻（`jAlertSendTimeSeconds`）
- メッセージ内容（`jAlertMessage`）
- 発信シナリオ（`jAlertScenarios` - どのシナリオで発信するか）

**受信者の判定：**

- `PersonaData.has_smartphone` が `true` の避難者のみが受信
- 距離による制限はなし（エリアメールとして一斉配信）

**発信タイミング：**

- シミュレーション開始から `jAlertSendTimeSeconds` 秒後
- かつ、現在のシナリオが `jAlertScenarios` に含まれている場合のみ

---

### **3. 避難者側の状態管理（Evacuee）**

**保持する状態：**

- `_hasHeardBroadcast` - 行政無線の放送を聞いたか
- `_lastBroadcastMessage` - 最後に聞いた放送内容
- `_hasReceivedJAlert` - Jアラートを受信したか
- `_lastJAlertMessage` - 最後に受信したJアラート内容
- `HasSmartphone` - スマホを持っているか（`PersonaData` から取得）

**メソッド：**

- `OnHeardBroadcast(string message)` - AlertManager から呼ばれる
- `OnReceivedJAlert(string message)` - AlertManager から呼ばれる
- `ResetAlertState()` - エピソード開始時にリセット

---

### **4. LLMコンテキストへの反映**

**リクエスト（LLMEvacDecisionRequest）：**

- `has_heard_broadcast` - 放送を聞いたか
- `last_broadcast_message` - 放送内容
- `has_received_j_alert` - Jアラートを受信したか
- `last_j_alert_message` - Jアラート内容

**プロンプト（server.py）：**

- 放送を聞いた場合：

  ```
  【防災行政無線の放送】
  あなたは屋外スピーカーからの放送を聞きました:
  [放送内容]
  ```

- Jアラートを受信した場合：

  ```
  【スマホの緊急速報（Jアラート）】
  あなたのスマートフォンに、次のような警報が届きました:
  [Jアラート内容]
  ```

---

## データフロー

### **防災行政無線の流れ**

```
1. シーン起動時
   └─ EmergencyBroadcastSpeaker が GameObject にアタッチされている
   └─ AlertManager が自動検出（autoFindSpeakers = true）

2. シミュレーション開始
   └─ AlertManager.Update() が1秒ごとに実行

3. 放送開始時刻に達したら
   └─ speaker.IsBroadcasting(currentTime) == true

4. 全避難者をチェック
   └─ foreach (evacuee in Evacuees)
      └─ if (speaker.IsWithinAudibleRange(evacuee.position))
         └─ evacuee.OnHeardBroadcast(message)

5. 避難者が放送を記録
   └─ _hasHeardBroadcast = true
   └─ _lastBroadcastMessage = message

6. 次回のLLM呼び出し時
   └─ BuildEvacDecisionRequest() で
      └─ has_heard_broadcast = true
      └─ last_broadcast_message = "..."

7. LLMプロンプトに反映
   └─ 【防災行政無線の放送】セクションが追加される
```

### **Jアラートの流れ**

```
1. シミュレーション開始
   └─ AlertManager.Update() が1秒ごとに実行

2. 発信時刻に達したら
   └─ if (currentTime >= jAlertSendTimeSeconds)
      └─ if (現在のシナリオが jAlertScenarios に含まれる)

3. 全避難者をチェック
   └─ foreach (evacuee in Evacuees)
      └─ if (evacuee.HasSmartphone)
         └─ evacuee.OnReceivedJAlert(message)

4. 避難者がJアラートを記録
   └─ _hasReceivedJAlert = true
   └─ _lastJAlertMessage = message

5. 次回のLLM呼び出し時
   └─ BuildEvacDecisionRequest() で
      └─ has_received_j_alert = true
      └─ last_j_alert_message = "..."

6. LLMプロンプトに反映
   └─ 【スマホの緊急速報（Jアラート）】セクションが追加される
```

---

## 行政無線の位置設定方法（詳細）

### **Unity上での設定手順**

#### **方法1: 手動でGameObjectを作成（推奨）**

1. **GameObjectを作成**
   - Hierarchy で右クリック → `Create Empty`
   - 名前を変更（例: `Speaker_CityHall`）

2. **位置を設定**
   - Inspector の `Transform` で座標を入力
   - 例: 市役所の屋上 → `(5000, 20, 0)`
   - Scene ビューでドラッグして配置してもOK

3. **コンポーネントをアタッチ**
   - `Add Component` → `Emergency Broadcast Speaker`

4. **パラメータを設定**
   - `Audible Radius Meters`: 500（例）
   - `Broadcast Message`: "津波警報が発表されました..."
   - `Broadcast Start Time Seconds`: 10
   - `Broadcast Duration Seconds`: 30

5. **可視化を確認**
   - `Show Gizmos` を `true` にすると、Scene ビューで黄色のワイヤースフィアが表示される
   - これが可聴範囲

#### **方法2: スクリプトで自動配置（将来的な拡張）**

将来的には、PLATEAU の建物データから「公共施設」を自動検出し、
その位置にスピーカーを自動配置する機能も追加可能です。

```csharp
// 将来的な拡張例（実装はしていない）
var publicFacilities = FindObjectsByType<PLATEAUCityObjectGroup>()
    .Where(b => BuildingCategorizer.Categorize(b) == BuildingCategory.PublicFacility);

foreach (var facility in publicFacilities)
{
    GameObject speaker = new GameObject($"Speaker_{facility.name}");
    speaker.transform.position = facility.transform.position + Vector3.up * 10f; // 屋上
    speaker.AddComponent<EmergencyBroadcastSpeaker>();
}
```

---

### **位置の決め方（実用的なアプローチ）**

#### **アプローチ1: PLATEAU の建物データを参考にする**

1. `BuildingAttributeAnalyzer` ツールで建物の位置を確認
2. 公共施設（市役所、学校など）の `position` を取得
3. その座標にスピーカーを配置
4. Y座標は建物の高さに応じて調整（屋上を想定）

#### **アプローチ2: 実際の地図データを使用**

1. 実際の防災行政無線の設置場所があれば、その座標を Unity 座標系に変換
2. 座標変換は、PLATEAU の座標系と Unity の座標系の対応関係を考慮

#### **アプローチ3: シーン内で視覚的に配置**

1. Scene ビューで建物の位置を確認
2. マウスでドラッグしてスピーカーを配置
3. `Show Gizmos` で可聴範囲を確認しながら調整

---

### **可聴範囲の設定指針**

**一般的な屋外スピーカーの可聴範囲：**

- 小型（学校など）: 200-400m
- 中型（市役所など）: 400-600m
- 大型（広域放送）: 600-1000m

**シミュレーションでの推奨値：**

- デフォルト: `500m`
- 市役所などの重要施設: `800m`
- 小学校などの小規模施設: `400m`

**注意点：**

- 実際の可聴範囲は風向きや建物の遮蔽で変わるが、現在の実装では考慮していない
- 必要に応じて、シナリオに応じた調整（例: 風が強い日は範囲が広がる）も可能

---

## 実装の詳細説明

### **可聴域の実装方法（なぜこの方法が選ばれたか）**

#### **採用した方法：単純な距離判定**

```csharp
// EmergencyBroadcastSpeaker.cs
public bool IsWithinAudibleRange(Vector3 position)
{
    float distance = Vector3.Distance(transform.position, position);
    return distance <= audibleRadiusMeters;
}
```

#### **なぜこの方法が「一番簡単かつ問題なく動作する」か**

1. **実装が極めてシンプル**
   - `Vector3.Distance()` は Unity の標準関数で、バグの余地が少ない
   - 複雑な物理計算や遮蔽判定が不要

2. **パフォーマンスが良い**
   - 距離計算は O(1) で高速
   - 数百人の避難者 × 数個のスピーカーでも、1秒ごとのチェックなら十分軽い
   - 最適化の余地も大きい（距離の二乗で比較するなど）

3. **バグが入りにくい**
   - 判定ロジックが単純なので、予期しない動作が起きにくい
   - デバッグも容易（距離をログ出力するだけ）

4. **拡張しやすい**
   - 後から遮蔽判定を追加する場合も、距離判定の上に `Raycast` を重ねるだけ
   - 段階的に機能を追加できる

#### **他の方法と比較**

**❌ 3D AudioSource に完全依存**

- 問題: 「物理的に聞こえる音」と「シミュレーション上の認知」が混在
- 問題: 「誰が聞いたか」をロジック側で取り出せない

**❌ Physics.OverlapSphere を使用**

- メリット: Unity の標準機能
- デメリット: コライダーが必要で、避難者にコライダーを追加する必要がある
- デメリット: オーバーヘッドが大きい

**❌ NavMesh 距離を使用**

- メリット: 経路距離でより現実的
- デメリット: 音は直線で伝わるので、NavMesh距離は不適切
- デメリット: 計算コストが高い

**✅ 単純な3D距離判定（採用）**

- メリット: シンプル、高速、バグが少ない
- デメリット: 遮蔽を考慮しない（ただし、最初の実装としては十分）

---

### **判定のタイミングと頻度**

#### **現在の実装**

```csharp
// AlertManager.cs
private const float CHECK_INTERVAL = 1f; // 1秒ごとにチェック

void Update()
{
    float currentTime = _envManager.CurrentTimeSec;
    if (currentTime - _lastCheckTime < CHECK_INTERVAL)
    {
        return; // 1秒経過していない場合はスキップ
    }
    _lastCheckTime = currentTime;
    
    CheckBroadcastSpeakers(currentTime);
}
```

**なぜ1秒ごと？**

- 毎フレーム（60fps なら 0.016秒ごと）チェックする必要はない
- 避難者の移動速度を考えると、1秒ごとで十分
- パフォーマンスと精度のバランスが良い

**将来的な最適化：**

- 放送中のみチェック（放送前後はスキップ）
- 空間分割（グリッド）で近くのスピーカーのみチェック
- ただし、現時点では不要（十分に軽い）

---

### **エピソード開始時のリセット**

#### **リセットが必要な理由**

- エピソードが終了して新しいエピソードが始まると、避難者が再スポーンされる
- 前のエピソードで「聞いた」状態が残っていると、新しいエピソードで誤動作する

#### **リセットのタイミング**

```csharp
// EnvManager.OnEpisodeBegin()
1. AlertManager.OnEpisodeStart() を呼ぶ
   └─ _jAlertSent = false
   └─ 全スピーカーの ResetBroadcastState()

2. 全避難者の ResetAlertState() を呼ぶ
   └─ _hasHeardBroadcast = false
   └─ _lastBroadcastMessage = ""
   └─ _hasReceivedJAlert = false
   └─ _lastJAlertMessage = ""
```

---

## 使用例

### **シナリオ: 震度7＋津波**

1. **シーン設定**
   - 市役所の位置に `Speaker_CityHall` を配置（可聴範囲: 800m）
   - 小学校の位置に `Speaker_School` を配置（可聴範囲: 400m）

2. **放送設定**
   - `Speaker_CityHall`: "津波警報が発表されました。海岸から離れて高台に避難してください。"
   - 開始時刻: 15秒後、継続: 60秒

3. **Jアラート設定**
   - `AlertManager`: 発信時刻 20秒後
   - メッセージ: "【緊急速報】津波警報が発表されました..."
   - シナリオ: `Shindo7Tsunami` のみ

4. **実行結果**
   - 市役所から800m以内の避難者 → 行政無線を聞く
   - スマホ保有者 → 20秒後にJアラートを受信
   - LLM は「行政無線とJアラートの両方で津波警報を知った」という情報を元に判断

---

## まとめ

この実装により：

1. **防災行政無線**: 位置ベースの可聴範囲で、一部の避難者だけが情報を得る
2. **Jアラート**: 端末ベースで、スマホ保有者が一斉に情報を得る
3. **LLMへの反映**: どちらの情報も、避難者の判断に影響する

**可聴域の実装は「単純な距離判定」を採用**することで、シンプルで確実に動作するシステムを実現しています。





