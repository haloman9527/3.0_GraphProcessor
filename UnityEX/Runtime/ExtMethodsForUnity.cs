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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion

namespace Atom.GraphProcessor
{
    public static class UnityExtMethods
    {
        public static UnityEngine.Color ToColor(this InternalColor self)
        {
            return new UnityEngine.Color(self.r, self.g, self.b, self.a);
        }

        public static InternalColor ToInternalColor(this UnityEngine.Color self)
        {
            return new InternalColor(self.r, self.g, self.b, self.a);
        }

        public static UnityEngine.Vector2 ToVector2(this InternalVector2Int self)
        {
            return new UnityEngine.Vector2(self.x, self.y);
        }

        public static UnityEngine.Vector2Int ToVector2Int(this InternalVector2Int self)
        {
            return new UnityEngine.Vector2Int(self.x, self.y);
        }

        public static InternalVector2Int ToInternalVector2Int(this UnityEngine.Vector2 self)
        {
            // 使用 RoundToInt 而非强制截断，保留亚像素精度
            return new InternalVector2Int(UnityEngine.Mathf.RoundToInt(self.x), UnityEngine.Mathf.RoundToInt(self.y));
        }

        public static UnityEngine.Vector3 ToVector3(this InternalVector3Int self)
        {
            return new UnityEngine.Vector3(self.x, self.y, self.z);
        }

        public static UnityEngine.Vector3Int ToVector3Int(this InternalVector3Int self)
        {
            return new UnityEngine.Vector3Int(self.x, self.y, self.z);
        }

        public static InternalVector3Int ToInternalVector3Int(this UnityEngine.Vector3 self)
        {
            // 使用 RoundToInt 而非强制截断，保留亚像素精度
            return new InternalVector3Int(UnityEngine.Mathf.RoundToInt(self.x), UnityEngine.Mathf.RoundToInt(self.y), UnityEngine.Mathf.RoundToInt(self.z));
        }
    }
}
