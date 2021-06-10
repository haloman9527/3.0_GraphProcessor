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
using CZToolKit.Core.SharedVariable;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphAssetOwner 
    {
        BaseGraphAsset GraphAsset { get; }
    }
}
