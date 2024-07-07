using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public static partial class HelpText
    {
        public static string controlList;
        private static readonly Color accentGrey = new Color(200, 200, 210);
        private static readonly GlyphFormat 
            highlight = new GlyphFormat(accentGrey),
            subheader = new GlyphFormat(accentGrey, TextAlignment.Center, 1.1f, FontStyles.Underline),
            subsection = new GlyphFormat(QuickActionMenu.highlightFormat.Color, style: FontStyles.Underline);

        public static RichText GetHelpMessage()
        {
            return new RichText(GlyphFormat.White)
            {
                { " Update 3.0:\n\n", subheader },

                $"This update brings a complete redesign of Build Vision's UI and control scheme. The functionality is the same as it was " +
                $"in 2.5, but the old list menu has been (mostly) replaced with an easier-to-use wheel menu.\n\n",
                
                $"The list menu is still available for properties not in the wheel menu and the old control scheme can be restored using the ",
                { "Legacy Mode", highlight }, " option in the settings. See the description below for the new binds and usage.\n\n",

                { "Usage \n\n", subheader },

                $"This menu is opened by aiming at the desired block and pressing either ", {$"[{GetBindString(BvBinds.OpenWheel)}]", highlight}, 
                " or ", {$"[{GetBindString(BvBinds.OpenList)}].", highlight}, " The first opens the wheel menu; the second opens the older " +
                "list-style menu. Holding ", { $"[{GetBindString(BvBinds.MultXOrMouse)}]", highlight }, $" (peek) will show a summary of the target " +
                $"block's current status without opening the controls. Pressing ", {$"[{GetBindString(BvBinds.Cancel)}]", highlight}, " (Cancel/Back) " +
                "will close the menu.\n\n",

                $"By default, the menu will close if you move more than 10 meters ", {"(4 large blocks)", highlight }, " from your target block. " +
                "The exact distance can be customized in the settings section.\n\n",

                { "Main Binds \n\n", subheader },

                $"\tOpen Wheel\t\t|\t[{GetBindString(BvBinds.OpenWheel)}]\n",
                $"\tOpen List\t\t\t|\t[{GetBindString(BvBinds.OpenList)}]\n",
                $"\tSelect/Confirm\t|\t[{GetBindString(BvBinds.Select)}]\n",
                $"\tCancel/Back\t\t|\t[{GetBindString(BvBinds.Cancel)}]\n",
                $"\tScroll Up\t\t\t|\t[{GetBindString(BvBinds.ScrollUp)}]\n",
                $"\tScroll Down\t\t|\t[{GetBindString(BvBinds.ScrollDown)}]\n\n",

                { "Selection:\n", subsection},
                "There are two ways to select properties: scrolling or by moving the mouse. To highlight with the mouse, hold ",
                { $"[{GetBindString(BvBinds.MultXOrMouse)}]", highlight }, " (MultX/Peek), and move your mouse either clockwise or counterclockwise, like you're " +
                "tracing a circle.\n\n",

                "Pressing ", { "Select/Confirm", highlight }, " will open the highlighted property. Pressing that bind again will confirm any changes. " +
                "Pressing ", { "Back/Cancel", highlight }, " will close the property without saving changes; pressing it again will close the menu.\n\n",

                { "Note:", highlight }, " The list menu only supports scrolling with the mouse wheel (or scroll binds).\n\n",

                { "Changing Settings:\n", subsection},
                "Properties can be changed using the scroll wheel in conjunction with the multiplier binds listed below, with text-only fields being the " +
                "only exception.\n\n",
               
                "Alternatively, ",{ "MultX/Peek", highlight }, " will allow you to use your cursor in the wheel menu with the control in the center. Depending on " +
                "the use case, this is often easier.\n\n",

                { "Text:\n", subsection},
                "Text fields are opened/closed with ", { "chat", highlight }, " (Enter) and support the usage of the usual Ctrl+A/X/C/V binds to select, cut, " +
                "copy and paste text.  Fair warning: you'll only be able to copy text within Build Vision; it won't work with the terminal or anything else.\n\n" +

                "Opening ", { "chat", highlight }, " (pressing Enter) while a text field is highlighted will open text input automatically.\n\n",

                { "Note:", highlight }, " Most numerical fields support text input.\n\n",

                { "Modifiers \n", subheader},

                $"The multiplier binds are used to change the ", {"speed", highlight }, " at which a selected property will change with each tick of the scroll wheel, ",
                { $"1/10th", highlight }, $" normal, ", {$"5x", highlight }, $" normal, ", {$"10x", highlight }, $", etc.\n\n",

                $"\tMultX/Peek (x{BvConfig.Current.block.floatMult.X}):\t|\t[{GetBindString(BvBinds.MultXOrMouse)}]\n",
                $"\tMultY (x{BvConfig.Current.block.floatMult.Y}):\t\t\t|\t[{GetBindString(BvBinds.MultY)}]\n",
                $"\tMultZ (x{BvConfig.Current.block.floatMult.Z}):\t\t\t|\t[{GetBindString(BvBinds.MultZ)}]\n\n",

                $"The MultX bind, Ctrl by default, can also be used to scroll through the list ", {"faster", highlight }, ". Just hold it down while scrolling.\n\n",

                { $"Property Duplication Shortcuts \n", subheader},

                $"Property or settings duplication, as the name implies, is used to copy block settings from one block to another. The controls below can be used to " +
                $"quickly copy block settings, but there are buttons for these controls in the wheel menu as well.\n\n",

                $"\tStart Dupe:\t\t|\t[{GetBindString(BvBinds.StartDupe)}]\n",
                $"\tStop Dupe:\t\t|\t[{GetBindString(BvBinds.StopDupe)}]\n",
                $"\tToggle Dupe:\t\t|\t[{GetBindString(BvBinds.ToggleDupe)}]\n",
                $"\tSelect All:\t\t|\t[{GetBindString(BvBinds.SelectAll)}]\n",
                $"\tCopy Selection:\t|\t[{GetBindString(BvBinds.CopySelection)}]\n",
                $"\tPaste Copy:\t\t|\t[{GetBindString(BvBinds.PasteProperties)}]\n",
                $"\tUndo Paste:\t\t|\t[{GetBindString(BvBinds.UndoPaste)}]\n\n",

                "When selecting properties from the list, pressing the back bind will take you back to the wheel menu to finish copying.\n\n",

                { "Settings Menu:\n", subheader},

                $"By default, this settings menu can be toggled by pressing ", { "F2.", highlight }, " From here, you can configure block ",
                $"targeting, change UI settings and configure your keybinds.\n\n",

                { "Chat Commands:\n", subheader},

                $"The chat commands in this mod are largely redundant; the functionality they provide is also provided by the settings menu. ",
                $"If you prefer to use the chat commands for whatever reason, here they are:\n\n",

                $"All chat commands must begin with ", { "“/bv2”", highlight }, "and are not case-sensitive. The arguments following ", { "“/bv2”", highlight }, 
                " can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat; just make sure there’s something between them.\n\n",

                $"• help:\t\t\tYou are here.\n",
                $"• save:\t\t\tSaves the current configuration\n",
                $"• load:\t\t\tLoads configuration from the config file\n",
                $"• resetBinds:\t\tResets all keybinds\n",
                $"• resetConfig:\tResets all settings to default\n",
                $"• bind\t\t\t\t[bindName] [control1] [control2] [control3]\n\n",

                $"Example: \"/bv2 bind open control shift\"\n\n",

                $"For more information, see the Build Vision workshop page."
            };
        }

        private static string GetBindString(IBind bind)
        {
            IList<ControlHandle> combo = bind.GetCombo();
            string bindString = "";

            for (int n = 0; n < combo.Count; n++)
            {
                if (n != combo.Count - 1)
                bindString += combo[n].Control.DisplayName + " + ";
                else
                bindString += combo[n].Control.DisplayName;
            }

            return bindString;
        }
    }
}