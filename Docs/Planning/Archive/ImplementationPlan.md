# 浪江町避難シミュレーション 改良実装案

## 概要

Proposal.mdとFeedback.mdの内容を踏まえ、「ダイバーシティのあるマルチエージェント」を核とした、現実的な避難行動シミュレーションの実装計画を提示する。

---

## 1. コアコンセプト

### 1.1 研究の肝：ダイバーシティのマルチエージェント

**既存研究との差別化**

- 既存研究：同じ型のエージェントでシミュレーション
- **本研究：個性とメモリーを持ったエージェントによる経路・意思決定の違い**

**実装すべき個性の軸**

- パーソナリティ：ポジティブ/ネガティブ、インドア/アウトドア
- 運動能力：年齢、リハビリデータ（足腰の強さ）
- 認知特性：リスク認知、正常性バイアスの強さ
- 社会的属性：家族構成、地域知識レベル

---

## 2. アーキテクチャ設計

### 2.1 ハイブリッドシミュレーション構成

```text
┌─────────────────────────────────────────┐
│  Unity (可視化・物理演算)                │
│  - PLATEAU 3D環境                        │
│  - NavMesh経路制御                       │
│  - リアルタイム可視化                    │
└──────────────┬──────────────────────────┘
               │ WebSocket/gRPC
┌──────────────┴──────────────────────────┐
│  Python シミュレーションサーバー          │
│  ┌─────────────────────────────────────┐│
│  │ セルオートマトン計算エンジン          ││
│  │ - グリッドベースの移動計算            ││
│  │ - 密度・流れの計算                    ││
│  │ - 災害状況の時系列変化                ││
│  └─────────────────────────────────────┘│
│  ┌─────────────────────────────────────┐│
│  │ マルチエージェントシステム            ││
│  │ - LLMエージェント管理                 ││
│  │ - エージェント間通信                  ││
│  │ - 意思決定の実行                      ││
│  └─────────────────────────────────────┘│
└───────────────────────────────────────┘
```

**メリット**

- セルオートマトンで高速な流れ計算
- Unityで直感的な可視化
- リアルタイムまたはバッチ処理の選択可

---

## 3. LLMエージェント実装

### 3.1 既存フレームワークの活用

**推奨フレームワーク**

1. **LangGraph** (推奨)
   - エージェントの状態管理とワークフロー
   - メモリーシステム統合
   - ツール呼び出しのサポート

2. **AgentOps / LangSmith**
   - エージェント動作のトレース・デバッグ
   - コスト管理

3. **Chroma / FAISS**
   - RAG用ベクトルDB
   - 地域知識・個人記憶の検索

### 3.2 エージェント認知アーキテクチャ

```python
class EvacueeAgent:
    def __init__(self, persona: Persona):
        self.persona = persona  # パーソナリティ・属性
        self.short_term_memory = []  # 短期記憶
        self.long_term_memory = VectorStore()  # 長期記憶（RAG）
        self.emotional_state = EmotionalState()  # 感情状態
        self.knowledge_base = KnowledgeBase()  # 地域知識
        
    def perceive(self, environment: Environment) -> Observation:
        """環境の知覚（視界、音、他者の行動）"""
        pass
        
    def reason(self, observation: Observation) -> Decision:
        """LLMによる推論・意思決定"""
        # 1. 記憶検索（RAG）
        relevant_memories = self.long_term_memory.search(observation)
        
        # 2. プロンプト構築
        prompt = self._build_prompt(
            persona=self.persona,
            observation=observation,
            memories=relevant_memories,
            emotional_state=self.emotional_state
        )
        
        # 3. LLM推論
        decision = self.llm.generate(prompt)
        
        # 4. 記憶への保存
        self.short_term_memory.append(observation)
        
        return decision
        
    def act(self, decision: Decision) -> Action:
        """決定を行動に変換"""
        pass
```

---

## 4. ペルソナシステム

### 4.1 ペルソナ設計（AAMASの論文参考）

```python
@dataclass
class Persona:
    # 基本属性
    age: int
    gender: str
    physical_ability: float  # 0-1: リハビリデータより
    
    # パーソナリティ（Big Five + 避難関連）
    extraversion: float  # 外向性
    neuroticism: float  # 神経症的傾向
    conscientiousness: float  # 誠実性
    risk_aversion: float  # リスク回避傾向
    normalcy_bias: float  # 正常性バイアス強度
    
    # 社会的属性
    family_members: List[str]  # 家族メンバーID
    local_knowledge: float  # 地域知識レベル
    social_influence: float  # 同調傾向
    
    # 認知特性
    decision_speed: float  # 意思決定速度
    memory_capacity: int  # 記憶容量
    
    # 背景ストーリー（LLMへの入力）
    background_story: str
    daily_routine: str  # 日常行動パターン
    
    def to_llm_prompt(self) -> str:
        """LLMプロンプト用のペルソナ記述を生成"""
        return f"""
あなたは以下の人物です：
- 年齢: {self.age}歳、性別: {self.gender}
- 身体能力: {'健康' if self.physical_ability > 0.7 else '足腰が弱い'}
- 性格: {'社交的' if self.extraversion > 0.6 else '内向的'}、
        {'慎重' if self.risk_aversion > 0.6 else '楽観的'}
- 背景: {self.background_story}
- 家族: {', '.join(self.family_members) if self.family_members else 'なし'}
- 地域知識: {'豊富' if self.local_knowledge > 0.7 else '限定的'}
"""
```

### 4.2 ペルソナ生成パイプライン

```python
class PersonaGenerator:
    def generate_diverse_population(
        self, 
        n: int, 
        demographic_data: Dict,
        rehabilitation_data: Optional[pd.DataFrame] = None
    ) -> List[Persona]:
        """
        多様な住民ペルソナを生成
        
        Args:
            n: 生成するエージェント数
            demographic_data: 浪江町の人口統計データ
            rehabilitation_data: リハビリデータ（利用可能な場合）
        """
        personas = []
        
        # 年齢分布に従って生成
        age_distribution = demographic_data['age_distribution']
        
        for i in range(n):
            age = self._sample_from_distribution(age_distribution)
            
            # リハビリデータがあれば運動能力を設定
            if rehabilitation_data is not None:
                physical_ability = self._get_physical_ability(
                    age, rehabilitation_data
                )
            else:
                physical_ability = self._estimate_physical_ability(age)
            
            # パーソナリティをランダム生成（分布を考慮）
            personality = self._generate_personality_traits()
            
            # 背景ストーリーをLLMで生成
            background = self._generate_background_story(
                age=age,
                personality=personality,
                local_context="浪江町"
            )
            
            persona = Persona(
                age=age,
                physical_ability=physical_ability,
                **personality,
                background_story=background
            )
            
            personas.append(persona)
        
        return personas
```

---

## 5. メモリーシステム

### 5.1 階層的メモリーアーキテクチャ

