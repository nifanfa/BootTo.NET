#include <stdint.h>
#include <intrin.h>
#include <smmintrin.h>

// Used by Native AOT
uint64_t RhpDbl2ULng(double val)
{
	return((uint64_t)val);
}

double MathSqrt(double value)
{
	__m128d input = _mm_set_sd(value);
	return _mm_cvtsd_f64(_mm_sqrt_sd(_mm_setzero_pd(), input));
}

double MathAbs(double value)
{
	__m128d input = _mm_set_sd(value);
	__m128d mask = _mm_castsi128_pd(_mm_set_epi32(0, 0, 0x7FFFFFFF, 0xFFFFFFFF));
	return _mm_cvtsd_f64(_mm_and_pd(input, mask));
}

double MathMax(double left, double right)
{
	return _mm_cvtsd_f64(_mm_max_sd(_mm_set_sd(left), _mm_set_sd(right)));
}

double MathMin(double left, double right)
{
	return _mm_cvtsd_f64(_mm_min_sd(_mm_set_sd(left), _mm_set_sd(right)));
}

float MathAbsSingle(float value)
{
	__m128 input = _mm_set_ss(value);
	__m128 mask = _mm_castsi128_ps(_mm_set_epi32(0, 0, 0, 0x7FFFFFFF));
	return _mm_cvtss_f32(_mm_and_ps(input, mask));
}

float MathMaxSingle(float left, float right)
{
	return _mm_cvtss_f32(_mm_max_ss(_mm_set_ss(left), _mm_set_ss(right)));
}

float MathMinSingle(float left, float right)
{
	return _mm_cvtss_f32(_mm_min_ss(_mm_set_ss(left), _mm_set_ss(right)));
}

double MathFloor(double value)
{
	return _mm_cvtsd_f64(_mm_round_sd(_mm_setzero_pd(), _mm_set_sd(value), _MM_FROUND_TO_NEG_INF | _MM_FROUND_NO_EXC));
}

double MathCeiling(double value)
{
	return _mm_cvtsd_f64(_mm_round_sd(_mm_setzero_pd(), _mm_set_sd(value), _MM_FROUND_TO_POS_INF | _MM_FROUND_NO_EXC));
}

double MathTruncate(double value)
{
	return _mm_cvtsd_f64(_mm_round_sd(_mm_setzero_pd(), _mm_set_sd(value), _MM_FROUND_TO_ZERO | _MM_FROUND_NO_EXC));
}

double MathRound(double value)
{
	return _mm_cvtsd_f64(_mm_round_sd(_mm_setzero_pd(), _mm_set_sd(value), _MM_FROUND_TO_NEAREST_INT | _MM_FROUND_NO_EXC));
}

int SupportRdrand(void)
{
	int cpuInfo[4];
	__cpuidex(cpuInfo, 1, 0);
	return (cpuInfo[2] & (1 << 30)) != 0;
}

int Rdrand64(uint64_t* value)
{
	return _rdrand64_step((unsigned __int64*)value) != 0;
}
