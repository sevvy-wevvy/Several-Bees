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
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

namespace SeveralBees
{
    [BepInPlugin("com.Sev.gorillatag.SeveralBees", "Several Bees", SeveralBees.Config.CurrentModVersion)]
    public class Plugin : BaseUnityPlugin
    {

        public static Plugin Instance { get; private set; }

        private async void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Plugin Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Plugin Instance Set");

            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] Error loading sound: " + e.Message); }
            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/close.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] Error loading sound: " + e.Message); }
            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/open.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] Error loading sound: " + e.Message); }

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

        internal Dictionary<string, AudioClip> LoadedSounds = new Dictionary<string, AudioClip>();

        internal IEnumerator LoadWav(string fileLink)
        {
            if (string.IsNullOrEmpty(fileLink) || !fileLink.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) yield break;

            if (LoadedSounds.ContainsKey(fileLink)) yield break;

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Several Bees", "Resources", "Sounds");

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            string fileName = Path.GetFileName(fileLink);
            string fullPath = Path.Combine(basePath, fileName);

            using (UnityWebRequest www = UnityWebRequest.Get(fileLink))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    yield break;

                File.WriteAllBytes(fullPath, www.downloadHandler.data);
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + fullPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    yield break;

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                LoadedSounds[fileLink] = clip;
            }
        }

    }
}
