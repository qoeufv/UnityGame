using System.Collections.Generic;

namespace StrategyRPG.Flight
{
    /// <summary>Play-session progress shared between prototype scenes.</summary>
    public static class PrototypeSessionState
    {
        private static readonly HashSet<int> discoveredDestinations = new HashSet<int>();

        public static IEnumerable<int> DiscoveredDestinations => discoveredDestinations;
        public static bool AzureSurveyComplete { get; set; }

        public static void MarkDestinationDiscovered(int destinationIndex)
        {
            discoveredDestinations.Add(destinationIndex);
        }

        public static void Reset()
        {
            discoveredDestinations.Clear();
            AzureSurveyComplete = false;
        }
    }
}
