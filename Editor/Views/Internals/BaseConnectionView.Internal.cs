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

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class BaseConnectionView : Edge, IBindableView<BaseConnectionVM>
    {
        public BaseConnectionVM ViewModel { get; private set; }
        protected BaseGraphView Owner { get; private set; }

        public BaseConnectionView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
            this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        }

        public void SetUp(BaseConnectionVM connection, BaseGraphView graphView)
        {
            ViewModel = connection;
            Owner = graphView;
        }

        public virtual void BindingProperties()
        {

        }

        public virtual void UnBindingProperties()
        {

        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }
    }
}
#endif