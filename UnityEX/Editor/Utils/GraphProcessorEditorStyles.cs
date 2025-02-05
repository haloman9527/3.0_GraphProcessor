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

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace Moyo.GraphProcessor.Editors
{
    public static class GraphProcessorEditorStyles
    {
        public static Styles DefaultStyles { get; private set; } = new Styles()
        {
            GraphWindowTree = Resources.Load<VisualTreeAsset>("GraphProcessor/UXML/GraphWindow"),

            BasicStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BasicStyle"),
            BaseGraphViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BaseGraphView"),
            BaseNodeViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BaseNodeView"),
            BaseSimpleNodeViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BaseSimpleNodeView"),
            BasePortViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BasePortView"),
            BaseConnectionViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/BaseConnectionView"),
            GroupViewStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/GroupView"),
            StickyNodeStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/StickyNodeView"),
            StickyNoteStyle = Resources.Load<StyleSheet>("GraphProcessor/USS/StickyNoteView"),
        };

        public class Styles
        {
            public VisualTreeAsset GraphWindowTree;
            
            public StyleSheet BasicStyle;
            public StyleSheet BaseGraphViewStyle;
            public StyleSheet BaseNodeViewStyle;
            public StyleSheet BaseSimpleNodeViewStyle;
            public StyleSheet BasePortViewStyle;
            public StyleSheet BaseConnectionViewStyle;
            public StyleSheet GroupViewStyle;
            public StyleSheet StickyNodeStyle;
            public StyleSheet StickyNoteStyle;
        }
    }
}
#endif