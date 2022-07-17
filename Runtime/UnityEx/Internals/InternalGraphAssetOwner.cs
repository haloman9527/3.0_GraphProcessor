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
using System.Collections.Generic;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor.Internal
{
    public abstract class InternalGraphAssetOwner : MonoBehaviour, IGraphOwner, IGraphAssetOwner
    {
        #region Properties
        public abstract UnityObject GraphAsset { get; }
        public abstract BaseGraphVM Graph { get; }
        #endregion
    }
}