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
using System.Reflection;

namespace SeveralBees
{
    internal class Config : MonoBehaviour
    {
        // tried my best to put all gorilla tag specific varibles in here, please let me know if i missed any sevvy-wevvy.com/discord






        // Where the mod catalog is pulled from, check the link for how to format it.
        internal static string ModListLink = "https://raw.githubusercontent.com/sevvy-wevvy/Several-Bees/main/mods.txt";
        // Used when restarting the game to esure BEPINEX boots properly. Go to the steam game, settings icon, properties, updates, and there you can find the app id.
        internal static string SteamAppId = "1533390";
        // Update this number along with the number in your version.txt
        internal const string CurrentModVersion = "1.4.0";
        // The txt that holds the latest version number of the mod.
        internal static string ModVersionLink = "https://raw.githubusercontent.com/sevvy-wevvy/Several-Bees/main/ver.txt";
        // Latest mod download link
        internal static string ModDownload = "https://github.com/sevvy-wevvy/Several-Bees/releases/latest/download/SeveralBees-Gtag.dll";


        // How close the hands have to be to start checking to spawn the mod machine
        internal static float gripThreshold = 0.2f;
        // How fast the hands need to move apart to spawn the mod machine
        internal static float pullSpeedThreshold = 0.1f;
        // The minimum distance the hands need to be apart to spawn the mod machine
        internal static float minPullDistance = 0.04f;
        // The position relitive to the player the machine spawns at when the jester is detected
        internal static Vector3 machineRelSpawn() { return BodyReference().position + BodyReference().forward * 0.75f; }
        // The distance away a machine has to be from another to spawn
        internal static float machineSpawnClearance = 1f;


        // Whats used when making an object visable (Use "Def" to use the defualt shader)
        internal static string PropperShader = "Universal Render Pipeline/Lit";
        // If the game uses URP, and if the fix shader is a URP shader.
        internal static bool IsURP = true;
        // What triggers everything to startup, MUST TRIGGER Plugin.Instance.CustomStart()
        internal static async void StartupTriggerThing() { GorillaTagger.OnPlayerSpawned(() => Plugin.Instance.CustomStart()); }
        // The layer for buttons interactions
        internal static string ButtonLayer = "GorillaInteractable";


        // The right hand transform (Set to "new Vector3(0, -integer.MaxValue, 0)" if its not a vr game/doesnt use hands)
        internal static Transform RightHandReference() { return GorillaTagger.Instance.rightHandTransform; }
        // The input for the right grip button
        internal static bool RightGripDown() { return ControllerInputPoller.instance.rightControllerGripFloat > 0.75f; }
        // How to check if the button was clicked by the right hand
        internal static bool RightButtonClicked(Collider Col) { return Col.TryGetComponent(out GorillaTriggerColliderHandIndicator component); }


        // The left hand transform (Set to "new Vector3(0, -integer.MaxValue, 0)" if its not a vr game/doesnt use hands)
        internal static Transform LeftHandReference() { return GorillaTagger.Instance.leftHandTransform; }
        // The input for the left grip button
        internal static bool LeftGripDown() { return ControllerInputPoller.instance.leftControllerGripFloat > 0.75f; }
        // How to check if the button was clicked by the left hand
        internal static bool LeftButtonClicked(Collider Col) { return Col.TryGetComponent(out GorillaTriggerColliderHandIndicator component); }


        // The transform used to determen if the player is alllowed to use keyboard controlls
        internal static Transform BodyReference() { return GorillaTagger.Instance.bodyCollider.transform; }
        // The max distance away from the body the player can be to use keyboard controlls
        internal static float MaxKeyboardControllsDisctance = 1.5f;
        // The distance away from the machine (a spawned one) the player has to be to despawn it
        internal static float MachineDespawnDistance = 3f;
        // The position for the maching to "spawn"
        internal static Vector3 MachineSpawnPoint = new Vector3(-63.2366f, 12.0326f, -81.5984f);
        // The rotation the machine "spawns" with (yes this is made into a qauturnion later)
        internal static Vector3 MachineSpawnRoto = new Vector3(0f, 4.075f, 0f);
        // The scale the machine "spawns" with
        internal static Vector3 MachineSpawnScale = new Vector3(1f, 1f, 1f);

        // The type of computer/Ui that shows in game [FullMachine = Normal Machine/Wallmount, FullComputer = Normal Computer, SimpleButtons = Simple Cubes For Buttons, Text = Only Text, None = Nothing, recommended for using the gui.]
        internal static ComputerType CompType = ComputerType.FullMachine;
        // If it should be a GUI instead of an in game maching.
        internal static bool IsGui = false;
    }

    internal enum ComputerType
    {
        FullComputer,
        FullMachine,
        SimpleButtons,
        None,
        Text
    }
}
