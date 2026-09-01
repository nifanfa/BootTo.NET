using Internal.Runtime.CompilerServices;

namespace System
{
    public static class Math
    {
        public const double E = 2.7182818284590452354;
        public const double PI = 3.14159265358979323846;
        public const double Tau = 6.28318530717958647692;

        private const double Log2 = 0.69314718055994530942;
        private const double SqrtHalf = 0.70710678118654752440;

        public static int Abs(int value)
        {
            if (value == int.MinValue)
                throw new OverflowException("The absolute value of Int32.MinValue cannot be represented as an Int32.");
            return value < 0 ? -value : value;
        }

        public static long Abs(long value)
        {
            if (value == long.MinValue)
                throw new OverflowException("The absolute value of Int64.MinValue cannot be represented as an Int64.");
            return value < 0 ? -value : value;
        }

        public static short Abs(short value) => (short)Abs((int)value);
        public static sbyte Abs(sbyte value) => (sbyte)Abs((int)value);
        public static float Abs(float value)
            => MathAbsSingle(value);

        public static double Abs(double value)
            => MathAbs(value);

        public static int Sign(int value) => value < 0 ? -1 : (value > 0 ? 1 : 0);
        public static long Sign(long value) => value < 0 ? -1 : (value > 0 ? 1 : 0);
        public static int Sign(float value)
        {
            if (IsNaN(value))
                throw new ArgumentException("The sign of NaN is undefined.");
            return value < 0 ? -1 : (value > 0 ? 1 : 0);
        }

        public static int Sign(double value)
        {
            if (IsNaN(value))
                throw new ArgumentException("The sign of NaN is undefined.");
            return value < 0 ? -1 : (value > 0 ? 1 : 0);
        }

        public static int Max(int x, int y) => x > y ? x : y;
        public static uint Max(uint x, uint y) => x > y ? x : y;
        public static long Max(long x, long y) => x > y ? x : y;
        public static ulong Max(ulong x, ulong y) => x > y ? x : y;
        public static short Max(short x, short y) => x > y ? x : y;
        public static ushort Max(ushort x, ushort y) => x > y ? x : y;
        public static byte Max(byte x, byte y) => x > y ? x : y;
        public static sbyte Max(sbyte x, sbyte y) => x > y ? x : y;
        public static float Max(float x, float y) => MaxFloating(x, y);
        public static double Max(double x, double y) => MaxFloating(x, y);

        public static int Min(int x, int y) => x < y ? x : y;
        public static uint Min(uint x, uint y) => x < y ? x : y;
        public static long Min(long x, long y) => x < y ? x : y;
        public static ulong Min(ulong x, ulong y) => x < y ? x : y;
        public static short Min(short x, short y) => x < y ? x : y;
        public static ushort Min(ushort x, ushort y) => x < y ? x : y;
        public static byte Min(byte x, byte y) => x < y ? x : y;
        public static sbyte Min(sbyte x, sbyte y) => x < y ? x : y;
        public static float Min(float x, float y) => MinFloating(x, y);
        public static double Min(double x, double y) => MinFloating(x, y);

        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
                throw new ArgumentException("The minimum value cannot exceed the maximum value.");
            return value < min ? min : (value > max ? max : value);
        }

        public static long Clamp(long value, long min, long max)
        {
            if (min > max)
                throw new ArgumentException("The minimum value cannot exceed the maximum value.");
            return value < min ? min : (value > max ? max : value);
        }

        public static float Clamp(float value, float min, float max)
        {
            if (IsNaN(value) || IsNaN(min) || IsNaN(max) || min > max)
                throw new ArgumentException("Clamp requires finite bounds and a minimum that does not exceed the maximum.");
            return value < min ? min : (value > max ? max : value);
        }

        public static double Clamp(double value, double min, double max)
        {
            if (IsNaN(value) || IsNaN(min) || IsNaN(max) || min > max)
                throw new ArgumentException("Clamp requires finite bounds and a minimum that does not exceed the maximum.");
            return value < min ? min : (value > max ? max : value);
        }

        public static double Ceiling(double value)
        {
            return MathCeiling(value);
        }

        public static double Floor(double value)
        {
            return MathFloor(value);
        }

        public static double Truncate(double value)
        {
            return MathTruncate(value);
        }

        public static double Round(double value) => Round(value, MidpointRounding.ToEven);
        public static double Round(double value, MidpointRounding mode)
        {
            if (mode != MidpointRounding.ToEven && mode != MidpointRounding.AwayFromZero)
                throw new ArgumentException("The midpoint rounding mode is not supported.");
            if (!IsFinite(value) || value == 0)
                return value;

            if (mode == MidpointRounding.ToEven)
                return MathRound(value);

            double floor = Floor(value);
            double fraction = value - floor;
            if (fraction < 0.5)
                return floor;
            if (fraction > 0.5)
                return floor + 1;
            if (mode == MidpointRounding.AwayFromZero)
                return value < 0 ? floor : floor + 1;

            if (Abs(floor) >= 9223372036854775808.0)
                return value;
            return ((long)floor & 1) == 0 ? floor : floor + 1;
        }

