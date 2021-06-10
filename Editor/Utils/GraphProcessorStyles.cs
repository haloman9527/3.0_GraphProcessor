#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    public static class GraphProcessorStyles
    {
        const string GraphViewStyleFile = "GraphProcessor/Styles/BaseGraphView";
        const string BaseNodeViewStyleFile = "GraphProcessor/Styles/BaseNodeView";
        const string SimpleNodeViewStyleFile = "GraphProcessor/Styles/SimpleNodeView";
        const string SettingNodeViewStyleFile = "GraphProcessor/Styles/SettingsNodeView";
        const string PortViewStyleFile = "GraphProcessor/Styles/PortView";
        const string PortViewTypesStyleFile = "GraphProcessor/Styles/PortViewTypes";
        const string EdgeStyleFile = "GraphProcessor/Styles/EdgeView";
        const string GroupStyleFile = "GraphProcessor/Styles/GroupView";
        const string NodeSettingViewStyleFile = "GraphProcessor/Styles/NodeSettings";
        const string NodeSettingsTreeFile = "GraphProcessor/UXML/NodeSettings";
        const string RelayNodeViewStyleFile = "GraphProcessor/Styles/RelayNode";
        public const int DefaultPortSize = 8;

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
