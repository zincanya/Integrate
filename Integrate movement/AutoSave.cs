using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

[InitializeOnLoad]
public class SmallFeaturesPlugin : EditorWindow
{
    private static bool isAutoSaveEnabled;
    private static float saveIntervalMinutes;
    private static double lastSaveTime;

    private string renamePrefix = string.Empty;
    private string renameSuffix = string.Empty;

    private enum FeatureType
    {
        自动保存场景 = 0,
        创建分割线 = 1,
        批量重命名 = 2,
        检查三角形数量 = 3
    }

    private FeatureType selectedFeature = FeatureType.自动保存场景;

    static SmallFeaturesPlugin()
    {
        // 注册事件
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += OnEditorQuitting;
        EditorApplication.update += OnEditorUpdate;
    }

    [MenuItem("整合插件/功能统合")]
    private static void Init()
    {
        SmallFeaturesPlugin window = GetWindow<SmallFeaturesPlugin>("小功能插件");
        window.Show();
    }

    private void OnEnable()
    {
        LoadSettings();
        lastSaveTime = EditorApplication.timeSinceStartup;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        selectedFeature = (FeatureType)EditorGUILayout.EnumPopup("选择功能：", selectedFeature);
        EditorGUILayout.Space();

        switch (selectedFeature)
        {
            case FeatureType.自动保存场景:
                AutoSaveSceneUI();
                break;
            case FeatureType.创建分割线:
                CreateDividerUI();
                break;
            case FeatureType.批量重命名:
                BatchRenameUI();
                break;
            case FeatureType.检查三角形数量:
                CheckTriangleCountUI();
                break;
        }
    }

    #region 功能界面方法

    private void AutoSaveSceneUI()
    {
        EditorGUILayout.LabelField("自动保存场景设置", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        bool newAutoSaveEnabled = EditorGUILayout.Toggle("启用自动保存", isAutoSaveEnabled);
        if (newAutoSaveEnabled != isAutoSaveEnabled)
        {
            isAutoSaveEnabled = newAutoSaveEnabled;
            SaveSettings();
        }

        EditorGUI.BeginDisabledGroup(!isAutoSaveEnabled);
        float newSaveInterval = EditorGUILayout.Slider("保存间隔（分钟）", saveIntervalMinutes, 1f, 30f);
        if (Mathf.Abs(newSaveInterval - saveIntervalMinutes) > Mathf.Epsilon)
        {
            saveIntervalMinutes = newSaveInterval;
            SaveSettings();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        if (GUILayout.Button("立即保存", GUILayout.Height(30)))
        {
            SaveScene();
            EditorUtility.DisplayDialog("自动保存", "场景已成功保存。", "确定");
        }
    }

    private void CreateDividerUI()
    {
        EditorGUILayout.LabelField("创建分割线", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("在场景中创建分割线", GUILayout.Height(30)))
        {
            CreateDivider();
        }
    }

    private void BatchRenameUI()
    {
        EditorGUILayout.LabelField("批量重命名", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        renamePrefix = EditorGUILayout.TextField("前缀：", renamePrefix);
        renameSuffix = EditorGUILayout.TextField("后缀：", renameSuffix);

        EditorGUILayout.Space();

        if (GUILayout.Button("开始批量重命名", GUILayout.Height(30)))
        {
            if (ValidateBatchRenameInput())
            {
                BatchRename();
                EditorUtility.DisplayDialog("批量重命名", "批量重命名已成功完成。", "确定");
            }
        }
    }

    private void CheckTriangleCountUI()
    {
        EditorGUILayout.LabelField("检查三角形数量", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("检查所选对象的总三角形数", GUILayout.Height(30)))
        {
            CheckTriangleCount();
        }
    }

    #endregion

    #region 功能实现方法

    private static void SaveScene()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.SaveOpenScenes();
            lastSaveTime = EditorApplication.timeSinceStartup;
            Debug.Log("场景已自动保存于: " + DateTime.Now);
        }
    }

    private void CreateDivider()
    {
        Transform selectedTransform = Selection.activeTransform;

        if (selectedTransform != null)
        {
            GameObject dividerObject = new GameObject("---------------------------------");
            Undo.RegisterCreatedObjectUndo(dividerObject, "创建分割线");
            dividerObject.transform.SetParent(selectedTransform, false);
            Selection.activeGameObject = dividerObject;
        }
    }

    private bool ValidateBatchRenameInput()
    {
        if (string.IsNullOrEmpty(renamePrefix) && string.IsNullOrEmpty(renameSuffix))
        {
            EditorUtility.DisplayDialog("输入无效", "请输入前缀或后缀。", "确定");
            return false;
        }

        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("没有选择对象", "请先选择一个要重命名的对象。", "确定");
            return false;
        }

        return true;
    }

    private void BatchRename()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("没有选择对象", "请先选择要重命名的对象。", "确定");
            return;
        }

        Undo.RecordObjects(selectedObjects, "批量重命名");

        foreach (var obj in selectedObjects)
        {
            string originalName = obj.name;
            obj.name = $"{renamePrefix}{originalName}{renameSuffix}";
        }
    }

    private void CheckTriangleCount()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            int totalTriangleCount = 0;

            MeshFilter[] meshFilters = selectedObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (var filter in meshFilters)
            {
                if (filter.sharedMesh != null)
                {
                    totalTriangleCount += filter.sharedMesh.triangles.Length / 3;
                }
            }

            SkinnedMeshRenderer[] skinnedMeshRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer.sharedMesh != null)
                {
                    totalTriangleCount += renderer.sharedMesh.triangles.Length / 3;
                }
            }

            EditorUtility.DisplayDialog("三角形数量",
                $"所选对象及其子对象的总三角形数：{totalTriangleCount}", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("没有选择对象", "请先选择一个要检查的对象。", "确定");
        }
    }

    #endregion

    #region 设置持久化

    private static void LoadSettings()
    {
        isAutoSaveEnabled = EditorPrefs.GetBool("SFP_IsAutoSaveEnabled", true);
        saveIntervalMinutes = EditorPrefs.GetFloat("SFP_SaveIntervalMinutes", 5f);
    }

    private static void SaveSettings()
    {
        EditorPrefs.SetBool("SFP_IsAutoSaveEnabled", isAutoSaveEnabled);
        EditorPrefs.SetFloat("SFP_SaveIntervalMinutes", saveIntervalMinutes);
    }

    #endregion

    #region 事件处理

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SaveScene();
            Debug.Log("进入游戏模式前自动保存场景于: " + DateTime.Now);
        }
    }

    private static void OnEditorQuitting()
    {
        if (isAutoSaveEnabled)
        {
            SaveScene();
            Debug.Log("退出编辑器时自动保存场景于: " + DateTime.Now);
        }
    }

    private static void OnEditorUpdate()
    {
        if (isAutoSaveEnabled && EditorApplication.timeSinceStartup - lastSaveTime > saveIntervalMinutes * 60)
        {
            SaveScene();
            Debug.Log("定时自动保存场景于: " + DateTime.Now);
        }
    }

    #endregion
}
