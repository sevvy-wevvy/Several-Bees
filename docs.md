## Refrance Setup
**Puting the mod a refrance for your csproj tut coming soon**
For every script you plan on using Several Bees on please add ``using SeveralBees;`` into it so the doc template integrate semlessly.
## Menu Creation (Api)
To create a menu for your own mods its quite easy. First you must generate and save you page token:
```c#
public string SbToken = "";
SbToken = Api.Instance.GenerateToken("My Epic Mod", true, "Main");
```
"My Epic Mod" - __What shows at the top of the Several Bees mod manger when your section is open__  
true - __Wether your new page is visable from the Several Bees mod manger main menu__  
"Main" - __The token for the page the back button takes you too__ (Useful if you want to make a multi page/sorted mod)  
  
Then with your token you can preforme the folling actions, they are minor actions so i will not be including detaled descriptions for them.
```c#
// This retreve the current button info for your mod which will be discused later.
List<ModButtonInfo> MyModButtonInfo = Api.Instance.GetButtonInfo(SbToken);

// Sets the button overlay text which displays over your buttons real name incase you want to make dynamicly changing text.
Api.Instance.SetButtonNickname(SbToken, "Button Name", "New Cool Overlay Text");

// Used to change the ModButtonInfo for a single button instead of your whole menu/token.
Api.Instance.SetButtonInfoSignle(SbToken, "Button Name", new ModButtonInfo { buttonText = "Button Name" });

// Used to grab the ModButtonInfo for a single button.
ModButtonInfo ButtonInfo = Api.Instance.GrabButton(SbToken, "Button Name");

// Used for opening a menu with the click of a button, say to create a menu for the rest of your menu, or take you back to Main without the defualt back button.
Api.Instance.OpenMenu("CoolMenuToken");
```
  
To set you ModButtonInfo you would do somthing like the below:
```c#
Api.Instance.SetButtonInfo(SbToken, new List<ModButtonInfo>
{
    new ModButtonInfo { buttonText = "Theme Presets", isTogglable = false, method = () => Api.Instance.OpenMenu("3"), toolTip = "Opens the theme presets menu." },
    new ModButtonInfo { buttonText = "cfs", isTogglable = false, method = () => Settings.cfs(), toolTip = "Adjusts the speed of the menu color fading." },
    new ModButtonInfo { buttonText = "nfc", isTogglable = false, method = () => Settings.nfc(), toolTip = "Changes the menu title first fade color." },
    new ModButtonInfo { buttonText = "nsc", isTogglable = false, method = () => Settings.nsc(), toolTip = "Changes the menu title second fade color." },
});
// This sets and overwrite the ModButtonInfo for an entire menu/token.
```
## ModButtonInfo varible
ModButtonInfo has many varible we will go over here to help you understand how to make buttons.
```c#
// To start the mod button info class is below
public class ModButtonInfo
{
    public string buttonText;
    public string buttonOverlayText = null;
    public bool isTogglable;
    public bool enabled;
    public Action method;
    public Action enableMethod;
    public Action disableMethod;
    public string toolTip = null;
}

// Button text is used for searching a certion button, and if theres no overlay its what shows.
new ModButtonInfo { buttonText = "My Cool Button Text" }

// Button overlay text shows as a "overlay" to your button text. This is usefull for dynamics changing buttons, or just making a long names simpler to search.
new ModButtonInfo { buttonOverlayText = "My Cool Button Text" }

// Wether the button is a toggle.
new ModButtonInfo { isTogglable = Maybe? }

// Can be used to define the defualt state of a toggle, or fetched when you not using the built in metheds to detect when the toggle is on.
new ModButtonInfo { enabled = false }

// Called when a non-toggle button is clicked, or called every update when a toggle is on.
new ModButtonInfo { method = () => MyCoolFunction() }

// Called when a toggle is turned on.
new ModButtonInfo { enableMethod = () => MyCoolFunction() }

// Called when a toggle is turned off.
new ModButtonInfo { disableMethod = () => MyCoolFunction() }

// The tooltip, what shows at the bottom of the mod manger when a button is pressed or a toggle is toggled. Use this to help poeple understand what this thing does.
new ModButtonInfo { toolTip = "Im a very cool button, or maybe a toggle?" }
```

## Extra functions
Heres some functions that will help you out when creating mods.
```c#
// Lets you gradient your text with a scrolling effect as seen in the title of Several Bees.
Extra.Instance.GradientText("Awsome text", Color.white, Color.black, 0.2f);

// Applies the correct shader depending on the game or game update. Say if the game uses urp, use this function to applie the games main shader or the defualt urp shader.
Extra.Instance.MakeObjectVisible(MyCoolGameObject, true);
// The bool defines wether you would like to reset the color of your object aswell.

// Lets you set the theme of Several Bees, the default fade speed (3rd varible) is 0.2
Extra.Instance.SetTheme(Color.black, Color.white, float 0.2f);
```

## Asset loading
First run ``Extra.AssetLoad("MyCoolNameSpcae.MyCoolBundle");`` to properly load the assets from your bundle. Recommeneded you do this on plugin awake. To load an asset from your bundle do somthing like the below.
```c#
GameObject coolObjectSet = null;
if (Extra.Instance.TryGetAsset<GameObject>("MyCoolNameSpcae.MyCoolBundle", "My Cool Object", out coolObjectSet))
{
    GameObject coolObject = Instantiate(coolObjectSet);
    //Work with "coolObject" here
}
```

## Extra Varible
In the main Several Bees script there are few public parivle that may be of use to you.
```c#
// A list of detailed colors
List<DetailedColor> colors = SeveralBees.Instance.CycleColors;

// A list of floats
List<float> floats = SeveralBees.Instance.CycleFloats;

// Emulating button clicks on the mod manger, the bool input is wether it was the left hand.
SeveralBees.Instance.MmDown(true);
SeveralBees.Instance.MmUp(true);
SeveralBees.Instance.MmSelect(true);

// The theme colors for Several Bees
Color theme1 = SeveralBees.Instance.Theme1;
Color theme2 = SeveralBees.Instance.Theme2;

// The custom colliders for finger pointers/tips.
SphereCollider leftPointer = SeveralBees.Instance.LeftPointerCollider;
SphereCollider realRightPointer = SeveralBees.Instance.RightPointerCollider;
```

## Forking
In the source code (script Config.cs) you can find multiple varibles for you to change so this lib works for difrent games.