```python
class MemorySystem:
    def __init__(self):
        self.working_memory = WorkingMemory()  # 即時の状況
        self.episodic_memory = EpisodicMemory()  # エピソード記憶
        self.semantic_memory = SemanticMemory()  # 一般知識
        self.procedural_memory = ProceduralMemory()  # 手続き記憶
        
class WorkingMemory:
    """短期記憶：現在の状況、直近の観測"""
    def __init__(self, capacity: int = 7):
        self.capacity = capacity
        self.items = deque(maxlen=capacity)
        
class EpisodicMemory:
    """エピソード記憶：過去の経験、RAGで実装"""
    def __init__(self):
        self.vector_store = Chroma()
        
    def store(self, episode: Episode):
        """エピソードを記憶"""
        embedding = self._create_embedding(episode)
        self.vector_store.add(
            documents=[episode.description],
            embeddings=[embedding],
            metadatas=[episode.metadata]
        )
        
    def retrieve(self, query: str, k: int = 5) -> List[Episode]:
        """関連するエピソードを検索"""
        results = self.vector_store.similarity_search(query, k=k)
        return results
        
class SemanticMemory:
    """意味記憶：地域知識、避難所情報など"""
    def __init__(self):
        self.knowledge_graph = nx.Graph()
        self.vector_store = Chroma()
        
    def add_local_knowledge(self, knowledge: LocalKnowledge):
        """地域知識を追加"""
        pass
```

### 5.2 記憶の反映方法（修論のコア）

**LLMの内面を行動に反映する方法**

```python
class DecisionReflection:
    """LLMの内面的推論を外部行動に変換"""
    
    def reflect_internal_state(
        self, 
        llm_output: str,
        persona: Persona,
        emotional_state: EmotionalState
    ) -> Action:
        """
        LLMの推論テキストから具体的な行動を抽出
        
        課題：
        - LLMの出力は自然言語で曖昧
        - 物理シミュレーションには具体的な数値が必要
        
        解決策：
        1. 構造化出力（JSON形式）
        2. 感情状態による行動修正
        3. ペルソナによる行動フィルタリング
        """
        
        # 1. LLM出力をパース（JSON / Function Calling）
        decision = self._parse_llm_output(llm_output)
        
        # 2. 感情による行動修正
        modified_decision = self._apply_emotional_modifier(
            decision, emotional_state
        )
        
        # 3. ペルソナによる制約
        final_action = self._apply_persona_constraints(
            modified_decision, persona
        )
        
        # 4. 物理パラメータに変換
        action = Action(
            target_location=final_action['target'],
            speed=self._calculate_speed(persona, emotional_state),
            communication=final_action.get('communication'),
            priority=final_action.get('priority', 0.5)
        )
        
        return action
        
    def _calculate_speed(self, persona: Persona, emotion: EmotionalState):
        """ペルソナと感情から移動速度を計算"""
        base_speed = persona.physical_ability * 1.5  # m/s
        
        # パニック状態では速度が変化
        if emotion.panic_level > 0.7:
            # 高齢者はパニックで転倒リスク増加→減速
            if persona.age > 65:
                base_speed *= 0.8
            else:
                base_speed *= 1.2
                
        return base_speed
```

**構造化出力の例（Function Calling）**

```python
decision_schema = {
    "type": "object",
    "properties": {
        "action_type": {
            "type": "string",
            "enum": ["evacuate", "wait", "search_family", "help_others", "gather_info"]
        },
        "target_location": {
            "type": "string",
            "description": "目的地（避難所名または地点）"
        },
        "reasoning": {
            "type": "string",
            "description": "この行動を選んだ理由"
        },
        "emotional_state": {
            "type": "object",
            "properties": {
                "anxiety": {"type": "number", "minimum": 0, "maximum": 1},
                "urgency": {"type": "number", "minimum": 0, "maximum": 1}
            }
        },
        "communication": {
            "type": "object",
            "properties": {
                "target": {"type": "string"},
                "message": {"type": "string"}
            }
        }
    }
}
```

---

## 6. セルオートマトン×Unity連携

### 6.1 アーキテクチャ

```python
# Python側：セルオートマトンエンジン
class CellularAutomataEngine:
    def __init__(self, grid_size: Tuple[int, int], cell_size: float):
        self.grid = np.zeros(grid_size)
        self.cell_size = cell_size  # メートル/セル
        self.agents = {}
        
    def update(self, dt: float) -> Dict[str, Any]:
        """
        シミュレーションを1ステップ進める
        
        Returns:
            Unity送信用の状態データ
        """
        # 1. エージェントの意思決定（LLM）
        for agent_id, agent in self.agents.items():
            observation = self._get_observation(agent)
            decision = agent.reason(observation)
            self._apply_decision(agent, decision)
        
        # 2. セルオートマトンの更新
        self._update_grid()
        
        # 3. 衝突・密度計算
        self._calculate_density()
        
        # 4. Unity用データ作成
        unity_data = {
            'agents': [
                {
                    'id': aid,
                    'position': agent.position.tolist(),
                    'velocity': agent.velocity.tolist(),
                    'emotional_state': agent.emotional_state.to_dict()
                }
                for aid, agent in self.agents.items()
            ],
            'grid_density': self.grid.tolist(),
            'events': self.events
        }
        
        return unity_data
```

```csharp
// Unity側：データ受信と可視化
public class SimulationManager : MonoBehaviour
{
    private WebSocket ws;
    private Dictionary<string, GameObject> agentObjects;
    
    async void Start()
    {
        ws = new WebSocket("ws://localhost:8765");
        ws.OnMessage += OnSimulationUpdate;
        await ws.Connect();
    }
    
    void OnSimulationUpdate(object sender, MessageEventArgs e)
    {
        var data = JsonUtility.FromJson<SimulationState>(e.Data);
        
        // エージェント位置を更新
        foreach (var agentData in data.agents)
        {
            UpdateAgent(agentData);
        }
        
        // 密度ヒートマップを更新
        UpdateDensityHeatmap(data.grid_density);
    }
    
    void UpdateAgent(AgentData data)
    {
        if (!agentObjects.ContainsKey(data.id))
        {
            agentObjects[data.id] = InstantiateAgent(data);
        }
        
        var agent = agentObjects[data.id];
        agent.transform.position = new Vector3(
            data.position[0], 
            data.position[1], 
            data.position[2]
        );
        
        // 感情状態をビジュアルに反映
        var renderer = agent.GetComponent<Renderer>();
        renderer.material.color = GetEmotionColor(data.emotional_state);
    }
}
```

---

## 7. 入出力の詳細設計：行動の自由度と分散の実現

### 7.1 設計思想

**従来の避難シミュレーションの問題点**

- 出力が「避難所A/B/Cのどれかを選ぶ」という離散的な選択肢に限定
- 行動が画一的で、個人の多様性が反映されない
- 現実の人間の複雑な意思決定プロセスを再現できない

**本研究のアプローチ**

- **連続的な行動空間**：目的地、速度、経路、行動の優先度を連続値で表現
- **階層的意思決定**：戦略レベル → 戦術レベル → 実行レベルの多段階決定
- **コンテキスト依存**：状況に応じて創発的に行動を生成
- **制約付き自由度**：物理的・社会的制約の範囲内で自由な行動

### 7.2 入力設計：マルチモーダルな環境認識

#### 7.2.1 入力の階層構造

