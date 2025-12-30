using System.Collections.Generic;
using UnityEngine;
using RoadNetwork;

/// <summary>
/// 避難所に紐づく駐車場管理クラス
/// </summary>
public class ParkingArea : MonoBehaviour
{
    [Header("駐車場設定")]
    [Tooltip("関連する避難所")]
    public Shelter associatedShelter;

    [Tooltip("最大駐車スポット数")]
    public int maxParkingSpots = 20;

    [Tooltip("1スポットあたりの面積（㎡）")]
    public float spotArea = 12.5f; // 2.5m x 5m

    [Tooltip("PLATEAUの面積から自動計算するか")]
    public bool autoCalculateCapacity = false;

    [Header("アクセス設定")]
    [Tooltip("アクセス道路")]
    public RoadSegment accessRoad;

    [Tooltip("入口位置")]
    public Transform entryPoint;

    [Tooltip("駐車スポット位置")]
    public List<Transform> parkingSpots = new List<Transform>();

    [Header("状態")]
    [Tooltip("現在の駐車台数")]
    public int currentOccupancy = 0;

    [Tooltip("駐車中の車両")]
    public List<Vehicle.Vehicle> parkedVehicles = new List<Vehicle.Vehicle>();

    /// <summary>
    /// 空きスポット数
    /// </summary>
    public int AvailableSpots => maxParkingSpots - currentOccupancy;

    /// <summary>
    /// 駐車可能かどうか
    /// </summary>
    public bool HasAvailableSpot => AvailableSpots > 0;

    private void Awake()
    {
        // 関連する避難所を取得
        if (associatedShelter == null)
        {
            associatedShelter = GetComponentInParent<Shelter>();
            if (associatedShelter == null)
            {
                associatedShelter = GetComponent<Shelter>();
            }
        }

        // 入口位置が未設定の場合は自身の位置を使用
        if (entryPoint == null)
        {
            entryPoint = transform;
        }

        // 自動計算が有効な場合
        if (autoCalculateCapacity)
        {
            CalculateCapacityFromArea();
        }
    }

    private void Start()
    {
        // アクセス道路を自動検出
        if (accessRoad == null)
        {
            var roadNetwork = RoadNetworkManager.Instance;
            if (roadNetwork != null)
            {
                accessRoad = roadNetwork.GetNearestRoad(entryPoint.position);
            }
        }

        // 避難所に駐車場を登録
        if (associatedShelter != null)
        {
            // Shelterに駐車場参照を設定（Shelter.csの修正が必要）
            Debug.Log($"[ParkingArea] {gameObject.name}: 駐車場を初期化 - 容量{maxParkingSpots}台, 避難所: {associatedShelter.displayName}");
        }
    }

