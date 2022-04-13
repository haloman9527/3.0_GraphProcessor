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
using System;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphOwner : IVariableOwner
    {
        IGraph Graph { get; }
        Type GraphType { get; }

        void SaveVariables();
    }
}