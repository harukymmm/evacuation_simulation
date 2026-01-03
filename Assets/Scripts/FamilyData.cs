using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 家族メンバーの情報
/// </summary>
[Serializable]
public class FamilyMember
{
    public int agent_id;           // -1 = シーン内に存在しないNPC、正の値 = シーン内の他の避難者
    public string name;            // 名前
    public string relation;        // 続柄（妻、夫、息子、娘、父、母など）
    public BuildingCategory spawn_category; // スポーンする建物カテゴリ
    public Vector3 spawn_position; // 実際のスポーン位置（初期化時に設定）
    public string spawn_building_name; // スポーンした建物名
    public bool has_phone;         // 連絡手段を持っているか
    public bool exists_in_scene;   // シーン内に実在するか
    
    // 探索用
    public string likely_location; // 想定される場所の説明（"自宅", "小学校"など）
    public Vector3 search_position; // 探索する座標
    public float distance_meters;   // 所有者からの距離（計算時に設定）
}

/// <summary>
/// 避難者の家族構成情報
/// </summary>
[Serializable]
public class FamilyData
{
    public int owner_agent_id;                  // この家族情報の所有者
    public BuildingCategory owner_spawn_category; // 所有者のスポーンカテゴリ
    public List<FamilyMember> members;          // 家族メンバーリスト
    
    public FamilyData()
    {
        members = new List<FamilyMember>();
    }
}

/// <summary>
/// 家族情報を管理するクラス
/// </summary>
public static class FamilyManager
{
    private static Dictionary<int, FamilyData> _families = null;
    private static readonly string FamilyCsvPath = Path.Combine(Application.dataPath, "Config", "families.csv");

    /// <summary>
    /// CSVファイルから家族データを読み込む
    /// </summary>
    public static void LoadFamilies()
    {
        _families = new Dictionary<int, FamilyData>();

        if (!File.Exists(FamilyCsvPath))
        {
            Debug.LogWarning($"[FamilyManager] CSV file not found: {FamilyCsvPath}");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(FamilyCsvPath);
            if (lines.Length < 2)
            {
                Debug.LogWarning("[FamilyManager] CSV file is empty or has no data rows");
                return;
            }

            // ヘッダー行をスキップ（1行目）
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                var fields = line.Split(',');
                if (fields.Length < 7)
                {
                    Debug.LogWarning($"[FamilyManager] Invalid CSV line {i}: {line}");
                    continue;
                }

                int ownerId = int.Parse(fields[0]);
                string relation = fields[1];
                int targetAgentId = int.Parse(fields[2]);
                string targetName = fields[3];
                string spawnCategoryStr = fields[4];
                bool hasPhone = bool.Parse(fields[5]);
                bool existsInScene = bool.Parse(fields[6]);

                // BuildingCategoryをパース
                BuildingCategory spawnCategory;
                if (!Enum.TryParse(spawnCategoryStr, true, out spawnCategory))
                {
                    Debug.LogWarning($"[FamilyManager] Invalid spawn_category '{spawnCategoryStr}' at line {i}");
                    spawnCategory = BuildingCategory.Other;
                }

                // 所有者のFamilyDataを取得または作成
                if (!_families.ContainsKey(ownerId))
                {
                    _families[ownerId] = new FamilyData
                    {
                        owner_agent_id = ownerId,
                        owner_spawn_category = BuildingCategory.Other // 後で設定
                    };
                }

                // relationが"なし"や"self"でない場合は家族メンバーとして追加
                if (relation.ToLower() != "なし" && relation.ToLower() != "self")
                {
                    var member = new FamilyMember
                    {
                        agent_id = targetAgentId,
                        name = targetName,
                        relation = relation,
                        spawn_category = spawnCategory,
                        has_phone = hasPhone,
                        exists_in_scene = existsInScene,
                        likely_location = BuildingCategorizer.GetCategoryDisplayName(spawnCategory)
                    };

                    _families[ownerId].members.Add(member);
                }
                else
                {
                    // 本人のスポーンカテゴリを設定
                    _families[ownerId].owner_spawn_category = spawnCategory;
                }
            }

            Debug.Log($"[FamilyManager] Loaded family data for {_families.Count} agents from {FamilyCsvPath}");
            
            // デバッグ: 各エージェントの家族構成を表示
            foreach (var kvp in _families)
            {
                int agentId = kvp.Key;
                var familyData = kvp.Value;
                Debug.Log($"[FamilyManager] Agent {agentId}: {familyData.members.Count}人の家族, スポーンカテゴリ: {familyData.owner_spawn_category}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FamilyManager] Failed to load families: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// エージェントIDに対応する家族データを取得
    /// </summary>
    public static FamilyData GetFamily(int agentId)
    {
        if (_families == null)
        {
            LoadFamilies();
        }

        if (_families != null && _families.TryGetValue(agentId, out var family))
        {
            return family;
        }

        return null;
    }

    /// <summary>
    /// 全ての家族データを取得
    /// </summary>
    public static Dictionary<int, FamilyData> GetAllFamilies()
    {
        if (_families == null)
        {
            LoadFamilies();
        }
        return _families ?? new Dictionary<int, FamilyData>();
    }
}





