using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PLATEAU.CityInfo;
using Newtonsoft.Json;

/// <summary>
/// 環境コンテキストプロバイダー
/// 空間インデックス（Grid方式）を使って、避難者の周辺環境情報を高速に取得します
/// </summary>
public class EnvironmentalContextProvider : MonoBehaviour
{
    [Header("空間インデックス設定")]
    [Tooltip("グリッドセルのサイズ（メートル）。検索範囲の約半分が推奨")]
    public float cellSize = 50f;
    
    [Header("デバッグ設定")]
    [Tooltip("シーン起動時にインデックス情報をJSON出力するか")]
    public bool exportToJsonOnStart = false;
    public string jsonExportPath = "Assets/ExportedData/spatial_index_debug.json";
    
    [Header("パフォーマンス設定")]
    [Tooltip("検索結果をキャッシュする時間（秒）。0で無効化")]
    public float cacheLifetime = 1f;
    
    // 空間分割グリッド: セルごとに建物リストを保持
    private Dictionary<Vector2Int, List<BuildingContext>> spatialGrid;
    
    // 検索結果のキャッシュ
    private Dictionary<int, CachedSearchResult> searchCache;
    
    private class CachedSearchResult
    {
        public float timestamp;
        public List<BuildingContext> buildings;
    }
    
    /// <summary>
    /// 建物のコンテキスト情報
    /// </summary>
    [Serializable]
    public class BuildingContext
    {
        public string name;
        public Vector3 position;
        public float distance; // 避難者からの距離（検索時に設定）
        
        // PLATEAU属性
        public string usage;           // 用途（住宅、商業、公共施設など）
        public string majorUsage;      // 主要用途
        public float height;           // 高さ
        public int floors;             // 階数
        public string structureType;   // 構造種別
        public string landUseType;     // 土地利用種別
        public string gmlId;           // GML ID
    }
    
    void Start()
    {
        searchCache = new Dictionary<int, CachedSearchResult>();
        BuildSpatialIndex();
        
        if (exportToJsonOnStart)
        {
            ExportToJson();
        }
    }
    
    /// <summary>
    /// 空間インデックスを構築（シーン起動時に実行）
    /// </summary>
    private void BuildSpatialIndex()
    {
        var startTime = Time.realtimeSinceStartup;
        spatialGrid = new Dictionary<Vector2Int, List<BuildingContext>>();
        
        // 全PLATEAUオブジェクトを検索（ソート不要なのでFindObjectsSortMode.Noneを使用）
        var cityObjects = FindObjectsByType<PLATEAUCityObjectGroup>(FindObjectsSortMode.None);
        int indexedCount = 0;
        int skippedCount = 0;
        
        foreach (var cityObj in cityObjects)
        {
            // 建物のみを対象（道路や地形を除外）
            if (!cityObj.name.StartsWith("bldg_"))
            {
                skippedCount++;
                continue;
            }
            
            var renderer = cityObj.GetComponent<Renderer>();
            if (renderer == null)
            {
                skippedCount++;
                continue;
            }
            
            // PLATEAUオブジェクトは全て同じTransform.positionを持つため、Renderer.bounds.centerを使用
            Vector3 center = renderer.bounds.center;
            Vector2Int gridKey = WorldToGridKey(center);
            
            // セルが存在しなければ作成
            if (!spatialGrid.ContainsKey(gridKey))
                spatialGrid[gridKey] = new List<BuildingContext>();
            
            // 建物情報を抽出してセルに追加
            var context = ExtractBuildingContext(cityObj, center);
            spatialGrid[gridKey].Add(context);
            indexedCount++;
        }
        
        var elapsed = Time.realtimeSinceStartup - startTime;
        
        Debug.Log($"[EnvironmentalContext] 空間インデックス構築完了: {indexedCount}件の建物をインデックス化（処理時間: {elapsed:F3}秒）");
    }
    
    /// <summary>
    /// 指定位置から一定範囲内の建物を取得（キャッシュ対応）
    /// </summary>
    public List<BuildingContext> GetNearbyBuildings(Vector3 position, float radius)
    {
        // キャッシュキーを生成
        int cacheKey = GetCacheKey(position, radius);
        
        // キャッシュをチェック
        if (cacheLifetime > 0 && searchCache.TryGetValue(cacheKey, out var cached))
        {
            if (Time.time - cached.timestamp < cacheLifetime)
            {
                return cached.buildings;
            }
        }
        
        // 新規検索
        var result = SearchNearbyBuildings(position, radius);
        
        // キャッシュに保存
        if (cacheLifetime > 0)
        {
            searchCache[cacheKey] = new CachedSearchResult
            {
                timestamp = Time.time,
                buildings = result
            };
        }
        
        return result;
    }
    
