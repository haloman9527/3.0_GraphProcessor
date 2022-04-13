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
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public interface INode
    {
        event Action<BasePort> onPortAdded;
        event Action<BasePort> onPortRemoved;

        IGraph Owner { get; }
        string GUID { get; }
        IReadOnlyDictionary<string, BasePort> Ports { get; }
        string Title { get; set; }
        Color TitleColor { get; set; }
        string Tooltip { get; set; }
        Vector2 Position { get; set; }

        IEnumerable<INode> GetConnections(string portName);
        void AddPort(BasePort port);
        void RemovePort(BasePort port);
        void RemovePort(string portName);
    }
}
