using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("LLM Decision")]
    public bool UseLLMDecision = false;
    public LLMDecisionClient DecisionClient;
    public bool FallbackToNearest = true;
    [Tooltip("LLMの再呼び出し間隔（秒）")]
    public float LLMDecisionInterval = 120f;
    private CancellationTokenSource _llmCts;
    private bool _isRequestingLLMDecision;
    private float _lastLLMDecisionTime = 0f;
    private string _uniqueId; // 各避難者を区別するための一意のID（生成順序の数字）
    private PersonaData _persona; // この避難者のペルソナデータ

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
        if (UseLLMDecision && DecisionClient == null)
        {
            DecisionClient = FindObjectOfType<LLMDecisionClient>();
        }
        _env.Agent.OnDidActioned += () => {
            // エージェントが建物を選択したことを検知して最短距離の避難所を探す
            if(this != null && this.gameObject.activeSelf) {
                if (UseLLMDecision && DecisionClient != null)
                {
                    RequestLLMDecision();
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
        _llmCts?.Cancel();
        _llmCts?.Dispose();
        _llmCts = null;
    }

    void Update()
    {
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
        if (_isRequestingLLMDecision)
        {
            return;
        }

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
                    MoveToNearestShelter();
                }
            }
            // LLMの応答を受けた時刻を記録（成功・失敗に関わらず、シミュレーション内の経過時間を使用）
            _lastLLMDecisionTime = _env != null ? _env.CurrentTimeSec : Time.time;
        }
        catch (OperationCanceledException)
        {
            // ignore
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
        if (response == null || string.IsNullOrEmpty(response.selected_shelter_id))
        {
            return false;
        }

        GameObject selectedShelter = null;
        foreach (var shelter in _env.Shelters)
        {
            if (shelter != null && shelter.name == response.selected_shelter_id)
            {
                selectedShelter = shelter;
                break;
            }
        }

        if (selectedShelter == null)
        {
            selectedShelter = GameObject.Find(response.selected_shelter_id);
        }

        if (selectedShelter == null)
        {
            return false;
        }

        var point = selectedShelter.transform.childCount > 0 ? selectedShelter.transform.GetChild(0).gameObject : selectedShelter;
        Target = point;
        NavAgent.SetDestination(Target.transform.position);
        ApplySpeedFromLLM(response.desired_speed);
        Debug.Log($"[Evacuee] {gameObject.name}: LLM selected {selectedShelter.name} "
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
            
            // デバッグログ：エピソード開始直後（elapsed_timeが0に近い）の場合にログ出力
            if (_env != null && _env.CurrentTimeSec < 0.1f && shelter != null)
            {
                Debug.Log($"[Evacuee] {gameObject.name}: BuildEvacDecisionRequest - Shelter: {shelterObj.name}, " +
                         $"MaxCapacity: {maxCapacity}, NowAccCount: {shelter.NowAccCount}, CurrentCapacity: {currentCapacity}");
            }
            
            shelterPayloads.Add(new ShelterCandidatePayload
            {
                id = shelterObj.name,
                position = new Vector3Payload(pointPosition),
                current_capacity = currentCapacity,
                max_capacity = maxCapacity
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
            persona = personaPayload
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

}
