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
using UnityEngine.InputSystem.HID;
using UnityEngine.Networking;
using Photon.Voice;
using Valve.VR.InteractionSystem;

namespace SeveralBees
{
    public class SeveralBeesCore : MonoBehaviour
    {
        internal bool IsLatestVersion = true;
        public bool TestMode = false;
        public static SeveralBeesCore Instance { get; private set; }

        public List<DetailedColor> CycleColors = new List<DetailedColor>
        {
            new DetailedColor { color = Color.red, name = "Red" }, // 0
            new DetailedColor { color = new Color(1f, 0.5f, 0f), name = "Orange" }, // 1
            new DetailedColor { color = Color.yellow, name = "Yellow" }, // 2
            new DetailedColor { color = Color.green, name = "Green" }, // 3
            new DetailedColor { color = Color.cyan, name = "Cyan" }, // 4
            new DetailedColor { color = Color.blue, name = "Blue" }, // 5
            new DetailedColor { color = new Color(0.5f, 0f, 1f), name = "Purple" }, // 6
            new DetailedColor { color = Color.black, name = "Black" }, // 7
            new DetailedColor { color = Color.white, name = "White" }, // 8
            new DetailedColor { color = Color.magenta, name = "Magenta" }, // 9
            new DetailedColor { color = new Color(1f, 0.75f, 0.8f), name = "Pink" }, // 10
            new DetailedColor { color = new Color(0.6f, 0.3f, 0f), name = "Brown" }, // 11
            new DetailedColor { color = Color.gray, name = "Gray" }, // 12
            new DetailedColor { color = Color.green * 1.5f, name = "Lime" }, // 13
            new DetailedColor { color = new Color(0f, 0f, 0.5f), name = "Navy" }, // 14
            new DetailedColor { color = new Color(0.75f, 0.75f, 0.75f), name = "Silver" }, // 15
        };


        public List<float> CycleFloats = new List<float>
        {
            0.1f,
            0.2f,
            0.25f,
            0.5f,
            0.75f,
            1f,
            1.5f,
            2f,
            3f,
            5f,
        };

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Several Bees Object Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Several Bees Instance Set");
        }

        private async void Start()
        {
            try
            {
                if (ErrorParent == null)
                {
                    ErrorParent = new GameObject("Several Bees || Error Parent");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] Error in Creating Error Parent (Start SeveralBees.SeveralBees): " + e.Message);
            }

            UnityEngine.Debug.Log("[Several Bees] Error Parent Created");



            try
            {
                StartCoroutine(WaitForRigThenCreatePointers());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] Error in CreatePointers (Start SeveralBees.SeveralBees): " + e.Message);
            }

            UnityEngine.Debug.Log("[Several Bees] Pointers Created");



