using System.Collections.Generic;
using UnityEngine;
using RoadNetwork;

namespace Vehicle
{
    /// <summary>
    /// 車両用経路探索クラス
    /// RoadNetworkManager.FindPathのラッパーとして機能し、
    /// 車両固有の経路最適化を提供
    /// </summary>
    public class VehiclePathfinder : MonoBehaviour
    {
        [Header("経路探索設定")]
        [Tooltip("渋滞による再ルーティングの閾値（密度）")]
        public float congestionRerouteThreshold = 0.7f;

        [Tooltip("再ルーティングのクールダウン時間（秒）")]
        public float rerouteCooldown = 30f;

        [Tooltip("経路キャッシュを有効にするか")]
        public bool enablePathCaching = true;

        [Header("デバッグ")]
        [Tooltip("デバッグログを出力")]
        public bool debugLog = false;

        // 参照
        private Vehicle _vehicle;
        private RoadNetworkManager _roadNetwork;

        // 経路キャッシュ
        private struct PathCacheEntry
        {
            public Vector3 start;
            public Vector3 end;
            public List<RoadSegment> path;
            public float timestamp;
        }
        private List<PathCacheEntry> _pathCache = new List<PathCacheEntry>();
        private const float PathCacheLifetime = 60f; // キャッシュの有効期間（秒）
        private const float PathCacheDistanceThreshold = 10f; // キャッシュヒット判定の距離閾値

        // 再ルーティング管理
        private float _lastRerouteTime = 0f;

        private void Awake()
        {
            _vehicle = GetComponent<Vehicle>();
            _roadNetwork = RoadNetworkManager.Instance;
        }

        /// <summary>
        /// 2点間の経路を探索
        /// </summary>
        /// <param name="startPos">開始位置</param>
        /// <param name="endPos">終了位置</param>
        /// <returns>経路（道路セグメントのリスト）</returns>
        public List<RoadSegment> FindPath(Vector3 startPos, Vector3 endPos)
        {
            if (_roadNetwork == null || !_roadNetwork.IsBuilt)
            {
                Debug.LogWarning("[VehiclePathfinder] 道路ネットワークが構築されていません");
                return new List<RoadSegment>();
            }

            // キャッシュをチェック
            if (enablePathCaching)
            {
                var cachedPath = GetCachedPath(startPos, endPos);
                if (cachedPath != null)
                {
                    if (debugLog)
                    {
                        Debug.Log($"[VehiclePathfinder] キャッシュヒット: {cachedPath.Count}セグメント");
                    }
                    return cachedPath;
                }
            }

            // RoadNetworkManagerで経路探索
            var path = _roadNetwork.FindPath(startPos, endPos);

            // キャッシュに保存
            if (enablePathCaching && path.Count > 0)
            {
                CachePath(startPos, endPos, path);
            }

            if (debugLog)
            {
                Debug.Log($"[VehiclePathfinder] 経路探索完了: {path.Count}セグメント");
            }

            return path;
        }

        /// <summary>
        /// 避難所への経路を探索
        /// </summary>
        public List<RoadSegment> FindPathToShelter(Shelter shelter)
        {
            if (shelter == null)
            {
                Debug.LogWarning("[VehiclePathfinder] 避難所がnullです");
                return new List<RoadSegment>();
            }

            return FindPath(transform.position, shelter.transform.position);
        }

        /// <summary>
        /// 渋滞を考慮した経路再計算
        /// </summary>
        public List<RoadSegment> RerouteIfCongested()
        {
            if (_vehicle == null || _vehicle.plannedPath.Count == 0)
            {
                return new List<RoadSegment>();
            }

            // クールダウンチェック
            if (Time.time - _lastRerouteTime < rerouteCooldown)
            {
                return _vehicle.plannedPath;
            }

            // 現在の経路上の渋滞をチェック
            bool shouldReroute = false;
            for (int i = _vehicle.currentPathIndex; i < _vehicle.plannedPath.Count; i++)
            {
                var segment = _vehicle.plannedPath[i];
                if (segment != null)
                {
                    segment.UpdateTrafficDensity();
                    float density = segment.currentDensity / (segment.length / 1000f); // vehicles/km

                    // 飽和密度を超えている場合
                    if (density > congestionRerouteThreshold * 100f) // 100 vehicles/km = 飽和
                    {
                        shouldReroute = true;
                        break;
                    }
                }
            }

            if (shouldReroute)
            {
                if (debugLog)
                {
                    Debug.Log($"[VehiclePathfinder] 渋滞検出、再ルーティング中...");
                }

                // 現在位置から目的地への再経路探索
                var newPath = FindPath(transform.position, _vehicle.targetShelter.transform.position);

                if (newPath.Count > 0)
                {
                    // 新しい経路の方が短いか確認
                    float currentPathLength = CalculateRemainingPathLength(_vehicle.plannedPath, _vehicle.currentPathIndex);
                    float newPathLength = _roadNetwork.CalculatePathLength(newPath);

                    if (newPathLength < currentPathLength * 1.5f) // 1.5倍まで許容
                    {
                        _lastRerouteTime = Time.time;
                        return newPath;
                    }
                }
            }

            return _vehicle.plannedPath;
        }

