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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion
using Moyo;
using Moyo.GraphProcessor;
using System;

[Serializable]
public class SampleGraph : BaseGraph { }

[ViewModel(typeof(SampleGraph))]
public class SampleGraphProcessor : BaseGraphProcessor
{
    public SampleGraphProcessor(BaseGraph model) : base(model) { }
}
