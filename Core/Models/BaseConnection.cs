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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;

namespace Atom.GraphProcessor
{
#if UNITY_5_3_OR_NEWER
    [Serializable]
    public class BaseConnection
    {
        [UnityEngine.HideInInspector] public int fromNode;
        [UnityEngine.HideInInspector] public string fromPort;
        [UnityEngine.HideInInspector] public int toNode;
        [UnityEngine.HideInInspector] public string toPort;
    }
#else
    [Serializable]
    public class BaseConnection
    {
        public int fromNode;
        public string fromPort;
        public int toNode;
        public string toPort;
    }
#endif
}