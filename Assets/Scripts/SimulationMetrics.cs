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

        Debug.Log($"[SimulationMetrics] エピソード {_envManager.currentEpisodeId} 開始 - 指標計測開始");
    }

    private void OnEpisodeEnd(float evacuationRate)
    {
        if (!EnableMetrics) return;

        // 避難完了記録を収集
        CollectEvacuationRecords();

        // サマリを生成
        var summary = GenerateEpisodeSummary();

        // ログを出力
        SaveEpisodeLogs();
        SaveEpisodeSummary(summary);

        Debug.Log($"[SimulationMetrics] エピソード {_envManager.currentEpisodeId} 終了 - " +
                  $"避難完了率: {summary.evacuationRate:P1}, " +
                  $"平均避難時間: {summary.averageEvacuationTime:F1}秒");
    }

    /// <summary>
    /// エージェントの行動を記録（Evacueeから呼び出される）
    /// </summary>
    public void RecordAction(string agentId, ActionType actionType, string targetShelter = null,
                             string reasoning = null, float confidence = 0f)
    {
        if (!EnableMetrics) return;

        var entry = new ActionLogEntry
        {
            timestamp = _envManager.CurrentTimeSec,
            agentId = agentId,
            actionType = actionType.ToString(),
            targetShelter = targetShelter ?? "",
            reasoning = reasoning ?? "",
            confidence = confidence
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
            // ヘッダー
            writer.WriteLine("timestamp,agent_id,action_type,target_shelter,reasoning,confidence");

            // データ
            foreach (var log in _actionLogs)
            {
                string reasoning = log.reasoning.Replace(",", ";").Replace("\n", " ");
                writer.WriteLine($"{log.timestamp:F2},{log.agentId},{log.actionType}," +
                               $"{log.targetShelter},{reasoning},{log.confidence:F2}");
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
    /// 現在のエピソードの指標を取得（デバッグ用）
    /// </summary>
    public EpisodeSummary GetCurrentMetrics()
    {
        return GenerateEpisodeSummary();
    }
}
