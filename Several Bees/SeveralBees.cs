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

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using SeveralBees;
using System.Collections;
using BepInEx;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SeveralBees.Scripts;
using UnityEngine.Networking;
using System.Reflection;
using BepInEx.Logging;
using Constants;

namespace SeveralBees
{
    public class SeveralBeesCore : MonoBehaviour
    {
        #region Fields

        internal bool IsLatestVersion = true;
        public bool TestMode = false;
        public static SeveralBeesCore Instance { get; private set; }

        internal Color Theme1 = new Color(0.5f, 0f, 1f);
        internal Color Theme2 = Color.black;
        internal float ThemeFadeSpeed = 0.2f;

        internal int PointerPositionIndex = 0;
        internal string SectionName = "Main";
        internal bool LoadedPlugins = false;
        internal bool PCControlActive = false;
        internal bool ShowGUIMenu = false;
        internal string ToolTipText = "";
        internal float ToolTipKillTime = 8f;

        internal string TestMod1Token = "";
        internal string TestMod2Token = "";
        internal string TestMod3Token = "";

        internal GameObject ErrorParent = null;
        internal AssetBundle Bundle;

        private GameObject ModManegerParent = null;
        private List<TextMeshPro> ModMangerTextList = new List<TextMeshPro>();
        private List<GameObject> ModMangerDistanceIndicators = new List<GameObject>();
        private int ErrorInt = 1;
        private int MaxPointerPosition = 0;
        private float lastSpawnTime;

        private bool DownArrowPress = false;
        private bool UpArrowPress = false;
        private bool EnterPress = false;
        private bool TestModeDone = false;
        private bool SpawnNewThingPress = false;
        private bool GuiButtonPress = false;
        private bool PBB = PlayerPrefs.GetInt("SBPhysBackButton", 0) == 1;

        private Vector3 previousLeftPos;
        private Vector3 previousRightPos;
        private Coroutine TooltipKillCoroutine = null;

        private GUIStyle gradientStyle;
        private Vector2 scrollPosition;
        private Rect menuRect = new Rect(10, 50, 400, 300);
        private bool dragging = false;
        private Vector2 dragOffset;

        private Dictionary<string, List<ConfigEntry>> configCache = new Dictionary<string, List<ConfigEntry>>();

        private string currentSortMode = "Classic";
        private List<KeyValuePair<string, string>> cachedMods = new List<KeyValuePair<string, string>>();
        private Dictionary<string, int> modDownloadCounts = new Dictionary<string, int>();
        private bool fetchingDownloadCounts = false;
        private HashSet<string> modUpdateAvailable = new HashSet<string>();
        private bool fetchingModUpdates = false;

        private const string SbApiBase = "https://sevvy-wevvy.com/mods/sb/api.php";
        private string sbToken = null;
        private string sbUsername = null;
        private List<SbMod> sbModCache = new List<SbMod>();
        private List<SbMod> sbModDisplayed = new List<SbMod>();
        private bool sbFetchingMods = false;
        private string sbCurrentBrowserTab = "all";
        private HashSet<int> sbUpvotedMods = new HashSet<int>();
        private bool sbLoginPending = false;
        private string sbLoginCode = null;
        private int sbModPageSize = 15;
        private int sbModDisplayCount = 15;

        #endregion

        #region Color & Float Cycles

        internal List<DetailedColor> CycleColors = new List<DetailedColor>
        {
            new DetailedColor { color = Color.red,                        name = "Red"     },
            new DetailedColor { color = new Color(1f, 0.5f, 0f),          name = "Orange"  },
            new DetailedColor { color = Color.yellow,                     name = "Yellow"  },
            new DetailedColor { color = Color.green,                      name = "Green"   },
            new DetailedColor { color = Color.cyan,                       name = "Cyan"    },
            new DetailedColor { color = Color.blue,                       name = "Blue"    },
            new DetailedColor { color = new Color(0.5f, 0f, 1f),          name = "Purple"  },
            new DetailedColor { color = Color.black,                      name = "Black"   },
            new DetailedColor { color = Color.white,                      name = "White"   },
            new DetailedColor { color = Color.magenta,                    name = "Magenta" },
            new DetailedColor { color = new Color(1f, 0.75f, 0.8f),       name = "Pink"    },
            new DetailedColor { color = new Color(0.6f, 0.3f, 0f),        name = "Brown"   },
            new DetailedColor { color = Color.gray,                       name = "Gray"    },
            new DetailedColor { color = Color.green * 1.5f,               name = "Lime"    },
            new DetailedColor { color = new Color(0f, 0f, 0.5f),          name = "Navy"    },
            new DetailedColor { color = new Color(0.75f, 0.75f, 0.75f),   name = "Silver"  },
        };

        internal List<float> CycleFloats = new List<float>
        {
            0.1f, 0.2f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f, 5f,
        };

        #endregion

        #region Lifecycle

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Awake");
            Instance = this;
            CheckModUpdatesAsync();
            SbValidateStoredToken();
            SbValidateInstalledMods();
        }

        private async void Start()
        {
            try
            {
                if (ErrorParent == null)
                    ErrorParent = new GameObject("Several Bees || Error Parent");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] " + e.Message);
            }

            try { await Create(); }
            catch (Exception e) { UnityEngine.Debug.Log("[Several Bees] " + e.Message); }

            if (Config.IsGui) ShowGUIMenu = true;

