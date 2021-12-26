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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseConnectionView : Edge, IBindableView<BaseConnection>
    {
        public Label IndexLabel { get; private set; }
        public BaseConnection Model { get; private set; }
        protected BaseGraphView Owner { get; private set; }

        public BaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
            IndexLabel = new Label();
            IndexLabel.style.display = DisplayStyle.None;
            IndexLabel.style.flexGrow = 1;
            IndexLabel.style.fontSize = 20;
            IndexLabel.style.color = new Color(1, 0.5f, 0, 1);
            IndexLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            edgeControl.Add(IndexLabel);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        public void SetUp(BaseConnection connection, BaseGraphView graphView)
        {
            Model = connection;
            Owner = graphView;
        }

        public virtual void UnBindingProperties()
        {

        }

        public void ShowIndex(int index)
        {
            IndexLabel.text = index.ToString();
            IndexLabel.style.display = DisplayStyle.Flex;
            BringToFront();
        }

        public void HideIndex()
        {
            IndexLabel.text = "";
            IndexLabel.style.display = DisplayStyle.None;
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }

        protected override void DrawEdge()
        {
            base.DrawEdge();
        }
    }
}
#endif