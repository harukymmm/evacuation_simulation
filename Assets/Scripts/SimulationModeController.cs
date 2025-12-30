using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// シミュレーションモードの制御クラス
/// 歩行者モードと車両モードの切り替え、将来的な混合モードをサポート
/// </summary>
public class SimulationModeController : MonoBehaviour
{
    /// <summary>
    /// シミュレーションモード
    /// </summary>
    public enum SimulationMode
    {
        PedestrianOnly, // 歩行者のみ
        VehicleOnly,    // 車両のみ
        Mixed           // 混合（将来対応）
    }

    [Header("モード設定")]
    [Tooltip("現在のシミュレーションモード")]
    public SimulationMode currentMode = SimulationMode.PedestrianOnly;

    [Tooltip("開始時に自動でシミュレーションを開始するか")]
    public bool autoStart = false;

    [Header("マネージャー参照")]
    [Tooltip("歩行者環境マネージャー")]
    public EnvManager pedestrianEnvManager;

    [Tooltip("車両環境マネージャー")]
    public VehicleEnvManager vehicleEnvManager;

    [Header("統計")]
    [Tooltip("総合避難率")]
    [Range(0f, 1f)]
    public float overallEvacuationRate = 0f;

    [Tooltip("シミュレーション実行中かどうか")]
    public bool isRunning = false;

    [Header("イベント")]
    public UnityEvent<SimulationMode> OnModeChanged;
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationEnded;

    // 前回のモード
    private SimulationMode _previousMode;

    private void Awake()
    {
        // マネージャーを自動検索
        if (pedestrianEnvManager == null)
        {
            pedestrianEnvManager = FindFirstObjectByType<EnvManager>();
        }

        if (vehicleEnvManager == null)
        {
            vehicleEnvManager = FindFirstObjectByType<VehicleEnvManager>();
        }

        _previousMode = currentMode;
    }

    private void Start()
    {
        // 初期モードを適用
        ApplyMode(currentMode);

        if (autoStart)
        {
            StartSimulation();
        }
    }

    private void Update()
    {
        // モード変更を検出
        if (currentMode != _previousMode)
        {
            SetMode(currentMode);
            _previousMode = currentMode;
        }

        // 統合避難率を計算
        if (isRunning)
        {
            UpdateOverallEvacuationRate();
        }
    }

    /// <summary>
    /// シミュレーションモードを設定
    /// </summary>
    public void SetMode(SimulationMode mode)
    {
        if (isRunning)
        {
            Debug.LogWarning("[SimulationModeController] シミュレーション実行中はモード変更できません。先に停止してください。");
            return;
        }

        SimulationMode previousMode = currentMode;
        currentMode = mode;

        ApplyMode(mode);

        Debug.Log($"[SimulationModeController] モード変更: {previousMode} → {mode}");
        OnModeChanged?.Invoke(mode);
    }

    /// <summary>
    /// モードを適用
    /// </summary>
    private void ApplyMode(SimulationMode mode)
    {
        switch (mode)
        {
            case SimulationMode.PedestrianOnly:
                SetManagerActive(pedestrianEnvManager, true);
                SetManagerActive(vehicleEnvManager, false);
                break;

            case SimulationMode.VehicleOnly:
                SetManagerActive(pedestrianEnvManager, false);
                SetManagerActive(vehicleEnvManager, true);
                break;

            case SimulationMode.Mixed:
                SetManagerActive(pedestrianEnvManager, true);
                SetManagerActive(vehicleEnvManager, true);
                Debug.Log("[SimulationModeController] 混合モードは現在開発中です");
                break;
        }
    }

    /// <summary>
    /// マネージャーの有効/無効を設定
    /// </summary>
    private void SetManagerActive(MonoBehaviour manager, bool active)
    {
        if (manager != null)
        {
            manager.enabled = active;
        }
    }

    /// <summary>
    /// シミュレーションを開始
    /// </summary>
    public void StartSimulation()
    {
        if (isRunning)
        {
            Debug.LogWarning("[SimulationModeController] シミュレーションは既に実行中です");
            return;
        }

        isRunning = true;
        overallEvacuationRate = 0f;

        switch (currentMode)
        {
            case SimulationMode.PedestrianOnly:
                if (pedestrianEnvManager != null)
                {
                    pedestrianEnvManager.OnEpisodeBegin();
                }
                break;

            case SimulationMode.VehicleOnly:
                if (vehicleEnvManager != null)
                {
                    vehicleEnvManager.OnEpisodeBegin();
                }
                break;

            case SimulationMode.Mixed:
                if (pedestrianEnvManager != null)
                {
                    pedestrianEnvManager.OnEpisodeBegin();
                }
                if (vehicleEnvManager != null)
                {
                    vehicleEnvManager.OnEpisodeBegin();
                }
                break;
        }

        Debug.Log($"[SimulationModeController] シミュレーション開始: {currentMode}");
        OnSimulationStarted?.Invoke();
    }

