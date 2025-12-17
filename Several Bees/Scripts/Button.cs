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

namespace SeveralBees.Scripts
{
    public class Button : MonoBehaviour
    {
        public Action<bool> Click;
        public string Name;

        public float Debounce = 0.25f;
        private static float LastPress;

        private void Awake()
        {
            UnityEngine.Debug.Log($"[Several Bees] Button '{Name}' Awake");
            gameObject.layer = LayerMask.NameToLayer(Config.ButtonLayer);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (Config.LeftButtonClicked(col) || Config.RightButtonClicked(col) && Time.time > LastPress + Debounce)
            {
                LastPress = Time.time;
                UnityEngine.Debug.Log($"[Several Bees] Button '{Name}' Clicked");
                Click?.Invoke(Config.LeftButtonClicked(col));
            }
        }
    }
}
