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
using System.Reflection;
using System.Collections;

namespace SeveralBees.Scripts
{
    public class MachineFullBackButtonHandler : MonoBehaviour
    {
        Transform SbMach = null;
        void Update()
        {
            if (SbMach == null)
            {
                SbMach = gameObject.transform.parent.Find("SbMachine(Clone)").transform;
            }
            foreach (Transform Child in SbMach)
            {
                if (Child.name == "Back")
                {
                    Child.gameObject.SetActive(Api.Instance.GrabButton("8", "Physical Back Button").enabled);
                }
                else if (Child.name == "Back1" || Child.name == "Back2")
                {
                    Child.gameObject.SetActive(Api.Instance.GrabButton("8", "Physical Back Button").enabled);
                }
            }
        }
    }
}

