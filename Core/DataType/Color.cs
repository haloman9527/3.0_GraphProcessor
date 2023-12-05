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
 *  Blog: https://www.mindgear.net/
 *
 */
#endregion
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public struct InternalColor : IEquatable<InternalColor>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public InternalColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1;
        }

        public InternalColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public bool Equals(InternalColor other)
        {
            return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
        }

        public override bool Equals(object obj)
        {
            return obj is InternalColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = r.GetHashCode();
                hashCode = (hashCode * 397) ^ g.GetHashCode();
                hashCode = (hashCode * 397) ^ b.GetHashCode();
                hashCode = (hashCode * 397) ^ a.GetHashCode();
                return hashCode;
            }
        }
    }
}
