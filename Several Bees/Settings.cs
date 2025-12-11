using SeveralBees;
using System.Collections.Generic;
using UnityEngine;

namespace SeveralBees
{
    public class Settings : MonoBehaviour
    {
        public static string GetColorName(Color color, bool ColorText = true)
        {
            foreach (var detailed in SeveralBees.Instance.CycleColors)
            {
                if (detailed.color == color)
                    return ColorText ? $"<color=#{ColorUtility.ToHtmlStringRGB(detailed.color)}>" + detailed.name + "</color>" : detailed.name;
            }
            return "Custom";
        }

        public static Color StringToColor(string colorString)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString($"#{colorString}", out color))
            {
                return color;
            }
            return Color.white;
        }
        public static void SetButtonNames()
        {
            Api.Instance.SetButtonNickname("2", "cfs", $"Change Fade Speed (Current: {SeveralBees.Instance.ThemeFadeSpeed:F2})");
            Api.Instance.SetButtonNickname("2", "nfc", $"Theme First Color (Current: {GetColorName(SeveralBees.Instance.Theme1)})");
            Api.Instance.SetButtonNickname("2", "nsc", $"Theme Second Color (Current: {GetColorName(SeveralBees.Instance.Theme2)})");
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("SeveralBees_ColorFadeSpeed", SeveralBees.Instance.ThemeFadeSpeed);
            PlayerPrefs.SetString("SeveralBees_TitleColorStart", ColorUtility.ToHtmlStringRGB(SeveralBees.Instance.Theme1));
            PlayerPrefs.SetString("SeveralBees_TitleColorEnd", ColorUtility.ToHtmlStringRGB(SeveralBees.Instance.Theme2));
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            SeveralBees.Instance.ThemeFadeSpeed = PlayerPrefs.GetFloat("SeveralBees_ColorFadeSpeed", SeveralBees.Instance.ThemeFadeSpeed);
            SeveralBees.Instance.Theme1 = StringToColor(PlayerPrefs.GetString("SeveralBees_TitleColorStart", ColorUtility.ToHtmlStringRGB(SeveralBees.Instance.Theme1)));
            SeveralBees.Instance.Theme2 = StringToColor(PlayerPrefs.GetString("SeveralBees_TitleColorEnd", ColorUtility.ToHtmlStringRGB(SeveralBees.Instance.Theme2)));
        }

        public static void cfs()
        {
            List<float> Floats = SeveralBees.Instance.CycleFloats;
            int currentIndex = Floats.IndexOf(SeveralBees.Instance.ThemeFadeSpeed);
            int nextIndex = (currentIndex + 1) % Floats.Count;
            SeveralBees.Instance.ThemeFadeSpeed = Floats[nextIndex];
            Save();
            SetButtonNames();
            SeveralBees.Instance.ReMakeModManger();
        }

        public static void nfc()
        {
            List<DetailedColor> Colors = SeveralBees.Instance.CycleColors;
            int currentIndex = Colors.FindIndex(c => c.color == SeveralBees.Instance.Theme1);
            int nextIndex = (currentIndex + 1) % Colors.Count;
            SeveralBees.Instance.Theme1 = Colors[nextIndex].color;
            Save();
            SetButtonNames();
            SeveralBees.Instance.ReMakeModManger();
        }

        public static void nsc()
        {
            List<DetailedColor> Colors = SeveralBees.Instance.CycleColors;
            int currentIndex = Colors.FindIndex(c => c.color == SeveralBees.Instance.Theme2);
            int nextIndex = (currentIndex + 1) % Colors.Count;
            SeveralBees.Instance.Theme2 = Colors[nextIndex].color;
            Save();
            SetButtonNames();
            SeveralBees.Instance.ReMakeModManger();
        }
    }
}