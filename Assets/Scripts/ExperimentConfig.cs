using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 試行結果を保持する構造体
/// HotReloadの互換性のためクラス外に定義
/// </summary>
[System.Serializable]
public struct ExperimentTrialResult
{
    public int trialIndex;
    public float evacuationRate;
    public float elapsedTime;
    public string timestamp;
}

/// <summary>
/// 実験条件を定義する構造体（バッチ実験用）
/// </summary>
[System.Serializable]
public struct ExperimentCondition
{
    public string conditionId;
    public ExperimentConfig.AgentType agentType;
    public ExperimentConfig.BiasCondition biasCondition;
    public ExperimentConfig.InformationStrategy informationStrategy;
    public bool overrideBias;
}

/// <summary>
/// バッチ実験の条件別結果を保持する構造体
/// </summary>
[System.Serializable]
public struct BatchConditionResult
{
    public string conditionId;
    public List<ExperimentTrialResult> trialResults;
    public float averageEvacuationRate;
    public float stdDev;
}

/// <summary>
/// 実験条件の設定を管理するクラス
/// UnityのInspectorから設定を変更可能
/// N回の試行後に自動的に実験を終了し、結果をまとめて出力する
/// </summary>
public class ExperimentConfig : MonoBehaviour
{
    /// <summary>
    /// エージェントタイプの選択
    /// </summary>
    public enum AgentType
    {
        LLM,        // LLMエージェント
        RuleBased   // ルールベースエージェント
    }

    /// <summary>
    /// 実験終了時のアクション
    /// </summary>
    public enum ExperimentEndAction
    {
        PauseSimulation,    // シミュレーションを一時停止
        StopSimulation,     // シミュレーションを完全停止
        QuitApplication     // アプリケーションを終了
    }

    /// <summary>
    /// 認知バイアス条件（実験2-2用）
    /// </summary>
    public enum BiasCondition
    {
        None,              // ベースライン（既存ペルソナのmental_stateを使用）
        NormalcyBias,      // 正常性バイアス（全員に強制適用）
        ConformityBias,    // 同調バイアス（全員に強制適用）
        Combined           // 複合バイアス（正常性 + 同調）
    }

    /// <summary>
    /// 情報提供戦略（実験3用）
    /// 「情報の具体性」×「表現の切迫感」の2軸で4条件を設定
    /// </summary>
    public enum InformationStrategy
    {
        /// <summary>
        /// 条件A: 標準警報のみ（具体性:低 × 切迫感:低）
        /// 気象庁発表をそのまま伝達する定型的対応
        /// </summary>
        Standard,

        /// <summary>
        /// 条件B: 強い警告表現（具体性:低 × 切迫感:高）
        /// NHKアナウンサー調の危機感を強調した表現
        /// </summary>
        Urgent,

        /// <summary>
        /// 条件C: 詳細な避難先指示（具体性:高 × 切迫感:低）
        /// 具体的な避難所名と海抜情報を提供
        /// </summary>
        Detailed,

        /// <summary>
        /// 条件D: 詳細＋強い切迫感（具体性:高 × 切迫感:高）
        /// 最大限の情報提供と強い切迫感の両方
        /// </summary>
        DetailedUrgent
    }

    /// <summary>
    /// バッチ実験のプリセット
    /// </summary>
    public enum BatchExperimentPreset
    {
        /// <summary>バッチモード無効（従来どおり単一条件）</summary>
        None,
        /// <summary>実験1のみ（LLM vs RuleBased、2条件）</summary>
        Experiment1,
        /// <summary>実験2のみ（認知バイアス4条件）</summary>
        Experiment2,
        /// <summary>実験3のみ（情報提供戦略4条件）</summary>
        Experiment3,
        /// <summary>全実験（10条件）</summary>
        AllExperiments
    }

    [Header("Experiment Settings")]
    [Tooltip("使用するエージェントタイプ")]
    public AgentType SelectedAgentType = AgentType.LLM;

    [Tooltip("ランダムシード（-1で毎回異なるシード）")]
    public int RandomSeed = -1;

    [Tooltip("試行回数（1試行 = 1エピソード）")]
    public int NumberOfTrials = 10;

