using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using LLM;

/// <summary>
/// ルールベースエージェントの意思決定を行うクラス（Level 2）
/// 実験1でLLMエージェントとの比較に使用
///
/// Level 2のルール:
/// - 行動タイプは EVACUATE, STAY, SEARCH_FAMILY, CONTACT
/// - 津波警報（Jアラート/行政無線/消防団）受信 → EVACUATE
/// - 地震のみでは動かない（津波警報時のみ避難）
/// - シーン内に未合流の家族がいる場合 → SEARCH_FAMILY
/// - シーン外の家族に未連絡の場合 → CONTACT（1回限り）
/// </summary>
public class RuleBasedDecisionMaker : MonoBehaviour
{
    [Header("Rule Parameters")]
    [Tooltip("待機から避難に切り替える最大時間（秒）")]
    public float MaxStayDuration = 600f;

    [Tooltip("危険と判定する震度の閾値（現在未使用：津波警報のみで判定）")]
    public int DangerousSeismicIntensity = 6;

    [Tooltip("ルール再評価の間隔（秒）")]
    public float ReevaluationInterval = 10f;

    [Tooltip("家族探索の距離閾値（メートル）")]
    public float SearchFamilyDistanceThreshold = 500f;

    private Evacuee _evacuee;
    private EnvManager _envManager;
    private NavMeshAgent _navAgent;
    private float _stayStartTime = -1f;
    private float _lastEvaluationTime = 0f;
    private bool _hasHeardBroadcast = false;
    private bool _hasReceivedJAlert = false;
    private bool _hasHeardFireTruck = false;

