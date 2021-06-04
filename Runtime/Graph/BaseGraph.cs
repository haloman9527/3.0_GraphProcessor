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
    [Serializable]
    public class BaseGraph : AbstrackBaseGraph, IBaseGraphFromAsset
    {
        #region 变量
        [NonSerialized]
        UnityObject asset;
        #endregion

        #region 属性
        /// <summary> 图的所有者，即SO对象 </summary>
        public UnityObject Asset { get { return asset; } }
        #endregion

        public void Enable(UnityObject _asset)
        {
            // 保存_owner的引用
            asset = _asset;
        }

        protected virtual void OnEnable() { }
    }
}
