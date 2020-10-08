## Description:
Build Vision 2 is a recreation and improvement upon the [original Build Vision by Jimmacle](https://steamcommunity.com/sharedfiles/filedetails/?id=756792814). This mod adds a context menu to the game designed to supplement the in-game terminal. Most of the time, the terminal is a perfectly acceptable way to configure blocks, but sometimes, it's just cumbersome. With BV, you can just point and click at a block and have immediate access to its settings, no need to go find a button panel and search for a name you likely can't remember.

This mod can be found on the Steam Workshop [here](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408).

## Controls:
All keybinds are configurable via the Rich Hud Terminal and through chat commands. To access the terminal, open chat, press F1, click "Build Vision" and then click on "Binds." If you're completely mad and just _have_ to use chat commands, see the /bv2 bind cmd in the Chat Commands section. A list of supported controls can be found [here](https://pastebin.com/mGNq3u1T).  
  
**Main Binds:**

If you don't need to change any settings, holding down the peek bind will show you a summary of the status/configuration of the block you're targeting, including information like its name, power and inventory usage. Just hold it down, aim at the block you want to check and release when you're done.  
  
|Name|Controls|
|--|--|
|Peek|Control|
|Open|Control + Middle Button|
|Close|Shift + MiddleButton|
|Select|MiddleButton|
|Scroll Up|MouseWheelUp|
|Scroll Down|MouseWheelDown|
  
Text fields are opened/closed by opening/closing chat and support the usage of the usual Ctrl+A/X/C/V binds to select, cut, copy and paste text. Fair warning: you'll only be able to copy text within Build Vision; it won't work with the terminal or anything else.  
  
Numerical fields can be changed either by using the scroll wheel to increment/decrement their values or by opening them like a text field and typing in a value.  
  
**Multiplier Binds:**  

The multiplier binds are used to change the speed a selected property will change with each tick of the scroll wheel, **1/10th** normal, **5x** normal, **10x**, etc. The base rate is proportional to the maximum value of the selected property. If you have a thruster override selected (probably with a maximum in the millions) that’s going to change in much larger increments than the targeting range of a turret (with a max around 800 meters, usually).  
  
The MultX bind, Ctrl by default, can also be used to scroll through the list faster. Just hold it down while scrolling.  

|Name|Control|
|--|--|
|MultX (x0.1 for float values, x8 for colors)|Control|
|MultY (x5 for float values, x16 for colors)|Shift|
|MultZ (x10 for float values, x64 for colors)|Control + Shift|


**Copy/Paste Binds:**  

These binds are used to copy settings between compatible block types. When in this mode, you can either select/deselect properties one at a time using the Scroll and Select binds or you can select them all at once using the Select All bind. Pressing Select All will also automatically change the menu to copy mode if not it's already enabled.  
  
|Name|Controls|
|--|--|
|Toggle Copy Mode|Home|
|Select All|Insert|
|Copy Selection|PageUp|
|Paste Copied Properties|PageDown|
|Undo Paste|Delete|

**Settings Menu:**

This mod is using the Rich Hud Terminal for its settings menu and is accessed by opening chat and pressing F1. Through this menu, you can configure block targeting, change UI settings and configure your keybinds.  
  

## Chat Commands:
The chat commands in this mod are largely redundant; the functionality they provide is also provided by the settings menu. If you prefer to use the chat commands for whatever reason, here they are:  
  
All chat commands must begin with “/bv2” and are not case-sensitive. The arguments following “/bv2” can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat; just make sure there’s something between them.  
  
|Command|Description|
|--|--|
|help|Opens help menu|
|bindHelp|Help menu for changing key binds|
|printBinds|Prints your current key binds to chat|
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