```python
@dataclass
class AgentInput:
    """エージェントへの入力データの完全な構造"""
    
    # 1. 自己状態
    self_state: SelfState
    
    # 2. 環境知覚
    perception: EnvironmentPerception
    
    # 3. 社会的情報
    social_context: SocialContext
    
    # 4. 記憶
    memories: MemoryContext
    
    # 5. 時間情報
    temporal_context: TemporalContext

@dataclass
class SelfState:
    """自己の内部状態"""
    position: Tuple[float, float, float]  # (x, y, z) 現在位置
    velocity: Tuple[float, float]  # (vx, vy) 現在速度
    energy_level: float  # 0-1: 体力残量
    stress_level: float  # 0-1: ストレスレベル
    injuries: List[str]  # 怪我の状態
    current_goal: Optional[str]  # 現在の目標
    inventory: List[str]  # 所持品（水、食料、薬など）
    
@dataclass
class EnvironmentPerception:
    """環境の知覚情報"""
    
    # 視覚情報
    visible_area: VisibleArea
    obstacles: List[Obstacle]
    hazards: List[Hazard]  # 火災、浸水、建物倒壊など
    
    # 聴覚情報
    sounds: List[Sound]  # サイレン、叫び声、アナウンスなど
    
    # 空間情報
    nearby_locations: List[Location]
    shelters_in_range: List[Shelter]
    exit_routes: List[Route]
    
    # 環境状態
    weather: WeatherCondition
    time_of_day: float  # 0-24
    visibility: float  # 0-1: 視界の良さ
    crowd_density: float  # 周辺の混雑度

@dataclass
class VisibleArea:
    """視界内の情報"""
    radius: float  # 視界半径（メートル）
    agents_visible: List[AgentSnapshot]  # 視界内の他エージェント
    buildings: List[Building]
    landmarks: List[Landmark]  # 目印となる建物や看板

@dataclass
class SocialContext:
    """社会的文脈情報"""
    
    # 家族・知人
    family_members: List[FamilyMember]  # 家族の状態と位置
    acquaintances: List[Acquaintance]  # 知人情報
    
    # 集団行動
    nearby_crowd_behavior: CrowdBehavior  # 周囲の群衆の行動
    group_formation: Optional[GroupInfo]  # 所属する集団
    
    # コミュニケーション
    recent_messages: List[Message]  # 受信したメッセージ
    social_pressure: float  # 0-1: 同調圧力の強さ
    
    # 権威・指示
    official_announcements: List[Announcement]  # 公式アナウンス
    authority_presence: List[Authority]  # 警察官、消防士など

@dataclass
class MemoryContext:
    """記憶からの情報"""
    working_memory: List[RecentEvent]  # 直近の出来事
    relevant_experiences: List[PastExperience]  # RAGで取得した関連経験
    local_knowledge: List[LocalKnowledge]  # 地域知識
    learned_patterns: List[Pattern]  # 学習したパターン

@dataclass
class TemporalContext:
    """時間的文脈"""
    elapsed_time: float  # 災害発生からの経過時間
    time_pressure: float  # 0-1: 時間的切迫度
    predicted_hazard_evolution: HazardPrediction  # 災害の予測進展
```

#### 7.2.2 入力の構造化と圧縮

LLMのコンテキスト長には限界があるため、効率的な入力表現が必要：

```python
class InputCompressor:
    """入力情報を効率的に圧縮してLLMに渡す"""
    
    def compress_input(
        self, 
        agent_input: AgentInput,
        persona: Persona
    ) -> str:
        """
        入力を自然言語プロンプトに変換
        重要度に応じて情報を取捨選択
        """
        
        # 1. 最も重要な情報を抽出
        critical_info = self._extract_critical_info(agent_input)
        
        # 2. ペルソナに応じた知覚フィルタリング
        # 例：内向的な人は社会的情報への注目度が低い
        filtered_info = self._apply_persona_filter(agent_input, persona)
        
        # 3. 自然言語プロンプト生成
        prompt = f"""
## あなたの状態
位置: {agent_input.self_state.position}
体力: {agent_input.self_state.energy_level:.0%}
ストレス: {agent_input.self_state.stress_level:.0%}

## 現在の状況
{self._describe_environment(filtered_info.perception)}

## 周囲の人々
{self._describe_social_context(filtered_info.social_context)}

## あなたが知っていること
{self._describe_memories(filtered_info.memories)}

## 時間情報
災害発生から{agent_input.temporal_context.elapsed_time:.0f}分経過
{self._describe_time_pressure(agent_input.temporal_context)}

あなたは今、何をしますか？
"""
        return prompt
        
    def _describe_environment(self, perception: EnvironmentPerception) -> str:
        """環境を自然言語で記述"""
        desc = []
        
        # 視界内の重要な情報
        if perception.hazards:
            hazard_desc = ", ".join([h.description for h in perception.hazards])
            desc.append(f"危険: {hazard_desc}")
        
        # 避難所情報
        if perception.shelters_in_range:
            shelter_desc = "\n".join([
                f"  - {s.name}: {s.distance:.0f}m先、収容{s.current_capacity}/{s.max_capacity}人"
                for s in perception.shelters_in_range
            ])
            desc.append(f"近くの避難所:\n{shelter_desc}")
        
        # 混雑状況
        if perception.crowd_density > 0.5:
            desc.append(f"周囲は{'非常に' if perception.crowd_density > 0.8 else 'やや'}混雑している")
        
        return "\n".join(desc)
```

### 7.3 出力設計：高自由度な行動空間

#### 7.3.1 階層的行動決定

単一の意思決定ではなく、複数レイヤーで段階的に決定：

