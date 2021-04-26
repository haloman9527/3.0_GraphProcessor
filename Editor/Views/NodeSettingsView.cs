using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    class NodeSettingsView : VisualElement
    {
        VisualElement m_ContentContainer;

        public NodeSettingsView()
        {
            pickingMode = PickingMode.Ignore;
            styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessor/Styles/NodeSettings"));
            var uxml = Resources.Load<VisualTreeAsset>("GraphProcessor/UXML/NodeSettings");
            uxml.CloneTree(this);

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