## Description:
This is a quick access context menu, based on the [original Build Vision by Jimmacle](https://steamcommunity.com/sharedfiles/filedetails/?id=756792814), designed to supplement the game's terminal. Most of the time, the terminal works just fine for setting up blocks, but sometimes, it's cumbersome. With BV, you can just point and click at a block and have immediate access to its settings, no need to go find a button panel and search for a name you likely can't remember.

This mod can be found on the Steam Workshop [here](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408).

## Update 3.0:
This update brings a complete redesign of Build Vision's UI and control scheme. The functionality is the same as it was in 2.5, but the old list menu has been (mostly) replaced with an easier-to-use wheel menu. The list menu is still available for properties not in the wheel menu, or if you simply prefer it. See the description below for the new binds and usage.

## Controls:
The binds are configurable via the Rich Hud Terminal and through chat commands. To access the configuration press F2 to open the terminal, click "Build Vision" and then click on "Binds."
  
**Usage:**

This menu is opened by aiming at the desired block and pressing either **Ctrl + MwUp** or **Ctrl + MwDn**. The first opens the wheel menu; the second opens the older list-style menu. Holding Ctrl (peek) will show a summary of the target block's current status without opening the controls.

Mouse input can be enabled for the wheel menu by holding the **Peek** bind. Alternatively, you can use the same scrolling + multiplier controls listed below that are shared with the list menu. By default, the menu will close if you move more than 10 meters 4 large blocks from your target block. The exact distance can be customized in the settings.
  
|Name|Controls|
|--|--|
|Open Wheel|Control + MouseWheelUp|
|Open List|Control + MouseWheelDown|
|Select/Confirm|LeftButton|
|Cancel/Back|RightButton|
|Scroll Up|MouseWheelUp|
|Scroll Down|MouseWheelDown|
  
Text fields are opened/closed by opening/closing chat and support the usage of the usual Ctrl+A/X/C/V binds to select, cut, copy and paste text. Fair warning: you'll only be able to copy text within Build Vision; it won't work with the terminal or anything else.  
  
Numerical fields can be changed either by using the scroll wheel to increment/decrement their values or by opening them like a text field and typing in a value.  
  
**Modifier/Multiplier Binds:**  

The multiplier binds are used to change the speed a selected property will change with each tick of the scroll wheel, **1/10th** normal, **5x** normal, **10x**, etc. The base rate is proportional to the maximum value of the selected property. If you have a thruster override selected (probably with a maximum in the millions) that’s going to change in much larger increments than the targeting range of a turret (with a max around 800 meters, usually). 

The MultX bind, Ctrl by default, can also be used to scroll through the list faster. Just hold it down while scrolling.

|Name|Control|
|--|--|
|MultX/Peek (x0.1 for float values, x8 for colors)|Control|
|MultY (x5 for float values, x16 for colors)|Shift|
|MultZ (x10 for float values, x64 for colors)|Control + Shift|


**Property Duplication:**  

Property or settings duplication, as the name implies, is used to copy block settings from one block to another. The controls below can be used to quickly switch between dupe mode and normal control, but there are buttons for these controls in the wheel menu as well.
  
|Name|Controls|
|--|--|
|Start Dupe|Ctrl + Alt + MouseWheelUp|
|Stop Dupe|Ctrl + Alt + MouseWheelDown|

**Settings Menu:**

This mod is using the Rich HUD Terminal for its settings menu and, by default, can be opened by pressing F2. This menu allows you to configure block targeting, change UI settings and configure your keybinds.
  

## Chat Commands:
The chat commands in this mod are largely redundant; the functionality they provide is also provided by the settings menu. If you prefer to use the chat commands for whatever reason, here they are:  
  
All chat commands must begin with “/bv2” and are not case-sensitive. The arguments following “/bv2” can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat; just make sure there’s something between them.  
  
|Command|Description|
|--|--|
|help|Opens help menu|
|bindHelp|Help menu for changing key binds|
|bind|[bindName] [control1] [control2] [control3]|
|save|For manually saving the configuration|
|load|For manually loading the config file|
|resetBinds|Resets all key binds to default|
|resetConfig|Resets all of your settings to default|

  
Example: "/bv2 bind open control shift"  
  

## Editing the Config File:
The config file can be found in **%AppData%\Roaming\SpaceEngineers\Storage\1697184408.sbm_BuildVision2**. At the risk of stating the obvious, improperly formatted XML will not be interpreted correctly and will result in the default settings being loaded instead (unless a set of valid settings are already loaded). The config file will not be generated until you run the mod at least once.  
  
Any formatting errors or general IO errors will be saved to bvLog.txt in the same directory. If the error log isn't working, you might have a problem with your file access permissions. The log file won't be created unless there was an error to log at some point.  
  

**Troubleshooting and Bug Reports:**

If you're having trouble getting this mod to work, see the [Troubleshooting Guide](https://steamcommunity.com/workshop/filedetails/discussion/1697184408/2259060348521461027).  
  
If you have a bug to report, see the [instructions](https://steamcommunity.com/workshop/filedetails/discussion/1697184408/1769259642874284751) for submitting bug reports in the discussions section.
