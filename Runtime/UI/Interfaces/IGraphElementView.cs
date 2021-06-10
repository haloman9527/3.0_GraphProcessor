#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion

namespace CZToolKit.GraphProcessor
{
    public interface IGraphElementView
    {
        void SetUp(IGraphElement _graphElement, CommandDispatcher _commandDispatcher, IGraphView _graphView);
    }
}
