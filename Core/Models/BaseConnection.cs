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
        [UnityEngine.HideInInspector] public long fromNode;
        [UnityEngine.HideInInspector] public string fromPort;
        [UnityEngine.HideInInspector] public long toNode;
        [UnityEngine.HideInInspector] public string toPort;
    }
#else
    [Serializable]
    public class BaseConnection
    {
        public long fromNode;
        public string fromPort;
        public long toNode;
        public string toPort;
    }
#endif
}