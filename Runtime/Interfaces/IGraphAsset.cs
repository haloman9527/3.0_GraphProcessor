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
    public interface IGraphAsset
    {
        BaseGraph Graph { get; }

        void SaveGraph();

        void CheckGraphSerialization();
    }
}