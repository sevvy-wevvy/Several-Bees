using UnityEngine;
using BepInEx;
using System.Reflection;
using System;
using System.Net.Http;

namespace SeveralBees
{
    [BepInPlugin("com.Sev.gorillatag.SeveralBees", "Several Bees", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private async void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Plugin Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Plugin Instance Set");

            var url = global::SeveralBees.Config.ModVersionLink + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            using (HttpClient client = new HttpClient())
            {
                var content = await client.GetStringAsync(url);
                var latestVersion = content.Trim();
                SeveralBees.Instance.IsLatestVersion = (latestVersion == global::SeveralBees.Config.CurrentModVersion);
            }
        }

        private void Start()
        {
            svrlbs = new GameObject("Several Bees");
            svrlbs.AddComponent<SeveralBees>();
            svrlbs.AddComponent<Extra>();
            svrlbs.AddComponent<Api>();
            svrlbs.AddComponent<ModBrowser>();
            UnityEngine.Debug.Log("[Several Bees] SeveralBees Object Created");
            AssetLoader.LoadAssets();
        }

        GameObject svrlbs = null;
    }
}
