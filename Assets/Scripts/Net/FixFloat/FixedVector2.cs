#if UNITY_ENV
using UnityEngine;
#endif

namespace Net.FixFloat
{
    /// <summary>
    /// 二维定点向量
    /// </summary>
    public struct FixedVector2
    {
        public FixedFloat x;
        public FixedFloat y;

        /// <summary>
        /// 平方长度
        /// </summary>
        public FixedFloat sqrMagnitude
        {
            get => x * x + y * y;
        }

        /// <summary>
        /// 长度
        /// </summary>
        public FixedFloat magnitude
        {
            get => CalculateUtility.BetterSqrt(sqrMagnitude);
        }

        /// <summary>
        /// 单位向量
        /// </summary>
        public FixedVector2 normalized
        {
            get
            {
                if (magnitude > FixedFloat.zero)
                {
                    FixedFloat rate = FixedFloat.one / magnitude;
                    return new FixedVector2(x * rate, y * rate);
                }

                return zero;
            }
        }

        public FixedVector2(FixedFloat x, FixedFloat y)
        {
            this.x = x;
            this.y = y;
        }
        public static FixedVector2 MoveTowards(
            FixedVector2 current,
            FixedVector2 target,
            FixedFloat maxDistanceDelta)
        {
            FixedVector2 delta = target - current;

            FixedFloat sqrDist = delta.sqrMagnitude;

            if (sqrDist == FixedFloat.zero)
                return target;

            FixedFloat dist = CalculateUtility.BetterSqrt(sqrDist);

            if (dist <= maxDistanceDelta)
                return target;

            return current + delta / dist * maxDistanceDelta;
        }

#if UNITY_ENV
        public FixedVector2(Vector2 vector)
        {
            x = (FixedFloat)vector.x;
            y = (FixedFloat)vector.y;
        }

        public FixedVector2(Vector3 vector)
        {
            x = (FixedFloat)vector.x;
            y = (FixedFloat)vector.y;
        }
#endif

        public FixedFloat this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default: return FixedFloat.zero;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                }
            }
        }

        #region 常用向量

        public static FixedVector2 zero => new FixedVector2(0, 0);

        public static FixedVector2 one => new FixedVector2(1, 1);

        public static FixedVector2 right => new FixedVector2(1, 0);

        public static FixedVector2 left => new FixedVector2(-1, 0);

        public static FixedVector2 up => new FixedVector2(0, 1);

        public static FixedVector2 down => new FixedVector2(0, -1);

        #endregion

        #region 运算符

        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b)
        {
            return new FixedVector2(
                a.x + b.x,
                a.y + b.y);
        }

        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b)
        {
            return new FixedVector2(
                a.x - b.x,
                a.y - b.y);
        }

        public static FixedVector2 operator -(FixedVector2 v)
        {
            return new FixedVector2(-v.x, -v.y);
        }

        public static FixedVector2 operator *(FixedVector2 v, FixedFloat value)
        {
            return new FixedVector2(
                v.x * value,
                v.y * value);
        }

        public static FixedVector2 operator *(FixedFloat value, FixedVector2 v)
        {
            return new FixedVector2(
                v.x * value,
                v.y * value);
        }

        public static FixedVector2 operator /(FixedVector2 v, FixedFloat value)
        {
            return new FixedVector2(
                v.x / value,
                v.y / value);
        }

        public static bool operator ==(FixedVector2 a, FixedVector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(FixedVector2 a, FixedVector2 b)
        {
            return !(a == b);
        }

        #endregion

        #region 重写

        public override string ToString()
        {
            return $"({x.RawFloat}, {y.RawFloat})";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FixedVector2))
                return false;

            FixedVector2 v = (FixedVector2)obj;
            return this == v;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

        #endregion

        #region 转换

        public long[] ConvertLongArray()
        {
            return new long[]
            {
                x.ScaledValue,
                y.ScaledValue
            };
        }

#if UNITY_ENV
        public Vector2 ConvertUnityVector2()
        {
            return new Vector2(
                x.RawFloat,
                y.RawFloat);
        }
#endif

        #endregion

        #region 向量计算

        public static FixedFloat SqrMagnitude(FixedVector2 v)
        {
            return v.x * v.x + v.y * v.y;
        }

        public static FixedFloat Magnitude(FixedVector2 v)
        {
            return CalculateUtility.BetterSqrt(v.sqrMagnitude);
        }

        public static FixedVector2 Normalize(FixedVector2 v)
        {
            if (v.magnitude > FixedFloat.zero)
            {
                FixedFloat rate = FixedFloat.one / v.magnitude;

                return new FixedVector2(
                    v.x * rate,
                    v.y * rate);
            }

            return zero;
        }

        public void Normalize()
        {
            if (magnitude <= FixedFloat.zero)
                return;

            FixedFloat rate = FixedFloat.one / magnitude;

            x *= rate;
            y *= rate;
        }

        /// <summary>
        /// 点乘
        /// </summary>
        public static FixedFloat Dot(FixedVector2 a, FixedVector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        /// <summary>
        /// 二维叉乘（返回标量）
        /// </summary>
        public static FixedFloat Cross(FixedVector2 a, FixedVector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        /// <summary>
        /// 夹角（弧度）
        /// </summary>
        public static FixedFloat Angle(FixedVector2 from, FixedVector2 to)
        {
            FixedFloat dot = Dot(from, to);
            FixedFloat mod = from.magnitude * to.magnitude;

            if (mod == FixedFloat.zero)
                return FixedFloat.zero;

            return CalculateUtility.Acos(dot / mod);
        }

        /// <summary>
        /// 两点距离
        /// </summary>
        public static FixedFloat Distance(FixedVector2 a, FixedVector2 b)
        {
            return (a - b).magnitude;
        }

        #endregion
    }
}