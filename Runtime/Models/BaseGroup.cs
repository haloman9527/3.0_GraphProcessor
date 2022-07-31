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
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
#if UNITY_5_3_OR_NEWER
    public class BaseGroup
    {
        [UnityEngine.HideInInspector] public string groupName;
        [UnityEngine.HideInInspector] public InternalVector2 position;
        [UnityEngine.HideInInspector] public InternalColor backgroundColor = new InternalColor(0.3f, 0.3f, 0.3f, 0.3f);
        [UnityEngine.HideInInspector] public List<string> nodes = new List<string>();
    }
#else
    public class BaseGroup
    {
        public string groupName;
        public InternalVector2 position;
        public InternalColor backgroundColor = new InternalColor(0.3f, 0.3f, 0.3f, 0.3f);
        public List<string> nodes = new List<string>();
    }
#endif
}