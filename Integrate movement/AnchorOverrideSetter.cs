using UnityEngine;
using UnityEditor;

public class AnchorOverrideSetter : EditorWindow
{
    private Transform targetAnchor;

    [MenuItem("整合插件/统一光照锚点")]
    public static void ShowWindow()
    {
        GetWindow<AnchorOverrideSetter>("设置光照锚点");
    }

    private void OnEnable()
    {
        // 尝试自动设置默认锚点
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            Transform hipsTransform = selectedObject.transform.Find("Armature/Hips");
            if (hipsTransform != null)
            {
                targetAnchor = hipsTransform;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("光照锚点设置", EditorStyles.boldLabel);
        targetAnchor = (Transform)EditorGUILayout.ObjectField("目标锚点", targetAnchor, typeof(Transform), true);

        if (GUILayout.Button("设置光照锚点"))
        {
            SetAnchorOverride();
        }
    }

    private void SetAnchorOverride()
    {
        if (targetAnchor == null)
        {
            Debug.LogWarning("请指定一个目标锚点。");
            return;
        }

        // 获取当前选中的游戏对象
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogWarning("请在层级视图中选择一个父对象。");
            return;
        }

        // 在当前选中的层级下遍历所有子对象的 SkinnedMeshRenderer 组件
        SkinnedMeshRenderer[] skinnedMeshRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            // 设置 Anchor Override 为指定的锚点
            renderer.probeAnchor = targetAnchor;
            Debug.Log($"已将 {renderer.gameObject.name} 的光照锚点设置为 {targetAnchor.name}");
        }

        Debug.Log("光照锚点设置完成。");
    }
}
