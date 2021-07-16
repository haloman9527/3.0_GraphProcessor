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
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(DebugNode))]
    public sealed class DebugNodeView : BaseNodeView
    {
        Label label;

        public DebugNode TViewModel { get { return Model as DebugNode; } }

        public DebugNodeView() : base()
        {
            label = new Label();
            controlsContainer.Add(label);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            UpdateLabel();
            Add(new IMGUIContainer(UpdateLabel));
        }

        void UpdateLabel()
        {
            Model.TryGetPort("input", out NodePort port);
            object value = port.GetConnectValue();
            if (value != null)
            {
                if (value.Equals(null))
                    label.text = "NULL";
                else
                    label.text = value.ToString();
            }
            else
            {
                label.text = TViewModel.Input;
            }
        }
    }
}
