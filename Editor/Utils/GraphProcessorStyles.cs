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
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class GraphProcessorStyles
    {
        public const string BassicStyleFile = "GraphProcessor/Styles/BasicStyle";
        public const string GraphViewStyleFile = "GraphProcessor/Styles/BaseGraphView";
        public const string BaseNodeViewStyleFile = "GraphProcessor/Styles/BaseNodeView";
        public const string SimpleNodeViewStyleFile = "GraphProcessor/Styles/SimpleNodeView";
        public const string SettingNodeViewStyleFile = "GraphProcessor/Styles/SettingsNodeView";
        public const string PortViewStyleFile = "GraphProcessor/Styles/PortView";
        public const string PortViewTypesStyleFile = "GraphProcessor/Styles/PortViewTypes";
        public const string EdgeStyleFile = "GraphProcessor/Styles/EdgeView";
        public const string GroupStyleFile = "GraphProcessor/Styles/GroupView";
        public const string NodeSettingViewStyleFile = "GraphProcessor/Styles/NodeSettings";
        public const string NodeSettingsTreeFile = "GraphProcessor/UXML/NodeSettings";
        public const string RelayNodeViewStyleFile = "GraphProcessor/Styles/RelayNode";

        public static StyleSheet BasicStyle { get; } = Resources.Load<StyleSheet>(BassicStyleFile);
        public static StyleSheet GraphViewStyle { get; } = Resources.Load<StyleSheet>(GraphViewStyleFile);
        public static StyleSheet BaseNodeViewStyle { get; } = Resources.Load<StyleSheet>(BaseNodeViewStyleFile);
        public static StyleSheet SimpleNodeViewStyle { get; } = Resources.Load<StyleSheet>(SimpleNodeViewStyleFile);
        public static StyleSheet SettingsNodeViewStyle { get; } = Resources.Load<StyleSheet>(SettingNodeViewStyleFile);
        public static StyleSheet PortViewStyle { get; } = Resources.Load<StyleSheet>(PortViewStyleFile);
        public static StyleSheet PortViewTypesStyle { get; } = Resources.Load<StyleSheet>(PortViewTypesStyleFile);
        public static StyleSheet EdgeViewStyle { get; } = Resources.Load<StyleSheet>(EdgeStyleFile);
        public static StyleSheet GroupViewStyle { get; } = Resources.Load<StyleSheet>(GroupStyleFile);
        public static StyleSheet NodeSettingsViewStyle { get; } = Resources.Load<StyleSheet>(NodeSettingViewStyleFile);
        public static VisualTreeAsset NodeSettingsViewTree { get; } = Resources.Load<VisualTreeAsset>(NodeSettingsTreeFile);
        public static StyleSheet RelayNodeViewStyle { get; } = Resources.Load<StyleSheet>(RelayNodeViewStyleFile);
    }
}
#endif