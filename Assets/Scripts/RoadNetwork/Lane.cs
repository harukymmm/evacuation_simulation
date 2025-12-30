using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadNetwork
{
    /// <summary>
    /// 車線の方向
    /// </summary>
    public enum LaneDirection
    {
        Forward,    // 道路の進行方向
        Backward    // 道路の逆方向
    }

    /// <summary>
    /// 車線のタイプ
    /// </summary>
    public enum LaneType
    {
        Regular,    // 通常車線
        TurnLeft,   // 左折専用
        TurnRight,  // 右折専用
        Bus,        // バス専用
        Emergency   // 緊急車両専用
    }

    /// <summary>
    /// 車線データ構造
    /// 道路区間(RoadSegment)内の個別車線を表現
    /// </summary>
    [Serializable]
    public class Lane
    {
        [Header("車線識別")]
        [Tooltip("車線インデックス（0 = 最も左側）")]
        public int laneIndex;

        [Tooltip("車線の方向")]
        public LaneDirection direction = LaneDirection.Forward;

        [Tooltip("車線のタイプ")]
        public LaneType type = LaneType.Regular;

        [Header("車線ジオメトリ")]
        [Tooltip("車線幅（メートル）")]
        public float width = 3.0f;

        [Tooltip("車線中心線のウェイポイント")]
        public List<Vector3> waypoints = new List<Vector3>();

        [Tooltip("車線の長さ（メートル）")]
        public float length;

        [Header("交通状態")]
        [Tooltip("車線内の車両リスト")]
        [NonSerialized]
        public List<GameObject> vehiclesInLane = new List<GameObject>();

        [Tooltip("占有率（0-1）")]
        [Range(0f, 1f)]
        public float occupancyRate;

        /// <summary>
        /// 車線の開始位置を取得
        /// </summary>
        public Vector3 StartPosition => waypoints.Count > 0 ? waypoints[0] : Vector3.zero;

        /// <summary>
        /// 車線の終了位置を取得
        /// </summary>
        public Vector3 EndPosition => waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : Vector3.zero;

        /// <summary>
        /// 車線の長さを計算して更新
        /// </summary>
        public void CalculateLength()
        {
            length = 0f;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                length += Vector3.Distance(waypoints[i], waypoints[i + 1]);
            }
        }

        /// <summary>
        /// 車線上の位置（0-1）からワールド座標を取得
        /// </summary>
        /// <param name="t">車線上の位置（0 = 開始, 1 = 終了）</param>
        /// <returns>ワールド座標</returns>
        public Vector3 GetPositionAtT(float t)
        {
            if (waypoints.Count == 0) return Vector3.zero;
            if (waypoints.Count == 1) return waypoints[0];

            t = Mathf.Clamp01(t);
            float targetDistance = length * t;
            float accumulatedDistance = 0f;

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(waypoints[i], waypoints[i + 1]);
                if (accumulatedDistance + segmentLength >= targetDistance)
                {
                    float segmentT = (targetDistance - accumulatedDistance) / segmentLength;
                    return Vector3.Lerp(waypoints[i], waypoints[i + 1], segmentT);
                }
                accumulatedDistance += segmentLength;
            }

            return waypoints[waypoints.Count - 1];
        }

        /// <summary>
        /// 車線上の位置（0-1）から進行方向を取得
        /// </summary>
        /// <param name="t">車線上の位置（0 = 開始, 1 = 終了）</param>
        /// <returns>正規化された進行方向ベクトル</returns>
        public Vector3 GetDirectionAtT(float t)
        {
            if (waypoints.Count < 2) return Vector3.forward;

            t = Mathf.Clamp01(t);
            float targetDistance = length * t;
            float accumulatedDistance = 0f;

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(waypoints[i], waypoints[i + 1]);
                if (accumulatedDistance + segmentLength >= targetDistance || i == waypoints.Count - 2)
                {
                    return (waypoints[i + 1] - waypoints[i]).normalized;
                }
                accumulatedDistance += segmentLength;
            }

            return (waypoints[waypoints.Count - 1] - waypoints[waypoints.Count - 2]).normalized;
        }

        /// <summary>
        /// ワールド座標から車線上の最近傍点の位置（0-1）を取得
        /// </summary>
        /// <param name="worldPosition">ワールド座標</param>
        /// <returns>車線上の位置（0-1）</returns>
        public float GetTFromWorldPosition(Vector3 worldPosition)
        {
            if (waypoints.Count < 2) return 0f;

            float minDistance = float.MaxValue;
            float resultT = 0f;
            float accumulatedDistance = 0f;

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector3 segmentStart = waypoints[i];
                Vector3 segmentEnd = waypoints[i + 1];
                float segmentLength = Vector3.Distance(segmentStart, segmentEnd);

                // セグメント上の最近傍点を計算
                Vector3 segmentDir = (segmentEnd - segmentStart).normalized;
                float projection = Vector3.Dot(worldPosition - segmentStart, segmentDir);
                projection = Mathf.Clamp(projection, 0f, segmentLength);

                Vector3 closestPoint = segmentStart + segmentDir * projection;
                float distance = Vector3.Distance(worldPosition, closestPoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    resultT = (accumulatedDistance + projection) / length;
                }

                accumulatedDistance += segmentLength;
            }

            return Mathf.Clamp01(resultT);
        }

        /// <summary>
        /// 車線に車両を追加
        /// </summary>
        public void AddVehicle(GameObject vehicle)
        {
            if (!vehiclesInLane.Contains(vehicle))
            {
                vehiclesInLane.Add(vehicle);
                UpdateOccupancyRate();
            }
        }

        /// <summary>
        /// 車線から車両を削除
        /// </summary>
        public void RemoveVehicle(GameObject vehicle)
        {
            if (vehiclesInLane.Remove(vehicle))
            {
                UpdateOccupancyRate();
            }
        }

        /// <summary>
        /// 占有率を更新
        /// 1台あたり約7.5m（車体長4.5m + 車間3m）として計算
        /// </summary>
        private void UpdateOccupancyRate()
        {
            const float vehicleSpace = 7.5f; // 1台あたりの占有スペース（メートル）
            float maxVehicles = length / vehicleSpace;
            occupancyRate = maxVehicles > 0 ? Mathf.Clamp01(vehiclesInLane.Count / maxVehicles) : 0f;
        }

        /// <summary>
        /// 指定位置の前方にいる最も近い車両を取得
        /// </summary>
        /// <param name="currentT">現在位置（0-1）</param>
        /// <param name="currentVehicle">自車（検索から除外）</param>
        /// <returns>前方車両のGameObject、なければnull</returns>
        public GameObject GetLeadVehicle(float currentT, GameObject currentVehicle)
        {
            GameObject leadVehicle = null;
            float minTDiff = float.MaxValue;

            foreach (var vehicle in vehiclesInLane)
            {
                if (vehicle == currentVehicle || vehicle == null) continue;

                // 車両の位置をT値で取得（Vehicleコンポーネントから取得する想定）
                // 暫定的にTransformから計算
                float vehicleT = GetTFromWorldPosition(vehicle.transform.position);
                float tDiff = vehicleT - currentT;

                // 前方（tDiff > 0）で最も近い車両を探す
                if (tDiff > 0.001f && tDiff < minTDiff)
                {
                    minTDiff = tDiff;
                    leadVehicle = vehicle;
                }
            }

            return leadVehicle;
        }

        /// <summary>
        /// 指定位置の後方にいる最も近い車両を取得
        /// </summary>
        /// <param name="currentT">現在位置（0-1）</param>
        /// <param name="currentVehicle">自車（検索から除外）</param>
        /// <returns>後方車両のGameObject、なければnull</returns>
        public GameObject GetFollowingVehicle(float currentT, GameObject currentVehicle)
        {
            GameObject followingVehicle = null;
            float minTDiff = float.MaxValue;

            foreach (var vehicle in vehiclesInLane)
            {
                if (vehicle == currentVehicle || vehicle == null) continue;

                float vehicleT = GetTFromWorldPosition(vehicle.transform.position);
                float tDiff = currentT - vehicleT;

                // 後方（tDiff > 0）で最も近い車両を探す
                if (tDiff > 0.001f && tDiff < minTDiff)
                {
                    minTDiff = tDiff;
                    followingVehicle = vehicle;
                }
            }

            return followingVehicle;
        }
    }
}
