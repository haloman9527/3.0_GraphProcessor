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
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public struct InternalVector2Int : IEquatable<InternalVector2Int>
    {
        public int x;
        public int y;

        public InternalVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        private static readonly InternalVector2Int zeroVector = new InternalVector2Int(0, 0);

        private static readonly InternalVector2Int oneVector = new InternalVector2Int(1, 1);

        private static readonly InternalVector2Int upVector = new InternalVector2Int(0, 1);

        private static readonly InternalVector2Int downVector = new InternalVector2Int(0, -1);

        private static readonly InternalVector2Int leftVector = new InternalVector2Int(-1, 0);

        private static readonly InternalVector2Int rightVector = new InternalVector2Int(1, 0);

        public static InternalVector2Int zero => zeroVector;

        public static InternalVector2Int one => oneVector;

        public static InternalVector2Int up => upVector;

        public static InternalVector2Int down => downVector;

        public static InternalVector2Int left => leftVector;

        public static InternalVector2Int right => rightVector;

        public static implicit operator InternalVector3Int(InternalVector2Int other)
        {
            return new InternalVector3Int(other.x, other.y, 0);
        }
        
        public static InternalVector2Int operator +(InternalVector2Int lhs, InternalVector2Int rhs)
        {
            return new InternalVector2Int(lhs.x + rhs.x, lhs.y + rhs.y);
        }
        
        public static InternalVector2Int operator -(InternalVector2Int lhs, InternalVector2Int rhs)
        {
            return new InternalVector2Int(lhs.x - rhs.x, lhs.y - rhs.y);
        }
        
        public static bool operator ==(InternalVector2Int lhs, InternalVector2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(InternalVector2Int lhs, InternalVector2Int rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is InternalVector2Int))
                return false;
            return Equals((InternalVector2Int)other);
        }

        public bool Equals(InternalVector2Int other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }
    }
}