            await LoadPluginsAsync();
        }

        #endregion

        #region Plugin System

        private async Task LoadPluginsAsync()
        {
            float waited = 0f;
            while (!Plugin.Instance.PluginDirLoaded && waited < 10f)
            {
                await Task.Delay(100);
                waited += 0.1f;
            }

            if (Plugin.Instance.PluginNames.Count == 0)
            {
                LoadedPlugins = true;
                if (SectionName == "LoadPlugins") SectionName = "Main";
                return;
            }

            foreach (string link in Plugin.Instance.PluginNames)
            {
                try
                {
                    string tag = await ModBrowser.Instance.GetGitHubTagAsync(link);
                    string saved = PlayerPrefs.GetString("SBPluginVer_" + link, "");
                    if (saved == tag) continue;
                    InstallModAndInjectPlugin(link, ModBrowser.Instance.GetModName(link));
                    PlayerPrefs.SetString("SBPluginVer_" + link, tag);
                }
                catch { }
            }

            LoadedPlugins = true;
            if (SectionName == "LoadPlugins") SectionName = "Main";
        }

        internal void InstallModAndInjectPlugin(string modLink, string modName)
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);

            string dllPath = Path.Combine(pluginsPath, modName + ".dll");
            if (File.Exists(dllPath)) File.Delete(dllPath);

            using (var client = new System.Net.WebClient())
            {
                client.DownloadFileAsync(new Uri(modLink), dllPath);
                while (client.IsBusy) Thread.Sleep(100);
            }

            TryInjectAssembly(dllPath);
        }

        private void TryInjectAssembly(string dllPath)
        {
            try
            {
                Assembly loaded = Assembly.Load(File.ReadAllBytes(dllPath));

                foreach (Type type in loaded.GetTypes())
                {
                    if (!typeof(BaseUnityPlugin).IsAssignableFrom(type) || type.IsAbstract) continue;

                    var meta = type.GetCustomAttributes(typeof(BepInPlugin), true).FirstOrDefault() as BepInPlugin;
                    if (meta == null) continue;

                    GameObject go = new GameObject(meta.GUID);
                    UnityEngine.Object.DontDestroyOnLoad(go);

                    var plugin = (BaseUnityPlugin)go.AddComponent(type);
                    var info = new PluginInfo();
                    typeof(PluginInfo).GetProperty("Metadata", BindingFlags.Public | BindingFlags.Instance)?.SetValue(info, meta);
                    typeof(PluginInfo).GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance)?.SetValue(info, plugin);
                    typeof(BaseUnityPlugin).GetProperty("Info", BindingFlags.Public | BindingFlags.Instance)?.SetValue(plugin, info);
                }

                foreach (var action in Plugin.Instance.Startup)
                {
                    try { action(); }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Startup action: " + ex.Message); }
                }
                Plugin.Instance.Startup.Clear();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[Several Bees] Inject error: " + ex.Message);
            }
        }

        #endregion

        #region Mod Manager Setup

        internal void ReMakeModManger()
        {
            if (ModManegerParent != null) Destroy(ModManegerParent);
            Create();
        }

        internal async Task Create()
        {
            try
            {
                try
                {
                    if (!Api.Instance.HasMadeSettings)
                    {
                        MakeSettings();
                        Api.Instance.HasMadeSettings = true;
                    }
                }
                catch { }

                var mm = InstanceModManger();
                mm.name = "Several Bees || Mod Manger";
                ModManegerParent = mm;

                ModManegerParent.transform.position = Config.MachineSpawnPoint;
                ModManegerParent.transform.rotation = Quaternion.Euler(Config.MachineSpawnRoto);
                ModManegerParent.transform.localScale = Config.MachineSpawnScale;

                if (Config.CompType == ComputerType.None)
                {
                    ModManegerParent.transform.position = new Vector3(0f, -int.MaxValue, 0f);
                    ModManegerParent.transform.rotation = Quaternion.identity;
                    ModManegerParent.transform.localScale = Vector3.zero;
                    ModManegerParent.name = "Several Bees || Mod Manger (Killed)";
                }
            }
            catch (Exception e)
            {
                ListError("Create error: " + e.Message + $" [{e.StackTrace}]");
            }
        }

        internal GameObject InstanceModManger()
        {
            var parent = new GameObject("Several Bees || Mod Manger (Instance)");

            try
            {
                if (Config.CompType == ComputerType.SimpleButtons || Config.CompType == ComputerType.Text)
                    AddDisplayText(parent, new Vector3(0f, 0.4f, 0f));

                if (Config.CompType == ComputerType.SimpleButtons)
                    BuildSimpleButtons(parent);

                if (Config.CompType == ComputerType.FullComputer)
                    BuildFullComputer(parent);

                if (Config.CompType == ComputerType.FullMachine)
                    BuildFullMachine(parent);
            }
            catch
            {
                BuildFallback(parent);
            }

            var indicator = new GameObject("Distance Indicator");
            indicator.transform.SetParent(parent.transform);
            ModMangerDistanceIndicators.Add(indicator);

            return parent;
        }

        private TextMeshPro AddDisplayText(GameObject parent, Vector3 localPos, Vector2? rectSize = null)
        {
            var obj = new GameObject("SB_Text");
            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = "<color=orange>Loading...</color>";
            tmp.fontSize = 0.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            obj.transform.SetParent(parent.transform);
            obj.transform.position = localPos;
            if (rectSize.HasValue) obj.GetComponent<RectTransform>().sizeDelta = rectSize.Value;
            ModMangerTextList.Add(tmp);
            return tmp;
        }

        private void BuildSimpleButtons(GameObject parent)
        {
            var btnParent = new GameObject("Buttons");
            btnParent.transform.SetParent(parent.transform);

            SpawnNavCube(btnParent, "SB_Down", new Vector3(-0.1f, 0f, 0.075f), l => MmDown(l));
            SpawnNavCube(btnParent, "SB_Up", new Vector3(-0.1f, 0f, -0.075f), l => MmUp(l));

            var sel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sel.name = "SB_Select";
            sel.transform.position = new Vector3(0.125f, 0f, 0f);
            sel.transform.SetParent(btnParent.transform);
            sel.transform.localScale = new Vector3(0.05f, 0.25f, 0.25f);
            sel.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
            sel.AddComponent<Scripts.Button>().Name = "SB_Select_Button";
            sel.GetComponent<Scripts.Button>().Click += l => MmSelect(l);
            Extra.Instance.MakeObjectVisible(sel);
            sel.GetComponent<Renderer>().material.color = Theme2;

            btnParent.transform.rotation = Quaternion.Euler(90f, 0, 0f);
            btnParent.transform.position = new Vector3(-0.025f, 0f, 0f);

            foreach (Transform t in btnParent.transform)
            {
                var col = t.gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }
        }

        private void SpawnNavCube(GameObject parent, string name, Vector3 pos, Action<bool> onClick)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = pos;
            cube.transform.SetParent(parent.transform);
            cube.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);
            cube.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
            cube.AddComponent<Scripts.Button>().Name = name + "_Button";
            cube.GetComponent<Scripts.Button>().Click += onClick;
            Extra.Instance.MakeObjectVisible(cube);
            cube.GetComponent<Renderer>().material.color = Theme2;
        }

        private void BuildFullComputer(GameObject parent)
        {
            var tmp = AddDisplayText(parent, new Vector3(0f, 0.38f, -0.01f), new Vector2(0.4f, 0.35f));
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = 0.5f;
            tmp.fontSizeMin = 0.2f;

            if (!AssetLoader.TryGetAsset<GameObject>("Sb Computer Variant", out var prefab)) return;

            var computer = Instantiate(prefab);
            computer.transform.SetParent(parent.transform);
            BindMachineButtons(computer, hasBack: false);
            computer.transform.position = new Vector3(0.07f, 0.09f, -0.1563f);
            computer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            computer.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        }

        private void BuildFullMachine(GameObject parent)
        {
            AddDisplayText(parent, new Vector3(-0.005f, 0.38f, -0.01f));

            if (!AssetLoader.TryGetAsset<GameObject>("SbMachine", out var prefab)) return;

            var machine = Instantiate(prefab);
            machine.transform.SetParent(parent.transform);
            BindMachineButtons(machine, hasBack: true);

            var mfbbh = new GameObject("Machine Full Back Button Handler");
            mfbbh.AddComponent<MachineFullBackButtonHandler>();
            mfbbh.transform.SetParent(parent.transform);

            machine.transform.position = new Vector3(0f, 0.04f, 0.02f);
            machine.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
            machine.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        }

        private void BuildFallback(GameObject parent)
        {
            var obj = new GameObject("SB_Text");
            var tmp = obj.AddComponent<TextMeshPro>();
            tmp.text = "Several Bees";
            tmp.fontSize = 0.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            obj.transform.SetParent(parent.transform);
            obj.transform.position = new Vector3(0f, 0.4f, 0f);
            ModMangerTextList.Add(tmp);

            var btnParent = new GameObject("Buttons");
            btnParent.transform.SetParent(parent.transform);

            SpawnNavCube(btnParent, "SB_Down", new Vector3(-0.1f, 0f, 0.075f), l => MmDown(l));
            SpawnNavCube(btnParent, "SB_Up", new Vector3(-0.1f, 0f, -0.075f), l => MmUp(l));

            var sel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sel.name = "SB_Select";
            sel.transform.position = new Vector3(0.125f, 0f, 0f);
            sel.transform.SetParent(btnParent.transform);
            sel.transform.localScale = new Vector3(0.05f, 0.25f, 0.25f);
            sel.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
            sel.AddComponent<Scripts.Button>().Name = "SB_Select_Button";
            sel.GetComponent<Scripts.Button>().Click += l => MmSelect(l);
            Extra.Instance.MakeObjectVisible(sel);
            sel.GetComponent<Renderer>().material.color = Theme2;

            btnParent.transform.rotation = Quaternion.Euler(90f, 0, 0f);
            btnParent.transform.position = new Vector3(-0.025f, 0f, 0f);

            foreach (Transform t in btnParent.transform)
            {
                t.gameObject.AddComponent<BoxCollider>().isTrigger = false;
            }
        }

        private void BindMachineButtons(GameObject root, bool hasBack)
        {
            foreach (Transform child in root.transform)
            {
                Extra.Instance.MakeObjectVisible(child.gameObject, false);
                switch (child.name)
                {
                    case "Up":
                        child.AddComponent<Scripts.Button>().Click += l => MmUp(l);
                        child.AddComponent<BoxCollider>().isTrigger = true;
                        break;
                    case "Down":
                        child.AddComponent<Scripts.Button>().Click += l => MmDown(l);
                        child.AddComponent<BoxCollider>().isTrigger = true;
                        break;
                    case "Select":
                        child.AddComponent<Scripts.Button>().Click += l => MmSelect(l);
                        child.AddComponent<BoxCollider>().isTrigger = true;
                        break;
                    case "Back" when hasBack:
                        child.AddComponent<Scripts.Button>().Click += l => MmBack(l);
                        child.AddComponent<BoxCollider>().isTrigger = true;
                        child.gameObject.SetActive(false);
                        break;
                    case "Back1":
                    case "Back2":
                        child.gameObject.SetActive(false);
                        break;
                }
            }
        }

        #endregion

        #region Navigation

        internal void MmDown(bool left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            if (MaxPointerPosition == 0) return;
            PointerPositionIndex = (PointerPositionIndex + 1) % MaxPointerPosition;
        }

        internal void MmUp(bool left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            if (MaxPointerPosition == 0) return;
            PointerPositionIndex = (PointerPositionIndex - 1 + MaxPointerPosition) % MaxPointerPosition;
        }

        internal void MmSelect(bool left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            var things = GetThings();
            if (things.Count == 0 || PointerPositionIndex >= things.Count) return;

            var thing = things[PointerPositionIndex];

            if (thing.Enterable)
            {
                SectionName = thing.Token;
                PointerPositionIndex = 0;
                SetToolTip(thing.Name == "Back"
                    ? "<color=grey>[</color>Back<color=grey>]</color> Takes you back to the last tab."
                    : $"<color=grey>[</color>{thing.Name}<color=grey>]</color> Takes you to the '{thing.Name}' tab.");
                return;
            }

            var mbi = thing.mbi;
            if (mbi == null) return;

            if (mbi.isTogglable)
            {
                mbi.enabled = !mbi.enabled;
                (mbi.enabled ? mbi.enableMethod : mbi.disableMethod)?.Invoke();
            }
            else
            {
                mbi.method?.Invoke();
            }

            if (mbi.toolTip == null) return;

            string label = mbi.buttonOverlayText ?? mbi.buttonText;
            if (mbi.isTogglable)
            {
                string col = mbi.enabled ? "green" : "red";
                SetToolTip($"<color=grey>[</color><color={col}>{label}</color><color=grey>]</color> {mbi.toolTip}");
            }
            else
            {
                SetToolTip($"<color=grey>[</color>{label}<color=grey>]</color> {mbi.toolTip}");
            }
        }

        internal void MmBack(bool left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            if (SectionName == "Main")
            {
                SetToolTip($"<color=grey>[</color>{Extra.GradientText("Several Bees", Theme1, Theme2, ThemeFadeSpeed)}<color=grey>]</color> You are already at the main menu.");
                return;
            }
            string back = "Main";
            try { back = Api.Instance.tokenListBackToken[SectionName]; }
            catch (Exception e) { ListError("Back token error: " + e.Message); }
            Api.Instance.OpenMenu(back);
        }

        internal void SetToolTip(string text)
        {
            if (TooltipKillCoroutine != null) { StopCoroutine(TooltipKillCoroutine); TooltipKillCoroutine = null; }
            ToolTipText = text;
            TooltipKillCoroutine = StartCoroutine(TooltipKill());
        }

        private IEnumerator TooltipKill()
        {
            yield return new WaitForSeconds(ToolTipKillTime);
            ToolTipText = "";
        }

        internal List<Things> GetThings()
        {
            var things = new List<Things>();

            if (SectionName == "Main")
            {
                foreach (var kv in Api.Instance.tokenList)
                    if (Api.Instance.tokenListVisable[kv.Key])
                        things.Add(new Things { Name = kv.Value, Enterable = true, Token = kv.Key });
                return things;
            }

            if (SectionName == "NotNew" || SectionName == "LoadPlugins")
            {
                foreach (var mbi in Api.Instance.tokenListButtonInfo[SectionName])
                    things.Add(new Things { Name = FormatButtonName(mbi), Enterable = false, Token = SectionName, mbi = mbi });
                return things;
            }

            string backTarget = "Main";
            try { backTarget = Api.Instance.tokenListBackToken[SectionName]; }
            catch (Exception e) { ListError("Back token error: " + e.Message); }

            if (!Api.Instance.GrabButton("8", "Physical Back Button").enabled)
                things.Add(new Things { Name = "<color=red>Back</color>", Enterable = true, Token = backTarget });

            foreach (var mbi in Api.Instance.tokenListButtonInfo[SectionName])
                things.Add(new Things { Name = FormatButtonName(mbi), Enterable = false, Token = SectionName, mbi = mbi });

            return things;
        }

        private string FormatButtonName(ModButtonInfo mbi)
        {
            if (!mbi.isTogglable) return mbi.buttonText;
            return mbi.enabled
                ? $"<color=green>{mbi.buttonText} [ON]</color>"
                : $"<color=red>{mbi.buttonText} [OFF]</color>";
        }

        internal List<string> GetButtons() => GetThings().Select(t => t.Name).ToList();

        #endregion

        #region Settings Menus

        internal void MakeSettings()
        {
            try
            {
                Api.Instance.tokenList.Add("1", "Settings");
                Api.Instance.tokenListVisable.Add("1", true);
                Api.Instance.tokenListBackToken.Add("1", "Main");
                Api.Instance.tokenListButtonInfo["1"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "General Settings", method = () => Api.Instance.OpenMenu("8"), toolTip = "Opens the general settings." },
                    new ModButtonInfo { buttonText = "Theme Settings",   method = () => Api.Instance.OpenMenu("2"), toolTip = "Opens the theme settings." },
                    new ModButtonInfo { buttonText = "<color=yellow>Credits</color>", method = () => Api.Instance.OpenMenu("7"), toolTip = "Opens the credits." },
                    new ModButtonInfo { buttonText = "<color=green>Donate</color>",   method = () => Process.Start(new ProcessStartInfo { FileName = "https://sevvy-wevvy.com/donate/", UseShellExecute = true }), toolTip = "Lets you donate to me." },
                };

                Api.Instance.tokenList.Add("2", "Theme");
                Api.Instance.tokenListVisable.Add("2", false);
                Api.Instance.tokenListBackToken.Add("2", "1");
                Api.Instance.tokenListButtonInfo["2"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Theme Presets", method = () => Api.Instance.OpenMenu("3"), toolTip = "Opens the theme presets." },
                    new ModButtonInfo { buttonText = "cfs", method = () => Settings.cfs(), toolTip = "Adjusts the fade speed." },
                    new ModButtonInfo { buttonText = "nfc", method = () => Settings.nfc(), toolTip = "Changes the first fade color." },
                    new ModButtonInfo { buttonText = "nsc", method = () => Settings.nsc(), toolTip = "Changes the second fade color." },
                };

                Api.Instance.tokenList.Add("3", "Theme Presets");
                Api.Instance.tokenListVisable.Add("3", false);
                Api.Instance.tokenListBackToken.Add("3", "2");
                Api.Instance.tokenListButtonInfo["3"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Defualt",   buttonOverlayText = "<color=#8A2BE2>D</color><color=#9B30FF>e</color><color=#A64AC9>f</color><color=#B266FF>u</color><color=#C080FF>a</color><color=#D19EFF>l</color><color=#E0BFFF>t</color>",                                                                                         method = () => Extra.Instance.SetTheme(CycleColors[6].color,  CycleColors[7].color,  0.2f), toolTip = "Purple and black."    },
                    new ModButtonInfo { buttonText = "Breeze",    buttonOverlayText = "<color=#00FFFF>B</color><color=#1CEEEE>r</color><color=#3AFFFF>e</color><color=#5AFFEE>e</color><color=#7AFFFF>z</color><color=#9CFFFF>e</color>",                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[4].color,  CycleColors[9].color,  0.2f), toolTip = "Cyan and magenta."    },
                    new ModButtonInfo { buttonText = "Rose",      buttonOverlayText = "<color=#FFBFCF>R</color><color=#FF9FBF>o</color><color=#FF7FBF>s</color><color=#FF5FAF>e</color>",                                                                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[10].color, CycleColors[8].color,  0.2f), toolTip = "Pink and white."      },
                    new ModButtonInfo { buttonText = "Earth",     buttonOverlayText = "<color=#964B00>E</color><color=#A65A0F>a</color><color=#B66B1F>r</color><color=#C57C2F>t</color><color=#D58D3F>h</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[11].color, CycleColors[3].color,  0.2f), toolTip = "Brown and green."     },
                    new ModButtonInfo { buttonText = "Storm",     buttonOverlayText = "<color=#808080>S</color><color=#909090>t</color><color=#A0A0A0>o</color><color=#B0B0B0>r</color><color=#C0C0C0>m</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[12].color, CycleColors[7].color,  0.2f), toolTip = "Gray and black."      },
                    new ModButtonInfo { buttonText = "Ocean",     buttonOverlayText = "<color=#00BFFF>O</color><color=#1ECFFF>c</color><color=#3ADFFF>e</color><color=#56EFFF>a</color><color=#72FFFF>n</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[5].color,  CycleColors[4].color,  0.2f), toolTip = "Blue and cyan."       },
                    new ModButtonInfo { buttonText = "Neon",      buttonOverlayText = "<color=#7FFF00>N</color><color=#8CFF19>e</color><color=#9AFF33>o</color><color=#A7FF4D>n</color>",                                                                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[13].color, CycleColors[4].color,  0.2f), toolTip = "Lime and cyan."       },
                    new ModButtonInfo { buttonText = "Toxic",     buttonOverlayText = "<color=#7FFF00>T</color><color=#8CFF19>o</color><color=#9AFF33>x</color><color=#A7FF4D>i</color><color=#B5FF66>c</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[13].color, CycleColors[11].color, 0.2f), toolTip = "Lime and brown."      },
                    new ModButtonInfo { buttonText = "Royal",     buttonOverlayText = "<color=#8A2BE2>R</color><color=#9B30FF>o</color><color=#A64AC9>y</color><color=#B266FF>a</color><color=#C080FF>l</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[6].color,  CycleColors[14].color, 0.2f), toolTip = "Purple and navy."     },
                    new ModButtonInfo { buttonText = "Flare",     buttonOverlayText = "<color=#FF0000>F</color><color=#FF1A1A>l</color><color=#FF3333>a</color><color=#FF4D4D>r</color><color=#FF6666>e</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[0].color,  CycleColors[1].color,  0.2f), toolTip = "Red and orange."      },
                    new ModButtonInfo { buttonText = "Sunset",    buttonOverlayText = "<color=#FFA500>S</color><color=#FFB733>u</color><color=#FFC966>n</color><color=#FFD999>s</color><color=#FFECCC>e</color><color=#FFF0FF>t</color>",                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[1].color,  CycleColors[8].color,  0.2f), toolTip = "Orange and white."    },
                    new ModButtonInfo { buttonText = "Solar",     buttonOverlayText = "<color=#FFFF00>S</color><color=#FFFF33>o</color><color=#FFFF66>l</color><color=#FFFF99>a</color><color=#FFFFCC>r</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[2].color,  CycleColors[1].color,  0.2f), toolTip = "Yellow and orange."   },
                    new ModButtonInfo { buttonText = "Frost",     buttonOverlayText = "<color=#00FFFF>F</color><color=#33FFFF>r</color><color=#66FFFF>o</color><color=#99FFFF>s</color><color=#CCFFFF>t</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[4].color,  CycleColors[15].color, 0.2f), toolTip = "Cyan and silver."     },
                    new ModButtonInfo { buttonText = "Steel",     buttonOverlayText = "<color=#C0C0C0>S</color><color=#D0D0D0>t</color><color=#E0E0E0>e</color><color=#F0F0F0>e</color><color=#FFFFFF>l</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[15].color, CycleColors[12].color, 0.2f), toolTip = "Silver and gray."     },
                    new ModButtonInfo { buttonText = "Shadow",    buttonOverlayText = "<color=#000000>S</color><color=#111111>h</color><color=#222222>a</color><color=#333333>d</color><color=#444444>o</color><color=#555555>w</color>",                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[7].color,  CycleColors[12].color, 0.2f), toolTip = "Black and gray."      },
                    new ModButtonInfo { buttonText = "Inferno",   buttonOverlayText = "<color=#FF0000>I</color><color=#FF3300>n</color><color=#FF6600>f</color><color=#FF9900>e</color><color=#FFCC00>r</color><color=#FFFF00>n</color><color=#FFFF33>o</color>",                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[0].color,  CycleColors[2].color,  0.2f), toolTip = "Red and yellow."      },
                    new ModButtonInfo { buttonText = "Berry",     buttonOverlayText = "<color=#FF00FF>B</color><color=#FF33FF>e</color><color=#FF66FF>r</color><color=#FF99FF>r</color><color=#FFCCFF>y</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[9].color,  CycleColors[10].color, 0.2f), toolTip = "Magenta and pink."    },
                    new ModButtonInfo { buttonText = "Midnight",  buttonOverlayText = "<color=#000000>M</color><color=#000033>i</color><color=#000066>d</color><color=#000099>n</color><color=#0000CC>i</color><color=#0000FF>g</color><color=#3333FF>h</color><color=#6666FF>t</color>",                                                             method = () => Extra.Instance.SetTheme(CycleColors[7].color,  CycleColors[5].color,  0.2f), toolTip = "Black and blue."      },
                    new ModButtonInfo { buttonText = "Lava",      buttonOverlayText = "<color=#FF0000>L</color><color=#FF3300>a</color><color=#FF6600>v</color><color=#FF9900>a</color>",                                                                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[0].color,  CycleColors[12].color, 0.2f), toolTip = "Red and gray."        },
                    new ModButtonInfo { buttonText = "Mint",      buttonOverlayText = "<color=#00FF7F>M</color><color=#33FF99>i</color><color=#66FFBB>n</color><color=#99FFDD>t</color>",                                                                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[3].color,  CycleColors[13].color, 0.2f), toolTip = "Green and lime."      },
                    new ModButtonInfo { buttonText = "Peach",     buttonOverlayText = "<color=#FFA07A>P</color><color=#FFB080>e</color><color=#FFC099>a</color><color=#FFD0B3>c</color><color=#FFE0CC>h</color>",                                                                                                                                     method = () => Extra.Instance.SetTheme(CycleColors[1].color,  CycleColors[10].color, 0.2f), toolTip = "Orange and pink."     },
                    new ModButtonInfo { buttonText = "Twilight",  buttonOverlayText = "<color=#8A2BE2>T</color><color=#9B30FF>w</color><color=#A64AC9>i</color><color=#B266FF>l</color><color=#C080FF>i</color><color=#D19EFF>g</color><color=#E0BFFF>h</color><color=#F0DFFF>t</color>",                                                             method = () => Extra.Instance.SetTheme(CycleColors[6].color,  CycleColors[4].color,  0.2f), toolTip = "Purple and cyan."     },
                    new ModButtonInfo { buttonText = "Cobalt",    buttonOverlayText = "<color=#0000FF>C</color><color=#3333FF>o</color><color=#6666FF>b</color><color=#9999FF>a</color><color=#CCCCFF>l</color><color=#FFFFFF>t</color>",                                                                                                             method = () => Extra.Instance.SetTheme(CycleColors[5].color,  CycleColors[14].color, 0.2f), toolTip = "Blue and navy."       },
                };

                Api.Instance.tokenList.Add("4", "Mods");
                Api.Instance.tokenListVisable.Add("4", true);
                Api.Instance.tokenListBackToken.Add("4", "Main");
                Api.Instance.tokenListButtonInfo["4"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Install Mods",   method = () => { SbLoadBrowser("all"); Api.Instance.OpenMenu("11"); }, toolTip = "Browse and install mods from the SB catalog." },
                    new ModButtonInfo { buttonText = "Mod Toggle",     method = () => { RefreshInstalledMods(); Api.Instance.OpenMenu("9"); },  toolTip = "Toggle installed mods on or off."            },
                    new ModButtonInfo { buttonText = "Mod Config",     method = () => Api.Instance.OpenMenu("6"),                              toolTip = "Opens the mod config editor."                 },
                    new ModButtonInfo { buttonText = "Loadouts",       method = () => { RefreshLoadoutsMenu(); Api.Instance.OpenMenu("10"); },  toolTip = "Manage mod loadouts."                         },
                };

                Api.Instance.tokenList.Add("11", "Install Mods");
                Api.Instance.tokenListVisable.Add("11", false);
                Api.Instance.tokenListBackToken.Add("11", "4");
                Api.Instance.tokenListButtonInfo["11"] = new List<ModButtonInfo>();

                Api.Instance.tokenList.Add("6", "Mod Config");
                Api.Instance.tokenListVisable.Add("6", false);
                Api.Instance.tokenListBackToken.Add("6", "4");
                Api.Instance.tokenListButtonInfo["6"] = new List<ModButtonInfo>();

                Api.Instance.tokenList.Add("9", "Mod Toggle");
                Api.Instance.tokenListVisable.Add("9", false);
                Api.Instance.tokenListBackToken.Add("9", "4");
                Api.Instance.tokenListButtonInfo["9"] = new List<ModButtonInfo>();

                Api.Instance.tokenList.Add("10", "Loadouts");
                Api.Instance.tokenListVisable.Add("10", false);
                Api.Instance.tokenListBackToken.Add("10", "4");
                Api.Instance.tokenListButtonInfo["10"] = new List<ModButtonInfo>();

                Api.Instance.tokenList.Add("7", "Credits");
                Api.Instance.tokenListVisable.Add("7", false);
                Api.Instance.tokenListBackToken.Add("7", "1");
                Api.Instance.tokenListButtonInfo["7"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "<color=purple>Sev</color>",    toolTip = "Nearly everything." },
                    new ModButtonInfo { buttonText = "<color=grey>Skellon</color>",  toolTip = "Asset loader."      },
                };

                Api.Instance.tokenList.Add("NotNew", "<color=red>Update</color>");
                Api.Instance.tokenListVisable.Add("NotNew", false);
                Api.Instance.tokenListBackToken.Add("NotNew", "NotNew");
                Api.Instance.tokenListButtonInfo["NotNew"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "<color=red>Please Update</color>", toolTip = "Click 'Update' to update." },
                    new ModButtonInfo { buttonText = "<color=orange>Update</color>", method = () => InstallLatestMod(Config.ModDownload, ModBrowser.Instance.GetModName(Config.ModDownload)), toolTip = "Installs the latest Several Bees." },
                    new ModButtonInfo { buttonText = "<color=yellow>GitHub</color>", method = () =>
                    {
                        string link = Config.ModDownload;
                        int idx = link.IndexOf("/releases/");
                        if (idx != -1) link = link.Substring(0, idx);
                        Process.Start(new ProcessStartInfo { FileName = link, UseShellExecute = true });
                    }, toolTip = "Opens the Several Bees GitHub." },
                };

                Api.Instance.tokenList.Add("8", "General Settings");
                Api.Instance.tokenListVisable.Add("8", false);
                Api.Instance.tokenListBackToken.Add("8", "1");
                var gen = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Sound Effects",  isTogglable = true, enabled = PlayerPrefs.GetInt("SBSoundEffects",  1) == 1, enableMethod = () => PlayerPrefs.SetInt("SBSoundEffects",  1), disableMethod = () => PlayerPrefs.SetInt("SBSoundEffects",  0), toolTip = "Toggles sound effects."            },
                    new ModButtonInfo { buttonText = "Animations",     isTogglable = true, enabled = PlayerPrefs.GetInt("SBAnimations",     1) == 1, enableMethod = () => PlayerPrefs.SetInt("SBAnimations",     1), disableMethod = () => PlayerPrefs.SetInt("SBAnimations",     0), toolTip = "Toggles spawn animations."         },
                    new ModButtonInfo { buttonText = "Restart On Mod", isTogglable = true, enabled = PlayerPrefs.GetInt("SBRestartOnMod",   1) == 1, enableMethod = () => PlayerPrefs.SetInt("SBRestartOnMod",   1), disableMethod = () => PlayerPrefs.SetInt("SBRestartOnMod",   0), toolTip = "Auto-restart on mod changes."      },
                    new ModButtonInfo { buttonText = "Open Gesture",   isTogglable = true, enabled = PlayerPrefs.GetInt("SBOpenGesture",    1) == 1, enableMethod = () => PlayerPrefs.SetInt("SBOpenGesture",    1), disableMethod = () => PlayerPrefs.SetInt("SBOpenGesture",    0), toolTip = "Toggles the hand spawn gesture."   },
                };

                if (Config.CompType == ComputerType.FullMachine)
                    gen.Add(new ModButtonInfo { buttonText = "Physical Back Button", isTogglable = true, enabled = PlayerPrefs.GetInt("SBPhysBackButton", 0) == 1, enableMethod = () => PlayerPrefs.SetInt("SBPhysBackButton", 1), disableMethod = () => PlayerPrefs.SetInt("SBPhysBackButton", 0), toolTip = "Toggles the physical back button." });

                Api.Instance.tokenListButtonInfo["8"] = gen;

                Api.Instance.tokenList.Add("LoadPlugins", "<color=orange>Loading Plugins</color>");
                Api.Instance.tokenListVisable.Add("LoadPlugins", false);
                Api.Instance.tokenListBackToken.Add("LoadPlugins", "LoadPlugins");
                Api.Instance.tokenListButtonInfo["LoadPlugins"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Loading Plugins,", toolTip = "" },
                    new ModButtonInfo { buttonText = "Please wait...",   toolTip = "" },
                };

                Settings.Load();
                Settings.SetButtonNames();
                RefreshConfigEditor();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] MakeSettings error: " + e.Message);
            }
        }

        #endregion

        #region Config Editor

        internal void RefreshConfigEditor()
        {
            configCache.Clear();

            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "<color=red>Refresh</color>", method = RefreshConfigEditor, toolTip = "Refreshes the config list." }
            };

            string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "config");
            if (!Directory.Exists(configFolder)) { Api.Instance.tokenListButtonInfo["6"] = buttons; return; }

            foreach (var file in Directory.GetFiles(configFolder, "*.cfg", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                var entries = ParseConfigFile(file);
                configCache[file] = entries;

                string capturedFile = file;
                string capturedName = name;
                buttons.Add(new ModButtonInfo
                {
                    buttonText = capturedName,
                    toolTip = $"Edit config for {capturedName}.",
                    method = () => OpenConfigPage(capturedName, capturedFile)
                });
            }

            Api.Instance.tokenListButtonInfo["6"] = buttons;
        }

        private List<ConfigEntry> ParseConfigFile(string path)
        {
            var entries = new List<ConfigEntry>();
            string section = "";
            var comments = new List<string>();

            foreach (var line in File.ReadAllLines(path))
            {
                string t = line.Trim();
                if (string.IsNullOrEmpty(t)) continue;

                if (t.StartsWith("[") && t.EndsWith("]"))
                {
                    section = t.Substring(1, t.Length - 2);
                    comments.Clear();
                    continue;
                }

                if (t.StartsWith("#")) { comments.Add(t.TrimStart('#', ' ').Trim()); continue; }

                int eq = t.IndexOf('=');
                if (eq < 0) continue;

                string key = t.Substring(0, eq).Trim();
                string val = t.Substring(eq + 1).Trim();
                string desc = "";
                string acceptable = "";
                string typeName = "";

                foreach (var c in comments)
                {
                    if (c.StartsWith("Acceptable values:", StringComparison.OrdinalIgnoreCase))
                        acceptable = c.Substring("Acceptable values:".Length).Trim();
                    else if (c.StartsWith("Setting type:", StringComparison.OrdinalIgnoreCase))
                        typeName = c.Substring("Setting type:".Length).Trim();
                    else if (!c.StartsWith("Default value:", StringComparison.OrdinalIgnoreCase))
                        desc += c + " ";
                }

                entries.Add(new ConfigEntry
                {
                    Section = section,
                    Key = key,
                    Value = val,
                    Description = desc.Trim(),
                    AcceptableValues = acceptable,
                    TypeName = typeName,
                    FilePath = path
                });

                comments.Clear();
            }

            return entries;
        }

        internal void OpenConfigPage(string modName, string filePath)
        {
            var entries = configCache.ContainsKey(filePath) ? configCache[filePath] : ParseConfigFile(filePath);

            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo
                {
                    buttonText = "<color=red>Refresh</color>",
                    method = () => { configCache.Remove(filePath); OpenConfigPage(modName, filePath); },
                    toolTip = "Reloads config from disk."
                }
            };

            foreach (var entry in entries)
            {
                var e = entry;
                bool isBool = e.TypeName.Equals("Boolean", StringComparison.OrdinalIgnoreCase)
                    || e.Value.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || e.Value.Equals("false", StringComparison.OrdinalIgnoreCase);

                bool isEnum = !isBool && !string.IsNullOrEmpty(e.AcceptableValues) && e.AcceptableValues.Contains(",");

                if (isBool)
                {
                    bool cur = e.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                    string col = cur ? "green" : "red";
                    string tip = string.IsNullOrEmpty(e.Description) ? $"{e.Section} > {e.Key}" : e.Description;

                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = $"{e.Section}/{e.Key}: <color={col}>{e.Value}</color>",
                        toolTip = tip,
                        method = () =>
                        {
                            bool next = !e.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                            WriteConfigValue(e.FilePath, e.Section, e.Key, next.ToString());
                            e.Value = next.ToString();
                            OpenConfigPage(modName, filePath);
                        }
                    });
                    AddConfigResetButton(buttons, e);
                }
                else if (isEnum)
                {
                    var options = e.AcceptableValues.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    int cur = Mathf.Max(0, options.IndexOf(e.Value));
                    string tip = string.IsNullOrEmpty(e.Description)
                        ? $"Options: {e.AcceptableValues}"
                        : $"{e.Description} | Options: {e.AcceptableValues}";

                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = $"{e.Section}/{e.Key}: {e.Value}",
                        toolTip = tip,
                        method = () =>
                        {
                            string next = options[(cur + 1) % options.Count];
                            WriteConfigValue(e.FilePath, e.Section, e.Key, next);
                            e.Value = next;
                            OpenConfigPage(modName, filePath);
                        }
                    });
                    AddConfigResetButton(buttons, e);
                }
            }

            string token = "cfg_" + modName;
            Api.Instance.tokenList[token] = modName;
            if (!Api.Instance.tokenListVisable.ContainsKey(token)) Api.Instance.tokenListVisable[token] = false;
            if (!Api.Instance.tokenListBackToken.ContainsKey(token)) Api.Instance.tokenListBackToken[token] = "6";

            int savedIndex = SectionName == token ? PointerPositionIndex : 0;
            Api.Instance.tokenListButtonInfo[token] = buttons;
            Api.Instance.OpenMenu(token);
            PointerPositionIndex = Mathf.Clamp(savedIndex, 0, buttons.Count - 1);
        }

        private void WriteConfigValue(string filePath, string section, string key, string value)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();
                string cur = "";
                for (int i = 0; i < lines.Count; i++)
                {
                    string t = lines[i].Trim();
                    if (t.StartsWith("[") && t.EndsWith("]")) { cur = t.Substring(1, t.Length - 2); continue; }
                    if (cur != section) continue;
                    int eq = t.IndexOf('=');
                    if (eq < 0) continue;
                    if (t.Substring(0, eq).Trim() != key) continue;
                    lines[i] = key + " = " + value;
                    break;
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[Several Bees] Config write error: " + ex.Message);
            }
        }

        #endregion

        #region Mod Browser

        internal async void CheckModUpdatesAsync()
        {
            if (fetchingModUpdates) return;
            fetchingModUpdates = true;

            await Task.Delay(1000);

            if (!sbModCache.Any())
            {
                fetchingModUpdates = false;
                return;
            }

            var tasks = sbModCache.Select(async mod =>
            {
                string repoUrl = mod.RepoUrl;
                try
                {
                    string latest = await ModBrowser.Instance.GetGitHubTagAsync(repoUrl + "/releases/latest/download/" + mod.DllName);
                    string saved = PlayerPrefs.GetString("SBModVer_" + repoUrl, "");
                    return new KeyValuePair<string, bool>(repoUrl, !string.IsNullOrEmpty(saved) && saved != latest);
                }
                catch { return new KeyValuePair<string, bool>(repoUrl, false); }
            }).ToList();

            var results = await Task.WhenAll(tasks);
            modUpdateAvailable.Clear();
            foreach (var r in results)
                if (r.Value) modUpdateAvailable.Add(r.Key);

            fetchingModUpdates = false;

            if (modUpdateAvailable.Count > 0)
                SbRebuildBrowserButtons();
        }

        internal void RefreshInstalledMods()
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "<color=red>Refresh</color>", toolTip = "Refreshes the mod list.", method = RefreshInstalledMods }
            };

            if (!Directory.Exists(pluginsPath))
            {
                buttons.Add(new ModButtonInfo { buttonText = "<color=grey>No plugins folder</color>", toolTip = "BepInEx plugins folder not found." });
                Api.Instance.tokenListButtonInfo["9"] = buttons;
                return;
            }

            var dlls = Directory.GetFiles(pluginsPath, "*.dll").Select(f => new InstalledMod { Name = Path.GetFileNameWithoutExtension(f), Path = f, OnDisk = true });
            var disabled = Directory.GetFiles(pluginsPath, "*.dll.disabled").Select(f => new InstalledMod { Name = Path.GetFileNameWithoutExtension(f).Replace(".dll", ""), Path = f, OnDisk = false });
            var all = dlls.Concat(disabled).OrderBy(m => m.Name).ToList();

            if (all.Count == 0)
            {
                buttons.Add(new ModButtonInfo { buttonText = "<color=grey>No mods installed</color>", toolTip = "Install mods via Mod Browser." });
                Api.Instance.tokenListButtonInfo["9"] = buttons;
                return;
            }

            foreach (var mod in all)
            {
                var guid = BepInEx.Bootstrap.Chainloader.PluginInfos.Keys.FirstOrDefault(k =>
                    string.Equals(k, mod.Name, StringComparison.OrdinalIgnoreCase) ||
                    BepInEx.Bootstrap.Chainloader.PluginInfos[k].Metadata.Name.Equals(mod.Name, StringComparison.OrdinalIgnoreCase));

                if (guid != null)
                {
                    BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(guid, out var info);
                    mod.PluginInfo = info;
                    mod.LiveInstance = info?.Instance;
                }

                if (mod.LiveInstance != null)
                {
                    var prop = mod.LiveInstance.GetType().GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null) mod.LiveEnabled = (bool?)prop.GetValue(mod.LiveInstance);
                }
            }

            foreach (var mod in all)
            {
                var m = mod;
                bool effectivelyOn = m.LiveEnabled ?? m.OnDisk;
                bool isLive = m.LiveInstance != null;
                bool supportsLive = m.LiveEnabled != null;

                string stateCol = effectivelyOn ? "green" : "red";
                string stateText = effectivelyOn ? "ON" : "OFF";
                string suffix = isLive && !supportsLive ? " <color=grey>(?)</color>" : "";

                string tip = isLive
                    ? (supportsLive ? $"{m.Name} supports live toggling. Currently {stateText}." : $"{m.Name} is loaded but has no Enabled property — will use disk toggle + restart.")
                    : (effectivelyOn ? $"{m.Name} is enabled on disk." : $"{m.Name} is disabled. Select to re-enable.");

                buttons.Add(new ModButtonInfo
                {
                    buttonText = $"<color={stateCol}>[{stateText}]</color> {m.Name}{suffix}",
                    toolTip = tip,
                    method = () => ToggleInstalledMod(m, !effectivelyOn)
                });
            }

            Api.Instance.tokenListButtonInfo["9"] = buttons;
        }

        private void ToggleInstalledMod(InstalledMod mod, bool enable)
        {
            bool handledLive = false;

            if (mod.LiveInstance != null)
            {
                var type = mod.LiveInstance.GetType();

                var prop = type.GetProperty("Enabled", BindingFlags.Public | BindingFlags.Instance);
                if (prop?.CanWrite == true)
                {
                    try { prop.SetValue(mod.LiveInstance, enable); handledLive = true; }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Enabled prop: " + ex.Message); }
                }

                if (!handledLive)
                {
                    var method = type.GetMethod(enable ? "Enable" : "Disable", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        try { method.Invoke(mod.LiveInstance, null); handledLive = true; }
                        catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Enable/Disable method: " + ex.Message); }
                    }
                }

                if (!handledLive)
                {
                    mod.LiveInstance.gameObject.SetActive(enable);
                    handledLive = true;
                }
            }

            try
            {
                if (enable && !mod.OnDisk)
                {
                    string dest = mod.Path.Replace(".dll.disabled", ".dll");
                    if (File.Exists(dest)) File.Delete(dest);
                    File.Move(mod.Path, dest);
                }
                else if (!enable && mod.OnDisk)
                {
                    string dest = mod.Path + ".disabled";
                    if (File.Exists(dest)) File.Delete(dest);
                    File.Move(mod.Path, dest);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[Several Bees] Disk toggle: " + ex.Message);
            }

            if (!handledLive && Api.Instance.GrabButton("8", "Restart On Mod").enabled)
                RestartApp();

            RefreshInstalledMods();
        }

        #endregion

        #region Mod Install / Toggle / Uninstall

        internal void ToggleMod(string modName, string modLink, bool enable)
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", modName + ".dll");
            string disabledPath = dllPath + ".disabled";

            try
            {
                if (enable && File.Exists(disabledPath))
                {
                    SetToolTip($"<color=orange>Enabling {modName}...</color>");
                    if (File.Exists(dllPath)) File.Delete(dllPath);
                    File.Move(disabledPath, dllPath);
                    SetToolTip($"<color=green>{modName} enabled. Restart to apply.</color>");
                }
                else if (!enable && File.Exists(dllPath))
                {
                    SetToolTip($"<color=orange>Disabling {modName}...</color>");
                    if (File.Exists(disabledPath)) File.Delete(disabledPath);
                    File.Move(dllPath, disabledPath);
                    SetToolTip($"<color=yellow>{modName} disabled. Restart to apply.</color>");
                }

                var mod = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll");
                if (mod != null) SbOpenModPage(mod);

                if (Api.Instance.GrabButton("8", "Restart On Mod").enabled) RestartApp();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[Several Bees] Toggle error: " + ex.Message);
                SetToolTip($"<color=red>Toggle failed: {ex.Message}</color>");
            }
        }

        internal async void InstallMod(string modLink, string modName)
        {
            UnityEngine.Debug.Log($"[Several Bees] InstallMod — link: {modLink} | name: {modName}");

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);
            string tempFile = Path.Combine(Path.GetTempPath(), modName + ".dll");

            SetToolTip($"<color=orange>Starting download for {modName}...</color>");

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                        SetToolTip($"<color=orange>Downloading {modName}... {e.ProgressPercentage}%</color>");
                    await client.DownloadFileTaskAsync(new Uri(modLink), tempFile);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] InstallMod download failed: {ex.Message}");
                SetToolTip($"<color=red>Download failed: {ex.Message}</color>");
                return;
            }

            SetToolTip($"<color=orange>Download complete. Installing {modName}...</color>");

            try
            {
                string dest = Path.Combine(pluginsPath, modName + ".dll");
                if (File.Exists(dest)) File.Delete(dest);
                File.Move(tempFile, dest);
                UnityEngine.Debug.Log($"[Several Bees] Installed to: {dest}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] InstallMod file move failed: {ex.Message}");
                SetToolTip($"<color=red>Install failed: {ex.Message}</color>");
                return;
            }

            string repoUrl = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll")?.RepoUrl ?? modLink;
            modUpdateAvailable.Remove(repoUrl);
            SbSaveVersionAsync(repoUrl, modLink);

            SetToolTip($"<color=green>{modName} installed!</color>");
            var installedMod = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll");
            if (installedMod != null) SbOpenModPage(installedMod);

            if (!Api.Instance.GrabButton("8", "Restart On Mod").enabled) return;
            SetToolTip($"<color=green>{modName} installed! Restarting...</color>");
            RestartApp();
        }

        internal async void InstallModAndInject(string modLink, string modName)
        {
            UnityEngine.Debug.Log($"[Several Bees] InstallModAndInject — link: {modLink} | name: {modName}");

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);
            string dllPath = Path.Combine(pluginsPath, modName + ".dll");
            if (File.Exists(dllPath)) File.Delete(dllPath);

            SetToolTip($"<color=orange>Starting download for {modName}...</color>");

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                        SetToolTip($"<color=orange>Downloading {modName}... {e.ProgressPercentage}%</color>");
                    await client.DownloadFileTaskAsync(new Uri(modLink), dllPath);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] InstallModAndInject download failed: {ex.Message}");
                SetToolTip($"<color=red>Download failed: {ex.Message}</color>");
                return;
            }

            SetToolTip($"<color=orange>Download done. Injecting {modName}...</color>");
            UnityEngine.Debug.Log($"[Several Bees] Injecting: {dllPath}");

            try
            {
                TryInjectAssembly(dllPath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] Inject failed: {ex.Message}");
                SetToolTip($"<color=red>Inject failed: {ex.Message}</color>");
                return;
            }

            string repoUrl = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll")?.RepoUrl ?? modLink;
            modUpdateAvailable.Remove(repoUrl);
            SbSaveVersionAsync(repoUrl, modLink);

            SetToolTip($"<color=green>{modName} installed and loaded!</color>");
            UnityEngine.Debug.Log($"[Several Bees] InstallModAndInject complete: {dllPath}");

            var installedMod = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll");
            if (installedMod != null) SbOpenModPage(installedMod);
        }

        internal void UninstallMod(string modName, string modLink)
        {
            string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            string dllPath = Path.Combine(basePath, modName + ".dll");
            string disabledPath = dllPath + ".disabled";

            string target = File.Exists(dllPath) ? dllPath : File.Exists(disabledPath) ? disabledPath : null;
            if (target == null) { SetToolTip($"<color=red>{modName} not found on disk.</color>"); return; }

            SetToolTip($"<color=orange>Uninstalling {modName}...</color>");

            try
            {
                string deletePath = target + ".delete";
                if (File.Exists(deletePath)) File.Delete(deletePath);
                File.Move(target, deletePath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] Uninstall failed: {ex.Message}");
                SetToolTip($"<color=red>Uninstall failed: {ex.Message}</color>");
                return;
            }

            string repoUrl = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll")?.RepoUrl ?? modLink;
            PlayerPrefs.DeleteKey("SBModVer_" + repoUrl);
            modUpdateAvailable.Remove(repoUrl);

            SetToolTip($"<color=green>{modName} uninstalled.</color>");
            var uninstalledMod = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll");
            if (uninstalledMod != null) SbOpenModPage(uninstalledMod);

            if (!Api.Instance.GrabButton("8", "Restart On Mod").enabled) return;
            SetToolTip($"<color=green>{modName} uninstalled. Restarting...</color>");
            RestartApp();
        }

        internal async void InstallLatestMod(string modLink, string modName)
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            string dllPath = Path.Combine(pluginsPath, modName + ".dll");

            if (!File.Exists(dllPath)) { SetToolTip($"<color=red>{modName}.dll not found.</color>"); return; }

            SetToolTip($"<color=orange>Preparing update for {modName}...</color>");

            string deletePath = dllPath + ".delete";
            if (File.Exists(deletePath)) File.Delete(deletePath);
            File.Move(dllPath, deletePath);

            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);
            string tempFile = Path.Combine(Path.GetTempPath(), modName + ".dll");

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                        SetToolTip($"<color=orange>Downloading update for {modName}... {e.ProgressPercentage}%</color>");
                    await client.DownloadFileTaskAsync(new Uri(modLink), tempFile);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] InstallLatestMod download failed: {ex.Message}");
                SetToolTip($"<color=red>Update download failed: {ex.Message}</color>");
                if (File.Exists(deletePath)) File.Move(deletePath, dllPath);
                return;
            }

            SetToolTip($"<color=orange>Download complete. Installing update for {modName}...</color>");

            try
            {
                string dest = Path.Combine(pluginsPath, modName + ".dll");
                if (File.Exists(dest)) File.Delete(dest);
                File.Move(tempFile, dest);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[Several Bees] InstallLatestMod file move failed: {ex.Message}");
                SetToolTip($"<color=red>Update install failed: {ex.Message}</color>");
                if (File.Exists(deletePath)) File.Move(deletePath, dllPath);
                return;
            }

            string repoUrl = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll")?.RepoUrl ?? modLink;
            modUpdateAvailable.Remove(repoUrl);
            SbSaveVersionAsync(repoUrl, modLink);

            SetToolTip($"<color=green>{modName} updated!</color>");
            var updatedMod = sbModCache.FirstOrDefault(m => m.DllName == modName + ".dll");
            if (updatedMod != null) SbOpenModPage(updatedMod);

            if (!Api.Instance.GrabButton("8", "Restart On Mod").enabled) return;
            SetToolTip($"<color=green>{modName} updated! Restarting...</color>");
            RestartApp();
        }

        private async void SbSaveVersionAsync(string repoUrl, string downloadLink)
        {
            try
            {
                string tag = await ModBrowser.Instance.GetGitHubTagAsync(downloadLink);
                if (!string.IsNullOrEmpty(tag))
                    PlayerPrefs.SetString("SBModVer_" + repoUrl, tag);
            }
            catch { }
        }

        #endregion

        #region SB Platform

        private async Task<string> SbPost(string action, Dictionary<string, string> fields)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var data = new System.Collections.Specialized.NameValueCollection();
                    data["action"] = action;
                    foreach (var kv in fields) data[kv.Key] = kv.Value;
                    if (!string.IsNullOrEmpty(sbToken)) data["token"] = sbToken;
                    var result = await client.UploadValuesTaskAsync(new Uri(SbApiBase), "POST", data);
                    return System.Text.Encoding.UTF8.GetString(result);
                }
            }
            catch { return null; }
        }

        private async Task<string> SbGet(string action, Dictionary<string, string> query = null)
        {
            try
            {
                var url = SbApiBase + "?action=" + Uri.EscapeDataString(action);
                if (query != null)
                    foreach (var kv in query) url += "&" + Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value);
                if (!string.IsNullOrEmpty(sbToken)) url += "&token=" + Uri.EscapeDataString(sbToken);
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "SeveralBees");
                    return await client.DownloadStringTaskAsync(url);
                }
            }
            catch { return null; }
        }

        private string SbJsonString(string json, string key)
        {
            string search = $"\"{key}\":\"";
            int idx = json?.IndexOf(search) ?? -1;
            if (idx < 0) return null;
            int start = idx + search.Length;
            int end = start;
            while (end < json.Length && json[end] != '"') { if (json[end] == '\\') end++; end++; }
            return json.Substring(start, end - start)
                .Replace("\\/", "/")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "")
                .Replace("\\t", "\t");
        }

        private bool SbJsonBool(string json, string key)
        {
            string search = $"\"{key}\":";
            int idx = json?.IndexOf(search) ?? -1;
            if (idx < 0) return false;
            int start = idx + search.Length;
            return json.Substring(start).TrimStart().StartsWith("true");
        }

        private int SbJsonInt(string json, string key)
        {
            string search = $"\"{key}\":";
            int idx = json?.IndexOf(search) ?? -1;
            if (idx < 0) return 0;
            int start = idx + search.Length;
            int end = start;
            string tail = json.Substring(start).TrimStart();
            int i = 0;
            while (i < tail.Length && (char.IsDigit(tail[i]) || tail[i] == '-')) i++;
            int.TryParse(tail.Substring(0, i), out int val);
            return val;
        }

        private List<SbMod> SbParseModList(string json)
        {
            var result = new List<SbMod>();
            if (string.IsNullOrEmpty(json)) return result;
            int modsStart = json.IndexOf("\"mods\":[");
            if (modsStart >= 0) json = json.Substring(modsStart + 8);

            int depth = 0; int objStart = -1;
            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth == 0) objStart = i; depth++; }
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0 && objStart >= 0)
                    {
                        string obj = json.Substring(objStart, i - objStart + 1);
                        result.Add(new SbMod
                        {
                            Id = SbJsonInt(obj, "id"),
                            Name = SbJsonString(obj, "name") ?? "",
                            DllName = SbJsonString(obj, "dll_name") ?? "",
                            RepoUrl = SbJsonString(obj, "repo_url") ?? "",
                            Description = SbJsonString(obj, "description") ?? "",
                            ImageUrl = SbJsonString(obj, "image_url") ?? "",
                            Author = SbJsonString(obj, "author_username") ?? "",
                            Upvotes = SbJsonInt(obj, "upvotes"),
                            IsVerified = SbJsonInt(obj, "is_verified") == 1,
                            IsFeatured = SbJsonInt(obj, "is_featured") == 1,
                        });
                        objStart = -1;
                    }
                }
            }
            return result;
        }

        internal async void SbLoadBrowser(string tab)
        {
            sbCurrentBrowserTab = tab;
            sbModDisplayCount = sbModPageSize;
            sbToken = PlayerPrefs.GetString("GggUserToken", null);

            Api.Instance.tokenListButtonInfo["11"] = new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "<color=orange>Loading...</color>", toolTip = "Fetching mod catalog." }
            };

            string gameSlug = UnityEngine.Application.productName.Replace(" ", "").ToLowerInvariant();
            string gameJson = await SbGet("register_game", new Dictionary<string, string> { ["slug"] = gameSlug });
            int gameId = gameJson != null ? SbJsonInt(gameJson, "id") : 0;

            var queryParams = new Dictionary<string, string> { ["tab"] = tab, ["page"] = "1" };
            if (gameId > 0) queryParams["game_id"] = gameId.ToString();

            string json = await SbGet("list_mods", queryParams);
            var raw = SbParseModList(json);

            sbModCache = raw
                .OrderByDescending(m => m.IsFeatured ? 2 : m.IsVerified ? 1 : 0)
                .ThenByDescending(m => m.Upvotes)
                .ThenBy(m => m.Name)
                .ToList();

            if (!string.IsNullOrEmpty(sbToken))
                await SbRefreshUpvotes();

            SbRebuildBrowserButtons();
        }

        private void SbRebuildBrowserButtons()
        {
            var tabs = new[] { "all", "featured", "verified", "unverified" };
            int curIdx = Array.IndexOf(tabs, sbCurrentBrowserTab);
            int nextIdx = (curIdx + 1) % tabs.Length;

            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo
                {
                    buttonText = $"<color=grey>Tab: </color>{sbCurrentBrowserTab}",
                    toolTip = "Cycle through All, Featured, Verified, Unverified.",
                    method = () => SbLoadBrowser(tabs[(Array.IndexOf(tabs, sbCurrentBrowserTab) + 1) % tabs.Length])
                },
                new ModButtonInfo
                {
                    buttonText = string.IsNullOrEmpty(sbToken) ? "<color=grey>Click Me To Login</color>" : $"<color=green>@{sbUsername ?? "Logged In"}</color>",
                    toolTip = string.IsNullOrEmpty(sbToken) ? "Login with GGGravity to upvote mods." : "Click to log out.",
                    method = () =>
                    {
                        if (string.IsNullOrEmpty(sbToken)) SbStartLogin();
                        else
                        {
                            sbToken = null;
                            sbUsername = null;
                            PlayerPrefs.DeleteKey("GggUserToken");
                            sbUpvotedMods.Clear();
                            SetToolTip("<color=grey>Logged out.</color>");
                            SbRebuildBrowserButtons();
                        }
                    }
                },
                new ModButtonInfo
                {
                    buttonText = "Open Dashboard",
                    toolTip = "Opens the Several Bees mod dashboard in your browser.",
                    method = () => Process.Start(new ProcessStartInfo { FileName = "https://sevvy-wevvy.com/mods/sb/dashboard/", UseShellExecute = true })
                },
                new ModButtonInfo { buttonText = "<color=red>Refresh</color>", toolTip = "Reloads the catalog.", method = () => SbLoadBrowser(sbCurrentBrowserTab) }
            };

            if (!sbModCache.Any())
            {
                buttons.Add(new ModButtonInfo { buttonText = "<color=grey>No mods found.</color>", toolTip = "" });
            }
            else
            {
                int count = Mathf.Min(sbModDisplayCount, sbModCache.Count);
                for (int i = 0; i < count; i++)
                {
                    var m = sbModCache[i];
                    bool upvoted = sbUpvotedMods.Contains(m.Id);
                    bool needsUpdate = modUpdateAvailable.Contains(m.RepoUrl);
                    string label = m.IsFeatured ? $"<color=yellow>[Featured]</color> {m.Name}"
                                 : m.IsVerified ? $"<color=green>[Verified]</color> {m.Name}"
                                                 : $"<color=grey>[Unverified]</color> {m.Name}";
                    if (upvoted) label += " <color=purple>▲</color>";
                    if (needsUpdate) label += " <color=red>(!)</color>";
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = label,
                        toolTip = $"{m.Name} by @{m.Author} · ▲{m.Upvotes}" + (m.IsVerified ? " · Verified" : " · Unverified") + (needsUpdate ? " · Update available" : ""),
                        method = () => SbOpenModPage(m)
                    });
                }

                if (sbModDisplayCount < sbModCache.Count)
                {
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = $"<color=grey>Load More ({sbModCache.Count - sbModDisplayCount} remaining)</color>",
                        toolTip = "Load 15 more mods.",
                        method = () =>
                        {
                            sbModDisplayCount += sbModPageSize;
                            SbRebuildBrowserButtons();
                        }
                    });
                }
            }

            Api.Instance.tokenListButtonInfo["11"] = buttons;
        }

        private async Task SbRefreshUpvotes()
        {
            sbUpvotedMods.Clear();
            foreach (var mod in sbModCache)
            {
                string res = await SbGet("get_user_upvote", new Dictionary<string, string> { ["mod_id"] = mod.Id.ToString() });
                if (res != null && SbJsonBool(res, "upvoted")) sbUpvotedMods.Add(mod.Id);
            }
        }

        internal void SbOpenModPage(SbMod mod)
        {
            string token = $"sbmod_{mod.Id}";
            if (!Api.Instance.tokenListVisable.ContainsKey(token)) Api.Instance.tokenListVisable[token] = false;
            if (!Api.Instance.tokenListBackToken.ContainsKey(token)) Api.Instance.tokenListBackToken[token] = "11";

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            string dllPath = Path.Combine(pluginsPath, mod.DllName);
            string disabledPath = dllPath + ".disabled";
            bool installed = File.Exists(dllPath);
            bool disabled = !installed && File.Exists(disabledPath);
            bool upvoted = sbUpvotedMods.Contains(mod.Id);

            var buttons = new List<ModButtonInfo>();

            if (!mod.IsVerified)
                buttons.Add(new ModButtonInfo { buttonText = "<color=yellow>[Unverified]</color> Install at your own risk.", toolTip = "This mod has not been reviewed." });

            if (!string.IsNullOrEmpty(mod.Description))
                buttons.Add(new ModButtonInfo { buttonText = "<color=grey>About</color>", toolTip = mod.Description });

            buttons.Add(new ModButtonInfo
            {
                buttonText = upvoted ? "<color=purple>▲ Upvoted</color>" : "▲ Upvote",
                toolTip = $"{mod.Upvotes} upvotes. {(string.IsNullOrEmpty(sbToken) ? "Login to upvote." : "Click to toggle your upvote.")}",
                method = () => SbToggleUpvote(mod)
            });

            if (installed)
            {
                buttons.Add(new ModButtonInfo { buttonText = "<color=yellow>Disable</color>", toolTip = $"Disables {mod.Name}.", method = () => ToggleMod(mod.DllName.Replace(".dll", ""), mod.RepoUrl, false) });
                buttons.Add(new ModButtonInfo { buttonText = "<color=orange>Install Latest</color>", toolTip = $"Updates {mod.Name}.", method = () => InstallLatestMod(mod.RepoUrl + "/releases/latest/download/" + mod.DllName, mod.DllName.Replace(".dll", "")) });
                buttons.Add(new ModButtonInfo { buttonText = "<color=red>Uninstall</color>", toolTip = $"Removes {mod.Name}.", method = () => UninstallMod(mod.DllName.Replace(".dll", ""), mod.RepoUrl) });
            }
            else if (disabled)
            {
                buttons.Add(new ModButtonInfo { buttonText = "<color=green>Enable</color>", toolTip = $"Re-enables {mod.Name}.", method = () => ToggleMod(mod.DllName.Replace(".dll", ""), mod.RepoUrl, true) });
                buttons.Add(new ModButtonInfo { buttonText = "<color=red>Uninstall</color>", toolTip = $"Removes disabled {mod.Name}.", method = () => UninstallMod(mod.DllName.Replace(".dll", ""), mod.RepoUrl) });
            }
            else
            {
                if (mod.IsVerified)
                {
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = "<color=green>Install</color>",
                        toolTip = $"Downloads and installs {mod.Name}. Requires restart.",
                        method = () =>
                        {
                            string dlUrl = mod.RepoUrl.TrimEnd('/') + "/releases/latest/download/" + mod.DllName;
                            UnityEngine.Debug.Log($"[Several Bees] Install button clicked — RepoUrl: {mod.RepoUrl} | DllName: {mod.DllName} | Built URL: {dlUrl}");
                            InstallMod(dlUrl, mod.DllName.Replace(".dll", ""));
                        }
                    });
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = "<color=green>Install & Inject</color>",
                        toolTip = $"Downloads, installs, and hot-loads {mod.Name} without restarting.",
                        method = () =>
                        {
                            string dlUrl = mod.RepoUrl.TrimEnd('/') + "/releases/latest/download/" + mod.DllName;
                            UnityEngine.Debug.Log($"[Several Bees] Install & Inject button clicked — RepoUrl: {mod.RepoUrl} | DllName: {mod.DllName} | Built URL: {dlUrl}");
                            InstallModAndInject(dlUrl, mod.DllName.Replace(".dll", ""));
                        }
                    });
                }
                else
                {
                    bool[] confirmed = { false };
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = "<color=yellow>Install (Unverified)</color>",
                        toolTip = $"{mod.Name} is unverified. Select again to confirm install.",
                        method = () =>
                        {
                            if (!confirmed[0])
                            {
                                confirmed[0] = true;
                                SetToolTip($"<color=yellow>[Unverified]</color> Select Install again to confirm.");
                            }
                            else
                            {
                                string dlUrl = mod.RepoUrl.TrimEnd('/') + "/releases/latest/download/" + mod.DllName;
                                UnityEngine.Debug.Log($"[Several Bees] Install (unverified confirmed) — RepoUrl: {mod.RepoUrl} | DllName: {mod.DllName} | Built URL: {dlUrl}");
                                InstallMod(dlUrl, mod.DllName.Replace(".dll", ""));
                            }
                        }
                    });
                    buttons.Add(new ModButtonInfo
                    {
                        buttonText = "<color=yellow>Install & Inject (Unverified)</color>",
                        toolTip = $"{mod.Name} is unverified. Select again to confirm hot-load.",
                        method = () =>
                        {
                            if (!confirmed[0])
                            {
                                confirmed[0] = true;
                                SetToolTip($"<color=yellow>[Unverified]</color> Select Install & Inject again to confirm.");
                            }
                            else
                            {
                                string dlUrl = mod.RepoUrl.TrimEnd('/') + "/releases/latest/download/" + mod.DllName;
                                UnityEngine.Debug.Log($"[Several Bees] Install & Inject (unverified confirmed) — RepoUrl: {mod.RepoUrl} | DllName: {mod.DllName} | Built URL: {dlUrl}");
                                InstallModAndInject(dlUrl, mod.DllName.Replace(".dll", ""));
                            }
                        }
                    });
                }
            }

            buttons.Add(new ModButtonInfo
            {
                buttonText = "Open GitHub",
                toolTip = $"Opens {mod.Name} on GitHub.",
                method = () => Process.Start(new ProcessStartInfo { FileName = mod.RepoUrl, UseShellExecute = true })
            });

            Api.Instance.tokenList[token] = $"SB: {mod.Name}";
            Api.Instance.tokenListButtonInfo[token] = buttons;
            Api.Instance.OpenMenu(token);
        }

        internal async void SbToggleUpvote(SbMod mod)
        {
            if (string.IsNullOrEmpty(sbToken)) { SbStartLogin(); return; }
            string res = await SbPost("toggle_upvote", new Dictionary<string, string> { ["mod_id"] = mod.Id.ToString() });
            if (res == null) { SetToolTip("<color=red>Failed to upvote.</color>"); return; }
            bool nowUpvoted = SbJsonBool(res, "upvoted");
            if (nowUpvoted) { sbUpvotedMods.Add(mod.Id); mod.Upvotes++; }
            else { sbUpvotedMods.Remove(mod.Id); mod.Upvotes = Math.Max(0, mod.Upvotes - 1); }
            SetToolTip(nowUpvoted ? $"<color=purple>▲ Upvoted {mod.Name}!</color>" : $"Removed upvote from {mod.Name}.");
            SbOpenModPage(mod);
        }

        internal async void SbStartLogin()
        {
            if (sbLoginPending) { SetToolTip($"<color=orange>Code: {sbLoginCode ?? "..."}</color> — visit 3gv.org/link"); return; }
            sbLoginPending = true;

            Api.Instance.tokenListButtonInfo["11"][1] = new ModButtonInfo
            {
                buttonText = "<color=grey>Click Me To Login</color>",
                toolTip = "Generating login code...",
                method = () => { if (sbLoginPending && !string.IsNullOrEmpty(sbLoginCode)) Process.Start(new ProcessStartInfo { FileName = "https://3gv.org/link?code=" + Uri.EscapeDataString(sbLoginCode), UseShellExecute = true }); }
            };

            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "SeveralBees");
                    string json = await client.DownloadStringTaskAsync("https://3gv.org/link/?action=generate");
                    sbLoginCode = SbJsonString(json, "code");
                }
            }
            catch { sbLoginCode = null; }

            if (string.IsNullOrEmpty(sbLoginCode))
            {
                sbLoginPending = false;
                SetToolTip("<color=red>Failed to generate login code.</color>");
                Api.Instance.tokenListButtonInfo["11"][1] = new ModButtonInfo
                {
                    buttonText = "<color=grey>Click Me To Login</color>",
                    toolTip = "Login with GGGravity to upvote mods.",
                    method = () => SbStartLogin()
                };
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = "https://3gv.org/link?code=" + Uri.EscapeDataString(sbLoginCode), UseShellExecute = true });
            SetToolTip($"<color=orange>Code: {sbLoginCode}</color> — visit 3gv.org/link and enter this code.");
            Api.Instance.tokenListButtonInfo["11"][1] = new ModButtonInfo
            {
                buttonText = "<color=grey>Click Me To Login</color>",
                toolTip = $"Code: {sbLoginCode} — visit 3gv.org/link and enter this code. Click to reopen.",
                method = () => Process.Start(new ProcessStartInfo { FileName = "https://3gv.org/link?code=" + Uri.EscapeDataString(sbLoginCode), UseShellExecute = true })
            };

            _ = SbPollLogin(sbLoginCode);
        }

        private async Task SbPollLogin(string code)
        {
            for (int attempt = 0; attempt < 60; attempt++)
            {
                await Task.Delay(3000);
                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        client.Headers.Add("User-Agent", "SeveralBees");
                        string json = await client.DownloadStringTaskAsync($"https://3gv.org/link/?action=status&code={Uri.EscapeDataString(code)}");
                        string token = SbJsonString(json, "token");
                        if (!string.IsNullOrEmpty(token))
                        {
                            string valJson = await SbPost("validate_token", new Dictionary<string, string> { ["token"] = token });
                            string username = SbJsonString(valJson ?? "", "username");
                            if (!string.IsNullOrEmpty(username))
                            {
                                sbToken = token;
                                sbUsername = username;
                                PlayerPrefs.SetString("GggUserToken", token);
                                sbLoginPending = false;
                                sbLoginCode = null;
                                SetToolTip($"<color=green>Logged in as @{username}!</color>");
                                SbLoadBrowser(sbCurrentBrowserTab);
                                return;
                            }
                        }
                    }
                }
                catch { }
            }
            sbLoginPending = false;
            sbLoginCode = null;
            SetToolTip("<color=red>Login timed out. Try again.</color>");
            if (Api.Instance.tokenListButtonInfo.ContainsKey("11") && Api.Instance.tokenListButtonInfo["11"].Count > 1)
            {
                Api.Instance.tokenListButtonInfo["11"][1] = new ModButtonInfo
                {
                    buttonText = "<color=grey>Click Me To Login</color>",
                    toolTip = "Login with GGGravity to upvote mods.",
                    method = () => SbStartLogin()
                };
            }
        }

        internal async void SbValidateStoredToken()
        {
            string token = PlayerPrefs.GetString("GggUserToken", null);
            if (string.IsNullOrEmpty(token)) return;
            sbToken = token;
            string res = await SbPost("validate_token", new Dictionary<string, string> { ["token"] = token });
            if (res != null && SbJsonBool(res, "valid"))
                sbUsername = SbJsonString(res, "username");
            else
            {
                sbToken = null;
                sbUsername = null;
            }
        }

        internal async void SbValidateInstalledMods()
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) return;

            var issues = new List<string>();
            foreach (var file in Directory.GetFiles(pluginsPath, "*.dll"))
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.Length == 0) { issues.Add(Path.GetFileName(file) + " (empty)"); continue; }
                    Assembly.Load(File.ReadAllBytes(file));
                }
                catch
                {
                    issues.Add(Path.GetFileName(file) + " (corrupt)");
                }
            }

            if (issues.Count == 0) return;

            string token = "sb_validation";
            if (!Api.Instance.tokenListVisable.ContainsKey(token)) Api.Instance.tokenListVisable[token] = true;
            if (!Api.Instance.tokenListBackToken.ContainsKey(token)) Api.Instance.tokenListBackToken[token] = "4";
            Api.Instance.tokenList[token] = "<color=red>⚠ Mod Issues</color>";
            Api.Instance.tokenListButtonInfo[token] = issues
                .Select(i => new ModButtonInfo { buttonText = $"<color=red>⚠</color> {i}", toolTip = "This file may cause a crash. Consider removing it." })
                .ToList();
        }

        internal async void SbBatchUpdateAll()
        {
            if (!modUpdateAvailable.Any()) { SetToolTip("<color=green>All mods are up to date.</color>"); return; }
            SetToolTip($"<color=orange>Updating {modUpdateAvailable.Count} mods...</color>");

            var toUpdate = new List<SbMod>(sbModCache.Where(m => modUpdateAvailable.Contains(m.RepoUrl)));
            foreach (var mod in toUpdate)
            {
                string dlUrl = mod.RepoUrl.TrimEnd('/') + "/releases/latest/download/" + mod.DllName;
                InstallLatestMod(dlUrl, mod.DllName.Replace(".dll", ""));
                await Task.Delay(500);
            }

            SetToolTip("<color=green>Batch update complete. Restart to apply.</color>");
        }

        #endregion

        #region Config Reset

        private void AddConfigResetButton(List<ModButtonInfo> buttons, ConfigEntry entry)
        {
            string defVal = null;
            try
            {
                foreach (var line in File.ReadAllLines(entry.FilePath))
                {
                    string t = line.Trim();
                    if (t.StartsWith("# Default value:", StringComparison.OrdinalIgnoreCase))
                    {
                        defVal = t.Substring("# Default value:".Length).Trim();
                        break;
                    }
                }
            }
            catch { }

            if (defVal == null) return;

            var e = entry;
            string captured = defVal;
            buttons.Add(new ModButtonInfo
            {
                buttonText = "<color=grey>↺ Reset Default</color>",
                toolTip = $"Resets '{e.Key}' to: {captured}",
                method = () =>
                {
                    WriteConfigValue(e.FilePath, e.Section, e.Key, captured);
                    e.Value = captured;
                    OpenConfigPage(Path.GetFileNameWithoutExtension(e.FilePath), e.FilePath);
                }
            });
        }

        #endregion

        #region Loadouts

        private string LoadoutsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "config", "sb_loadouts.json");

        private List<ModLoadout> ReadLoadouts()
        {
            try
            {
                if (!File.Exists(LoadoutsFilePath)) return new List<ModLoadout>();
                return ParseLoadoutsJson(File.ReadAllText(LoadoutsFilePath));
            }
            catch { return new List<ModLoadout>(); }
        }

        private void WriteLoadouts(List<ModLoadout> loadouts)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("[");
                for (int i = 0; i < loadouts.Count; i++)
                {
                    var l = loadouts[i];
                    sb.Append("{");
                    sb.Append($"\"Number\":{l.Number},");
                    sb.Append($"\"EnabledMods\":[{string.Join(",", l.EnabledMods.Select(m => $"\"{EscapeJson(m)}\""))}],");
                    sb.Append($"\"DisabledMods\":[{string.Join(",", l.DisabledMods.Select(m => $"\"{EscapeJson(m)}\""))}],");
                    sb.Append($"\"MissingMods\":[{string.Join(",", l.MissingMods.Select(m => $"\"{EscapeJson(m)}\""))}]");
                    sb.Append("}");
                    if (i < loadouts.Count - 1) sb.Append(",");
                }
                sb.Append("]");

                string dir = Path.GetDirectoryName(LoadoutsFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(LoadoutsFilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[Several Bees] Loadout write error: " + ex.Message);
            }
        }

        private List<ModLoadout> ParseLoadoutsJson(string json)
        {
            var result = new List<ModLoadout>();
            json = json.Trim();
            if (!json.StartsWith("[")) return result;

            int depth = 0;
            int objStart = -1;
            var objects = new List<string>();

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') { if (depth == 0) objStart = i; depth++; }
                else if (json[i] == '}') { depth--; if (depth == 0 && objStart >= 0) { objects.Add(json.Substring(objStart, i - objStart + 1)); objStart = -1; } }
            }

            foreach (var obj in objects)
            {
                var loadout = new ModLoadout();
                loadout.Number = ParseJsonInt(obj, "Number");
                loadout.EnabledMods = ParseJsonStringArray(obj, "EnabledMods");
                loadout.DisabledMods = ParseJsonStringArray(obj, "DisabledMods");
                loadout.MissingMods = ParseJsonStringArray(obj, "MissingMods");
                result.Add(loadout);
            }

            return result;
        }

        private int ParseJsonInt(string json, string key)
        {
            string search = $"\"{key}\":";
            int idx = json.IndexOf(search);
            if (idx < 0) return 0;
            int start = idx + search.Length;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            int.TryParse(json.Substring(start, end - start), out int val);
            return val;
        }

        private List<string> ParseJsonStringArray(string json, string key)
        {
            var result = new List<string>();
            string search = $"\"{key}\":[";
            int idx = json.IndexOf(search);
            if (idx < 0) return result;

            int start = idx + search.Length;
            int end = json.IndexOf("]", start);
            if (end < 0) return result;

            string inner = json.Substring(start, end - start).Trim();
            if (string.IsNullOrEmpty(inner)) return result;

            bool inStr = false;
            var cur = new System.Text.StringBuilder();
            for (int i = 0; i < inner.Length; i++)
            {
                char c = inner[i];
                if (c == '"' && (i == 0 || inner[i - 1] != '\\')) { inStr = !inStr; if (!inStr && cur.Length > 0) { result.Add(cur.ToString()); cur.Clear(); } continue; }
                if (inStr) cur.Append(c);
            }

            return result;
        }

        private string EscapeJson(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        internal void SaveCurrentLoadout()
        {
            var loadouts = ReadLoadouts();

            if (loadouts.Count >= 20)
            {
                SetToolTip("<color=red>Loadout cap reached (20). Delete one first.</color>");
                return;
            }

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");

            var enabled = new List<string>();
            var disabled = new List<string>();

            if (Directory.Exists(pluginsPath))
            {
                foreach (var f in Directory.GetFiles(pluginsPath, "*.dll"))
                    enabled.Add(Path.GetFileNameWithoutExtension(f));

                foreach (var f in Directory.GetFiles(pluginsPath, "*.dll.disabled"))
                    disabled.Add(Path.GetFileNameWithoutExtension(f).Replace(".dll", ""));
            }

            int nextNum = loadouts.Count == 0 ? 1 : loadouts.Max(l => l.Number) + 1;

            loadouts.Add(new ModLoadout
            {
                Number = nextNum,
                EnabledMods = enabled,
                DisabledMods = disabled,
                MissingMods = new List<string>()
            });

            WriteLoadouts(loadouts);
            SetToolTip($"<color=green>Loadout {nextNum} saved.</color>");
            RefreshLoadoutsMenu();
        }

        internal void RefreshLoadoutsMenu()
        {
            var loadouts = ReadLoadouts();

            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo
                {
                    buttonText = loadouts.Count >= 20 ? "<color=grey>Save Loadout (Full)</color>" : "<color=green>Save Loadout</color>",
                    toolTip = loadouts.Count >= 20 ? "At the 20 loadout cap. Delete one to save a new one." : "Snapshots your current mod state as a new loadout.",
                    method = SaveCurrentLoadout
                }
            };

            foreach (var loadout in loadouts.OrderBy(l => l.Number))
            {
                var l = loadout;
                buttons.Add(new ModButtonInfo
                {
                    buttonText = $"Loadout {l.Number}",
                    toolTip = $"{l.EnabledMods.Count} enabled, {l.DisabledMods.Count} disabled, {l.MissingMods.Count} missing.",
                    method = () => OpenLoadoutPage(l)
                });
            }

            Api.Instance.tokenListButtonInfo["10"] = buttons;
        }

        internal void OpenLoadoutPage(ModLoadout loadout)
        {
            string token = $"loadout_{loadout.Number}";

            if (!Api.Instance.tokenListVisable.ContainsKey(token)) Api.Instance.tokenListVisable[token] = false;
            if (!Api.Instance.tokenListBackToken.ContainsKey(token)) Api.Instance.tokenListBackToken[token] = "10";

            var buttons = new List<ModButtonInfo>
            {
                new ModButtonInfo
                {
                    buttonText = "<color=green>Enable & Restart</color>",
                    toolTip = "Applies this loadout and restarts the game.",
                    method = () => ApplyLoadout(loadout)
                },
                new ModButtonInfo
                {
                    buttonText = "View Mods",
                    toolTip = $"Shows the mods in this loadout.",
                    method = () => OpenLoadoutModList(loadout)
                },
                new ModButtonInfo
                {
                    buttonText = "<color=red>Delete Loadout</color>",
                    toolTip = $"Permanently deletes Loadout {loadout.Number}.",
                    method = () =>
                    {
                        var all = ReadLoadouts();
                        all.RemoveAll(l => l.Number == loadout.Number);
                        WriteLoadouts(all);
                        SetToolTip($"<color=red>Loadout {loadout.Number} deleted.</color>");
                        Api.Instance.OpenMenu("10");
                        RefreshLoadoutsMenu();
                    }
                }
            };

            Api.Instance.tokenListButtonInfo[token] = buttons;
            Api.Instance.tokenList[token] = $"Loadout {loadout.Number}";
            Api.Instance.OpenMenu(token);
        }

        internal void OpenLoadoutModList(ModLoadout loadout)
        {
            string token = $"loadout_{loadout.Number}_mods";

            if (!Api.Instance.tokenListVisable.ContainsKey(token)) Api.Instance.tokenListVisable[token] = false;
            if (!Api.Instance.tokenListBackToken.ContainsKey(token)) Api.Instance.tokenListBackToken[token] = $"loadout_{loadout.Number}";

            var buttons = new List<ModButtonInfo>();

            foreach (var m in loadout.EnabledMods)
                buttons.Add(new ModButtonInfo { buttonText = $"<color=green>[ON]</color> {m}", toolTip = "Enabled in this loadout." });

            foreach (var m in loadout.DisabledMods)
                buttons.Add(new ModButtonInfo { buttonText = $"<color=red>[OFF]</color> {m}", toolTip = "Disabled in this loadout." });

            foreach (var m in loadout.MissingMods)
                buttons.Add(new ModButtonInfo { buttonText = $"<color=grey>[MISSING]</color> {m}", toolTip = "Was in loadout but no longer found on disk." });

            if (buttons.Count == 0)
                buttons.Add(new ModButtonInfo { buttonText = "<color=grey>No mods recorded.</color>", toolTip = "" });

            Api.Instance.tokenListButtonInfo[token] = buttons;
            Api.Instance.tokenList[token] = $"Loadout {loadout.Number} Mods";
            Api.Instance.OpenMenu(token);
        }

        internal void ApplyLoadout(ModLoadout loadout)
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);

            var allDlls = Directory.GetFiles(pluginsPath, "*.dll")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToList();

            var allDisabled = Directory.GetFiles(pluginsPath, "*.dll.disabled")
                .Select(f => Path.GetFileNameWithoutExtension(f).Replace(".dll", ""))
                .ToList();

            var allKnown = new HashSet<string>(allDlls.Concat(allDisabled), StringComparer.OrdinalIgnoreCase);

            var updatedLoadout = new ModLoadout
            {
                Number = loadout.Number,
                EnabledMods = new List<string>(loadout.EnabledMods),
                DisabledMods = new List<string>(loadout.DisabledMods),
                MissingMods = new List<string>(loadout.MissingMods)
            };

            foreach (var name in loadout.EnabledMods)
            {
                string dll = Path.Combine(pluginsPath, name + ".dll");
                string dis = dll + ".disabled";

                if (File.Exists(dis))
                {
                    try { if (File.Exists(dll)) File.Delete(dll); File.Move(dis, dll); }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Loadout enable " + name + ": " + ex.Message); }
                }
                else if (!File.Exists(dll))
                {
                    if (!updatedLoadout.MissingMods.Contains(name))
                        updatedLoadout.MissingMods.Add(name);
                    updatedLoadout.EnabledMods.Remove(name);
                }
            }

            foreach (var name in loadout.DisabledMods)
            {
                string dll = Path.Combine(pluginsPath, name + ".dll");
                string dis = dll + ".disabled";

                if (File.Exists(dll))
                {
                    try { if (File.Exists(dis)) File.Delete(dis); File.Move(dll, dis); }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[Several Bees] Loadout disable " + name + ": " + ex.Message); }
                }
                else if (!File.Exists(dis))
                {
                    if (!updatedLoadout.MissingMods.Contains(name))
                        updatedLoadout.MissingMods.Add(name);
                    updatedLoadout.DisabledMods.Remove(name);
                }
            }

            var all = ReadLoadouts();
            int idx = all.FindIndex(l => l.Number == loadout.Number);
            if (idx >= 0) all[idx] = updatedLoadout;
            WriteLoadouts(all);

            RestartApp();
        }

        #endregion

        #region Machine Spawn

        internal void SpawnMachineAtPlayer()
        {
            Vector3 spawnPos = Config.machineRelSpawn();
            foreach (GameObject obj in ModMangerDistanceIndicators)
                if (Vector3.Distance(spawnPos, obj.transform.position) <= Config.machineSpawnClearance) return;

            if (Time.time - lastSpawnTime < 2f) return;
            lastSpawnTime = Time.time;

            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/open.wav");

            var machine = InstanceModManger();
            machine.transform.position = spawnPos;
            machine.transform.LookAt(Config.BodyReference());
            machine.transform.Rotate(0, 180, 0);
            machine.AddComponent<MachineDespawn>().DespawnDistance = Config.MachineDespawnDistance;

            if (Api.Instance.GrabButton("8", "Animations").enabled) StartCoroutine(MachineAnn(machine));
        }

        private IEnumerator MachineAnn(GameObject machine)
        {
            Vector3 og = machine.transform.localScale;
            Vector3 over = og * 1.1f;
            machine.transform.localScale = Vector3.zero;

            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                machine.transform.localScale = Vector3.Lerp(Vector3.zero, over, Mathf.Sin((t / 0.25f) * Mathf.PI * 0.5f));
                yield return null;
            }

            t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                machine.transform.localScale = Vector3.Lerp(over, og, t / 0.1f);
                yield return null;
            }

            machine.transform.localScale = og;
        }

        #endregion

        #region Utility

        internal void ListError(string error)
        {
            if (ErrorParent == null) ErrorParent = new GameObject("Several Bees || Error Parent");
            new GameObject("Several Bees |" + ErrorInt++ + "| " + error).transform.SetParent(ErrorParent.transform);
        }

        internal AudioClip GetLoadedSound(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return null;
                Plugin.Instance.LoadedSounds.TryGetValue(url, out var clip);
                return clip;
            }
            catch { return null; }
        }

        internal void PlaySound(string url, float volume = 0.4f)
        {
            try
            {
                if (!Api.Instance.GrabButton("8", "Sound Effects").enabled) return;
                var clip = GetLoadedSound(url);
                if (clip == null) return;
                var go = new GameObject("SB Sound Player");
                var src = go.AddComponent<AudioSource>();
                src.clip = clip;
                src.volume = volume;
                src.Play();
                Destroy(go, clip.length);
            }
            catch { }
        }

        private static string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var sb = new System.Text.StringBuilder();
            bool inTag = false;
            foreach (char c in input)
            {
                if (c == '<') { inTag = true; continue; }
                if (c == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        internal void RestartApp()
        {
            string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "config");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            string exe = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string bat = $@"@echo off
title Several Bees - Restarting...
:WAIT
tasklist /FI ""IMAGENAME eq {exe}"" | find /I ""{exe}"" >nul
if %ERRORLEVEL%==0 (timeout /t 1 >nul && goto WAIT)
start steam://run/{Config.SteamAppId}
exit";

            string batPath = Path.Combine(configDir, "sb_restart.bat");
            File.WriteAllText(batPath, bat);
            Process.Start(batPath);
            Application.Quit();
        }

        #endregion

        #region Update

        private void Update()
        {
            if (Api.Instance.GrabButton("8", "Physical Back Button").enabled != PBB)
            {
                PBB = Api.Instance.GrabButton("8", "Physical Back Button").enabled;
                PointerPositionIndex += PBB ? -1 : 1;
            }

            for (int i = ModMangerDistanceIndicators.Count - 1; i >= 0; i--)
                if (ModMangerDistanceIndicators[i] == null) ModMangerDistanceIndicators.RemoveAt(i);

            for (int i = ModMangerTextList.Count - 1; i >= 0; i--)
                if (ModMangerTextList[i] == null) ModMangerTextList.RemoveAt(i);

            if (UnityInput.Current.GetKey(KeyCode.M) && !SpawnNewThingPress) { SpawnMachineAtPlayer(); SpawnNewThingPress = true; }
            if (!UnityInput.Current.GetKey(KeyCode.M)) SpawnNewThingPress = false;

            try
            {
                float dist = Vector3.Distance(Config.LeftHandReference().position, Config.RightHandReference().position);
                if (dist < Config.gripThreshold * 1.25f && Config.RightGripDown() && Config.LeftGripDown() && Api.Instance.GrabButton("8", "Open Gesture").enabled)
                {
                    float prevDist = Vector3.Distance(previousLeftPos, previousRightPos);
                    Vector3 lv = (Config.LeftHandReference().position - previousLeftPos) / Mathf.Max(Time.deltaTime, 0.0001f);
                    Vector3 rv = (Config.RightHandReference().position - previousRightPos) / Mathf.Max(Time.deltaTime, 0.0001f);
                    float speed = Vector3.Dot((Config.RightHandReference().position - Config.LeftHandReference().position).normalized, rv - lv);

                    if (speed > Config.pullSpeedThreshold * 0.6f && (dist - prevDist) > Config.minPullDistance * 0.5f)
                        SpawnMachineAtPlayer();
                }

                previousLeftPos = Vector3.Lerp(previousLeftPos, Config.LeftHandReference().position, 0.5f);
                previousRightPos = Vector3.Lerp(previousRightPos, Config.RightHandReference().position, 0.5f);
            }
            catch { }

            if (UnityInput.Current.GetKey(KeyCode.T) && UnityInput.Current.GetKey(KeyCode.E) && UnityInput.Current.GetKey(KeyCode.S))
                TestMode = true;

            bool pcActive = false;
            foreach (var obj in ModMangerDistanceIndicators)
                if (obj != null && Vector3.Distance(Config.BodyReference().position, obj.transform.position) < Config.MaxKeyboardControllsDisctance)
                    pcActive = true;
            PCControlActive = pcActive;

            if (Config.IsGui)
            {
                if (UnityInput.Current.GetKey(KeyCode.BackQuote) && !GuiButtonPress) { ShowGUIMenu = !ShowGUIMenu; GuiButtonPress = true; }
                if (!UnityInput.Current.GetKey(KeyCode.BackQuote)) GuiButtonPress = false;
            }

            if (PCControlActive)
            {
                if (UnityInput.Current.GetKey(KeyCode.DownArrow) && !DownArrowPress) { MmDown(false); DownArrowPress = true; }
                if (!UnityInput.Current.GetKey(KeyCode.DownArrow)) DownArrowPress = false;

                if (UnityInput.Current.GetKey(KeyCode.UpArrow) && !UpArrowPress) { MmUp(false); UpArrowPress = true; }
                if (!UnityInput.Current.GetKey(KeyCode.UpArrow)) UpArrowPress = false;

                bool ret = UnityInput.Current.GetKey(KeyCode.Return);
                bool rgt = UnityInput.Current.GetKey(KeyCode.RightArrow);
                if ((ret || rgt) && !EnterPress) { MmSelect(rgt); EnterPress = true; }
                if (!ret && !rgt) EnterPress = false;
            }

            if (!IsLatestVersion) SectionName = "NotNew";
            else if (!LoadedPlugins) SectionName = "LoadPlugins";

            if (TestMode && !TestModeDone)
            {
                TestMod1Token = Api.Instance.GenerateToken("Test Mod 1");
                TestMod2Token = Api.Instance.GenerateToken("Test Mod 2");
                TestMod3Token = Api.Instance.GenerateToken("Alot Of Stuff");

                Api.Instance.SetButtonInfo(TestMod1Token, new List<ModButtonInfo> { new ModButtonInfo { buttonText = "Test1 in 1" } });

                var btns = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "Toggle", isTogglable = true } };
                for (int i = 1; i <= 300; i++) btns.Add(new ModButtonInfo { buttonText = "Toggle " + i, isTogglable = true });
                Api.Instance.SetButtonInfo(TestMod3Token, btns);

                Config.IsGui = true;
                TestModeDone = true;
            }

            BuildDisplayText();

            try
            {
                foreach (string token in Api.Instance.tokenList.Keys)
                    foreach (var mbi in Api.Instance.tokenListButtonInfo[token])
                        if (mbi.enabled && mbi.isTogglable) mbi.method?.Invoke();
            }
            catch (Exception e) { ListError("Update loop error: " + e.Message); }
        }

        private void BuildDisplayText()
        {
            string sectionLabel = SectionName;
            if (Api.Instance.tokenList.ContainsKey(SectionName))
                sectionLabel = Api.Instance.tokenList[SectionName];

            string text = Extra.GradientText("Several Bees", Theme1, Theme2, ThemeFadeSpeed)
                        + $"\n<color=grey>---</color> <size=0.35>{sectionLabel}</size> <color=grey>---</color>\n <size=0.3>";

            var things = GetThings();
            int total = things.Count;
            int window = 4;
            int start = Mathf.Clamp(PointerPositionIndex - 3, 0, Mathf.Max(0, total - window));

            if (start > 0) text += "\n</size><size=0.1>••••••</size><size=0.3>";

            for (int i = start; i < start + window && i < total; i++)
            {
                var t = things[i];
                if (t.mbi?.buttonOverlayText != null) t.Name = t.mbi.buttonOverlayText;
                string ptr = i == PointerPositionIndex ? $"<color=#{ColorUtility.ToHtmlStringRGB(Theme1)}>> </color>" : "";
                text += $"\n{ptr}{t.Name}";
            }

            if (start + window < total) text += "\n</size><size=0.1>••••••</size><size=0.3>";

            MaxPointerPosition = total;
            text += "</size>";
            if (!string.IsNullOrEmpty(ToolTipText)) text += $"\n \n<size=0.2>{ToolTipText}</size>";

            foreach (var tmp in ModMangerTextList) tmp.text = text;
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (gradientStyle == null)
            {
                gradientStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (PCControlActive)
                GUI.Label(new Rect(10, Screen.height - 30, 400, 30),
                    "<b>" + Extra.GradientText("SB PC Control Active", Theme1, Theme2, ThemeFadeSpeed) + "</b>",
                    gradientStyle);

            if (!ShowGUIMenu) return;

            var ev = Event.current;
            if (ev.type == EventType.MouseDown && menuRect.Contains(ev.mousePosition)) { dragging = true; dragOffset = ev.mousePosition - new Vector2(menuRect.x, menuRect.y); }
            if (dragging && ev.type == EventType.MouseDrag) menuRect.position = ev.mousePosition - dragOffset;
            if (dragging && ev.type == EventType.MouseUp) dragging = false;

            var prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.1f);
            GUI.Box(menuRect, "");
            GUI.color = prev;

            GUILayout.BeginArea(menuRect);

            float tipH = 40;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUIStyle.none,
                GUILayout.Width(menuRect.width), GUILayout.Height(menuRect.height - tipH));

            GUILayout.Label(Extra.GradientText("Several Bees", Theme1, Theme2, ThemeFadeSpeed) + "\n<size=8>` To Toggle</size>", gradientStyle);

            string sec = SectionName;
            if (Api.Instance.tokenList.ContainsKey(SectionName))
                sec = Api.Instance.tokenList[SectionName];
            GUILayout.Label($"<color=grey>---</color> <size=14>{sec}</size> <color=grey>---</color>", gradientStyle);

            var things = GetThings();
            for (int i = 0; i < things.Count; i++)
            {
                var t = things[i];
                if (t.mbi?.buttonOverlayText != null) t.Name = t.mbi.buttonOverlayText;
                if (GUILayout.Button(t.Name, gradientStyle)) { PointerPositionIndex = i; MmSelect(true); }
            }

            GUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(ToolTipText))
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<size=12>{ToolTipText}</size>", gradientStyle, GUILayout.Height(tipH));
            }

            GUILayout.EndArea();
        }

        #endregion
    }

    #region Support Types

    internal class Things
    {
        public string Name = null;
        public bool Enterable = false;
        public string Token = null;
        public ModButtonInfo mbi = null;
    }

    internal class DetailedColor
    {
        public Color color = Color.white;
        public string name = "null";
    }

    internal class ConfigEntry
    {
        public string Section;
        public string Key;
        public string Value;
        public string Description;
        public string AcceptableValues;
        public string TypeName;
        public string FilePath;
    }

    internal class InstalledMod
    {
        public string Name;
        public string Path;
        public bool OnDisk;
        public PluginInfo PluginInfo;
        public BaseUnityPlugin LiveInstance;
        public bool? LiveEnabled;
    }

    internal class ModLoadout
    {
        public int Number;
        public List<string> EnabledMods = new List<string>();
        public List<string> DisabledMods = new List<string>();
        public List<string> MissingMods = new List<string>();
    }

    internal class SbMod
    {
        public int Id;
        public string Name = "";
        public string DllName = "";
        public string RepoUrl = "";
        public string Description = "";
        public string ImageUrl = "";
        public string Author = "";
        public int Upvotes;
        public bool IsVerified;
        public bool IsFeatured;
    }

    #endregion
}