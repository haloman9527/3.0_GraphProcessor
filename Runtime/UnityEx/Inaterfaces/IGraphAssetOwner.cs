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
using System;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphAssetOwner : IGraphOwner
    {
        UnityObject GraphAsset { get; }
    }
}
