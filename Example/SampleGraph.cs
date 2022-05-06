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
using CZToolKit.Core.SharedVariable;
using CZToolKit.GraphProcessor;
using System;
using System.Collections.Generic;

public class SampleGraph : BaseGraph, IGraphForMono
{
    [NonSerialized] internal List<SharedVariable> variables;

    public IGraphOwner GraphOwner
    {
        get;
        private set;
    }
    public IVariableOwner VarialbeOwner
    {
        get { return GraphOwner as IVariableOwner; }
    }
    public IReadOnlyList<SharedVariable> Variables
    {
        get { return variables; }
    }

    protected override void OnEnabled()
    {
        base.OnEnabled();

        onNodeAdded += OnNodeAdded;
    }

    public void Initialize(IGraphOwner graphOwner)
    {
        GraphOwner = graphOwner;

        foreach (var node in Nodes.Values)
        {
            if (node is INodeForMono monoNode)
                monoNode.Initialize();
        }

        variables = new List<SharedVariable>();
        foreach (var node in Nodes.Values)
        {
            variables.AddRange(SharedVariableUtility.CollectionObjectSharedVariables(node));
        }
        foreach (var variable in variables)
        {
            variable.InitializePropertyMapping(VarialbeOwner);
        }

        OnInitialized();
    }

    protected virtual void OnInitialized() { }

    public void OnNodeAdded(BaseNode node)
    {
        if (!(node is INodeForMono monoNode))
            return;
        if (GraphOwner != null)
            monoNode.Initialize();

        IEnumerable<SharedVariable> nodeVariables = SharedVariableUtility.CollectionObjectSharedVariables(node);
        variables.AddRange(nodeVariables);
        if (VarialbeOwner != null)
        {
            foreach (var variable in nodeVariables)
            {
                variable.InitializePropertyMapping(VarialbeOwner);
            }
        }
    }
}
