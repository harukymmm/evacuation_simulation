using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RoadNetwork;
using Vehicle;
using Traffic;

/// <summary>
/// 車両避難シミュレーションの環境管理クラス
/// EnvManager（歩行者用）と並列構造で車両のスポーン・管理を担当
/// </summary>
public class VehicleEnvManager : MonoBehaviour
{
    [Header("車両設定")]
    [Tooltip("スポーンする車両数")]
    public int spawnVehicleCount = 10;

    [Tooltip("車両プレハブ")]
    public GameObject vehiclePrefab;

    [Tooltip("車両のスポーンポイント")]
    public List<VehicleSpawnPoint> spawnPoints = new List<VehicleSpawnPoint>();

    [Header("シミュレーション設定")]
    [Tooltip("シミュレーションモード")]
    public SpawnMode spawnMode = SpawnMode.Random;

    [Tooltip("ランダムスポーンの中心")]
    public Vector3 spawnCenter = Vector3.zero;

    [Tooltip("ランダムスポーンの半径")]
    public float spawnRadius = 100f;

    [Tooltip("シミュレーション時間の倍率")]
    public float timeScale = 1f;

    [Tooltip("シミュレーション時間の上限（秒）")]
    public float maxSimulationTime = 600f;

    [Header("避難所設定")]
    [Tooltip("避難所リスト")]
    public List<GameObject> shelters = new List<GameObject>();

    [Header("オブジェクト管理")]
    [Tooltip("生成された車両リスト")]
    public List<Vehicle.Vehicle> vehicles = new List<Vehicle.Vehicle>();

    [Header("統計情報")]
    [Tooltip("現在のシミュレーション時間")]
    public float currentTimeSec = 0f;

    [Tooltip("車両避難率")]
    [Range(0f, 1f)]
    public float vehicleEvacuationRate = 0f;

    [Tooltip("避難完了した乗客数")]
    public int evacuatedPassengerCount = 0;

    [Tooltip("総乗客数")]
    public int totalPassengerCount = 0;

    [Header("参照")]
    [Tooltip("道路ネットワークマネージャー")]
    public RoadNetworkManager roadNetwork;

    [Tooltip("交通密度計算機")]
    public TrafficDensityCalculator densityCalculator;

    [Header("イベント")]
    public UnityEvent OnEpisodeBeginEvent;
    public UnityEvent OnEpisodeEndEvent;

    /// <summary>
    /// スポーンモード
    /// </summary>
    public enum SpawnMode
    {
        Random,     // ランダム位置
        SpawnPoints // 指定スポーンポイント
    }

    // エピソード管理
    private int _episodeCount = 0;
    private bool _isRunning = false;

    // 毎秒の避難率記録
    private List<float> _evacuationRateHistory = new List<float>();
    private float _lastRecordTime = 0f;

    private void Awake()
    {
        // 参照を取得
        if (roadNetwork == null)
        {
            roadNetwork = RoadNetworkManager.Instance;
        }

        if (densityCalculator == null)
        {
            densityCalculator = FindFirstObjectByType<TrafficDensityCalculator>();
        }

        // 避難所を検索
        if (shelters.Count == 0)
        {
            CollectShelters();
        }
    }

