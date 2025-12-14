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

using SeveralBees;
using System.Collections.Generic;
using UnityEngine;

namespace SeveralBees
{
    public class Settings : MonoBehaviour
    {
        public static string GetColorName(Color color, bool ColorText = true)
        {
            foreach (var detailed in SeveralBeesCore.Instance.CycleColors)
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
            Api.Instance.SetButtonNickname("2", "cfs", $"Change Fade Speed (Current: {SeveralBeesCore.Instance.ThemeFadeSpeed:F2})");
            Api.Instance.SetButtonNickname("2", "nfc", $"Theme First Color (Current: {GetColorName(SeveralBeesCore.Instance.Theme1)})");
            Api.Instance.SetButtonNickname("2", "nsc", $"Theme Second Color (Current: {GetColorName(SeveralBeesCore.Instance.Theme2)})");
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat("SeveralBees_ColorFadeSpeed", SeveralBeesCore.Instance.ThemeFadeSpeed);
            PlayerPrefs.SetString("SeveralBees_TitleColorStart", ColorUtility.ToHtmlStringRGB(SeveralBeesCore.Instance.Theme1));
            PlayerPrefs.SetString("SeveralBees_TitleColorEnd", ColorUtility.ToHtmlStringRGB(SeveralBeesCore.Instance.Theme2));
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            SeveralBeesCore.Instance.ThemeFadeSpeed = PlayerPrefs.GetFloat("SeveralBees_ColorFadeSpeed", SeveralBeesCore.Instance.ThemeFadeSpeed);
            SeveralBeesCore.Instance.Theme1 = StringToColor(PlayerPrefs.GetString("SeveralBees_TitleColorStart", ColorUtility.ToHtmlStringRGB(SeveralBeesCore.Instance.Theme1)));
            SeveralBeesCore.Instance.Theme2 = StringToColor(PlayerPrefs.GetString("SeveralBees_TitleColorEnd", ColorUtility.ToHtmlStringRGB(SeveralBeesCore.Instance.Theme2)));
        }

        public static void cfs()
        {
            List<float> Floats = SeveralBeesCore.Instance.CycleFloats;
            int currentIndex = Floats.IndexOf(SeveralBeesCore.Instance.ThemeFadeSpeed);
            int nextIndex = (currentIndex + 1) % Floats.Count;
            SeveralBeesCore.Instance.ThemeFadeSpeed = Floats[nextIndex];
            Save();
            SetButtonNames();
            SeveralBeesCore.Instance.ReMakeModManger();
        }

        public static void nfc()
        {
            List<DetailedColor> Colors = SeveralBeesCore.Instance.CycleColors;
            int currentIndex = Colors.FindIndex(c => c.color == SeveralBeesCore.Instance.Theme1);
            int nextIndex = (currentIndex + 1) % Colors.Count;
            SeveralBeesCore.Instance.Theme1 = Colors[nextIndex].color;
            Save();
            SetButtonNames();
            SeveralBeesCore.Instance.ReMakeModManger();
        }

        public static void nsc()
        {
            List<DetailedColor> Colors = SeveralBeesCore.Instance.CycleColors;
            int currentIndex = Colors.FindIndex(c => c.color == SeveralBeesCore.Instance.Theme2);
            int nextIndex = (currentIndex + 1) % Colors.Count;
            SeveralBeesCore.Instance.Theme2 = Colors[nextIndex].color;
            Save();
            SetButtonNames();
            SeveralBeesCore.Instance.ReMakeModManger();
        }
    }
}