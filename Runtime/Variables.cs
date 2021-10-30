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

[DisallowMultipleComponent]
public class Variables : MonoBehaviour
{

}

public sealed class VariableDeclaration
{
    public string name;
    public object value;

    public VariableDeclaration(string name, object value)
    {
        this.name = name;
        this.value = value;
    }
}
