using UnityEngine;
using UnityEditor;

public class FlattenCoordinates : MonoBehaviour
{
    // SDKの仕様に合わせて、強制的に合わせる高さ（0.0f）
    private const float TARGET_HEIGHT = 0.0f;

    [MenuItem("Tools/PLATEAU/Flatten Objects to Y=0 (SDK Style)")]
    public static void FlattenSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("エラー", "オブジェクトを選択してください。", "OK");
            return;
        }

        int count = 0;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Flatten to Y=0");

        foreach (var obj in selectedObjects)
        {
            // 進捗バー
            float progress = (float)count / selectedObjects.Length;
            EditorUtility.DisplayProgressBar("座標をY=0に投影中...", $"{obj.name}", progress);
            count++;

            // 1. Static（静的）フラグを強制解除
            // これをしないと、座標を変えても見た目が元の場所に残ってしまいます
            GameObjectUtility.SetStaticEditorFlags(obj, 0);

            // 2. 「底面」の高さを計算して移動
            // 単純に position.y = 0 だと、Pivotがズレている場合に浮いたり沈んだりするため、
            // 「見た目の底面（Mesh Bounds Bottom）」を基準にします。
            Transform tr = obj.transform;
            Renderer rend = obj.GetComponent<Renderer>();

            float offset = 0f;
            
            if (rend != null)
            {
                // メッシュがある場合は、その「最下点」を基準にする
                float currentBottomY = rend.bounds.min.y;
                offset = TARGET_HEIGHT - currentBottomY;
            }
            else
            {
                // メッシュがない（親オブジェクトなど）場合は、Pivotを基準にする
                offset = TARGET_HEIGHT - tr.position.y;
            }

            // 3. 座標更新
            if (Mathf.Abs(offset) > 0.0001f)
            {
                Undo.RecordObject(tr, "Flatten Move");
                tr.position += new Vector3(0, offset, 0);
                EditorUtility.SetDirty(obj);
            }
        }

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        EditorUtility.ClearProgressBar();
        Debug.Log($"完了: {count}個のオブジェクトを Y={TARGET_HEIGHT} 平面に投影しました。");
    }
}