#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *      为Unity编写的一些扩展方法
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
    public static class ExtMethodsForUnity
    {
        public static UnityEngine.Color ToColor(this InternalColor self)
        {
            return new UnityEngine.Color(self.r, self.g, self.b, self.a);
        }

        public static InternalColor ToInternalColor(this UnityEngine.Color self)
        {
            return new InternalColor(self.r, self.g, self.b, self.a);
        }

        public static UnityEngine.Vector2 ToVector2(this InternalVector2 self)
        {
            return new UnityEngine.Vector2(self.x, self.y);
        }

        public static InternalVector2 ToInternalVector2(this UnityEngine.Vector2 self)
        {
            return new InternalVector2(self.x, self.y);
        }

        public static UnityEngine.Vector3 ToVector3(this InternalVector3 self)
        {
            return new UnityEngine.Vector3(self.x, self.y, self.z);
        }

        public static InternalVector3 ToInternalVector3(this UnityEngine.Vector3 self)
        {
            return new InternalVector3(self.x, self.y, self.z);
        }
    }
}
