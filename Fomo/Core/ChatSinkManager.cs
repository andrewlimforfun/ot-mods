using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace Fomo.Core
{
    /// <summary>
    /// Holds a list of <see cref="IChatSink"/> implementations and fans out each
    /// <see cref="ChatEntry"/> to all of them. Errors in individual sinks are caught
    /// and logged without interrupting the others.
    /// </summary>
    public class ChatSinkManager
    {
        private readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource($"{FomoPlugin.ModName}.CSM");
        private readonly List<IChatSink> _sinks = new List<IChatSink>();

        public void Register(IChatSink sink) { 
            _sinks.Add(sink); 
            _log.LogInfo($"Registered chat sink: {sink.GetType().Name}");
        }

        public async Task BroadcastAsync(ChatEntry entry)
        {
            foreach (var sink in _sinks)
            {
                try
                {
                    await sink.SendAsync(entry);
                }
                catch (Exception ex)
                {
                    _log.LogWarning($"[{sink.GetType().Name}] Unhandled error: {ex.Message}");
                }
            }
        }
    }
}
