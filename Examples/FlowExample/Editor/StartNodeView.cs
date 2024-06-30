#if UNITY_EDITOR
using CZToolKit.GraphProcessor.Editors;
using CZToolKitEditor;
using UnityEditor;
using UnityEngine.UIElements;

[CustomView(typeof(StartNode))]
public class StartNodeView : FlowNodeView
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.SetMovable(false);
    }
}
#endif