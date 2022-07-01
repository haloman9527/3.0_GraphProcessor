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
    public struct InternalVector2 : IEquatable<InternalVector2>
    {
        public float x;
        public float y;

        public InternalVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object other)
        {
            if (!(other is InternalVector2))
                return false;
            return Equals((InternalVector2)other);
        }

        public bool Equals(InternalVector2 other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        private static readonly InternalVector2 zeroVector = new InternalVector2(0f, 0f);

        private static readonly InternalVector2 oneVector = new InternalVector2(1f, 1f);

        private static readonly InternalVector2 upVector = new InternalVector2(0f, 1f);

        private static readonly InternalVector2 downVector = new InternalVector2(0f, -1f);

        private static readonly InternalVector2 leftVector = new InternalVector2(-1f, 0f);

        private static readonly InternalVector2 rightVector = new InternalVector2(1f, 0f);

        public static InternalVector2 zero => zeroVector;

        public static InternalVector2 one => oneVector;

        public static InternalVector2 up => upVector;

        public static InternalVector2 down => downVector;

        public static InternalVector2 left => leftVector;

        public static InternalVector2 right => rightVector;

        public static InternalVector2 operator +(InternalVector2 a, InternalVector2 b)
        {
            return new InternalVector2(a.x + b.x, a.y + b.y);
        }

        public static InternalVector2 operator -(InternalVector2 a, InternalVector2 b)
        {
            return new InternalVector2(a.x - b.x, a.y - b.y);
        }

        public static InternalVector2 operator *(InternalVector2 a, InternalVector2 b)
        {
            return new InternalVector2(a.x * b.x, a.y * b.y);
        }

        public static InternalVector2 operator /(InternalVector2 a, InternalVector2 b)
        {
            return new InternalVector2(a.x / b.x, a.y / b.y);
        }

        public static InternalVector2 operator -(InternalVector2 a)
        {
            return new InternalVector2(0f - a.x, 0f - a.y);
        }

        public static InternalVector2 operator *(InternalVector2 a, float d)
        {
            return new InternalVector2(a.x * d, a.y * d);
        }

        public static InternalVector2 operator *(float d, InternalVector2 a)
        {
            return new InternalVector2(a.x * d, a.y * d);
        }

        public static InternalVector2 operator /(InternalVector2 a, float d)
        {
            return new InternalVector2(a.x / d, a.y / d);
        }

        public static bool operator ==(InternalVector2 lhs, InternalVector2 rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            return num * num + num2 * num2 < 9.99999944E-11f;
        }

        public static bool operator !=(InternalVector2 lhs, InternalVector2 rhs)
        {
            return !(lhs == rhs);
        }

        public static implicit operator InternalVector2(InternalVector3 v)
        {
            return new InternalVector2(v.x, v.y);
        }

        public static implicit operator InternalVector3(InternalVector2 v)
        {
            return new InternalVector3(v.x, v.y, 0f);
        }
    }
}
