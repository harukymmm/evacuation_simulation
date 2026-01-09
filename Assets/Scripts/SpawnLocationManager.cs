using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 建物カテゴリ別のスポーン位置を管理する
/// </summary>
public class SpawnLocationManager : MonoBehaviour
{
    private static SpawnLocationManager _instance;
    private Dictionary<BuildingCategory, List<EnvironmentalContextProvider.BuildingContext>> _buildingsByCategory;
    
    [Header("設定")]
    [Tooltip("スポーン位置を地上に補正する")]
    public bool adjustToGround = true;
    public float groundYOffset = 0.5f; // 地面からの高さオフセット
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    
    void Start()
    {
        CategorizeAllBuildings();
    }
    
    /// <summary>
    /// 全建物をカテゴリごとに分類
    /// </summary>
    public void CategorizeAllBuildings()
    {
        _buildingsByCategory = new Dictionary<BuildingCategory, List<EnvironmentalContextProvider.BuildingContext>>();
        
        // 各カテゴリのリストを初期化
        foreach (BuildingCategory category in System.Enum.GetValues(typeof(BuildingCategory)))
        {
            _buildingsByCategory[category] = new List<EnvironmentalContextProvider.BuildingContext>();
        }
        
        // EnvironmentalContextProviderから全建物を取得
        var envProvider = FindFirstObjectByType<EnvironmentalContextProvider>();
        if (envProvider == null)
        {
            Debug.LogError("[SpawnLocationManager] EnvironmentalContextProviderが見つかりません");
            return;
        }
        
        var allBuildings = envProvider.GetAllBuildings();
        
        foreach (var building in allBuildings)
        {
            BuildingCategory category = BuildingCategorizer.Categorize(building);
            _buildingsByCategory[category].Add(building);
        }
        
        // デバッグログ：カテゴリ別の建物数を表示
        Debug.Log("[SpawnLocationManager] 建物カテゴリ分類完了:");
        foreach (var kvp in _buildingsByCategory)
        {
            if (kvp.Value.Count > 0)
            {
                Debug.Log($"  - {BuildingCategorizer.GetCategoryDisplayName(kvp.Key)}: {kvp.Value.Count}件");
            }
        }
    }
    
