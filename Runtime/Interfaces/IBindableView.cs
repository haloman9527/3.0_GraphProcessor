#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion

namespace CZToolKit.GraphProcessor
{
    public interface IBindableView
    {
        void UnBindingProperties();
    }

    public interface IBindableView<VM> : IBindableView where VM : BaseGraphElement
    {
        VM Model { get; }
    }
}
