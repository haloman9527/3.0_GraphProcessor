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

namespace Jiange.GraphProcessor
{
    [Serializable]
    public abstract class BaseNode
    {
        public int id;
        public InternalVector2Int position;
    }
}