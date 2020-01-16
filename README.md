## Description:
Build Vision 2 is a recreation and improvement upon the [original Build Vision by Jimmacle](https://steamcommunity.com/sharedfiles/filedetails/?id=756792814). This mod adds a context menu to the game designed to supplement the in-game terminal. Most of the time, the terminal is a perfectly acceptable way to configure blocks, but sometimes, it's just cumbersome. With BV, you can just point and click at a block and have immediate access to its settings, no need to go find a button panel and search for a name you likely can't remember.

This mod can be found on the Steam Workshop [here](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408).

## What's New in 2.4:
* Migrated to the Rich HUD Framework.
	* New settings menu
	* New text renderer
* No longer using the Text HUD API
* Text fields:
  * The caret can be moved using arrow keys
  * Numerical values can be typed in
  * Pressing Ctrl + A will select all text in the field
  * Text can be copied/cut/pasted using (Ctrl + C/X/V)
    * Note: The clipboard the text is stored to only works between mods
    using the Rich Hud Framework. Copy/pasting from in-game fields or from
    outside the game is not supported.
* Power consumption/production, battery charge and tank fill pct. are now
* displayed in the menu.
* Rotor and piston positions are now displayed in the menu.
* Menu scrolling now wraps around. If you attempt to scroll off either edge of the menu, it
* will jump to the other end of the list.
* Holding down Ctrl (MultX) while scrolling will allow you to scroll through the menu faster
* Improved block targeting. Block targeting now checks the neighboring block as well as the bounding box of the models.
* The fallback GUI has been removed.
* Support for custom UI colors has been removed. Opacity is still configurable.

## Binds:
All keybinds are configurable via the Rich Hud Terminal and through chat commands. To access the terminal, open chat, press ~ (tilde), click "Build Vision" and then click on "Binds." If you're completely mad and just HAVE to use chat commands, see the /bv2 bind cmd below.

#### Default Binds:
* Open: Control + MiddleButton
* Close: Shift + MiddleButton
* Select: Middlebutton
* Scroll Up: MouseWheelUp
* Scroll Down: MouseWheelDown
* MultX: Control (0.1 for float values, x8 for colors)
* MultY: Shift (x5 for float values, x16 for colors)
* MultZ: Control + Shift (x10 for float values, x64 for colors)

The “Mult” binds or multiplier binds are used to increase the speed a selected property will change with each tick of the scroll wheel, ten times normal, **1/10th** normal, **5** times, **10** times, etc. The base rate is proportional to the maximum value of the selected property. If you have a thruster override selected (probably with a max value in the millions) that’s going to change in much larger increments than the targeting range of a turret (with a max around 800 meters, usually).

## Settings Menu:
This mod is using the Rich Hud Terminal for its settings menu and is accessed by opening chat and pressing ~. Through this menu, you can configure block targeting, change UI settings and configure your keybinds.

## Chat Commands:
All chat commands must begin with “/bv2” and are not case-sensitive. The arguments following “/bv2” can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat, just make sure there’s something between them.

* help – It’s a help menu
* bindHelp – Help menu for changing key binds
* printBinds – Prints your current key binds to chat
* bind [bindName] [control1] [control2] [control3]
* save – For manually saving the configuration
* load – For manually loading the config file
* resetConfig – Resets all of your settings to default
* resetBinds – Resets all key binds to default
* toggleAutoclose – If enabled, the menu will close if it goes off your screen
* toggleOpenWhileHolding -- If enabled, you'll be able to open the menu while holding a tool or weapon

Example: "/bv2 bind open control shift"

## Editing the Config File:
The config file can be found in **%AppData%\Roaming\SpaceEngineers\Storage\1697184408.sbm_BuildVision2.** At the risk of stating the obvious, improperly formatted XML will not be interpreted correctly and will result in the default settings being loaded instead (unless a set of valid settings are already loaded). The config file will not be generated until you run the mod at least once.

Any formatting errors or general IO errors will be saved to **bvLog.txt** in the same directory. If the error log isn't working, you might have a problem with your file access permissions. The log file won't be created unless there was an error to log at some point.
