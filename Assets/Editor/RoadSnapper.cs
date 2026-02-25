using UnityEngine;
using UnityEditor;

public class RoadSnapper : MonoBehaviour
{
    // 改良版: 上下双方向対応 & 自己吸着防止機能付き
    // 使い方:
    // 1. Hierarchyで道路オブジェクト(tran)を全て選択する。
    // 2. メニューバーの Tools > Snap Roads (Bi-Directional) を実行。

#if UNITY_EDITOR
    [MenuItem("Tools/Snap Roads (Bi-Directional)")]
    public static void Snap()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("エラー", "道路オブジェクトを選択してください。", "OK");
            return;
        }

        // 地形があるレイヤー（demがDefaultレイヤーならそのままでOK）
        int layerMask = Physics.DefaultRaycastLayers;
        
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Snap Roads Bi-Directional");

        int count = 0;

        foreach (var obj in selectedObjects)
        {
            // 進捗表示
            float progress = (float)count / selectedObjects.Length;
            EditorUtility.DisplayProgressBar("地形を捜索中...", $"{obj.name} ({count}/{selectedObjects.Length})", progress);
            count++;

            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf == null) continue;

            // 自分自身のColliderが邪魔をしてレイが当たらないように一時的に無効化
            Collider myCollider = obj.GetComponent<Collider>();
            bool wasEnabled = false;
            if (myCollider != null)
            {
                wasEnabled = myCollider.enabled;
                myCollider.enabled = false;
            }

            Mesh mesh = mf.sharedMesh;
            if (mesh == null) continue;

            Mesh newMesh = Instantiate(mesh);
            newMesh.name = mesh.name + "_SnappedV2";
            Vector3[] vertices = newMesh.vertices;
            Transform tr = obj.transform;
            bool hasChanged = false;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = tr.TransformPoint(vertices[i]);
                Vector3 targetPos = worldPos;
                bool hitFound = false;
                float minDistance = float.MaxValue;

                // 1. 真上を探す (ユーザー様の状況に対応)
                RaycastHit hitUp;
                if (Physics.Raycast(worldPos, Vector3.up, out hitUp, 2000.0f, layerMask))
                {
                    // ヒットしたら距離を記録
                    if (hitUp.distance < minDistance)
                    {
                        minDistance = hitUp.distance;
                        // 地形の表面(hitUp.point)の少し上(+0.15m)
                        targetPos = hitUp.point + Vector3.up * 0.15f;
                        hitFound = true;
                    }
                }

                // 2. 真下を探す (念のため上空5000mから落とす)
                RaycastHit hitDown;
                Vector3 skyPos = worldPos + Vector3.up * 5000.0f;
                // ※自分より上にある地形に遮られないよう、レイの始点に注意が必要だが
                // 今回は「一番近い面」を採用するため、上空からのRayも試行する
                if (Physics.Raycast(skyPos, Vector3.down, out hitDown, 10000.0f, layerMask))
                {
                    float dist = Vector3.Distance(worldPos, hitDown.point);
                    if (dist < minDistance)
                    {
                        // こちらの方が近ければ採用（通常の地面の上にあるケース）
                        targetPos = hitDown.point + Vector3.up * 0.15f;
                        hitFound = true;
                    }
                }

                if (hitFound)
                {
                    vertices[i] = tr.InverseTransformPoint(targetPos);
                    hasChanged = true;
                }
            }

            // Colliderを元に戻す
            if (myCollider != null)
            {
                myCollider.enabled = wasEnabled;
            }

            if (hasChanged)
            {
                newMesh.vertices = vertices;
                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals(); 
                newMesh.RecalculateTangents();

                Undo.RegisterCompleteObjectUndo(obj, "Snap Mesh Assign");
                mf.mesh = newMesh;
                
                // MeshColliderも更新
                if (myCollider is MeshCollider mc)
                {
                    mc.sharedMesh = newMesh;
                }
                EditorUtility.SetDirty(obj);
            }
        }

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        EditorUtility.ClearProgressBar();
        Debug.Log("双方向吸着完了！");
    }
#endif
}