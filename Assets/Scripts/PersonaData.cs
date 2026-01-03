using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// ペルソナデータを保持するクラス
/// </summary>
[Serializable]
public class PersonaData
{
    public int agent_id;
    public string name;
    public string role;
    public string age_group;
    public float speed_multiplier;
    public string stairs_usage; // "allowed" or "forbidden"
    public string mental_state;
    public string priority;
    public string system_prompt_context;
    public bool has_smartphone = true; // デフォルトはtrue（スマホを持っている）

    public bool CanUseStairs => stairs_usage == "allowed";
}

/// <summary>
/// ペルソナデータを管理するクラス
/// </summary>
public static class PersonaManager
{
    private static Dictionary<int, PersonaData> _personas = null;
    private static readonly string PersonaCsvPath = Path.Combine(Application.dataPath, "Config", "personas.csv");

    /// <summary>
    /// CSVファイルからペルソナデータを読み込む
    /// </summary>
    public static void LoadPersonas()
    {
        _personas = new Dictionary<int, PersonaData>();

        if (!File.Exists(PersonaCsvPath))
        {
            Debug.LogWarning($"[PersonaManager] CSV file not found: {PersonaCsvPath}");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(PersonaCsvPath);
            if (lines.Length < 2)
            {
                Debug.LogWarning("[PersonaManager] CSV file is empty or has no data rows");
                return;
            }

            // ヘッダー行をスキップ（1行目）
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // CSVのパース（カンマ区切り、ただしsystem_prompt_context内のカンマは考慮）
                var fields = ParseCsvLine(line);
                if (fields.Length < 9)
                {
                    Debug.LogWarning($"[PersonaManager] Invalid CSV line {i}: {line}");
                    continue;
                }

                var persona = new PersonaData
                {
                    agent_id = int.Parse(fields[0]),
                    name = fields[1],
                    role = fields[2],
                    age_group = fields[3],
                    speed_multiplier = float.Parse(fields[4]),
                    stairs_usage = fields[5],
                    mental_state = fields[6],
                    priority = fields[7],
                    system_prompt_context = fields[8]
                };
                
                // has_smartphoneカラムが存在する場合は読み込む（オプショナル）
                if (fields.Length >= 10)
                {
                    if (bool.TryParse(fields[9], out bool hasPhone))
                    {
                        persona.has_smartphone = hasPhone;
                    }
                }
                // カラムがなければデフォルト値（true）を使用

                _personas[persona.agent_id] = persona;
            }

            Debug.Log($"[PersonaManager] Loaded {_personas.Count} personas from {PersonaCsvPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PersonaManager] Failed to load personas: {ex.Message}");
        }
    }

    /// <summary>
    /// CSV行をパース（ダブルクォートで囲まれたフィールドを考慮）
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        fields.Add(currentField); // 最後のフィールド

        return fields.ToArray();
    }

    /// <summary>
    /// エージェントIDに対応するペルソナデータを取得
    /// </summary>
    public static PersonaData GetPersona(int agentId)
    {
        if (_personas == null)
        {
            LoadPersonas();
        }

        if (_personas != null && _personas.TryGetValue(agentId, out var persona))
        {
            return persona;
        }

        return null;
    }

    /// <summary>
    /// 全てのペルソナデータを取得
    /// </summary>
    public static Dictionary<int, PersonaData> GetAllPersonas()
    {
        if (_personas == null)
        {
            LoadPersonas();
        }
        return _personas ?? new Dictionary<int, PersonaData>();
    }
}

