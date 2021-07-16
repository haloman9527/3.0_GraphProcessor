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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    class NodeSettingsView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<NodeSettingsView, GraphView.UxmlTraits> { }


        VisualElement m_ContentContainer;

        public NodeSettingsView()
        {
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(GraphProcessorStyles.NodeSettingsViewStyle);
            GraphProcessorStyles.NodeSettingsViewTree.CloneTree(this);

            m_ContentContainer = this.Q("contentContainer");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            evt.StopPropagation();
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            evt.StopPropagation();
        }

        public override VisualElement contentContainer
        {
            get { return m_ContentContainer; }
        }
    }
}