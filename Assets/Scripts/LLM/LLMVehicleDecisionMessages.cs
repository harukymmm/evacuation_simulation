using System;
using UnityEngine;

namespace LLM
{
    /// <summary>
    /// 車両情報ペイロード
    /// </summary>
    [Serializable]
    public class VehiclePayload
    {
        public string id;
        public Vector3Payload position;
        public float current_speed_kmh;
        public int passenger_count;
        public string current_road;
        public string vehicle_type;
    }

    /// <summary>
    /// 道路状況ペイロード
    /// </summary>
    [Serializable]
    public class RoadConditionPayload
    {
        public string road_id;
        public string road_name;
        public float congestion_level;       // 0-1（渋滞度）
        public float average_speed_kmh;      // 平均速度
        public float density;                // 密度（vehicles/km）
        public string level_of_service;      // LOS（A-F）
    }

    /// <summary>
    /// 経路情報ペイロード
    /// </summary>
    [Serializable]
    public class RouteInfoPayload
    {
        public float total_distance_km;
        public float estimated_travel_time_minutes;
        public float average_congestion_level;
        public RoadConditionPayload[] road_segments;
    }

    /// <summary>
    /// 車両用の避難所候補ペイロード
    /// </summary>
    [Serializable]
    public class VehicleShelterPayload
    {
        public string id;
        public string display_name;
        public string description;
        public Vector3Payload position;
        public int current_capacity;
        public int max_capacity;
        public int available_parking_spots;
        public int max_parking_spots;
        public float driving_distance_km;
        public float driving_time_minutes;
        public RouteInfoPayload route_info;
    }

    /// <summary>
    /// 車両ペルソナペイロード
    /// </summary>
    [Serializable]
    public class VehiclePersonaPayload
    {
        public int vehicle_id;
        public string driver_name;
        public int passenger_count;
        public float aggressiveness;          // 運転の積極性（0-1）
        public float speed_preference;        // 速度嗜好（制限速度に対する倍率）
        public float lane_change_tendency;    // 車線変更の傾向（0-1）
        public string vehicle_type;           // Sedan, SUV, Minivan, Truck
        public string system_prompt_context;  // LLM用の追加コンテキスト
    }

    /// <summary>
    /// 車両状態ペイロード
    /// </summary>
    [Serializable]
    public class VehicleStatePayload
    {
        public Vector3Payload position;
        public Vector3Payload velocity;
        public float current_speed_kmh;
        public float fuel_level;              // 0-1
        public float stress_level;            // 0-1
        public string current_road_name;
        public string current_lane;
        public string current_goal;
        public string vehicle_state;          // Idle, Driving, Waiting, Parking
    }

    /// <summary>
    /// 交通状況ペイロード
    /// </summary>
    [Serializable]
    public class TrafficContextPayload
    {
        public float average_network_density;
        public int congested_road_count;
        public string overall_level_of_service;
        public RoadConditionPayload[] nearby_roads;
    }

    /// <summary>
    /// 車両用LLM意思決定リクエスト
    /// </summary>
    [Serializable]
    public class LLMVehicleDecisionRequest
    {
        public string request_id;
        public float timestamp;
        public VehiclePayload vehicle;
        public VehicleShelterPayload[] shelter_candidates;
        public VehicleStatePayload vehicle_state;
        public VehiclePersonaPayload persona;
        public TemporalContextPayload temporal_context;
        public TrafficContextPayload traffic_context;
        public EnvironmentalContextPayload environmental_context;
    }

    /// <summary>
    /// 車両用LLM意思決定レスポンス
    /// </summary>
    [Serializable]
    public class LLMVehicleDecisionResponse
    {
        public string request_id;
        public string vehicle_id;
        public string selected_shelter_id;
        public string reasoning;
        public float confidence;
        public float desired_speed_kmh;
        public bool should_change_route;      // 経路変更を推奨するか
        public string suggested_action;        // 追加のアクション提案
    }

    /// <summary>
    /// 車両ペルソナデータ
    /// </summary>
    [Serializable]
    public class VehiclePersonaData
    {
        public int vehicle_id;
        public string driver_name;
        public int base_passenger_count;

        // 運転行動パラメータ
        public float aggressiveness;          // 運転の積極性（0-1）
        public float speed_preference;        // 速度嗜好
        public float lane_change_tendency;    // 車線変更傾向

        // 車両特性
        public string vehicle_type;
        public float fuel_efficiency;

        // LLMコンテキスト
        public string system_prompt_context;

        /// <summary>
        /// デフォルトのペルソナを生成
        /// </summary>
        public static VehiclePersonaData CreateDefault(int id)
        {
            return new VehiclePersonaData
            {
                vehicle_id = id,
                driver_name = $"Driver_{id}",
                base_passenger_count = UnityEngine.Random.Range(1, 5),
                aggressiveness = UnityEngine.Random.Range(0.3f, 0.7f),
                speed_preference = UnityEngine.Random.Range(0.9f, 1.1f),
                lane_change_tendency = UnityEngine.Random.Range(0.3f, 0.7f),
                vehicle_type = "Sedan",
                fuel_efficiency = 15f, // km/L
                system_prompt_context = ""
            };
        }
    }

    /// <summary>
    /// 車両ペルソナ管理
    /// </summary>
    public static class VehiclePersonaManager
    {
        private static System.Collections.Generic.Dictionary<int, VehiclePersonaData> _vehiclePersonas
            = new System.Collections.Generic.Dictionary<int, VehiclePersonaData>();
        private static bool _isLoaded = false;

        /// <summary>
        /// 車両ペルソナを読み込み
        /// </summary>
        public static void LoadVehiclePersonas()
        {
            _vehiclePersonas.Clear();

            // CSVから読み込む実装（将来対応）
            // 現在はデフォルト値を使用

            _isLoaded = true;
            Debug.Log("[VehiclePersonaManager] ペルソナ管理を初期化しました");
        }

        /// <summary>
        /// 車両ペルソナを取得（なければ生成）
        /// </summary>
        public static VehiclePersonaData GetVehiclePersona(int vehicleId)
        {
            if (!_isLoaded)
            {
                LoadVehiclePersonas();
            }

            if (_vehiclePersonas.TryGetValue(vehicleId, out var persona))
            {
                return persona;
            }

            // 新規生成してキャッシュ
            var newPersona = VehiclePersonaData.CreateDefault(vehicleId);
            _vehiclePersonas[vehicleId] = newPersona;
            return newPersona;
        }

        /// <summary>
        /// ペルソナをペイロードに変換
        /// </summary>
        public static VehiclePersonaPayload ToPayload(VehiclePersonaData data)
        {
            if (data == null) return null;

            return new VehiclePersonaPayload
            {
                vehicle_id = data.vehicle_id,
                driver_name = data.driver_name,
                passenger_count = data.base_passenger_count,
                aggressiveness = data.aggressiveness,
                speed_preference = data.speed_preference,
                lane_change_tendency = data.lane_change_tendency,
                vehicle_type = data.vehicle_type,
                system_prompt_context = data.system_prompt_context
            };
        }
    }
}
