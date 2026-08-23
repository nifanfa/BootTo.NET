#include <intrin.h>

int IsTcg(void)
{
	int cpuInfo[4];
	__cpuidex(cpuInfo, 1, 0);
	if ((cpuInfo[2] & (1 << 31)) == 0)
		return 0;

	__cpuidex(cpuInfo, 0x40000000, 0);
	return cpuInfo[1] == 0x54474354 && // "TCGT"
		cpuInfo[2] == 0x43544743 &&     // "CGTC"
		cpuInfo[3] == 0x47435447;       // "GTCG"
}