    [Tooltip("試行回数に達したら自動的に実験を終了する")]
    public bool AutoStopOnComplete = true;

    [Tooltip("実験終了時のアクション")]
    public ExperimentEndAction EndAction = ExperimentEndAction.PauseSimulation;

    [Tooltip("シミュレーション開始時に自動的に実験を開始する")]
    public bool AutoStartOnPlay = true;

    [Header("Experiment 2-2: Cognitive Bias Settings")]
    [Tooltip("認知バイアス実験条件")]
    public BiasCondition SelectedBiasCondition = BiasCondition.None;

    [Tooltip("バイアス条件を全エージェントに一律適用（false=ペルソナのmental_stateを使用）")]
    public bool OverrideBiasForAllAgents = false;

    [Header("Experiment 3: Information Strategy Settings")]
    [Tooltip("情報提供戦略（具体性×切迫感の4条件）")]
    public InformationStrategy SelectedInformationStrategy = InformationStrategy.Standard;

    [Tooltip("津波の実際の高さ（メートル）- 生存判定に使用")]
    public float TsunamiHeight = 8f;

    [Tooltip("津波到達時刻（秒）- 生存判定のタイミング")]
    public float TsunamiArrivalTime = 1500f; // 25分 = 1500秒

    [Header("Output Settings")]
    [Tooltip("結果をCSVに出力する")]
    public bool ExportResults = true;

    [Tooltip("実験名（出力ファイルの識別用）")]
    public string ExperimentName = "experiment_1";

    [Tooltip("サマリーレポートを生成する")]
    public bool GenerateSummaryReport = true;

    [Header("Batch Experiment Settings")]
    [Tooltip("バッチ実験プリセット（Noneで従来の単一実験モード）")]
    public BatchExperimentPreset BatchPreset = BatchExperimentPreset.None;

    [Tooltip("各条件の試行回数")]
    public int TrialsPerCondition = 10;

    private EnvManager _envManager;
    private int _currentTrialIndex = 0;
    private bool _experimentStarted = false;
    private bool _experimentCompleted = false;

    // 各試行の結果を保存
    private List<ExperimentTrialResult> _trialResults = new List<ExperimentTrialResult>();

    // バッチ実験用内部状態
    private List<ExperimentCondition> _batchConditions;
    private int _currentConditionIndex = 0;
    private List<BatchConditionResult> _batchResults = new List<BatchConditionResult>();
    private string _batchTimestamp;

    public static ExperimentConfig Instance { get; private set; }

    /// <summary>
    /// 現在の試行番号（1始まり）
    /// </summary>
    public int CurrentTrialNumber => _currentTrialIndex + 1;

    /// <summary>
    /// 実験が完了したかどうか
    /// </summary>
    public bool IsExperimentCompleted => _experimentCompleted;

    /// <summary>
    /// 実験が進行中かどうか
    /// </summary>
    public bool IsExperimentRunning => _experimentStarted && !_experimentCompleted;

    void Awake()
    {
        // シングルトンパターン
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ランダムシードを設定
        if (RandomSeed >= 0)
        {
            Random.InitState(RandomSeed);
            Debug.Log($"[ExperimentConfig] ランダムシードを設定: {RandomSeed}");
        }
        else
        {
            int seed = System.Environment.TickCount;
            Random.InitState(seed);
            Debug.Log($"[ExperimentConfig] ランダムシードを自動生成: {seed}");
        }
    }

    void Start()
    {
        _envManager = FindFirstObjectByType<EnvManager>();

        if (_envManager != null)
        {
            _envManager.OnEndEpisode += OnTrialEnd;
        }

        // 実験設定をログに出力
        Debug.Log($"[ExperimentConfig] 実験設定:" +
                  $"\n  - エージェントタイプ: {SelectedAgentType}" +
                  $"\n  - 試行回数: {NumberOfTrials}" +
                  $"\n  - 実験名: {ExperimentName}" +
                  $"\n  - 自動開始: {AutoStartOnPlay}" +
                  $"\n  - バイアス条件: {SelectedBiasCondition}" +
                  $"\n  - バイアスオーバーライド: {OverrideBiasForAllAgents}" +
                  $"\n  - 情報提供戦略: {SelectedInformationStrategy}" +
                  $"\n  - 津波高さ: {TsunamiHeight}m" +
                  $"\n  - 津波到達時刻: {TsunamiArrivalTime}秒");

        // 自動開始が有効な場合、実験を自動的に開始
        if (AutoStartOnPlay)
        {
            if (IsBatchMode)
            {
                StartBatchExperiment();
            }
            else
            {
                StartExperiment();
            }
        }
    }

