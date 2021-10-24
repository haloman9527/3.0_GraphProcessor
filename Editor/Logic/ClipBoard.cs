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
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class ClipBoard
    {
        public static List<UnityObject> objectReferences;

        public List<BaseNode> copiedNodes = new List<BaseNode>();
        public List<BaseConnection> copiedEdges = new List<BaseConnection>();
    }
}
#endif