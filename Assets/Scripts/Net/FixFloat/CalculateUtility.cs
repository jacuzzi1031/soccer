using System;

namespace Net.FixFloat
{
    /// <summary>
    /// 常用定点数数学运算
    /// </summary>
    public static class CalculateUtility
    {
        /// <summary>
        /// 快速平方根
        /// sqrt(x) = x * (1 / sqrt(x))
        /// </summary>
        public static FixedFloat Sqrt(FixedFloat value)
        {
            if (value < FixedFloat.zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value == FixedFloat.zero)
                return FixedFloat.zero;

            return value * InvSqrt(value);
        }

        /// <summary>
        /// 快速求平方根倒数
        /// Quake III Fast Inverse Square Root
        /// </summary>
        public static FixedFloat InvSqrt(FixedFloat value)
        {
            if (value <= FixedFloat.zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            return FixedFloat.one / BetterSqrt(value);
        }

        /// <summary>
        /// 牛顿迭代法求平方根
        /// 精度高于 Fast InvSqrt
        /// </summary>
        public static FixedFloat BetterSqrt(
            FixedFloat value,
            int iteratorCount = 8)
        {
            if (value == FixedFloat.zero)
                return FixedFloat.zero;

            if (value < FixedFloat.zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            FixedFloat result = value;
            FixedFloat previous;

            int count = 0;

            do
            {
                previous = result;
                result = (result + value / result) >> 1;
                count++;
            }
            while (
                previous != result &&
                count < iteratorCount);

            return result;
        }

        /// <summary>
        /// 反余弦（弧度）
        /// 范围：[0, PI]
        /// </summary>
        public static FixedFloat Acos(FixedFloat value)
        {
            FixedFloat rate =
                value * AcosTable.HalfIndexCount +
                AcosTable.HalfIndexCount;

            rate = Clamp(
                rate,
                FixedFloat.zero,
                AcosTable.IndexCount);

            int index = rate.RawInt;

            if (index < 0)
                index = 0;

            if (index > AcosTable.IndexCount)
                index = AcosTable.IndexCount;

            return AcosTable.table[index];
        }

        /// <summary>
        /// 限制范围
        /// </summary>
        public static FixedFloat Clamp(
            FixedFloat input,
            FixedFloat min,
            FixedFloat max)
        {
            if (input < min)
                return min;

            if (input > max)
                return max;

            return input;
        }

        /// <summary>
        /// 返回最小值
        /// </summary>
        public static FixedFloat Min(
            FixedFloat a,
            FixedFloat b)
        {
            return a <= b ? a : b;
        }

        /// <summary>
        /// 返回最大值
        /// </summary>
        public static FixedFloat Max(
            FixedFloat a,
            FixedFloat b)
        {
            return a >= b ? a : b;
        }

        /// <summary>
        /// 绝对值
        /// </summary>
        public static FixedFloat Abs(FixedFloat value)
        {
            return value >= FixedFloat.zero
                ? value
                : -value;
        }
    }

    /// <summary>
    /// Acos查表
    /// </summary>
    public static class AcosTable
    {
        /// <summary>
        /// 查表精度
        /// 越大越精确，占用内存越高
        /// </summary>
        public const int IndexCount = 10000;

        public const int HalfIndexCount = IndexCount / 2;

        /// <summary>
        /// acos结果表
        /// table[i] 对应 acos(x)
        /// x范围[-1,1]
        /// </summary>
        public static readonly FixedFloat[] table;

        static AcosTable()
        {
            table = new FixedFloat[IndexCount + 1];

            for (int i = 0; i <= IndexCount; i++)
            {
                double x =
                    ((double)i / HalfIndexCount) - 1.0;

                double radians =
                    Math.Acos(x);

                table[i] =
                    (FixedFloat)radians;
            }
        }
    }
}