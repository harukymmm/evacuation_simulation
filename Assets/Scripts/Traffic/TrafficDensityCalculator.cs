using System.Collections.Generic;
using UnityEngine;
using RoadNetwork;

namespace Traffic
{
    /// <summary>
    /// サービスレベル（Level of Service）
    /// 交通工学における道路の混雑度の指標
    /// </summary>
    public enum LevelOfService
    {
        A,  // 自由流動（密度 < 7 veh/km）
        B,  // 安定流動（密度 < 11 veh/km）
        C,  // 安定流動（密度 < 16 veh/km）
        D,  // 接近飽和（密度 < 22 veh/km）
        E,  // 不安定流動（密度 < 28 veh/km）
        F   // 強制流動・渋滞（密度 >= 28 veh/km）
    }

    /// <summary>
    /// 交通密度・渋滞計算クラス
    /// リアルタイムで道路ネットワークの交通状態を監視・計算
    /// </summary>
    public class TrafficDensityCalculator : MonoBehaviour
    {
        [Header("計算設定")]
        [Tooltip("更新間隔（秒）")]
        public float updateInterval = 1.0f;

        [Tooltip("渋滞判定の密度閾値（vehicles/km）")]
        public float congestionThreshold = 50f;

        [Tooltip("飽和密度（vehicles/km）")]
        public float saturationDensity = 100f;

        [Header("参照")]
        [Tooltip("道路ネットワークマネージャー")]
        public RoadNetworkManager roadNetwork;

        [Header("統計情報")]
        [Tooltip("ネットワーク全体の平均密度")]
        public float averageDensity;

        [Tooltip("渋滞中の道路数")]
        public int congestedRoadCount;

        [Tooltip("ネットワーク全体のサービスレベル")]
        public LevelOfService overallLOS = LevelOfService.A;

        // 道路ごとの交通データ
        private Dictionary<RoadSegment, TrafficData> _trafficData = new Dictionary<RoadSegment, TrafficData>();

        // タイマー
        private float _updateTimer = 0f;

        /// <summary>
        /// 道路の交通データ
        /// </summary>
        public class TrafficData
        {
            public float density;           // 密度（vehicles/km）
            public float flow;              // 流量（vehicles/hour）
            public float averageSpeed;      // 平均速度（km/h）
            public float travelTime;        // 所要時間（seconds）
            public LevelOfService los;      // サービスレベル
            public bool isCongested;        // 渋滞フラグ
            public float lastUpdateTime;    // 最終更新時刻
        }

        private void Awake()
        {
            if (roadNetwork == null)
            {
                roadNetwork = RoadNetworkManager.Instance;
            }
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;

            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateAllTrafficData();
            }
        }

        /// <summary>
        /// すべての道路の交通データを更新
        /// </summary>
        public void UpdateAllTrafficData()
        {
            if (roadNetwork == null || roadNetwork.roadSegments == null) return;

            float totalDensity = 0f;
            int validRoadCount = 0;
            congestedRoadCount = 0;

            foreach (var road in roadNetwork.roadSegments)
            {
                if (road == null) continue;

                var data = CalculateTrafficData(road);
                _trafficData[road] = data;

                // 道路オブジェクトにも反映
                road.currentDensity = data.density;
                road.averageSpeed = data.averageSpeed;

                totalDensity += data.density;
                validRoadCount++;

                if (data.isCongested)
                {
                    congestedRoadCount++;
                }
            }

            // ネットワーク全体の統計
            averageDensity = validRoadCount > 0 ? totalDensity / validRoadCount : 0f;
            overallLOS = DensityToLOS(averageDensity);
        }

        /// <summary>
        /// 道路の交通データを計算
        /// </summary>
        public TrafficData CalculateTrafficData(RoadSegment road)
        {
            var data = new TrafficData
            {
                lastUpdateTime = Time.time
            };

            if (road == null || road.length <= 0)
            {
                return data;
            }

            // 車両数を取得
            int vehicleCount = road.GetTotalVehicleCount();

            // 密度計算（vehicles/km）
            float lengthKm = road.length / 1000f;
            data.density = vehicleCount / Mathf.Max(lengthKm, 0.01f);

            // 平均速度を計算
            data.averageSpeed = CalculateAverageSpeed(road);

            // 流量を計算（vehicles/hour）
            // Q = k × v（流量 = 密度 × 速度）
            data.flow = data.density * data.averageSpeed;

            // 所要時間を計算（seconds）
            if (data.averageSpeed > 0)
            {
                data.travelTime = (road.length / 1000f) / (data.averageSpeed / 3600f);
            }
            else
            {
                data.travelTime = float.MaxValue;
            }

            // サービスレベルを判定
            data.los = DensityToLOS(data.density);

            // 渋滞判定
            data.isCongested = data.density >= congestionThreshold;

            return data;
        }