```python
@dataclass
class HierarchicalDecision:
    """階層的意思決定の出力"""
    
    # レベル1: 戦略的決定（高レベル目標）
    strategic_intent: StrategicIntent
    
    # レベル2: 戦術的決定（具体的な行動計画）
    tactical_plan: TacticalPlan
    
    # レベル3: 実行レベル（即座の行動）
    immediate_action: ImmediateAction
    
    # メタ情報
    reasoning: str  # 推論プロセス
    confidence: float  # 0-1: 決定への確信度
    alternatives_considered: List[Alternative]  # 検討した代替案

@dataclass
class StrategicIntent:
    """戦略的意図（長期目標）"""
    
    primary_goal: str  # 主目標
    # 例: "家族と合流する", "高台に避難する", "安全な場所で様子を見る"
    
    secondary_goals: List[str]  # 副次目標
    # 例: ["食料を確保", "怪我人を助ける"]
    
    goal_priority: Dict[str, float]  # 各目標の優先度 0-1
    
    risk_tolerance: float  # 0-1: リスク許容度
    time_horizon: float  # 計画の時間スケール（分）
    
    constraints: List[str]  # 制約条件
    # 例: ["避難所Aは避ける（過去に嫌な経験）", "水辺は通らない"]

@dataclass
class TacticalPlan:
    """戦術的計画（中期行動）"""
    
    action_sequence: List[PlannedAction]  # 行動のシーケンス
    
    waypoints: List[Waypoint]  # 経由地点
    
    contingency_plans: Dict[str, Plan]  # 状況別の代替計画
    # 例: {"道路が塞がれている": Plan(...), "避難所が満員": Plan(...)}
    
    resource_allocation: ResourcePlan  # リソース配分
    
    collaboration_strategy: Optional[CollaborationPlan]  # 協力戦略

@dataclass
class PlannedAction:
    """計画された行動"""
    action_type: str
    duration: float  # 予想所要時間（分）
    location: Optional[Tuple[float, float]]
    rationale: str  # 理由

@dataclass
class ImmediateAction:
    """即座の実行行動（連続値パラメータ）"""
    
    # 移動行動
    movement: MovementAction
    
    # 相互作用
    interaction: Optional[InteractionAction]
    
    # 内部状態の変化
    emotional_response: EmotionalResponse
    
    # 実行パラメータ
    execution_params: ExecutionParameters

@dataclass
class MovementAction:
    """移動行動の詳細パラメータ"""
    
    # 目的地（連続空間）
    target_position: Tuple[float, float]  # (x, y) 絶対座標
    target_type: str  # "shelter", "waypoint", "person", "arbitrary"
    
    # 移動方式
    movement_type: str  # "direct", "cautious", "exploratory", "follow_crowd"
    
    # 速度パラメータ（連続値）
    desired_speed: float  # 0.0-3.0 m/s: 希望速度
    urgency: float  # 0.0-1.0: 切迫度
    
    # 経路選好（連続値）
    route_preferences: RoutePreferences
    
    # 群衆行動
    crowd_behavior: CrowdBehavior

@dataclass
class RoutePreferences:
    """経路選択の選好パラメータ（すべて0-1の連続値）"""
    
    prefer_main_roads: float  # 主要道路の選好度
    prefer_shortcuts: float  # 近道の選好度
    avoid_crowds: float  # 混雑回避度
    prefer_familiar_routes: float  # 慣れた道の選好度
    avoid_hazards: float  # 危険回避度
    follow_others: float  # 他者追従度
    
    def calculate_route_score(self, route: Route) -> float:
        """経路のスコアを計算"""
        score = 0.0
        score += self.prefer_main_roads * route.is_main_road
        score += self.prefer_shortcuts * (1.0 - route.normalized_distance)
        score -= self.avoid_crowds * route.crowd_density
        score += self.prefer_familiar_routes * route.familiarity
        score -= self.avoid_hazards * route.hazard_level
        return score

@dataclass
class InteractionAction:
    """他者との相互作用"""
    
    interaction_type: str
    # "communicate", "help", "follow", "avoid", "lead", "request_info"
    
    target_agents: List[str]  # 対象エージェントID
    
    # コミュニケーション
    message: Optional[Message]
    communication_mode: str  # "verbal", "gesture", "phone"
    
    # 協力行動
    cooperation: Optional[CooperationAction]
    
    # 相互作用の強度
    interaction_intensity: float  # 0-1

@dataclass
class EmotionalResponse:
    """感情的反応（連続値）"""
    
    anxiety_change: float  # -1 to 1: 不安度の変化
    panic_change: float  # -1 to 1: パニック度の変化
    hope_level: float  # 0-1: 希望の度合い
    frustration: float  # 0-1: 苛立ち
    
    # 感情の伝播
    empathy_response: float  # 0-1: 他者の感情への共感度

@dataclass
class ExecutionParameters:
    """実行時のパラメータ調整"""
    
    attention_focus: Dict[str, float]  # 注意の配分
    # 例: {"navigation": 0.6, "hazard_avoidance": 0.3, "social_awareness": 0.1}
    
    decision_flexibility: float  # 0-1: 計画の柔軟性
    monitoring_frequency: float  # 環境監視の頻度（秒）
    
    replanning_threshold: float  # 0-1: 再計画を始める閾値
```

#### 7.3.2 構造化出力スキーマ（Function Calling）

LLMに対して構造化された出力を要求：

```python
hierarchical_decision_schema = {
    "type": "object",
    "required": ["strategic_intent", "tactical_plan", "immediate_action"],
    "properties": {
        "strategic_intent": {
            "type": "object",
            "properties": {
                "primary_goal": {
                    "type": "string",
                    "description": "あなたの主要な目標（自由記述）"
                },
                "secondary_goals": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "副次的な目標のリスト"
                },
                "goal_priority": {
                    "type": "object",
                    "description": "各目標の優先度（0-1）",
                    "additionalProperties": {
                        "type": "number",
                        "minimum": 0,
                        "maximum": 1
                    }
                },
                "risk_tolerance": {
                    "type": "number",
                    "minimum": 0,
                    "maximum": 1,
                    "description": "どれだけリスクを取れるか"
                },
                "constraints": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "行動上の制約"
                }
            }
        },
        "tactical_plan": {
            "type": "object",
            "properties": {
                "action_sequence": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "action_type": {"type": "string"},
                            "duration": {"type": "number"},
                            "location": {
                                "type": "array",
                                "items": {"type": "number"},
                                "minItems": 2,
                                "maxItems": 2
                            },
                            "rationale": {"type": "string"}
                        }
                    }
                },
                "waypoints": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "position": {
                                "type": "array",
                                "items": {"type": "number"}
                            },
                            "description": {"type": "string"}
                        }
                    }
                },
                "contingency_plans": {
                    "type": "object",
                    "description": "状況別の代替計画"
                }
            }
        },
        "immediate_action": {
            "type": "object",
            "required": ["movement"],
            "properties": {
                "movement": {
                    "type": "object",
                    "properties": {
                        "target_position": {
                            "type": "array",
                            "items": {"type": "number"},
                            "minItems": 2,
                            "maxItems": 2,
                            "description": "目的地の座標 [x, y]"
                        },
                        "target_type": {
                            "type": "string",
                            "enum": ["shelter", "waypoint", "person", "arbitrary", "wait"],
                            "description": "目的地のタイプ"
                        },
                        "movement_type": {
                            "type": "string",
                            "enum": ["direct", "cautious", "exploratory", "follow_crowd"],
                            "description": "移動方式"
                        },
                        "desired_speed": {
                            "type": "number",
                            "minimum": 0.0,
                            "maximum": 3.0,
                            "description": "希望速度 (m/s)"
                        },
                        "urgency": {
                            "type": "number",
                            "minimum": 0.0,
                            "maximum": 1.0,
                            "description": "切迫度"
                        },
                        "route_preferences": {
                            "type": "object",
                            "properties": {
                                "prefer_main_roads": {"type": "number", "minimum": 0, "maximum": 1},
                                "prefer_shortcuts": {"type": "number", "minimum": 0, "maximum": 1},
                                "avoid_crowds": {"type": "number", "minimum": 0, "maximum": 1},
                                "prefer_familiar_routes": {"type": "number", "minimum": 0, "maximum": 1},
                                "avoid_hazards": {"type": "number", "minimum": 0, "maximum": 1},
                                "follow_others": {"type": "number", "minimum": 0, "maximum": 1}
                            },
                            "description": "経路選好パラメータ（すべて0-1）"
                        }
                    }
                },
                "interaction": {
                    "type": "object",
                    "properties": {
                        "interaction_type": {
                            "type": "string",
                            "enum": ["communicate", "help", "follow", "avoid", "lead", "request_info", "none"]
                        },
                        "target_agents": {
                            "type": "array",
                            "items": {"type": "string"}
                        },
                        "message": {
                            "type": "object",
                            "properties": {
                                "content": {"type": "string"},
                                "urgency": {"type": "number", "minimum": 0, "maximum": 1}
                            }
                        }
                    }
                },
                "emotional_response": {
                    "type": "object",
                    "properties": {
                        "anxiety_change": {"type": "number", "minimum": -1, "maximum": 1},
                        "panic_change": {"type": "number", "minimum": -1, "maximum": 1},
                        "hope_level": {"type": "number", "minimum": 0, "maximum": 1},
                        "frustration": {"type": "number", "minimum": 0, "maximum": 1}
                    }
                }
            }
        },
        "reasoning": {
            "type": "string",
            "description": "この決定に至った思考プロセスの説明"
        },
        "confidence": {
            "type": "number",
            "minimum": 0,
            "maximum": 1,
            "description": "この決定への確信度"
        }
    }
}
```

### 7.4 行動分散を実現する仕組み

#### 7.4.1 分散のソース（多様性の源泉）

