using System;
using UnityEngine;

namespace LLM
{
    [Serializable]
    public class Vector3Payload
    {
        public float x;
        public float y;
        public float z;

        public Vector3Payload() { }

        public Vector3Payload(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    [Serializable]
    public class ShelterCandidatePayload
    {
        public string id;
        public string display_name;      // 表示名（例: 豊間小学校）
        public string description;       // 説明（例: 海抜20m、耐震構造）
        public Vector3Payload position;
        public int current_capacity;
        public int max_capacity;
        public float distance_meters;    // NavMeshでの経路距離（メートル）
        public float walking_time_minutes; // 徒歩所要時間（分）
    }

    [Serializable]
    public class EvacueePayload
    {
        public string id;
        public Vector3Payload position;
    }

    [Serializable]
    public class SelfStatePayload
    {
        public Vector3Payload position;
        public Vector3Payload velocity;
        public float energy_level;
        public string energy_label;
        public float stress_level;
        public string stress_label;
        public string stress_reason;
        public string current_goal;
        public float stamina;
        public string[] injuries;
        public string injury_notes;
    }

    [Serializable]
    public class TemporalContextPayload
    {
        public float elapsed_time;
        public bool has_time_limit;
        public float time_limit;
    }

    [Serializable]
    public class LLMActionRequest
    {
        public string request_id;
        public float timestamp;
        public ShelterCandidatePayload[] shelter_candidates;
        public EvacueePayload[] evacuees;
    }

    [Serializable]
    public class LLMActionResponse
    {
        public string request_id;
        public int[] actions;
        public string reasoning;
        public float confidence;
    }

    [Serializable]
    public class PersonaPayload
    {
        public int agent_id;
        public string name;
        public string role;
        public string age_group;
        public float speed_multiplier;
        public string stairs_usage;
        public string mental_state;
        public string priority;
        public string system_prompt_context;
    }

    [Serializable]
    public class BuildingContextPayload
    {
        public string name;
        public Vector3Payload position;
        public float distance;
        public string usage;
        public string major_usage;
        public float height;
        public int floors;
        public string structure_type;
        public string land_use_type;
    }

    [Serializable]
    public class EnvironmentalContextPayload
    {
        public BuildingContextPayload[] nearby_buildings;
        public float search_radius;
        public int total_buildings_in_area;
    }

    [Serializable]
    public class LLMEvacDecisionRequest
    {
        public string request_id;
        public float timestamp;
        public EvacueePayload evacuee;
        public ShelterCandidatePayload[] shelter_candidates;
        public SelfStatePayload self_state;
        public TemporalContextPayload temporal_context;
        public PersonaPayload persona; // ペルソナ情報
        public EnvironmentalContextPayload environmental_context; // 環境コンテキスト
    }

    [Serializable]
    public class LLMEvacDecisionResponse
    {
        public string request_id;
        public string evacuee_id;
        public string selected_shelter_id;
        public string reasoning;
        public float confidence;
        public float desired_speed;
    }

    [Serializable]
    internal class ResponseEnvelope
    {
        public string request_id;
    }
}