    private void Update()
    {
        if (!_isRunning) return;

        // シミュレーション時間を更新
        currentTimeSec += Time.deltaTime * timeScale;

        // 避難率を計算
        UpdateEvacuationRate();

        // 毎秒の避難率を記録
        if (currentTimeSec - _lastRecordTime >= 1f)
        {
            _evacuationRateHistory.Add(vehicleEvacuationRate);
            _lastRecordTime = currentTimeSec;
        }

        // 終了判定
        if (vehicleEvacuationRate >= 1f || currentTimeSec >= maxSimulationTime)
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// エピソードを開始
    /// </summary>
    public void OnEpisodeBegin()
    {
        _episodeCount++;
        Debug.Log($"[VehicleEnvManager] エピソード {_episodeCount} 開始");

        // 前回のエピソードをクリア
        Dispose();

        // 車両を生成
        Create();

        // 統計をリセット
        currentTimeSec = 0f;
        vehicleEvacuationRate = 0f;
        evacuatedPassengerCount = 0;
        _evacuationRateHistory.Clear();
        _lastRecordTime = 0f;

        _isRunning = true;
        OnEpisodeBeginEvent?.Invoke();
    }

    /// <summary>
    /// エピソードを終了
    /// </summary>
    public void EndEpisode()
    {
        _isRunning = false;
        Debug.Log($"[VehicleEnvManager] エピソード {_episodeCount} 終了 - 避難率: {vehicleEvacuationRate:P2}, 時間: {currentTimeSec:F1}秒");

        OnEpisodeEndEvent?.Invoke();
    }

    /// <summary>
    /// 車両を生成
    /// </summary>
    public void Create()
    {
        if (vehiclePrefab == null)
        {
            Debug.LogWarning("[VehicleEnvManager] 車両プレハブが設定されていません");
            return;
        }

        vehicles.Clear();
        totalPassengerCount = 0;

        for (int i = 0; i < spawnVehicleCount; i++)
        {
            Vector3 spawnPosition;
            RoadSegment spawnRoad;

            if (spawnMode == SpawnMode.SpawnPoints && spawnPoints.Count > 0)
            {
                // スポーンポイントから選択
                var spawnPoint = spawnPoints[i % spawnPoints.Count];
                spawnPosition = spawnPoint.GetRandomSpawnPosition();
                spawnRoad = roadNetwork?.GetNearestRoad(spawnPosition);
            }
            else
            {
                // ランダムスポーン
                (spawnPosition, spawnRoad) = GetRandomSpawnPositionOnRoad();
            }

            if (spawnRoad == null)
            {
                Debug.LogWarning($"[VehicleEnvManager] 車両 {i + 1} のスポーン位置に道路が見つかりません");
                continue;
            }

            // 車両を生成
            var vehicleObj = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity, transform);
            vehicleObj.name = $"Vehicle_{i + 1}";

            var vehicle = vehicleObj.GetComponent<Vehicle.Vehicle>();
            if (vehicle == null)
            {
                vehicle = vehicleObj.AddComponent<Vehicle.Vehicle>();
            }

            // 初期設定
            vehicle.SetVehicleId(i + 1);
            vehicle.currentRoad = spawnRoad;
            vehicle.currentLane = spawnRoad.GetNearestLane(spawnPosition, LaneDirection.Forward);
            vehicle.positionOnLane = vehicle.currentLane?.GetTFromWorldPosition(spawnPosition) ?? 0f;

            // 乗客数をランダム設定（1-5人）
            vehicle.currentPassengers = UnityEngine.Random.Range(1, vehicle.passengerCapacity + 1);
            totalPassengerCount += vehicle.currentPassengers;

            // 車線に登録
            vehicle.currentLane?.AddVehicle(vehicleObj);

            vehicles.Add(vehicle);

            // 最寄り避難所に向けて移動開始
            vehicle.MoveToNearestShelter();
        }

        Debug.Log($"[VehicleEnvManager] {vehicles.Count}台の車両を生成（総乗客数: {totalPassengerCount}人）");
    }

