#include <math.h>
#include <intrin.h>
#include <smmintrin.h>
#include <stdint.h>

static double round_nearest(double value)
{
    return _mm_cvtsd_f64(_mm_round_sd(_mm_setzero_pd(), _mm_set_sd(value),
        _MM_FROUND_TO_NEAREST_INT | _MM_FROUND_NO_EXC));
}

static void sine_cosine(double angle, double* sine, double* cosine)
{
    double turns = round_nearest(angle * 0.63661977236758134308);
    double reduced = (angle - turns * 1.5707963267948966) - turns * 6.123233995736766e-17;
    double square = reduced * reduced;
    double sin_value = reduced * (1.0 + square * (-1.0 / 6.0 + square *
        (1.0 / 120.0 + square * (-1.0 / 5040.0 + square *
        (1.0 / 362880.0 + square * (-1.0 / 39916800.0 + square / 6227020800.0))))));
    double cos_value = 1.0 + square * (-1.0 / 2.0 + square *
        (1.0 / 24.0 + square * (-1.0 / 720.0 + square *
        (1.0 / 40320.0 + square * (-1.0 / 3628800.0 + square / 479001600.0)))));
    switch ((long long)turns & 3)
    {
    case 0: *sine = sin_value; *cosine = cos_value; break;
    case 1: *sine = cos_value; *cosine = -sin_value; break;
    case 2: *sine = -sin_value; *cosine = -cos_value; break;
    default: *sine = -cos_value; *cosine = sin_value; break;
    }
}

float __cdecl sinf(float value)
{
    double sine, cosine;
    sine_cosine(value, &sine, &cosine);
    return (float)sine;
}

float __cdecl cosf(float value)
{
    double sine, cosine;
    sine_cosine(value, &sine, &cosine);
    return (float)cosine;
}

float __cdecl tanf(float value)
{
    double sine, cosine;
    sine_cosine(value, &sine, &cosine);
    return (float)(sine / cosine);
}

double __cdecl tan(double value)
{
    double sine, cosine;
    sine_cosine(value, &sine, &cosine);
    return sine / cosine;
}

static double arctangent(double value)
{
    double magnitude = value < 0.0 ? -value : value;
    if (magnitude > 1.0)
    {
        double result = 1.5707963267948966 - arctangent(1.0 / magnitude);
        return value < 0.0 ? -result : result;
    }
    double reduced = magnitude > 0.41421356237309505
        ? (magnitude - 1.0) / (magnitude + 1.0) : magnitude;
    double square = reduced * reduced;
    double result = reduced * (1.0 + square * (-1.0 / 3.0 + square *
        (1.0 / 5.0 + square * (-1.0 / 7.0 + square *
        (1.0 / 9.0 + square * (-1.0 / 11.0 + square *
        (1.0 / 13.0 + square * (-1.0 / 15.0 + square *
        (1.0 / 17.0 - square / 19.0)))))))));
    if (magnitude > 0.41421356237309505)
        result += 0.78539816339744831;
    return value < 0.0 ? -result : result;
}

float __cdecl atan2f(float y, float x)
{
    double result;
    if (x == 0.0f)
        return y > 0.0f ? 1.5707963267948966f : y < 0.0f ? -1.5707963267948966f : 0.0f;
    result = arctangent((double)y / x);
    if (x < 0.0f)
        result += y < 0.0f ? -3.14159265358979324 : 3.14159265358979324;
    return (float)result;
}

double __cdecl log2(double value)
{
    union { double number; uint64_t bits; } representation;
    int exponent;
    double fraction, reduced, square, sum, term;
    if (value == 0.0)
    {
        representation.bits = 0xfff0000000000000ULL;
        return representation.number;
    }
    if (value < 0.0)
    {
        representation.bits = 0x7ff8000000000000ULL;
        return representation.number;
    }
    representation.bits = 0x7ff0000000000000ULL;
    if (value != value || value == representation.number)
        return value;
    if (value < 2.2250738585072014e-308)
    {
        value *= 18014398509481984.0;
        exponent = -54;
    }
    else
        exponent = 0;
    representation.number = value;
    exponent += (int)((representation.bits >> 52) & 2047) - 1023;
    representation.bits = (representation.bits & 0x000fffffffffffffULL) | 0x3ff0000000000000ULL;
    fraction = representation.number;
    if (fraction > 1.4142135623730951)
    {
        fraction *= 0.5;
        exponent++;
    }
    reduced = (fraction - 1.0) / (fraction + 1.0);
    square = reduced * reduced;
    term = reduced;
    sum = 0.0;
    for (int divisor = 1; divisor <= 19; divisor += 2)
    {
        sum += term / divisor;
        term *= square;
    }
    return exponent + sum * 2.8853900817779268;
}

static double stdlib_exp2(double value)
{
    union { double number; uint64_t bits; } scale;
    long long exponent;
    double reduced, term, sum;
    if (value != value)
        return value;
    if (value >= 1024.0)
    {
        scale.bits = 0x7ff0000000000000ULL;
        return scale.number;
    }
    if (value <= -1075.0)
        return 0.0;
    exponent = (long long)round_nearest(value);
    if (exponent <= -1075)
        return 0.0;
    reduced = (value - exponent) * 0.69314718055994531;
    term = sum = 1.0;
    for (int divisor = 1; divisor <= 12; divisor++)
    {
        term *= reduced / divisor;
        sum += term;
    }
    scale.bits = exponent >= -1022
        ? (uint64_t)(exponent + 1023) << 52
        : 1ULL << (exponent + 1074);
    return sum * scale.number;
}

double __cdecl exp(double value)
{
    return stdlib_exp2(value * 1.4426950408889634074);
}

double __cdecl pow(double base, double exponent)
{
    double magnitude;
    if (exponent == 0.0 || base == 1.0)
        return 1.0;
    if (base == 0.0)
        return exponent < 0.0 ? 1.0 / base : 0.0;
    if (base < 0.0) {
        if (exponent != (double)(long long)exponent)
            return (base - base) / (base - base);
        magnitude = stdlib_exp2(exponent * log2(-base));
        return ((long long)exponent & 1) ? -magnitude : magnitude;
    }
    return stdlib_exp2(exponent * log2(base));
}

float __cdecl powf(float base, float exponent)
{
    return (float)pow(base, exponent);
}

float __cdecl atanf(float value)
{
    return atan2f(value, 1.0f);
}

float __cdecl asinf(float value)
{
    return atan2f(value, sqrtf(1.0f - value * value));
}

float __cdecl acosf(float value)
{
    return atan2f(sqrtf(1.0f - value * value), value);
}

float __cdecl sqrtf(float value)
{
    return _mm_cvtss_f32(_mm_sqrt_ss(_mm_set_ss(value)));
}

double __cdecl sqrt(double value)
{
    return _mm_cvtsd_f64(_mm_sqrt_sd(_mm_setzero_pd(), _mm_set_sd(value)));
}

float __cdecl floorf(float value)
{
    return _mm_cvtss_f32(_mm_round_ss(_mm_setzero_ps(), _mm_set_ss(value),
        _MM_FROUND_TO_NEG_INF | _MM_FROUND_NO_EXC));
}

float __cdecl ceilf(float value)
{
    return _mm_cvtss_f32(_mm_round_ss(_mm_setzero_ps(), _mm_set_ss(value),
        _MM_FROUND_TO_POS_INF | _MM_FROUND_NO_EXC));
}