```python
class DiversityEngine:
    """行動の多様性を生み出すエンジン"""
    
    def generate_diverse_response(
        self,
        agent_input: AgentInput,
        persona: Persona,
        llm: LLM
    ) -> HierarchicalDecision:
        """
        同じ状況でもペルソナによって異なる応答を生成
        """
        
        # 1. ペルソナによる知覚バイアス
        biased_input = self._apply_perceptual_bias(agent_input, persona)
        
        # 2. ペルソナ特化のプロンプト
        persona_prompt = self._create_persona_specific_prompt(persona)
        
        # 3. 記憶からのユニークな経験
        personal_memories = self._retrieve_personal_memories(
            agent_input, persona
        )
        
        # 4. 確率的サンプリング（temperature制御）
        temperature = self._calculate_temperature(persona)
        
        # 5. LLM推論
        decision = llm.generate(
            input=biased_input,
            persona_prompt=persona_prompt,
            memories=personal_memories,
            temperature=temperature,
            schema=hierarchical_decision_schema
        )
        
        # 6. ペルソナによる後処理フィルタ
        final_decision = self._apply_persona_constraints(decision, persona)
        
        return final_decision
    
    def _apply_perceptual_bias(
        self,
        agent_input: AgentInput,
        persona: Persona
    ) -> AgentInput:
        """ペルソナに基づく知覚バイアス"""
        
        biased_input = copy.deepcopy(agent_input)
        
        # 神経症的傾向が高い→危険を過大評価
        if persona.neuroticism > 0.7:
            for hazard in biased_input.perception.hazards:
                hazard.perceived_severity *= 1.5
        
        # リスク回避傾向が高い→安全な選択肢を優先
        if persona.risk_aversion > 0.7:
            # 避難所情報を詳しく知覚
            biased_input.perception.shelters_in_range = sorted(
                biased_input.perception.shelters_in_range,
                key=lambda s: s.safety_rating,
                reverse=True
            )
        
        # 外向的→社会的情報への注目度が高い
        if persona.extraversion > 0.6:
            biased_input.social_context.social_pressure *= 1.3
        
        # 正常性バイアス→危険を過小評価
        if persona.normalcy_bias > 0.7:
            for hazard in biased_input.perception.hazards:
                hazard.perceived_severity *= 0.7
        
        return biased_input
    
    def _calculate_temperature(self, persona: Persona) -> float:
        """
        ペルソナに基づいてLLMのtemperatureを調整
        → 出力のランダム性を制御
        """
        base_temp = 0.7
        
        # 誠実性が高い→一貫した行動（低temperature）
        if persona.conscientiousness > 0.7:
            base_temp *= 0.8
        
        # 神経症的傾向が高い→不安定な行動（高temperature）
        if persona.neuroticism > 0.7:
            base_temp *= 1.2
        
        # 外向性が高い→多様な行動（高temperature）
        if persona.extraversion > 0.7:
            base_temp *= 1.1
        
        return np.clip(base_temp, 0.3, 1.5)
```

#### 7.4.2 行動の自由度を保証する仕組み

```python
class ActionSpaceManager:
    """行動空間の管理と自由度の保証"""
    
    def __init__(self, environment: Environment):
        self.environment = environment
        self.action_space = self._define_action_space()
    
    def _define_action_space(self) -> ActionSpace:
        """
        連続的な行動空間を定義
        避難所の選択に限定しない
        """
        return ActionSpace(
            # 移動先（連続2D空間）
            target_position=ContinuousSpace(
                min_x=self.environment.bounds.min_x,
                max_x=self.environment.bounds.max_x,
                min_y=self.environment.bounds.min_y,
                max_y=self.environment.bounds.max_y
            ),
            
            # 速度（連続値）
            speed=ContinuousRange(min=0.0, max=3.0),
            
            # 行動タイプ（離散的だが多様）
            action_types=[
                "evacuate_to_shelter",
                "evacuate_to_safe_area",  # 避難所以外の安全な場所
                "search_family",
                "help_others",
                "gather_information",
                "wait_and_observe",
                "return_home",  # 物を取りに戻る
                "follow_crowd",
                "explore_alternative",  # 代替経路を探索
                "rest",  # 休憩
                "avoid_hazard",  # 危険を回避する移動
                "custom"  # 自由な目的地
            ],
            
            # 経路選好（連続多次元空間）
            route_preferences=ContinuousVector(dim=6, range=(0, 1)),
            
            # 相互作用（多様な選択肢）
            interaction_types=[
                "none",
                "ask_for_info",
                "share_info",
                "request_help",
                "offer_help",
                "coordinate",
                "warn",
                "reassure"
            ]
        )
    
    def validate_and_adjust_action(
        self,
        decision: HierarchicalDecision,
        agent: Agent
    ) -> HierarchicalDecision:
        """
        決定を検証し、必要に応じて調整
        物理的・社会的制約を適用しつつ自由度を保つ
        """
        
        adjusted = copy.deepcopy(decision)
        
        # 1. 物理的制約のチェック
        adjusted = self._apply_physical_constraints(adjusted, agent)
        
        # 2. 到達可能性のチェック
        adjusted = self._check_reachability(adjusted, agent)
        
        # 3. ソフト制約（推奨事項）
        # ハード制約と違い、エージェントが敢えて破ることも可能
        warnings = self._check_soft_constraints(adjusted, agent)
        
        return adjusted
    
    def _apply_physical_constraints(
        self,
        decision: HierarchicalDecision,
        agent: Agent
    ) -> HierarchicalDecision:
        """物理的制約の適用"""
        
        # 速度制限（体力と年齢に応じて）
        max_speed = agent.persona.physical_ability * 2.0
        if agent.persona.age > 70:
            max_speed *= 0.7
        
        if decision.immediate_action.movement.desired_speed > max_speed:
            decision.immediate_action.movement.desired_speed = max_speed
        
        # 移動範囲制限（障害物を考慮）
        target = decision.immediate_action.movement.target_position
        if not self.environment.is_accessible(target):
            # 最も近い到達可能な地点に調整
            decision.immediate_action.movement.target_position = \
                self.environment.find_nearest_accessible(target)
        
        return decision
```

#### 7.4.3 創発的行動の促進

```python
class EmergentBehaviorPromoter:
    """創発的行動を促すシステム"""
    
    def enhance_prompt_for_creativity(
        self,
        base_prompt: str,
        persona: Persona,
        situation_novelty: float
    ) -> str:
        """
        状況の新規性に応じてプロンプトを調整し、
        創発的な行動を引き出す
        """
        
        creativity_boost = ""
        
        if situation_novelty > 0.7:
            creativity_boost = """
あなたは前例のない状況に直面しています。
従来の避難マニュアル通りの行動だけでなく、
状況に応じた柔軟で創造的な判断が求められます。

以下のような選択肢も検討してください：
- マニュアルにない安全な場所への避難
- 独自の経路の選択
- 他者との自発的な協力
- 状況に応じた待機や情報収集
"""
        
        # ペルソナが開放性が高い場合
        if hasattr(persona, 'openness') and persona.openness > 0.7:
            creativity_boost += "\nあなたは新しいアイデアや独自の判断を好む性格です。"
        
        return base_prompt + "\n" + creativity_boost
    
    def detect_emergent_patterns(
        self,
        action_history: List[HierarchicalDecision]
    ) -> List[EmergentPattern]:
        """
        エージェント集団から創発的なパターンを検出
        """
        patterns = []
        
        # 例1: 自発的な群れ形成
        if self._detect_spontaneous_grouping(action_history):
            patterns.append(EmergentPattern(
                type="spontaneous_grouping",
                description="エージェントが自発的に集団を形成"
            ))
        
        # 例2: 代替経路の発見
        if self._detect_alternative_route_discovery(action_history):
            patterns.append(EmergentPattern(
                type="route_innovation",
                description="マニュアルにない経路を複数のエージェントが選択"
            ))
        
        # 例3: 情報カスケード
        if self._detect_information_cascade(action_history):
            patterns.append(EmergentPattern(
                type="info_cascade",
                description="情報が連鎖的に伝播"
            ))
        
        return patterns
```

