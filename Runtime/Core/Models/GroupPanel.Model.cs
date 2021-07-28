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
    [Serializable]
    public sealed partial class GroupPanel : IntegratedViewModel
    {
        [SerializeField] string title;
        [SerializeField] Color color = new Color(0, 0, 0, 0.7f);
        [SerializeField] Rect position;

        [SerializeField] List<string> innerNodeGUIDs = new List<string>();
        //[SerializeField] List<string> innerStackGUIDs = new List<string>();
    }
}