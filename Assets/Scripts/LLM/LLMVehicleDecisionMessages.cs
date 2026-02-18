using System;
using UnityEngine;

namespace LLM
{
    /// <summary>
    /// 車両の行動タイプ
    /// </summary>
    public enum VehicleActionType
    {
        DRIVE_TO_SHELTER,   // 避難所へ向かって走行
        WAIT_IN_CAR,        // 車内で待機
        PICKUP_FAMILY,      // 家族を迎えに行く
        CONTACT,            // 電話連絡
        PARK_AND_WALK       // 駐車して徒歩避難
    }

    /// <summary>
    /// 車両の速度選択
    /// </summary>
    public enum VehicleSpeedChoice
    {
        SLOW,       // ゆっくり（制限速度の70%）
        NORMAL,     // 通常（制限速度）
        FAST        // 速め（制限速度の120%、安全な範囲で）
    }

    /// <summary>
    /// 経路の優先設定
    /// </summary>
    public enum RoutePreference
    {
        SHORTEST,   // 最短距離
        FASTEST,    // 最速（渋滞考慮）
        SAFEST      // 最安全（広い道路優先）
    }

    /// <summary>
    /// エンジン状態
    /// </summary>
    public enum EngineState
    {
        ON,         // エンジンオン（アイドリング）
        OFF         // エンジンオフ
    }
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
    /// 車両の家族メンバー情報ペイロード
    /// </summary>
    [Serializable]
    public class VehicleFamilyMemberPayload
    {
        public string name;                   // 家族の名前
        public string relation;               // 続柄（妻、夫、子ども等）
        public string likely_location;        // 推定位置の説明
        public Vector3Payload position;       // 推定位置座標
        public float distance_meters;         // 現在地からの距離
        public bool has_phone;                // 電話連絡可能か
        public bool is_in_vehicle;            // 既に車内にいるか
        public bool is_safe;                  // 安全確認済みか
    }

    /// <summary>
    /// 車両用LLM意思決定リクエスト
    /// </summary>
    [Serializable]
    public class LLMVehicleDecisionRequest
    {
        public string request_type = "vehicle_decision";  // サーバー側でのルーティング用
        public string request_id;
        public float timestamp;
        public VehiclePayload vehicle;
        public VehicleShelterPayload[] shelter_candidates;
        public VehicleStatePayload vehicle_state;
        public PersonaPayload persona;        // 歩行者と同じペルソナ形式を使用
        public TemporalContextPayload temporal_context;
        public TrafficContextPayload traffic_context;
        public EnvironmentalContextPayload environmental_context;
        public VehicleFamilyMemberPayload[] family_members;  // 家族情報
        public string current_action;         // 現在の行動タイプ
        public string[] action_history;       // 行動履歴（直近の判断）
    }

    /// <summary>
    /// 車両用LLM意思決定レスポンス
    /// </summary>
    [Serializable]
    public class LLMVehicleDecisionResponse
    {
        public string request_id;
        public string vehicle_id;

        // 行動タイプ（必須）
        public string action_type;            // VehicleActionType の文字列表現

        // 共通フィールド
        public string reasoning;              // 判断理由
        public float confidence;              // 信頼度（0-1）

        // DRIVE_TO_SHELTER 用
        public string selected_shelter_id;    // 目的地の避難所ID
        public string desired_speed;          // VehicleSpeedChoice: SLOW/NORMAL/FAST
        public string route_preference;       // RoutePreference: SHORTEST/FASTEST/SAFEST

        // WAIT_IN_CAR 用
        public string wait_reason;            // 待機理由
        public float max_wait_time_sec;       // 最大待機時間（秒）
        public string engine_state;           // EngineState: ON/OFF

        // PICKUP_FAMILY 用
        public string target_family_member;   // 対象の家族名
        public Vector3Payload pickup_location; // 迎えに行く場所
        public string after_pickup_shelter;   // 合流後の避難所ID

        // CONTACT 用
        public string contact_target;         // 連絡相手
        public string contact_message;        // 伝える内容
        public bool should_stop;              // 停車するか

        // PARK_AND_WALK 用
        public Vector3Payload parking_location; // 駐車場所
        public string walking_destination;    // 徒歩での目的地（避難所ID）
        public bool abandon_vehicle;          // 車両放棄するか

        // 後方互換性のため維持
        public float desired_speed_kmh;       // 推奨速度（km/h）- 旧形式
        public bool should_change_route;      // 経路変更を推奨するか
        public string suggested_action;       // 追加のアクション提案
    }

}
