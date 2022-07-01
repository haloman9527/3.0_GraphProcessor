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

namespace CZToolKit.GraphProcessor
{
#if UNITY_5_3_OR_NEWER
    [Serializable]
    public partial class BasePort
    {
        #region Define
        public enum Direction
        {
            Input,
            Output
        }
        public enum Orientation
        {
            Horizontal,
            Vertical
        }
        public enum Capacity
        {
            Single,
            Multi
        }
        #endregion

        [UnityEngine.HideInInspector] public readonly string name;
        [UnityEngine.HideInInspector] public readonly Orientation orientation;
        [UnityEngine.HideInInspector] public readonly Direction direction;
        [UnityEngine.HideInInspector] public readonly Capacity capacity;
        [UnityEngine.HideInInspector] public Type type;
    }
#else
    [Serializable]
    public partial class BasePort
    {
    #region Define
        public enum Direction
        {
            Input,
            Output
        }
        public enum Orientation
        {
            Horizontal,
            Vertical
        }
        public enum Capacity
        {
            Single,
            Multi
        }
    #endregion

        public readonly string name;
        public readonly Orientation orientation;
        public readonly Direction direction;
        public readonly Capacity capacity;
        public Type type;
    }
#endif

}