        public static double Round(double value, int digits)
            => Round(value, digits, MidpointRounding.ToEven);

        public static double Round(double value, int digits, MidpointRounding mode)
        {
            if (digits < 0 || digits > 15)
                throw new ArgumentException("The number of rounding digits must be between 0 and 15.");
            if (digits == 0)
                return Round(value, mode);
            double scale = Pow10(digits);
            return Round(value * scale, mode) / scale;
        }

        public static float Ceiling(float value) => (float)Ceiling((double)value);
        public static float Floor(float value) => (float)Floor((double)value);
        public static float Truncate(float value) => (float)Truncate((double)value);
        public static float Round(float value) => (float)Round((double)value);
        public static float Round(float value, MidpointRounding mode) => (float)Round((double)value, mode);

        public static double Sqrt(double value)
        {
            return MathSqrt(value);
        }

        public static double Pow(double x, double y)
        {
            if (y == 0)
                return 1;
            if (x == 0)
                return y > 0 ? 0 : PositiveInfinity;
            if (IsNaN(x) || IsNaN(y))
                return NaN;

            if (x < 0)
            {
                double integer = Truncate(y);
                if (integer != y)
                    return NaN;
                double result = Exp(y * Log(-x));
                return ((long)integer & 1) == 0 ? result : -result;
            }

            if (y == Truncate(y) && Abs(y) <= 2147483647)
            {
                int exponent = (int)y;
                bool invert = exponent < 0;
                if (invert)
                    exponent = -exponent;
                double result = 1;
                double factor = x;
                while (exponent != 0)
                {
                    if ((exponent & 1) != 0)
                        result *= factor;
                    factor *= factor;
                    exponent >>= 1;
                }
                return invert ? 1 / result : result;
            }
            return Exp(y * Log(x));
        }

        public static double Exp(double value)
        {
            if (IsNaN(value))
                return NaN;
            if (value > 709.782712893384)
                return PositiveInfinity;
            if (value < -745.133219101941)
                return 0;

            int exponent = (int)Round(value / Log2, MidpointRounding.AwayFromZero);
            double reduced = value - exponent * Log2;
            double term = 1;
            double sum = 1;
            for (int i = 1; i <= 24; i++)
            {
                term *= reduced / i;
                sum += term;
            }
            if (exponent > 0)
                while (exponent-- > 0) sum *= 2;
            else
                while (exponent++ < 0) sum *= 0.5;
            return sum;
        }

        public static double Log(double value)
        {
            if (IsNaN(value) || value < 0)
                return NaN;
            if (value == 0)
                return NegativeInfinity;
            if (IsInfinity(value))
                return value;
            if (value == 1)
                return 0;

            int exponent = 0;
            while (value > 1.5) { value *= 0.5; exponent++; }
            while (value < 0.75) { value *= 2; exponent--; }

            double z = (value - 1) / (value + 1);
            double power = z;
            double sum = 0;
            for (int denominator = 1; denominator <= 39; denominator += 2)
            {
                sum += power / denominator;
                power *= z * z;
            }
            return 2 * sum + exponent * Log2;
        }

        public static double Log(double value, double newBase)
            => Log(value) / Log(newBase);

        public static double Log10(double value) => Log(value) / 2.30258509299404568402;

        public static double Sin(double value)
        {
            if (!IsFinite(value))
                return NaN;
            value = ReduceAngle(value);
            double square = value * value;
            return value * (1 - square / 6 + square * square / 120 - square * square * square / 5040 +
                square * square * square * square / 362880 - square * square * square * square * square / 39916800);
        }

        public static double Cos(double value)
        {
            if (!IsFinite(value))
                return NaN;
            value = ReduceAngle(value);
            double square = value * value;
            return 1 - square / 2 + square * square / 24 - square * square * square / 720 +
                square * square * square * square / 40320 - square * square * square * square * square / 3628800;
        }

        public static double Tan(double value) => Sin(value) / Cos(value);

        public static double Atan(double value)
        {
            if (IsNaN(value))
                return NaN;
            if (value == PositiveInfinity)
                return PI / 2;
            if (value == NegativeInfinity)
                return -PI / 2;

            bool negative = value < 0;
            value = Abs(value);
            double result;
            if (value > 1)
                result = PI / 2 - Atan(1 / value);
            else
            {
                double square = value * value;
                double term = value;
                result = 0;
                for (int denominator = 1; denominator <= 41; denominator += 2)
                {
                    result += (denominator & 2) == 0 ? term / denominator : -term / denominator;
                    term *= square;
                }
            }
            return negative ? -result : result;
        }