    void OnDestroy()
    {
        if (_envManager != null)
        {
            _envManager.OnEndEpisode -= OnTrialEnd;
        }
    }

    /// <summary>
    /// アプリケーション終了時（エディタの再生停止を含む）にリセット
    /// </summary>
    void OnApplicationQuit()
    {
        // 次回実行時のためにリセット状態を記録
        Debug.Log("[ExperimentConfig] アプリケーション終了 - 実験状態をリセット");
        ResetExperimentState();
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタの再生モード変更時に呼ばれる
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnPlayModeStateChanged()
    {
        // ドメインリロード時に静的変数をリセット
        Instance = null;
    }
#endif

    /// <summary>
    /// 実験状態を内部的にリセット（ログ出力なし）
    /// </summary>
    private void ResetExperimentState()
    {
        _currentTrialIndex = 0;
        _experimentStarted = false;
        _experimentCompleted = false;
        _trialResults.Clear();
    }

    private void OnTrialEnd(float evacuationRate)
    {
        if (!_experimentStarted || _experimentCompleted) return;

        // 試行結果を記録
        var result = new ExperimentTrialResult
        {
            trialIndex = _currentTrialIndex + 1,
            evacuationRate = evacuationRate,
            elapsedTime = _envManager != null ? _envManager.CurrentTimeSec : 0f,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        _trialResults.Add(result);

        _currentTrialIndex++;
        Debug.Log($"[ExperimentConfig] 試行 {_currentTrialIndex}/{NumberOfTrials} 完了 - 避難率: {evacuationRate:P1}");

        if (_currentTrialIndex >= NumberOfTrials)
        {
            OnExperimentComplete();
        }
    }

    /// <summary>
    /// 実験完了時の処理
    /// </summary>
    private void OnExperimentComplete()
    {
        _experimentCompleted = true;
        _experimentStarted = false;

        // バッチモード時は次の条件へ移行
        if (IsBatchMode)
        {
            OnConditionComplete();
            return;
        }

        // 単一実験モード時は従来の処理
        Debug.Log($"[ExperimentConfig] ========================================");
        Debug.Log($"[ExperimentConfig] 実験完了！全{NumberOfTrials}試行が終了しました");
        Debug.Log($"[ExperimentConfig] ========================================");

        // サマリーレポートを生成
        if (GenerateSummaryReport)
        {
            GenerateReport();
        }

        // 結果をCSV出力
        if (ExportResults)
        {
            ExportResultsToCSV();
        }

        // 終了アクションを実行
        if (AutoStopOnComplete)
        {
            ExecuteEndAction();
        }
    }

    /// <summary>
    /// 実験終了時のアクションを実行
    /// </summary>
    private void ExecuteEndAction()
    {
        switch (EndAction)
        {
            case ExperimentEndAction.PauseSimulation:
                Time.timeScale = 0f;
                Debug.Log("[ExperimentConfig] シミュレーションを一時停止しました");
                break;

            case ExperimentEndAction.StopSimulation:
                Time.timeScale = 0f;
                if (_envManager != null)
                {
                    _envManager.EnableEnv = false;
                }
                Debug.Log("[ExperimentConfig] シミュレーションを停止しました");
                break;

            case ExperimentEndAction.QuitApplication:
                Debug.Log("[ExperimentConfig] アプリケーションを終了します...");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    /// <summary>
    /// サマリーレポートを生成してログに出力
    /// </summary>
    private void GenerateReport()
    {
        if (_trialResults.Count == 0) return;

        float totalEvacRate = 0f;
        float minEvacRate = float.MaxValue;
        float maxEvacRate = float.MinValue;
        float totalTime = 0f;

        foreach (var result in _trialResults)
        {
            totalEvacRate += result.evacuationRate;
            totalTime += result.elapsedTime;
            if (result.evacuationRate < minEvacRate) minEvacRate = result.evacuationRate;
            if (result.evacuationRate > maxEvacRate) maxEvacRate = result.evacuationRate;
        }

        float avgEvacRate = totalEvacRate / _trialResults.Count;
        float avgTime = totalTime / _trialResults.Count;

        // 標準偏差を計算
        float variance = 0f;
        foreach (var result in _trialResults)
        {
            variance += (result.evacuationRate - avgEvacRate) * (result.evacuationRate - avgEvacRate);
        }
        float stdDev = Mathf.Sqrt(variance / _trialResults.Count);

        var sb = new StringBuilder();
        sb.AppendLine("\n========== 実験サマリーレポート ==========");
        sb.AppendLine($"実験名: {ExperimentName}");
        sb.AppendLine($"エージェントタイプ: {SelectedAgentType}");
        sb.AppendLine($"試行回数: {_trialResults.Count}");
        sb.AppendLine($"------------------------------------------");
        sb.AppendLine($"平均避難率: {avgEvacRate:P2}");
        sb.AppendLine($"最小避難率: {minEvacRate:P2}");
        sb.AppendLine($"最大避難率: {maxEvacRate:P2}");
        sb.AppendLine($"標準偏差: {stdDev:P2}");
        sb.AppendLine($"平均経過時間: {avgTime:F1}秒");
        sb.AppendLine($"------------------------------------------");
        sb.AppendLine("各試行の結果:");
        foreach (var result in _trialResults)
        {
            sb.AppendLine($"  試行{result.trialIndex}: 避難率 {result.evacuationRate:P2}, 時間 {result.elapsedTime:F1}秒");
        }
        sb.AppendLine("==========================================\n");

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// 結果をCSVファイルに出力
    /// </summary>
    private void ExportResultsToCSV()
    {
        if (_trialResults.Count == 0) return;

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{ExperimentName}_{SelectedAgentType}_{timestamp}_results.csv";
        string directoryPath = Path.Combine(Application.dataPath, "..", "Logs", "experiment_results");

        // ディレクトリが存在しない場合は作成
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = Path.Combine(directoryPath, fileName);

        var sb = new StringBuilder();
        sb.AppendLine("trial_index,evacuation_rate,elapsed_time,timestamp");

        foreach (var result in _trialResults)
        {
            sb.AppendLine($"{result.trialIndex},{result.evacuationRate:F4},{result.elapsedTime:F2},{result.timestamp}");
        }

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"[ExperimentConfig] 結果をCSVに出力しました: {filePath}");
    }

    /// <summary>
    /// 現在のエージェントタイプを取得
    /// </summary>
    public static AgentType GetAgentType()
    {
        if (Instance != null)
        {
            return Instance.SelectedAgentType;
        }
        return AgentType.LLM; // デフォルト
    }

    /// <summary>
    /// エージェントがLLMを使用するかどうかを判定
    /// </summary>
    public static bool ShouldUseLLM()
    {
        return GetAgentType() == AgentType.LLM;
    }

    /// <summary>
    /// 現在のバイアス条件を取得
    /// </summary>
    public static BiasCondition GetBiasCondition()
    {
        return Instance != null ? Instance.SelectedBiasCondition : BiasCondition.None;
    }

    /// <summary>
    /// バイアス条件をオーバーライドするかどうかを判定
    /// </summary>
    public static bool ShouldOverrideBias()
    {
        return Instance != null && Instance.OverrideBiasForAllAgents;
    }

    /// <summary>
    /// 現在の情報提供戦略を取得
    /// </summary>
    public static InformationStrategy GetInformationStrategy()
    {
        return Instance != null ? Instance.SelectedInformationStrategy : InformationStrategy.Standard;
    }

    /// <summary>
    /// 津波の高さを取得（メートル）
    /// </summary>
    public static float GetTsunamiHeight()
    {
        return Instance != null ? Instance.TsunamiHeight : 8f;
    }

    /// <summary>
    /// 津波到達時刻を取得（秒）
    /// </summary>
    public static float GetTsunamiArrivalTime()
    {
        return Instance != null ? Instance.TsunamiArrivalTime : 1500f;
    }

    /// <summary>
    /// 情報提供戦略が「具体性:高」かどうかを判定
    /// </summary>
    public static bool IsDetailedStrategy()
    {
        var strategy = GetInformationStrategy();
        return strategy == InformationStrategy.Detailed || strategy == InformationStrategy.DetailedUrgent;
    }

    /// <summary>
    /// 情報提供戦略が「切迫感:高」かどうかを判定
    /// </summary>
    public static bool IsUrgentStrategy()
    {
        var strategy = GetInformationStrategy();
        return strategy == InformationStrategy.Urgent || strategy == InformationStrategy.DetailedUrgent;
    }

    /// <summary>
    /// 実験を開始
    /// </summary>
    public void StartExperiment()
    {
        _currentTrialIndex = 0;
        _experimentStarted = true;
        Debug.Log($"[ExperimentConfig] 実験開始: {ExperimentName}");
    }

    /// <summary>
    /// 実験をリセット
    /// </summary>
    public void ResetExperiment()
    {
        _currentTrialIndex = 0;
        _experimentStarted = false;
        _experimentCompleted = false;
        _trialResults.Clear();

        // ランダムシードを再設定
        if (RandomSeed >= 0)
        {
            Random.InitState(RandomSeed);
        }

        // TimeScaleを復元
        if (_envManager != null)
        {
            Time.timeScale = _envManager.TimeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Debug.Log("[ExperimentConfig] 実験をリセットしました");
    }

    /// <summary>
    /// 実験を再開（一時停止状態から）
    /// </summary>
    public void ResumeExperiment()
    {
        if (_experimentCompleted)
        {
            Debug.LogWarning("[ExperimentConfig] 実験は既に完了しています。ResetExperiment()を呼び出してください");
            return;
        }

        if (_envManager != null)
        {
            Time.timeScale = _envManager.TimeScale;
        }
        else
        {
            Time.timeScale = 1f;
        }

        Debug.Log("[ExperimentConfig] 実験を再開しました");
    }

    /// <summary>
    /// 現在の試行結果リストを取得
    /// </summary>
    public List<ExperimentTrialResult> GetTrialResults()
    {
        return new List<ExperimentTrialResult>(_trialResults);
    }

    /// <summary>
    /// 残り試行回数を取得
    /// </summary>
    public int RemainingTrials => NumberOfTrials - _currentTrialIndex;

    #region Batch Experiment

    /// <summary>
    /// バッチ実験を開始
    /// </summary>
    public void StartBatchExperiment()
    {
        if (BatchPreset == BatchExperimentPreset.None)
        {
            Debug.LogWarning("[ExperimentConfig] BatchPreset is None. Use StartExperiment() instead.");
            return;
        }

        _batchConditions = GenerateConditions(BatchPreset);
        _currentConditionIndex = 0;
        _batchResults.Clear();
        _batchTimestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        Debug.Log($"[ExperimentConfig] ========================================");
        Debug.Log($"[ExperimentConfig] バッチ実験開始: {BatchPreset}");
        Debug.Log($"[ExperimentConfig] 条件数: {_batchConditions.Count}, 各条件試行数: {TrialsPerCondition}");
        Debug.Log($"[ExperimentConfig] 総試行数: {_batchConditions.Count * TrialsPerCondition}");
        Debug.Log($"[ExperimentConfig] ========================================");

        ApplyCondition(_batchConditions[0]);
        StartExperiment();
    }

    /// <summary>
    /// プリセットに基づいて条件リストを生成
    /// </summary>
    private List<ExperimentCondition> GenerateConditions(BatchExperimentPreset preset)
    {
        var conditions = new List<ExperimentCondition>();

        // 実験1: エージェントタイプ比較（LLM vs RuleBased）
        if (preset == BatchExperimentPreset.Experiment1 || preset == BatchExperimentPreset.AllExperiments)
        {
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp1-A",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = false
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp1-B",
                agentType = AgentType.RuleBased,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = false
            });
        }

        // 実験2: 認知バイアスの影響（4条件）
        if (preset == BatchExperimentPreset.Experiment2 || preset == BatchExperimentPreset.AllExperiments)
        {
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp2-1",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = true
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp2-2",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.NormalcyBias,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = true
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp2-3",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.ConformityBias,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = true
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp2-4",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.Combined,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = true
            });
        }

        // 実験3: 情報提供戦略の検証（4条件）
        if (preset == BatchExperimentPreset.Experiment3 || preset == BatchExperimentPreset.AllExperiments)
        {
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp3-A",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Standard,
                overrideBias = false
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp3-B",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Urgent,
                overrideBias = false
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp3-C",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.Detailed,
                overrideBias = false
            });
            conditions.Add(new ExperimentCondition
            {
                conditionId = "Exp3-D",
                agentType = AgentType.LLM,
                biasCondition = BiasCondition.None,
                informationStrategy = InformationStrategy.DetailedUrgent,
                overrideBias = false
            });
        }

