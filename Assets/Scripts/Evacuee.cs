using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LLM;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 避難者の制御を行うクラス
/// </summary>
public class Evacuee : MonoBehaviour {
    
    public enum EnergyLabel
    {
        High,
        Medium,
        Low
    }

    public enum StressLabel
    {
        Calm,
        Alert,
        Panicked
    }

    [Header("Movement Target")]
    public GameObject Target; // 現在の移動目標
    private NavMeshAgent NavAgent; // NavMeshAgentコンポーネント
    private EnvManager _env; // ShelterEnvManagerの参照
    private bool isEvacuating = false; // 避難処理中のフラグ。当たり判定により発火するため、複数回避難処理が行われるのを防ぐためのフラグ
    private List<string> excludeShelters; //1度避難したタワーのUUIDを格納するリスト
    
    [Header("Movement Settings")]
    public float DefaultSpeed = 5f;
    public float MinSpeed = 1f;
    public float MaxSpeed = 10f;

    [Header("Status")]
    [Range(0f, 1f)]
    public float EnergyLevel = 1.0f;
    public EnergyLabel EnergyState = EnergyLabel.High;
    [Range(0f, 1f)]
    public float StressLevel = 0.0f;
    public StressLabel StressState = StressLabel.Calm;
    [TextArea]
    public string StressReason;
    [Range(0f, 1f)]
    public float StaminaLevel = 1.0f;
    public List<string> InjuryTags = new List<string>();
    [TextArea]
    public string InjuryNotes;

    [Header("Goal Labeling")]
    [SerializeField] private string GoalLabelOverride;

    [Header("Decision Mode")]
    [Tooltip("意思決定モード: LLM=LLMエージェント, RuleBased=ルールベースエージェント")]
    public bool UseLLMDecision = false;
    [Tooltip("ルールベース意思決定を使用する（UseLLMDecision=falseの場合に適用）")]
    public bool UseRuleBasedDecision = false;
    public LLMDecisionClient DecisionClient;
    public bool FallbackToNearest = true;
    private RuleBasedDecisionMaker _ruleBasedDecisionMaker;
    [Tooltip("LLMの再呼び出し間隔（秒）")]
    public float LLMDecisionInterval = 120f;
    [Tooltip("最小リクエスト間隔（秒）- 連続リクエストを防ぐ")]
    public float MinRequestInterval = 5f;
    private CancellationTokenSource _llmCts;
    private bool _isRequestingLLMDecision;
    private float _lastLLMDecisionTime = 0f;
    private float _lastRequestTime = 0f; // 実際にリクエストを送った時刻
    private string _uniqueId; // 各避難者を区別するための一意のID（生成順序の数字）
    private PersonaData _persona; // この避難者のペルソナデータ

    [Header("Action State")]
    public LLM.ActionType CurrentAction = LLM.ActionType.EVACUATE; // 現在の行動タイプ
    private Vector3 _stayPosition; // 待機位置

    [Header("Contact / Family Actions")]
    private FamilyData _familyData;          // この避難者の家族情報
    private string _lastContactTarget;       // 直近で連絡を試みた家族
    private string _lastContactMessage;      // 直近で送信したメール本文
    private int _consecutiveContactCount = 0; // 連続CONTACT回数
    private const int MAX_CONSECUTIVE_CONTACT = 2; // 連続CONTACTの上限

    [Header("Follow Action")]
    private Evacuee _followTarget;           // 追従対象の避難者
    private LLM.ActionType _followTargetLastAction; // 追従対象の前回の行動（行動変更検知用）
    private float _followDistance = 2f;      // 追従距離（メートル）
    private float _followCheckInterval = 1f; // 追従チェック間隔（秒）
    private float _lastFollowCheck = 0f;     // 最後に追従チェックした時刻
    private const float FOLLOW_SEARCH_RADIUS = 30f; // 周辺避難者検索半径（メートル）
    private List<Evacuee> _followers = new List<Evacuee>(); // この避難者を追従しているFOLLOWERのリスト

    [Header("Talk Action")]
    private List<LLM.ConversationLogEntry> _conversationHistory = new List<LLM.ConversationLogEntry>();
    private int _consecutiveTalkCount = 0;
    private const int MAX_CONSECUTIVE_TALK = 2;
    private const int MAX_CONVERSATION_HISTORY = 5;
    private TaskCompletionSource<LLM.ConversationResponse> _conversationResponseTCS;
    private bool _isWaitingForConversationResponse = false;

    [Header("Hierarchical Decision Making")]
    private LLM.LongTermGoalPayload _currentLongTermGoal;    // 現在の長期目標
    private LLM.MidTermPlanPayload _currentMidTermPlan;      // 現在の中期計画
    private float _lastGoalUpdateTime = 0f;                  // 最後に長期目標を更新した時刻
    private float _lastPlanUpdateTime = 0f;                  // 最後に中期計画を更新した時刻

    [Header("Alert / Broadcast State")]
    private bool _hasHeardBroadcast = false;     // 行政無線の放送を聞いたか
    private string _lastBroadcastMessage = "";    // 最後に聞いた放送内容
    private bool _hasReceivedJAlert = false;      // Jアラートを受信したか
    private string _lastJAlertMessage = "";       // 最後に受信したJアラート内容

    [Header("Audio Perception State")]
    private bool _hasHeardRumble = false;         // 地鳴りを聞いたか
    private float _lastRumbleIntensity = 0f;      // 最後に感じた地鳴りの強度
    private bool _hasHeardSiren = false;          // サイレンを聞いたか
    private bool _hasHeardFireTruck = false;      // 消防団の呼びかけを聞いたか
    private string _lastFireTruckMessage = "";    // 最後に聞いた消防団のメッセージ
    
    /// <summary>
    /// スマホを持っているか（ペルソナから取得）
    /// </summary>
    public bool HasSmartphone => _persona != null && _persona.has_smartphone;

    /// <summary>
    /// 避難者のID（TALK行動で使用）
    /// </summary>
    public string EvacueeId => _uniqueId;

    /// <summary>
    /// 避難者のペルソナ名（TALK行動で使用）
    /// </summary>
    public string PersonaName => _persona?.name ?? gameObject.name;

    /// <summary>
    /// 避難者の役割（TALK行動で使用）
    /// </summary>
    public string PersonaRole => _persona?.role ?? "";

    /// <summary>
    /// 現在の目標避難所ID（TALK行動で使用）
    /// </summary>
    public string CurrentTargetShelterId => Target?.name ?? "";


    /// <summary>
    /// 避難者のIDを設定する（EnvManagerから呼び出される）
    /// </summary>
    /// <param name="id">避難者の生成順序（1から始まる）</param>
    public void SetEvacueeId(int id)
    {
        _uniqueId = id.ToString();
        
        // ペルソナデータを読み込んで設定
        _persona = PersonaManager.GetPersona(id);
        if (_persona != null)
        {
            // 速度倍率を適用（NavAgentが初期化されていない場合は後で適用）
            if (NavAgent != null)
            {
                NavAgent.speed = DefaultSpeed * _persona.speed_multiplier;
            }
            Debug.Log($"[Evacuee] {gameObject.name}: ペルソナ設定 - {_persona.name} ({_persona.role}), 速度倍率: {_persona.speed_multiplier}, agent_id: {id}");
        }
        else
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: ペルソナデータが見つかりません (agent_id: {id})");
        }
        
