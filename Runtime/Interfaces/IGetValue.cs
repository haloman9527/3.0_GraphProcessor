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

namespace CZToolKit.GraphProcessor
{
    public interface IGetValue
    {
        object GetValue(string port);
    }

    public interface IGetValue<T>
    {
        T GetValue(string port);
    }
}
