using System;

namespace Net.FixFloat
{
    /// <summary>
    /// 定点数（20位小数）
    /// </summary>
    public struct FixedFloat : IEquatable<FixedFloat>, IComparable<FixedFloat>
    {
        private long scaledValue;

        public long ScaledValue
        {
            get => scaledValue;
            set => scaledValue = value;
        }

        /// <summary>
        /// 小数位数
        /// </summary>
        private const int BITMOVE_COUNT = 20;

        /// <summary>
        /// 缩放倍数 2^20
        /// </summary>
        private const long MULTIPLIER_FACTOR = 1L << BITMOVE_COUNT;

        public static readonly FixedFloat zero = new FixedFloat(0L, true);
        public static readonly FixedFloat one = new FixedFloat(1);

        /// <summary>
        /// 内部构造（scaledValue已经缩放）
        /// </summary>
        private FixedFloat(long scaledValue, bool raw)
        {
            this.scaledValue = scaledValue;
        }

        /// <summary>
        /// 从原始缩放值创建
        /// </summary>
        public static FixedFloat FromRaw(long rawValue)
        {
            return new FixedFloat(rawValue, true);
        }

        public FixedFloat(int value)
        {
            scaledValue = (long)value * MULTIPLIER_FACTOR;
        }

        public FixedFloat(float value)
        {
            scaledValue = (long)Math.Round(value * MULTIPLIER_FACTOR);
        }

        public FixedFloat(double value)
        {
            scaledValue = (long)Math.Round(value * MULTIPLIER_FACTOR);
        }

        #region 类型转换

        public static implicit operator FixedFloat(int value)
        {
            return new FixedFloat(value);
        }

        public static explicit operator FixedFloat(float value)
        {
            return new FixedFloat(value);
        }

        public static explicit operator FixedFloat(double value)
        {
            return new FixedFloat(value);
        }

        public static explicit operator float(FixedFloat value)
        {
            return value.RawFloat;
        }

        public static explicit operator double(FixedFloat value)
        {
            return value.RawDouble;
        }

        public static explicit operator int(FixedFloat value)
        {
            return value.RawInt;
        }

        #endregion

        #region 运算符

        public static FixedFloat operator +(FixedFloat a, FixedFloat b)
        {
            return FromRaw(a.scaledValue + b.scaledValue);
        }

        public static FixedFloat operator -(FixedFloat value)
        {
            return FromRaw(-value.scaledValue);
        }

        public static FixedFloat operator -(FixedFloat a, FixedFloat b)
        {
            return FromRaw(a.scaledValue - b.scaledValue);
        }

        public static FixedFloat operator *(FixedFloat a, FixedFloat b)
        {
            long value = a.scaledValue * b.scaledValue;

            if (value >= 0)
            {
                value >>= BITMOVE_COUNT;
            }
            else
            {
                value = -((-value) >> BITMOVE_COUNT);
            }

            return FromRaw(value);
        }

        public static FixedFloat operator /(FixedFloat a, FixedFloat b)
        {
            if (b.scaledValue == 0)
                throw new DivideByZeroException();

            return FromRaw(
                (a.scaledValue << BITMOVE_COUNT) / b.scaledValue
            );
        }

        public static FixedFloat operator >>(FixedFloat value, int moveCount)
        {
            if (value.scaledValue >= 0)
            {
                return FromRaw(value.scaledValue >> moveCount);
            }

            return FromRaw(-((-value.scaledValue) >> moveCount));
        }

        public static FixedFloat operator <<(FixedFloat value, int moveCount)
        {
            return FromRaw(value.scaledValue << moveCount);
        }

        #endregion

        #region 比较运算符

        public static bool operator ==(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue == b.scaledValue;
        }

        public static bool operator !=(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue != b.scaledValue;
        }

        public static bool operator >(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue > b.scaledValue;
        }

        public static bool operator <(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue < b.scaledValue;
        }

        public static bool operator >=(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue >= b.scaledValue;
        }

        public static bool operator <=(FixedFloat a, FixedFloat b)
        {
            return a.scaledValue <= b.scaledValue;
        }

        #endregion

        #region 原始值

        public float RawFloat
        {
            get => scaledValue / (float)MULTIPLIER_FACTOR;
        }

        public double RawDouble
        {
            get => scaledValue / (double)MULTIPLIER_FACTOR;
        }

        public int RawInt
        {
            get
            {
                if (scaledValue >= 0)
                {
                    return (int)(scaledValue >> BITMOVE_COUNT);
                }

                return (int)(-((-scaledValue) >> BITMOVE_COUNT));
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return RawFloat.ToString("F6");
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FixedFloat))
                return false;

            return Equals((FixedFloat)obj);
        }

        public bool Equals(FixedFloat other)
        {
            return scaledValue == other.scaledValue;
        }

        public override int GetHashCode()
        {
            return scaledValue.GetHashCode();
        }

        public int CompareTo(FixedFloat other)
        {
            return scaledValue.CompareTo(other.scaledValue);
        }

        #endregion

        #region 常用函数

        public static FixedFloat Abs(FixedFloat value)
        {
            return value.scaledValue >= 0
                ? value
                : FromRaw(-value.scaledValue);
        }

        public static FixedFloat Min(FixedFloat a, FixedFloat b)
        {
            return a < b ? a : b;
        }

        public static FixedFloat Max(FixedFloat a, FixedFloat b)
        {
            return a > b ? a : b;
        }

        public static FixedFloat Clamp(
            FixedFloat value,
            FixedFloat min,
            FixedFloat max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        #endregion
    }
}