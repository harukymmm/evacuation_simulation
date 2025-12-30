using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PLATEAU.CityInfo;

namespace RoadNetwork
{
    /// <summary>
    /// PLATEAUの道路データ（tran_*オブジェクト）から道路ネットワークを構築するビルダー
    /// </summary>
    public class RoadNetworkBuilder : MonoBehaviour
    {
        [Header("ビルド設定")]
        [Tooltip("シーン開始時に自動ビルドするか")]
        public bool buildOnStart = true;

        [Tooltip("RoadNetworkManagerへの参照（未設定の場合は自動検索）")]
        public RoadNetworkManager networkManager;

        [Header("道路抽出設定")]
        [Tooltip("道路オブジェクトのプレフィックス")]
        public string roadObjectPrefix = "tran_";

        [Tooltip("デフォルトの道路幅（属性から取得できない場合）")]
        public float defaultRoadWidth = 6.0f;

        [Tooltip("車両通行可能な最小道路幅（メートル）- この幅未満の道路はスキップ")]
        public float minRoadWidth = RoadSegment.MIN_VEHICLE_ROAD_WIDTH;

        [Tooltip("交差点判定の距離閾値（メートル）")]
        public float intersectionThreshold = 5.0f;

        [Header("デバッグ")]
        [Tooltip("デバッグログを出力するか")]
        public bool debugLog = true;

        [Tooltip("道路のGizmoを描画するか")]
        public bool drawGizmos = true;

        // 抽出された道路データ
        private List<ExtractedRoadData> _extractedRoads = new List<ExtractedRoadData>();

        // スキップされた道路数（統計用）
        private int _skippedNarrowRoadCount = 0;

        /// <summary>
        /// 抽出された道路データ（中間データ構造）
        /// </summary>
        private class ExtractedRoadData
        {
            public string id;
            public string name;
            public List<Vector3> vertices;
            public float width;
            public RoadType roadType;
            public bool isOneWay;
            public int laneCount;
            public float speedLimit;
            public GameObject sourceObject;
        }

        private void Start()
        {
            if (buildOnStart)
            {
                BuildRoadNetwork();
            }
        }

        /// <summary>
        /// 道路ネットワークを構築
        /// </summary>
        public void BuildRoadNetwork()
        {
            var startTime = Time.realtimeSinceStartup;
            Log("道路ネットワーク構築を開始...");

            // ネットワークマネージャーを取得
            if (networkManager == null)
            {
                networkManager = RoadNetworkManager.Instance;
            }

            // 既存データをクリア
            _extractedRoads.Clear();
            _skippedNarrowRoadCount = 0;
            networkManager.roadSegments.Clear();
            networkManager.intersections.Clear();

            // Step 1: PLATEAUの道路オブジェクトを抽出
            ExtractRoadObjects();

            // Step 2: 道路セグメントを生成
            CreateRoadSegments();

            // Step 3: 交差点を検出して生成
            DetectAndCreateIntersections();

            // Step 4: 道路と交差点を接続
            ConnectRoadsAndIntersections();

            // Step 5: ネットワークを構築
            networkManager.BuildNetwork();

            var elapsed = Time.realtimeSinceStartup - startTime;
            Log($"道路ネットワーク構築完了: {networkManager.roadSegments.Count}道路, {networkManager.intersections.Count}交差点 ({elapsed:F2}秒)");
            if (_skippedNarrowRoadCount > 0)
            {
                Log($"狭小道路スキップ: {_skippedNarrowRoadCount}件 (幅 < {minRoadWidth:F1}m)");
            }
        }