        public static double Atan2(double y, double x)
        {
            if (IsNaN(x) || IsNaN(y))
                return NaN;
            if (x > 0)
                return Atan(y / x);
            if (x < 0 && y >= 0)
                return Atan(y / x) + PI;
            if (x < 0 && y < 0)
                return Atan(y / x) - PI;
            if (x == 0 && y > 0)
                return PI / 2;
            if (x == 0 && y < 0)
                return -PI / 2;
            return y == 0 ? 0 : (y > 0 ? PI / 2 : -PI / 2);
        }

        public static double Asin(double value)
        {
            if (IsNaN(value) || Abs(value) > 1)
                return NaN;
            return Atan2(value, Sqrt(1 - value * value));
        }

        public static double Acos(double value)
        {
            if (IsNaN(value) || Abs(value) > 1)
                return NaN;
            return PI / 2 - Asin(value);
        }

        public static double Sinh(double value) => (Exp(value) - Exp(-value)) / 2;
        public static double Cosh(double value) => (Exp(value) + Exp(-value)) / 2;
        public static double Tanh(double value)
        {
            if (value > 20) return 1;
            if (value < -20) return -1;
            double positive = Exp(2 * value);
            return (positive - 1) / (positive + 1);
        }

        public static double IEEERemainder(double x, double y)
        {
            if (IsNaN(x) || IsNaN(y) || y == 0 || IsInfinity(x))
                return NaN;
            if (IsInfinity(y))
                return x;
            double quotient = Round(x / y, MidpointRounding.ToEven);
            return x - quotient * y;
        }

        private static double ReduceAngle(double value)
        {
            value -= Tau * Floor(value / Tau + 0.5);
            if (value > PI) value -= Tau;
            if (value < -PI) value += Tau;
            return value;
        }

        private static double Pow10(int digits)
        {
            double result = 1;
            while (digits-- > 0)
                result *= 10;
            return result;
        }

        private static float MaxFloating(float x, float y)
        {
            if (IsNaN(x)) return x;
            if (IsNaN(y)) return y;
            if (x == y) return IsNegative(x) ? y : x;
            return MathMaxSingle(x, y);
        }

        private static double MaxFloating(double x, double y)
        {
            if (IsNaN(x)) return x;
            if (IsNaN(y)) return y;
            if (x == y) return IsNegative(x) ? y : x;
            return MathMax(x, y);
        }

        private static float MinFloating(float x, float y)
        {
            if (IsNaN(x)) return x;
            if (IsNaN(y)) return y;
            if (x == y) return IsNegative(x) ? x : y;
            return MathMinSingle(x, y);
        }

        private static double MinFloating(double x, double y)
        {
            if (IsNaN(x)) return x;
            if (IsNaN(y)) return y;
            if (x == y) return IsNegative(x) ? x : y;
            return MathMin(x, y);
        }

        private static bool IsNegative(float value)
        {
            int bits = Unsafe.As<float, int>(ref value);
            return bits < 0;
        }

        private static bool IsNegative(double value)
        {
            long bits = Unsafe.As<double, long>(ref value);
            return bits < 0;
        }

        public static bool IsNaN(float value)
        {
            int bits = Unsafe.As<float, int>(ref value) & 0x7FFFFFFF;
            return bits > 0x7F800000;
        }

        public static bool IsNaN(double value)
        {
            long bits = Unsafe.As<double, long>(ref value) & 0x7FFFFFFFFFFFFFFF;
            return (ulong)bits > 0x7FF0000000000000UL;
        }

        public static bool IsFinite(double value) => !IsNaN(value) && !IsInfinity(value);
        public static bool IsFinite(float value)
        {
            int bits = Unsafe.As<float, int>(ref value) & 0x7FFFFFFF;
            return bits < 0x7F800000;
        }

        public static bool IsInfinity(double value)
        {
            long bits = Unsafe.As<double, long>(ref value) & 0x7FFFFFFFFFFFFFFF;
            return (ulong)bits == 0x7FF0000000000000UL;
        }

        public static bool IsInfinity(float value)
        {
            int bits = Unsafe.As<float, int>(ref value) & 0x7FFFFFFF;
            return bits == 0x7F800000;
        }

        public static bool IsPositiveInfinity(double value) => IsInfinity(value) && !IsNegative(value);
        public static bool IsNegativeInfinity(double value) => IsInfinity(value) && IsNegative(value);
        public static bool IsPositiveInfinity(float value) => IsInfinity(value) && !IsNegative(value);
        public static bool IsNegativeInfinity(float value) => IsInfinity(value) && IsNegative(value);

        private static readonly double NaN = FromBits(unchecked((long)0x7FF8000000000000UL));
        private static readonly double PositiveInfinity = FromBits(unchecked((long)0x7FF0000000000000UL));
        private static readonly double NegativeInfinity = FromBits(unchecked((long)0xFFF0000000000000UL));

        private static double FromBits(long bits) => Unsafe.As<long, double>(ref bits);
    }
}
