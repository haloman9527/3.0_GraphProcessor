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
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Atom.GraphProcessor.Editors
{
    public partial class BaseConnectionView : Edge, IGraphElementView<BaseConnectionProcessor>
    {
        public BaseConnectionProcessor ViewModel { get; private set; }
        public IGraphElementProcessor V => ViewModel;
        protected BaseGraphView Owner { get; private set; }

        public BaseConnectionView()
        {
            styleSheets.Add(GraphProcessorEditorStyles.DefaultStyles.BaseConnectionViewStyle);
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
            this.RegisterCallback<ClickEvent>(OnClick);
            
            BindProperties();
        }

        public void OnDestroy()
        {
            this.UnregisterCallback<ClickEvent>(OnClick);

            UnbindProperties();
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            this.BringToFront();
        }
        
        private void OnClick(ClickEvent evt)
        {
            if (evt.clickCount == 2)
            {
                
            }
        }
    }
}
#endif