        /// <summary>
        /// PLATEAUの道路オブジェクトを抽出
        /// </summary>
        private void ExtractRoadObjects()
        {
            Log("PLATEAUオブジェクトから道路を抽出中...");

            // PLATEAUCityObjectGroupを検索
            var cityObjects = FindObjectsByType<PLATEAUCityObjectGroup>(FindObjectsSortMode.None);
            int extractedCount = 0;

            foreach (var cityObj in cityObjects)
            {
                // 道路オブジェクトのみを対象
                if (!cityObj.name.StartsWith(roadObjectPrefix))
                {
                    continue;
                }

                var extracted = ExtractRoadData(cityObj);
                if (extracted != null)
                {
                    _extractedRoads.Add(extracted);
                    extractedCount++;
                }
            }

            // PLATEAUオブジェクトがない場合は通常のMeshRendererも検索
            if (extractedCount == 0)
            {
                Log("PLATEAUCityObjectGroupが見つかりません。通常のGameObjectから道路を検索...");
                var allObjects = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

                foreach (var renderer in allObjects)
                {
                    if (!renderer.name.StartsWith(roadObjectPrefix))
                    {
                        continue;
                    }

                    var extracted = ExtractRoadDataFromMesh(renderer.gameObject);
                    if (extracted != null)
                    {
                        _extractedRoads.Add(extracted);
                        extractedCount++;
                    }
                }
            }

            Log($"{extractedCount}件の道路オブジェクトを抽出しました");
        }

        /// <summary>
        /// PLATEAUCityObjectGroupから道路データを抽出
        /// </summary>
        private ExtractedRoadData ExtractRoadData(PLATEAUCityObjectGroup cityObj)
        {
            var meshFilter = cityObj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return null;
            }

            var mesh = meshFilter.sharedMesh;
            var transform = cityObj.transform;

            // メッシュの頂点から中心線を抽出
            var centerLine = ExtractCenterLineFromMesh(mesh, transform);
            if (centerLine.Count < 2)
            {
                return null;
            }

            // 道路幅を推定
            float width = EstimateRoadWidth(mesh, transform);

            // 最小道路幅未満の場合はスキップ（車両通行不可）
            if (width < minRoadWidth)
            {
                _skippedNarrowRoadCount++;
                if (debugLog)
                {
                    Debug.Log($"[RoadNetworkBuilder] 道路幅が狭いためスキップ: {cityObj.name} (幅: {width:F1}m < {minRoadWidth:F1}m)");
                }
                return null;
            }

            // PLATEAU属性から道路情報を取得
            var (roadType, isOneWay, laneCount, speedLimit) = ExtractRoadAttributes(cityObj);

            return new ExtractedRoadData
            {
                id = cityObj.name,
                name = GetRoadName(cityObj),
                vertices = centerLine,
                width = width,
                roadType = roadType,
                isOneWay = isOneWay,
                laneCount = laneCount,
                speedLimit = speedLimit,
                sourceObject = cityObj.gameObject
            };
        }

