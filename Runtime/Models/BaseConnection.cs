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
    public class BaseConnection
    {
        [UnityEngine.HideInInspector] public string fromNode;
        [UnityEngine.HideInInspector] public string fromPort;
        [UnityEngine.HideInInspector] public string toNode;
        [UnityEngine.HideInInspector] public string toPort;
    }
#else
    [Serializable]
    public class BaseConnection
    {
        public string fromNode;
        public string fromPort;
        public string toNode;
        public string toPort;
    }
#endif
}
