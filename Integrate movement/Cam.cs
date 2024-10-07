using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class CameraCaptureTool : EditorWindow
{
    private List<Camera> targetCameras = new List<Camera>();
    private string saveFolderPath = "CapturedImages"; // 更新路径为相对于项目根目录的相对路径
    private int width = 1080;
    private int height = 1440; // 默认比例为 3:4

    [MenuItem("整合插件/照相机")]
    public static void ShowWindow()
    {
        CameraCaptureTool window = GetWindow<CameraCaptureTool>("照相机");
        window.LoadPreferences();
    }

    private void OnGUI()
    {
        GUILayout.Label("捕捉设置", EditorStyles.boldLabel);

        if (GUILayout.Button("选择场景中的所有摄像机（除 Main Camera）"))
        {
            targetCameras.Clear();
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCameras)
            {
                if (cam != null && cam.name != "Main Camera")
                {
                    targetCameras.Add(cam);
                }
            }
        }

        GUILayout.Label("目标摄像机", EditorStyles.boldLabel);
        for (int i = 0; i < targetCameras.Count; i++)
        {
            targetCameras[i] = (Camera)EditorGUILayout.ObjectField(targetCameras[i], typeof(Camera), true);
        }

        width = EditorGUILayout.IntField("宽度", width);
        height = EditorGUILayout.IntField("高度", height);
        saveFolderPath = EditorGUILayout.TextField("保存文件夹路径", saveFolderPath);

        if (GUILayout.Button("捕捉截图"))
        {
            if (targetCameras.Count > 0)
            {
                CaptureCameras();
                SavePreferences();
            }
            else
            {
                Debug.LogError("请至少选择一个目标摄像机。");
            }
        }
    }

    private void CaptureCameras()
    {
        string absoluteSavePath = Path.Combine(Application.dataPath, saveFolderPath);
        if (!Directory.Exists(absoluteSavePath))
        {
            Directory.CreateDirectory(absoluteSavePath);
        }

        for (int i = 0; i < targetCameras.Count; i++)
        {
            Camera camera = targetCameras[i];
            if (camera == null) continue;

            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            camera.targetTexture = renderTexture;
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);

            camera.Render();

            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();

            camera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(renderTexture);

            byte[] bytes = screenshot.EncodeToPNG();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(absoluteSavePath, $"{camera.name}_{timestamp}.png");
            File.WriteAllBytes(filePath, bytes);

            Debug.Log("截图已保存到 " + filePath);
        }

        AssetDatabase.Refresh();
    }

    private void SavePreferences()
    {
        EditorPrefs.SetString("CameraCaptureTool_SaveFolderPath", saveFolderPath);
    }

    private void LoadPreferences()
    {
        saveFolderPath = EditorPrefs.GetString("CameraCaptureTool_SaveFolderPath", "CapturedImages");
    }
}