    /// <summary>
    /// 初期化が完了しているかチェックし、未初期化の場合は初期化する（lazy initialization）
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_instance == null)
        {
            Debug.LogWarning("[SpawnLocationManager] インスタンスが存在しません");
            return;
        }
        
        if (_instance._buildingsByCategory == null)
        {
            Debug.Log("[SpawnLocationManager] 初期化が未完了のため、遅延初期化を実行します");
            _instance.CategorizeAllBuildings();
            
            // それでも null のまま（EnvironmentalContextProvider が無いなど）の場合を防御
            if (_instance._buildingsByCategory == null)
            {
                Debug.LogWarning("[SpawnLocationManager] 建物カテゴリ辞書の初期化に失敗しました");
            }
        }
    }
    
    /// <summary>
    /// 指定カテゴリの建物からランダムにスポーン位置を取得
    /// </summary>
    public static Vector3 GetRandomSpawnPosition(BuildingCategory category)
    {
        EnsureInitialized();
        
        if (_instance == null || _instance._buildingsByCategory == null || !_instance._buildingsByCategory.ContainsKey(category))
        {
            Debug.LogWarning($"[SpawnLocationManager] カテゴリ {category} の建物が見つかりません");
            return Vector3.zero;
        }
        
        var buildings = _instance._buildingsByCategory[category];
        if (buildings.Count == 0)
        {
            Debug.LogWarning($"[SpawnLocationManager] カテゴリ {category} に建物がありません");
            return Vector3.zero;
        }
        
        // ランダムに選択
        int randomIndex = Random.Range(0, buildings.Count);
        Vector3 position = buildings[randomIndex].position;
        
        // 地上に補正
        if (_instance.adjustToGround)
        {
            position.y = _instance.groundYOffset;
        }
        
        return position;
    }
    
    /// <summary>
    /// 指定カテゴリの建物情報をランダムに取得（建物名も含む）
    /// </summary>
    public static (Vector3 position, string buildingName) GetRandomSpawnPositionWithName(BuildingCategory category)
    {
        EnsureInitialized();
        
        if (_instance == null || _instance._buildingsByCategory == null || !_instance._buildingsByCategory.ContainsKey(category))
        {
            Debug.LogWarning($"[SpawnLocationManager] カテゴリ {category} の建物が見つかりません");
            return (Vector3.zero, "不明");
        }
        
        var buildings = _instance._buildingsByCategory[category];
        if (buildings.Count == 0)
        {
            Debug.LogWarning($"[SpawnLocationManager] カテゴリ {category} に建物がありません");
            return (Vector3.zero, "不明");
        }
        
        // ランダムに選択
        int randomIndex = Random.Range(0, buildings.Count);
        var building = buildings[randomIndex];
        Vector3 position = building.position;
        
        // 地上に補正
        if (_instance.adjustToGround)
        {
            position.y = _instance.groundYOffset;
        }
        
        return (position, building.name);
    }
    
    /// <summary>
    /// 指定カテゴリの建物数を取得
    /// </summary>
    public static int GetBuildingCount(BuildingCategory category)
    {
        EnsureInitialized();
        
        if (_instance == null || _instance._buildingsByCategory == null || !_instance._buildingsByCategory.ContainsKey(category))
            return 0;
        
        return _instance._buildingsByCategory[category].Count;
    }
    
    /// <summary>
    /// 全カテゴリの建物数を取得（デバッグ用）
    /// </summary>
    public static Dictionary<BuildingCategory, int> GetAllCategoryCounts()
    {
        EnsureInitialized();

        if (_instance == null || _instance._buildingsByCategory == null)
            return new Dictionary<BuildingCategory, int>();

        return _instance._buildingsByCategory.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count
        );
    }

    /// <summary>
    /// Schoolタグ付きオブジェクトからランダムにスポーン位置を取得
    /// PLATEAUデータに学校がない場合のフォールバック用
    /// </summary>
    public static (Vector3 position, string buildingName) GetSchoolSpawnPosition()
    {
        try
        {
            GameObject[] schools = GameObject.FindGameObjectsWithTag("School");
            if (schools.Length > 0)
            {
                int randomIndex = Random.Range(0, schools.Length);
                var school = schools[randomIndex];
                Vector3 position = school.transform.position;

                if (_instance != null && _instance.adjustToGround)
                {
                    position.y = _instance.groundYOffset;
                }

                return (position, school.name);
            }
        }
        catch (UnityException)
        {
            // Schoolタグが存在しない場合は無視
        }

        return (Vector3.zero, "不明");
    }

    /// <summary>
    /// PreSchoolタグ付きオブジェクトからランダムにスポーン位置を取得
    /// PLATEAUデータに保育園がない場合のフォールバック用
    /// </summary>
    public static (Vector3 position, string buildingName) GetPreSchoolSpawnPosition()
    {
        try
        {
            GameObject[] preschools = GameObject.FindGameObjectsWithTag("PreSchool");
            if (preschools.Length > 0)
            {
                int randomIndex = Random.Range(0, preschools.Length);
                var preschool = preschools[randomIndex];
                Vector3 position = preschool.transform.position;

                if (_instance != null && _instance.adjustToGround)
                {
                    position.y = _instance.groundYOffset;
                }

                return (position, preschool.name);
            }
        }
        catch (UnityException)
        {
            // PreSchoolタグが存在しない場合は無視
        }

        return (Vector3.zero, "不明");
    }

    /// <summary>
    /// 指定カテゴリの建物情報をランダムに取得（タグベースのフォールバック付き）
    /// School/PreSchoolカテゴリはタグから直接取得（PLATEAUデータには含まれないため）
    /// </summary>
    public static (Vector3 position, string buildingName) GetRandomSpawnPositionWithNameAndFallback(BuildingCategory category)
    {
        // School/PreSchoolカテゴリはタグから直接取得（PLATEAUデータには含まれない）
        if (category == BuildingCategory.School)
        {
            return GetSchoolSpawnPosition();
        }
        else if (category == BuildingCategory.PreSchool)
        {
            return GetPreSchoolSpawnPosition();
        }

        // その他のカテゴリはPLATEAUデータから取得
        return GetRandomSpawnPositionWithName(category);
    }
}








