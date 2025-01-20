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

using System;

namespace Moyo.GraphProcessor
{
    public interface IGraphElementProcessor
    {
        object Model { get; }
        
        Type ModelType { get; }
    }
    
    public interface IGraphElementProcessor<T> : IGraphElementProcessor
    {
        
    }
    
    public interface IGraphElementProcessor_Scope
    {
        public InternalVector2Int Position { get; set; }
    }
}