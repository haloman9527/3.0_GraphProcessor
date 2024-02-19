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

namespace CZToolKit.GraphProcessor
{
    public interface IGraphElementViewModel
    {
    }

    public interface IGraphScopeViewModel : IGraphElementViewModel
    {
        public InternalVector2Int Position { get; set; }
    }
}