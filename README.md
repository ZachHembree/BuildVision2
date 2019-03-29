##Description:
Build Vision 2 is a recreation and improvement upon the original Build Vision by Jimmacle. For the uninitiated among you, Build Vision adds a text-based control panel for (almost) all functional blocks and can be accessed by simply walking up to a block and clicking on it. Whether you’re trying to add a piston head, open a hangar door or change a light color, all you need to do is aim at the block, click and do it. No fumbling about in the terminal required.
As of writing this, all vanilla terminal blocks (anything with terminal settings) work with this mod, save the Space Ball (much to my chagrin), and probably a fair number of modded blocks considering they derive from the same interfaces. I’ve also taken the liberty of adding a number of actions to the menu for doing things like detonating warheads, attaching wheels, adding rotor heads and changing battery charge modes. I’d make a full list of all the differences between this mod and the original, but frankly, I’m not sure I can recall what all is in this thing.


##Usage:
Build Vision 2 uses seven key binds for its controls, all of which are user configurable via chat commands or by editing the config file manually (details below).
Default Binds:
Open: Control + MiddleButton
Close: Shift + MiddleButton
Select: Middlebutton
Scroll Up: MouseWheelUp
Scroll Down: MouseWheelDown
MultX: Control (x10 for float values, x8 for colors)
MultY: Shift (x25 for float values, x16 for colors)
MultZ: Control + Shift (x100 for float values, x64 for colors)
The “Mult” binds or multiplier binds are used to increase the speed a selected property will change with each tick of the scroll wheel, ten times normal, 25 times normal, 100 times, etc. The base rate is proportional to the maximum value of the selected property. If you have a thruster override selected (probably with a max value in the millions) that’s going to change in much larger increments than the targeting range of a turret (with a max around 800 meters, usually).


##Chat Commands:
All chat commands must begin with “/bv2” and are not case-sensitive. The arguments following “/bv2” can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat, just make sure there’s something between them.
/help – It’s a help menu
/bindHelp – Help menu for changing key binds
/printBinds – Prints your current key binds to chat
/bind [bindName] [bind1] [bind2] [bind3]
/save – For manually saving the configuration
/load – For manually loading the config file
/resetConfig – Resets all of your settings to default
/resetBinds – Resets all key binds to default
/toggleApi – Toggles between the Text Hud API and the fallback UI (not pretty)
/toggleAutoclose – If enabled, the menu will close if it goes off your screen


##Editing the Config File:
The config file can be found in %AppData%\Roaming\SpaceEngineers\Storage\modID_BuildVision2\BuildVision2.xml. At the risk of stating the obvious, improperly formatted XML will not be interpreted correctly and will result in the default settings being loaded instead.
