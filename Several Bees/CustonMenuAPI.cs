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
using System;
using Random = System.Random;
using System.Reflection;
using System.Linq;

namespace SeveralBees
{
    public class CustonMenuAPI : MonoBehaviour
    {
        public static CustonMenuAPI Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[Several Bees] Custon Menu API Awake");
            Instance = this;
            UnityEngine.Debug.Log("[Several Bees] Custon Menu API Instance Set");
        }

        /*-------------------------------------------------------------------------------------------------------------------*/








        public List<string> GetButtons()
        {
            return SeveralBeesCore.Instance.GetButtons();
        }

        public void ClickButton(int Button)
        {
            SeveralBeesCore.Instance.PointerPositionIndex = Button;
            SeveralBeesCore.Instance.MmSelect(false);
        }

        public int GetCurrentButton()
        {
            return SeveralBeesCore.Instance.PointerPositionIndex;
        }

        public void SetCurrentButton(int Button)
        {
            SeveralBeesCore.Instance.PointerPositionIndex = Button;
        }

        public void ClickCurrentButton()
        {
            SeveralBeesCore.Instance.MmSelect(false);
        }

        public GameObject InstanceModManger()
        {
            return SeveralBeesCore.Instance.InstanceModManger();
        }

        public string GetTabName()
        {
            string SectionName2 = SeveralBeesCore.Instance.SectionName;
            if (Api.Instance.tokenList.ContainsValue(SeveralBeesCore.Instance.SectionName)) SectionName2 = Api.Instance.tokenList.FirstOrDefault(kvp => kvp.Value == SeveralBeesCore.Instance.SectionName).Key;
            
            return SectionName2;
        }
    }
}
