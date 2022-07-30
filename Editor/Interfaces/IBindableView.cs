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
using CZToolKit.Core.ViewModel;

namespace CZToolKit.GraphProcessor
{
    public interface IBindableView
    {
        void BindingProperties();
        void UnBindingProperties();
    }

    public interface IBindableView<VM> : IBindableView where VM : ViewModel
    {
        VM ViewModel { get; }
    }
}
