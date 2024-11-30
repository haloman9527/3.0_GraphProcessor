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

namespace Moyo.GraphProcessor.Editors
{
    public interface IGraphElementView
    {
        IGraphElementProcessor V { get; }
        
        void OnCreate();
        
        void OnDestroy();
    }

    public interface IGraphElementView<T> : IGraphElementView where T: IGraphElementProcessor
    {
        T ViewModel { get; }
    }
}
