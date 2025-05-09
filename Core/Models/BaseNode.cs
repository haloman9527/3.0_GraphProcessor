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
#if UNITY_5_4_OR_NEWER
    [Serializable]
    public abstract class BaseNode
    {
        [UnityEngine.HideInInspector] public int id;
        [UnityEngine.HideInInspector] public InternalVector2Int position;
    }
#else
    [Serializable]
    public abstract class BaseNode
    {
        public int id;
        public InternalVector2Int position;
    }
#endif
}