/*
 Copyright (C) 2025 GGGravity
 https://github.com/sevvy-wevvy/Several-Bees/
 
 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
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

