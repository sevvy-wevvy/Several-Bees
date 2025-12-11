using UnityEngine;

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
        internal static string CurrentModVersion = "1.0.0";
        // The txt that holds the latest version number of the mod.
        internal static string ModVersionLink = "https://raw.githubusercontent.com/sevvy-wevvy/Several-Bees/main/ver.txt";
        // Latest mod download link
        internal static string ModDownload = "https://github.com/sevvy-wevvy/Several-Bees/releases/latest/download/SeveralBees-Gtag.dll";


        // Whats used when making an object visable (Use "Def" to use the defualt shader)
        internal static string PropperShader = "GorillaTag/UberShader";


        // The right hand transform (Set to "new Vector3(0, -integer.MaxValue, 0)" if its not a vr game/doesnt use hands)
        internal static Transform RightHandReference() { return GorillaTagger.Instance.rightHandTransform; }
        // The offset of the real collider/pointer for interactions
        internal static Vector3 RightHandPointerOffset = new Vector3(0f, -0.1f, 0f);


        // The left hand transform (Set to "new Vector3(0, -integer.MaxValue, 0)" if its not a vr game/doesnt use hands)
        internal static Transform LeftHandReference() { return GorillaTagger.Instance.leftHandTransform; }
        // The offset of the real collider/pointer for interactions
        internal static Vector3 LeftHandPointerOffset = new Vector3(0f, -0.1f, 0f);


        // The transform used to determen if the player is alllowed to use keyboard controlls
        internal static Vector3 BodyReference() { return GorillaTagger.Instance.bodyCollider.transform.position; }
        // The max distance away from the body the player can be to use keyboard controlls
        internal static float MaxKeyboardControllsDisctance = 1.5f;
        // The position for the maching to "spawn"
        internal static Vector3 MachineSpawnPoint = new Vector3(-63.1911f, 11.7771f, -81.8163f);
        // The rotation the machine "spawns" with (yes this is made into a qauturnion later)
        internal static Vector3 MachineSpawnRoto = new Vector3(0f, 4.075f, 9.0033f);
        // The scale the machine "spawns" with
        internal static Vector3 MachineSpawnScale = new Vector3(1f, 1f, 1f);
        // When the machine should have colliders
        internal static bool MachineHasColliders() { return true; }

        // The type of computer/Ui that shows in game [Full = normal, SimpleButtons = Simple Cubes For Buttons, Text = Only Text, None = Nothing, recommended for using the gui.]
        internal static ComputerType CompType = ComputerType.Full;
        // If it should be a GUI instead of an in game maching.
        internal static bool IsGui = false;
    }

    internal enum ComputerType
    {
        Full,
        SimpleButtons,
        None,
        Text
    }
}
