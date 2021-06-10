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
using System;

namespace CZToolKit.GraphProcessor
{
    public interface IPortView : IGraphElementView
    {
        PortTypeConstraint TypeConstraint { get; }

        Type DisplayType { get; }

        string FieldName { get; }
    }
}
