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
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomView(typeof(BaseConnection))]
    public partial class BaseConnectionView : Edge, IBindableView<BaseConnectionVM>
    {
        public BaseConnectionVM ViewModel
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
            styleSheets.Add(GraphProcessorStyles.ConnectionViewStyle);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        public void SetUp(BaseConnectionVM connection, BaseGraphView graphView)
        {
            ViewModel = connection;
            Owner = graphView;
            OnInitialized();
        }

        public void BindingProperties()
        {
            OnBindingProperties();
        }

        public void UnBindingProperties()
        {
            OnUnbindingProperties();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }
    }
}
#endif