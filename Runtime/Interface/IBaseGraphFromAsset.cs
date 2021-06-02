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
using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    public interface IBaseGraphFromAsset
    {
        UnityObject From { get; }

        void SetFrom(UnityObject _from);
    }
}
