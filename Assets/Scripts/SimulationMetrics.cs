using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using LLM;

/// <summary>
/// シミュレーションの評価指標を計測・記録するクラス
/// 実験1（LLM vs ルールベース比較）のための指標収集
/// </summary>
public class SimulationMetrics : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("指標の記録を有効にする")]
    public bool EnableMetrics = true;

    [Tooltip("ログ出力先ディレクトリ（Assets/Logsからの相対パス）")]
    public string OutputDirectory = "experiment_results";

    [Tooltip("避難完了判定の制限時間（秒）- paper.mdでは30分=1800秒")]
    public float EvacuationTimeLimit = 1800f;

    private EnvManager _envManager;
    private string _experimentId;
    private float _episodeStartTime;

    // エピソードごとの行動ログ
    private List<ActionLogEntry> _actionLogs = new List<ActionLogEntry>();

    // エピソードごとの避難完了記録
    private List<EvacuationRecord> _evacuationRecords = new List<EvacuationRecord>();

    // 行動タイプごとのカウント
    private Dictionary<ActionType, int> _actionCounts = new Dictionary<ActionType, int>();

    // 避難所ごとの選択カウント
    private Dictionary<string, int> _shelterSelectionCounts = new Dictionary<string, int>();

    // エージェント別の評価指標
    private Dictionary<string, AgentMetrics> _agentMetrics = new Dictionary<string, AgentMetrics>();

    // エージェント別のSTAY開始時刻（STAY継続時間計算用）
    private Dictionary<string, float> _stayStartTimes = new Dictionary<string, float>();

    /// <summary>
    /// 行動ログエントリ
    /// </summary>
    [Serializable]
    public class ActionLogEntry
    {
        public float timestamp;
        public string agentId;
        public string actionType;
        public string targetShelter;
        public string reasoning;
        public float confidence;
        // 階層的意思決定用フィールド
        public string primaryGoal;      // 長期目標
        public string planSteps;        // 中期計画のステップ（カンマ区切り）
        public bool goalUpdated;        // 目標更新フラグ
        public bool planUpdated;        // 計画更新フラグ
    }

    /// <summary>
    /// エージェント別の評価指標
    /// </summary>
    [Serializable]
    public class AgentMetrics
    {
        public string agentId;
        public string personaName;
        public string mentalState;          // バイアス条件（mental_state）
        public float firstEvacuateTime;     // 最初のEVACUATE時刻（-1なら未避難）
        public float totalStayDuration;     // STAY継続時間合計
        public int followCount;             // FOLLOW選択回数
        public int talkCount;               // TALK選択回数
        public int contactCount;            // CONTACT選択回数
        public int searchFamilyCount;       // SEARCH_FAMILY選択回数
        public int goalUpdateCount;         // 長期目標更新回数
        public int planUpdateCount;         // 中期計画更新回数
        public bool evacuationCompleted;    // 避難完了したか
        public float evacuationTime;        // 避難完了時間（-1なら未完了）
        public string finalShelter;         // 到達した避難所
    }

    /// <summary>
    /// 避難完了記録
    /// </summary>
    [Serializable]
    public class EvacuationRecord
    {
        public string agentId;
        public float evacuationTime;  // 発災からの経過時間（秒）
        public bool completedInTime;  // 制限時間内に完了したか
        public string finalShelter;   // 到達した避難所
        public string agentType;      // "LLM" or "RuleBased"
    }

    /// <summary>
    /// エピソードサマリ
    /// </summary>
    [Serializable]
    public class EpisodeSummary
    {
        public int episodeId;
        public string agentType;
        public float evacuationRate;
        public float averageEvacuationTime;
        public float medianEvacuationTime;
        public float maxEvacuationTime;
        public int totalAgents;
        public int evacuatedAgents;
        public Dictionary<string, float> actionDistribution;
        public Dictionary<string, int> shelterDistribution;
    }

    void Awake()
    {
        _envManager = FindFirstObjectByType<EnvManager>();
        if (_envManager == null)
        {
            Debug.LogError("[SimulationMetrics] EnvManagerが見つかりません");
            enabled = false;
            return;
        }

        // 実験IDを生成
        _experimentId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // イベントハンドラを登録
        _envManager.OnStartEpisode += OnEpisodeStart;
        _envManager.OnEndEpisode += OnEpisodeEnd;

        InitializeActionCounts();
    }

    void OnDestroy()
    {
        if (_envManager != null)
        {
            _envManager.OnStartEpisode -= OnEpisodeStart;
            _envManager.OnEndEpisode -= OnEpisodeEnd;
        }
    }

    private void InitializeActionCounts()
    {
        _actionCounts.Clear();
        foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
        {
            _actionCounts[action] = 0;
        }
    }

    private void OnEpisodeStart()
    {
        if (!EnableMetrics) return;

        _episodeStartTime = Time.time;
        _actionLogs.Clear();
        _evacuationRecords.Clear();
        InitializeActionCounts();
        _shelterSelectionCounts.Clear();
        _agentMetrics.Clear();
        _stayStartTimes.Clear();

        // エージェント別メトリクスを初期化
        InitializeAgentMetrics();

        Debug.Log($"[SimulationMetrics] エピソード {_envManager.currentEpisodeId} 開始 - 指標計測開始");
    }

    private void InitializeAgentMetrics()
    {
        foreach (var evacueeObj in _envManager.Evacuees)
        {
            if (evacueeObj == null) continue;

            var evacuee = evacueeObj.GetComponent<Evacuee>();
            if (evacuee == null) continue;

            string agentId = evacuee.EvacueeId ?? evacueeObj.name;

            // ペルソナ情報を取得（EvacueeIdからintに変換）
            PersonaData persona = null;
            if (int.TryParse(agentId, out int personaId))
            {
                persona = PersonaManager.GetPersona(personaId);
            }

            var metrics = new AgentMetrics
            {
                agentId = agentId,
                personaName = persona?.name ?? evacuee.PersonaName ?? "Unknown",
                mentalState = persona?.mental_state ?? "",
                firstEvacuateTime = -1f,
                totalStayDuration = 0f,
                followCount = 0,
                talkCount = 0,
                contactCount = 0,
                searchFamilyCount = 0,
                goalUpdateCount = 0,
                planUpdateCount = 0,
                evacuationCompleted = false,
                evacuationTime = -1f,
                finalShelter = ""
            };

            _agentMetrics[agentId] = metrics;
        }
    }

    private void OnEpisodeEnd(float evacuationRate)
    {
        if (!EnableMetrics) return;

        // 避難完了記録を収集
        CollectEvacuationRecords();

        // 未完了エージェントのSTAY継続時間を確定
        FinalizeAgentMetrics();

        // サマリを生成
        var summary = GenerateEpisodeSummary();

        // ログを出力
        SaveEpisodeLogs();
        SaveEpisodeSummary(summary);
        SaveAgentMetrics();

        Debug.Log($"[SimulationMetrics] エピソード {_envManager.currentEpisodeId} 終了 - " +
                  $"避難完了率: {summary.evacuationRate:P1}, " +
                  $"平均避難時間: {summary.averageEvacuationTime:F1}秒");
    }

    /// <summary>
    /// エージェント別メトリクスを確定（エピソード終了時）
    /// </summary>
    private void FinalizeAgentMetrics()
    {
        float endTime = _envManager.CurrentTimeSec;

        foreach (var kvp in _agentMetrics)
        {
            var metrics = kvp.Value;

            // 残っているSTAY継続時間があれば加算
            if (_stayStartTimes.TryGetValue(kvp.Key, out float stayStart))
            {
                metrics.totalStayDuration += endTime - stayStart;
            }
        }

        _stayStartTimes.Clear();
    }

    /// <summary>
    /// エージェントの行動を記録（Evacueeから呼び出される）
    /// </summary>
    public void RecordAction(string agentId, ActionType actionType, string targetShelter = null,
                             string reasoning = null, float confidence = 0f)
    {
        RecordActionWithHierarchy(agentId, actionType, targetShelter, reasoning, confidence, null, null, false, false);
    }

    /// <summary>
    /// エージェントの行動を記録（階層的意思決定情報を含む拡張版）
    /// </summary>
    public void RecordActionWithHierarchy(string agentId, ActionType actionType, string targetShelter,
                                          string reasoning, float confidence,
                                          string primaryGoal, string[] planSteps,
                                          bool goalUpdated, bool planUpdated)
    {
        if (!EnableMetrics) return;

        float currentTime = _envManager.CurrentTimeSec;

        var entry = new ActionLogEntry
        {
            timestamp = currentTime,
            agentId = agentId,
            actionType = actionType.ToString(),
            targetShelter = targetShelter ?? "",
            reasoning = reasoning ?? "",
            confidence = confidence,
            primaryGoal = primaryGoal ?? "",
            planSteps = planSteps != null ? string.Join("|", planSteps) : "",
            goalUpdated = goalUpdated,
            planUpdated = planUpdated
        };

        _actionLogs.Add(entry);

        // 行動カウントを更新
        if (_actionCounts.ContainsKey(actionType))
        {
            _actionCounts[actionType]++;
        }

        // 避難所選択カウントを更新
        if (actionType == ActionType.EVACUATE && !string.IsNullOrEmpty(targetShelter))
        {
            if (!_shelterSelectionCounts.ContainsKey(targetShelter))
            {
                _shelterSelectionCounts[targetShelter] = 0;
            }
            _shelterSelectionCounts[targetShelter]++;
        }

        // エージェント別メトリクスを更新
        UpdateAgentMetrics(agentId, actionType, currentTime, goalUpdated, planUpdated);
    }

    /// <summary>
    /// エージェント別メトリクスを更新
    /// </summary>
    private void UpdateAgentMetrics(string agentId, ActionType actionType, float currentTime, bool goalUpdated, bool planUpdated)
    {
        if (!_agentMetrics.TryGetValue(agentId, out var metrics))
        {
            // まだ初期化されていないエージェントの場合は新規作成
            metrics = new AgentMetrics
            {
                agentId = agentId,
                personaName = "Unknown",
                mentalState = "",
                firstEvacuateTime = -1f,
                totalStayDuration = 0f,
                followCount = 0,
                talkCount = 0,
                contactCount = 0,
                searchFamilyCount = 0,
                goalUpdateCount = 0,
                planUpdateCount = 0,
                evacuationCompleted = false,
                evacuationTime = -1f,
                finalShelter = ""
            };
            _agentMetrics[agentId] = metrics;
        }

        // STAYからの遷移時にSTAY継続時間を加算
        if (_stayStartTimes.TryGetValue(agentId, out float stayStart) && actionType != ActionType.STAY)
        {
            metrics.totalStayDuration += currentTime - stayStart;
            _stayStartTimes.Remove(agentId);
        }

        // 行動タイプ別の処理
        switch (actionType)
        {
            case ActionType.EVACUATE:
                if (metrics.firstEvacuateTime < 0)
                {
                    metrics.firstEvacuateTime = currentTime;
                }
                break;
            case ActionType.STAY:
                if (!_stayStartTimes.ContainsKey(agentId))
                {
                    _stayStartTimes[agentId] = currentTime;
                }
                break;
            case ActionType.FOLLOW:
                metrics.followCount++;
                break;
            case ActionType.TALK:
                metrics.talkCount++;
                break;
            case ActionType.CONTACT:
                metrics.contactCount++;
                break;
            case ActionType.SEARCH_FAMILY:
                metrics.searchFamilyCount++;
                break;
        }

        // 目標・計画更新カウント
        if (goalUpdated) metrics.goalUpdateCount++;
        if (planUpdated) metrics.planUpdateCount++;
    }

    /// <summary>
    /// エージェントの避難完了を記録（Shelterから呼び出される）
    /// </summary>
    public void RecordEvacuation(string agentId, float evacuationTime, string shelterName, string agentType)
    {
        if (!EnableMetrics) return;

        var record = new EvacuationRecord
        {
            agentId = agentId,
            evacuationTime = evacuationTime,
            completedInTime = evacuationTime <= EvacuationTimeLimit,
            finalShelter = shelterName,
            agentType = agentType
        };

        _evacuationRecords.Add(record);

        // エージェント別メトリクスを更新
        if (_agentMetrics.TryGetValue(agentId, out var metrics))
        {
            metrics.evacuationCompleted = true;
            metrics.evacuationTime = evacuationTime;
            metrics.finalShelter = shelterName;

            // 残っているSTAY継続時間があれば加算
            if (_stayStartTimes.TryGetValue(agentId, out float stayStart))
            {
                metrics.totalStayDuration += evacuationTime - stayStart;
                _stayStartTimes.Remove(agentId);
            }
        }
    }

    private void CollectEvacuationRecords()
    {
        // 既に記録されていない避難者について、最終状態を収集
        foreach (var evacueeObj in _envManager.Evacuees)
        {
            if (evacueeObj == null) continue;

            var evacuee = evacueeObj.GetComponent<Evacuee>();
            if (evacuee == null) continue;

            string agentId = evacueeObj.name;

            // 既に記録済みかチェック
            if (_evacuationRecords.Any(r => r.agentId == agentId)) continue;

            // 避難完了していない場合は記録
            if (evacueeObj.activeSelf)
            {
                var record = new EvacuationRecord
                {
                    agentId = agentId,
                    evacuationTime = _envManager.CurrentTimeSec,
                    completedInTime = false,
                    finalShelter = "",
                    agentType = evacuee.UseLLMDecision ? "LLM" : "RuleBased"
                };
                _evacuationRecords.Add(record);
            }
        }
    }

    private EpisodeSummary GenerateEpisodeSummary()
    {
        var completedRecords = _evacuationRecords.Where(r => r.completedInTime).ToList();
        var evacuationTimes = completedRecords.Select(r => r.evacuationTime).ToList();

        // 行動分布を計算
        int totalActions = _actionCounts.Values.Sum();
        var actionDistribution = new Dictionary<string, float>();
        foreach (var kvp in _actionCounts)
        {
            actionDistribution[kvp.Key.ToString()] = totalActions > 0
                ? (float)kvp.Value / totalActions
                : 0f;
        }

        return new EpisodeSummary
        {
            episodeId = _envManager.currentEpisodeId,
            agentType = DetermineAgentType(),
            evacuationRate = _envManager.EvacuationRate,
            averageEvacuationTime = evacuationTimes.Count > 0 ? evacuationTimes.Average() : 0f,
            medianEvacuationTime = evacuationTimes.Count > 0 ? GetMedian(evacuationTimes) : 0f,
            maxEvacuationTime = evacuationTimes.Count > 0 ? evacuationTimes.Max() : 0f,
            totalAgents = _envManager.Evacuees.Count,
            evacuatedAgents = completedRecords.Count,
            actionDistribution = actionDistribution,
            shelterDistribution = new Dictionary<string, int>(_shelterSelectionCounts)
        };
    }

    private string DetermineAgentType()
    {
        // 最初の避難者のタイプを取得
        if (_envManager.Evacuees.Count > 0 && _envManager.Evacuees[0] != null)
        {
            var evacuee = _envManager.Evacuees[0].GetComponent<Evacuee>();
            if (evacuee != null)
            {
                return evacuee.UseLLMDecision ? "LLM" : "RuleBased";
            }
        }
        return "Unknown";
    }

    private float GetMedian(List<float> values)
    {
        if (values.Count == 0) return 0f;

        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;

        if (sorted.Count % 2 == 0)
        {
            return (sorted[mid - 1] + sorted[mid]) / 2f;
        }
        return sorted[mid];
    }

    private void SaveEpisodeLogs()
    {
        string directory = GetOutputDirectory();
        string filename = $"episode_{_envManager.currentEpisodeId}_actions.csv";
        string filepath = Path.Combine(directory, filename);

        using (var writer = new StreamWriter(filepath))
        {
            // ヘッダー（階層的意思決定フィールドを追加）
            writer.WriteLine("timestamp,agent_id,action_type,target_shelter,reasoning,confidence,primary_goal,plan_steps,goal_updated,plan_updated");

            // データ
            foreach (var log in _actionLogs)
            {
                string reasoning = log.reasoning.Replace(",", ";").Replace("\n", " ");
                string primaryGoal = (log.primaryGoal ?? "").Replace(",", ";").Replace("\n", " ");
                string planSteps = (log.planSteps ?? "").Replace(",", ";").Replace("\n", " ");
                writer.WriteLine($"{log.timestamp:F2},{log.agentId},{log.actionType}," +
                               $"{log.targetShelter},{reasoning},{log.confidence:F2}," +
                               $"{primaryGoal},{planSteps},{log.goalUpdated},{log.planUpdated}");
            }
        }

        Debug.Log($"[SimulationMetrics] 行動ログを保存: {filepath}");
    }

    private void SaveEpisodeSummary(EpisodeSummary summary)
    {
        string directory = GetOutputDirectory();
        string filename = $"episode_{summary.episodeId}_summary.csv";
        string filepath = Path.Combine(directory, filename);

        using (var writer = new StreamWriter(filepath))
        {
            writer.WriteLine("metric,value");
            writer.WriteLine($"episode_id,{summary.episodeId}");
            writer.WriteLine($"agent_type,{summary.agentType}");
            writer.WriteLine($"evacuation_rate,{summary.evacuationRate:F4}");
            writer.WriteLine($"average_evacuation_time,{summary.averageEvacuationTime:F2}");
            writer.WriteLine($"median_evacuation_time,{summary.medianEvacuationTime:F2}");
            writer.WriteLine($"max_evacuation_time,{summary.maxEvacuationTime:F2}");
            writer.WriteLine($"total_agents,{summary.totalAgents}");
            writer.WriteLine($"evacuated_agents,{summary.evacuatedAgents}");

            // 行動分布
            writer.WriteLine("");
            writer.WriteLine("action_type,ratio");
            foreach (var kvp in summary.actionDistribution)
            {
                writer.WriteLine($"{kvp.Key},{kvp.Value:F4}");
            }

            // 避難所分布
            writer.WriteLine("");
            writer.WriteLine("shelter,count");
            foreach (var kvp in summary.shelterDistribution)
            {
                writer.WriteLine($"{kvp.Key},{kvp.Value}");
            }
        }

        Debug.Log($"[SimulationMetrics] サマリを保存: {filepath}");
    }

    private string GetOutputDirectory()
    {
        string basePath = Path.Combine(Application.dataPath, "Logs", OutputDirectory, _experimentId);

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return basePath;
    }

    /// <summary>
    /// エージェント別メトリクスをCSVに保存
    /// </summary>
    private void SaveAgentMetrics()
    {
        string directory = GetOutputDirectory();
        string filename = $"episode_{_envManager.currentEpisodeId}_agents.csv";
        string filepath = Path.Combine(directory, filename);

        using (var writer = new StreamWriter(filepath))
        {
            // ヘッダー
            writer.WriteLine("agent_id,persona_name,mental_state,first_evacuate_time,total_stay_duration," +
                           "follow_count,talk_count,contact_count,search_family_count," +
                           "goal_update_count,plan_update_count,evacuation_completed,evacuation_time,final_shelter");

            // データ
            foreach (var kvp in _agentMetrics)
            {
                var m = kvp.Value;
                string personaName = (m.personaName ?? "").Replace(",", ";");
                string mentalState = (m.mentalState ?? "").Replace(",", ";");
                string finalShelter = (m.finalShelter ?? "").Replace(",", ";");

                writer.WriteLine($"{m.agentId},{personaName},{mentalState},{m.firstEvacuateTime:F2},{m.totalStayDuration:F2}," +
                               $"{m.followCount},{m.talkCount},{m.contactCount},{m.searchFamilyCount}," +
                               $"{m.goalUpdateCount},{m.planUpdateCount},{m.evacuationCompleted},{m.evacuationTime:F2},{finalShelter}");
            }
        }

        Debug.Log($"[SimulationMetrics] エージェント別メトリクスを保存: {filepath}");
    }

    /// <summary>
    /// 現在のエピソードの指標を取得（デバッグ用）
    /// </summary>
    public EpisodeSummary GetCurrentMetrics()
    {
        return GenerateEpisodeSummary();
    }
}