        /// <summary>
        /// 通常のMeshRendererから道路データを抽出
        /// </summary>
        private ExtractedRoadData ExtractRoadDataFromMesh(GameObject obj)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return null;
            }

            var mesh = meshFilter.sharedMesh;
            var transform = obj.transform;

            var centerLine = ExtractCenterLineFromMesh(mesh, transform);
            if (centerLine.Count < 2)
            {
                return null;
            }

            float width = EstimateRoadWidth(mesh, transform);
            if (width <= 0)
            {
                width = defaultRoadWidth;
            }

            // 最小道路幅未満の場合はスキップ（車両通行不可）
            if (width < minRoadWidth)
            {
                _skippedNarrowRoadCount++;
                if (debugLog)
                {
                    Debug.Log($"[RoadNetworkBuilder] 道路幅が狭いためスキップ: {obj.name} (幅: {width:F1}m < {minRoadWidth:F1}m)");
                }
                return null;
            }

            return new ExtractedRoadData
            {
                id = obj.name,
                name = obj.name,
                vertices = centerLine,
                width = width,
                roadType = RoadType.MunicipalRoad,
                isOneWay = false,
                laneCount = 0,
                speedLimit = 40f,
                sourceObject = obj
            };
        }

        /// <summary>
        /// メッシュから道路の中心線を抽出
        /// PLATEAUの道路メッシュは通常、道路の形状に沿ったポリゴンを持つ
        /// </summary>
        private List<Vector3> ExtractCenterLineFromMesh(Mesh mesh, Transform transform)
        {
            var vertices = mesh.vertices;
            if (vertices.Length == 0)
            {
                return new List<Vector3>();
            }

            // ワールド座標に変換
            var worldVertices = new List<Vector3>();
            foreach (var v in vertices)
            {
                worldVertices.Add(transform.TransformPoint(v));
            }

            // バウンディングボックスを計算
            var bounds = new Bounds(worldVertices[0], Vector3.zero);
            foreach (var v in worldVertices)
            {
                bounds.Encapsulate(v);
            }

            // 道路の主軸方向を判定（長い方）
            bool isXLonger = bounds.size.x > bounds.size.z;

            // 道路に沿って頂点をグループ化してサンプリング
            var centerLine = new List<Vector3>();

            if (isXLonger)
            {
                // X方向に沿ってサンプリング
                centerLine = SampleCenterLineAlongAxis(worldVertices, bounds, true);
            }
            else
            {
                // Z方向に沿ってサンプリング
                centerLine = SampleCenterLineAlongAxis(worldVertices, bounds, false);
            }

            // 頂点数が少ない場合は端点のみを返す
            if (centerLine.Count < 2 && worldVertices.Count >= 2)
            {
                // 最も離れた2点を見つける
                float maxDist = 0;
                int idx1 = 0, idx2 = 1;
                for (int i = 0; i < worldVertices.Count; i++)
                {
                    for (int j = i + 1; j < worldVertices.Count; j++)
                    {
                        float dist = Vector3.Distance(worldVertices[i], worldVertices[j]);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            idx1 = i;
                            idx2 = j;
                        }
                    }
                }
                centerLine = new List<Vector3> { worldVertices[idx1], worldVertices[idx2] };
            }

            return centerLine;
        }

        /// <summary>
        /// 指定軸に沿って中心線をサンプリング
        /// </summary>
        private List<Vector3> SampleCenterLineAlongAxis(List<Vector3> vertices, Bounds bounds, bool alongX)
        {
            const int numSamples = 10; // サンプリング点数
            var centerLine = new List<Vector3>();

            float minVal = alongX ? bounds.min.x : bounds.min.z;
            float maxVal = alongX ? bounds.max.x : bounds.max.z;
            float range = maxVal - minVal;

            if (range < 0.1f)
            {
                return centerLine;
            }

            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / (numSamples - 1);
                float targetVal = minVal + range * t;

                // この位置に近い頂点を収集
                var nearbyVertices = new List<Vector3>();
                float tolerance = range / numSamples;

                foreach (var v in vertices)
                {
                    float val = alongX ? v.x : v.z;
                    if (Mathf.Abs(val - targetVal) < tolerance)
                    {
                        nearbyVertices.Add(v);
                    }
                }

                if (nearbyVertices.Count > 0)
                {
                    // 中心を計算
                    Vector3 center = Vector3.zero;
                    foreach (var v in nearbyVertices)
                    {
                        center += v;
                    }
                    center /= nearbyVertices.Count;
                    centerLine.Add(center);
                }
            }

            // 重複を除去
            var filtered = new List<Vector3>();
            for (int i = 0; i < centerLine.Count; i++)
            {
                if (i == 0 || Vector3.Distance(centerLine[i], filtered[filtered.Count - 1]) > 0.5f)
                {
                    filtered.Add(centerLine[i]);
                }
            }

            return filtered;
        }

        /// <summary>
        /// メッシュから道路幅を推定
        /// 注意: この値は推定値であり、後続のフィルタリングで最小幅チェックが行われる
        /// </summary>
        private float EstimateRoadWidth(Mesh mesh, Transform transform)
        {
            var vertices = mesh.vertices;
            if (vertices.Length < 2)
            {
                return defaultRoadWidth;
            }

            // バウンディングボックスの短い方の辺を道路幅とする
            var bounds = mesh.bounds;
            var worldSize = transform.TransformVector(bounds.size);
            float width = Mathf.Min(Mathf.Abs(worldSize.x), Mathf.Abs(worldSize.z));

            // 妥当な範囲にクランプ（下限は0として正確な値を保持、フィルタリングは呼び出し側で行う）
            return Mathf.Clamp(width, 0f, 50f);
        }

        /// <summary>
        /// PLATEAUオブジェクトから道路属性を抽出
        /// </summary>
        private (RoadType roadType, bool isOneWay, int laneCount, float speedLimit) ExtractRoadAttributes(PLATEAUCityObjectGroup cityObj)
        {
            RoadType roadType = RoadType.MunicipalRoad;
            bool isOneWay = false;
            int laneCount = 0;
            float speedLimit = 40f;

            try
            {
                // PLATEAUの属性情報を取得
                var primaryCityObject = cityObj.PrimaryCityObjects?.FirstOrDefault();
                if (primaryCityObject != null)
                {
                    var attributes = primaryCityObject.AttributesMap;

                    // 道路種別
                    if (attributes.TryGetValue("tran:function", out var funcAttr))
                    {
                        string funcValue = funcAttr.StringValue ?? "";
                        if (funcValue.Contains("高速") || funcValue.Contains("expressway"))
                        {
                            roadType = RoadType.Expressway;
                            speedLimit = 80f;
                        }
                        else if (funcValue.Contains("国道") || funcValue.Contains("national"))
                        {
                            roadType = RoadType.NationalRoad;
                            speedLimit = 60f;
                        }
                        else if (funcValue.Contains("県道") || funcValue.Contains("prefectural"))
                        {
                            roadType = RoadType.PrefecturalRoad;
                            speedLimit = 50f;
                        }
                    }

                    // 車線数
                    if (attributes.TryGetValue("tran:numberOfLanes", out var lanesAttr))
                    {
                        if (int.TryParse(lanesAttr.StringValue, out int lanes))
                        {
                            laneCount = lanes;
                        }
                    }

                    // 一方通行
                    if (attributes.TryGetValue("tran:trafficDirection", out var dirAttr))
                    {
                        string dirValue = dirAttr.StringValue ?? "";
                        isOneWay = dirValue.Contains("oneWay") || dirValue.Contains("一方");
                    }
                }
            }
            catch (Exception ex)
            {
                if (debugLog)
                {
                    Debug.LogWarning($"[RoadNetworkBuilder] 属性抽出エラー: {cityObj.name} - {ex.Message}");
                }
            }

            return (roadType, isOneWay, laneCount, speedLimit);
        }

        /// <summary>
        /// 道路名を取得
        /// </summary>
        private string GetRoadName(PLATEAUCityObjectGroup cityObj)
        {
            try
            {
                var primaryCityObject = cityObj.PrimaryCityObjects?.FirstOrDefault();
                if (primaryCityObject != null)
                {
                    var attributes = primaryCityObject.AttributesMap;
                    if (attributes.TryGetValue("gml:name", out var nameAttr))
                    {
                        return nameAttr.StringValue ?? cityObj.name;
                    }
                }
            }
            catch { }
            return cityObj.name;
        }

        /// <summary>
        /// 抽出されたデータからRoadSegmentを生成
        /// </summary>
        private void CreateRoadSegments()
        {
            Log("道路セグメントを生成中...");

            foreach (var data in _extractedRoads)
            {
                // 新しいGameObjectを作成
                var roadGO = new GameObject($"RoadSegment_{data.id}");
                roadGO.transform.parent = networkManager.transform;

                var roadSegment = roadGO.AddComponent<RoadSegment>();
                roadSegment.segmentId = data.id;
                roadSegment.roadName = data.name;
                roadSegment.centerLine = new List<Vector3>(data.vertices);
                roadSegment.width = data.width;
                roadSegment.roadType = data.roadType;
                roadSegment.isOneWay = data.isOneWay;
                roadSegment.speedLimit = data.speedLimit > 0 ? data.speedLimit : RoadSegment.GetDefaultSpeedLimit(data.roadType);

                // 道路の長さを計算
                roadSegment.CalculateLength();

                // 車線を生成
                if (data.laneCount > 0)
                {
                    // PLATEAU属性から車線数を使用
                    roadSegment.forwardLaneCount = data.isOneWay ? data.laneCount : data.laneCount / 2;
                    roadSegment.backwardLaneCount = data.isOneWay ? 0 : data.laneCount / 2;
                }
                roadSegment.BuildLanes();

                networkManager.AddRoad(roadSegment);
            }
        }

        /// <summary>
        /// 交差点を検出して生成
        /// </summary>
        private void DetectAndCreateIntersections()
        {
            Log("交差点を検出中...");

            // 道路端点を収集
            var endpoints = new List<(Vector3 position, RoadSegment road, bool isStart)>();

            foreach (var road in networkManager.roadSegments)
            {
                if (road.centerLine.Count >= 2)
                {
                    endpoints.Add((road.StartPosition, road, true));
                    endpoints.Add((road.EndPosition, road, false));
                }
            }

            // 近接する端点をクラスタリングして交差点を生成
            var used = new HashSet<int>();
            int intersectionCount = 0;

            for (int i = 0; i < endpoints.Count; i++)
            {
                if (used.Contains(i)) continue;

                var cluster = new List<(Vector3 position, RoadSegment road, bool isStart)> { endpoints[i] };
                used.Add(i);

                // 近傍の端点を収集
                for (int j = i + 1; j < endpoints.Count; j++)
                {
                    if (used.Contains(j)) continue;

                    if (Vector3.Distance(endpoints[i].position, endpoints[j].position) < intersectionThreshold)
                    {
                        cluster.Add(endpoints[j]);
                        used.Add(j);
                    }
                }

                // 2つ以上の道路が接続している場合は交差点を生成
                if (cluster.Count >= 2)
                {
                    // クラスタの中心位置を計算
                    Vector3 centerPos = Vector3.zero;
                    foreach (var ep in cluster)
                    {
                        centerPos += ep.position;
                    }
                    centerPos /= cluster.Count;

                    // 交差点を生成
                    var intersectionGO = new GameObject($"Intersection_{intersectionCount}");
                    intersectionGO.transform.parent = networkManager.transform;
                    intersectionGO.transform.position = centerPos;

                    var intersection = intersectionGO.AddComponent<Intersection>();
                    intersection.intersectionId = $"intersection_{intersectionCount}";
                    intersection.position = centerPos;
                    intersection.radius = intersectionThreshold;

                    // 接続道路を設定
                    foreach (var ep in cluster)
                    {
                        intersection.ConnectRoad(ep.road);

                        // 道路側にも交差点への参照を設定
                        if (ep.isStart)
                        {
                            ep.road.startIntersection = intersection;
                        }
                        else
                        {
                            ep.road.endIntersection = intersection;
                        }
                    }

                    networkManager.AddIntersection(intersection);
                    intersectionCount++;
                }
            }

            Log($"{intersectionCount}件の交差点を検出しました");
        }

        /// <summary>
        /// 道路と交差点を接続
        /// </summary>
        private void ConnectRoadsAndIntersections()
        {
            Log("道路と交差点を接続中...");

            foreach (var road in networkManager.roadSegments)
            {
                // connectedSegmentsを更新
                var connectedSet = new HashSet<RoadSegment>();

                if (road.startIntersection != null)
                {
                    foreach (var connected in road.startIntersection.connectedRoads)
                    {
                        if (connected != road)
                        {
                            connectedSet.Add(connected);
                        }
                    }
                }

                if (road.endIntersection != null)
                {
                    foreach (var connected in road.endIntersection.connectedRoads)
                    {
                        if (connected != road)
                        {
                            connectedSet.Add(connected);
                        }
                    }
                }

                road.connectedSegments = connectedSet.ToList();
            }
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        private void Log(string message)
        {
            if (debugLog)
            {
                Debug.Log($"[RoadNetworkBuilder] {message}");
            }
        }

        /// <summary>
        /// デバッグ用Gizmo描画
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!drawGizmos || networkManager == null) return;

            // 道路を描画
            Gizmos.color = Color.gray;
            foreach (var road in networkManager.roadSegments)
            {
                if (road == null || road.centerLine.Count < 2) continue;

                for (int i = 0; i < road.centerLine.Count - 1; i++)
                {
                    Gizmos.DrawLine(road.centerLine[i], road.centerLine[i + 1]);
                }
            }

            // 交差点を描画
            Gizmos.color = Color.yellow;
            foreach (var intersection in networkManager.intersections)
            {
                if (intersection != null)
                {
                    Gizmos.DrawWireSphere(intersection.position, intersection.radius);
                }
            }
        }
    }
}
