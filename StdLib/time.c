#include <stdint.h>
#include <time.h>

typedef struct firmware_time {
    uint16_t year;
    uint8_t month;
    uint8_t day;
    uint8_t hour;
    uint8_t minute;
    uint8_t second;
    uint8_t padding;
    uint32_t nanosecond;
    int16_t timezone;
    uint8_t daylight;
    uint8_t padding2;
} firmware_time;

typedef struct runtime_clock {
    uint64_t header[3];
    uint64_t (__cdecl* get_time)(firmware_time* time, void* capabilities);
} runtime_clock;

static runtime_clock* clock_services;
static struct tm local_calendar;
static int timezone_minutes;

static int leap_year(int year)
{
    return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
}

static int month_days(int year, int month)
{
    static const uint8_t days[12] = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    return days[month - 1] + (month == 2 && leap_year(year));
}

void stdlib_time_initialize(void* runtime_services)
{
    clock_services = (runtime_clock*)runtime_services;
}

__time64_t __cdecl _time64(__time64_t* destination)
{
    firmware_time value;
    int year;
    int month;
    int64_t days = 0;
    int64_t result;
    if (!clock_services || clock_services->get_time(&value, 0) != 0 ||
        value.year < 1970 || value.month < 1 || value.month > 12 ||
        value.day < 1 || value.day > month_days(value.year, value.month)) {
        if (destination)
            *destination = -1;
        return -1;
    }
    for (year = 1970; year < value.year; ++year)
        days += 365 + leap_year(year);
    for (month = 1; month < value.month; ++month)
        days += month_days(value.year, month);
    days += value.day - 1;
    timezone_minutes = value.timezone == 0x7ff ? 0 : value.timezone;
    result = days * 86400 + value.hour * 3600 + value.minute * 60 + value.second;
    result -= timezone_minutes * 60;
    if (destination)
        *destination = result;
    return result;
}

struct tm* __cdecl _localtime64(const __time64_t* epoch)
{
    int64_t local_seconds;
    int64_t days;
    int year = 1970;
    int month = 1;
    int year_days;
    if (!epoch)
        return 0;
    local_seconds = *epoch + timezone_minutes * 60;
    if (local_seconds < 0)
        return 0;
    days = local_seconds / 86400;
    local_seconds %= 86400;
    local_calendar.tm_sec = (int)(local_seconds % 60);
    local_calendar.tm_min = (int)(local_seconds / 60 % 60);
    local_calendar.tm_hour = (int)(local_seconds / 3600);
    local_calendar.tm_wday = (int)((days + 4) % 7);
    while (days >= (year_days = 365 + leap_year(year))) {
        days -= year_days;
        ++year;
    }
    local_calendar.tm_yday = (int)days;
    while (days >= month_days(year, month)) {
        days -= month_days(year, month);
        ++month;
    }
    local_calendar.tm_year = year - 1900;
    local_calendar.tm_mon = month - 1;
    local_calendar.tm_mday = (int)days + 1;
    local_calendar.tm_isdst = 0;
    return &local_calendar;
}
