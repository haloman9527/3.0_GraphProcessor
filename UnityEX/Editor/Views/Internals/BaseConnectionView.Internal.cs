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
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseConnection))]
    public partial class BaseConnectionView : Edge, IGraphElementView<BaseConnectionProcessor>
    {
        public BaseConnectionProcessor ViewModel
        {
            get;
            private set;
        }
        protected BaseGraphView Owner
        {
            get;
            private set;
        }

        public BaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.BaseConnectionViewStyle);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        public void SetUp(BaseConnectionProcessor connection, BaseGraphView graphView)
        {
            ViewModel = connection;
            Owner = graphView;
            OnInitialized();
        }

        public void OnCreate()
        {
            BindProperties();
        }

        public void OnDestroy()
        {
            UnbindProperties();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }
    }
}
#endif