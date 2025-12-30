using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadNetwork
{
    /// <summary>
    /// 道路種別（PLATEAUの分類に対応）
    /// </summary>
    public enum RoadType
    {
        Expressway,       // 高速道路
        NationalRoad,     // 国道
        PrefecturalRoad,  // 県道
        MunicipalRoad,    // 市道
        PrivateRoad,      // 私道
        Other             // その他
    }

    /// <summary>
    /// 道路区間（RoadSegment）
    /// 交差点間の道路セグメントを表現
    /// </summary>
    public class RoadSegment : MonoBehaviour
    {
        /// <summary>
        /// 車両が通行可能な最小道路幅（メートル）
        /// 3m未満の道路は車両通行不可
        /// </summary>
        public const float MIN_VEHICLE_ROAD_WIDTH = 3.0f;

        /// <summary>
        /// 指定された道路幅が車両通行可能かどうかを判定
        /// </summary>
        /// <param name="roadWidth">道路幅（メートル）</param>
        /// <returns>車両通行可能な場合はtrue</returns>
        public static bool IsPassableForVehicles(float roadWidth)
        {
            return roadWidth >= MIN_VEHICLE_ROAD_WIDTH;
        }

        [Header("識別情報")]
        [Tooltip("道路区間ID（PLATEAU GML ID等）")]
        public string segmentId;

        [Tooltip("道路名")]
        public string roadName;

        [Header("道路ジオメトリ")]
        [Tooltip("道路中心線のウェイポイント")]
        public List<Vector3> centerLine = new List<Vector3>();

        [Tooltip("道路の長さ（メートル）")]
        public float length;

        [Tooltip("道路幅（メートル）")]
        public float width = 6.0f;

        [Header("車線構成")]
        [Tooltip("車線リスト")]
        public List<Lane> lanes = new List<Lane>();

        [Tooltip("順方向の車線数")]
        public int forwardLaneCount = 1;

        [Tooltip("逆方向の車線数")]
        public int backwardLaneCount = 1;

        [Header("接続情報")]
        [Tooltip("開始側の交差点")]
        public Intersection startIntersection;

        [Tooltip("終了側の交差点")]
        public Intersection endIntersection;

        [Tooltip("接続している道路区間")]
        public List<RoadSegment> connectedSegments = new List<RoadSegment>();

        [Header("道路属性")]
        [Tooltip("道路種別")]
        public RoadType roadType = RoadType.MunicipalRoad;

        [Tooltip("制限速度（km/h）")]
        public float speedLimit = 40f;

        [Tooltip("一方通行かどうか")]
        public bool isOneWay = false;

        [Header("交通状態")]
        [Tooltip("現在の交通密度（vehicles/km）")]
        public float currentDensity;

        [Tooltip("平均速度（km/h）")]
        public float averageSpeed;

        /// <summary>
        /// 道路の開始位置
        /// </summary>
        public Vector3 StartPosition => centerLine.Count > 0 ? centerLine[0] : transform.position;

        /// <summary>
        /// 道路の終了位置
        /// </summary>
        public Vector3 EndPosition => centerLine.Count > 0 ? centerLine[centerLine.Count - 1] : transform.position;

        /// <summary>
        /// 道路の中心位置
        /// </summary>
        public Vector3 CenterPosition
        {
            get
            {
                if (centerLine.Count == 0) return transform.position;
                if (centerLine.Count == 1) return centerLine[0];
                int midIndex = centerLine.Count / 2;
                return centerLine[midIndex];
            }
        }

        /// <summary>
        /// 道路種別から制限速度を取得
        /// </summary>
        public static float GetDefaultSpeedLimit(RoadType type)
        {
            return type switch
            {
                RoadType.Expressway => 80f,
                RoadType.NationalRoad => 60f,
                RoadType.PrefecturalRoad => 50f,
                RoadType.MunicipalRoad => 40f,
                RoadType.PrivateRoad => 30f,
                _ => 40f
            };
        }

        /// <summary>
        /// 道路幅から車線数を推定
        /// </summary>
        /// <param name="roadWidth">道路幅（メートル）</param>
        /// <param name="isOneWay">一方通行かどうか</param>
        /// <returns>(順方向車線数, 逆方向車線数) - 道路幅が最小幅未満の場合は(0, 0)</returns>
        public static (int forward, int backward) EstimateLaneCount(float roadWidth, bool isOneWay)
        {
            // 最小道路幅未満の場合は車両通行不可
            if (!IsPassableForVehicles(roadWidth))
            {
                return (0, 0);
            }

            const float laneWidth = 3.0f;        // 標準車線幅
            const float minLaneWidth = 2.5f;     // 最小車線幅
            const float sidewalkWidth = 1.5f;    // 歩道幅の推定

            // 歩道を除いた車道幅を推定
            float carriageWidth = Mathf.Max(roadWidth - sidewalkWidth * 2, roadWidth * 0.7f);

            // 車道幅も最小幅以上である必要がある
            if (carriageWidth < MIN_VEHICLE_ROAD_WIDTH)
            {
                return (0, 0);
            }

            int totalLanes = Mathf.Max(1, Mathf.FloorToInt(carriageWidth / minLaneWidth));

            if (isOneWay)
            {
                return (totalLanes, 0);
            }
            else
            {
                int halfLanes = Mathf.Max(1, totalLanes / 2);
                return (halfLanes, halfLanes);
            }
        }

        /// <summary>
        /// 中心線から道路の長さを計算
        /// </summary>
        public void CalculateLength()
        {
            length = 0f;
            for (int i = 0; i < centerLine.Count - 1; i++)
            {
                length += Vector3.Distance(centerLine[i], centerLine[i + 1]);
            }
        }

        /// <summary>
        /// 道路幅から車線を生成
        /// </summary>
        public void BuildLanes()
        {
            lanes.Clear();

            if (centerLine.Count < 2) return;

            var (forward, backward) = EstimateLaneCount(width, isOneWay);
            forwardLaneCount = forward;
            backwardLaneCount = backward;

            float laneWidth = width / (forwardLaneCount + backwardLaneCount);
            laneWidth = Mathf.Clamp(laneWidth, 2.5f, 4.0f);

            // 順方向車線を生成
            for (int i = 0; i < forwardLaneCount; i++)
            {
                var lane = CreateLane(i, LaneDirection.Forward, laneWidth, forwardLaneCount);
                lanes.Add(lane);
            }

            // 逆方向車線を生成
            for (int i = 0; i < backwardLaneCount; i++)
            {
                var lane = CreateLane(i, LaneDirection.Backward, laneWidth, backwardLaneCount);
                lanes.Add(lane);
            }
        }

        /// <summary>
        /// 車線を生成
        /// </summary>
        private Lane CreateLane(int index, LaneDirection direction, float laneWidth, int totalLanesInDirection)
        {
            var lane = new Lane
            {
                laneIndex = index,
                direction = direction,
                type = LaneType.Regular,
                width = laneWidth,
                waypoints = new List<Vector3>()
            };

            // 中心線からのオフセットを計算
            float totalOffset = (totalLanesInDirection - 1) * laneWidth / 2f;
            float laneOffset = index * laneWidth - totalOffset;

            // 逆方向の場合は反対側にオフセット
            if (direction == LaneDirection.Backward)
            {
                laneOffset = -laneOffset - laneWidth / 2f - width / 4f;
            }
            else
            {
                laneOffset = laneOffset + laneWidth / 2f + width / 4f;
            }

            // 中心線に沿って車線のウェイポイントを生成
            for (int i = 0; i < centerLine.Count; i++)
            {
                Vector3 tangent;
                if (i == 0)
                {
                    tangent = (centerLine[1] - centerLine[0]).normalized;
                }
                else if (i == centerLine.Count - 1)
                {
                    tangent = (centerLine[i] - centerLine[i - 1]).normalized;
                }
                else
                {
                    tangent = ((centerLine[i + 1] - centerLine[i]).normalized +
                              (centerLine[i] - centerLine[i - 1]).normalized).normalized;
                }

                // 法線（右方向）を計算
                Vector3 normal = Vector3.Cross(Vector3.up, tangent).normalized;
                Vector3 lanePoint = centerLine[i] + normal * laneOffset;

                // 逆方向の場合はウェイポイントの順序を逆にする
                if (direction == LaneDirection.Backward)
                {
                    lane.waypoints.Insert(0, lanePoint);
                }
                else
                {
                    lane.waypoints.Add(lanePoint);
                }
            }

            lane.CalculateLength();
            return lane;
        }

        /// <summary>
        /// 指定方向の車線を取得
        /// </summary>
        public List<Lane> GetLanesByDirection(LaneDirection direction)
        {
            return lanes.FindAll(l => l.direction == direction);
        }

        /// <summary>
        /// 指定位置に最も近い車線を取得
        /// </summary>
        public Lane GetNearestLane(Vector3 position, LaneDirection? preferredDirection = null)
        {
            Lane nearestLane = null;
            float minDistance = float.MaxValue;

            foreach (var lane in lanes)
            {
                if (preferredDirection.HasValue && lane.direction != preferredDirection.Value)
                    continue;

                float t = lane.GetTFromWorldPosition(position);
                Vector3 closestPoint = lane.GetPositionAtT(t);
                float distance = Vector3.Distance(position, closestPoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestLane = lane;
                }
            }

            return nearestLane;
        }

        /// <summary>
        /// 道路内の全車両数を取得
        /// </summary>
        public int GetTotalVehicleCount()
        {
            int count = 0;
            foreach (var lane in lanes)
            {
                count += lane.vehiclesInLane.Count;
            }
            return count;
        }

        /// <summary>
        /// 交通密度を更新（vehicles/km）
        /// </summary>
        public void UpdateTrafficDensity()
        {
            int vehicleCount = GetTotalVehicleCount();
            currentDensity = length > 0 ? (vehicleCount / (length / 1000f)) : 0f;
        }

        /// <summary>
        /// 道路がポイントを含むかどうかを判定（近傍判定）
        /// </summary>
        public bool ContainsPoint(Vector3 point, float tolerance = 5f)
        {
            foreach (var wp in centerLine)
            {
                if (Vector3.Distance(point, wp) < tolerance + width / 2f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// デバッグ用のGizmo描画
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 中心線を描画
            if (centerLine.Count > 1)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < centerLine.Count - 1; i++)
                {
                    Gizmos.DrawLine(centerLine[i], centerLine[i + 1]);
                }
            }

            // 車線を描画
            foreach (var lane in lanes)
            {
                Gizmos.color = lane.direction == LaneDirection.Forward ? Color.green : Color.red;
                if (lane.waypoints.Count > 1)
                {
                    for (int i = 0; i < lane.waypoints.Count - 1; i++)
                    {
                        Gizmos.DrawLine(lane.waypoints[i], lane.waypoints[i + 1]);
                    }
                }
            }
        }
    }
}
