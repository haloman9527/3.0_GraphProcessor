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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("Math", "Add")]
    public partial class AddNode : BaseNode
    {
        [Input]
        [SerializeField] float input;

        [Output]
        [SerializeField] float output;
    }
}
