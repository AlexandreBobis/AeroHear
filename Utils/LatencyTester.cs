using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AeroHear.Utils
{
    public static class LatencyTester
    {
        public static async Task<int> EstimateLatencyAsync(Action playBeep)
        {
            var stopwatch = Stopwatch.StartNew();
            playBeep();
            await Task.Delay(200); // Simuler une latence mesurable
            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }
    }
}
