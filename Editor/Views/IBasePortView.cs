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
using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    public interface IBasePortView
    {
        Port Self { get; }

        BaseNodeView Owner { get; }

        PortTypeConstraint TypeConstraint { get; }

        Type DisplayType { get; }

        string FieldName { get; }
    }
}
