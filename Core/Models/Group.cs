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
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    [Serializable]
    public sealed class Group
    {
        public long id;
        public string groupName;
        public InternalVector2Int position;
        public InternalVector2Int size;
        public InternalColor backgroundColor = new InternalColor(0.3f, 0.3f, 0.3f, 0.3f);
        public List<long> nodes = new List<long>();
    }
}