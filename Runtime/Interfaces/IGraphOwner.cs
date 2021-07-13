#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  提供GraphOwner的相关接口
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion

using CZToolKit.Core.SharedVariable;

namespace CZToolKit.GraphProcessor
{
    public interface IGraphOwner : IVariableOwner
    {
        BaseGraph Graph { get; }

        void SaveVariables();
    }
}