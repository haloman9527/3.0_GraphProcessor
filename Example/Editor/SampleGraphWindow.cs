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
using CZToolKit.Core;
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomGraphWindow(typeof(SampleGraph))]
public class SampleGraphWindow : BaseGraphWindow
{
    protected override BaseGraphView NewGraphView(BaseGraph graph)
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
                    break;
            }
        }
    }

    void Save()
    {
        if (GraphAsset is IGraphAsset graphAsset)
        {
            graphAsset.SaveGraph(Graph);
        }
        if (GraphOwner is IGraphOwner graphOwner)
        {
            graphOwner.SaveVariables();
        }
        GraphView.SetDirty(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GraphView.UnsetDirty();
    }
}
