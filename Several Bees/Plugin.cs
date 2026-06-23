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
using System.Linq;

namespace SeveralBees
{
    [BepInPlugin("com.Sev.gorillatag.SeveralBees", "Several Bees", SeveralBees.Config.CurrentModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly string PluginDir = "https://raw.githubusercontent.com/sevvy-wevvy/Several-Bees/refs/heads/main/Plugins/Dir.txt";

        public static Plugin Instance { get; private set; }
        private static bool _skipVersionCheck = false;

        public List<Action> Startup = new List<Action>();
        public List<string> PluginNames = new List<string>();
        public bool PluginDirLoaded = false;

        internal Dictionary<string, AudioClip> LoadedSounds = new Dictionary<string, AudioClip>();

        private string appName;
        private GameObject svrlbs = null;

        private void Awake()
        {
            appName = UnityEngine.Application.productName.Replace(" ", "").ToLowerInvariant();
            UnityEngine.Debug.Log("[Several Bees] Awake " + appName);
            Instance = this;

            StartCoroutine(CheckVersionThenStart());
        }

        private IEnumerator CheckVersionThenStart()
        {
            if (!_skipVersionCheck)
            {
                var url = SeveralBees.Config.ModVersionLink + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 30;
                    yield return request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string latest = request.downloadHandler.text.Trim();
                        if (latest != SeveralBees.Config.CurrentModVersion)
                        {
                            UnityEngine.Debug.Log($"[Several Bees] Update detected: {SeveralBees.Config.CurrentModVersion} -> {latest}. Self-updating...");
                            yield return PerformSelfUpdate();
                            yield break;
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[Several Bees] Version check skipped: " + request.error);
                    }
                }
            }

            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] " + e.Message); }
            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/close.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] " + e.Message); }
            try { StartCoroutine(LoadWav("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/open.wav")); } catch (Exception e) { UnityEngine.Debug.LogError("[Several Bees] " + e.Message); }

            StartCoroutine(LoadPluginDirectory());
            SeveralBees.Config.StartupTriggerThing();
        }

        private IEnumerator PerformSelfUpdate()
        {
            string selfPath = Assembly.GetExecutingAssembly().Location;
            string deletePath = selfPath + ".delete";

            try
            {
                if (File.Exists(deletePath)) File.Delete(deletePath);
                File.Move(selfPath, deletePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[Several Bees] Self-update rename failed: " + e.Message);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(SeveralBees.Config.ModDownload + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss")))
            {
                request.timeout = 30;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogError("[Several Bees] Self-update download failed: " + request.error);
                    try { File.Move(deletePath, selfPath); } catch { }
                    yield break;
                }

                try
                {
                    File.WriteAllBytes(selfPath, request.downloadHandler.data);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("[Several Bees] Self-update write failed: " + e.Message);
                    try { File.Move(deletePath, selfPath); } catch { }
                    yield break;
                }
            }

            UnityEngine.Debug.Log("[Several Bees] Self-update downloaded. Injecting new version...");
            InjectNewVersion(selfPath);

            try { File.Delete(deletePath); } catch { }
            Destroy(this);
        }

        private void InjectNewVersion(string dllPath)
        {
            var inheritedStartup = new List<Action>(Startup);
            try
            {
                Assembly loaded = Assembly.Load(File.ReadAllBytes(dllPath));
                foreach (Type type in loaded.GetTypes())
                {
                    if (!typeof(BaseUnityPlugin).IsAssignableFrom(type) || type.IsAbstract) continue;
                    var meta = type.GetCustomAttributes(typeof(BepInPlugin), true).FirstOrDefault() as BepInPlugin;
                    if (meta == null) continue;

                    _skipVersionCheck = true;
                    GameObject go = new GameObject("Updated SB");
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    go.AddComponent(type);

                    if (Plugin.Instance != null && Plugin.Instance != this)
                        Plugin.Instance.Startup.AddRange(inheritedStartup);

                    UnityEngine.Debug.Log("[Several Bees] Self-update inject complete: " + meta.Name + " " + meta.Version);
                    break;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("[Several Bees] Self-update inject failed: " + e.Message);
            }
        }

        private IEnumerator LoadPluginDirectory()
        {
            var url = PluginDir + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogError("[Several Bees] Plugin dir fetch failed: " + request.error);
                    PluginDirLoaded = true;
                    yield break;
                }

                var lines = request.downloadHandler.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(';').ToList();
                    if (parts.Count != 2) continue;

                    if (!parts[0].Trim().Equals(appName, StringComparison.OrdinalIgnoreCase)) continue;

                    PluginNames = parts[1]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToList();

                    UnityEngine.Debug.Log("[Several Bees] Plugins: " + string.Join(", ", PluginNames));
                    break;
                }

                if (PluginNames.Count == 0)
                    UnityEngine.Debug.LogWarning("[Several Bees] No plugins found for app: " + appName);
            }

            PluginDirLoaded = true;
        }

        internal async void CustomStart()
        {
            try
            {
                svrlbs = new GameObject("Several Bees");
                svrlbs.AddComponent<SeveralBeesCore>();
                svrlbs.AddComponent<Extra>();
                svrlbs.AddComponent<Api>();
                svrlbs.AddComponent<ModBrowser>();
                svrlbs.AddComponent<CustonMenuAPI>();
                UnityEngine.Debug.Log("[Several Bees] Core created");
                AssetLoader.LoadAssets();
                foreach (var action in Startup)
                {
                    try { action(); }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Startup action: " + ex.Message); }
                }
                Startup.Clear();
            }
            catch { }

            try
            {
                var url = global::SeveralBees.Config.ModVersionLink + "?date=" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                using (HttpClient client = new HttpClient())
                {
                    var content = await client.GetStringAsync(url);
                    SeveralBeesCore.Instance.IsLatestVersion = content.Trim() == global::SeveralBees.Config.CurrentModVersion;
                }
            }
            catch { }
        }

        internal IEnumerator LoadWav(string fileLink)
        {
            if (string.IsNullOrEmpty(fileLink) || !fileLink.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) yield break;
            if (LoadedSounds.ContainsKey(fileLink)) yield break;

            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Several Bees", "Resources", "Sounds");
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

            string fullPath = Path.Combine(basePath, Path.GetFileName(fileLink));

            using (UnityWebRequest www = UnityWebRequest.Get(fileLink))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success) yield break;
                File.WriteAllBytes(fullPath, www.downloadHandler.data);
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + fullPath, AudioType.WAV))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success) yield break;
                LoadedSounds[fileLink] = DownloadHandlerAudioClip.GetContent(www);
            }
        }
    }
}