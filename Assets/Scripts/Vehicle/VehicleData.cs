using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// 車両データ（CSVから読み込み）
/// 車両と家族の関連付けを管理
/// </summary>
[Serializable]
public class VehicleData
{
    public string vehicleId;              // 車両ID (V001, V002...)
    public string familyId;               // 所有家族ID
    public int driverAgentId;             // 運転者のagent_id
    public List<int> passengerAgentIds;   // 同乗者のagent_idリスト

    public VehicleData()
    {
        passengerAgentIds = new List<int>();
    }

    /// <summary>
    /// 総乗車人数（運転者 + 同乗者）
    /// </summary>
    public int TotalPassengers => 1 + passengerAgentIds.Count;
}

/// <summary>
/// 車両データ管理クラス
/// vehicles.csvから読み込み、またはfamilies.csvから自動生成
/// </summary>
public static class VehicleDataManager
{
    private static Dictionary<string, VehicleData> _vehicleCache = null;
    private static Dictionary<string, VehicleData> _vehicleByFamilyId = null;
    private static Dictionary<int, VehicleData> _vehicleByDriverId = null;
    private static readonly string VehiclesCsvPath =
        Path.Combine(Application.dataPath, "Config", "vehicles.csv");

    /// <summary>
    /// CSVから車両データを読み込み
    /// </summary>
    public static void LoadVehicles()
    {
        _vehicleCache = new Dictionary<string, VehicleData>();
        _vehicleByFamilyId = new Dictionary<string, VehicleData>();
        _vehicleByDriverId = new Dictionary<int, VehicleData>();

        if (!File.Exists(VehiclesCsvPath))
        {
            Debug.Log($"[VehicleDataManager] vehicles.csv not found, generating from families...");
            GenerateVehiclesFromFamilies();
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(VehiclesCsvPath);
            if (lines.Length < 2)
            {
                Debug.LogWarning("[VehicleDataManager] vehicles.csv is empty, generating from families...");
                GenerateVehiclesFromFamilies();
                return;
            }

            // ヘッダー行をスキップ（1行目）
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var fields = line.Split(',');
                if (fields.Length < 4)
                {
                    Debug.LogWarning($"[VehicleDataManager] Invalid CSV line {i}: {line}");
                    continue;
                }

                var data = new VehicleData
                {
                    vehicleId = fields[0].Trim(),
                    familyId = fields[1].Trim(),
                    driverAgentId = int.Parse(fields[2].Trim()),
                    passengerAgentIds = ParseIntList(fields[3].Trim())
                };

                _vehicleCache[data.vehicleId] = data;
                _vehicleByFamilyId[data.familyId] = data;
                _vehicleByDriverId[data.driverAgentId] = data;
            }

            Debug.Log($"[VehicleDataManager] Loaded {_vehicleCache.Count} vehicles from CSV");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VehicleDataManager] Failed to load vehicles.csv: {ex.Message}");
            GenerateVehiclesFromFamilies();
        }
    }

    /// <summary>
    /// families.csvから車両データを自動生成
    /// </summary>
    public static void GenerateVehiclesFromFamilies()
    {
        _vehicleCache = new Dictionary<string, VehicleData>();
        _vehicleByFamilyId = new Dictionary<string, VehicleData>();
        _vehicleByDriverId = new Dictionary<int, VehicleData>();

        // FamilyManagerから家族グループを取得
        var families = FamilyManager.GetAllFamilies();
        int vehicleIndex = 1;

        // 既に車両を割り当てたagent_idを追跡
        var assignedDrivers = new HashSet<int>();

        foreach (var family in families.Values)
        {
            // 運転可能な家族メンバーを探す
            int driverId = FindDriverInFamily(family, assignedDrivers);
            if (driverId <= 0) continue;  // 運転者がいない家族は車両なし

            var vehicleData = new VehicleData
            {
                vehicleId = $"V{vehicleIndex:D3}",
                familyId = $"F{family.owner_agent_id:D3}",
                driverAgentId = driverId,
                passengerAgentIds = GetPassengerIds(family, driverId)
            };

            _vehicleCache[vehicleData.vehicleId] = vehicleData;
            _vehicleByFamilyId[vehicleData.familyId] = vehicleData;
            _vehicleByDriverId[driverId] = vehicleData;
            assignedDrivers.Add(driverId);
            vehicleIndex++;
        }

        Debug.Log($"[VehicleDataManager] Auto-generated {_vehicleCache.Count} vehicles from families");
    }

    /// <summary>
    /// 家族内で運転可能なメンバーを探す
    /// </summary>
    private static int FindDriverInFamily(FamilyData family, HashSet<int> assignedDrivers)
    {
        // 本人が運転可能か確認
        if (!assignedDrivers.Contains(family.owner_agent_id))
        {
            var ownerPersona = PersonaManager.GetPersona(family.owner_agent_id);
            if (ownerPersona != null && CanDrive(ownerPersona))
                return family.owner_agent_id;
        }

        // 家族メンバーから運転可能な人を探す
        foreach (var member in family.members)
        {
            if (member.agent_id <= 0 || !member.exists_in_scene) continue;
            if (assignedDrivers.Contains(member.agent_id)) continue;

            var persona = PersonaManager.GetPersona(member.agent_id);
            if (persona != null && CanDrive(persona))
                return member.agent_id;
        }

        return -1;  // 運転可能な人がいない
    }

    /// <summary>
    /// 運転可能かどうかを判定
    /// </summary>
    public static bool CanDrive(PersonaData persona)
    {
        if (persona == null) return false;

        // 年齢グループがnullの場合は運転不可
        if (string.IsNullOrEmpty(persona.age_group)) return false;

        // 10代は運転不可
        if (persona.age_group.StartsWith("10s")) return false;

        // 80代以上は運転不可
        if (persona.age_group.StartsWith("80s") ||
            persona.age_group.StartsWith("90s")) return false;

        // 身体状況がnullの場合は運転可
        if (string.IsNullOrEmpty(persona.physical_condition)) return true;

        // 要介護者・歩行困難者は運転不可
        if (persona.physical_condition.Contains("介護") ||
            persona.physical_condition.Contains("歩行困難")) return false;

        return true;
    }

    /// <summary>
    /// 同乗者IDリストを取得
    /// </summary>
    private static List<int> GetPassengerIds(FamilyData family, int driverId)
    {
        var passengers = new List<int>();

        // 本人が運転者でなければ同乗者
        if (family.owner_agent_id != driverId)
            passengers.Add(family.owner_agent_id);

        // 家族メンバーを同乗者として追加
        foreach (var member in family.members)
        {
            if (member.agent_id > 0 &&
                member.agent_id != driverId &&
                member.exists_in_scene)
            {
                passengers.Add(member.agent_id);
            }
        }

        return passengers;
    }

    /// <summary>
    /// 車両IDで車両データを取得
    /// </summary>
    public static VehicleData GetVehicle(string vehicleId)
    {
        EnsureLoaded();
        return _vehicleCache?.GetValueOrDefault(vehicleId);
    }

    /// <summary>
    /// 家族IDで車両データを取得
    /// </summary>
    public static VehicleData GetVehicleByFamilyId(string familyId)
    {
        EnsureLoaded();
        return _vehicleByFamilyId?.GetValueOrDefault(familyId);
    }

    /// <summary>
    /// 運転者のagent_idで車両データを取得
    /// </summary>
    public static VehicleData GetVehicleByDriverId(int driverAgentId)
    {
        EnsureLoaded();
        return _vehicleByDriverId?.GetValueOrDefault(driverAgentId);
    }

    /// <summary>
    /// 全ての車両データを取得
    /// </summary>
    public static IEnumerable<VehicleData> GetAllVehicles()
    {
        EnsureLoaded();
        return _vehicleCache?.Values ?? Enumerable.Empty<VehicleData>();
    }

    /// <summary>
    /// 車両数を取得
    /// </summary>
    public static int VehicleCount
    {
        get
        {
            EnsureLoaded();
            return _vehicleCache?.Count ?? 0;
        }
    }

    /// <summary>
    /// 必要に応じてデータを読み込む
    /// </summary>
    private static void EnsureLoaded()
    {
        if (_vehicleCache == null)
        {
            LoadVehicles();
        }
    }

    /// <summary>
    /// キャッシュをクリア（テスト用）
    /// </summary>
    public static void ClearCache()
    {
        _vehicleCache = null;
        _vehicleByFamilyId = null;
        _vehicleByDriverId = null;
    }

    /// <summary>
    /// セミコロン区切りの整数リストをパース
    /// </summary>
    private static List<int> ParseIntList(string s)
    {
        if (string.IsNullOrEmpty(s)) return new List<int>();

        var result = new List<int>();
        foreach (var item in s.Split(';'))
        {
            if (int.TryParse(item.Trim(), out int value))
            {
                result.Add(value);
            }
        }
        return result;
    }
}
