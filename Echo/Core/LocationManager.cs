using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Echo.Core
{
    /// <summary>
    /// Manages named saved locations, persisted to a simple key=x|y|z file.
    /// Instantiate once and store as a singleton - inject the file path for testability.
    /// </summary>
    public class SavedLocationManager
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Vector3> _locations;

        public SavedLocationManager(string filePath)
        {
            _filePath = filePath;
            _locations = new Dictionary<string, Vector3>(System.StringComparer.OrdinalIgnoreCase);
            Load();
        }

        /// <summary>Saves (or overwrites) a named location and persists to disk.</summary>
        public void Save(string name, Vector3 pos)
        {
            _locations[name] = pos;
            Persist();
        }

        /// <summary>Returns true and sets <paramref name="pos"/> if the name is known.</summary>
        public bool TryGet(string name, out Vector3 pos) =>
            _locations.TryGetValue(name, out pos);

        /// <summary>Removes a saved location. Returns true if it existed.</summary>
        public bool Delete(string name)
        {
            if (!_locations.Remove(name)) return false;
            Persist();
            return true;
        }

        /// <summary>Returns a snapshot of all saved locations (name → position).</summary>
        public IReadOnlyDictionary<string, Vector3> GetAll() =>
            new System.Collections.ObjectModel.ReadOnlyDictionary<string, Vector3>(_locations);

        private void Load()
        {
            if (!File.Exists(_filePath)) return;
            foreach (string line in File.ReadAllLines(_filePath))
            {
                int eq = line.IndexOf('=');
                if (eq < 1) continue;
                string key = line.Substring(0, eq).Trim();
                string[] parts = line.Substring(eq + 1).Split('|');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    _locations[key] = new Vector3(x, y, z);
                }
            }
        }

        private void Persist()
        {
            var lines = new List<string>();
            foreach (var kv in _locations)
            {
                string x = kv.Value.x.ToString(CultureInfo.InvariantCulture);
                string y = kv.Value.y.ToString(CultureInfo.InvariantCulture);
                string z = kv.Value.z.ToString(CultureInfo.InvariantCulture);
                lines.Add($"{kv.Key}={x}|{y}|{z}");
            }
            File.WriteAllLines(_filePath, lines);
        }
    }
}
