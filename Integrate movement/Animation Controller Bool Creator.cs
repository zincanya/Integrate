using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Globalization;

public class AnimationControllerCreator : EditorWindow
{
    private string controllerPath = ""; // 动画控制器路径
    private string[] animationClipPaths = new string[] { "" }; // 动画剪辑路径数组
    private int animationClipCount = 1; // 动画剪辑数量
    private static bool isEnglish = true; // 系统语言是否为英语

    [MenuItem("整合插件/动画控制器插件/Bool")]
    public static void ShowWindow()
    {
        isEnglish = CultureInfo.CurrentCulture.Name.StartsWith("en");
        GetWindow<AnimationControllerCreator>(isEnglish ? "Animation Controller Creator" : "动画控制器创建器");
    }

    private void OnGUI()
    {
        GUILayout.Label(isEnglish ? "Animation Controller Path" : "动画控制器路径", EditorStyles.boldLabel);
        controllerPath = EditorGUILayout.TextField(isEnglish ? "Controller Path" : "控制器路径", controllerPath);

        GUILayout.Label(isEnglish ? "Animation Clips" : "动画剪辑", EditorStyles.boldLabel);
        animationClipCount = EditorGUILayout.IntSlider(isEnglish ? "Number of Animations" : "动画数量", animationClipCount, 1, 25);

        if (animationClipPaths.Length != animationClipCount)
        {
            System.Array.Resize(ref animationClipPaths, animationClipCount);
        }

        for (int i = 0; i < animationClipCount; i++)
        {
            animationClipPaths[i] = EditorGUILayout.TextField(isEnglish ? $"Animation {i + 1}" : $"动画 {i + 1}", animationClipPaths[i]);
        }

        if (GUILayout.Button(isEnglish ? "Create Animation Controller" : "创建动画控制器"))
        {
            CreateAnimationController();
        }
    }

    private void CreateAnimationController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError(isEnglish ? "Invalid controller path" : "无效的控制器路径");
            return;
        }

        Undo.RecordObject(controller, "Create Animation Layers");

        for (int i = 0; i < animationClipCount; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animationClipPaths[i]);
            if (clip == null)
            {
                Debug.LogError(isEnglish ? $"Invalid animation path: {animationClipPaths[i]}" : $"无效的动画路径: {animationClipPaths[i]}");
                continue;
            }

            string animationName = System.IO.Path.GetFileNameWithoutExtension(animationClipPaths[i]);

            // 创建一个新的动画层
            var layerName = animationName;
            controller.AddLayer(layerName);

            var layer = controller.layers[controller.layers.Length - 1];
            layer.defaultWeight = 1;

            // 创建一个默认的空闲状态
            var idleState = layer.stateMachine.AddState(isEnglish ? "Idle" : "空闲");
            layer.stateMachine.defaultState = idleState;

            // 创建一个新的动画状态
            var animationState = layer.stateMachine.AddState(animationName);
            animationState.motion = clip;

            // 从空闲状态过渡到动画状态
            var transitionToState = idleState.AddTransition(animationState);
            transitionToState.hasExitTime = false;
            transitionToState.duration = 0;
            transitionToState.AddCondition(AnimatorConditionMode.If, 0, $"分件_{i + 1}");

            // 从动画状态过渡回空闲状态
            var transitionToIdle = animationState.AddTransition(idleState);
            transitionToIdle.hasExitTime = true;
            transitionToIdle.exitTime = 1.0f;
            transitionToIdle.duration = 0;
            transitionToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, $"分件_{i + 1}");

            // 添加布尔参数来控制动画
            controller.AddParameter($"分件_{i + 1}", AnimatorControllerParameterType.Bool);

            // 更新层信息
            controller.layers[controller.layers.Length - 1] = layer;
        }

        // 在所有层创建完成后，设置每个层的权重为1
        for (int i = 0; i < controller.layers.Length; i++)
        {
            var layer = controller.layers[i];
            layer.defaultWeight = 1;
            controller.layers[i] = layer;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(isEnglish ? "Animation Controller created successfully" : "动画控制器创建成功");
    }
}
