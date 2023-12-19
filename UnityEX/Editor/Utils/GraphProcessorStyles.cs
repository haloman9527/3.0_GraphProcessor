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
 *  Blog: https://www.mindgear.net/
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
        public const string GraphWindowUXMLFile = "GraphProcessor/UXML/GraphWindow";
        public const string BassicStyleFile = "GraphProcessor/USS/BasicStyle";
        public const string BaseGraphViewStyleFile = "GraphProcessor/USS/BaseGraphView";
        public const string BaseNodeViewStyleFile = "GraphProcessor/USS/BaseNodeView";
        public const string BaseSimpleNodeViewStyleFile = "GraphProcessor/USS/BaseSimpleNodeView";
        public const string BasePortViewStyleFile = "GraphProcessor/USS/BasePortView";
        public const string BaseEdgeStyleFile = "GraphProcessor/USS/BaseEdgeView";
        public const string BaseGroupStyleFile = "GraphProcessor/USS/BaseGroupView";

        public static VisualTreeAsset GraphWindowTree { get; } = Resources.Load<VisualTreeAsset>(GraphWindowUXMLFile);
        public static StyleSheet BasicStyle { get; } = Resources.Load<StyleSheet>(BassicStyleFile);
        public static StyleSheet BaseGraphViewStyle { get; } = Resources.Load<StyleSheet>(BaseGraphViewStyleFile);
        public static StyleSheet BaseNodeViewStyle { get; } = Resources.Load<StyleSheet>(BaseNodeViewStyleFile);
        public static StyleSheet SimpleNodeViewStyle { get; } = Resources.Load<StyleSheet>(BaseSimpleNodeViewStyleFile);
        public static StyleSheet BasePortViewStyle { get; } = Resources.Load<StyleSheet>(BasePortViewStyleFile);
        public static StyleSheet BaseConnectionViewStyle { get; } = Resources.Load<StyleSheet>(BaseEdgeStyleFile);
        public static StyleSheet BaseGroupViewStyle { get; } = Resources.Load<StyleSheet>(BaseGroupStyleFile);
    }
}
#endif