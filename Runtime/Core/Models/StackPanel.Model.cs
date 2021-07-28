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
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [Obsolete()]
    public partial class StackPanel
    {
        [SerializeField] string guid;
        [SerializeField] Vector2 position;
        [SerializeField] string title = "New Stack";
        [SerializeField] List<string> nodeGUIDs = new List<string>();
    }
}