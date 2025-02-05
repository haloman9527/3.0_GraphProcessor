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

namespace Moyo.GraphProcessor
{
    [Serializable]
    public sealed class Group
    {
        public int id;
        public string groupName;
        public InternalVector2Int position;
        public InternalVector2Int size;
        public InternalColor backgroundColor = new InternalColor(0.3f, 0.3f, 0.3f, 0.3f);
        public List<int> nodes = new List<int>();
    }
}