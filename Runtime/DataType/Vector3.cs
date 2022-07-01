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
    public struct InternalVector3 : IEquatable<InternalVector3>
    {
        public float x;
        public float y;
        public float z;

        public InternalVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public override bool Equals(object other)
        {
            if (!(other is InternalVector3))
                return false;

            return Equals((InternalVector3)other);
        }

        public bool Equals(InternalVector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        private static readonly InternalVector3 zeroVector = new InternalVector3(0f, 0f, 0f);

        private static readonly InternalVector3 oneVector = new InternalVector3(1f, 1f, 1f);

        private static readonly InternalVector3 upVector = new InternalVector3(0f, 1f, 0f);

        private static readonly InternalVector3 downVector = new InternalVector3(0f, -1f, 0f);

        private static readonly InternalVector3 leftVector = new InternalVector3(-1f, 0f, 0f);

        private static readonly InternalVector3 rightVector = new InternalVector3(1f, 0f, 0f);

        private static readonly InternalVector3 forwardVector = new InternalVector3(0f, 0f, 1f);

        private static readonly InternalVector3 backVector = new InternalVector3(0f, 0f, -1f);

        public static InternalVector3 zero => zeroVector;

        public static InternalVector3 one => oneVector;

        public static InternalVector3 forward => forwardVector;

        public static InternalVector3 back => backVector;

        public static InternalVector3 up => upVector;

        public static InternalVector3 down => downVector;

        public static InternalVector3 left => leftVector;

        public static InternalVector3 right => rightVector;

        public static InternalVector3 operator +(InternalVector3 a, InternalVector3 b)
        {
            return new InternalVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static InternalVector3 operator -(InternalVector3 a, InternalVector3 b)
        {
            return new InternalVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static InternalVector3 operator -(InternalVector3 a)
        {
            return new InternalVector3(0f - a.x, 0f - a.y, 0f - a.z);
        }

        public static InternalVector3 operator *(InternalVector3 a, float d)
        {
            return new InternalVector3(a.x * d, a.y * d, a.z * d);
        }

        public static InternalVector3 operator *(float d, InternalVector3 a)
        {
            return new InternalVector3(a.x * d, a.y * d, a.z * d);
        }

        public static InternalVector3 operator /(InternalVector3 a, float d)
        {
            return new InternalVector3(a.x / d, a.y / d, a.z / d);
        }

        public static bool operator ==(InternalVector3 lhs, InternalVector3 rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            float num3 = lhs.z - rhs.z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            return num4 < 9.99999944E-11f;
        }

        public static bool operator !=(InternalVector3 lhs, InternalVector3 rhs)
        {
            return !(lhs == rhs);
        }
    }
}
