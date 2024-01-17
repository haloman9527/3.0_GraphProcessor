using System.Collections.Generic;
using CZToolKit.GraphProcessor;
using CZToolKit;
using Sirenix.OdinInspector;

[NodeMenu("SVN Update")]
public class SVNUpdateNode : FlowNode
{
    [FolderPath]
    [LabelText("目标文件夹")]
    public List<string> folders = new List<string>();
}

[ViewModel(typeof(SVNUpdateNode))]
public class SvnUpdateNodeProcessor : FlowNodeProcessor
{
    public SvnUpdateNodeProcessor(SVNUpdateNode model) : base(model)
    {
        this[nameof(SVNUpdateNode.folders)] = new BindableList<string>(() => model.folders, v => model.folders = v);
    }

    protected override void Execute()
    {
        FlowNext();
    }
}
