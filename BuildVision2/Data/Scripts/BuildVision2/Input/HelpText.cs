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
            subheader = new GlyphFormat(accentGrey, TextAlignment.Center, 1.2f, FontStyles.Underline),
            subsection = new GlyphFormat(accentGrey, style: FontStyles.Underline);

        public static RichText GetHelpMessage()
        {
            return new RichText
            {
                $"This is a quick access context menu for changing block settings, opened by aiming at the desired block and pressing either ",
                {$"[{GetBindString(BvBinds.OpenWheel)}]", highlight}, " or ", {$"[{GetBindString(BvBinds.OpenList)}].", highlight}, " The first " +
                "opens the wheel menu; the second opens the older list-style menu. Holding ", { $"[{GetBindString(BvBinds.MultX)}]", highlight },
                $" (peek) will show a summary of the target block's current status without opening the controls.\n\n",

                $"Mouse input can be enabled for the wheel menu by holding the ", {"Peek", highlight }, " bind. Alternatively, you can use the same " +
                "scrolling + multiplier controls listed below  that are shared with the list menu. By default, the menu will close if you move more than " +
                "10 meters ", {"(4 large blocks)", highlight }, " from your target block. The exact distance can be customized in the settings section.\n\n",

                { $"Main Binds:\n\n", subsection },

                $"\tOpen Wheel:\t\t|\t[{GetBindString(BvBinds.OpenWheel)}]\n",
                $"\tOpen List:\t\t|\t[{GetBindString(BvBinds.OpenList)}]\n",
                $"\tSelect/Confirm:\t|\t[{GetBindString(BvBinds.Select)}]\n",
                $"\tCancel/Back:\t|\t[{GetBindString(BvBinds.Cancel)}]\n",
                $"\tScroll Up:\t\t\t|\t[{GetBindString(BvBinds.ScrollUp)}]\n",
                $"\tScroll Down:\t\t|\t[{GetBindString(BvBinds.ScrollDown)}]\n\n",

                { $"Modifiers:\n", subsection},

                $"The multiplier binds are used to change the ", {"speed", highlight }, " at which a selected property will change with each tick of the scroll wheel, ",
                { $"1/10th", highlight }, $" normal, ", {$"5x", highlight }, $" normal, ", {$"10x", highlight }, $", etc.\n\n",

                $"\tMultX/Peek (x{BvConfig.Current.block.floatMult.X}):\t|\t[{GetBindString(BvBinds.MultX)}]\n",
                $"\tMultY (x{BvConfig.Current.block.floatMult.Y}):\t\t\t|\t[{GetBindString(BvBinds.MultY)}]\n",
                $"\tMultZ (x{BvConfig.Current.block.floatMult.Z}):\t\t\t|\t[{GetBindString(BvBinds.MultZ)}]\n\n",

                $"The MultX bind, Ctrl by default, can also be used to scroll through the list ", {"faster", highlight }, ". Just hold it down while scrolling.\n\n",

                { $"Property Duplication:\n", subsection},

                $"Property or settings duplication, as the name implies, is used to copy block settings from one block to another. The controls below can be used to " +
                $"quickly switch between dupe mode and normal control, but there are buttons for these controls in the wheel menu as well.\n\n",

                $"\tStart Dupe:\t\t|\t[{GetBindString(BvBinds.StartDupe)}]\n",
                $"\tStope Dupe:\t\t|\t[{GetBindString(BvBinds.StopDupe)}]\n\n",

                { $"Settings Menu:\n", subheader},
                $"By default, this settings menu can be toggled by pressing ", { "F2", highlight }, " From here, you can configure block ",
                $"targeting, change UI settings and configure your keybinds.\n\n",

                { $"Chat Commands:\n", subheader},
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
            IList<IControl> combo = bind.GetCombo();
            string bindString = "";

            for (int n = 0; n < combo.Count; n++)
            {
                if (n != combo.Count - 1)
                bindString += combo[n].DisplayName + " + ";
                else
                bindString += combo[n].DisplayName;
            }

            return bindString;
        }
    }
}