        /// <summary>
        /// 残りの経路長を計算
        /// </summary>
        private float CalculateRemainingPathLength(List<RoadSegment> path, int currentIndex)
        {
            float length = 0f;
            for (int i = currentIndex; i < path.Count; i++)
            {
                if (path[i] != null)
                {
                    length += path[i].length;
                }
            }
            return length;
        }

        /// <summary>
        /// 経路の推定所要時間を計算
        /// </summary>
        public float EstimateTravelTime(List<RoadSegment> path)
        {
            if (_roadNetwork == null || path.Count == 0)
            {
                return 0f;
            }

            return _roadNetwork.CalculatePathTravelTime(path);
        }

        /// <summary>
        /// 経路の総延長を計算
        /// </summary>
        public float CalculatePathLength(List<RoadSegment> path)
        {
            if (_roadNetwork == null || path.Count == 0)
            {
                return 0f;
            }

            return _roadNetwork.CalculatePathLength(path);
        }

        /// <summary>
        /// 複数の目的地の中から最適な経路を持つものを選択
        /// </summary>
        public (Shelter shelter, List<RoadSegment> path) FindBestShelter(List<Shelter> candidates)
        {
            Shelter bestShelter = null;
            List<RoadSegment> bestPath = new List<RoadSegment>();
            float bestScore = float.MaxValue;

            foreach (var shelter in candidates)
            {
                if (shelter == null) continue;

                var path = FindPath(transform.position, shelter.transform.position);
                if (path.Count == 0) continue;

                // スコア計算：所要時間 + 容量ペナルティ
                float travelTime = EstimateTravelTime(path);
                float capacityPenalty = shelter.currentCapacity <= 0 ? 1000f : 0f;
                float score = travelTime + capacityPenalty;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestShelter = shelter;
                    bestPath = path;
                }
            }

            return (bestShelter, bestPath);
        }

        /// <summary>
        /// キャッシュから経路を取得
        /// </summary>
        private List<RoadSegment> GetCachedPath(Vector3 start, Vector3 end)
        {
            // 古いキャッシュを削除
            CleanExpiredCache();

            foreach (var entry in _pathCache)
            {
                if (Vector3.Distance(entry.start, start) < PathCacheDistanceThreshold &&
                    Vector3.Distance(entry.end, end) < PathCacheDistanceThreshold)
                {
                    return new List<RoadSegment>(entry.path);
                }
            }

            return null;
        }

        /// <summary>
        /// 経路をキャッシュに保存
        /// </summary>
        private void CachePath(Vector3 start, Vector3 end, List<RoadSegment> path)
        {
            _pathCache.Add(new PathCacheEntry
            {
                start = start,
                end = end,
                path = new List<RoadSegment>(path),
                timestamp = Time.time
            });

            // キャッシュサイズの制限
            while (_pathCache.Count > 100)
            {
                _pathCache.RemoveAt(0);
            }
        }

        /// <summary>
        /// 期限切れのキャッシュを削除
        /// </summary>
        private void CleanExpiredCache()
        {
            float currentTime = Time.time;
            _pathCache.RemoveAll(entry => currentTime - entry.timestamp > PathCacheLifetime);
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _pathCache.Clear();
        }

        /// <summary>
        /// 経路上の次の交差点を取得
        /// </summary>
        public Intersection GetNextIntersection()
        {
            if (_vehicle == null || _vehicle.currentRoad == null)
            {
                return null;
            }

            return _vehicle.currentRoad.endIntersection;
        }

        /// <summary>
        /// 次の交差点までの距離を取得
        /// </summary>
        public float GetDistanceToNextIntersection()
        {
            if (_vehicle == null || _vehicle.currentLane == null)
            {
                return float.MaxValue;
            }

            float remainingT = 1f - _vehicle.positionOnLane;
            return remainingT * _vehicle.currentLane.length;
        }

        /// <summary>
        /// 経路のデバッグ描画
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_vehicle == null || _vehicle.plannedPath == null) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < _vehicle.plannedPath.Count - 1; i++)
            {
                var current = _vehicle.plannedPath[i];
                var next = _vehicle.plannedPath[i + 1];

                if (current != null && next != null)
                {
                    Gizmos.DrawLine(current.EndPosition, next.StartPosition);
                }
            }
        }
    }
}