        // 家族データを読み込んで設定
        _familyData = FamilyManager.GetFamily(id);
        if (_familyData != null && _familyData.members.Count > 0)
        {
            Debug.Log($"[Evacuee] {gameObject.name}: 家族情報設定 - {_familyData.members.Count}人の家族");
        }
    }
    
    /// <summary>
    /// 家族データを取得
    /// </summary>
    public FamilyData GetFamilyData()
    {
        return _familyData;
    }
    
    /// <summary>
    /// 行政無線の放送を聞いたことを記録（AlertManagerから呼ばれる）
    /// </summary>
    public void OnHeardBroadcast(string message)
    {
        if (!_hasHeardBroadcast || _lastBroadcastMessage != message)
        {
            _hasHeardBroadcast = true;
            _lastBroadcastMessage = message;
            Debug.Log($"[Evacuee] {gameObject.name}: 行政無線の放送を聞きました - {message}");

            // ルールベースDecisionMakerにも通知
            if (_ruleBasedDecisionMaker != null)
            {
                _ruleBasedDecisionMaker.OnHeardBroadcast();
            }
        }
    }

    /// <summary>
    /// Jアラートを受信したことを記録（AlertManagerから呼ばれる）
    /// </summary>
    public void OnReceivedJAlert(string message)
    {
        if (!_hasReceivedJAlert || _lastJAlertMessage != message)
        {
            _hasReceivedJAlert = true;
            _lastJAlertMessage = message;
            // ログはAlertManager側で一括出力するため、ここでは出力しない

            // ルールベースDecisionMakerにも通知
            if (_ruleBasedDecisionMaker != null)
            {
                _ruleBasedDecisionMaker.OnReceivedJAlert();
            }
        }
    }

    /// <summary>
    /// 消防団の呼びかけを聞いたことを記録（FireTruckAgentから呼ばれる）
    /// </summary>
    public void OnHeardFireTruckAnnounce(string message)
    {
        if (!_hasHeardFireTruck || _lastFireTruckMessage != message)
        {
            _hasHeardFireTruck = true;
            _lastFireTruckMessage = message;
            Debug.Log($"[Evacuee] {gameObject.name}: 消防団の呼びかけを聞きました - {message}");

            // ルールベースDecisionMakerにも通知
            if (_ruleBasedDecisionMaker != null)
            {
                _ruleBasedDecisionMaker.OnHeardFireTruck();
            }
        }
    }

    /// <summary>
    /// 聴覚情報の更新（AudioEventManagerから定期的に呼ばれる、または自身で更新）
    /// </summary>
    public void UpdateAudioPerception()
    {
        var audioManager = FindFirstObjectByType<AudioEventManager>();
        if (audioManager == null) return;

        // 地鳴り
        float rumbleIntensity = audioManager.GetRumbleIntensityAt(transform.position);
        if (rumbleIntensity > 0.3f && !_hasHeardRumble)
        {
            _hasHeardRumble = true;
            _lastRumbleIntensity = rumbleIntensity;
            Debug.Log($"[Evacuee] {gameObject.name}: 地鳴りを感じました（強度: {rumbleIntensity:F2}）");
        }
        else if (rumbleIntensity > _lastRumbleIntensity)
        {
            _lastRumbleIntensity = rumbleIntensity;
        }

        // サイレン
        if (!_hasHeardSiren && audioManager.CanHearSiren(transform.position))
        {
            _hasHeardSiren = true;
            Debug.Log($"[Evacuee] {gameObject.name}: サイレンが聞こえます");
        }
    }

    /// <summary>
    /// 知覚情報をLLMリクエスト用のPayloadとして取得
    /// </summary>
    public LLM.PerceptionStatePayload BuildPerceptionPayload()
    {
        return new LLM.PerceptionStatePayload
        {
            // 聴覚情報
            has_heard_rumble = _hasHeardRumble,
            rumble_intensity = _lastRumbleIntensity,
            has_heard_siren = _hasHeardSiren,
            has_heard_fire_truck = _hasHeardFireTruck,
            last_fire_truck_message = _lastFireTruckMessage,

            // 既存の放送・Jアラート情報
            has_heard_broadcast = _hasHeardBroadcast,
            last_broadcast_message = _lastBroadcastMessage,
            has_received_j_alert = _hasReceivedJAlert,
            last_j_alert_message = _lastJAlertMessage
        };
    }

    /// <summary>
    /// エピソード開始時に放送・Jアラートの状態をリセット
    /// </summary>
    public void ResetAlertState()
    {
        _hasHeardBroadcast = false;
        _lastBroadcastMessage = "";
        _hasReceivedJAlert = false;
        _lastJAlertMessage = "";

        // 聴覚状態もリセット
        _hasHeardRumble = false;
        _lastRumbleIntensity = 0f;
        _hasHeardSiren = false;
        _hasHeardFireTruck = false;
        _lastFireTruckMessage = "";

        // ルールベースDecisionMakerもリセット
        if (_ruleBasedDecisionMaker != null)
        {
            _ruleBasedDecisionMaker.Reset();
        }
    }

    void Awake() {
        // IDはEnvManagerから設定されるため、ここでは初期化しない
        NavAgent = GetComponent<NavMeshAgent>();
        if (NavAgent != null)
        {
            NavAgent.speed = DefaultSpeed;
        }
        excludeShelters = new List<string>();

        _env = GetComponentInParent<EnvManager>();

        // ExperimentConfigから設定を取得
        if (ExperimentConfig.Instance != null)
        {
            UseLLMDecision = ExperimentConfig.ShouldUseLLM();
            UseRuleBasedDecision = !UseLLMDecision;
        }

        if (UseLLMDecision && DecisionClient == null)
        {
            DecisionClient = FindFirstObjectByType<LLMDecisionClient>();
        }

        // ルールベース意思決定の初期化
        if (UseRuleBasedDecision)
        {
            _ruleBasedDecisionMaker = gameObject.AddComponent<RuleBasedDecisionMaker>();
            _ruleBasedDecisionMaker.Initialize(this, _env, NavAgent);
        }

        _env.Agent.OnDidActioned += () => {
            // エージェントが建物を選択したことを検知して最短距離の避難所を探す
            if(this != null && this.gameObject.activeSelf) {
                if (UseLLMDecision && DecisionClient != null)
                {
                    RequestLLMDecision();
                }
                else if (UseRuleBasedDecision && _ruleBasedDecisionMaker != null)
                {
                    RequestRuleBasedDecision();
                }
                else
                {
                    MoveToNearestShelter();
                }
            }
        };
    }

    void Start()
    {
        // NavAgentが初期化された後にペルソナの速度倍率を適用
        if (_persona != null && NavAgent != null)
        {
            NavAgent.speed = DefaultSpeed * _persona.speed_multiplier;
        }
    }

    private void OnDestroy()
    {
        // FOLLOWERから自分を削除
        UnregisterFromFollowTarget();
        
        // 自分を追従しているFOLLOWERに通知
        NotifyFollowersOfTargetLost();
        
        _llmCts?.Cancel();
        _llmCts?.Dispose();
        _llmCts = null;
    }

    void Update()
    {
        // FOLLOW行動の更新処理（位置追跡のみ、行動変更の検知は通知で行う）
        if (CurrentAction == LLM.ActionType.FOLLOW && _followTarget != null)
        {
            UpdateFollowAction();
        }

        // ルールベースモードの定期評価
        if (UseRuleBasedDecision && _ruleBasedDecisionMaker != null && gameObject.activeSelf && _env != null)
        {
            // ルールベースは常に再評価可能（内部で間隔制御）
            RequestRuleBasedDecision();
            return;
        }

        // LLMを使用している場合、定期的に再呼び出しをチェック
        if (!UseLLMDecision || DecisionClient == null || !gameObject.activeSelf || _env == null)
        {
            return;
        }

        // 最後にLLMを呼び出した時刻が記録されている場合（初期呼び出し後）
        if (_lastLLMDecisionTime <= 0f)
        {
            return;
        }

        // シミュレーション内の経過時間を使用
        float currentSimTime = _env.CurrentTimeSec;
        float elapsedSinceLastDecision = currentSimTime - _lastLLMDecisionTime;
        if (elapsedSinceLastDecision >= LLMDecisionInterval)
        {
            // 既にリクエスト中でない場合のみ再呼び出し
            if (!_isRequestingLLMDecision)
            {
                Debug.Log($"[Evacuee] {gameObject.name}: 定期LLM再呼び出し（シミュレーション経過時間: {elapsedSinceLastDecision:F2}秒）");
                RequestLLMDecision();
            }
        }
    }

    /// <summary>
    /// ルールベース意思決定を実行
    /// </summary>
    private void RequestRuleBasedDecision()
    {
        if (_ruleBasedDecisionMaker == null) return;

        var (actionType, targetShelter, reasoning) = _ruleBasedDecisionMaker.MakeDecision();

        // 行動が変更された場合のみ処理
        if (CurrentAction != actionType || (actionType == LLM.ActionType.EVACUATE && Target != targetShelter))
        {
            CurrentAction = actionType;

            // SimulationMetricsに記録
            var metrics = FindFirstObjectByType<SimulationMetrics>();
            if (metrics != null)
            {
                string shelterName = targetShelter != null ? targetShelter.name : "";
                metrics.RecordAction(_uniqueId ?? gameObject.name, actionType, shelterName, reasoning, 1.0f);
            }

            switch (actionType)
            {
                case LLM.ActionType.EVACUATE:
                    if (targetShelter != null)
                    {
                        Target = targetShelter;
                        if (NavAgent != null)
                        {
                            NavAgent.isStopped = false;
                            NavAgent.SetDestination(Target.transform.position);
                        }
                        Debug.Log($"[Evacuee] {gameObject.name}: RuleBased EVACUATE - {targetShelter.name}に向かいます ({reasoning})");
                    }
                    break;

                case LLM.ActionType.STAY:
                    _stayPosition = transform.position;
                    if (NavAgent != null)
                    {
                        NavAgent.isStopped = true;
                        NavAgent.ResetPath();
                    }
                    Debug.Log($"[Evacuee] {gameObject.name}: RuleBased STAY - 待機します ({reasoning})");
                    break;
            }
        }
    }

    
    /// <summary>
    /// タグ名から避難所を検索する。フィールドに存在する全てのタワーを検索し、距離別にソートして返す
    /// </summary>
    /// <param name="excludeTowerUUIDs">除外するタワーのUUID.未指定の場合はnull</param>
    /// <returns>localField内のTowerオブジェクトのリスト</returns>
    private List<GameObject> SearchShelters(List<string> excludeTowerUUIDs = null) {
        // タグ名から避難所を検索する
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Shelter");
        GameObject[] constShelters = GameObject.FindGameObjectsWithTag("ConstShelter");
        List<GameObject> Iterates = new List<GameObject>();
        foreach (var shelter in towers) {
            Iterates.Add(shelter);
        }
        foreach (var shelter in constShelters) {
            Iterates.Add(shelter);
        }
        // 訪れたことのない避難所を探す
        List<GameObject> sortedTowerPoints = new List<GameObject>();
        foreach (var tower in Iterates) {
            if(excludeTowerUUIDs != null && excludeTowerUUIDs.Contains(tower.GetComponent<Shelter>().uuid)) {
                continue;
            }
            GameObject point = tower.transform.GetChild(0).gameObject; // 避難所に設置した目印オブジェクトを取得
            sortedTowerPoints.Add(point);
        }
        // NOTE: エピソード更新時にgameObjectがnullになることがあるので、nullチェックを行う
        if(this != null) {
            // 距離別にソート
            sortedTowerPoints.Sort((a, b) => Vector3.Distance(a.transform.position, transform.position).CompareTo(Vector3.Distance(b.transform.position, transform.position))); 
        }
        return sortedTowerPoints;
    }

    /// <summary>
    /// 避難を行う処理
    /// 避難所のオブジェクトにアタッチされ、当たり判定により呼び出される 
    /// </summary>
    public void Evacuation(Shelter shelter) {
        if(isEvacuating) {
            return;
        }
        isEvacuating = true;
        // キャパシティーがある場合、避難処理を行う
        if(shelter.currentCapacity > 0) {
            shelter.NowAccCount++;
            gameObject.SetActive(false);
        } else { //キャパシティがいっぱいの場合、次の避難所を探す
            excludeShelters.Add(shelter.uuid);
            if (UseLLMDecision && DecisionClient != null)
            {
                RequestLLMDecision();
            }
            else
            {
                List<GameObject> shelters = SearchShelters(excludeShelters);
                if(shelters.Count > 0) {
                    Target = shelters[0]; //最短距離のタワーを目標に設定
                    NavAgent.SetDestination(Target.transform.position);
                    ResetMovementSpeed();
                }
            }
        }
        isEvacuating = false;
    }

    private void RequestLLMDecision()
    {
        // 既にリクエスト中なら拒否
        if (_isRequestingLLMDecision)
        {
            return;
        }
        
        // ★ 最小リクエスト間隔のチェック
        float currentTime = _env != null ? _env.CurrentTimeSec : Time.time;
        float elapsedSinceLastRequest = currentTime - _lastRequestTime;
        
        if (_lastRequestTime > 0f && elapsedSinceLastRequest < MinRequestInterval)
        {
            // 間隔が短すぎる場合は拒否
            Debug.Log($"[Evacuee] {gameObject.name}: 最小リクエスト間隔({MinRequestInterval}秒)を満たしていません。"
                      + $"前回から{elapsedSinceLastRequest:F2}秒経過（必要: {MinRequestInterval}秒）。リクエストをスキップします。");
            return;
        }
        
        // リクエストを許可
        _lastRequestTime = currentTime;  // リクエスト時刻を記録
        _ = DecideAndMoveAsync();
    }

    private async Task DecideAndMoveAsync()
    {
        _isRequestingLLMDecision = true;
        _llmCts?.Cancel();
        _llmCts = new CancellationTokenSource();

        try
        {
            var request = BuildEvacDecisionRequest();
            var response = await DecisionClient.RequestEvacueeDecisionAsync(request, _llmCts.Token);
            if (!ApplyLLMDecision(response))
            {
                if (FallbackToNearest)
                {
                    Debug.Log($"[Evacuee] {gameObject.name}: LLM応答なし／無効な応答のため最寄り避難所へフォールバックします。");
                    MoveToNearestShelter();
                }
            }
            // LLMの応答を受けた時刻を記録（成功・失敗に関わらず、シミュレーション内の経過時間を使用）
            _lastLLMDecisionTime = _env != null ? _env.CurrentTimeSec : Time.time;
        }
        catch (OperationCanceledException)
        {
            // タイムアウトなどでキャンセルされた場合も最寄り避難所へフォールバック
            Debug.LogWarning($"[Evacuee] {gameObject.name}: LLM decision canceled (timeout?). Fallback to nearest shelter.");
            if (FallbackToNearest)
            {
                MoveToNearestShelter();
            }
            _lastLLMDecisionTime = _env != null ? _env.CurrentTimeSec : Time.time;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: LLM decision failed - {ex.Message}");
            if (FallbackToNearest)
            {
                MoveToNearestShelter();
            }
            // エラーが発生した場合も時刻を記録（次回の再呼び出しタイミングをリセット、シミュレーション内の経過時間を使用）
            _lastLLMDecisionTime = _env != null ? _env.CurrentTimeSec : Time.time;
        }
        finally
        {
            _isRequestingLLMDecision = false;
        }
    }

    private bool ApplyLLMDecision(LLMEvacDecisionResponse response)
    {
        if (response == null)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: LLMレスポンスがありません");
            return false;
        }

        // action_typeを解析（デフォルトはEVACUATE）
        LLM.ActionType actionType = LLM.ActionType.EVACUATE;
        if (!string.IsNullOrEmpty(response.action_type))
        {
            if (System.Enum.TryParse(response.action_type, true, out LLM.ActionType parsedAction))
            {
                actionType = parsedAction;
            }
            else
            {
                Debug.LogWarning($"[Evacuee] {gameObject.name}: 不明なaction_type '{response.action_type}'、EVACUATEとして処理");
            }
        }

        // 行動が変更されたかどうかを確認
        bool actionChanged = CurrentAction != actionType;
        
        // FOLLOW行動をやめる場合、追従対象から自分を削除
        if (CurrentAction == LLM.ActionType.FOLLOW && actionType != LLM.ActionType.FOLLOW)
        {
            UnregisterFromFollowTarget();
        }
        
        CurrentAction = actionType;

        // CONTACT以外の行動を選択した場合は連続CONTACTカウントをリセット
        if (actionType != LLM.ActionType.CONTACT)
        {
            _consecutiveContactCount = 0;
        }

        // TALK以外の行動を選択した場合は連続TALKカウントをリセット
        if (actionType != LLM.ActionType.TALK)
        {
            _consecutiveTalkCount = 0;
        }

        // 行動タイプに応じた処理
        bool result = false;
        switch (actionType)
        {
            case LLM.ActionType.STAY:
                result = ExecuteStayAction(response);
                break;

            case LLM.ActionType.EVACUATE:
                result = ExecuteEvacuateAction(response);
                break;

            case LLM.ActionType.SEARCH_FAMILY:
                result = ExecuteSearchFamilyAction(response);
                break;

            case LLM.ActionType.CONTACT:
                result = ExecuteContactAction(response);
                break;

            case LLM.ActionType.FOLLOW:
                result = ExecuteFollowAction(response);
                break;

            case LLM.ActionType.TALK:
                result = ExecuteTalkAction(response);
                break;

            default:
                Debug.LogWarning($"[Evacuee] {gameObject.name}: 未知の行動タイプ {actionType}");
                return false;
        }

        // 長期目標の更新
        if (response.should_update_goal && response.long_term_goal != null)
        {
            _currentLongTermGoal = response.long_term_goal;
            _lastGoalUpdateTime = _env != null ? _env.CurrentTimeSec : Time.time;
            Debug.Log($"[Evacuee] {gameObject.name}: 長期目標を更新 - Primary Goal: {_currentLongTermGoal.primary_goal}");
        }

        // 中期計画の更新
        if (response.should_update_plan && response.mid_term_plan != null)
        {
            _currentMidTermPlan = response.mid_term_plan;
            _lastPlanUpdateTime = _env != null ? _env.CurrentTimeSec : Time.time;
            if (_currentMidTermPlan.steps != null && _currentMidTermPlan.steps.Length > 0)
            {
                Debug.Log($"[Evacuee] {gameObject.name}: 中期計画を更新 - Steps: {string.Join(", ", _currentMidTermPlan.steps)}");
            }
        }

        // 行動が変更された場合、FOLLOWERに通知
        if (actionChanged && result)
        {
            NotifyFollowersOfActionChange();
        }

        return result;
    }

    /// <summary>
    /// STAY行動を実行（その場で待機）
    /// </summary>
    private bool ExecuteStayAction(LLMEvacDecisionResponse response)
    {
        _stayPosition = transform.position;
        
        // NavMeshAgentを停止
        if (NavAgent != null)
        {
            NavAgent.isStopped = true;
            NavAgent.ResetPath();
        }
        
        Debug.Log($"[Evacuee] {gameObject.name}: STAY行動を選択 - その場で待機します " +
                 $"(reason='{response.reasoning}', confidence={response.confidence:F2})");
        
        return true;
    }

    /// <summary>
    /// SEARCH_FAMILY 行動を実行（特定の家族の位置を目標に移動）
    /// </summary>
    private bool ExecuteSearchFamilyAction(LLMEvacDecisionResponse response)
    {
        if (_familyData == null || _familyData.members == null || _familyData.members.Count == 0)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 家族情報がないためSEARCH_FAMILYを実行できません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        string targetKey = response.target_family_member;
        FamilyMember target = null;

        if (!string.IsNullOrEmpty(targetKey))
        {
            // まず名前で一致を試みる
            target = _familyData.members.FirstOrDefault(m => m.name == targetKey);
            // 見つからなければ続柄（「息子」「娘」「妻」など）で一致を試みる
            if (target == null)
            {
                target = _familyData.members.FirstOrDefault(m => m.relation == targetKey);
            }
        }

        // LLMがtarget_family_memberを指定しなかった、または見つからなかった場合は
        // 簡単なヒューリスティックでターゲットを選ぶ（子供優先 → 配偶者 → その他）
        if (target == null)
        {
            target = _familyData.members
                .FirstOrDefault(m => m.relation.Contains("子") || m.relation.Contains("息子") || m.relation.Contains("娘"))
                ?? _familyData.members.FirstOrDefault(m => m.relation.Contains("妻") || m.relation.Contains("夫"))
                ?? _familyData.members.First();
        }

        // 目標位置を決定（search_position があればそれを優先、なければspawn_positionを使用）
        Vector3 destination = target.search_position != Vector3.zero
            ? target.search_position
            : target.spawn_position;

        if (destination == Vector3.zero)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 家族 {target.name} の探索位置が未設定のためSEARCH_FAMILYを実行できません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // NavMeshAgentで探索地点へ移動
        if (NavAgent != null)
        {
            NavAgent.isStopped = false;
            NavAgent.SetDestination(destination);
        }

        Debug.Log($"[Evacuee] {gameObject.name}: SEARCH_FAMILY行動を選択 - {target.relation} {target.name} を探しに {destination} に向かいます "
                  + $"(reason='{response.reasoning}', confidence={response.confidence:F2})");

        return true;
    }

    /// <summary>
    /// CONTACT 行動を実行（家族にメールで連絡）
    /// TALKと同様に、家族がシーン内に存在する場合はその避難者のLLMを呼び出す
    /// 存在しない場合はサーバーに直接家族としての応答を要求する
    /// </summary>
    private bool ExecuteContactAction(LLMEvacDecisionResponse response)
    {
        if (_familyData == null || _familyData.members == null || _familyData.members.Count == 0)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 家族情報がないためCONTACTを実行できません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // ★ 無限ループ防止: 連続CONTACT制限
        _consecutiveContactCount++;
        if (_consecutiveContactCount >= MAX_CONSECUTIVE_CONTACT)
        {
            // 強制的にEVACUATEに切り替え
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 連続CONTACTが上限({MAX_CONSECUTIVE_CONTACT}回)に達しました。EVACUATEに切り替えます。");
            _consecutiveContactCount = 0;
            return ExecuteEvacuateAction(response);
        }

        string targetKey = response.contact_target;
        FamilyMember target = null;

        if (!string.IsNullOrEmpty(targetKey))
        {
            // まず名前で一致を試みる
            target = _familyData.members.FirstOrDefault(m => m.name == targetKey);
            // 見つからなければ続柄で一致を試みる
            if (target == null)
            {
                target = _familyData.members.FirstOrDefault(m => m.relation == targetKey);
            }
        }

        // それでも見つからなければ、「連絡手段ありの家族」を優先して一人選ぶ
        if (target == null)
        {
            target = _familyData.members
                .Where(m => m.has_phone)
                .FirstOrDefault() ?? _familyData.members.First();
        }

        _lastContactTarget = target.name;

        // 連絡中は一旦立ち止まるイメージにする
        if (NavAgent != null)
        {
            NavAgent.isStopped = true;
            NavAgent.ResetPath();
        }

        Debug.Log($"[Evacuee] {gameObject.name}: CONTACT行動を選択 - {target.relation} {target.name} に連絡します " +
                  $"(reason='{response.reasoning}', confidence={response.confidence:F2})");

        // 非同期で連絡を開始（家族のLLMを呼び出す）
        _ = InitiateFamilyContactAsync(target, response);

        return true;
    }

    /// <summary>
    /// 家族への連絡を開始し、相手からの返信を待機（非同期）
    /// </summary>
    private async Task InitiateFamilyContactAsync(FamilyMember target, LLMEvacDecisionResponse response)
    {
        try
        {
            // 連絡リクエストを作成
            var contactRequest = new LLM.FamilyContactRequest
            {
                sender_id = EvacueeId,
                sender_name = PersonaName,
                sender_relation = GetMyRelationTo(target), // 相手から見た自分の続柄
                message = $"大丈夫？今どこにいる？避難できそう？",
                sender_action = CurrentAction.ToString(),
                sender_target = CurrentTargetShelterId,
                sender_location = GetCurrentLocationDescription()
            };

            LLM.FamilyContactResponse contactResponse;

            // 家族がシーン内に存在するかチェック
            if (target.exists_in_scene && target.agent_id > 0)
            {
                // シーン内の避難者を検索してそのLLMを呼び出す
                Evacuee familyEvacuee = FindEvacueeById(target.agent_id.ToString());
                if (familyEvacuee != null)
                {
                    Debug.Log($"[Evacuee] {gameObject.name}: 家族 {target.name} (agent_id: {target.agent_id}) がシーン内に存在 - 双方向通信開始");

                    // 家族エージェントに連絡を送信し、応答を待機
                    _conversationResponseTCS = new TaskCompletionSource<LLM.ConversationResponse>();
                    familyEvacuee.OnFamilyContactReceived(contactRequest, this);

                    // タイムアウト付きで応答を待機（10秒）
                    var timeoutTask = Task.Delay(10000);
                    var completedTask = await Task.WhenAny(_conversationResponseTCS.Task, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        Debug.LogWarning($"[Evacuee] {gameObject.name}: 家族連絡応答タイムアウト - デフォルト応答を使用");
                        contactResponse = new LLM.FamilyContactResponse
                        {
                            responder_id = target.agent_id.ToString(),
                            responder_name = target.name,
                            response_message = "ごめん、今バタバタしてて。無事だから心配しないで。",
                            current_status = "無事",
                            current_location = "不明",
                            planned_action = ""
                        };
                    }
                    else
                    {
                        // ConversationResponseをFamilyContactResponseに変換
                        var convResponse = _conversationResponseTCS.Task.Result;
                        contactResponse = new LLM.FamilyContactResponse
                        {
                            responder_id = convResponse.responder_id,
                            responder_name = convResponse.responder_name,
                            response_message = convResponse.response_message,
                            current_status = "不明",
                            current_location = "",
                            planned_action = ""
                        };
                    }
                }
                else
                {
                    Debug.LogWarning($"[Evacuee] {gameObject.name}: 家族 {target.name} (agent_id: {target.agent_id}) がシーン内に見つかりません - サーバー経由で応答生成");
                    contactResponse = await RequestFamilyContactFromServerAsync(contactRequest, target);
                }
            }
            else
            {
                // シーン内に存在しない場合はサーバーに直接リクエスト
                Debug.Log($"[Evacuee] {gameObject.name}: 家族 {target.name} はシーン外 - サーバー経由で応答生成");
                contactResponse = await RequestFamilyContactFromServerAsync(contactRequest, target);
            }

            // 返信内容を保存
            _lastContactMessage = contactResponse.response_message;

            Debug.Log($"[Evacuee] {gameObject.name}: 家族 {target.name} からの返信: 「{contactResponse.response_message}」\n" +
                      $"  状況: {contactResponse.current_status}, 場所: {contactResponse.current_location}, 予定: {contactResponse.planned_action}");

            // 次の意思決定のためにLLMを呼び出し
            if (UseLLMDecision && DecisionClient != null)
            {
                RequestLLMDecision();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Evacuee] {gameObject.name}: 家族連絡中にエラー: {ex.Message}");

            // エラー時もデフォルトの返信を設定して続行
            _lastContactMessage = $"{target.name}からの返信: 「連絡が取れませんでした...」";

            if (UseLLMDecision && DecisionClient != null)
            {
                RequestLLMDecision();
            }
        }
        finally
        {
            _conversationResponseTCS = null;
        }
    }

    /// <summary>
    /// サーバーに家族としての応答を生成させる（シーン外の家族用）
    /// </summary>
    private async Task<LLM.FamilyContactResponse> RequestFamilyContactFromServerAsync(LLM.FamilyContactRequest contactRequest, FamilyMember target)
    {
        if (DecisionClient == null)
        {
            return new LLM.FamilyContactResponse
            {
                responder_id = target.agent_id.ToString(),
                responder_name = target.name,
                response_message = "無事だよ。今は自宅付近にいる。様子を見ている。",
                current_status = "無事",
                current_location = "自宅付近",
                planned_action = "様子を見て避難する"
            };
        }

        // LLMリクエストを構築
        var llmRequest = new LLM.LLMFamilyContactResponseRequest
        {
            request_id = $"fam-{Guid.NewGuid():N}",
            request_type = "family_contact_response",
            persona = BuildFamilyPersonaPayload(target),
            environment = BuildEnvironmentPayload(),
            incoming_contact = contactRequest,
            current_action = "STAY", // シーン外の家族はデフォルトでSTAY
            current_target = "",
            family_relationship = $"{PersonaName}の{target.relation}"
        };

        var llmResponse = await DecisionClient.RequestFamilyContactResponseAsync(llmRequest);

        return new LLM.FamilyContactResponse
        {
            responder_id = target.agent_id.ToString(),
            responder_name = target.name,
            response_message = llmResponse.response_message,
            current_status = llmResponse.current_status,
            current_location = llmResponse.current_location,
            planned_action = llmResponse.planned_action
        };
    }

    /// <summary>
    /// 家族メンバーからLLM用のPersonaPayloadを構築
    /// </summary>
    private LLM.PersonaPayload BuildFamilyPersonaPayload(FamilyMember target)
    {
        return new LLM.PersonaPayload
        {
            agent_id = target.agent_id,
            name = target.name,
            role = target.relation, // 続柄を役割として使用
            age_group = InferAgeGroupFromRelation(target.relation),
            speed_multiplier = 1.0f,
            stairs_usage = "normal",
            mental_state = "普通",
            priority = "安全確保",
            system_prompt_context = $"{PersonaName}の{target.relation}です。"
        };
    }

    /// <summary>
    /// 続柄から年齢層を推定
    /// </summary>
    private string InferAgeGroupFromRelation(string relation)
    {
        if (string.IsNullOrEmpty(relation)) return "成人";

        if (relation.Contains("息子") || relation.Contains("娘") || relation.Contains("子"))
        {
            return "子ども";
        }
        if (relation.Contains("父") || relation.Contains("母") || relation.Contains("祖父") || relation.Contains("祖母"))
        {
            return "高齢者";
        }
        return "成人";
    }

    /// <summary>
    /// 現在位置の説明を取得
    /// </summary>
    private string GetCurrentLocationDescription()
    {
        // TODO: 周辺の建物情報などを使って詳細な位置説明を生成
        if (Target != null)
        {
            return $"{Target.name}付近";
        }
        return "移動中";
    }

    /// <summary>
    /// 相手から見た自分の続柄を取得
    /// </summary>
    private string GetMyRelationTo(FamilyMember target)
    {
        // 相手の続柄から自分の続柄を推定
        string theirRelation = target.relation;
        if (theirRelation == "妻") return "夫";
        if (theirRelation == "夫") return "妻";
        if (theirRelation == "息子" || theirRelation == "娘") return "親";
        if (theirRelation == "父" || theirRelation == "母") return "子";
        if (theirRelation == "兄" || theirRelation == "弟") return "兄弟";
        if (theirRelation == "姉" || theirRelation == "妹") return "姉妹";
        return "家族";
    }

    /// <summary>
    /// 環境情報ペイロードを構築（会話/連絡応答用）
    /// </summary>
    private LLM.EnvironmentPayload BuildEnvironmentPayload()
    {
        return new LLM.EnvironmentPayload
        {
            scenario_id = "shindo_7_tsunami", // TODO: 実際のシナリオIDを取得
            has_heard_broadcast = _hasHeardBroadcast,
            last_broadcast_message = _lastBroadcastMessage,
            has_received_j_alert = _hasReceivedJAlert,
            last_j_alert_message = _lastJAlertMessage
        };
    }

    /// <summary>
    /// 他のエージェントから家族連絡を受信（シーン内の家族として）
    /// </summary>
    public void OnFamilyContactReceived(LLM.FamilyContactRequest request, Evacuee sender)
    {
        Debug.Log($"[Evacuee] {gameObject.name}: 家族 {request.sender_name} からメール受信: 「{request.message}」");

        // LLMに応答生成をリクエスト
        _ = RequestFamilyContactResponseAsync(request, sender);
    }

    /// <summary>
    /// LLMに家族連絡への応答を生成させ、送信者に返す
    /// </summary>
    private async Task RequestFamilyContactResponseAsync(LLM.FamilyContactRequest request, Evacuee sender)
    {
        try
        {
            LLM.ConversationResponse response;

            if (UseLLMDecision && DecisionClient != null)
            {
                // LLMに応答生成をリクエスト
                var llmRequest = new LLM.LLMFamilyContactResponseRequest
                {
                    request_id = $"fam-{Guid.NewGuid():N}",
                    request_type = "family_contact_response",
                    persona = new LLM.PersonaPayload
                    {
                        agent_id = int.TryParse(_uniqueId, out var id) ? id : 0,
                        name = PersonaName,
                        role = _persona?.role ?? "",
                        age_group = _persona?.age_group ?? "成人",
                        speed_multiplier = _persona?.speed_multiplier ?? 1.0f,
                        stairs_usage = _persona?.stairs_usage ?? "normal",
                        mental_state = _persona?.mental_state ?? "普通",
                        priority = _persona?.priority ?? "安全確保",
                        system_prompt_context = _persona?.system_prompt_context ?? ""
                    },
                    environment = BuildEnvironmentPayload(),
                    incoming_contact = request,
                    current_action = CurrentAction.ToString(),
                    current_target = CurrentTargetShelterId,
                    family_relationship = $"{request.sender_name}の{request.sender_relation}の家族"
                };

                var llmResponse = await DecisionClient.RequestFamilyContactResponseAsync(llmRequest);

                response = new LLM.ConversationResponse
                {
                    responder_id = EvacueeId,
                    responder_name = PersonaName,
                    response_message = llmResponse.response_message,
                    willing_to_share = true
                };
            }
            else
            {
                // LLMが利用できない場合はデフォルト応答
                response = new LLM.ConversationResponse
                {
                    responder_id = EvacueeId,
                    responder_name = PersonaName,
                    response_message = "無事だよ。今避難中。そっちも気をつけて。",
                    willing_to_share = true
                };
            }

            // 送信者に応答を返す
            sender.OnFamilyContactResponseReceived(response);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Evacuee] {gameObject.name}: 家族連絡応答生成中にエラー: {ex.Message}");

            // エラー時もデフォルト応答を返す
            sender.OnFamilyContactResponseReceived(new LLM.ConversationResponse
            {
                responder_id = EvacueeId,
                responder_name = PersonaName,
                response_message = "ごめん、今バタバタしてる。無事だから。",
                willing_to_share = true
            });
        }
    }

    /// <summary>
    /// 家族からの応答を受信（連絡した側）
    /// </summary>
    public void OnFamilyContactResponseReceived(LLM.ConversationResponse response)
    {
        _conversationResponseTCS?.TrySetResult(response);
    }

    /// <summary>
    /// FOLLOW 行動を実行（周囲の避難者について行く）
    /// </summary>
    private bool ExecuteFollowAction(LLMEvacDecisionResponse response)
    {
        if (string.IsNullOrEmpty(response.target_evacuee_id))
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: FOLLOW行動においてtarget_evacuee_idが確認できません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // 対象の避難者を検索
        _followTarget = null;
        
        // まず、EnvManagerのEvacueesリストから検索
        if (_env != null && _env.Evacuees != null)
        {
            foreach (var evacueeObj in _env.Evacuees)
            {
                if (evacueeObj == null || evacueeObj == this.gameObject) continue;
                
                var evacuee = evacueeObj.GetComponent<Evacuee>();
                if (evacuee != null)
                {
                    // uniqueIdまたはGameObject名で一致を確認
                    string evacueeId = !string.IsNullOrEmpty(evacuee._uniqueId) ? evacuee._uniqueId : evacueeObj.name;
                    if (evacueeId == response.target_evacuee_id || evacueeObj.name == response.target_evacuee_id)
                    {
                        _followTarget = evacuee;
                        break;
                    }
                }
            }
        }

        // 見つからなければGameObject.Findで検索（フォールバック）
        if (_followTarget == null)
        {
            GameObject targetObj = GameObject.Find(response.target_evacuee_id);
            if (targetObj != null)
            {
                _followTarget = targetObj.GetComponent<Evacuee>();
            }
        }

        if (_followTarget == null)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 追従対象の避難者 '{response.target_evacuee_id}' が見つかりません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // 追従開始：追従対象に自分をFOLLOWERとして登録
        _followTarget.RegisterFollower(this);
        
        // 追従開始
        _lastFollowCheck = Time.time;
        _followTargetLastAction = _followTarget.CurrentAction; // 相手の現在の行動を記録
        
        // 相手の行動に合わせた初期処理
        if (_followTargetLastAction == LLM.ActionType.EVACUATE)
        {
            // 相手がEVACUATEを選択していた場合 → 同じ避難所を目指す
            if (_followTarget.Target != null)
            {
                Target = _followTarget.Target;
                if (NavAgent != null)
                {
                    NavAgent.isStopped = false;
                    NavAgent.SetDestination(Target.transform.position);
                }
                Debug.Log($"[Evacuee] {gameObject.name}: FOLLOW行動を選択 - {response.target_evacuee_id}について行きます（相手は避難所に向かっているため、同じ避難所を目指します）");
            }
            else
            {
                // 相手の目標が設定されていない場合は位置を追従
                if (NavAgent != null)
                {
                    NavAgent.isStopped = false;
                    NavAgent.SetDestination(_followTarget.transform.position);
                }
                Debug.Log($"[Evacuee] {gameObject.name}: FOLLOW行動を選択 - {response.target_evacuee_id}について行きます（相手の位置を追従）");
            }
        }
        else if (_followTargetLastAction == LLM.ActionType.STAY)
        {
            // 相手がSTAYだった場合 → 同じようにSTAYする
            CurrentAction = LLM.ActionType.STAY;
            _stayPosition = transform.position;
            if (NavAgent != null)
            {
                NavAgent.isStopped = true;
                NavAgent.ResetPath();
            }
            Debug.Log($"[Evacuee] {gameObject.name}: FOLLOW行動を選択 - {response.target_evacuee_id}について行きます（相手は待機中なので、同じく待機します）");
        }
        else
        {
            // その他の行動（SEARCH_FAMILY, CONTACT, FOLLOW）の場合は位置を追従
            if (NavAgent != null)
            {
                NavAgent.isStopped = false;
                NavAgent.SetDestination(_followTarget.transform.position);
            }
            Debug.Log($"[Evacuee] {gameObject.name}: FOLLOW行動を選択 - {response.target_evacuee_id}について行きます " +
                      $"(reason='{response.reasoning}', confidence={response.confidence:F2})");
        }

        return true;
    }

    /// <summary>
    /// TALK 行動を実行（周辺の避難者と会話）
    /// </summary>
    private bool ExecuteTalkAction(LLMEvacDecisionResponse response)
    {
        if (string.IsNullOrEmpty(response.talk_target_id))
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: TALK行動においてtalk_target_idが確認できません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // ★ 無限ループ防止: 連続TALK制限
        _consecutiveTalkCount++;
        if (_consecutiveTalkCount >= MAX_CONSECUTIVE_TALK)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 連続TALKが上限({MAX_CONSECUTIVE_TALK}回)に達しました。EVACUATEに切り替えます。");
            _consecutiveTalkCount = 0;
            return ExecuteEvacuateAction(response);
        }

        // 対象の避難者を検索
        Evacuee talkTarget = FindEvacueeById(response.talk_target_id);
        if (talkTarget == null)
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: TALK対象 {response.talk_target_id} が見つかりません。EVACUATEにフォールバックします。");
            return ExecuteEvacuateAction(response);
        }

        // 会話中は立ち止まる
        if (NavAgent != null)
        {
            NavAgent.isStopped = true;
            NavAgent.ResetPath();
        }

        Debug.Log($"[Evacuee] {gameObject.name}: TALK行動を選択 - {talkTarget.PersonaName}に話しかけます " +
                  $"(topic='{response.talk_topic}', message='{response.talk_message}', reason='{response.reasoning}')");

        // 非同期で会話を開始
        _ = InitiateConversationAsync(talkTarget, response.talk_topic, response.talk_message);

        return true;
    }

    /// <summary>
    /// 会話を開始し、相手からの応答を待機（非同期）
    /// </summary>
    private async Task InitiateConversationAsync(Evacuee target, string topic, string message)
    {
        try
        {
            _isWaitingForConversationResponse = true;
            _conversationResponseTCS = new TaskCompletionSource<LLM.ConversationResponse>();

            // 相手に会話リクエストを送信
            var request = new LLM.ConversationRequest
            {
                initiator_id = EvacueeId,
                initiator_name = PersonaName,
                topic = topic ?? "General",
                message = message,
                initiator_action = CurrentAction.ToString(),
                initiator_target = CurrentTargetShelterId
            };

            target.OnConversationReceived(request);

            // タイムアウト付きで応答を待機（5秒）
            var timeoutTask = Task.Delay(5000);
            var completedTask = await Task.WhenAny(_conversationResponseTCS.Task, timeoutTask);

            LLM.ConversationResponse response;
            if (completedTask == timeoutTask)
            {
                // タイムアウト - デフォルト応答を使用
                Debug.LogWarning($"[Evacuee] {gameObject.name}: 会話応答タイムアウト - デフォルト応答を使用");
                response = new LLM.ConversationResponse
                {
                    responder_id = target.EvacueeId,
                    responder_name = target.PersonaName,
                    response_message = "すみません、今は急いでいるので...",
                    willing_to_share = false
                };
            }
            else
            {
                response = _conversationResponseTCS.Task.Result;
            }

            // 会話履歴に追加
            AddConversationToHistory(target.EvacueeId, target.PersonaName, topic, message, response.response_message, true);

            Debug.Log($"[Evacuee] {gameObject.name}: 会話完了 - {target.PersonaName}の返答: 「{response.response_message}」");

            // 次の意思決定のためにLLMを呼び出し
            if (UseLLMDecision && DecisionClient != null)
            {
                RequestLLMDecision();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Evacuee] {gameObject.name}: 会話中にエラー: {ex.Message}");
        }
        finally
        {
            _isWaitingForConversationResponse = false;
            _conversationResponseTCS = null;
        }
    }

    /// <summary>
    /// 他のエージェントから会話リクエストを受信（話しかけられた側）
    /// </summary>
    public void OnConversationReceived(LLM.ConversationRequest request)
    {
        Debug.Log($"[Evacuee] {gameObject.name}: {request.initiator_name}から話しかけられました: 「{request.message}」");

        // LLMに応答生成をリクエスト
        _ = RequestConversationResponseAsync(request);
    }

    /// <summary>
    /// LLMに会話応答を生成させ、話しかけてきた相手に返す
    /// </summary>
    private async Task RequestConversationResponseAsync(LLM.ConversationRequest request)
    {
        try
        {
            LLM.ConversationResponse response;

            if (UseLLMDecision && DecisionClient != null)
            {
                // LLMに応答生成をリクエスト
                var llmRequest = new LLM.LLMConversationResponseRequest
                {
                    request_id = $"conv-{Guid.NewGuid()}",
                    request_type = "conversation_response",
                    persona = BuildPersonaPayload(),
                    environment = BuildEnvironmentPayload(),
                    incoming_conversation = request,
                    current_action = CurrentAction.ToString(),
                    current_target = CurrentTargetShelterId
                };

                var llmResponse = await DecisionClient.RequestConversationResponseAsync(llmRequest);

                if (llmResponse != null)
                {
                    response = new LLM.ConversationResponse
                    {
                        responder_id = EvacueeId,
                        responder_name = PersonaName,
                        response_message = llmResponse.response_message,
                        willing_to_share = llmResponse.willing_to_share
                    };
                }
                else
                {
                    // LLMが失敗した場合はデフォルト応答
                    response = GenerateDefaultConversationResponse(request);
                }
            }
            else
            {
                // LLMが無効な場合はデフォルト応答
                response = GenerateDefaultConversationResponse(request);
            }

            // 話しかけてきたエージェントに応答を送信
            var initiator = FindEvacueeById(request.initiator_id);
            if (initiator != null)
            {
                initiator.OnConversationResponseReceived(response);
            }

            // 自分の会話履歴にも追加（話しかけられた側として）
            AddConversationToHistory(request.initiator_id, request.initiator_name, request.topic,
                                      request.message, response.response_message, false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Evacuee] {gameObject.name}: 会話応答生成中にエラー: {ex.Message}");

            // エラー時はデフォルト応答を送信
            var initiator = FindEvacueeById(request.initiator_id);
            if (initiator != null)
            {
                initiator.OnConversationResponseReceived(GenerateDefaultConversationResponse(request));
            }
        }
    }

    /// <summary>
    /// 相手からの会話応答を受信（話しかける側）
    /// </summary>
    public void OnConversationResponseReceived(LLM.ConversationResponse response)
    {
        if (_conversationResponseTCS != null && !_conversationResponseTCS.Task.IsCompleted)
        {
            _conversationResponseTCS.SetResult(response);
        }
    }

    /// <summary>
    /// デフォルトの会話応答を生成
    /// </summary>
    private LLM.ConversationResponse GenerateDefaultConversationResponse(LLM.ConversationRequest request)
    {
        string responseMessage;
        bool willingToShare = true;

        switch (request.topic)
        {
            case "ShelterInfo":
                if (!string.IsNullOrEmpty(CurrentTargetShelterId))
                {
                    responseMessage = $"私は{CurrentTargetShelterId}に向かっています。そこに行けば安全だと思いますよ。";
                }
                else
                {
                    responseMessage = "すみません、私もまだどこに避難するか決めかねているんです...";
                }
                break;

            case "CurrentSituation":
                responseMessage = "今は避難を急いでいます。お互い気をつけましょう。";
                break;

            case "ActionAdvice":
                responseMessage = "とりあえず高台に向かうのがいいと思います。";
                break;

            case "SafetyConfirm":
                responseMessage = "大丈夫ですよ。一緒に避難しましょう。";
                break;

            default:
                responseMessage = "はい、わかりました。";
                break;
        }

        return new LLM.ConversationResponse
        {
            responder_id = EvacueeId,
            responder_name = PersonaName,
            response_message = responseMessage,
            willing_to_share = willingToShare
        };
    }

    /// <summary>
    /// 会話履歴に追加
    /// </summary>
    private void AddConversationToHistory(string partnerId, string partnerName, string topic,
                                           string myMessage, string partnerResponse, bool iInitiated)
    {
        var entry = new LLM.ConversationLogEntry
        {
            timestamp = _env != null ? _env.CurrentTimeSec : Time.time,
            partner_id = partnerId,
            partner_name = partnerName,
            topic = topic,
            my_message = iInitiated ? myMessage : partnerResponse,
            partner_response = iInitiated ? partnerResponse : myMessage,
            i_initiated = iInitiated
        };

        _conversationHistory.Add(entry);

        // 履歴が上限を超えたら古いものから削除
        while (_conversationHistory.Count > MAX_CONVERSATION_HISTORY)
        {
            _conversationHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// 会話履歴ペイロードを構築
    /// </summary>
    private LLM.ConversationHistoryPayload BuildConversationHistoryPayload()
    {
        if (_conversationHistory == null || _conversationHistory.Count == 0)
        {
            return null;
        }

        // 直近3件を返す
        var recentConversations = _conversationHistory
            .OrderByDescending(c => c.timestamp)
            .Take(3)
            .Reverse()
            .ToArray();

        return new LLM.ConversationHistoryPayload
        {
            recent_conversations = recentConversations,
            total_conversation_count = _conversationHistory.Count
        };
    }

    /// <summary>
    /// ペルソナ情報ペイロードを構築（会話応答用）
    /// </summary>
    private LLM.PersonaPayload BuildPersonaPayload()
    {
        PersonaData persona = _persona;
        if (persona == null && !string.IsNullOrEmpty(_uniqueId))
        {
            if (int.TryParse(_uniqueId, out int agentId))
            {
                persona = PersonaManager.GetPersona(agentId);
            }
        }

        if (persona != null)
        {
            return new LLM.PersonaPayload
            {
                agent_id = persona.agent_id,
                name = persona.name,
                role = persona.role,
                age_group = persona.age_group,
                speed_multiplier = persona.speed_multiplier,
                stairs_usage = persona.stairs_usage,
                mental_state = persona.mental_state,
                priority = persona.priority,
                system_prompt_context = persona.system_prompt_context
            };
        }

        // ペルソナがない場合はデフォルト値を返す
        return new LLM.PersonaPayload
        {
            agent_id = 0,
            name = gameObject.name,
            role = "避難者",
            age_group = "成人",
            speed_multiplier = 1.0f,
            stairs_usage = "allowed",
            mental_state = "平常",
            priority = "自分の安全",
            system_prompt_context = ""
        };
    }

    /// <summary>
    /// IDから避難者を検索
    /// </summary>
    private Evacuee FindEvacueeById(string targetId)
    {
        if (_env == null || _env.Evacuees == null || string.IsNullOrEmpty(targetId))
        {
            return null;
        }

        foreach (var evacueeObj in _env.Evacuees)
        {
            if (evacueeObj == null || !evacueeObj.activeSelf)
            {
                continue;
            }

            var evacuee = evacueeObj.GetComponent<Evacuee>();
            if (evacuee == null)
            {
                continue;
            }

            // IDで一致を確認
            if (evacuee._uniqueId == targetId || evacueeObj.name == targetId)
            {
                return evacuee;
            }
        }

        return null;
    }

    /// <summary>
    /// FOLLOW行動の更新処理（追従対象の位置を追跡）
    /// 注意: 行動変更の検知は追従対象からの通知で行うため、ここでは位置追跡のみ
    /// </summary>
    private void UpdateFollowAction()
    {
        if (_followTarget == null || !_followTarget.gameObject.activeSelf)
        {
            // 追従対象が消えた場合はEVACUATEに切り替え
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 追従対象が消えました。EVACUATEに切り替えます。");
            UnregisterFromFollowTarget();
            CurrentAction = LLM.ActionType.EVACUATE;
            if (UseLLMDecision && DecisionClient != null)
            {
                RequestLLMDecision();
            }
            else
            {
                MoveToNearestShelter();
            }
            return;
        }

        // 相手の現在の行動に応じた位置追跡のみ（行動変更の検知は通知で行う）
        LLM.ActionType currentTargetAction = _followTarget.CurrentAction;

        // 定期的に追従先の位置を更新
        float currentTime = Time.time;
        if (currentTime - _lastFollowCheck >= _followCheckInterval)
        {
            _lastFollowCheck = currentTime;

            // 相手の現在の行動に応じた処理
            if (currentTargetAction == LLM.ActionType.EVACUATE)
            {
                // 相手がEVACUATEの場合 → 同じ避難所を目指す
                if (_followTarget.Target != null)
                {
                    Target = _followTarget.Target;
                    Vector3 targetPos = Target.transform.position;
                    float distance = Vector3.Distance(transform.position, targetPos);

                    if (distance > _followDistance * 2)
                    {
                        if (NavAgent != null && !NavAgent.isStopped)
                        {
                            NavAgent.SetDestination(targetPos);
                        }
                    }
                    else if (distance < _followDistance)
                    {
                        if (NavAgent != null)
                        {
                            NavAgent.isStopped = true;
                        }
                    }
                    else
                    {
                        if (NavAgent != null)
                        {
                            NavAgent.isStopped = false;
                            NavAgent.SetDestination(targetPos);
                        }
                    }
                }
                else
                {
                    // 相手の目標が設定されていない場合は位置を追従
                    Vector3 targetPos = _followTarget.transform.position;
                    if (NavAgent != null && !NavAgent.isStopped)
                    {
                        NavAgent.SetDestination(targetPos);
                    }
                }
            }
            else if (currentTargetAction == LLM.ActionType.STAY)
            {
                // 相手がSTAYの場合 → 同じようにSTAYする
                if (CurrentAction != LLM.ActionType.STAY)
                {
                    CurrentAction = LLM.ActionType.STAY;
                    _stayPosition = transform.position;
                }
                if (NavAgent != null)
                {
                    NavAgent.isStopped = true;
                    NavAgent.ResetPath();
                }
            }
            else
            {
                // その他の行動（SEARCH_FAMILY, CONTACT, FOLLOW）の場合は位置を追従
                Vector3 targetPos = _followTarget.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);

                if (distance > _followDistance * 2)
                {
                    if (NavAgent != null && !NavAgent.isStopped)
                    {
                        NavAgent.SetDestination(targetPos);
                    }
                }
                else if (distance < _followDistance)
                {
                    if (NavAgent != null)
                    {
                        NavAgent.isStopped = true;
                    }
                }
                else
                {
                    if (NavAgent != null)
                    {
                        NavAgent.isStopped = false;
                        NavAgent.SetDestination(targetPos);
                    }
                }
            }
        }

        // 追従対象が避難所に到達したら、自分も同じ避難所に向かう
        if (_followTarget.isEvacuating && _followTarget.Target != null)
        {
            Target = _followTarget.Target;
            UnregisterFromFollowTarget(); // 追従解除
            CurrentAction = LLM.ActionType.EVACUATE;
            Debug.Log($"[Evacuee] {gameObject.name}: 追従対象が避難所に到達しました。同じ避難所に向かいます。");
        }
    }

    /// <summary>
    /// 追従対象に自分をFOLLOWERとして登録
    /// </summary>
    public void RegisterFollower(Evacuee follower)
    {
        if (follower == null) return;
        
        if (!_followers.Contains(follower))
        {
            _followers.Add(follower);
            Debug.Log($"[Evacuee] {gameObject.name}: {follower.gameObject.name}がFOLLOWERとして登録されました（現在{_followers.Count}人）");
        }
    }

    /// <summary>
    /// 追従対象から自分をFOLLOWERとして削除
    /// </summary>
    public void UnregisterFollower(Evacuee follower)
    {
        if (follower == null) return;
        
        if (_followers.Remove(follower))
        {
            Debug.Log($"[Evacuee] {gameObject.name}: {follower.gameObject.name}がFOLLOWERから削除されました（現在{_followers.Count}人）");
        }
    }

    /// <summary>
    /// 追従対象から自分を削除（内部用）
    /// </summary>
    private void UnregisterFromFollowTarget()
    {
        if (_followTarget != null)
        {
            _followTarget.UnregisterFollower(this);
            _followTarget = null;
        }
    }

    /// <summary>
    /// 行動が変更された際にFOLLOWERに通知
    /// </summary>
    private void NotifyFollowersOfActionChange()
    {
        if (_followers == null || _followers.Count == 0) return;

        Debug.Log($"[Evacuee] {gameObject.name}: 行動が変更されました（{CurrentAction}）。FOLLOWER {_followers.Count}人に通知します。");

        // FOLLOWERのリストをコピー（通知中にリストが変更される可能性があるため）
        var followersCopy = new List<Evacuee>(_followers);
        
        foreach (var follower in followersCopy)
        {
            if (follower == null || !follower.gameObject.activeSelf) continue;

            // FOLLOWERの行動変更を検知してLLM呼び出し
            follower.OnFollowTargetActionChanged(this, CurrentAction);
        }
    }

    /// <summary>
    /// 追従対象が消えた際にFOLLOWERに通知
    /// </summary>
    private void NotifyFollowersOfTargetLost()
    {
        if (_followers == null || _followers.Count == 0) return;

        var followersCopy = new List<Evacuee>(_followers);
        _followers.Clear();

        foreach (var follower in followersCopy)
        {
            if (follower == null || !follower.gameObject.activeSelf) continue;
            follower.OnFollowTargetLost();
        }
    }

    /// <summary>
    /// 追従対象の行動が変更された際に呼ばれる（追従対象から通知される）
    /// </summary>
    private void OnFollowTargetActionChanged(Evacuee target, LLM.ActionType newAction)
    {
        if (_followTarget != target) return; // 通知元が自分の追従対象か確認

        Debug.Log($"[Evacuee] {gameObject.name}: 追従対象の行動が変更されました（{_followTargetLastAction} → {newAction}）。LLMに判断を問い合わせます。");
        _followTargetLastAction = newAction;

        if (UseLLMDecision && DecisionClient != null)
        {
            RequestLLMDecision();
        }
        else
        {
            // LLMが無効な場合は自動的に同期
            SyncWithFollowTargetAction(newAction);
        }
    }

    /// <summary>
    /// 追従対象が消えた際に呼ばれる（追従対象から通知される）
    /// </summary>
    private void OnFollowTargetLost()
    {
        Debug.LogWarning($"[Evacuee] {gameObject.name}: 追従対象が消えました。EVACUATEに切り替えます。");
        _followTarget = null;
        CurrentAction = LLM.ActionType.EVACUATE;
        if (UseLLMDecision && DecisionClient != null)
        {
            RequestLLMDecision();
        }
        else
        {
            MoveToNearestShelter();
        }
    }

    /// <summary>
    /// 追従対象の行動に合わせて自身の行動を同期（LLMが無効な場合のフォールバック）
    /// </summary>
    private void SyncWithFollowTargetAction(LLM.ActionType targetAction)
    {
        if (_followTarget == null) return;

        if (targetAction == LLM.ActionType.EVACUATE)
        {
            // 相手がEVACUATE → 同じ避難所を目指す
            if (_followTarget.Target != null)
            {
                Target = _followTarget.Target;
                CurrentAction = LLM.ActionType.EVACUATE;
                if (NavAgent != null)
                {
                    NavAgent.isStopped = false;
                    NavAgent.SetDestination(Target.transform.position);
                }
                Debug.Log($"[Evacuee] {gameObject.name}: 追従対象がEVACUATEに変更したため、同じ避難所を目指します。");
            }
        }
        else if (targetAction == LLM.ActionType.STAY)
        {
            // 相手がSTAY → 同じようにSTAYする
            CurrentAction = LLM.ActionType.STAY;
            _stayPosition = transform.position;
            if (NavAgent != null)
            {
                NavAgent.isStopped = true;
                NavAgent.ResetPath();
            }
            Debug.Log($"[Evacuee] {gameObject.name}: 追従対象がSTAYに変更したため、同じく待機します。");
        }
        // その他の行動（SEARCH_FAMILY, CONTACT, FOLLOW）の場合は位置を追従し続ける
    }

    /// <summary>
    /// EVACUATE行動を実行（避難所に向かう）- 既存のロジック
    /// </summary>
    private bool ExecuteEvacuateAction(LLMEvacDecisionResponse response)
    {
        if (string.IsNullOrEmpty(response.selected_shelter_id))
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: EVACUATE行動においてselected_shelter_idが確認できません。");
            return false;
        }

        GameObject selectedShelter = null;
        
        // まずdisplayNameでマッチング
        foreach (var shelterObj in _env.Shelters)
        {
            if (shelterObj == null) continue;
            
            var shelterComp = shelterObj.GetComponent<Shelter>();
            if (shelterComp != null && !string.IsNullOrEmpty(shelterComp.displayName))
            {
                if (shelterComp.displayName == response.selected_shelter_id)
                {
                    selectedShelter = shelterObj;
                    break;
                }
            }
        }
        
        // displayNameで見つからなければGameObject.nameでフォールバック
        if (selectedShelter == null)
        {
            foreach (var shelterObj in _env.Shelters)
            {
                if (shelterObj != null && shelterObj.name == response.selected_shelter_id)
                {
                    selectedShelter = shelterObj;
                    break;
                }
            }
        }
        
        // それでも見つからなければGameObject.Findで検索
        if (selectedShelter == null)
        {
            selectedShelter = GameObject.Find(response.selected_shelter_id);
        }

        if (selectedShelter == null)
        {
            // 利用可能な避難所の名前リストを作成
            var availableShelters = _env.Shelters
                .Where(s => s != null)
                .Select(s => {
                    var sc = s.GetComponent<Shelter>();
                    return sc != null && !string.IsNullOrEmpty(sc.displayName) 
                        ? sc.displayName 
                        : s.name;
                })
                .ToList();
            
            Debug.LogWarning($"[Evacuee] {gameObject.name}: LLMが選択した避難所が見つかりません: '{response.selected_shelter_id}' " +
                           $"(利用可能な避難所: {string.Join(", ", availableShelters)})");
            return false;
        }

        var point = selectedShelter.transform.childCount > 0 ? selectedShelter.transform.GetChild(0).gameObject : selectedShelter;
        Target = point;
        
        // NavMeshAgentを再開（STAY状態から遷移した場合）
        if (NavAgent != null)
        {
            NavAgent.isStopped = false;
            NavAgent.SetDestination(Target.transform.position);
        }
        
        ApplySpeedFromLLM(response.desired_speed);
        
        // 表示名を取得（bldg_で始まる場合は簡略表示）
        var selectedShelterComp = selectedShelter.GetComponent<Shelter>();
        string displayName = selectedShelter.name;
        
        if (selectedShelterComp != null && !string.IsNullOrEmpty(selectedShelterComp.displayName))
        {
            // displayNameが「bldg_」で始まる場合は簡略表示、それ以外はそのまま
            if (selectedShelterComp.displayName.StartsWith("bldg_"))
            {
                // bldg_の後の最初の8文字を表示（例: bldg_37ad7316）
                displayName = selectedShelterComp.displayName.Length > 13 
                    ? selectedShelterComp.displayName.Substring(0, 13) + "..." 
                    : selectedShelterComp.displayName;
            }
            else
            {
                displayName = selectedShelterComp.displayName;
            }
        }
        
        Debug.Log($"[Evacuee] {gameObject.name}: EVACUATE - {displayName}に向かいます "
              + $"(pos: {Target.transform.position}), "
              + $"reason='{response.reasoning}', confidence={response.confidence:F2}, "
              + $"speed={NavAgent.speed:F2}");
        return true;
    }

    private LLMEvacDecisionRequest BuildEvacDecisionRequest()
    {
        var shelterPayloads = new List<ShelterCandidatePayload>();
        foreach (var shelterObj in _env.Shelters)
        {
            if (shelterObj == null)
            {
                continue;
            }
            var shelter = shelterObj.GetComponent<Shelter>();
            var pointTransform = shelterObj.transform.childCount > 0
                ? shelterObj.transform.GetChild(0)
                : shelterObj.transform;
            var pointPosition = pointTransform.position;

            // 最新のcurrentCapacityを取得（プロパティなので常に最新値が返される）
            int currentCapacity = shelter != null ? shelter.currentCapacity : 0;
            int maxCapacity = shelter != null ? shelter.MaxCapacity : 0;
            
            // 避難所の表示名と説明を取得
            string displayName = shelter != null && !string.IsNullOrEmpty(shelter.displayName) 
                ? shelter.displayName 
                : shelterObj.name;
            string description = shelter != null ? shelter.description : "";
            
            // NavMeshでの経路距離と徒歩時間を計算
            float distanceMeters = 0f;
            float walkingTimeMinutes = 0f;
            
            // パフォーマンス最適化: ShelterのNavMesh位置キャッシュを使用
            Vector3 targetPos = pointPosition;
            try
            {
                Vector3? cachedNavMeshPos = shelter != null ? shelter.GetNavMeshPosition() : null;
                if (cachedNavMeshPos.HasValue)
                {
                    targetPos = cachedNavMeshPos.Value;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Evacuee] {gameObject.name}: GetNavMeshPosition failed for {shelterObj.name}, using default position - {ex.Message}");
            }
            
            CalculateNavMeshDistance(transform.position, targetPos, out distanceMeters, out walkingTimeMinutes);
            
            shelterPayloads.Add(new ShelterCandidatePayload
            {
                id = shelterObj.name,
                display_name = displayName,
                description = description,
                position = new Vector3Payload(pointPosition),
                current_capacity = currentCapacity,
                max_capacity = maxCapacity,
                distance_meters = distanceMeters,
                walking_time_minutes = walkingTimeMinutes
            });
        }

        // ペルソナ情報をペイロードに変換
        PersonaPayload personaPayload = null;
        
        // _personaがnullの場合は、_uniqueIdからエージェントIDを取得して再取得を試みる
        PersonaData persona = _persona;
        if (persona == null && !string.IsNullOrEmpty(_uniqueId))
        {
            if (int.TryParse(_uniqueId, out int agentId))
            {
                persona = PersonaManager.GetPersona(agentId);
                if (persona != null)
                {
                    _persona = persona; // キャッシュしておく
                    Debug.Log($"[Evacuee] {gameObject.name}: ペルソナを再取得しました - {persona.name} (agent_id: {agentId})");
                }
            }
        }
        
        if (persona != null)
        {
            personaPayload = new PersonaPayload
            {
                agent_id = persona.agent_id,
                name = persona.name,
                role = persona.role,
                age_group = persona.age_group,
                speed_multiplier = persona.speed_multiplier,
                stairs_usage = persona.stairs_usage,
                mental_state = persona.mental_state,
                priority = persona.priority,
                system_prompt_context = persona.system_prompt_context
            };
        }
        else
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: ペルソナ情報が見つかりません (_uniqueId: {_uniqueId})");
        }

        // 環境コンテキストを取得
        EnvironmentalContextPayload envPayload = BuildEnvironmentalContextPayload();
        
        if (envPayload != null)
        {
            Debug.Log($"[Evacuee] {gameObject.name}: 環境コンテキストをLLMリクエストに含めます（建物{envPayload.nearby_buildings.Length}件）");
        }
        else
        {
            Debug.LogWarning($"[Evacuee] {gameObject.name}: 環境コンテキストはnullです（LLMリクエストに含まれません）");
        }
        
        // 家族情報を取得
        FamilyMemberPayload[] familyPayloads = BuildFamilyPayload();
        if (familyPayloads != null && familyPayloads.Length > 0)
        {
            Debug.Log($"[Evacuee] {gameObject.name}: 家族情報をLLMリクエストに含めます（{familyPayloads.Length}人）");
        }
        
        // 周辺避難者情報を取得（詳細情報と統計情報）
        int nearbyEvacueesCount = 0;
        float nearbyEvacueesDensity = 0f;
        NearbyEvacueePayload[] nearbyEvacuees = GetNearbyEvacuees(FOLLOW_SEARCH_RADIUS, out nearbyEvacueesCount, out nearbyEvacueesDensity);
        if (nearbyEvacuees != null && nearbyEvacuees.Length > 0)
        {
            Debug.Log($"[Evacuee] {gameObject.name}: 周辺避難者情報をLLMリクエストに含めます（詳細: {nearbyEvacuees.Length}人、総数: {nearbyEvacueesCount}人、混雑度: {nearbyEvacueesDensity:F2}人/100m²）");
        }
        
        return new LLMEvacDecisionRequest
        {
            request_id = $"{_env.currentEpisodeId}-{gameObject.name}-{Guid.NewGuid()}",
            timestamp = Time.time,
            evacuee = new EvacueePayload
            {
                id = !string.IsNullOrEmpty(_uniqueId) ? _uniqueId : gameObject.name, // 生成順序のIDを使用
                position = new Vector3Payload(transform.position)
            },
            shelter_candidates = shelterPayloads.ToArray(),
            self_state = BuildSelfStatePayload(),
            temporal_context = BuildTemporalContextPayload(),
            persona = personaPayload,
            environmental_context = envPayload,
            family_members = familyPayloads,
            nearby_evacuees = nearbyEvacuees,
            nearby_evacuees_count = nearbyEvacueesCount,
            nearby_evacuees_density = nearbyEvacueesDensity,
            last_contact_message = _lastContactMessage,
            scenario_id = _env != null ? _env.GetScenarioId() : "shindo_2",
            has_heard_broadcast = _hasHeardBroadcast,
            last_broadcast_message = _lastBroadcastMessage,
            has_received_j_alert = _hasReceivedJAlert,
            last_j_alert_message = _lastJAlertMessage,
            current_long_term_goal = _currentLongTermGoal,  // 現在の長期目標（更新判断用）
            current_mid_term_plan = _currentMidTermPlan,   // 現在の中期計画（更新判断用）
            conversation_history = BuildConversationHistoryPayload()  // 会話履歴（TALK行動用）
        };
    }

    /// <summary>
    /// 周辺の避難者を検出してペイロードに変換（FOLLOW行動用）
    /// </summary>
    /// <param name="radius">検索半径（メートル）</param>
    /// <param name="totalCount">検出された総人数（出力パラメータ）</param>
    /// <param name="density">混雑度（人数/100m²）（出力パラメータ）</param>
    /// <returns>詳細情報（最大5人、距離順）</returns>
    private NearbyEvacueePayload[] GetNearbyEvacuees(float radius, out int totalCount, out float density)
    {
        totalCount = 0;
        density = 0f;

        if (_env == null || _env.Evacuees == null)
        {
            return new NearbyEvacueePayload[0];
        }

        var nearbyList = new List<NearbyEvacueePayload>();
        Vector3 myPosition = transform.position;

        foreach (var evacueeObj in _env.Evacuees)
        {
            if (evacueeObj == null || evacueeObj == this.gameObject || !evacueeObj.activeSelf)
            {
                continue;
            }

            var evacuee = evacueeObj.GetComponent<Evacuee>();
            if (evacuee == null)
            {
                continue;
            }

            float distance = Vector3.Distance(myPosition, evacueeObj.transform.position);
            if (distance <= radius)
            {
                totalCount++; // 総人数をカウント

                // 目標避難所のIDを取得
                string targetShelterId = null;
                if (evacuee.Target != null)
                {
                    var shelter = evacuee.Target.GetComponentInParent<Shelter>();
                    if (shelter != null && !string.IsNullOrEmpty(shelter.displayName))
                    {
                        targetShelterId = shelter.displayName;
                    }
                    else
                    {
                        targetShelterId = evacuee.Target.name;
                    }
                }

                string evacueeId = !string.IsNullOrEmpty(evacuee._uniqueId) ? evacuee._uniqueId : evacueeObj.name;

                // ペルソナ情報を取得
                string evacueeName = evacuee._persona?.name ?? evacueeObj.name;
                string evacueeRole = evacuee._persona?.role ?? "";
                string evacueeAgeGroup = evacuee._persona?.age_group ?? "";

                nearbyList.Add(new NearbyEvacueePayload
                {
                    id = evacueeId,
                    position = new Vector3Payload(evacueeObj.transform.position),
                    distance_meters = distance,
                    current_action = evacuee.CurrentAction.ToString(),
                    target_shelter_id = targetShelterId,
                    name = evacueeName,
                    role = evacueeRole,
                    age_group = evacueeAgeGroup
                });
            }
        }

        // 混雑度を計算（人数/100m²）
        // 検索範囲の面積 = π * radius²
        float searchArea = Mathf.PI * radius * radius;
        density = totalCount > 0 ? (totalCount / searchArea) * 100f : 0f; // 100m²あたりの人数

        // 距離順にソートして最大5人まで返す（詳細情報）
        return nearbyList
            .OrderBy(e => e.distance_meters)
            .Take(5)
            .ToArray();
    }
    
    /// <summary>
    /// 家族情報をLLMペイロードに変換
    /// </summary>
    private FamilyMemberPayload[] BuildFamilyPayload()
    {
        if (_familyData == null || _familyData.members == null || _familyData.members.Count == 0)
        {
            return new FamilyMemberPayload[0];
        }
        
        var payloads = new List<FamilyMemberPayload>();
        
        foreach (var member in _familyData.members)
        {
            // 距離を計算
            float distance = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(member.search_position.x, 0, member.search_position.z)
            );
            
            payloads.Add(new FamilyMemberPayload
            {
                name = member.name,
                relation = member.relation,
                likely_location = member.likely_location,
                search_position = new Vector3Payload(member.search_position),
                distance_meters = distance,
                has_phone = member.has_phone,
                exists_in_scene = member.exists_in_scene
            });
        }
        
        return payloads.ToArray();
    }
    
    private EnvironmentalContextPayload BuildEnvironmentalContextPayload()
    {
        // EnvironmentalContextProviderを検索
        var envContext = FindFirstObjectByType<EnvironmentalContextProvider>();
        if (envContext == null)
        {
            // プロバイダーが見つからない場合はnullを返す
            Debug.LogWarning($"[Evacuee] {gameObject.name}: EnvironmentalContextProviderが見つかりません。環境情報は含まれません。");
            return null;
        }
        
        
        // 周辺建物を検索
        float searchRadius = 30f;
        var nearbyBuildings = envContext.GetNearbyBuildings(transform.position, searchRadius);
        
        // LLMのコンテキスト長を考慮して、最大10件に制限
        var limitedBuildings = nearbyBuildings.Take(10).ToList();
        
        // ペイロードに変換
        var buildingPayloads = limitedBuildings.Select(b => new BuildingContextPayload
        {
            name = b.name,
            position = new Vector3Payload(b.position),
            distance = b.distance,
            usage = b.usage,
            major_usage = b.majorUsage ?? "",
            height = b.height,
            floors = b.floors,
            structure_type = b.structureType ?? "",
            land_use_type = b.landUseType ?? ""
        }).ToArray();
        
        Debug.Log($"[Evacuee] {gameObject.name}: 環境情報ペイロードを作成しました（建物{buildingPayloads.Length}件、範囲内合計{nearbyBuildings.Count}件）");
        
        return new EnvironmentalContextPayload
        {
            nearby_buildings = buildingPayloads,
            search_radius = searchRadius,
            total_buildings_in_area = nearbyBuildings.Count
        };
    }

    private void MoveToNearestShelter()
    {
        List<GameObject> towers = SearchShelters();
        if(towers.Count > 0) {
            Target = towers[0]; //最短距離のタワーを目標に設定
            Vector3 destination = Target.transform.position;
            NavAgent.SetDestination(destination);
            ResetMovementSpeed();
            Debug.Log($"[Evacuee] {gameObject.name}: 目的地を設定しました - 座標: ({destination.x:F2}, {destination.y:F2}, {destination.z:F2}), 避難所: {Target.transform.parent.name}, speed={NavAgent.speed:F2}");
        }
    }

    private SelfStatePayload BuildSelfStatePayload()
    {
        var velocity = NavAgent != null ? NavAgent.velocity : Vector3.zero;
        return new SelfStatePayload
        {
            position = new Vector3Payload(transform.position),
            velocity = new Vector3Payload(velocity),
            energy_level = Mathf.Clamp01(EnergyLevel),
            energy_label = EnergyState.ToString().ToLowerInvariant(),
            stress_level = Mathf.Clamp01(StressLevel),
            stress_label = StressState.ToString().ToLowerInvariant(),
            stress_reason = string.IsNullOrWhiteSpace(StressReason) ? null : StressReason,
            current_goal = ResolveCurrentGoalLabel(),
            stamina = Mathf.Clamp01(StaminaLevel),
            injuries = InjuryTags?.ToArray() ?? Array.Empty<string>(),
            injury_notes = string.IsNullOrWhiteSpace(InjuryNotes) ? null : InjuryNotes
        };
    }

    private TemporalContextPayload BuildTemporalContextPayload()
    {
        if (_env == null)
        {
            return new TemporalContextPayload
            {
                elapsed_time = Time.time,
                has_time_limit = false,
                time_limit = 0f
            };
        }

        var hasManualLimit = _env.EnableTemporalOverrides && _env.ManualTimeLimitSeconds > 0f;
        return new TemporalContextPayload
        {
            elapsed_time = _env.CurrentTimeSec,
            has_time_limit = hasManualLimit,
            time_limit = hasManualLimit ? _env.ManualTimeLimitSeconds : 0f
        };
    }

    private string ResolveCurrentGoalLabel()
    {
        if (!string.IsNullOrWhiteSpace(GoalLabelOverride))
        {
            return GoalLabelOverride;
        }

        if (Target == null)
        {
            return null;
        }

        var parentName = Target.transform.parent != null ? Target.transform.parent.name : null;
        return parentName ?? Target.name;
    }

    private void ResetMovementSpeed()
    {
        if (NavAgent != null)
        {
            NavAgent.speed = DefaultSpeed;
        }
    }

    private void ApplySpeedFromLLM(float desiredSpeed)
    {
        if (NavAgent == null)
        {
            return;
        }

        if (desiredSpeed > 0f)
        {
            var clamped = Mathf.Clamp(desiredSpeed, MinSpeed, MaxSpeed);
            NavAgent.speed = clamped;
        }
        else
        {
            ResetMovementSpeed();
        }
    }

    private static bool _navMeshWarningShown = false; // 警告を一度だけ表示するためのフラグ
    
    /// <summary>
    /// NavMeshを使用して2点間の経路距離と徒歩所要時間を計算
    /// 注: endは既にNavMesh上の点に補正されていることを想定（Shelter.GetNavMeshPosition()で取得）
    /// </summary>
    /// <param name="start">開始位置</param>
    /// <param name="end">終了位置（NavMesh上の点）</param>
    /// <param name="distanceMeters">経路距離（メートル）</param>
    /// <param name="walkingTimeMinutes">徒歩所要時間（分）</param>
    private void CalculateNavMeshDistance(Vector3 start, Vector3 end, out float distanceMeters, out float walkingTimeMinutes)
    {
        // Googleマップのデフォルト徒歩速度: 4.8km/h = 80m/分
        const float WALKING_SPEED_M_PER_MIN = 80f;
        
        // 開始位置がNavMesh上にあることを確認（避難者は基本的にNavMesh上にいる）
        Vector3 navMeshStart = start;
        NavMeshHit startHit;
        if (NavMesh.SamplePosition(start, out startHit, 5f, NavMesh.AllAreas))
        {
            navMeshStart = startHit.position;
        }
        
        NavMeshPath path = new NavMeshPath();
        
        // 経路を計算（endは既にNavMesh上の点）
        if (NavMesh.CalculatePath(navMeshStart, end, NavMesh.AllAreas, path))
        {
            // 経路が完全か部分的に成功した場合
            if (path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                // 経路の総距離を計算
                distanceMeters = 0f;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    distanceMeters += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                }
                
                // 徒歩所要時間を計算（分）
                walkingTimeMinutes = distanceMeters / WALKING_SPEED_M_PER_MIN;
                return; // 成功したので終了
            }
        }
        
        // フォールバック: 経路計算が失敗した場合は直線距離を使用
        distanceMeters = Vector3.Distance(start, end);
        walkingTimeMinutes = distanceMeters / WALKING_SPEED_M_PER_MIN;
        
        // 警告は初回のみ表示
        if (!_navMeshWarningShown)
        {
            Debug.LogWarning($"[Evacuee] NavMesh経路計算が失敗しました。直線距離を使用します（以降この警告は表示されません）");
            _navMeshWarningShown = true;
        }
    }

}
