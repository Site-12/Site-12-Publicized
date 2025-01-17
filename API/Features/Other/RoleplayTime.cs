namespace Site12.API.Features.Other;

using System.Collections.Generic;
using Exiled.API.Features;
using MEC;

internal static class RoleplayTime
{
    internal static int MilitaryTime { get; private set; }

    internal static bool IsClockRunning { get; private set; }

    internal static void StartClock(int startingTime, int secondsPerHour)
    {
        StopClock();
        Timing.RunCoroutine(ClockRoutine(startingTime, secondsPerHour), "roleplay clock");
        IsClockRunning = true;
    }

    internal static void StopClock()
    {
        if (!IsClockRunning)
            return;

        Timing.KillCoroutines("roleplay clock");
        IsClockRunning = false;
        MilitaryTime = 0;
    }

    private static IEnumerator<float> ClockRoutine(int startingTime, int secondsPerHour)
    {
        MilitaryTime = startingTime < 0 ? 0600 : startingTime;
        float secondsPerMinute = secondsPerHour <= 0 ? 60f : secondsPerHour / 60f;
        Log.Debug($"Seconds per minute: {secondsPerMinute}s");

        while (true)
        {
            yield return Timing.WaitForSeconds(secondsPerMinute);

            int hours = MilitaryTime / 100;
            int minutes = MilitaryTime % 100;

            minutes++;
            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
                if (hours >= 24)
                    hours = 0;
            }

            MilitaryTime = hours * 100 + minutes;
            Log.Debug($"Updated military time: {MilitaryTime:D4}");
        }
    }

    internal static string GetFormattedTime()
    {
        int hours = MilitaryTime / 100;
        int minutes = MilitaryTime % 100;

        string period = hours >= 12 ? "PM" : "AM";
        hours %= 12;
        if (hours == 0)
            hours = 12;

        string formattedTime = $"{hours:D2}:{minutes:D2} {period}";
        if (formattedTime.StartsWith("0"))
            formattedTime = formattedTime.Substring(1);

        return formattedTime;
    }
}