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

[Serializable]
public class SampleGraph : BaseGraph { }

[ViewModel(typeof(SampleGraph))]
public class SampleGraphVM : BaseGraphVM
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

    public SampleGraphVM(BaseGraph model) : base(model) { }
}