### 7.5 実装例：完全なエージェントループ

```python
class LLMEvacuationAgent:
    """完全な入出力を持つLLM避難エージェント"""
    
    def __init__(
        self,
        agent_id: str,
        persona: Persona,
        llm: LLM,
        environment: Environment
    ):
        self.id = agent_id
        self.persona = persona
        self.llm = llm
        self.environment = environment
        
        # システムコンポーネント
        self.memory_system = MemorySystem()
        self.diversity_engine = DiversityEngine()
        self.action_space_manager = ActionSpaceManager(environment)
        self.input_compressor = InputCompressor()
        
        # 状態
        self.position = None
        self.emotional_state = EmotionalState()
        self.current_plan = None
        
    def step(self, dt: float) -> Action:
        """1ステップの実行"""
        
        # 1. 環境の知覚
        agent_input = self._perceive_environment()
        
        # 2. 入力の圧縮とプロンプト生成
        prompt = self.input_compressor.compress_input(
            agent_input, 
            self.persona
        )
        
        # 3. LLMによる意思決定（多様性を保証）
        decision = self.diversity_engine.generate_diverse_response(
            agent_input=agent_input,
            persona=self.persona,
            llm=self.llm
        )
        
        # 4. 行動の検証と調整
        validated_decision = self.action_space_manager.validate_and_adjust_action(
            decision, self
        )
        
        # 5. 記憶への保存
        self._memorize_decision(agent_input, validated_decision)
        
        # 6. 行動の実行
        action = self._execute_decision(validated_decision)
        
        # 7. ロギング
        self._log_decision(agent_input, validated_decision, action)
        
        return action
    
    def _perceive_environment(self) -> AgentInput:
        """環境知覚の実装"""
        
        # 視界内の情報を取得
        visible_agents = self.environment.get_agents_in_radius(
            self.position, 
            radius=50.0  # 50m
        )
        
        visible_hazards = self.environment.get_hazards_in_radius(
            self.position,
            radius=100.0
        )
        
        nearby_shelters = self.environment.get_shelters_in_radius(
            self.position,
            radius=500.0
        )
        
        # 社会的文脈
        social_context = self._build_social_context(visible_agents)
        
        # 記憶の検索
        memory_context = self._retrieve_relevant_memories(
            query=f"現在位置{self.position}での避難"
        )
        
        return AgentInput(
            self_state=SelfState(
                position=self.position,
                velocity=self.velocity,
                energy_level=self.energy_level,
                stress_level=self.emotional_state.stress_level,
                injuries=[],
                current_goal=self.current_plan.primary_goal if self.current_plan else None,
                inventory=self.inventory
            ),
            perception=EnvironmentPerception(
                visible_area=VisibleArea(
                    radius=50.0,
                    agents_visible=visible_agents,
                    buildings=self.environment.get_buildings_in_radius(self.position, 100),
                    landmarks=self.environment.get_landmarks_in_radius(self.position, 200)
                ),
                obstacles=self.environment.get_obstacles_nearby(self.position),
                hazards=visible_hazards,
                sounds=self.environment.get_sounds_at(self.position),
                nearby_locations=self.environment.get_locations_nearby(self.position),
                shelters_in_range=nearby_shelters,
                exit_routes=self.environment.find_routes_from(self.position),
                weather=self.environment.current_weather,
                time_of_day=self.environment.current_time,
                visibility=self.environment.get_visibility(),
                crowd_density=self.environment.get_crowd_density_at(self.position)
            ),
            social_context=social_context,
            memories=memory_context,
            temporal_context=TemporalContext(
                elapsed_time=self.environment.elapsed_time,
                time_pressure=self._calculate_time_pressure(),
                predicted_hazard_evolution=self.environment.get_hazard_prediction()
            )
        )
    
    def _log_decision(
        self,
        input_data: AgentInput,
        decision: HierarchicalDecision,
        action: Action
    ):
        """詳細なログを記録（分析用）"""
        log_entry = {
            "timestamp": self.environment.current_time,
            "agent_id": self.id,
            "persona": {
                "age": self.persona.age,
                "extraversion": self.persona.extraversion,
                "risk_aversion": self.persona.risk_aversion,
                # ... 他のペルソナ属性
            },
            "input": {
                "position": input_data.self_state.position,
                "nearby_shelters": len(input_data.perception.shelters_in_range),
                "hazards": [h.type for h in input_data.perception.hazards],
                "crowd_density": input_data.perception.crowd_density
            },
            "decision": {
                "primary_goal": decision.strategic_intent.primary_goal,
                "target_position": decision.immediate_action.movement.target_position,
                "desired_speed": decision.immediate_action.movement.desired_speed,
                "movement_type": decision.immediate_action.movement.movement_type,
                "route_preferences": decision.immediate_action.movement.route_preferences.__dict__,
                "reasoning": decision.reasoning,
                "confidence": decision.confidence
            },
            "action": {
                "actual_position": action.position,
                "actual_speed": action.speed
            }
        }
        
        # ログをファイルまたはDBに保存
        self.logger.log(log_entry)
```

### 7.6 行動分散の評価指標

```python
class DiversityMetrics:
    """行動の多様性を定量評価"""
    
    @staticmethod
    def calculate_action_entropy(
        decisions: List[HierarchicalDecision]
    ) -> float:
        """
        Shannon Entropyによる行動の分散度
        """
        from scipy.stats import entropy
        
        # 行動タイプの分布
        action_types = [d.immediate_action.movement.movement_type 
                       for d in decisions]
        type_counts = Counter(action_types)
        type_probs = np.array(list(type_counts.values())) / len(action_types)
        
        return entropy(type_probs)
    
    @staticmethod
    def calculate_route_diversity(
        agents: List[Agent],
        start_region: Region,
        end_region: Region
    ) -> float:
        """
        経路の多様性を計算
        同じ出発地・目的地でも経路が分散しているか
        """
        routes = []
        for agent in agents:
            if agent.route and agent.route.intersects(start_region, end_region):
                routes.append(agent.route)
        
        if len(routes) < 2:
            return 0.0
        
        # 経路間の類似度を計算
        similarities = []
        for i in range(len(routes)):
            for j in range(i+1, len(routes)):
                sim = routes[i].similarity(routes[j])  # Jaccard係数など
                similarities.append(sim)
        
        # 平均類似度が低い = 多様性が高い
        avg_similarity = np.mean(similarities)
        diversity = 1.0 - avg_similarity
        
        return diversity
    
    @staticmethod
    def calculate_decision_heterogeneity(
        decisions: List[HierarchicalDecision]
    ) -> Dict[str, float]:
        """
        意思決定の異質性を多角的に評価
        """
        
        # 目標の多様性
        goals = [d.strategic_intent.primary_goal for d in decisions]
        goal_diversity = len(set(goals)) / len(goals)
        
        # 速度の分散
        speeds = [d.immediate_action.movement.desired_speed for d in decisions]
        speed_variance = np.var(speeds)
        
        # 経路選好の多様性（多次元空間での分散）
        route_prefs = np.array([
            [
                d.immediate_action.movement.route_preferences.prefer_main_roads,
                d.immediate_action.movement.route_preferences.prefer_shortcuts,
                d.immediate_action.movement.route_preferences.avoid_crowds,
                d.immediate_action.movement.route_preferences.prefer_familiar_routes,
                d.immediate_action.movement.route_preferences.avoid_hazards,
                d.immediate_action.movement.route_preferences.follow_others
            ]
            for d in decisions
        ])
        route_pref_variance = np.mean(np.var(route_prefs, axis=0))
        
        return {
            "goal_diversity": goal_diversity,
            "speed_variance": speed_variance,
            "route_preference_variance": route_pref_variance,
            "overall_heterogeneity": (goal_diversity + route_pref_variance) / 2
        }
```