    /// <summary>
    /// シミュレーションを停止
    /// </summary>
    public void StopSimulation()
    {
        if (!isRunning)
        {
            Debug.LogWarning("[SimulationModeController] シミュレーションは実行されていません");
            return;
        }

        isRunning = false;

        switch (currentMode)
        {
            case SimulationMode.PedestrianOnly:
                if (pedestrianEnvManager != null)
                {
                    pedestrianEnvManager.Dispose();
                }
                break;

            case SimulationMode.VehicleOnly:
                if (vehicleEnvManager != null)
                {
                    vehicleEnvManager.Dispose();
                }
                break;

            case SimulationMode.Mixed:
                if (pedestrianEnvManager != null)
                {
                    pedestrianEnvManager.Dispose();
                }
                if (vehicleEnvManager != null)
                {
                    vehicleEnvManager.Dispose();
                }
                break;
        }

        Debug.Log("[SimulationModeController] シミュレーション停止");
        OnSimulationEnded?.Invoke();
    }

    /// <summary>
    /// 統合避難率を更新
    /// </summary>
    private void UpdateOverallEvacuationRate()
    {
        switch (currentMode)
        {
            case SimulationMode.PedestrianOnly:
                if (pedestrianEnvManager != null)
                {
                    overallEvacuationRate = pedestrianEnvManager.GetCurrentEvacueeRate();
                }
                break;

            case SimulationMode.VehicleOnly:
                if (vehicleEnvManager != null)
                {
                    overallEvacuationRate = vehicleEnvManager.GetVehicleEvacuationRate();
                }
                break;

            case SimulationMode.Mixed:
                overallEvacuationRate = CalculateMixedEvacuationRate();
                break;
        }

        // 完了判定
        if (overallEvacuationRate >= 1f)
        {
            OnSimulationComplete();
        }
    }

    /// <summary>
    /// 混合モードの避難率を計算
    /// </summary>
    private float CalculateMixedEvacuationRate()
    {
        float pedRate = 0f;
        float vehRate = 0f;
        int pedCount = 0;
        int vehPassengerCount = 0;

        if (pedestrianEnvManager != null)
        {
            pedRate = pedestrianEnvManager.GetCurrentEvacueeRate();
            pedCount = pedestrianEnvManager.Evacuees?.Count ?? 0;
        }

        if (vehicleEnvManager != null)
        {
            vehRate = vehicleEnvManager.GetPassengerEvacuationRate();
            vehPassengerCount = vehicleEnvManager.totalPassengerCount;
        }

        int totalPopulation = pedCount + vehPassengerCount;
        if (totalPopulation == 0) return 0f;

        // 人口ベースの加重平均
        return (pedRate * pedCount + vehRate * vehPassengerCount) / totalPopulation;
    }

    /// <summary>
    /// シミュレーション完了時の処理
    /// </summary>
    private void OnSimulationComplete()
    {
        Debug.Log($"[SimulationModeController] シミュレーション完了: 避難率 {overallEvacuationRate:P2}");
        OnSimulationEnded?.Invoke();
    }

    /// <summary>
    /// 現在のシミュレーション時間を取得
    /// </summary>
    public float GetSimulationTime()
    {
        switch (currentMode)
        {
            case SimulationMode.PedestrianOnly:
                return pedestrianEnvManager?.CurrentTimeSec ?? 0f;

            case SimulationMode.VehicleOnly:
                return vehicleEnvManager?.currentTimeSec ?? 0f;

            case SimulationMode.Mixed:
                // より長い方を返す
                float pedTime = pedestrianEnvManager?.CurrentTimeSec ?? 0f;
                float vehTime = vehicleEnvManager?.currentTimeSec ?? 0f;
                return Mathf.Max(pedTime, vehTime);

            default:
                return 0f;
        }
    }

    /// <summary>
    /// 統計情報を取得
    /// </summary>
    public string GetStatistics()
    {
        string stats = $"Simulation Mode: {currentMode}\n";
        stats += $"Running: {isRunning}\n";
        stats += $"Overall Evacuation Rate: {overallEvacuationRate:P2}\n";
        stats += $"Time: {GetSimulationTime():F1}s\n\n";

        if (currentMode == SimulationMode.PedestrianOnly || currentMode == SimulationMode.Mixed)
        {
            if (pedestrianEnvManager != null)
            {
                stats += "--- Pedestrians ---\n";
                stats += $"  Count: {pedestrianEnvManager.Evacuees?.Count ?? 0}\n";
                stats += $"  Rate: {pedestrianEnvManager.GetCurrentEvacueeRate():P2}\n";
            }
        }

        if (currentMode == SimulationMode.VehicleOnly || currentMode == SimulationMode.Mixed)
        {
            if (vehicleEnvManager != null)
            {
                stats += "--- Vehicles ---\n";
                stats += vehicleEnvManager.GetStatistics();
            }
        }

        return stats;
    }

    /// <summary>
    /// モードを歩行者モードに切り替え
    /// </summary>
    public void SwitchToPedestrianMode()
    {
        SetMode(SimulationMode.PedestrianOnly);
    }

    /// <summary>
    /// モードを車両モードに切り替え
    /// </summary>
    public void SwitchToVehicleMode()
    {
        SetMode(SimulationMode.VehicleOnly);
    }

    /// <summary>
    /// モードを混合モードに切り替え
    /// </summary>
    public void SwitchToMixedMode()
    {
        SetMode(SimulationMode.Mixed);
    }
}
