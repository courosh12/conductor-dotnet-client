using System;

namespace ConductorDotnetClient
{
    public class ConductorClientSettings
    {
        public enum IntervalStrategyType
        {
            Fixed,
            Linear,
            Exponential
        }

        public Uri ServerUrl { get; set; }

        public IntervalStrategyType IntervalStrategy { get; set; } = IntervalStrategyType.Exponential;
        public int SleepInterval { get; set; } = 1_000;
        public int MaxSleepInterval { get; set; } = 30_000;
        public string Domain { get; set; }
        public int ConcurrentWorkers { get; set; } = 1;
    }
}
