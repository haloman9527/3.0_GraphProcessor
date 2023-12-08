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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion

namespace CZToolKit.GraphProcessor.Editors
{
    public interface IGraphElementView
    {
        void OnCreate();
        
        void OnDestroy();
    }

    public interface IGraphElementView<VM> : IGraphElementView where VM: IGraphElementViewModel
    {
        VM ViewModel { get; }
    }
}
