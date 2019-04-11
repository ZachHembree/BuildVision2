## Description:
Build Vision 2 is a recreation and improvement upon the [original Build Vision by Jimmacle](https://steamcommunity.com/sharedfiles/filedetails/?id=756792814). For the uninitiated among you, Build Vision adds a text-based control panel for (almost) all functional blocks and can be accessed by simply walking up to a block and clicking on it. Whether you’re trying to add a piston head, open a hangar door or change a light's color, all you need to do is aim at the block, click and do it. No fumbling about in the terminal required.

As of writing this, all vanilla terminal blocks (anything with terminal settings) work with this mod, save the Space Ball (much to my chagrin), and probably a fair number of modded blocks considering they derive from the same interfaces. I’ve also taken the liberty of adding a number of actions to the menu for doing things like detonating warheads, attaching wheels, adding rotor heads and changing battery charge modes. I’d make a full list of all the little differences between this mod and the original, but frankly, I’m not sure I can recall what all is in this thing.

Like the original, this depends on Draygo's Text Hud API to function properly. There is a fallback GUI using the built-in notifications system, but I don't like it all that much.

This mod can be found on the Steam Workshop [here](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408).

## Usage:
Build Vision 2 uses seven key binds for its controls, all of which are user configurable via chat commands or by editing the config file manually (details below). The keys supported can be found in the bind help menu by using the /bv2 bindHelp command or in this [pastebin](https://pastebin.com/mGNq3u1T). I just grabbed the 200+ enums they had in the ModAPI and I'm not sure what does what or if they all work. Have fun!

#### Default Binds:
* Open: Control + MiddleButton
* Close: Shift + MiddleButton
* Select: Middlebutton
* Scroll Up: MouseWheelUp
* Scroll Down: MouseWheelDown
* MultX: Control (x10 for float values, x8 for colors)
* MultY: Shift (x25 for float values, x16 for colors)
* MultZ: Control + Shift (x100 for float values, x64 for colors)

The “Mult” binds or multiplier binds are used to increase the speed a selected property will change with each tick of the scroll wheel, ten times normal, 25 times normal, 100 times, etc. The base rate is proportional to the maximum value of the selected property. If you have a thruster override selected (probably with a max value in the millions) that’s going to change in much larger increments than the targeting range of a turret (with a max around 800 meters, usually).

#### Note: 
Some properties use fairly aggressive rounding/clamping for their values, so some settings may not change at all unless you use the multipliers; projectors, in particular, suffer from this problem. 

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
* toggleApi – Toggles between the Text Hud API and the fallback UI (not pretty)
* toggleAutoclose – If enabled, the menu will close if it goes off your screen
* toggleOpenWhileHolding -- If enabled, you'll be able to open the menu while holding a tool or weapon

Example: "/bv2 bind open control shift"

## Editing the Config File:
In addition to storing your keybinds, the config file has a handful of other settings for changing your multiplier rates and a few UI settings. For those of you comfortable tinkering with XML, it's a hell of a lot faster than dicking around with chat commands. Just make your changes, use /bv2 load and they'll be instantly applied.

The config file can be found in **%AppData%\Roaming\SpaceEngineers\Storage\1697184408.sbm_BuildVision2.** At the risk of stating the obvious, improperly formatted XML will not be interpreted correctly and will result in the default settings being loaded instead. The config file will not be generated until you run the mod at least once.

Any formatting errors or general IO errors will be saved to bvLog.txt in the same directory. If the error log isn't working, you might have a problem with your file access permissions. The log file won't be created unless there was an error to log at some point.