    // Level 2: 家族対応用フィールド
    private FamilyData _familyData;
    private bool _hasContactedFamily = false;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Evacuee evacuee, EnvManager envManager, NavMeshAgent navAgent)
    {
        _evacuee = evacuee;
        _envManager = envManager;
        _navAgent = navAgent;
    }

    /// <summary>
    /// 拡張初期化（Level 2: 家族情報を受け取る）
    /// </summary>
    public void InitializeExtended(FamilyData familyData)
    {
        _familyData = familyData;
    }

    /// <summary>
    /// 放送を聞いたことを記録
    /// </summary>
    public void OnHeardBroadcast()
    {
        _hasHeardBroadcast = true;
    }

    /// <summary>
    /// Jアラートを受信したことを記録
    /// </summary>
    public void OnReceivedJAlert()
    {
        _hasReceivedJAlert = true;
    }

    /// <summary>
    /// 消防団の呼びかけを聞いたことを記録
    /// </summary>
    public void OnHeardFireTruck()
    {
        _hasHeardFireTruck = true;
    }

    /// <summary>
    /// ルールベースで行動を決定（Level 1: 後方互換性のため維持）
    /// </summary>
    /// <returns>決定された行動タイプと目標避難所</returns>
    public (ActionType action, GameObject targetShelter, string reasoning) MakeDecision()
    {
        var (action, shelter, reasoning, _) = MakeDecisionExtended();
        return (action, shelter, reasoning);
    }

    /// <summary>
    /// ルールベースで行動を決定（Level 2: 家族対応を含む）
    /// </summary>
    /// <returns>決定された行動タイプ、目標避難所、理由、家族対象</returns>
    public (ActionType action, GameObject targetShelter, string reasoning, FamilyMember familyTarget) MakeDecisionExtended()
    {
        float currentTime = _envManager != null ? _envManager.CurrentTimeSec : Time.time;

        // 最小評価間隔のチェック
        if (currentTime - _lastEvaluationTime < ReevaluationInterval && _lastEvaluationTime > 0)
        {
            // 前回と同じ行動を継続
            return (_evacuee.CurrentAction, _evacuee.Target, "前回の判断を継続", null);
        }
        _lastEvaluationTime = currentTime;

        // 優先度1: 津波警報時のみ緊急避難（地震だけでは動かない）
        if (_hasReceivedJAlert || _hasHeardBroadcast || _hasHeardFireTruck)
        {
            var (shelter, _) = FindNearestShelter();
            string reason = BuildEvacuateReason();
            return (ActionType.EVACUATE, shelter, reason, null);
        }

        // 優先度2: 家族対応（津波警報がない場合）
        // SEARCH_FAMILY: シーン内の未合流家族を探す
        if (ShouldSearchFamily(out var searchTarget))
        {
            return (ActionType.SEARCH_FAMILY, null,
                $"{searchTarget.relation}（{searchTarget.name}）を探しに行く", searchTarget);
        }

        // CONTACT: シーン外の家族に連絡（1回限り）
        if (ShouldContact(out var contactTarget))
        {
            _hasContactedFamily = true;
            return (ActionType.CONTACT, null,
                $"{contactTarget.relation}（{contactTarget.name}）に連絡を取る", contactTarget);
        }

        // 優先度3: 待機（津波警報なし・家族対応なし）
        if (_stayStartTime < 0)
        {
            _stayStartTime = currentTime;
        }

        float stayDuration = currentTime - _stayStartTime;

        // 待機時間上限を超えた場合のみ避難（安全のため）
        if (stayDuration >= MaxStayDuration)
        {
            var (shelter, _) = FindNearestShelter();
            return (ActionType.EVACUATE, shelter, $"待機時間が{MaxStayDuration}秒を超過したため避難", null);
        }

        // 待機を継続
        return (ActionType.STAY, null, $"津波警報未受信のため様子見（経過{stayDuration:F0}秒）", null);
    }

    /// <summary>
    /// SEARCH_FAMILYを実行すべきか判定
    /// 条件: シーン内に未合流の家族がいて、距離閾値以内
    /// </summary>
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

    /// <summary>
    /// 子供かどうかを判定
    /// </summary>
    private bool IsChild(string relation)
    {
        if (string.IsNullOrEmpty(relation)) return false;
        return relation.Contains("息子") || relation.Contains("娘") ||
               relation.Contains("子供") || relation.Contains("子ども");
    }

    /// <summary>
    /// CONTACTを実行すべきか判定
    /// 条件: シーン外の家族がいて、まだ連絡を取っていない
    /// </summary>
    private bool ShouldContact(out FamilyMember target)
    {
        target = null;
        if (_hasContactedFamily) return false;  // 1回限り
        if (_familyData == null || _familyData.members == null) return false;

        target = _familyData.members
            .Where(m => !m.exists_in_scene && m.has_phone)
            .FirstOrDefault();

        return target != null;
    }

    /// <summary>
    /// 震度を取得（シナリオから推定）
    /// </summary>
    private int GetSeismicIntensity()
    {
        if (_envManager == null) return 2;

        switch (_envManager.Scenario)
        {
            case EnvManager.DisasterScenario.Shindo2:
                return 2;
            case EnvManager.DisasterScenario.Shindo6:
                return 6;
            case EnvManager.DisasterScenario.Shindo7Tsunami:
                return 7;
            default:
                return 2;
        }
    }

    /// <summary>
    /// 最寄りの避難所を検索
    /// </summary>
    private (GameObject shelter, float distance) FindNearestShelter()
    {
        if (_envManager == null || _envManager.Shelters == null || _envManager.Shelters.Count == 0)
        {
            return (null, float.MaxValue);
        }

        GameObject nearestShelter = null;
        float nearestDistance = float.MaxValue;
        Vector3 currentPos = _evacuee.transform.position;

        foreach (var shelterObj in _envManager.Shelters)
        {
            if (shelterObj == null) continue;

            var shelter = shelterObj.GetComponent<Shelter>();
            if (shelter == null || shelter.currentCapacity <= 0) continue;

            // 避難所の入口位置を取得
            var pointTransform = shelterObj.transform.childCount > 0
                ? shelterObj.transform.GetChild(0)
                : shelterObj.transform;

            float distance = Vector3.Distance(currentPos, pointTransform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestShelter = pointTransform.gameObject;
            }
        }

        return (nearestShelter, nearestDistance);
    }

    /// <summary>
    /// 避難理由の文字列を生成（津波警報のみで判定）
    /// </summary>
    private string BuildEvacuateReason()
    {
        var reasons = new List<string>();

        if (_hasReceivedJAlert)
        {
            reasons.Add("Jアラートを受信");
        }

        if (_hasHeardBroadcast)
        {
            reasons.Add("行政無線の避難指示を聞いた");
        }

        if (_hasHeardFireTruck)
        {
            reasons.Add("消防団の呼びかけを聞いた");
        }

        if (reasons.Count == 0)
        {
            return "避難を開始";
        }

        return string.Join("、", reasons) + "ため避難を開始";
    }

    /// <summary>
    /// 状態をリセット（エピソード開始時）
    /// </summary>
    public void Reset()
    {
        _stayStartTime = -1f;
        _lastEvaluationTime = 0f;
        _hasHeardBroadcast = false;
        _hasReceivedJAlert = false;
        _hasHeardFireTruck = false;
        _hasContactedFamily = false;  // Level 2: 連絡状態もリセット
    }
}