    /// <summary>
    /// ランダムな道路上のスポーン位置を取得
    /// </summary>
    private (Vector3 position, RoadSegment road) GetRandomSpawnPositionOnRoad()
    {
        if (roadNetwork == null || roadNetwork.roadSegments.Count == 0)
        {
            // 道路がない場合はランダム位置
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 position = spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            return (position, null);
        }

        // スポーン半径内の道路を取得
        var nearbyRoads = roadNetwork.GetRoadsInRadius(spawnCenter, spawnRadius);

        if (nearbyRoads.Count == 0)
        {
            // 範囲内に道路がない場合は全道路からランダム選択
            nearbyRoads = new List<RoadSegment>(roadNetwork.roadSegments);
        }

        if (nearbyRoads.Count == 0)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * spawnRadius;
            return (spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y), null);
        }

        // ランダムに道路を選択
        var selectedRoad = nearbyRoads[UnityEngine.Random.Range(0, nearbyRoads.Count)];

        // 道路上のランダムな位置を取得
        var lane = selectedRoad.GetNearestLane(selectedRoad.CenterPosition, LaneDirection.Forward);
        if (lane != null && lane.waypoints.Count > 0)
        {
            float t = UnityEngine.Random.value;
            Vector3 position = lane.GetPositionAtT(t);
            return (position, selectedRoad);
        }

        return (selectedRoad.CenterPosition, selectedRoad);
    }

    /// <summary>
    /// 前回のエピソードをクリア
    /// </summary>
    public void Dispose()
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle != null && vehicle.gameObject != null)
            {
                // 車線から削除
                vehicle.currentLane?.RemoveVehicle(vehicle.gameObject);
                Destroy(vehicle.gameObject);
            }
        }
        vehicles.Clear();
        totalPassengerCount = 0;
        evacuatedPassengerCount = 0;
    }

    /// <summary>
    /// 避難率を更新
    /// </summary>
    private void UpdateEvacuationRate()
    {
        int evacuatedCount = 0;
        evacuatedPassengerCount = 0;

        foreach (var vehicle in vehicles)
        {
            if (vehicle == null) continue;

            if (vehicle.state == VehicleState.Evacuated || !vehicle.gameObject.activeSelf)
            {
                evacuatedCount++;
                evacuatedPassengerCount += vehicle.currentPassengers;
            }
        }

        vehicleEvacuationRate = vehicles.Count > 0 ? (float)evacuatedCount / vehicles.Count : 0f;
    }

    /// <summary>
    /// 避難所を収集
    /// </summary>
    private void CollectShelters()
    {
        shelters.Clear();

        var shelterObjs = GameObject.FindGameObjectsWithTag("Shelter");
        foreach (var obj in shelterObjs)
        {
            shelters.Add(obj);
        }

        var constShelters = GameObject.FindGameObjectsWithTag("ConstShelter");
        foreach (var obj in constShelters)
        {
            shelters.Add(obj);
        }

        Debug.Log($"[VehicleEnvManager] {shelters.Count}件の避難所を検出");
    }

    /// <summary>
    /// 現在の避難率を取得
    /// </summary>
    public float GetVehicleEvacuationRate()
    {
        return vehicleEvacuationRate;
    }

    /// <summary>
    /// 乗客ベースの避難率を取得
    /// </summary>
    public float GetPassengerEvacuationRate()
    {
        return totalPassengerCount > 0 ? (float)evacuatedPassengerCount / totalPassengerCount : 0f;
    }

    /// <summary>
    /// アクティブな車両数を取得
    /// </summary>
    public int GetActiveVehicleCount()
    {
        int count = 0;
        foreach (var vehicle in vehicles)
        {
            if (vehicle != null && vehicle.gameObject.activeSelf &&
                vehicle.state != VehicleState.Evacuated)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 統計情報を取得
    /// </summary>
    public string GetStatistics()
    {
        int activeVehicles = GetActiveVehicleCount();
        int evacuatedVehicles = vehicles.Count - activeVehicles;

        return $"Vehicle Statistics:\n" +
               $"  Total: {vehicles.Count}\n" +
               $"  Active: {activeVehicles}\n" +
               $"  Evacuated: {evacuatedVehicles}\n" +
               $"  Evacuation Rate: {vehicleEvacuationRate:P2}\n" +
               $"  Passengers: {evacuatedPassengerCount}/{totalPassengerCount}\n" +
               $"  Time: {currentTimeSec:F1}s";
    }

    /// <summary>
    /// デバッグ用Gizmo描画
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // スポーン範囲を表示
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);

        // 車両位置を表示
        Gizmos.color = Color.blue;
        foreach (var vehicle in vehicles)
        {
            if (vehicle != null && vehicle.gameObject.activeSelf)
            {
                Gizmos.DrawWireSphere(vehicle.transform.position, 2f);
            }
        }
    }
}

/// <summary>
/// 車両スポーンポイント
/// </summary>
public class VehicleSpawnPoint : MonoBehaviour
{
    [Tooltip("スポーン半径")]
    public float radius = 10f;

    [Tooltip("スポーン数の割合")]
    [Range(0f, 1f)]
    public float spawnWeight = 1f;

    /// <summary>
    /// ランダムなスポーン位置を取得
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
        return transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
