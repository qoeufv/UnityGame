using UnityEngine;

namespace StrategyRPG.Flight
{
    public enum PlanetEnvironment { Civilized, Barren, Horror }

    [CreateAssetMenu(menuName = "Frontier/Planet Data", fileName = "PlanetData")]
    public sealed class PlanetData : ScriptableObject
    {
        public string displayName = "Unnamed World";
        public PlanetEnvironment environment = PlanetEnvironment.Barren;
        [TextArea(2, 5)] public string description = "An unexplored world.";
        public Color primaryColor = new Color(.2f, .35f, .5f);
        public Color secondaryColor = new Color(.08f, .12f, .18f);
        [Range(0, 5)] public int civilizationLevel;
        public bool landingAvailable = true;
        public string[] pointsOfInterest = { "Survey Site", "Unknown Signal" };
    }
}
