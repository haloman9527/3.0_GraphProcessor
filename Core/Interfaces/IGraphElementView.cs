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

namespace Atom.GraphProcessor.Editors
{
    public interface IGraphElementView
    {
        IGraphElementProcessor V { get; }
        
        void Init();
        
        void UnInit();
    }

    public interface IGraphElementView<T> : IGraphElementView where T: IGraphElementProcessor
    {
        T ViewModel { get; }
    }
}
