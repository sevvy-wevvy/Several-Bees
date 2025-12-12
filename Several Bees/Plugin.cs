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
*/

using UnityEngine;
using BepInEx;
using System.Reflection;
using System;
using System.Net.Http;
using Utilla;
using Utilla.Attributes; // Remove this line for non gorilla tag games

// You will have to remove the utilla dependance from this project aswell for non gorilla tag games

namespace SeveralBees
{
    [BepInPlugin("com.Sev.gorillatag.SeveralBees", "Several Bees", SeveralBees.Config.CurrentModVersion)]
    [ModdedGamemode] // Remove this line for non gorilla tag games
    public class Plugin : BaseUnityPlugin
    {
        internal bool ModdedRoom = false; // Remove this line for non gorilla tag games
        [ModdedGamemodeJoin] // Remove this line for non gorilla tag games
        private void RoomJoined(string gamemode) // Remove this function for non gorilla tag games
        {
            ModdedRoom = true;
        }

        [ModdedGamemodeLeave] // Remove this line for non gorilla tag games
        private void RoomLeft(string gamemode) // Remove function line for non gorilla tag games
        {
            ModdedRoom = false;
        }

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
                SeveralBeesCore.Instance.IsLatestVersion = (latestVersion == global::SeveralBees.Config.CurrentModVersion);
            }
        }

        private void Start()
        {
            svrlbs = new GameObject("Several Bees");
            svrlbs.AddComponent<SeveralBeesCore>();
            svrlbs.AddComponent<Extra>();
            svrlbs.AddComponent<Api>();
            svrlbs.AddComponent<ModBrowser>();
            UnityEngine.Debug.Log("[Several Bees] SeveralBees Object Created");
            AssetLoader.LoadAssets();
        }

        GameObject svrlbs = null;
    }
}

