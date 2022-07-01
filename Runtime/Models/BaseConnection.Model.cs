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
    public partial class BaseConnection
    {
        [UnityEngine.HideInInspector] public string from;
        [UnityEngine.HideInInspector] public string fromPortName;
        [UnityEngine.HideInInspector] public string to;
        [UnityEngine.HideInInspector] public string toPortName;
    }
#else
    [Serializable]
    public partial class BaseConnection
    {
        public string from;
        public string fromPortName;
        public string to;
        public string toPortName;
    }
#endif
}
