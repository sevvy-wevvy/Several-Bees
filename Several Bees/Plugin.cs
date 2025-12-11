using UnityEngine;
using BepInEx;
using System.Reflection;

namespace SeveralBees
{
    [BepInPlugin("com.Sev.gorillatag.SeveralBees", "Several Bees", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Plugin Awake");
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();
            foreach (var n in names)
            {
                Debug.Log("[Several Bees] Embedded Resource: " + n);
            }
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Plugin Instance Set");
        }

        private void Start()
        {
            svrlbs = new GameObject("Several Bees");
            svrlbs.AddComponent<SeveralBees>();
            svrlbs.AddComponent<Extra>();
            svrlbs.AddComponent<Api>();
            svrlbs.AddComponent<ModBrowser>();
            UnityEngine.Debug.Log("[Several Bees] SeveralBees Object Created");
            try
            {
                SeveralBees.Instance.Bundle = Extra.Instance.LoadAssetBundle("SeveralBees.sbbundle");
            }
            catch { }
            SeveralBees.Instance.Bundle.GetAllAssetNames().ForEach(name => UnityEngine.Debug.Log("[Several Bees] Bundle Asset: " + name));
            UnityEngine.Debug.Log("[Several Bees] Asset Bundle Loaded");
        }

        GameObject svrlbs = null;
    }
}