            try
            {
                await Create();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] Error in Create (Start SeveralBees.SeveralBees): " + e.Message);
            }

            UnityEngine.Debug.Log("[Several Bees] Setup Started");

            if (Config.IsGui) ShowGUIMenu = true;
        }

        private GameObject ModManegerParent = null;
        private List<TextMeshPro> ModMangerTextList = new List<TextMeshPro>();
        private List<GameObject> ModMangerDistanceIndicators = new List<GameObject>();
        private int ErrorInt = 1;

        public int ClickButtonSound = 66;

        public GameObject ErrorParent = null;

        internal AssetBundle Bundle;

        internal List<Collider> ObjectColliders = new List<Collider>();

        internal void ReMakeModManger()
        {
            if (ModManegerParent != null)
            {
                Destroy(ModManegerParent);
            }
            Create();
        }

        public GameObject InstanceModManger()
        {
            GameObject Parent = new GameObject("Several Bees || Mod Manger (Instance)");

            if (Config.CompType == ComputerType.SimpleButtons || Config.CompType == ComputerType.Text)
            {
                var textObj = new GameObject("SB_Text");
                TextMeshPro ModMangerText = textObj.AddComponent<TextMeshPro>();
                ModMangerText.text = "Several Bees";
                textObj.transform.position = Vector3.zero;
                textObj.transform.SetParent(Parent.transform);
                ModMangerText.fontSize = 0.5f;
                ModMangerText.alignment = TextAlignmentOptions.Center;
                ModMangerText.gameObject.transform.position = new Vector3(0f, 0.4f, 0f);
                ModMangerText.color = Color.white;
                ModMangerTextList.Add(ModMangerText);
            }

            if (Config.CompType == ComputerType.SimpleButtons)
            {
                GameObject ModMangerButtonParent = new GameObject("Buttons");
                ModMangerButtonParent.transform.SetParent(Parent.transform);



                var cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube1.name = "SB_Down";
                cube1.transform.position = new Vector3(-0.1f, 0f, 0.075f);
                cube1.transform.SetParent(ModMangerButtonParent.transform);
                cube1.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);
                cube1.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                cube1.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                cube1.GetComponent<Scripts.Button>().Click += (bool Left) =>
                {
                    MmDown(Left);
                };
                Extra.Instance.MakeObjectVisible(cube1);
                Material mat = cube1.GetComponent<Renderer>().material;
                mat.color = Theme2;
                ObjectColliders.Add(cube1.GetComponent<Collider>());

                var cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube2.name = "SB_Up";
                cube2.transform.position = new Vector3(-0.1f, 0f, -0.075f);
                cube2.transform.SetParent(ModMangerButtonParent.transform);
                cube2.transform.localScale = new Vector3(0.05f, 0.1f, 0.1f);
                cube2.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                cube2.AddComponent<Scripts.Button>().Name = "SB_Up_Button";
                cube2.GetComponent<Scripts.Button>().Click += (bool Left) =>
                {
                    MmUp(Left);
                };
                Extra.Instance.MakeObjectVisible(cube2);
                mat = cube2.GetComponent<Renderer>().material;
                mat.color = Theme2;
                ObjectColliders.Add(cube2.GetComponent<Collider>());

                var cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube3.name = "SB_Select";
                cube3.transform.position = new Vector3(0.125f, 0f, 0f);
                cube3.transform.SetParent(ModMangerButtonParent.transform);
                cube3.transform.localScale = new Vector3(0.05f, 0.25f, 0.25f);
                cube3.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                cube3.AddComponent<Scripts.Button>().Name = "SB_Select_Button";
                cube3.GetComponent<Scripts.Button>().Click += (bool Left) =>
                {
                    MmSelect(Left);
                };
                Extra.Instance.MakeObjectVisible(cube3);
                mat = cube3.GetComponent<Renderer>().material;
                mat.color = Theme2;
                ObjectColliders.Add(cube3.GetComponent<Collider>());




                ModMangerButtonParent.transform.rotation = Quaternion.Euler(90f, 0, 0f);
                ModMangerButtonParent.transform.position = new Vector3(-0.025f, 0f, 0f);
                foreach (Transform ajsfajk in ModMangerButtonParent.transform)
                {
                    ajsfajk.GetComponent<Collider>().isTrigger = false;
                }
            }

            if (Config.CompType == ComputerType.FullComputer)
            {
                var textObj = new GameObject("SB_Text");
                TextMeshPro ModMangerText = textObj.AddComponent<TextMeshPro>();
                ModMangerText.text = "Several Bees";
                textObj.transform.position = Vector3.zero;
                textObj.transform.SetParent(Parent.transform);
                ModMangerText.fontSize = 0.5f;
                ModMangerText.alignment = TextAlignmentOptions.Center;
                ModMangerText.gameObject.transform.position = new Vector3(0f, 0.38f, -0.01f);
                ModMangerText.color = Color.white;
                textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0.4f, 0.35f);
                ModMangerText.enableAutoSizing = true;
                ModMangerText.fontSizeMax = 0.5f;
                ModMangerText.fontSizeMin = 0.2f;
                ModMangerTextList.Add(ModMangerText);

                GameObject Computer = null;
                GameObject computerPrefab = null;
                if (AssetLoader.TryGetAsset<GameObject>("Sb Computer Variant", out computerPrefab))
                {
                    Computer = Instantiate(computerPrefab);
                    Computer.transform.SetParent(Parent.transform);
                    foreach (Transform Child in Computer.transform)
                    {
                        Extra.Instance.MakeObjectVisible(Child.gameObject, false);
                        if (Child.name == "Up")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmUp(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else if (Child.name == "Down")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmDown(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else if (Child.name == "Select")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmSelect(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else
                        {
                            MeshCollider ocl67 = Child.AddComponent<MeshCollider>();
                            ObjectColliders.Add(ocl67);
                        }
                    }
                    Computer.transform.position = new Vector3(0.07f, 0.09f, -0.1563f);
                    Computer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    Computer.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                }
            }

            if (Config.CompType == ComputerType.FullMachine)
            {
                var textObj = new GameObject("SB_Text");
                TextMeshPro ModMangerText = textObj.AddComponent<TextMeshPro>();
                ModMangerText.text = "Several Bees";
                textObj.transform.position = Vector3.zero;
                textObj.transform.SetParent(Parent.transform);
                ModMangerText.fontSize = 0.5f;
                ModMangerText.alignment = TextAlignmentOptions.Center;
                ModMangerText.gameObject.transform.position = new Vector3(-0.005f, 0.38f, -0.01f);
                ModMangerText.color = Color.white;
                ModMangerTextList.Add(ModMangerText);

                GameObject Machine = null;
                GameObject machinePrefab = null;
                if (AssetLoader.TryGetAsset<GameObject>("SbMachine", out machinePrefab))
                {
                    Machine = Instantiate(machinePrefab);
                    Machine.transform.SetParent(Parent.transform);
                    foreach (Transform Child in Machine.transform)
                    {
                        Extra.Instance.MakeObjectVisible(Child.gameObject, false);
                        if (Child.name == "Up")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmUp(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else if (Child.name == "Down")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmDown(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else if (Child.name == "Select")
                        {
                            Child.AddComponent<Scripts.Button>().Name = "SB_Down_Button";
                            Child.GetComponent<Scripts.Button>().Click += (bool Left) =>
                            {
                                MmSelect(Left);
                            };
                            Child.AddComponent<BoxCollider>();
                            ObjectColliders.Add(Child.GetComponent<Collider>());
                        }
                        else if (Child.name == "Machine")
                        {
                            MeshCollider ocl67 = Child.AddComponent<MeshCollider>();
                            ObjectColliders.Add(ocl67);
                        }
                    }
                    Machine.transform.position = new Vector3(0f, 0.04f, 0.02f);
                    Machine.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                    Machine.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
                }
            }

            GameObject iLoveMen = new GameObject("Distance Indicator"); //what am i doing with these varible
            iLoveMen.transform.SetParent(Parent.transform);
            ModMangerDistanceIndicators.Add(iLoveMen);

            return Parent;
        }
        public void ListError(string Error)
        {
            if(ErrorParent == null)
            {
                ErrorParent = new GameObject("Several Bees || Error Parent");
            }

            GameObject gameObject = new GameObject("Several Bees |" + ErrorInt + "| " + Error);
            gameObject.GetComponentInParent<Transform>().SetParent(ErrorParent.transform);
        }
        public async Task Create()
        {
            try
            {
                if (!Api.Instance.HasMadeSettings)
                {
                    SeveralBeesCore.Instance.MakeSettings();
                    Api.Instance.HasMadeSettings = true;
                }

                GameObject imAGayTwinkFurryFemboy = InstanceModManger();
                imAGayTwinkFurryFemboy.name = "Several Bees || Mod Manger";
                ModManegerParent = imAGayTwinkFurryFemboy;

                ModManegerParent.transform.position = Config.MachineSpawnPoint;
                ModManegerParent.transform.rotation = Quaternion.Euler(Config.MachineSpawnRoto);
                ModManegerParent.transform.localScale = Config.MachineSpawnScale;

                if (Config.CompType == ComputerType.None)
                {
                    ModManegerParent.transform.position = new Vector3(0f, -int.MaxValue, 0f);
                    ModManegerParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    ModManegerParent.transform.localScale = new Vector3(0f, 0f, 0f);
                    ModManegerParent.name = "Several Bees || Mod Manger (Killed)";
                }

                UnityEngine.Debug.Log("[Several Bees] Mod Manger Set Up Complete");
            }
            catch (Exception e)
            {
                ListError("Error in Creating Mod Manger: " + e.Message + $" [{e.StackTrace}]");
            }
        }

        public void MmDown(bool Left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            PointerPositionIndex++;
            if(PointerPositionIndex >= MaxPointerPosition)
            {
                PointerPositionIndex = 0;
            }
        }
        public void MmUp(bool Left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            PointerPositionIndex--;
            if (PointerPositionIndex < 0)
            {
                PointerPositionIndex = MaxPointerPosition - 1;
            }
        }
        public void MmSelect(bool Left)
        {
            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/click1.wav");
            List<Things> things = GetThings();

            bool Enterable = things[PointerPositionIndex].Enterable;
            string Name = things[PointerPositionIndex].Name;
            string Token = things[PointerPositionIndex].Token;
            ModButtonInfo mbi = things[PointerPositionIndex].mbi;

            if (Enterable)
            {
                SectionName = Token;
                PointerPositionIndex = 0;
                if (Name == "Back")
                {
                    SetToolTip($"<color=grey>[</color>Back<color=grey>]</color> Takes you back to the last tab.");
                    return;
                }
                SetToolTip($"<color=grey>[</color>{Name}<color=grey>]</color> Takes you to the '{Name}' tab.");
            }
            else
            {
                if (mbi != null)
                {
                    bool istgl = mbi.isTogglable;
                    if (istgl)
                    {
                        mbi.enabled = !mbi.enabled;

                        (mbi.enabled ? mbi.enableMethod : mbi.disableMethod)?.Invoke();
                    } else
                    {
                        mbi.method?.Invoke();
                    }

                    if (mbi.toolTip != null)
                    {
                        if(mbi.isTogglable)
                        {
                            if(mbi.enabled)
                            {
                                SetToolTip($"<color=grey>[</color><color=green>{(mbi.buttonOverlayText == null ? mbi.buttonText : mbi.buttonOverlayText)}</color><color=grey>]</color> {mbi.toolTip}");
                            }
                            else
                            {
                                SetToolTip($"<color=grey>[</color><color=red>{(mbi.buttonOverlayText == null ? mbi.buttonText : mbi.buttonOverlayText)}</color><color=grey>]</color> {mbi.toolTip}");
                            }
                        }
                        else
                        {
                            SetToolTip($"<color=grey>[</color>{(mbi.buttonOverlayText == null ? mbi.buttonText : mbi.buttonOverlayText)}<color=grey>]</color> {mbi.toolTip}");
                        }
                    }
                }
                else
                {
                }
            }
        }

        public string ToolTipText = "";

        internal void SetToolTip(string Text)
        {
            if(TooltipKillCoroutine != null)
            {
                StopCoroutine(TooltipKillCoroutine);
                TooltipKillCoroutine = null;
            }
            ToolTipText = Text;
            TooltipKillCoroutine = StartCoroutine(TooltipKill());
        }

        Coroutine TooltipKillCoroutine = null;

        private IEnumerator TooltipKill()
        {
            yield return new WaitForSeconds(ToolTipKillTime);
            ToolTipText = "";
        }

        private GameObject LeftPointer = null;
        public SphereCollider LeftPointerCollider = null;

        internal float ToolTipKillTime = 8f;

        private GameObject RightPointer = null;
        public SphereCollider RightPointerCollider = null;
        private void CreatePointers()
        {
            // Code Inspired By IIDK's menu temp
            try
            {
                LeftPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                LeftPointer.name = "Several Bees || Left Pointer";
                LeftPointer.transform.parent = Config.LeftHandReference();
                LeftPointer.transform.localPosition = Config.LeftHandPointerOffset;
                LeftPointer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                LeftPointerCollider = LeftPointer.GetComponent<SphereCollider>();
                LeftPointer.GetComponent<Renderer>().enabled = false;

                RightPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                RightPointer.name = "Several Bees || Right Pointer";
                RightPointer.transform.parent = Config.RightHandReference();
                RightPointer.transform.localPosition = Config.RightHandPointerOffset;
                RightPointer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                RightPointerCollider = RightPointer.GetComponent<SphereCollider>();
                RightPointer.GetComponent<Renderer>().enabled = false;

                previousLeftPos = Config.LeftHandReference().transform.position;
                previousRightPos = Config.RightHandReference().transform.position;
            }
            catch (Exception e)
            {
                ListError("Error in CreatePointers: " + e.Message + $" [{e.StackTrace}]");
            }
        }
        private IEnumerator WaitForRigThenCreatePointers()
        {
            while (Config.RightHandReference() == null || Config.RightHandReference() == null)
            {
                yield return null;
            }

            CreatePointers();
            UnityEngine.Debug.Log("[Several Bees] Pointers Created");
        }
        internal void MakeSettings()
        {
            try
            {
                Api.Instance.tokenList.Add("Settings", "1");
                Api.Instance.tokenListVisable.Add("1", true);
                Api.Instance.tokenListBackToken.Add("1", "Main");
                Api.Instance.tokenListButtonInfo["1"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "General Settings", isTogglable = false, method = () => Api.Instance.OpenMenu("8"), toolTip = "Opens the general settings." },
                    new ModButtonInfo { buttonText = "Theme Settings", isTogglable = false, method = () => Api.Instance.OpenMenu("2"), toolTip = "Opens the theme settings." },
                    new ModButtonInfo { buttonText = "<color=yellow>Credits</color>", isTogglable = false, method = () => Api.Instance.OpenMenu("7"), toolTip = "Opens the credits." },
                };

                Api.Instance.tokenList.Add("Theme", "2");
                Api.Instance.tokenListVisable.Add("2", false);
                Api.Instance.tokenListBackToken.Add("2", "1");
                Api.Instance.tokenListButtonInfo["2"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Theme Presets", isTogglable = false, method = () => Api.Instance.OpenMenu("3"), toolTip = "Opens the theme presets menu." },
                    new ModButtonInfo { buttonText = "cfs", isTogglable = false, method = () => Settings.cfs(), toolTip = "Adjusts the speed of the menu color fading." },
                    new ModButtonInfo { buttonText = "nfc", isTogglable = false, method = () => Settings.nfc(), toolTip = "Changes the menu title first fade color." },
                    new ModButtonInfo { buttonText = "nsc", isTogglable = false, method = () => Settings.nsc(), toolTip = "Changes the menu title second fade color." },
                };

                Api.Instance.tokenList.Add("Theme Presets", "3");
                Api.Instance.tokenListVisable.Add("3", false);
                Api.Instance.tokenListBackToken.Add("3", "2");
                Api.Instance.tokenListButtonInfo["3"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Defualt", buttonOverlayText = "<color=#8A2BE2>D</color><color=#9B30FF>e</color><color=#A64AC9>f</color><color=#B266FF>u</color><color=#C080FF>a</color><color=#D19EFF>l</color><color=#E0BFFF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[6].color, CycleColors[7].color, 0.2f), toolTip = "Purple and black." },
                    new ModButtonInfo { buttonText = "Breeze", buttonOverlayText = "<color=#00FFFF>B</color><color=#1CEEEE>r</color><color=#3AFFFF>e</color><color=#5AFFEE>e</color><color=#7AFFFF>z</color><color=#9CFFFF>e</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[4].color, CycleColors[9].color, 0.2f), toolTip = "Cyan and magenta." },
                    new ModButtonInfo { buttonText = "Rose", buttonOverlayText = "<color=#FFBFCF>R</color><color=#FF9FBF>o</color><color=#FF7FBF>s</color><color=#FF5FAF>e</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[10].color, CycleColors[8].color, 0.2f), toolTip = "Soft pink with white." },
                    new ModButtonInfo { buttonText = "Earth", buttonOverlayText = "<color=#964B00>E</color><color=#A65A0F>a</color><color=#B66B1F>r</color><color=#C57C2F>t</color><color=#D58D3F>h</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[11].color, CycleColors[3].color, 0.2f), toolTip = "Brown and green." },
                    new ModButtonInfo { buttonText = "Storm", buttonOverlayText = "<color=#808080>S</color><color=#909090>t</color><color=#A0A0A0>o</color><color=#B0B0B0>r</color><color=#C0C0C0>m</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[12].color, CycleColors[7].color, 0.2f), toolTip = "Gray with black." },
                    new ModButtonInfo { buttonText = "Ocean", buttonOverlayText = "<color=#00BFFF>O</color><color=#1ECFFF>c</color><color=#3ADFFF>e</color><color=#56EFFF>a</color><color=#72FFFF>n</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[5].color, CycleColors[4].color, 0.2f), toolTip = "Blue and cyan." },
                    new ModButtonInfo { buttonText = "Neon", buttonOverlayText = "<color=#7FFF00>N</color><color=#8CFF19>e</color><color=#9AFF33>o</color><color=#A7FF4D>n</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[13].color, CycleColors[4].color, 0.2f), toolTip = "Lime and cyan." },
                    new ModButtonInfo { buttonText = "Toxic", buttonOverlayText = "<color=#7FFF00>T</color><color=#8CFF19>o</color><color=#9AFF33>x</color><color=#A7FF4D>i</color><color=#B5FF66>c</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[13].color, CycleColors[11].color, 0.2f), toolTip = "Lime and brown." },
                    new ModButtonInfo { buttonText = "Royal", buttonOverlayText = "<color=#8A2BE2>R</color><color=#9B30FF>o</color><color=#A64AC9>y</color><color=#B266FF>a</color><color=#C080FF>l</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[6].color, CycleColors[14].color, 0.2f), toolTip = "Purple and navy." },
                    new ModButtonInfo { buttonText = "Flare", buttonOverlayText = "<color=#FF0000>F</color><color=#FF1A1A>l</color><color=#FF3333>a</color><color=#FF4D4D>r</color><color=#FF6666>e</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[0].color, CycleColors[1].color, 0.2f), toolTip = "Red and orange." },
                    new ModButtonInfo { buttonText = "Sunset", buttonOverlayText = "<color=#FFA500>S</color><color=#FFB733>u</color><color=#FFC966>n</color><color=#FFD999>s</color><color=#FFECCC>e</color><color=#FFF0FF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[1].color, CycleColors[8].color, 0.2f), toolTip = "Orange and white." },
                    new ModButtonInfo { buttonText = "Solar", buttonOverlayText = "<color=#FFFF00>S</color><color=#FFFF33>o</color><color=#FFFF66>l</color><color=#FFFF99>a</color><color=#FFFFCC>r</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[2].color, CycleColors[1].color, 0.2f), toolTip = "Yellow and orange." },
                    new ModButtonInfo { buttonText = "Frost", buttonOverlayText = "<color=#00FFFF>F</color><color=#33FFFF>r</color><color=#66FFFF>o</color><color=#99FFFF>s</color><color=#CCFFFF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[4].color, CycleColors[15].color, 0.2f), toolTip = "Cyan and silver." },
                    new ModButtonInfo { buttonText = "Steel", buttonOverlayText = "<color=#C0C0C0>S</color><color=#D0D0D0>t</color><color=#E0E0E0>e</color><color=#F0F0F0>e</color><color=#FFFFFF>l</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[15].color, CycleColors[12].color, 0.2f), toolTip = "Silver and gray." },
                    new ModButtonInfo { buttonText = "Shadow", buttonOverlayText = "<color=#000000>S</color><color=#111111>h</color><color=#222222>a</color><color=#333333>d</color><color=#444444>o</color><color=#555555>w</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[7].color, CycleColors[12].color, 0.2f), toolTip = "Black and gray." },
                    new ModButtonInfo { buttonText = "Inferno", buttonOverlayText = "<color=#FF0000>I</color><color=#FF3300>n</color><color=#FF6600>f</color><color=#FF9900>e</color><color=#FFCC00>r</color><color=#FFFF00>n</color><color=#FFFF33>o</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[0].color, CycleColors[2].color, 0.2f), toolTip = "Red and yellow." },
                    new ModButtonInfo { buttonText = "Berry", buttonOverlayText = "<color=#FF00FF>B</color><color=#FF33FF>e</color><color=#FF66FF>r</color><color=#FF99FF>r</color><color=#FFCCFF>y</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[9].color, CycleColors[10].color, 0.2f), toolTip = "Magenta and pink." },
                    new ModButtonInfo { buttonText = "Midnight", buttonOverlayText = "<color=#000000>M</color><color=#000033>i</color><color=#000066>d</color><color=#000099>n</color><color=#0000CC>i</color><color=#0000FF>g</color><color=#3333FF>h</color><color=#6666FF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[7].color, CycleColors[5].color, 0.2f), toolTip = "Black and blue." },
                    new ModButtonInfo { buttonText = "Lava", buttonOverlayText = "<color=#FF0000>L</color><color=#FF3300>a</color><color=#FF6600>v</color><color=#FF9900>a</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[0].color, CycleColors[12].color, 0.2f), toolTip = "Red and gray." },
                    new ModButtonInfo { buttonText = "Mint", buttonOverlayText = "<color=#00FF7F>M</color><color=#33FF99>i</color><color=#66FFBB>n</color><color=#99FFDD>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[3].color, CycleColors[13].color, 0.2f), toolTip = "Green and lime." },
                    new ModButtonInfo { buttonText = "Peach", buttonOverlayText = "<color=#FFA07A>P</color><color=#FFB080>e</color><color=#FFC099>a</color><color=#FFD0B3>c</color><color=#FFE0CC>h</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[1].color, CycleColors[10].color, 0.2f), toolTip = "Orange and pink." },
                    new ModButtonInfo { buttonText = "Twilight", buttonOverlayText = "<color=#8A2BE2>T</color><color=#9B30FF>w</color><color=#A64AC9>i</color><color=#B266FF>l</color><color=#C080FF>i</color><color=#D19EFF>g</color><color=#E0BFFF>h</color><color=#F0DFFF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[6].color, CycleColors[4].color, 0.2f), toolTip = "Purple and cyan." },
                    new ModButtonInfo { buttonText = "Cobalt", buttonOverlayText = "<color=#0000FF>C</color><color=#3333FF>o</color><color=#6666FF>b</color><color=#9999FF>a</color><color=#CCCCFF>l</color><color=#FFFFFF>t</color>", isTogglable = false, method = () => Extra.Instance.SetTheme(CycleColors[5].color, CycleColors[14].color, 0.2f), toolTip = "Blue and navy." },
                };

                Api.Instance.tokenList.Add("Mods", "4");
                Api.Instance.tokenListVisable.Add("4", true);
                Api.Instance.tokenListBackToken.Add("4", "Main");
                Api.Instance.tokenListButtonInfo["4"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Code/Dll Mod Browser", isTogglable = false, method = () => { Api.Instance.OpenMenu("5"); }, toolTip = "Opens the Code/Dll Mod Browser." },
                    // i made this config veiwer to see how hard it would be, it was really hard.
                    new ModButtonInfo { buttonText = "Mod Config Info", isTogglable = false, method = () => { Api.Instance.OpenMenu("6"); }, toolTip = "Opens the mod config editor." },
                };

                Api.Instance.tokenList.Add("Code/Dll Mod Browser", "5");
                Api.Instance.tokenListVisable.Add("5", false);
                Api.Instance.tokenListBackToken.Add("5", "4");
                Api.Instance.tokenListButtonInfo["5"] = new List<ModButtonInfo>
                {
                };

                Api.Instance.tokenList.Add("Code/Dll Mod Config Info", "6");
                Api.Instance.tokenListVisable.Add("6", false);
                Api.Instance.tokenListBackToken.Add("6", "4");
                Api.Instance.tokenListButtonInfo["6"] = new List<ModButtonInfo>
                {
                };

                Api.Instance.tokenList.Add("Credits", "7");
                Api.Instance.tokenListVisable.Add("7", false);
                Api.Instance.tokenListBackToken.Add("7", "1");
                Api.Instance.tokenListButtonInfo["7"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "<color=purple>Sev</color>", isTogglable = false, toolTip = "Nearly everything." },
                    new ModButtonInfo { buttonText = "<color=grey>Skellon</color>", isTogglable = false, toolTip = "Asset loader." },
                };

                Api.Instance.tokenList.Add("<color=red>Update</color>", "NotNew");
                Api.Instance.tokenListVisable.Add("NotNew", false);
                Api.Instance.tokenListBackToken.Add("NotNew", "NotNew");
                Api.Instance.tokenListButtonInfo["NotNew"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "<color=red>Please Update</color>", isTogglable = false, toolTip = "Click 'Update' to update the mod." },
                    new ModButtonInfo { buttonText = "<color=orange>Update</color>", method = () => InstallLatestMod(Config.ModDownload, ModBrowser.Instance.GetModName(Config.ModDownload)), isTogglable = false, toolTip = "Installs the latest version of Several Bees." },
                    new ModButtonInfo { buttonText = "<color=yellow>GitHub</color>", method = () => 
                    {
                        string repoLink = Config.ModDownload;
                        int index = repoLink.IndexOf("/releases/");
                        if (index != -1)
                        repoLink = repoLink.Substring(0, index);

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = repoLink,
                            UseShellExecute = true
                        });
                    }, isTogglable = false, toolTip = "Opens the Several Bees GitHub." },
                };

                Api.Instance.tokenList.Add("General Settings", "8");
                Api.Instance.tokenListVisable.Add("8", false);
                Api.Instance.tokenListBackToken.Add("8", "1");
                Api.Instance.tokenListButtonInfo["8"] = new List<ModButtonInfo>
                {
                    new ModButtonInfo { disableMethod = () => PlayerPrefs.SetInt("SBSoundEffects", 0), enableMethod = () => PlayerPrefs.SetInt("SBSoundEffects", 1), enabled = (PlayerPrefs.GetInt("SBSoundEffects", 1) == 1 ? true : false), buttonText = "Sound Effects", isTogglable = true, toolTip = "Toggles sound effects such as button clicks." },
                    new ModButtonInfo { disableMethod = () => PlayerPrefs.SetInt("SBAnimations", 0), enableMethod = () => PlayerPrefs.SetInt("SBAnimations", 1), enabled = (PlayerPrefs.GetInt("SBAnimations", 1) == 1 ? true : false), buttonText = "Animations", isTogglable = true, toolTip = "Toggles animations for Mod Machines spawning in and de-spawning." },
                };

                Settings.Load();
                Settings.SetButtonNames();
                RefreshModsList();
                RefreshConfigEditor();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[Several Bees] Error in Making Settings Menu (Start SeveralBees.SeveralBees): " + e.Message);
            }
        }

        public void RefreshConfigEditor()
        {
            Api.Instance.tokenListButtonInfo["6"] = new List<ModButtonInfo>
            {
                new ModButtonInfo
                {
                    buttonText = "<color=red>Loading...</color>",
                    toolTip = "Please wait while we load the mod config catalog. Time may vary depending on your disk speed."
                },
            };

            List<ModButtonInfo> Buttons = new List<ModButtonInfo>();

            Buttons.Add(new ModButtonInfo
            {
                buttonText = "<color=red>Refresh</color>",
                isTogglable = false,
                toolTip = $"Refreshes the config catalog.",
                method = () => RefreshConfigEditor()
            });

            string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "config");
            if (!Directory.Exists(configFolder))
            {
                Api.Instance.tokenListButtonInfo["6"] = Buttons;
                return;
            }

            foreach (var filePath in Directory.GetFiles(configFolder, "*.cfg", SearchOption.TopDirectoryOnly))
            {
                string pluginName = Path.GetFileNameWithoutExtension(filePath);

                List<string> Values = new List<string>();
                List<string> Acceptables = new List<string>();
                List<string> Description = new List<string>();
                List<string> Names = new List<string>();

                string currentSection = "";
                List<string> commentBuffer = new List<string>();

                foreach (var line in File.ReadAllLines(filePath))
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        currentSection = trimmed.Substring(1, trimmed.Length - 2);
                        commentBuffer.Clear();
                        continue;
                    }

                    if (trimmed.StartsWith("#") || trimmed.StartsWith("##"))
                    {
                        commentBuffer.Add(trimmed);
                        continue;
                    }

                    int eqIndex = trimmed.IndexOf('=');
                    if (eqIndex < 0) continue;

                    string key = trimmed.Substring(0, eqIndex).Trim();
                    string value = trimmed.Substring(eqIndex + 1).Trim();
                    string description = "";
                    string acceptable = "";

                    foreach (var comment in commentBuffer)
                    {
                        string c = comment.TrimStart('#', ' ').Trim();

                        if (c.StartsWith("Acceptable values:", StringComparison.OrdinalIgnoreCase))
                            acceptable = c.Substring("Acceptable values:".Length).Trim();
                        else if (c.StartsWith("Setting type:", StringComparison.OrdinalIgnoreCase))
                            continue;
                        else if (c.StartsWith("Default value:", StringComparison.OrdinalIgnoreCase))
                            continue;
                        else
                            description += c + " ";
                    }

                    description = description.Trim();
                    commentBuffer.Clear();

                    Values.Add(value);
                    Acceptables.Add(acceptable);
                    Description.Add(description);
                    Names.Add(currentSection);
                }

                Buttons.Add(new ModButtonInfo
                {
                    buttonText = pluginName,
                    isTogglable = false,
                    toolTip = $"Opens the config for {pluginName}.",
                    method = () => OpenModConfigPage(pluginName, filePath, Values, Acceptables, Description, Names)
                });
            }

            Api.Instance.tokenListButtonInfo["6"] = Buttons;
        }

        internal void OpenModConfigPage(string ModName, string MakeShiftToken, List<string> Values, List<string> Acceptables, List<string> Description, List<string> Names)
        {
            List<ModButtonInfo> Buttons = new List<ModButtonInfo>();
            Buttons.Add(new ModButtonInfo
            {
                buttonText = $"<color=grey>----------</color>",
                isTogglable = false,
                toolTip = $""
            });

            for (int r = 0; r < Values.Count; r++)
            {
                if (string.IsNullOrEmpty(Acceptables[r]) || !Acceptables[r].Contains(Values[r])) continue;
                Buttons.Add(new ModButtonInfo
                {
                    buttonText = Names[r],
                    isTogglable = false,
                    toolTip = $"The name of this config."
                });

                Buttons.Add(new ModButtonInfo
                {
                    buttonText = $"Description (Click)",
                    isTogglable = false,
                    toolTip = $"{Description[r]}"
                });

                string NewValue = Values[r];
                int NewValueNum = 0;

                NewValueNum = Acceptables[r].Split(", ").ToList().IndexOf(Values[r]);
                NewValueNum++;
                if (NewValueNum >= Acceptables[r].Split(", ").Length)
                {
                    NewValueNum = 0;
                }
                NewValue = Acceptables[r].Split(", ")[NewValueNum];

                Buttons.Add(new ModButtonInfo
                {
                    buttonText = $"Value: {Values[r]}",
                    isTogglable = false,
                    toolTip = $"The current setting of the config."
                });

                Buttons.Add(new ModButtonInfo
                {
                    buttonText = $"<color=grey>----------</color>",
                    isTogglable = false,
                    toolTip = $""
                });
            }
            Api.Instance.tokenList[ModName] = MakeShiftToken;
            Api.Instance.tokenListVisable[MakeShiftToken] = false;
            Api.Instance.tokenListBackToken[MakeShiftToken] = "6";
            Api.Instance.tokenListButtonInfo[MakeShiftToken] = Buttons;

            Api.Instance.OpenMenu(MakeShiftToken);
        }

        internal AudioClip GetLoadedSound(string fileLink)
        {
            if (string.IsNullOrEmpty(fileLink)) return null;
            if (Plugin.Instance.LoadedSounds.TryGetValue(fileLink, out AudioClip clip)) return clip;
            return null;
        }

        internal void PlaySound(string Url, float Volume = 0.4f)
        {
            if(!Api.Instance.GrabButton("8", "Sound Effects").enabled) return;
            AudioClip Clip = GetLoadedSound(Url);
            GameObject soundObject = new GameObject("Sev Essence Sound Player");
            AudioSource Player = soundObject.AddComponent<AudioSource>();
            Player.clip = Clip;
            Player.volume = Volume;
            Player.Play();
            GameObject.Destroy(soundObject, Player.clip.length);
        }

        public void RefreshModsList()
        {
            Api.Instance.tokenListButtonInfo["5"] = new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "<color=red>Loading...</color>", toolTip = "Please wait why we load the mod catalog. Time may varey depending on your internet connection." },
            };


            ModBrowser.Instance.GetMods(mods =>
            {
                List<ModButtonInfo> Buttons = new List<ModButtonInfo>();

                Buttons.Add(new ModButtonInfo
                {
                    buttonText = "<color=orange>Search</color>",
                    isTogglable = false,
                    toolTip = $"Lets you search the mod catalog."
                });
                Buttons.Add(new ModButtonInfo
                {
                    buttonText = "<color=red>Refresh</color>",
                    isTogglable = false,
                    toolTip = $"Refreshes the mod catalog.",
                    method = () => RefreshModsList()
                });

                foreach (var mod in mods)
                {
                    string modName = mod.Key;
                    string modLink = mod.Value;
                    Buttons.Add(new ModButtonInfo
                    {
                        buttonText = modName,
                        isTogglable = false,
                        toolTip = $"Opens the config info page for the '{modName}' mod.",
                        method = () => OpenCodeModPage(modName, modLink)
                    });
                }

                Api.Instance.tokenListButtonInfo["5"] = Buttons;

            });
        }

        internal void OpenCodeModPage(string ModName, string ModLink)
        {
            List<ModButtonInfo> Buttons = new List<ModButtonInfo>();

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", ModName + ".dll");
            if (File.Exists(path))
            {
                Buttons.Add(new ModButtonInfo
                {
                    buttonText = "<color=red>Uninstall</color>",
                    isTogglable = false,
                    toolTip = $"Uninstalls {ModName}.",
                    method = () => UninstallMod(ModName, ModLink)
                });
            }
            else
            {
                Buttons.Add(new ModButtonInfo
                {
                    buttonText = "<color=green>Install</color>",
                    isTogglable = false,
                    toolTip = $"Installs {ModName}.",
                    method = () => InstallMod(ModLink, ModName)
                });
            }
            Buttons.Add(new ModButtonInfo
            {
                buttonText = "<color=orange>Install Latest</color>",
                isTogglable = false,
                toolTip = $"Installs the latest version of {ModName}.",
                method = () => InstallLatestMod(ModLink, ModName)
            });

            Buttons.Add(new ModButtonInfo
            {
                buttonText = "Open GitHub",
                isTogglable = false,
                toolTip = $"Opens the GitHub for {ModName}.",
                method = () =>
                {
                    string repoLink = ModLink;
                    int index = repoLink.IndexOf("/releases/");
                    if (index != -1)
                        repoLink = repoLink.Substring(0, index);

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = repoLink,
                        UseShellExecute = true
                    });
                }
            });

            Api.Instance.tokenList[ModName] = ModLink;
            Api.Instance.tokenListVisable[ModLink] = false;
            Api.Instance.tokenListBackToken[ModLink] = "5";
            Api.Instance.tokenListButtonInfo[ModLink] = Buttons;

            Api.Instance.OpenMenu(ModLink);
        }

        internal void InstallMod(string ModLink, string ModName)
        {
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);

            string tempFile = Path.Combine(Path.GetTempPath(), ModName + ".dll");

            using (var client = new System.Net.WebClient())
            {
                client.DownloadProgressChanged += (s, e) => Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>Installing... " + e.ProgressPercentage + "%</color>", toolTip = "Please wait.." } };
                client.DownloadFileCompleted += (s, e) => Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=green>" + "Install Complete" + "</color>", toolTip = "Please wait.." } };
                client.DownloadFileAsync(new Uri(ModLink), tempFile);
                while (client.IsBusy) Thread.Sleep(100);
            }

            string destFile = Path.Combine(pluginsPath, ModName + ".dll");
            if (File.Exists(destFile)) File.Delete(destFile);
            File.Move(tempFile, destFile);

            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>" + "Restarting game..." + "</color>", toolTip = "Please wait.." } };

            RestartApp();
        }

        internal void UninstallMod(string ModName, string ModLink)
        {
            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>" + "Uninstalling..." + "</color>", toolTip = "Please wait.." } };
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", ModName + ".dll");
            if (!File.Exists(dllPath))
            {
                Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=red>" + "Mod Not Found." + "</color>", toolTip = "Please wait.." } };
                return;
            }

            string deletePath = dllPath + ".delete";
            if (File.Exists(deletePath)) File.Delete(deletePath);
            File.Move(dllPath, deletePath);

            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=green>" + "Uninstall Complete" + "</color>", toolTip = "Please wait.." } };
            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>" + "Restarting game..." + "</color>", toolTip = "Please wait.." } };

            RestartApp();
        }

        internal void InstallLatestMod(string ModLink, string ModName)
        {
            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>" + "Uninstalling..." + "</color>", toolTip = "Please wait.." } };
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins", ModName + ".dll");
            if (!File.Exists(dllPath))
            {
                Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=red>" + "Mod Not Found." + "</color>", toolTip = "Please wait.." } };
                return;
            }

            string deletePath = dllPath + ".delete";
            if (File.Exists(deletePath)) File.Delete(deletePath);
            File.Move(dllPath, deletePath);

            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=green>" + "Uninstall Complete" + "</color>", toolTip = "Please wait.." } };

            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "plugins");
            if (!Directory.Exists(pluginsPath)) Directory.CreateDirectory(pluginsPath);

            string tempFile = Path.Combine(Path.GetTempPath(), ModName + ".dll");

            using (var client = new System.Net.WebClient())
            {
                client.DownloadProgressChanged += (s, e) => Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>Installing... " + e.ProgressPercentage + "%</color>", toolTip = "Please wait.." } };
                client.DownloadFileCompleted += (s, e) => Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=green>" + "Install Complete" + "</color>", toolTip = "Please wait.." } };
                client.DownloadFileAsync(new Uri(ModLink), tempFile);
                while (client.IsBusy) Thread.Sleep(100);
            }

            string destFile = Path.Combine(pluginsPath, ModName + ".dll");
            if (File.Exists(destFile)) File.Delete(destFile);
            File.Move(tempFile, destFile);

            Api.Instance.tokenListButtonInfo[ModLink] = new List<ModButtonInfo> { new ModButtonInfo { buttonText = "<color=orange>" + "Restarting game..." + "</color>", toolTip = "Please wait.." } };

            RestartApp();
        }

        internal void RestartApp()
        {
            string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx", "config");
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            string restartScript = $@"@echo off
title Several Bees - Restarting...
echo Restarting, please wait...
echo.

:WAIT_LOOP
tasklist /FI ""IMAGENAME eq {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)}"" | find /I ""{Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)}"" >nul
if %ERRORLEVEL%==0 (
    timeout /t 1 >nul
    goto WAIT_LOOP
)

start steam://run/{Config.SteamAppId}
exit";

            string fileName = Path.Combine(configDir, "sb_restart.bat");
            File.WriteAllText(fileName, restartScript);

            Process.Start(fileName);
            Application.Quit();
        }

        private float lastSpawnTime;
        internal void SpawnMachineAtPlayer()
        {
            bool ShoudExist = false;
            Vector3 RelPosMach = Config.machineRelSpawn();
            foreach (GameObject objthingponr in ModMangerDistanceIndicators)
            {
                ShoudExist = Vector3.Distance(RelPosMach, objthingponr.transform.position) > Config.machineSpawnClearance;
            }
            if (!ShoudExist) return;

            if (Time.time - lastSpawnTime < 2f) return;
            lastSpawnTime = Time.time;

            PlaySound("https://github.com/sevvy-wevvy/Several-Bees/raw/refs/heads/main/Resources/Mod/open.wav");

            GameObject machine = InstanceModManger();
            machine.transform.position = RelPosMach;
            machine.transform.LookAt(Config.BodyReference());
            machine.transform.Rotate(0, 180, 0);
            machine.AddComponent<MachineDespawn>().DespawnDistance = Config.MachineDespawnDistance;

            if (Api.Instance.GrabButton("8", "Animations").enabled) StartCoroutine(MachineAnn(machine));
        }

        private IEnumerator MachineAnn(GameObject machine)
        {
            Vector3 OgScale = machine.transform.localScale;
            machine.transform.localScale = Vector3.zero;

            float time = 0f;
            float duration = 0.25f;
            Vector3 overshootScale = OgScale * 1.1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                machine.transform.localScale = Vector3.Lerp(Vector3.zero, overshootScale, t);
                yield return null;
            }

            time = 0f;
            duration = 0.1f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                machine.transform.localScale = Vector3.Lerp(overshootScale, OgScale, t);
                yield return null;
            }

            machine.transform.localScale = OgScale;
        }

        public List<Things> GetThings()
        {
            List<Things> things = new List<Things>();

            if (SectionName == "Main")
            {
                foreach (var key in Api.Instance.tokenList)
                {
                    if (Api.Instance.tokenListVisable[key.Value] == true) things.Add(new Things { Name = key.Key, Enterable = true, Token = key.Value });
                }
            }
            else if (SectionName == "NotNew")
            {
                foreach (ModButtonInfo mbi in Api.Instance.tokenListButtonInfo[SectionName])
                {
                    string ButtonName = mbi.buttonText;
                    if (mbi.isTogglable)
                    {
                        if (mbi.enabled)
                        {
                            ButtonName = "<color=green>" + ButtonName + " [ON]</color>";
                        }
                        else
                        {
                            ButtonName = "<color=red>" + ButtonName + " [OFF]</color>";
                        }
                    }
                    things.Add(new Things { Name = ButtonName, Enterable = false, Token = SectionName, mbi = mbi });
                }
            }
            else
            {
                string backTarget = "Main";
                try
                {
                    backTarget = Api.Instance.tokenListBackToken[SectionName];
                }
                catch (Exception e)
                {
                    ListError("Error in Getting Back Target Token: " + e.Message + $" [{e.StackTrace}]");
                }
                things.Add(new Things { Name = "<color=red>Back</color>", Enterable = true, Token = backTarget });

                foreach (ModButtonInfo mbi in Api.Instance.tokenListButtonInfo[SectionName])
                {
                    string ButtonName = mbi.buttonText;
                    if (mbi.isTogglable)
                    {
                        if (mbi.enabled)
                        {
                            ButtonName = "<color=green>" + ButtonName + " [ON]</color>";
                        }
                        else
                        {
                            ButtonName = "<color=red>" + ButtonName + " [OFF]</color>";
                        }
                    }
                    things.Add(new Things { Name = ButtonName, Enterable = false, Token = SectionName, mbi = mbi });
                }
            }

            return things;
        }


        private bool DownArrowPress = false;
        private bool UpArrowPress = false;
        private bool EnterPress = false;
        private bool TestModeDone = false;
        private bool SpawnNewThingPress = false;
        public string TestMod1Token = "";
        public string TestMod2Token = "";
        public string TestMod3Token = "";

        public Color Theme1 = new Color(0.5f, 0f, 1f);
        public Color Theme2 = Color.black;
        public float ThemeFadeSpeed = 0.2f;

        internal int PointerPositionIndex = 0;
        private int MaxPointerPosition = 0;
        internal string SectionName = "Main";
        private bool GuiButtonPress = false;

        private Vector3 previousLeftPos;
        private Vector3 previousRightPos;

        private void Update()
        {
            for (int i = ModMangerDistanceIndicators.Count - 1; i >= 0; i--)
            {
                if (ModMangerDistanceIndicators[i] == null) ModMangerDistanceIndicators.RemoveAt(i);
            }
            for (int i = ModMangerTextList.Count - 1; i >= 0; i--)
            {
                if (ModMangerTextList[i] == null) ModMangerTextList.RemoveAt(i);
            }

            if (UnityInput.Current.GetKey(KeyCode.M) && !SpawnNewThingPress)
            {
                SpawnMachineAtPlayer();
                SpawnNewThingPress = true;
            }
            if(!UnityInput.Current.GetKey(KeyCode.M))
            {
                SpawnNewThingPress = false;
            }

            try
            {
                // if you can figure out how to make this better PLEASE make a pull reqeust!
                float distance = Vector3.Distance(Config.LeftHandReference().position, Config.RightHandReference().position);

                if (distance < Config.gripThreshold * 1.25f && Config.RightGripDown() && Config.LeftGripDown())
                {
                    float previousDistance = Vector3.Distance(previousLeftPos, previousRightPos);
                    float distanceDelta = distance - previousDistance;

                    Vector3 leftVelocity = (Config.LeftHandReference().position - previousLeftPos) / Mathf.Max(Time.deltaTime, 0.0001f);
                    Vector3 rightVelocity = (Config.RightHandReference().position - previousRightPos) / Mathf.Max(Time.deltaTime, 0.0001f);

                    Vector3 pullDir = (Config.RightHandReference().position - Config.LeftHandReference().position).normalized;
                    float relativeSpeed = Vector3.Dot(pullDir, rightVelocity - leftVelocity);

                    if (relativeSpeed > Config.pullSpeedThreshold * 0.6f && distanceDelta > Config.minPullDistance * 0.5f)
                    {
                        SpawnMachineAtPlayer();
                        UnityEngine.Debug.Log("[Several Bees] Spawning New Machine");
                    }
                }

                previousLeftPos = Vector3.Lerp(previousLeftPos, Config.LeftHandReference().position, 0.5f);
                previousRightPos = Vector3.Lerp(previousRightPos, Config.RightHandReference().position, 0.5f);
            }
            catch { }

            if (ObjectColliders[0].isTrigger != Config.MachineHasColliders())
            {
                foreach(Collider col in ObjectColliders)
                {
                    col.isTrigger = Config.MachineHasColliders();
                }
            }
            if (UnityInput.Current.GetKey(KeyCode.T) && UnityInput.Current.GetKey(KeyCode.E) && UnityInput.Current.GetKey(KeyCode.S))
            {
                TestMode = true;
            }

            bool TempPcActive = false;
            foreach (GameObject objthingponr in ModMangerDistanceIndicators)
            {
                TempPcActive = Vector3.Distance(Config.BodyReference().position, objthingponr.transform.position) < Config.MaxKeyboardControllsDisctance;
            }
            PCControlActive = TempPcActive;

            if (Config.IsGui)
            {
                if (UnityInput.Current.GetKey(KeyCode.BackQuote))
                {
                    if (GuiButtonPress == false)
                    {
                        ShowGUIMenu = !ShowGUIMenu;
                        GuiButtonPress = true;
                    }
                }
                if (!UnityInput.Current.GetKey(KeyCode.BackQuote))
                {
                    GuiButtonPress = false;
                }
            }

            if (PCControlActive)
            {
                if (UnityInput.Current.GetKey(KeyCode.DownArrow))
                {
                    if (DownArrowPress == false)
                    {
                        MmDown(false);
                        DownArrowPress = true;
                    }
                }
                if (!UnityInput.Current.GetKey(KeyCode.DownArrow))
                {
                    DownArrowPress = false;
                }


                if (UnityInput.Current.GetKey(KeyCode.UpArrow))
                {
                    if (UpArrowPress == false)
                    {
                        MmUp(false);
                        UpArrowPress = true;
                    }
                }
                if (!UnityInput.Current.GetKey(KeyCode.UpArrow))
                {
                    UpArrowPress = false;
                }


                if (UnityInput.Current.GetKey(KeyCode.Return))
                {
                    if (EnterPress == false)
                    {
                        MmSelect(false);
                        EnterPress = true;
                    }
                }


                if (UnityInput.Current.GetKey(KeyCode.RightArrow))
                {
                    if (EnterPress == false)
                    {
                        MmSelect(true);
                        EnterPress = true;
                    }
                }
                if (!UnityInput.Current.GetKey(KeyCode.RightArrow) && !UnityInput.Current.GetKey(KeyCode.Return))
                {
                    EnterPress = false;
                }
            }

            if (!IsLatestVersion)
            {
                SectionName = "NotNew";
            }

            if (TestMode && !TestModeDone)
            {
                TestMod1Token = Api.Instance.GenerateToken("Test Mod 1");
                TestMod2Token = Api.Instance.GenerateToken("Test Mod 2");
                TestMod3Token = Api.Instance.GenerateToken("Alot Of Stuff");

                Api.Instance.SetButtonInfo(
                    TestMod1Token,
                    new List<ModButtonInfo>
                    {
                        new ModButtonInfo { buttonText = "Test1 in 1" }
                    }
                );

                var buttons = new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = "Toggle", isTogglable = true }
                };

                for (int i = 1; i <= 300; i++)
                {
                    buttons.Add(new ModButtonInfo { buttonText = "Toggle " + i, isTogglable = true });
                }

                Api.Instance.SetButtonInfo(
                    TestMod3Token,
                    buttons
                );

                Config.IsGui = true;


                TestModeDone = true;
            }

            string TextForThings = "";

            TextForThings = Extra.GradientText("Several Bees", Theme1, Theme2, ThemeFadeSpeed);
            string SectionName2 = SectionName;
            if (Api.Instance.tokenList.ContainsValue(SectionName)) SectionName2 = Api.Instance.tokenList.FirstOrDefault(kvp => kvp.Value == SectionName).Key;
            TextForThings += $"\n<color=grey>---</color> <size=0.35>{SectionName2}</size> <color=grey>---</color>\n <size=0.3>";

            int pointerindex = 0;
            List<Things> things = GetThings();

            int total = things.Count;
            int windowSize = 4;
            int startIndex = PointerPositionIndex - 3;
            if (startIndex < 0) startIndex = 0;
            if (startIndex + windowSize > total) startIndex = total - windowSize;
            if (startIndex < 0) startIndex = 0;

            if (startIndex > 0)
            {
                TextForThings += "\n</size><size=0.1>••••••</size><size=0.3>";
            }

            pointerindex = 0;
            for (int i = startIndex; i < startIndex + windowSize && i < total; i++)
            {
                Things thing = things[i];
                if(thing.mbi != null) if(thing.mbi.buttonOverlayText != null)
                    {
                        thing.Name = thing.mbi.buttonOverlayText;
                    }
                TextForThings += (i == PointerPositionIndex ? $"\n<color=#{ColorUtility.ToHtmlStringRGB(Theme1)}>> </color>" : "\n") + $"{thing.Name}";
                pointerindex++;
            }

            if (startIndex + windowSize < total)
            {
                TextForThings += "\n</size><size=0.1>••••••</size><size=0.3>";
            }

            MaxPointerPosition = things.Count;
            TextForThings += $"</size>";
            if(!string.IsNullOrEmpty(ToolTipText))
            {
                TextForThings += $"\n \n<size=0.2>{ToolTipText}</size>";
            }

            foreach(TextMeshPro tmp in ModMangerTextList)
            {
                tmp.text = TextForThings;
            }

            try
            {
                foreach (string token in Api.Instance.tokenList.Values)
                {
                    foreach (ModButtonInfo mbi in Api.Instance.tokenListButtonInfo[token])
                    {
                        if(mbi.enabled && mbi.isTogglable)
                        {
                            mbi.method?.Invoke();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ListError("Error in Update ModButtonInfo Check: " + e.Message + $" [{e.StackTrace}]");
            }
        }

        internal bool PCControlActive = false;
        internal bool ShowGUIMenu = false;

        private GUIStyle gradientStyle;
        private Vector2 scrollPosition;
        private Rect menuRect = new Rect(10, 50, 400, 300);
        private bool dragging = false;
        private Vector2 dragOffset;

        private void OnGUI()
        {
            if (gradientStyle == null)
            {
                gradientStyle = new GUIStyle(GUI.skin.label);
                gradientStyle.richText = true;
                gradientStyle.fontSize = 20;
                gradientStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (PCControlActive)
            {
                Rect rect = new Rect(10, Screen.height - 30, 400, 30);
                string scrollingText = "<b>" + Extra.GradientText("SB PC Control Active", Theme1, Theme2, ThemeFadeSpeed) + "</b>";
                GUI.Label(rect, scrollingText, gradientStyle);
            }

            if (ShowGUIMenu)
            {
                Event e = Event.current;

                if (e.type == EventType.MouseDown && menuRect.Contains(e.mousePosition))
                {
                    dragging = true;
                    dragOffset = new Vector2(e.mousePosition.x - menuRect.x, e.mousePosition.y - menuRect.y);
                }
                if (dragging)
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        menuRect.position = e.mousePosition - dragOffset;
                    }
                    if (e.type == EventType.MouseUp)
                    {
                        dragging = false;
                    }
                }

                Color prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.1f);
                GUI.Box(menuRect, "");
                GUI.color = prevColor;

                GUILayout.BeginArea(menuRect);

                float tooltipHeight = 40;

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none, GUIStyle.none,
                    GUILayout.Width(menuRect.width), GUILayout.Height(menuRect.height - tooltipHeight));

                GUILayout.Label(Extra.GradientText("Several Bees", Theme1, Theme2, ThemeFadeSpeed) + "\n<size=8>` To Toggle</size>", gradientStyle);

                string SectionName2 = SectionName;
                if (Api.Instance.tokenList.ContainsValue(SectionName)) SectionName2 = Api.Instance.tokenList.FirstOrDefault(kvp => kvp.Value == SectionName).Key;
                GUILayout.Label($"<color=grey>---</color> <size=14>" + SectionName2 + "</size> <color=grey>---</color>", gradientStyle);

                List<Things> things = GetThings();
                for (int i = 0; i < things.Count; i++)
                {
                    Things thing = things[i];

                    if (thing.mbi != null && thing.mbi.buttonOverlayText != null)
                        thing.Name = thing.mbi.buttonOverlayText;

                    if (GUILayout.Button(thing.Name, gradientStyle))
                    {
                        PointerPositionIndex = i;
                        MmSelect(true);
                    }
                }

                GUILayout.EndScrollView();

                if (!string.IsNullOrEmpty(ToolTipText))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"<size=12>" + ToolTipText + "</size>", gradientStyle, GUILayout.Height(tooltipHeight));
                }

                GUILayout.EndArea();
            }
        }
    }

    public class Things
    {
        public string Name = null;
        public bool Enterable = false;
        public string Token = null;
        public ModButtonInfo mbi = null;
    }

    public class DetailedColor
    {
        public Color color = Color.white;
        public string name = "null";
    }
}
