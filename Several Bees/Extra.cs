using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using SeveralBees;

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
            SeveralBees.Instance.Theme1 = nm1;
            SeveralBees.Instance.Theme2 = nm2;
            if(fadeSpeed != -8275892375)
            {
                SeveralBees.Instance.ThemeFadeSpeed = fadeSpeed;
            }
            Settings.SetButtonNames();
            Settings.Save();
            SeveralBees.Instance.ReMakeModManger();
        }

        public AssetBundle LoadAssetBundle(string path)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            stream.Close();
            return bundle;
        }
    }
}