        return conditions;
    }

    /// <summary>
    /// 条件を適用
    /// </summary>
    private void ApplyCondition(ExperimentCondition condition)
    {
        SelectedAgentType = condition.agentType;
        SelectedBiasCondition = condition.biasCondition;
        SelectedInformationStrategy = condition.informationStrategy;
        OverrideBiasForAllAgents = condition.overrideBias;
        ExperimentName = condition.conditionId;
        NumberOfTrials = TrialsPerCondition;

        Debug.Log($"[ExperimentConfig] ----------------------------------------");
        Debug.Log($"[ExperimentConfig] 条件適用: {condition.conditionId}");
        Debug.Log($"[ExperimentConfig]   AgentType: {condition.agentType}");
        Debug.Log($"[ExperimentConfig]   BiasCondition: {condition.biasCondition}");
        Debug.Log($"[ExperimentConfig]   InformationStrategy: {condition.informationStrategy}");
        Debug.Log($"[ExperimentConfig]   OverrideBias: {condition.overrideBias}");
        Debug.Log($"[ExperimentConfig] ----------------------------------------");
    }

    /// <summary>
    /// 単一条件の完了時（バッチモードでOnExperimentCompleteから呼び出し）
    /// </summary>
    private void OnConditionComplete()
    {
        // 現在条件の結果を保存
        float avgRate = CalculateAverageEvacuationRate();
        float stdDev = CalculateStdDev();

        var conditionResult = new BatchConditionResult
        {
            conditionId = _batchConditions[_currentConditionIndex].conditionId,
            trialResults = new List<ExperimentTrialResult>(_trialResults),
            averageEvacuationRate = avgRate,
            stdDev = stdDev
        };
        _batchResults.Add(conditionResult);

        // 条件別CSVを出力
        ExportConditionResultsToCSV(_batchConditions[_currentConditionIndex], conditionResult);

        _currentConditionIndex++;

        if (_currentConditionIndex < _batchConditions.Count)
        {
            // 次の条件へ
            Debug.Log($"[ExperimentConfig] 条件 {_currentConditionIndex}/{_batchConditions.Count} 完了");
            Debug.Log($"[ExperimentConfig] 次の条件: {_batchConditions[_currentConditionIndex].conditionId}");

            ResetExperiment();
            ApplyCondition(_batchConditions[_currentConditionIndex]);
            StartExperiment();

            // エピソードをリスタート
            if (_envManager != null)
            {
                _envManager.OnEpisodeBegin();
            }
        }
        else
        {
            // 全条件完了
            OnBatchExperimentComplete();
        }
    }

    /// <summary>
    /// バッチ実験全体の完了
    /// </summary>
    private void OnBatchExperimentComplete()
    {
        Debug.Log($"[ExperimentConfig] ========================================");
        Debug.Log($"[ExperimentConfig] バッチ実験完了！");
        Debug.Log($"[ExperimentConfig] 全{_batchConditions.Count}条件が終了しました");
        Debug.Log($"[ExperimentConfig] ========================================");

        GenerateBatchReport();
        ExportBatchSummaryToCSV();
        ExecuteEndAction();
    }

    /// <summary>
    /// バッチ実験のサマリーレポートを生成
    /// </summary>
    private void GenerateBatchReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n========== バッチ実験サマリーレポート ==========");
        sb.AppendLine($"プリセット: {BatchPreset}");
        sb.AppendLine($"条件数: {_batchConditions.Count}");
        sb.AppendLine($"各条件試行数: {TrialsPerCondition}");
        sb.AppendLine($"総試行数: {_batchConditions.Count * TrialsPerCondition}");
        sb.AppendLine($"------------------------------------------");

        for (int i = 0; i < _batchResults.Count; i++)
        {
            var result = _batchResults[i];
            var condition = _batchConditions[i];
            sb.AppendLine($"条件 {result.conditionId}:");
            sb.AppendLine($"  Agent: {condition.agentType}, Bias: {condition.biasCondition}, Info: {condition.informationStrategy}");
            sb.AppendLine($"  平均避難率: {result.averageEvacuationRate:P2}, 標準偏差: {result.stdDev:P2}");
        }

        sb.AppendLine("==========================================\n");
        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// バッチ実験のサマリーをCSVに出力
    /// </summary>
    private void ExportBatchSummaryToCSV()
    {
        string fileName = $"batch_{BatchPreset}_{_batchTimestamp}_summary.csv";
        string directoryPath = Path.Combine(Application.dataPath, "..", "Logs", "experiment_results");

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var sb = new StringBuilder();
        sb.AppendLine("condition_id,agent_type,bias_condition,info_strategy,avg_evacuation_rate,std_dev,trial_count");

        for (int i = 0; i < _batchResults.Count; i++)
        {
            var result = _batchResults[i];
            var condition = _batchConditions[i];

            sb.AppendLine($"{result.conditionId},{condition.agentType},{condition.biasCondition}," +
                          $"{condition.informationStrategy},{result.averageEvacuationRate:F4}," +
                          $"{result.stdDev:F4},{result.trialResults.Count}");
        }

        string filePath = Path.Combine(directoryPath, fileName);
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"[ExperimentConfig] バッチサマリーをCSVに出力: {filePath}");
    }

    /// <summary>
    /// 条件別の詳細結果をCSVに出力
    /// </summary>
    private void ExportConditionResultsToCSV(ExperimentCondition condition, BatchConditionResult result)
    {
        string fileName = $"{condition.conditionId}_{_batchTimestamp}_results.csv";
        string directoryPath = Path.Combine(Application.dataPath, "..", "Logs", "experiment_results");

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var sb = new StringBuilder();
        sb.AppendLine("trial_index,evacuation_rate,elapsed_time,timestamp");

        foreach (var trial in result.trialResults)
        {
            sb.AppendLine($"{trial.trialIndex},{trial.evacuationRate:F4},{trial.elapsedTime:F2},{trial.timestamp}");
        }

        string filePath = Path.Combine(directoryPath, fileName);
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"[ExperimentConfig] 条件別結果をCSVに出力: {filePath}");
    }

    /// <summary>
    /// 平均避難率を計算
    /// </summary>
    private float CalculateAverageEvacuationRate()
    {
        if (_trialResults.Count == 0) return 0f;

        float total = 0f;
        foreach (var result in _trialResults)
        {
            total += result.evacuationRate;
        }
        return total / _trialResults.Count;
    }

    /// <summary>
    /// 標準偏差を計算
    /// </summary>
    private float CalculateStdDev()
    {
        if (_trialResults.Count == 0) return 0f;

        float avg = CalculateAverageEvacuationRate();
        float variance = 0f;
        foreach (var result in _trialResults)
        {
            variance += (result.evacuationRate - avg) * (result.evacuationRate - avg);
        }
        return Mathf.Sqrt(variance / _trialResults.Count);
    }

    /// <summary>
    /// バッチモードが有効かどうか
    /// </summary>
    public bool IsBatchMode => BatchPreset != BatchExperimentPreset.None;

    /// <summary>
    /// 現在の条件番号（1始まり）
    /// </summary>
    public int CurrentConditionNumber => _currentConditionIndex + 1;

    /// <summary>
    /// 総条件数
    /// </summary>
    public int TotalConditions => _batchConditions?.Count ?? 0;

    #endregion
}
