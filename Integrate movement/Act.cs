using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

public class RemoveUnusedAnimatorParameters : EditorWindow
{
    private AnimatorController animatorController;

    [MenuItem("整合插件/删除未使用的Animator参数")]
    public static void ShowWindow()
    {
        GetWindow<RemoveUnusedAnimatorParameters>("删除未使用的Animator参数");
    }

    private void OnGUI()
    {
        GUILayout.Label("选择要清理的Animator Controller", EditorStyles.boldLabel);
        animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);

        if (animatorController != null)
        {
            if (GUILayout.Button("删除未使用的参数"))
            {
                RemoveUnusedParameters();
            }
        }
    }

    private void RemoveUnusedParameters()
    {
        var usedParameters = new HashSet<string>();

        // 遍历所有层
        foreach (var layer in animatorController.layers)
        {
            // 遍历所有状态机中的状态
            TraverseStates(layer.stateMachine, usedParameters);
        }

        // 获取所有参数名称
        var allParameters = new HashSet<string>();
        foreach (var param in animatorController.parameters)
        {
            allParameters.Add(param.name);
        }

        // 找出未使用的参数
        allParameters.ExceptWith(usedParameters);

        // 删除未使用的参数
        foreach (var paramName in allParameters)
        {
            AnimatorControllerParameter paramToRemove = null;
            foreach (var param in animatorController.parameters)
            {
                if (param.name == paramName)
                {
                    paramToRemove = param;
                    break;
                }
            }
            if (paramToRemove != null)
            {
                animatorController.RemoveParameter(paramToRemove);
                Debug.Log($"已删除未使用的参数：{paramName}");
            }
        }

        // 保存更改
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("完成", "未使用的参数已删除", "确定");
    }

    private void TraverseStates(AnimatorStateMachine stateMachine, HashSet<string> usedParameters)
    {
        // 检查过渡条件中的参数
        foreach (var state in stateMachine.states)
        {
            foreach (var transition in state.state.transitions)
            {
                foreach (var condition in transition.conditions)
                {
                    usedParameters.Add(condition.parameter);
                }
            }
        }

        // 检查AnyState的过渡
        foreach (var transition in stateMachine.anyStateTransitions)
        {
            foreach (var condition in transition.conditions)
            {
                usedParameters.Add(condition.parameter);
            }
        }

        // 检查Entry的过渡
        foreach (var transition in stateMachine.entryTransitions)
        {
            foreach (var condition in transition.conditions)
            {
                usedParameters.Add(condition.parameter);
            }
        }

        // 递归遍历子状态机
        foreach (var childStateMachine in stateMachine.stateMachines)
        {
            TraverseStates(childStateMachine.stateMachine, usedParameters);
        }
    }
}
