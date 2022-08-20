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

namespace CZToolKit.GraphProcessor
{
#if UNITY_5_3_OR_NEWER
    [Serializable]
    public abstract class BaseNode
    {
        [UnityEngine.HideInInspector] public InternalVector2 position;
    }
#else
    [Serializable]
    public abstract class BaseNode
    {
        public InternalVector2 position;
    }
#endif
}
