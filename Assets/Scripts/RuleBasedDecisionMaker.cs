using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using LLM;

/// <summary>
/// 簡易ルールベースエージェントの意思決定を行うクラス（Level 1）
/// 実験1でLLMエージェントとの比較に使用
///
/// Level 1のルール:
/// - 行動タイプは EVACUATE と STAY のみ
/// - 津波警報受信 OR 震度6以上 → EVACUATE
/// - 震度5以下 AND 警報未受信 → STAY（一定時間後に再評価）
/// </summary>
public class RuleBasedDecisionMaker : MonoBehaviour
{
    [Header("Rule Parameters")]
    [Tooltip("待機から避難に切り替える最大時間（秒）")]
    public float MaxStayDuration = 60f;

    [Tooltip("危険と判定する震度の閾値")]
    public int DangerousSeismicIntensity = 6;

    [Tooltip("ルール再評価の間隔（秒）")]
    public float ReevaluationInterval = 10f;

    private Evacuee _evacuee;
    private EnvManager _envManager;
    private NavMeshAgent _navAgent;
    private float _stayStartTime = -1f;
    private float _lastEvaluationTime = 0f;
    private bool _hasHeardBroadcast = false;
    private bool _hasReceivedJAlert = false;
    private bool _hasHeardFireTruck = false;

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
    /// ルールベースで行動を決定
    /// </summary>
    /// <returns>決定された行動タイプと目標避難所</returns>
    public (ActionType action, GameObject targetShelter, string reasoning) MakeDecision()
    {
        float currentTime = _envManager != null ? _envManager.CurrentTimeSec : Time.time;

        // 最小評価間隔のチェック
        if (currentTime - _lastEvaluationTime < ReevaluationInterval && _lastEvaluationTime > 0)
        {
            // 前回と同じ行動を継続
            return (_evacuee.CurrentAction, _evacuee.Target, "前回の判断を継続");
        }
        _lastEvaluationTime = currentTime;

        // 震度を取得
        int seismicIntensity = GetSeismicIntensity();

        // ルール1: 高危険度の場合は即座に避難
        if (seismicIntensity >= DangerousSeismicIntensity || _hasReceivedJAlert || _hasHeardBroadcast || _hasHeardFireTruck)
        {
            var (shelter, distance) = FindNearestShelter();
            string reason = BuildEvacuateReason(seismicIntensity);
            return (ActionType.EVACUATE, shelter, reason);
        }

        // ルール2: 低危険度の場合は待機
        if (_stayStartTime < 0)
        {
            _stayStartTime = currentTime;
        }

        float stayDuration = currentTime - _stayStartTime;

        // ルール3: 待機時間が上限を超えた場合は避難
        if (stayDuration >= MaxStayDuration)
        {
            var (shelter, distance) = FindNearestShelter();
            return (ActionType.EVACUATE, shelter, $"待機時間が{MaxStayDuration}秒を超えたため避難を開始");
        }

        // 待機を継続
        return (ActionType.STAY, null, $"震度{seismicIntensity}、警報未受信のため様子見（経過{stayDuration:F0}秒）");
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
    /// 避難理由の文字列を生成
    /// </summary>
    private string BuildEvacuateReason(int seismicIntensity)
    {
        var reasons = new List<string>();

        if (seismicIntensity >= DangerousSeismicIntensity)
        {
            reasons.Add($"震度{seismicIntensity}の強い揺れを感知");
        }

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
    }
}
