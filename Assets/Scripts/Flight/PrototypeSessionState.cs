using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StrategyRPG.Flight
{
    /// <summary>Versioned player progress shared between prototype scenes and application launches.</summary>
    public static class PrototypeSessionState
    {
        [Serializable]
        private sealed class SaveData
        {
            public int version = CurrentVersion;
            public List<int> discoveredDestinations = new List<int>();
            public bool azureSurveyComplete;
            public string savedAtUtc;
        }

        private const int CurrentVersion = 1;
        private const string SaveFileName = "prototype-save.json";
        private static readonly HashSet<int> discoveredDestinations = new HashSet<int>();
        private static bool loaded;
        private static bool azureSurveyComplete;

        public static IEnumerable<int> DiscoveredDestinations { get { EnsureLoaded(); return discoveredDestinations; } }
        public static bool AzureSurveyComplete { get { EnsureLoaded(); return azureSurveyComplete; } }
        public static bool HasSave { get { EnsureLoaded(); return File.Exists(SavePath); } }
        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            discoveredDestinations.Clear();
            azureSurveyComplete = false;
            if (!File.Exists(SavePath)) return;

            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (data == null || data.version > CurrentVersion) return;
                if (data.discoveredDestinations != null)
                    foreach (int destination in data.discoveredDestinations)
                        if (destination >= 0 && destination < 4) discoveredDestinations.Add(destination);
                azureSurveyComplete = data.azureSurveyComplete;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Game Frontier save could not be loaded: " + exception.Message);
            }
        }

        public static void MarkDestinationDiscovered(int destinationIndex)
        {
            EnsureLoaded();
            if (discoveredDestinations.Add(destinationIndex)) Save();
        }

        public static void MarkAzureSurveyComplete()
        {
            EnsureLoaded();
            if (azureSurveyComplete) return;
            azureSurveyComplete = true;
            Save();
        }

        public static void Reset()
        {
            loaded = true;
            discoveredDestinations.Clear();
            azureSurveyComplete = false;
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Game Frontier save could not be reset: " + exception.Message);
            }
        }

        private static void Save()
        {
            var data = new SaveData
            {
                version = CurrentVersion,
                discoveredDestinations = new List<int>(discoveredDestinations),
                azureSurveyComplete = azureSurveyComplete,
                savedAtUtc = DateTime.UtcNow.ToString("O")
            };

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                string temporaryPath = SavePath + ".tmp";
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(temporaryPath, SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Game Frontier progress could not be saved: " + exception.Message);
            }
        }
    }
}