        /// <summary>
        /// 道路上の車両の平均速度を計算
        /// </summary>
        private float CalculateAverageSpeed(RoadSegment road)
        {
            float totalSpeed = 0f;
            int vehicleCount = 0;

            foreach (var lane in road.lanes)
            {
                foreach (var vehicleObj in lane.vehiclesInLane)
                {
                    if (vehicleObj == null) continue;

                    var vehicle = vehicleObj.GetComponent<Vehicle.Vehicle>();
                    if (vehicle != null)
                    {
                        totalSpeed += vehicle.CurrentSpeedKmh;
                        vehicleCount++;
                    }
                }
            }

            if (vehicleCount > 0)
            {
                return totalSpeed / vehicleCount;
            }

            // 車両がいない場合は自由流速度（制限速度）を返す
            return road.speedLimit;
        }

        /// <summary>
        /// 密度からサービスレベルを判定
        /// グリーンシールズモデルに基づく
        /// </summary>
        public LevelOfService DensityToLOS(float density)
        {
            // 1車線あたりの密度に正規化
            float normalizedDensity = density;

            if (normalizedDensity < 7f) return LevelOfService.A;
            if (normalizedDensity < 11f) return LevelOfService.B;
            if (normalizedDensity < 16f) return LevelOfService.C;
            if (normalizedDensity < 22f) return LevelOfService.D;
            if (normalizedDensity < 28f) return LevelOfService.E;
            return LevelOfService.F;
        }

        /// <summary>
        /// 道路の密度を取得
        /// </summary>
        public float GetDensity(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data.density;
            }
            return CalculateTrafficData(road).density;
        }

        /// <summary>
        /// 道路の流量を取得
        /// </summary>
        public float GetFlow(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data.flow;
            }
            return CalculateTrafficData(road).flow;
        }

        /// <summary>
        /// 道路の平均速度を取得
        /// </summary>
        public float GetAverageSpeed(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data.averageSpeed;
            }
            return CalculateTrafficData(road).averageSpeed;
        }

        /// <summary>
        /// 道路のサービスレベルを取得
        /// </summary>
        public LevelOfService GetLOS(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data.los;
            }
            return CalculateTrafficData(road).los;
        }

        /// <summary>
        /// 道路が渋滞しているかどうか
        /// </summary>
        public bool IsCongested(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data.isCongested;
            }
            return CalculateTrafficData(road).isCongested;
        }

        /// <summary>
        /// 経路の推定所要時間を予測
        /// </summary>
        public float PredictTravelTime(List<RoadSegment> path)
        {
            float totalTime = 0f;

            foreach (var road in path)
            {
                if (_trafficData.TryGetValue(road, out var data))
                {
                    totalTime += data.travelTime;
                }
                else
                {
                    // データがない場合は制限速度で計算
                    float speedKmh = road.speedLimit;
                    float lengthKm = road.length / 1000f;
                    totalTime += (lengthKm / speedKmh) * 3600f;
                }
            }

            return totalTime;
        }

        /// <summary>
        /// 経路の渋滞レベルを取得（0-1）
        /// </summary>
        public float GetPathCongestionLevel(List<RoadSegment> path)
        {
            if (path == null || path.Count == 0) return 0f;

            float totalCongestion = 0f;
            float totalLength = 0f;

            foreach (var road in path)
            {
                if (road == null) continue;

                float density = GetDensity(road);
                float congestionRatio = Mathf.Clamp01(density / saturationDensity);

                totalCongestion += congestionRatio * road.length;
                totalLength += road.length;
            }

            return totalLength > 0 ? totalCongestion / totalLength : 0f;
        }

        /// <summary>
        /// 渋滞している道路のリストを取得
        /// </summary>
        public List<RoadSegment> GetCongestedRoads()
        {
            var result = new List<RoadSegment>();

            foreach (var kvp in _trafficData)
            {
                if (kvp.Value.isCongested)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        /// <summary>
        /// 道路の交通データを取得
        /// </summary>
        public TrafficData GetTrafficData(RoadSegment road)
        {
            if (_trafficData.TryGetValue(road, out var data))
            {
                return data;
            }
            return CalculateTrafficData(road);
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Traffic Status:\n" +
                   $"  Average Density: {averageDensity:F1} veh/km\n" +
                   $"  Congested Roads: {congestedRoadCount}\n" +
                   $"  Overall LOS: {overallLOS}";
        }

        /// <summary>
        /// サービスレベルを色で表現
        /// </summary>
        public static Color LOSToColor(LevelOfService los)
        {
            return los switch
            {
                LevelOfService.A => new Color(0f, 0.8f, 0f),        // 緑
                LevelOfService.B => new Color(0.5f, 0.9f, 0f),      // 黄緑
                LevelOfService.C => new Color(1f, 1f, 0f),          // 黄
                LevelOfService.D => new Color(1f, 0.6f, 0f),        // オレンジ
                LevelOfService.E => new Color(1f, 0.3f, 0f),        // 赤オレンジ
                LevelOfService.F => new Color(1f, 0f, 0f),          // 赤
                _ => Color.gray
            };
        }

        /// <summary>
        /// デバッグ用Gizmo描画
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (roadNetwork == null) return;

            foreach (var road in roadNetwork.roadSegments)
            {
                if (road == null || road.centerLine.Count < 2) continue;

                var los = GetLOS(road);
                Gizmos.color = LOSToColor(los);

                for (int i = 0; i < road.centerLine.Count - 1; i++)
                {
                    Gizmos.DrawLine(road.centerLine[i], road.centerLine[i + 1]);
                }
            }
        }
    }
}
