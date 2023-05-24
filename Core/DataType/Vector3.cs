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
    public struct InternalVector3Int
    {
        public int x;
        public int y;
        public int z;

        public InternalVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        private static readonly InternalVector3Int zeroVector = new InternalVector3Int(0, 0, 0);

        private static readonly InternalVector3Int oneVector = new InternalVector3Int(1, 1, 1);

        private static readonly InternalVector3Int upVector = new InternalVector3Int(0, 1, 0);

        private static readonly InternalVector3Int downVector = new InternalVector3Int(0, -1, 0);

        private static readonly InternalVector3Int leftVector = new InternalVector3Int(-1, 0, 0);

        private static readonly InternalVector3Int rightVector = new InternalVector3Int(1, 0, 0);

        private static readonly InternalVector3Int forwardVector = new InternalVector3Int(0, 0, 1);

        private static readonly InternalVector3Int backVector = new InternalVector3Int(0, 0, -1);

        public static InternalVector3Int zero => zeroVector;

        public static InternalVector3Int one => oneVector;

        public static InternalVector3Int forward => forwardVector;

        public static InternalVector3Int back => backVector;

        public static InternalVector3Int up => upVector;

        public static InternalVector3Int down => downVector;

        public static InternalVector3Int left => leftVector;

        public static InternalVector3Int right => rightVector;

        public static implicit operator InternalVector2Int(InternalVector3Int other)
        {
            return new InternalVector2Int(other.x, other.y);
        }

        public static InternalVector3Int operator +(InternalVector3Int lhs, InternalVector3Int rhs)
        {
            return new InternalVector3Int(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static InternalVector3Int operator -(InternalVector3Int lhs, InternalVector3Int rhs)
        {
            return new InternalVector3Int(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static bool operator ==(InternalVector3Int lhs, InternalVector3Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator !=(InternalVector3Int lhs, InternalVector3Int rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is InternalVector3Int))
                return false;
            return Equals((InternalVector3Int)other);
        }

        public bool Equals(InternalVector3Int other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
        }
    }
}