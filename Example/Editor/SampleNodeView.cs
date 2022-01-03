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
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SampleNodeView : BaseNodeView
{
    public string portName;
    Button btnAddPort;
    public SampleNodeView()
    {
        btnAddPort = new Button();
        controlsContainer.Add(btnAddPort);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        controlsContainer.Add(new IMGUIContainer(OnGUI));
        btnAddPort.clicked += OnClick;
    }

    private void OnClick()
    {
        Owner.CommandDispacter.Do(new AddPortCommand(Model, portName, BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Single, typeof(object)));
    }

    private void OnGUI()
    {
        portName = GUILayout.TextField(portName);
    }
}
