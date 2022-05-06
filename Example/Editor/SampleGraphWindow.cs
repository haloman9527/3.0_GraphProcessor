#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#if UNITY_EDITOR
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomGraphWindow(typeof(SampleGraph))]
public class SampleGraphWindow : BaseGraphWindow
{
    protected override BaseGraphView NewGraphView(IGraph graph)
    {
        return new SampleGraphView();
    }

    protected override void BuildToolbar(ToolbarView toolbar)
    {
        base.BuildToolbar(toolbar);
        ToolbarButton btnSave = new ToolbarButton();
        btnSave.text = "Save";
        btnSave.clicked += Save;
        toolbar.AddButtonToRight(btnSave);
    }

    protected override void KeyDownCallback(KeyDownEvent evt)
    {
        base.KeyDownCallback(evt);
        if (evt.commandKey || evt.ctrlKey)
        {
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    Save();
                    evt.StopImmediatePropagation();
                    break;
            }
        }
    }

    void Save()
    {
        if (GraphAsset is IGraphSerialization graphSerialization)
            graphSerialization.SaveGraph(Graph);
        if (GraphOwner is IVariableSerialization variableSerialization)
            variableSerialization.SaveVariables();
        GraphView.SetDirty();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GraphView.SetUndirty();
    }
}
#endif