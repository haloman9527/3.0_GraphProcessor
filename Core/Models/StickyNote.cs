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
    [Serializable]
    public sealed class StickyNote
    {
        public int id;
        public InternalVector2Int position;
        public InternalVector2Int size;
        public string title;
        public string content;
    }
}
