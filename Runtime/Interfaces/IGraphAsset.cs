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

using System;
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphAsset
    {
        IGraph Graph { get; }

        void SaveGraph();

        void CheckGraphSerialization();
    }
}