    /// <summary>
    /// 空間検索の本体処理
    /// </summary>
    private List<BuildingContext> SearchNearbyBuildings(Vector3 position, float radius)
    {
        if (spatialGrid == null || spatialGrid.Count == 0)
        {
            Debug.LogWarning("[EnvironmentalContext] 空間インデックスが未構築です");
            return new List<BuildingContext>();
        }
        
        var nearbyBuildings = new List<BuildingContext>();
        
        // 避難者のグリッドセルを計算
        Vector2Int centerCell = WorldToGridKey(position);
        
        // 検索範囲（半径）からセル数を計算
        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        
        // 周辺セルをループ
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int z = -cellRadius; z <= cellRadius; z++)
            {
                Vector2Int targetCell = centerCell + new Vector2Int(x, z);
                
                // セルが存在するかチェック
                if (spatialGrid.TryGetValue(targetCell, out var buildingsInCell))
                {
                    // このセル内の建物をチェック
                    foreach (var building in buildingsInCell)
                    {
                        // 正確な距離を計算（Y座標は考慮しない）
                        float distance = Vector3.Distance(
                            new Vector3(position.x, 0, position.z),
                            new Vector3(building.position.x, 0, building.position.z)
                        );
                        
                        // 半径内なら追加
                        if (distance <= radius)
                        {
                            // 距離情報を設定（元のオブジェクトは変更しない）
                            var buildingCopy = new BuildingContext
                            {
                                name = building.name,
                                position = building.position,
                                distance = distance,
                                usage = building.usage,
                                majorUsage = building.majorUsage,
                                height = building.height,
                                floors = building.floors,
                                structureType = building.structureType,
                                landUseType = building.landUseType,
                                gmlId = building.gmlId
                            };
                            
                            nearbyBuildings.Add(buildingCopy);
                        }
                    }
                }
            }
        }
        
        // 距離でソート
        nearbyBuildings.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        return nearbyBuildings;
    }
    
    /// <summary>
    /// PLATEAU建物から属性情報を抽出
    /// </summary>
    private BuildingContext ExtractBuildingContext(PLATEAUCityObjectGroup cityObj, Vector3 center)
    {
        var context = new BuildingContext
        {
            name = cityObj.gameObject.name,
            position = center,
            distance = 0,
            usage = "不明",
            majorUsage = "",
            height = 0,
            floors = 0,
            structureType = "",
            landUseType = "",
            gmlId = ""
        };
        
        try
        {
            if (cityObj.CityObjects == null || 
                cityObj.CityObjects.rootCityObjects == null || 
                cityObj.CityObjects.rootCityObjects.Count == 0)
            {
                return context;
            }
            
            var rootObject = cityObj.CityObjects.rootCityObjects[0];
            context.gmlId = rootObject.GmlID;
            
            var jsonStr = JsonConvert.SerializeObject(rootObject);
            var rootData = JsonConvert.DeserializeObject<RootObject>(jsonStr);
            
            if (rootData?.Attributes == null)
            {
                return context;
            }
            
            // 基本属性を抽出
            foreach (var attr in rootData.Attributes)
            {
                if (attr.Key == "bldg:usage")
                {
                    context.usage = attr.Value?.ToString() ?? "不明";
                }
                else if (attr.Key == "bldg:measuredheight")
                {
                    float.TryParse(attr.Value?.ToString(), out context.height);
                }
                else if (attr.Key == "bldg:storeysaboveground")
                {
                    int.TryParse(attr.Value?.ToString(), out context.floors);
                }
                else if (attr.Key == "uro:buildingDetailAttribute")
                {
                    // 詳細属性を抽出（3層構造に対応）
                    if (attr.AttributeSetValue != null)
                    {
                        foreach (var detailAttr in attr.AttributeSetValue)
                        {
                            if (detailAttr.Key == "uro:BuildingDetailAttribute" && 
                                detailAttr.AttributeSetValue != null)
                            {
                                foreach (var uroAttr in detailAttr.AttributeSetValue)
                                {
                                    if (uroAttr.Key == "uro:buildingStructureType")
                                        context.structureType = uroAttr.Value?.ToString() ?? "";
                                    else if (uroAttr.Key == "uro:landUseType")
                                        context.landUseType = uroAttr.Value?.ToString() ?? "";
                                    else if (uroAttr.Key == "uro:majorUsage")
                                        context.majorUsage = uroAttr.Value?.ToString() ?? "";
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[EnvironmentalContext] {cityObj.name} の属性抽出に失敗: {ex.Message}");
        }
        
        return context;
    }
    
    /// <summary>
    /// ワールド座標をグリッドセル座標に変換
    /// </summary>
    private Vector2Int WorldToGridKey(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }
    
    /// <summary>
    /// キャッシュキーを生成
    /// </summary>
    private int GetCacheKey(Vector3 position, float radius)
    {
        // 位置を粗くグリッド化してキーを生成（10mごと）
        int gridX = Mathf.FloorToInt(position.x / 10f);
        int gridZ = Mathf.FloorToInt(position.z / 10f);
        int radiusInt = Mathf.FloorToInt(radius);
        
        return HashCode.Combine(gridX, gridZ, radiusInt);
    }
    
    /// <summary>
    /// デバッグ用：空間インデックスをJSONで出力
    /// </summary>
    private void ExportToJson()
    {
        try
        {
            var data = new
            {
                metadata = new
                {
                    total_buildings = spatialGrid.Sum(g => g.Value.Count),
                    cell_size = cellSize,
                    grid_cells = spatialGrid.Count,
                    timestamp = DateTime.Now.ToString("o")
                },
                buildings = spatialGrid.SelectMany(g => g.Value).ToList()
            };
            
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            System.IO.File.WriteAllText(jsonExportPath, json);
            
            Debug.Log($"[EnvironmentalContext] デバッグ用JSONを出力: {jsonExportPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[EnvironmentalContext] JSON出力に失敗: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 統計情報を取得（デバッグ用）
    /// </summary>
    public string GetStatistics()
    {
        if (spatialGrid == null)
            return "空間インデックス未構築";
        
        int totalBuildings = spatialGrid.Sum(g => g.Value.Count);
        float avgBuildingsPerCell = spatialGrid.Count > 0 ? (float)totalBuildings / spatialGrid.Count : 0;
        int maxBuildingsInCell = spatialGrid.Count > 0 ? spatialGrid.Max(g => g.Value.Count) : 0;
        
        return $"建物総数: {totalBuildings}, " +
               $"セル数: {spatialGrid.Count}, " +
               $"平均: {avgBuildingsPerCell:F1}件/セル, " +
               $"最大: {maxBuildingsInCell}件/セル";
    }
}
