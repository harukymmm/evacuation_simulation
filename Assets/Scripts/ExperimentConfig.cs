using UnityEngine;

/// <summary>
/// 実験条件の設定を管理するクラス
/// UnityのInspectorから設定を変更可能
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

    [Header("Experiment Settings")]
    [Tooltip("使用するエージェントタイプ")]
    public AgentType SelectedAgentType = AgentType.LLM;

    [Tooltip("ランダムシード（-1で毎回異なるシード）")]
    public int RandomSeed = -1;

    [Tooltip("試行回数（1試行 = 1エピソード）")]
    public int NumberOfTrials = 10;

    [Header("Output Settings")]
    [Tooltip("結果をCSVに出力する")]
    public bool ExportResults = true;

    [Tooltip("実験名（出力ファイルの識別用）")]
    public string ExperimentName = "experiment_1";

    private EnvManager _envManager;
    private int _currentTrialIndex = 0;
    private bool _experimentStarted = false;

    public static ExperimentConfig Instance { get; private set; }

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
                  $"\n  - 実験名: {ExperimentName}");
    }

    void OnDestroy()
    {
        if (_envManager != null)
        {
            _envManager.OnEndEpisode -= OnTrialEnd;
        }
    }

    private void OnTrialEnd(float evacuationRate)
    {
        if (!_experimentStarted) return;

        _currentTrialIndex++;
        Debug.Log($"[ExperimentConfig] 試行 {_currentTrialIndex}/{NumberOfTrials} 完了 - 避難率: {evacuationRate:P1}");

        if (_currentTrialIndex >= NumberOfTrials)
        {
            Debug.Log($"[ExperimentConfig] 実験完了！全{NumberOfTrials}試行が終了しました");
            _experimentStarted = false;
        }
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

        // ランダムシードを再設定
        if (RandomSeed >= 0)
        {
            Random.InitState(RandomSeed);
        }
    }
}