    /// <summary>
    /// 面積から駐車場容量を計算
    /// </summary>
    private void CalculateCapacityFromArea()
    {
        // レンダラーのバウンドから面積を推定
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            var bounds = renderer.bounds;
            float area = bounds.size.x * bounds.size.z;
            maxParkingSpots = Mathf.Max(1, Mathf.FloorToInt(area / spotArea));
        }
    }

    /// <summary>
    /// 駐車スポットを割り当て
    /// </summary>
    /// <param name="vehicle">駐車する車両</param>
    /// <returns>割り当てられた駐車位置、満車の場合はVector3.zero</returns>
    public Vector3 AssignParkingSpot(Vehicle.Vehicle vehicle)
    {
        if (!HasAvailableSpot)
        {
            Debug.LogWarning($"[ParkingArea] {gameObject.name}: 満車です");
            return Vector3.zero;
        }

        // 利用可能なスポットを探す
        if (parkingSpots.Count > 0)
        {
            // 定義済みスポットから空きを探す
            for (int i = 0; i < parkingSpots.Count; i++)
            {
                if (parkingSpots[i] != null && !IsSpotOccupied(i))
                {
                    return parkingSpots[i].position;
                }
            }
        }

        // スポットが定義されていない場合はランダム位置を返す
        Vector2 randomOffset = Random.insideUnitCircle * 5f;
        return entryPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);
    }

    /// <summary>
    /// スポットが使用中かどうか
    /// </summary>
    private bool IsSpotOccupied(int spotIndex)
    {
        if (spotIndex >= parkingSpots.Count || parkingSpots[spotIndex] == null)
        {
            return false;
        }

        Vector3 spotPos = parkingSpots[spotIndex].position;
        foreach (var vehicle in parkedVehicles)
        {
            if (vehicle != null && Vector3.Distance(vehicle.transform.position, spotPos) < 2f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 車両の到着を処理
    /// </summary>
    /// <param name="vehicle">到着した車両</param>
    /// <returns>駐車成功したかどうか</returns>
    public bool VehicleArrived(Vehicle.Vehicle vehicle)
    {
        if (vehicle == null)
        {
            return false;
        }

        if (!HasAvailableSpot)
        {
            Debug.Log($"[ParkingArea] {gameObject.name}: 満車のため{vehicle.vehicleId}は駐車できません");
            return false;
        }

        // 駐車スポットを割り当て
        Vector3 parkingPosition = AssignParkingSpot(vehicle);
        if (parkingPosition == Vector3.zero)
        {
            return false;
        }

        // 車両を駐車状態に
        vehicle.transform.position = parkingPosition;
        vehicle.state = Vehicle.VehicleState.Parking;
        vehicle.currentSpeed = 0f;

        // 車線から削除
        vehicle.currentLane?.RemoveVehicle(vehicle.gameObject);

        // 駐車リストに追加
        parkedVehicles.Add(vehicle);
        currentOccupancy++;

        Debug.Log($"[ParkingArea] {gameObject.name}: {vehicle.vehicleId}が駐車しました（{currentOccupancy}/{maxParkingSpots}）");

        // 避難所への避難処理
        if (associatedShelter != null)
        {
            vehicle.Evacuation(associatedShelter);
        }

        return true;
    }

    /// <summary>
    /// 車両が出発
    /// </summary>
    public void VehicleDeparted(Vehicle.Vehicle vehicle)
    {
        if (vehicle == null) return;

        if (parkedVehicles.Remove(vehicle))
        {
            currentOccupancy--;
            Debug.Log($"[ParkingArea] {gameObject.name}: {vehicle.vehicleId}が出発しました（{currentOccupancy}/{maxParkingSpots}）");
        }
    }

    /// <summary>
    /// 駐車場情報を取得
    /// </summary>
    public string GetInfo()
    {
        return $"Parking: {currentOccupancy}/{maxParkingSpots} ({AvailableSpots} available)";
    }

    /// <summary>
    /// 入口位置を取得
    /// </summary>
    public Vector3 GetEntryPosition()
    {
        return entryPoint != null ? entryPoint.position : transform.position;
    }

    /// <summary>
    /// 入口までの距離を計算
    /// </summary>
    public float GetDistanceToEntry(Vector3 fromPosition)
    {
        return Vector3.Distance(fromPosition, GetEntryPosition());
    }

    /// <summary>
    /// トリガー領域に車両が入った時
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        var vehicle = other.GetComponent<Vehicle.Vehicle>();
        if (vehicle != null && vehicle.targetShelter == associatedShelter)
        {
            VehicleArrived(vehicle);
        }
    }

    /// <summary>
    /// デバッグ用Gizmo描画
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 駐車場エリア
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Gizmos.DrawCube(renderer.bounds.center, renderer.bounds.size);
        }

        // 入口
        if (entryPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(entryPoint.position, 1f);
        }

        // 駐車スポット
        Gizmos.color = Color.cyan;
        foreach (var spot in parkingSpots)
        {
            if (spot != null)
            {
                Gizmos.DrawWireCube(spot.position, new Vector3(2.5f, 0.5f, 5f));
            }
        }

        // 避難所への接続
        if (associatedShelter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, associatedShelter.transform.position);
        }
    }
}
