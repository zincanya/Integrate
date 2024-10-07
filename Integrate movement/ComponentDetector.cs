using UnityEngine;
using UnityEditor;

public class ComponentDetector
{
    [MenuItem("整合插件/检测选中对象的组件")]
    static void DetectComponents()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.Log("未选择任何游戏对象。");
            return;
        }

        bool componentsFound = false;

        foreach (GameObject obj in selectedObjects)
        {
            Component[] components = obj.GetComponents<Component>();

            // 排除 Transform 组件
            Component[] nonTransformComponents = System.Array.FindAll(components, c => !(c is Transform));

            if (nonTransformComponents.Length > 0)
            {
                componentsFound = true;
                string componentNames = "";
                foreach (Component comp in nonTransformComponents)
                {
                    componentNames += comp.GetType().Name + ", ";
                }
                // 去掉最后的逗号和空格
                if (componentNames.Length > 2)
                    componentNames = componentNames.Substring(0, componentNames.Length - 2);

                Debug.Log($"游戏对象 '{obj.name}' 上的组件: {componentNames}");
            }
        }

        if (!componentsFound)
        {
            Debug.Log("没有发现组件");
        }
    }
}