### 7.7 実験設定：多様性の検証

```python
class DiversityExperiment:
    """行動多様性の実証実験"""
    
    def run_diversity_validation(
        self,
        n_agents: int = 100,
        scenario: Scenario = "tsunami_warning"
    ):
        """
        同一シナリオ、同一初期条件で
        ペルソナの違いによる行動分散を検証
        """
        
        # 1. 多様なペルソナを生成
        personas = PersonaGenerator().generate_diverse_population(
            n=n_agents,
            demographic_data=NAMIE_DEMOGRAPHICS
        )
        
        # 2. 同一初期状態から開始
        initial_state = self._create_initial_state(scenario)
        
        # 3. 各エージェントの初回意思決定を取得
        decisions = []
        for persona in personas:
            agent = LLMEvacuationAgent(
                agent_id=f"agent_{len(decisions)}",
                persona=persona,
                llm=self.llm,
                environment=initial_state
            )
            decision = agent.step(dt=1.0)
            decisions.append(decision)
        
        # 4. 多様性指標を計算
        metrics = DiversityMetrics.calculate_decision_heterogeneity(decisions)
        
        # 5. 可視化
        self._visualize_decision_distribution(decisions, personas)
        
        # 6. ペルソナとの相関分析
        correlations = self._analyze_persona_behavior_correlation(
            decisions, personas
        )
        
        return {
            "diversity_metrics": metrics,
            "correlations": correlations,
            "decisions": decisions
        }
    
    def _visualize_decision_distribution(
        self,
        decisions: List[HierarchicalDecision],
        personas: List[Persona]
    ):
        """意思決定の分布を可視化"""
        
        import matplotlib.pyplot as plt
        
        # 目的地の分布（2D空間）
        positions = [d.immediate_action.movement.target_position 
                    for d in decisions]
        x = [p[0] for p in positions]
        y = [p[1] for p in positions]
        
        # ペルソナの属性で色分け
        colors = [p.risk_aversion for p in personas]
        
        plt.figure(figsize=(12, 8))
        plt.scatter(x, y, c=colors, cmap='RdYlGn_r', alpha=0.6)
        plt.colorbar(label='Risk Aversion')
        plt.title('Distribution of Target Positions by Risk Aversion')
        plt.xlabel('X Position')
        plt.ylabel('Y Position')
        plt.savefig('decision_diversity.png')
```

---

## 8. 実装ロードマップ

### Phase 1: 基盤整備（11月上旬～中旬）

**優先度：高**

- [ ] Python-Unity通信基盤の構築
  - WebSocket/gRPCサーバーの実装
  - Unity側の受信・送信クライアント
  
- [ ] ペルソナシステムの実装
  - Personaクラスの設計
  - 浪江町の人口統計データの取得・整理
  - ペルソナ生成パイプライン
  
- [ ] LLMフレームワークの統合
  - LangGraphのセットアップ
  - 基本的なエージェントループ
  - プロンプトテンプレート作成

### Phase 2: コア機能実装（11月中旬～下旬）

**優先度：高**

- [ ] **入出力システムの実装（重要）**
  - AgentInput構造の実装（環境知覚、社会的文脈、記憶）
  - HierarchicalDecision構造の実装（戦略・戦術・実行レベル）
  - InputCompressorの実装（ペルソナに応じた情報フィルタリング）
  - 構造化出力スキーマの定義（Function Calling対応）
  
- [ ] **行動空間の設計（重要）**
  - ActionSpaceManagerの実装
  - 連続的な行動空間の定義
  - 物理的・社会的制約の実装
  - RoutePreferencesシステム
  
- [ ] メモリーシステムの実装
  - 短期記憶（Working Memory）
  - 長期記憶（RAG + Vector DB）
  - 記憶検索ロジック
  
- [ ] 意思決定システム
  - LLM推論パイプライン
  - 階層的意思決定の実装
  - 内面→行動の変換ロジック
  
- [ ] セルオートマトンエンジン
  - グリッドベースの移動計算
  - 密度・流れの計算
  - NavMeshとの統合

### Phase 3: 多様性の実装（11月下旬～12月上旬）

**優先度：高（修論のコア）**

- [ ] **行動分散メカニズムの実装（最重要）**
  - DiversityEngineの実装
  - 知覚バイアスシステム（ペルソナ依存の環境認識）
  - Temperature制御による確率的サンプリング
  - ペルソナフィルタの実装
  
- [ ] **創発的行動の促進**
  - EmergentBehaviorPromoterの実装
  - 状況に応じたプロンプト調整
  - 創発パターンの検出ロジック
  
- [ ] パーソナリティの行動への反映
  - 性格特性による意思決定の違い
  - リスク認知の個人差
  - 正常性バイアスの実装
  
- [ ] 社会的相互作用
  - エージェント間通信プロトコル
  - 同調行動の実装
  - 家族合流行動
  
- [ ] 感情状態のダイナミクス
  - パニック伝播モデル
  - 感情による行動修正
  - EmotionalResponseの実装

### Phase 4: 検証・評価（12月上旬～中旬）

**優先度：中**

- [ ] **行動多様性の評価（重要）**
  - DiversityMetricsの実装
  - Shannon Entropyによる行動分散度計算
  - 経路多様性の定量化
  - 意思決定の異質性評価
  - DiversityExperimentの実行
  
- [ ] **多様性検証実験**
  - 同一シナリオでのペルソナ別比較
  - ペルソナと行動の相関分析
  - 意思決定分布の可視化
  - 創発パターンの分析
  
- [ ] 比較実験の実装
  - ① ルールベースABM
  - ② 心理・社会パラメータABM
  - ③ LLM + RAG ABM（多様性あり）
  - 各手法での行動分散度の比較
  
- [ ] 評価指標の自動計算
  - 避難時間分布
  - ボトルネック検出
  - 行動分散度（Shannon Entropy）
  - 経路多様性
  - 目標の多様性
  
- [ ] データ収集・可視化
  - 詳細ログシステム（入出力の記録）
  - ヒートマップ生成（目的地分布）
  - 時系列チャート
  - ペルソナ別行動分析

### Phase 5: 論文執筆（12月中旬～下旬）

- [ ] 実験データの整理
- [ ] 結果の分析・考察
- [ ] 論文執筆

---

## 8. 技術スタック

### Python側

```txt
# requirements.txt
# LLMフレームワーク
langchain>=0.1.0
langgraph>=0.0.20
openai>=1.0.0
anthropic>=0.8.0

# ベクトルDB・RAG
chromadb>=0.4.0
faiss-cpu>=1.7.0

# データ処理
numpy>=1.24.0
pandas>=2.0.0
scipy>=1.10.0

# ネットワーク・グラフ
networkx>=3.0

# 通信
websockets>=12.0
grpcio>=1.50.0

# データ検証
pydantic>=2.0.0

# 可視化
matplotlib>=3.7.0
seaborn>=0.12.0

# その他
python-dotenv>=1.0.0
tqdm>=4.65.0
```

