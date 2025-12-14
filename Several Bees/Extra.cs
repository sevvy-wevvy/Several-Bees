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
using System;
using System.IO;
using System.Reflection;
using SeveralBees;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SeveralBees
{
    public class Extra : MonoBehaviour
    {
        public static Extra Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Extra Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Extra Instance Set");
        }

        /*-------------------------------------------------------------------------------------------------------------------*/







        // this is from a scrapped project, should i bring it back?
        public static string GradientText(string text, Color colorStart, Color colorEnd, float scrollSpeed = 0.2f)
        {
            if (string.IsNullOrEmpty(text)) return "";

            float timeOffset = Time.time * scrollSpeed;
            string result = "";

            for (int i = 0; i < text.Length; i++)
            {
                float t = (i / (float)text.Length + timeOffset) % 1f;
                Color c = Color.Lerp(colorStart, colorEnd, t);
                string hex = ColorUtility.ToHtmlStringRGB(c);
                result += $"<color=#{hex}>{text[i]}</color>";
            }

            return result;
        }

        public void MakeObjectVisible(GameObject obj, bool ResetColor = true)
        {
            try
            {
                if (Config.PropperShader == "Def") return;
                obj.GetComponent<Renderer>().material.shader = Shader.Find(Config.PropperShader);
                if (ResetColor)
                {
                    obj.GetComponent<Renderer>().material.color = Color.white;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] Error in NakeObjectVisible: " + e.Message);
            }
        }

        public void SetTheme(Color nm1, Color nm2, float fadeSpeed = -8275892375)
        {
            SeveralBeesCore.Instance.Theme1 = nm1;
            SeveralBeesCore.Instance.Theme2 = nm2;
            if(fadeSpeed != -8275892375)
            {
                SeveralBeesCore.Instance.ThemeFadeSpeed = fadeSpeed;
            }
            Settings.SetButtonNames();
            Settings.Save();
            SeveralBeesCore.Instance.ReMakeModManger();
        }


        // Orignal asset loader by Skellon, slight tweaks so it works with other bundles https://github.com/skellondev
        private Dictionary<string, List<UnityEngine.Object>> _assetDict = new Dictionary<string, List<UnityEngine.Object>>();
        public List<string> LoadedBundles = new List<string>();
        public bool TryGetAsset<T>(string BundleName, string name, out T obj) where T : UnityEngine.Object
        {
            if (LoadedBundles.Contains(BundleName) && _assetDict[BundleName].FirstOrDefault(asset => asset.name == name) is T prefab)
            {
                obj = prefab;
                return true;
            }

            obj = null!;
            return false;
        }
        public void AssetLoad(string BundleName)
        {
            try
            {
                if (LoadedBundles.Contains(BundleName)) throw new Exception("Assets already loaded.");
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream? stream = assembly.GetManifestResourceStream(BundleName);
                AssetBundle bundle = AssetBundle.LoadFromStream(stream ?? throw new Exception("[Several Bees] Failed to get stream."));

                UnityEngine.Debug.Log($"[Several Bees] Retrieved bundle: {(bundle ?? throw new Exception("[Several Bees] Failed to get bundle.")).name}");
                foreach (var asset in bundle.LoadAllAssets())
                {
                    _assetDict[BundleName].AddIfNew(asset);
                    UnityEngine.Debug.Log($"[Several Bees] Loaded asset: {asset.name} ({asset.GetType().FullName})");
                }

                stream.Close();
                LoadedBundles.Add(BundleName);
                UnityEngine.Debug.Log($"[Several Bees] Loaded {_assetDict[BundleName].Count} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }
    }

    internal static class AssetLoader
    {
        // https://github.com/skellondev

        private static readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        public static bool BundleLoaded => _assets.Count > 0;
        public static bool TryGetAsset<T>(string name, out T obj) where T : UnityEngine.Object
        {
            if (BundleLoaded && _assets.FirstOrDefault(asset => asset.name == name) is T prefab)
            {
                obj = prefab;
                return true;
            }

            obj = null!;
            return false;
        }
        public static void LoadAssets()
        {
            try
            {
                if (BundleLoaded) throw new Exception("Assets already loaded.");
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream? stream = assembly.GetManifestResourceStream($"SeveralBees.sbbundle");
                AssetBundle bundle = AssetBundle.LoadFromStream(stream ?? throw new Exception("Failed to get stream."));

                UnityEngine.Debug.Log($"Retrieved bundle: {(bundle ?? throw new Exception("Failed to get bundle.")).name}");
                foreach (var asset in bundle.LoadAllAssets())
                {
                    _assets.AddIfNew(asset);
                    UnityEngine.Debug.Log($"Loaded asset: {asset.name} ({asset.GetType().FullName})");
                }

                stream.Close();
                UnityEngine.Debug.Log($"Loaded {_assets.Count} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }
    }
}
