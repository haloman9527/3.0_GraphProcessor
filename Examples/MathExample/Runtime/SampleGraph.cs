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
using System;

[Serializable]
public class SampleGraph : BaseGraph { }

[ViewModel(typeof(SampleGraph))]
public class SampleGraphVM : BaseGraphVM
{
    public SampleGraphVM(BaseGraph model) : base(model) { }
}
