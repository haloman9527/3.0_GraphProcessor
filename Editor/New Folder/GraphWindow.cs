#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class GraphWindow : EditorWindow
{
    [MenuItem("Window/UIElements/GraphWindow")]
    public static void ShowExample()
    {
        GraphWindow wnd = GetWindow<GraphWindow>();
        wnd.titleContent = new GUIContent("GraphWindow");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/CZToolKit/3.0_GraphProcessor/Editor/New Folder/GraphWindow.uxml");
        visualTree.CloneTree(root);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CZToolKit/3.0_GraphProcessor/Editor/New Folder/GraphWindow.uss");
        root.styleSheets.Add(styleSheet);
    }
}