using System;

namespace Net.FixFloat
{
    /// <summary>
    /// 常用定点数数学运算
    /// </summary>
    public static class FixedMath
    {
        
        private static uint _randomState = 123456789u;

        /// <summary>
        /// 设置随机种子（锁步时所有客户端必须一致）
        /// </summary>
        public static void SetRandomSeed(uint seed)
        {
            _randomState = seed;
        }

        /// <summary>
        /// Xorshift32
        /// </summary>
        private static uint NextUInt()
        {
            uint x = _randomState;

            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;

            _randomState = x;
            return x;
        }

        /// <summary>
        /// 返回[0,1)范围的随机FixedFloat
        /// </summary>
        public static FixedFloat RandomValue()
        {
            uint r = NextUInt() & ((1u << 20) - 1);

            return FixedFloat.FromRaw(r);
        }

        /// <summary>
        /// 按概率判定
        /// probability范围[0,1]
        /// </summary>
        public static bool Roll(FixedFloat probability)
        {
            if (probability <= FixedFloat.Zero)
                return false;

            if (probability >= FixedFloat.One)
                return true;

            uint r = NextUInt() & ((1u << 20) - 1);

            return r < probability.ScaledValue;
        }
        public static FixedFloat Lerp(FixedFloat a, FixedFloat b, FixedFloat t)
        {
            // 可选：Clamp到[0,1]
            if (t < FixedFloat.Zero)
                t = FixedFloat.Zero;
            else if (t > FixedFloat.One)
                t = FixedFloat.One;

            return a + (b - a) * t;
        }
        
        /// <summary>
        /// 快速平方根
        /// sqrt(x) = x * (1 / sqrt(x))
        /// </summary>
        public static FixedFloat Sqrt(FixedFloat value)
        {
            if (value < FixedFloat.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value == FixedFloat.Zero)
                return FixedFloat.Zero;

            return value * InvSqrt(value);
        }

        /// <summary>
        /// 快速求平方根倒数
        /// Quake III Fast Inverse Square Root
        /// </summary>
        public static FixedFloat InvSqrt(FixedFloat value)
        {
            if (value <= FixedFloat.Zero)
                throw new ArgumentOutOfRangeException(nameof(value));

            return FixedFloat.One / BetterSqrt(value);
        }

        /// <summary>
        /// 牛顿迭代法求平方根
        /// 精度高于 Fast InvSqrt
        /// </summary>
        public static FixedFloat BetterSqrt(FixedFloat value)
        {
            if (value <= FixedFloat.Zero)
                return FixedFloat.Zero;
            
            FixedFloat result = value >> 1;
            if (result <= FixedFloat.Zero)
                result = FixedFloat.One;

            for (int i = 0; i < 16; i++)
            {
                FixedFloat next = (result + value / result) >> 1;
                
                if (FixedMath.Abs(next - result) <= FixedFloat.One)
                    return next;

                result = next;
            }

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
                FixedFloat.Zero,
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
            return value >= FixedFloat.Zero
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
    public static class CosTable
    {
        public const int IndexCount = 10000;
        public static readonly FixedFloat PI =
            (FixedFloat)Math.PI;
        
        public static readonly FixedFloat TwoPI =
            PI * 2;

        public static readonly FixedFloat HalfPI =
            PI / 2;

        public static readonly FixedFloat[] table;

        static CosTable()
        {
            table = new FixedFloat[IndexCount + 1];

            for (int i = 0; i <= IndexCount; i++)
            {
                double radians =
                    (double)i / IndexCount *
                    Math.PI * 2.0;

                table[i] =
                    (FixedFloat)Math.Cos(radians);
            }
        }
        public static FixedFloat Cos(FixedFloat radians)
        {


            radians %= TwoPI ;

            if (radians < 0)
                radians += TwoPI;

            FixedFloat rate =
                radians / TwoPI;

            int index =
                (rate * CosTable.IndexCount).RawInt;

            return CosTable.table[index];
        }
        
    }


    
    
}