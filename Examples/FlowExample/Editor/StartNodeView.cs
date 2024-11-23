#if UNITY_EDITOR
using Jiange.GraphProcessor.Editors;
using JiangeEditor;
using UnityEditor;
using UnityEngine.UIElements;

[CustomView(typeof(StartNode))]
public class StartNodeView : FlowNodeView
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // this.SetMovable(false);
    }
}
#endif