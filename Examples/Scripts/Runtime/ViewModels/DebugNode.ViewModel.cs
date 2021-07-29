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

namespace CZToolKit.GraphProcessor
{
    public partial class DebugNode : BaseNode
    {
        public string Input
        {
            get { return GetPropertyValue<string>(nameof(Input)); }
            set { SetPropertyValue(nameof(Input), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            this[nameof(Input)] = new BindableProperty<string>(input, v => input = v);
        }
    }
}