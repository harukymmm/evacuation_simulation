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
    private CancellationTokenSource _llmCts;
    private bool _isRequestingLLMDecision;

    void Awake() {
        NavAgent = GetComponent<NavMeshAgent>();    
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

    private void OnDestroy()
    {
        _llmCts?.Cancel();
        _llmCts?.Dispose();
        _llmCts = null;
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
        Debug.Log($"[Evacuee] {gameObject.name}: LLM selected {selectedShelter.name} "
              + $"(pos: {Target.transform.position}), "
              + $"reason='{response.reasoning}', confidence={response.confidence:F2}");
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
            shelterPayloads.Add(new ShelterCandidatePayload
            {
                id = shelterObj.name,
                position = new Vector3Payload(shelterObj.transform.position),
                current_capacity = shelter != null ? shelter.currentCapacity : 0,
                max_capacity = shelter != null ? shelter.MaxCapacity : 0
            });
        }

        return new LLMEvacDecisionRequest
        {
            request_id = $"{_env.currentEpisodeId}-{gameObject.name}-{Guid.NewGuid()}",
            timestamp = Time.time,
            evacuee = new EvacueePayload
            {
                id = gameObject.name,
                position = new Vector3Payload(transform.position)
            },
            shelter_candidates = shelterPayloads.ToArray(),
            self_state = BuildSelfStatePayload(),
            temporal_context = BuildTemporalContextPayload()
        };
    }

    private void MoveToNearestShelter()
    {
        List<GameObject> towers = SearchShelters();
        if(towers.Count > 0) {
            Target = towers[0]; //最短距離のタワーを目標に設定
            Vector3 destination = Target.transform.position;
            NavAgent.SetDestination(destination);
            Debug.Log($"[Evacuee] {gameObject.name}: 目的地を設定しました - 座標: ({destination.x:F2}, {destination.y:F2}, {destination.z:F2}), 避難所: {Target.transform.parent.name}");
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

}
