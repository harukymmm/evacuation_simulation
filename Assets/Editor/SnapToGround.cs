using UnityEngine;
using UnityEditor;

public class SnapToGround : MonoBehaviour
{
    // 実行方法:
    // 1. Hierarchyで「dem（地形）」にMeshColliderがついていることを確認する。
    // 2. 「tran（道路）」オブジェクトを選択する。
    // 3. 上部メニューの Tools > Snap Roads to Ground を実行。

#if UNITY_EDITOR
    [MenuItem("Tools/Snap Roads to Ground")]
    public static void Snap()
    {
        // 選択されたオブジェクトを取得
        GameObject[] selectedObjects = Selection.gameObjects;
        
        // 地形のレイヤーマスク（Defaultなど、demがあるレイヤーを指定）
        // ※demには必ずColliderをつけておくこと！
        int layerMask = Physics.DefaultRaycastLayers;

        // 調整パラメータ
        const float rayStartHeight = 50.0f;   // 頂点からどれだけ上空から撃つか
        const float rayDistance    = 200.0f;  // レイの最大距離（高低差が大きい場合に備える）
        const float roadClearance  = 1.0f;   // 道路を浮かせる量
        const float normalOffset   = 0.02f;   // 法線方向に押し出す量（傾斜面でのめり込み防止）

        foreach (var obj in selectedObjects)
        {
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null) continue;

            Mesh mesh = mf.sharedMesh;
            // メッシュをコピー（元データを壊さないため）
            Mesh newMesh = Instantiate(mesh);
            Vector3[] vertices = newMesh.vertices;

            // ローカル座標系変換用
            Transform tr = obj.transform;

            float clearance = roadClearance;

            for (int i = 0; i < vertices.Length; i++)
            {
                // 頂点のワールド座標を取得
                Vector3 worldPos = tr.TransformPoint(vertices[i]);

                // その頂点の上空から真下にレイを飛ばす
                RaycastHit hit;
                if (Physics.Raycast(worldPos + Vector3.up * rayStartHeight, Vector3.down, out hit, rayDistance, layerMask))
                {
                    // ヒットした地点の少し上に座標を更新（浮かせ量 + 法線方向に少し押し出し）
                    Vector3 newWorldPos = hit.point + Vector3.up * clearance + hit.normal * normalOffset;
                    vertices[i] = tr.InverseTransformPoint(newWorldPos);
                }
            }

            // メッシュ更新
            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            
            mf.mesh = newMesh; // 置き換え
            
            // 保存（シーンを保存できるように）
            Undo.RegisterCreatedObjectUndo(newMesh, "Snap Mesh");
            EditorUtility.SetDirty(obj);
        }
        Debug.Log("道路の吸着完了！");
    }
#endif
}
