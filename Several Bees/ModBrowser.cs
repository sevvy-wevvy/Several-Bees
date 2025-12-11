using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace SeveralBees
{
    public class ModBrowser : MonoBehaviour
    {
        public static ModBrowser Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void GetMods(Action<Dictionary<string, string>> callback)
        {
            _ = GetModsInternal(callback);
        }

        private async Task GetModsInternal(Action<Dictionary<string, string>> callback)
        {
            var dict = new Dictionary<string, string>();
            var url = Config.ModListLink + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            using (HttpClient client = new HttpClient())
            {
                var content = await client.GetStringAsync(url);
                var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;

                    var lastSlash = trimmed.LastIndexOf('/');
                    if (lastSlash < 0) continue;

                    var modName = trimmed.Substring(lastSlash + 1);
                    var dllIndex = modName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase);
                    if (dllIndex > 0) modName = modName.Substring(0, dllIndex);

                    if (!dict.ContainsKey(modName)) dict.Add(modName, trimmed);
                }
            }

            callback?.Invoke(dict);
        }

        internal string GetModName(string link)
        {
            var trimmed = link.Trim();
            if (trimmed.Length == 0) return string.Empty;

            var lastSlash = trimmed.LastIndexOf('/');
            if (lastSlash < 0) return string.Empty;

            var modName = trimmed.Substring(lastSlash + 1);
            var dllIndex = modName.IndexOf(".dll", StringComparison.OrdinalIgnoreCase);
            if (dllIndex > 0) modName = modName.Substring(0, dllIndex);

            return modName;
        }

    }
}
