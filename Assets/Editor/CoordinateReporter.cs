using UnityEngine;
using UnityEditor;

public class CoordinateReporter : MonoBehaviour
{
    // メニューバーの Tools > Report > Report World Coordinates を実行
    [MenuItem("Tools/Report/Report World Coordinates")]
    public static void ReportSelectedObjectCoordinates()
    {
        // 選択中のオブジェクトを取得
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("エラー", "座標を確認したいオブジェクトを選択してください。", "OK");
            return;
        }

        Debug.Log("--- 🏢 選択オブジェクトのワールド座標報告 (Bounds基準) ---");
        
        foreach (var obj in selectedObjects)
        {
            // Transformコンポーネントを取得
            Transform tr = obj.transform;
            Renderer rend = obj.GetComponent<Renderer>();
            
            // ピボット位置
            Vector3 pivotPosition = tr.position;

            if (rend != null)
            {
                // Rendererがある場合は、Boundsの詳細情報を出力
                Bounds bounds = rend.bounds;
                
                Debug.Log($"━━━ オブジェクト名: {obj.name} ━━━");
                Debug.Log($"  📍 ピボット位置: X={pivotPosition.x:F3}, Y={pivotPosition.y:F3}, Z={pivotPosition.z:F3}");
                Debug.Log($"  📦 Bounds Center: X={bounds.center.x:F3}, Y={bounds.center.y:F3}, Z={bounds.center.z:F3}");
                Debug.Log($"  ⬇️  Bounds Min (底面): X={bounds.min.x:F3}, Y={bounds.min.y:F3}, Z={bounds.min.z:F3}");
                Debug.Log($"  ⬆️  Bounds Max (頂点): X={bounds.max.x:F3}, Y={bounds.max.y:F3}, Z={bounds.max.z:F3}");
                Debug.Log($"  📏 Bounds Size: X={bounds.size.x:F3}, Y={bounds.size.y:F3}, Z={bounds.size.z:F3}");
                Debug.Log($"  🎯 底面の高さ (Min Y): {bounds.min.y:F3}");
            }
            else
            {
                // Rendererがない場合は、ピボット位置のみ
                Debug.Log($"━━━ オブジェクト名: {obj.name} (Rendererなし) ━━━");
                Debug.Log($"  📍 ピボット位置: X={pivotPosition.x:F3}, Y={pivotPosition.y:F3}, Z={pivotPosition.z:F3}");
                Debug.Log($"  ⚠️  メッシュがないため、Bounds情報は取得できません");
            }
        }

        Debug.Log($"--- 完了: {selectedObjects.Length}個のオブジェクトの座標を出力しました。 ---");
    }
}