using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoadNetwork
{
    /// <summary>
    /// 道路ネットワーク全体を管理するシングルトンマネージャー
    /// 道路グラフの構築、検索、経路探索機能を提供
    /// </summary>
    public class RoadNetworkManager : MonoBehaviour
    {
        private static RoadNetworkManager _instance;
        public static RoadNetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RoadNetworkManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("RoadNetworkManager");
                        _instance = go.AddComponent<RoadNetworkManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("道路ネットワーク")]
        [Tooltip("道路区間のリスト")]
        public List<RoadSegment> roadSegments = new List<RoadSegment>();

        [Tooltip("交差点のリスト")]
        public List<Intersection> intersections = new List<Intersection>();

        [Header("空間インデックス設定")]
        [Tooltip("空間グリッドのセルサイズ（メートル）")]
        public float cellSize = 50f;

        [Header("統計情報")]
        [Tooltip("総道路長（km）")]
        public float totalRoadLengthKm;

        [Tooltip("平均道路密度")]
        public float averageRoadDensity;

        // 道路IDによる辞書
        private Dictionary<string, RoadSegment> _roadById = new Dictionary<string, RoadSegment>();

        // 交差点IDによる辞書
        private Dictionary<string, Intersection> _intersectionById = new Dictionary<string, Intersection>();

        // 空間インデックス
        private Dictionary<Vector2Int, List<RoadSegment>> _spatialGrid = new Dictionary<Vector2Int, List<RoadSegment>>();

        // 交差点の空間インデックス
        private Dictionary<Vector2Int, List<Intersection>> _intersectionGrid = new Dictionary<Vector2Int, List<Intersection>>();

        // グラフ隣接リスト（経路探索用）
        private Dictionary<RoadSegment, List<RoadSegment>> _adjacencyList = new Dictionary<RoadSegment, List<RoadSegment>>();

        // ネットワーク構築済みフラグ
        private bool _isBuilt = false;
        public bool IsBuilt => _isBuilt;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        /// <summary>
        /// 道路ネットワークを構築（初期化）
        /// </summary>
        public void BuildNetwork()
        {
            Debug.Log("[RoadNetworkManager] 道路ネットワークの構築を開始...");

            // インデックスをクリア
            _roadById.Clear();
            _intersectionById.Clear();
            _spatialGrid.Clear();
            _intersectionGrid.Clear();
            _adjacencyList.Clear();

            // 道路をIDで登録
            foreach (var road in roadSegments)
            {
                if (road == null) continue;
                if (!string.IsNullOrEmpty(road.segmentId))
                {
                    _roadById[road.segmentId] = road;
                }
                AddToSpatialGrid(road);
            }

            // 交差点をIDで登録
            foreach (var intersection in intersections)
            {
                if (intersection == null) continue;
                if (!string.IsNullOrEmpty(intersection.intersectionId))
                {
                    _intersectionById[intersection.intersectionId] = intersection;
                }
                AddIntersectionToGrid(intersection);
            }

            // グラフを構築
            BuildGraph();

            // 統計を計算
            CalculateStatistics();

            _isBuilt = true;
            Debug.Log($"[RoadNetworkManager] 道路ネットワーク構築完了: {roadSegments.Count}道路, {intersections.Count}交差点, 総延長{totalRoadLengthKm:F2}km");
        }

        /// <summary>
        /// 道路を空間グリッドに追加
        /// </summary>
        private void AddToSpatialGrid(RoadSegment road)
        {
            foreach (var point in road.centerLine)
            {
                var cell = WorldToCell(point);
                if (!_spatialGrid.ContainsKey(cell))
                {
                    _spatialGrid[cell] = new List<RoadSegment>();
                }
                if (!_spatialGrid[cell].Contains(road))
                {
                    _spatialGrid[cell].Add(road);
                }
            }
        }

        /// <summary>
        /// 交差点を空間グリッドに追加
        /// </summary>
        private void AddIntersectionToGrid(Intersection intersection)
        {
            var cell = WorldToCell(intersection.position);
            if (!_intersectionGrid.ContainsKey(cell))
            {
                _intersectionGrid[cell] = new List<Intersection>();
            }
            if (!_intersectionGrid[cell].Contains(intersection))
            {
                _intersectionGrid[cell].Add(intersection);
            }
        }

        /// <summary>
        /// ワールド座標をグリッドセルに変換
        /// </summary>
        private Vector2Int WorldToCell(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / cellSize),
                Mathf.FloorToInt(worldPos.z / cellSize)
            );
        }

        /// <summary>
        /// グラフ（隣接リスト）を構築
        /// </summary>
        private void BuildGraph()
        {
            foreach (var road in roadSegments)
            {
                if (road == null) continue;
                _adjacencyList[road] = new List<RoadSegment>();

                // 交差点経由での接続を構築
                if (road.startIntersection != null)
                {
                    foreach (var connected in road.startIntersection.connectedRoads)
                    {
                        if (connected != road && !_adjacencyList[road].Contains(connected))
                        {
                            _adjacencyList[road].Add(connected);
                        }
                    }
                }

                if (road.endIntersection != null)
                {
                    foreach (var connected in road.endIntersection.connectedRoads)
                    {
                        if (connected != road && !_adjacencyList[road].Contains(connected))
                        {
                            _adjacencyList[road].Add(connected);
                        }
                    }
                }

                // connectedSegmentsからの直接接続も追加
                foreach (var connected in road.connectedSegments)
                {
                    if (connected != null && !_adjacencyList[road].Contains(connected))
                    {
                        _adjacencyList[road].Add(connected);
                    }
                }
            }
        }

        /// <summary>
        /// 統計情報を計算
        /// </summary>
        private void CalculateStatistics()
        {
            totalRoadLengthKm = 0f;
            foreach (var road in roadSegments)
            {
                if (road != null)
                {
                    totalRoadLengthKm += road.length / 1000f;
                }
            }

            // 平均密度を計算
            int totalVehicles = 0;
            foreach (var road in roadSegments)
            {
                if (road != null)
                {
                    totalVehicles += road.GetTotalVehicleCount();
                }
            }
            averageRoadDensity = totalRoadLengthKm > 0 ? totalVehicles / totalRoadLengthKm : 0f;
        }

        /// <summary>
        /// 道路を追加
        /// </summary>
        public void AddRoad(RoadSegment road)
        {
            if (road == null) return;
            if (!roadSegments.Contains(road))
            {
                roadSegments.Add(road);
                if (!string.IsNullOrEmpty(road.segmentId))
                {
                    _roadById[road.segmentId] = road;
                }
                AddToSpatialGrid(road);

                if (_isBuilt)
                {
                    // 隣接リストに追加
                    if (!_adjacencyList.ContainsKey(road))
                    {
                        _adjacencyList[road] = new List<RoadSegment>();
                    }
                }
            }
        }

        /// <summary>
        /// 交差点を追加
        /// </summary>
        public void AddIntersection(Intersection intersection)
        {
            if (intersection == null) return;
            if (!intersections.Contains(intersection))
            {
                intersections.Add(intersection);
                if (!string.IsNullOrEmpty(intersection.intersectionId))
                {
                    _intersectionById[intersection.intersectionId] = intersection;
                }
                AddIntersectionToGrid(intersection);
            }
        }

        /// <summary>
        /// IDから道路を取得
        /// </summary>
        public RoadSegment GetRoadById(string id)
        {
            return _roadById.TryGetValue(id, out var road) ? road : null;
        }

        /// <summary>
        /// IDから交差点を取得
        /// </summary>
        public Intersection GetIntersectionById(string id)
        {
            return _intersectionById.TryGetValue(id, out var intersection) ? intersection : null;
        }

        /// <summary>
        /// 指定位置に最も近い道路を取得
        /// </summary>
        public RoadSegment GetNearestRoad(Vector3 position, float maxDistance = 100f)
        {
            RoadSegment nearest = null;
            float minDist = maxDistance;

            // 空間グリッドから候補を取得
            var cell = WorldToCell(position);
            int searchRadius = Mathf.CeilToInt(maxDistance / cellSize) + 1;

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    var checkCell = new Vector2Int(cell.x + x, cell.y + z);
                    if (_spatialGrid.TryGetValue(checkCell, out var roads))
                    {
                        foreach (var road in roads)
                        {
                            if (road == null) continue;

                            // 道路上の最近傍点を計算
                            var nearestLane = road.GetNearestLane(position);
                            if (nearestLane == null) continue;

                            float t = nearestLane.GetTFromWorldPosition(position);
                            Vector3 closestPoint = nearestLane.GetPositionAtT(t);
                            float dist = Vector3.Distance(position, closestPoint);

                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearest = road;
                            }
                        }
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 指定位置に最も近い交差点を取得
        /// </summary>
        public Intersection GetNearestIntersection(Vector3 position, float maxDistance = 200f)
        {
            Intersection nearest = null;
            float minDist = maxDistance;

            var cell = WorldToCell(position);
            int searchRadius = Mathf.CeilToInt(maxDistance / cellSize) + 1;

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    var checkCell = new Vector2Int(cell.x + x, cell.y + z);
                    if (_intersectionGrid.TryGetValue(checkCell, out var inters))
                    {
                        foreach (var inter in inters)
                        {
                            if (inter == null) continue;
                            float dist = Vector3.Distance(position, inter.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                nearest = inter;
                            }
                        }
                    }
                }
            }

            return nearest;
        }

        /// <summary>
        /// 指定範囲内の道路を取得
        /// </summary>
        public List<RoadSegment> GetRoadsInRadius(Vector3 center, float radius)
        {
            var result = new HashSet<RoadSegment>();
            var cell = WorldToCell(center);
            int searchRadius = Mathf.CeilToInt(radius / cellSize) + 1;

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    var checkCell = new Vector2Int(cell.x + x, cell.y + z);
                    if (_spatialGrid.TryGetValue(checkCell, out var roads))
                    {
                        foreach (var road in roads)
                        {
                            if (road != null && road.ContainsPoint(center, radius))
                            {
                                result.Add(road);
                            }
                        }
                    }
                }
            }

            return result.ToList();
        }

        /// <summary>
        /// A*アルゴリズムで2点間の経路を探索
        /// </summary>
        public List<RoadSegment> FindPath(Vector3 startPos, Vector3 endPos)
        {
            var startRoad = GetNearestRoad(startPos);
            var endRoad = GetNearestRoad(endPos);

            if (startRoad == null || endRoad == null)
            {
                Debug.LogWarning("[RoadNetworkManager] 経路探索: 開始または終了位置の近くに道路が見つかりません");
                return new List<RoadSegment>();
            }

            return FindPath(startRoad, endRoad);
        }

        /// <summary>
        /// A*アルゴリズムで道路間の経路を探索
        /// </summary>
        public List<RoadSegment> FindPath(RoadSegment start, RoadSegment end)
        {
            if (start == end)
            {
                return new List<RoadSegment> { start };
            }

            var openSet = new SortedSet<(float fScore, RoadSegment road)>(
                Comparer<(float fScore, RoadSegment road)>.Create((a, b) =>
                {
                    int cmp = a.fScore.CompareTo(b.fScore);
                    if (cmp == 0) cmp = a.road.GetHashCode().CompareTo(b.road.GetHashCode());
                    return cmp;
                }));
            var cameFrom = new Dictionary<RoadSegment, RoadSegment>();
            var gScore = new Dictionary<RoadSegment, float>();
            var fScore = new Dictionary<RoadSegment, float>();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, end);
            openSet.Add((fScore[start], start));

            while (openSet.Count > 0)
            {
                var current = openSet.Min.road;
                openSet.Remove(openSet.Min);

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                if (!_adjacencyList.TryGetValue(current, out var neighbors))
                {
                    continue;
                }

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;

                    float tentativeGScore = gScore[current] + GetEdgeCost(current, neighbor);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                        // 既存のエントリを削除して再追加
                        openSet.RemoveWhere(x => x.road == neighbor);
                        openSet.Add((fScore[neighbor], neighbor));
                    }
                }
            }

            Debug.LogWarning("[RoadNetworkManager] 経路が見つかりませんでした");
            return new List<RoadSegment>();
        }

        /// <summary>
        /// ヒューリスティック関数（ユークリッド距離 / 最高速度）
        /// </summary>
        private float Heuristic(RoadSegment from, RoadSegment to)
        {
            float distance = Vector3.Distance(from.CenterPosition, to.CenterPosition);
            float maxSpeed = 80f / 3.6f; // 80 km/h を m/s に変換
            return distance / maxSpeed;
        }

        /// <summary>
        /// エッジコスト（道路間の移動コスト）を計算
        /// 距離、渋滞、制限速度を考慮
        /// </summary>
        private float GetEdgeCost(RoadSegment from, RoadSegment to)
        {
            // 基本コスト：距離 / 速度
            float distance = from.length;
            float speedKmh = Mathf.Min(from.speedLimit, to.speedLimit);

            // 渋滞による速度低下を考慮
            float congestionFactor = 1f + from.currentDensity * 0.1f;
            float effectiveSpeed = speedKmh / congestionFactor;
            effectiveSpeed = Mathf.Max(effectiveSpeed, 5f); // 最低5km/h

            float speedMs = effectiveSpeed / 3.6f;
            float baseCost = distance / speedMs;

            // 交差点での待ち時間を追加
            if (from.endIntersection != null && from.endIntersection.hasSignal)
            {
                baseCost += 30f; // 信号待ちの平均時間
            }

            return baseCost;
        }

        /// <summary>
        /// 経路を再構築
        /// </summary>
        private List<RoadSegment> ReconstructPath(Dictionary<RoadSegment, RoadSegment> cameFrom, RoadSegment current)
        {
            var path = new List<RoadSegment> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        /// <summary>
        /// 経路の総延長を計算
        /// </summary>
        public float CalculatePathLength(List<RoadSegment> path)
        {
            float totalLength = 0f;
            foreach (var road in path)
            {
                totalLength += road.length;
            }
            return totalLength;
        }

        /// <summary>
        /// 経路の推定所要時間を計算
        /// </summary>
        public float CalculatePathTravelTime(List<RoadSegment> path)
        {
            float totalTime = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                totalTime += GetEdgeCost(path[i], path[i + 1]);
            }
            if (path.Count > 0)
            {
                // 最後の道路の走行時間を追加
                var lastRoad = path[path.Count - 1];
                float speedMs = lastRoad.speedLimit / 3.6f;
                totalTime += lastRoad.length / speedMs;
            }
            return totalTime;
        }

        /// <summary>
        /// 交通密度を更新
        /// </summary>
        public void UpdateTrafficDensity()
        {
            foreach (var road in roadSegments)
            {
                if (road != null)
                {
                    road.UpdateTrafficDensity();
                }
            }
            CalculateStatistics();
        }

        /// <summary>
        /// デバッグ情報をログ出力
        /// </summary>
        public void LogDebugInfo()
        {
            Debug.Log($"[RoadNetworkManager] 道路数: {roadSegments.Count}");
            Debug.Log($"[RoadNetworkManager] 交差点数: {intersections.Count}");
            Debug.Log($"[RoadNetworkManager] 総延長: {totalRoadLengthKm:F2} km");
            Debug.Log($"[RoadNetworkManager] 平均密度: {averageRoadDensity:F2} vehicles/km");
            Debug.Log($"[RoadNetworkManager] 空間グリッドセル数: {_spatialGrid.Count}");
        }
    }
}