### Unity側

```text
- Unity 2021.3 LTS以降
- PLATEAU SDK for Unity
- NavMesh Components
- WebSocket-Sharp（通信用）
```

---

## 9. データ要件

### 9.1 必要なデータ

| データ種類 | ソース | 用途 |
|----------|--------|------|
| 地図データ | PLATEAU, OSM | 道路網、建物配置 |
| 避難所情報 | 自治体 | 避難先、収容人数 |
| 人口統計 | 国勢調査 | ペルソナ生成 |
| リハビリデータ | 病院・施設 | 運動能力設定 |
| 災害シナリオ | 防災研究所 | シミュレーション条件 |
| 実際の避難データ | 訓練記録 | 検証用（あれば） |

### 9.2 地域知識（RAG用）

```python
# 地域知識のベクトルDB構築
local_knowledge = [
    "浪江町役場は津波警報時の避難所には適さない（標高が低い）",
    "高台にある○○小学校が最も安全な避難所",
    "国道6号線は渋滞しやすい",
    "××地区は高齢者が多く、避難に時間がかかる",
    # ...防災研の先生からの情報
]

# ベクトル化してChromaDBに保存
vector_store.add_texts(
    texts=local_knowledge,
    metadatas=[{"type": "local_knowledge", "region": "namie"}]
)
```

---

## 10. 参考論文・フレームワーク（AAMASなど）

### 論文

1. **Jiang et al. (AAMAS)**: Personalized Decision-Making in Multi-Agent Systems
   - ペルソナベースの意思決定モデル
   - Peak fileの活用

2. **Dang et al. (2025)**: Large-Language-Model-Driven Agents for Fire Evacuation
   - CA環境でのLLMエージェント
   - コミュニケーションの効果

3. **Li et al. (2025)**: What Makes LLM Agent Simulations Useful for Policy?
   - 政策担当者との協働設計
   - 実用的なシミュレーション設計

### フレームワーク

1. **LangGraph**: エージェント状態管理
2. **Generative Agents**: メモリーアーキテクチャ（Park et al.）
3. **MESA**: Pythonベースのマルチエージェントシミュレーション

---

## 11. 成功の鍵（修論で工夫すべきポイント）

### 11.1 高自由度な入出力設計の重要性

**従来手法の限界**

- 出力が避難所A/B/Cの離散選択に限定
- 行動の画一化
- 個人差が反映されない

**本研究の解決策（修論の重要な貢献）**

1. **階層的意思決定フレームワーク**
   - 戦略レベル（目標設定）
   - 戦術レベル（計画立案）
   - 実行レベル（即座の行動）
   - 各レベルで自由度を確保

2. **連続的な行動空間**
   - 目的地：連続2D空間（避難所に限定しない）
   - 速度：0-3 m/sの連続値
   - 経路選好：6次元の連続パラメータ空間
   - 行動タイプ：12種類の多様な選択肢

3. **マルチモーダルな入力設計**
   - 自己状態（体力、感情）
   - 環境知覚（視覚、聴覚）
   - 社会的文脈（家族、群衆、権威）
   - 記憶（過去の経験、地域知識）
   - 時間情報（切迫度）

### 11.2 LLMの内面を行動に反映する方法

**問題**

- LLMの出力は曖昧な自然言語
- 物理シミュレーションには数値が必要

**解決策（修論のコア）**

1. **多層的な変換パイプライン**

   ```text
   LLM推論 → 構造化出力 → 感情修正 → ペルソナフィルタ → 物理パラメータ
   ```

2. **構造化出力（Function Calling）**
   - JSONスキーマによる出力の構造化
   - 連続値パラメータの明示的な指定
   - 推論プロセスの可視化

3. **ペルソナの「プールリスト」**
   - 各ペルソナが持つ行動レパートリー
   - LLMは状況に応じてプールから選択
   - Peak fileを参考に実装

4. **感情状態の動的変化**
   - パニックレベル、不安度の時系列変化
   - 他者の影響を受けて伝播

### 11.3 行動分散の実現方法

**多様性の源泉**

1. **ペルソナによる知覚バイアス**
   - 神経症的傾向が高い → 危険を過大評価
   - リスク回避傾向が高い → 安全な選択肢を優先
   - 正常性バイアスが高い → 危険を過小評価

2. **Temperature制御**
   - 誠実性が高い → 低temperature（一貫した行動）
   - 神経症的傾向が高い → 高temperature（不安定な行動）
   - エージェントごとに異なるランダム性

3. **記憶の個別性**
   - RAGによる個人特有の経験検索
   - 地域知識レベルの差

4. **創発的行動の促進**
   - 状況に応じたプロンプト調整
   - マニュアル外の選択肢の提示

### 11.4 ダイバーシティの検証

**定量評価**

- 行動分散度（Shannon Entropy）
- 経路の多様性
- 意思決定の異質性

**定性評価**

- ペルソナごとのケーススタディ
- 「こういう人はこう行動した」という物語性

---

## 12. リスクと対策

| リスク | 影響 | 対策 |
|-------|------|------|
| LLMコストが高い | 実験回数制限 | 小規模実験で最適化→本実験 |
| 推論速度が遅い | リアルタイム困難 | セルオートマトンと分離、バッチ処理 |
| ペルソナの妥当性 | 現実性低下 | 専門家レビュー、実データとの照合 |
| Unity-Python連携 | 技術的複雑さ | 段階的実装、既存ライブラリ活用 |

---

## 13. まとめ

本実装計画は、フィードバックで指摘された「ダイバーシティのマルチエージェント」を核に据え、以下を重視している：

### 13.1 核心的な設計方針

1. **高自由度な入出力設計**
   - 避難所選択に限定しない連続的な行動空間
   - 階層的意思決定（戦略→戦術→実行）
   - マルチモーダルな環境認識（視覚、聴覚、社会的情報）

2. **行動分散の実現メカニズム**
   - ペルソナによる知覚バイアス
   - Temperature制御による確率的サンプリング
   - 創発的行動の促進システム

3. **ペルソナの多様性**
   - 性格特性（Big Five + 避難特化パラメータ）
   - 運動能力（リハビリデータ活用）
   - 認知特性（正常性バイアス、リスク認知）

4. **階層的メモリーシステム**
   - 短期記憶（Working Memory）
   - エピソード記憶（RAG）
   - 意味記憶（地域知識）

5. **内面→行動の変換**（修論で最も工夫すべき点）
   - 構造化出力による曖昧さの解消
   - 感情状態による行動修正
   - ペルソナ制約の適用

6. **既存フレームワークの活用**
   - LangGraph、ChromaDBなど

7. **ハイブリッドシミュレーション**
   - CA（高速計算）+ Unity（可視化）

### 13.2 期待される成果

- **行動の多様性**：同一状況でもペルソナにより異なる経路・意思決定
- **創発的パターン**：自発的な群れ形成、代替経路の発見、情報カスケード
- **定量評価**：Shannon Entropy、経路多様性、意思決定の異質性
- **実用性**：ボトルネック特定、実効的な避難計画の立案

これにより、「現実に近い人間行動を再現し、ボトルネック特定と実効的対策に繋げる」という目的を達成する。
