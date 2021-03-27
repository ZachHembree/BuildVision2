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
            subheader = new GlyphFormat(accentGrey, TextAlignment.Center, 1.2f, new Vector2I(0, (int)FontStyles.Underline)),
            subsection = new GlyphFormat(accentGrey, fontStyle: new Vector2I(0, (int)FontStyles.Underline));

        public static RichText GetHelpMessage()
        {
            return new RichText
            {
                { $"To open Build Vision, aim at the block you want to control and press press " }, { $"[{GetBindString(BvBinds.Open)}]", highlight }, {"; to close the menu, " },
                { $"[{GetBindString(BvBinds.Hide)}]", highlight }, { ". Alternatively, pressing and holding " }, {$"[{GetBindString(BvBinds.Peek)}]", highlight}, 
                {" will allow you to peek at a block's current status.\n\n" },

                { $"[{GetBindString(BvBinds.ScrollUp)}]", highlight }, { $" and " }, { $"[{GetBindString(BvBinds.ScrollDown)}]", highlight }, 
                { " can be used to scroll up and down the list and to increment / decrement numerical properties when selected. To select a property in the menu press " },
                { $"[{GetBindString(BvBinds.Select)}].\n\n", highlight },

                { $"By default, the menu will close if you move more than 10 meters " }, {"(4 large blocks)", highlight }, {" from your target block. The exact distance can " },
                { $"be customized in the settings section.\n\n" },

                { $"Main Binds:\n\n", subsection},

                { $"\tOpen Menu:\t\t|\t[{GetBindString(BvBinds.Open)}]\n" },
                { $"\tClose Menu:\t\t|\t[{GetBindString(BvBinds.Hide)}]\n" },
                { $"\tSelect/Trigger:\t|\t[{GetBindString(BvBinds.Select)}]\n" },
                { $"\tScroll Up:\t\t\t|\t[{GetBindString(BvBinds.ScrollUp)}]\n" },
                { $"\tScroll Down:\t\t|\t[{GetBindString(BvBinds.ScrollDown)}]\n\n" },

                { $"Multiplier Binds:\n", subsection},

                { $"The multiplier binds are used to change the " }, {"speed", highlight }, {" at which a selected property will change with each tick of the scroll wheel, " },
                { $"1/10th", highlight }, {$" normal, " }, {$"5x", highlight }, {$" normal, " }, {$"10x", highlight }, {$", etc.\n\n" },

                { $"\tMultX (x{BvConfig.Current.block.floatMult.X}):\t\t|\t[{GetBindString(BvBinds.MultX)}]\n" },
                { $"\tMultY (x{BvConfig.Current.block.floatMult.Y}):\t\t|\t[{GetBindString(BvBinds.MultY)}]\n" },
                { $"\tMultZ (x{BvConfig.Current.block.floatMult.Z}):\t\t|\t[{GetBindString(BvBinds.MultZ)}]\n\n" },

                { $"The MultX bind, Ctrl by default, can also be used to scroll through the list " }, {"faster", highlight }, {". Just hold it down while scrolling.\n\n" },

                { $"Copy/Paste Binds:\n\n", subsection},

                { $"\tToggle Dupe Mode:\t|\t[{GetBindString(BvBinds.ToggleSelectMode)}]\n" },
                { $"\tSelect All:\t\t\t|\t[{GetBindString(BvBinds.SelectAll)}]\n" },
                { $"\tCopy Selected:\t\t|\t[{GetBindString(BvBinds.CopySelection)}]\n" },
                { $"\tPaste Copied:\t\t|\t[{GetBindString(BvBinds.PasteProperties)}]\n" },
                { $"\tUndo Paste:\t\t\t|\t[{GetBindString(BvBinds.UndoPaste)}]\n\n" },

                { $"These binds are used to " }, {$"copy properties", highlight }, {$" between compatible block types. When in dupe mode, you'll be " },
                { $"able to select/deselect properties one at a time using the " }, {"scroll and select", highlight }, {" binds or you can select them " },
                { $"all at once using the select all bind. Pressing Select All will also automatically change the menu to " },
                { $"duplication mode if not it's already enabled.\n\n" },

                { $"Settings Menu:\n", subheader},
                { $"By default, this settings menu can be toggled by pressing " }, { "F1", highlight }, { " while having chat open. From here, you can configure block " },
                { $"targeting, change UI settings and configure your keybinds.\n\n" },

                { $"Chat Commands:\n", subheader},
                { $"The chat commands in this mod are largely redundant; the functionality they provide is also provided by the settings menu. " },
                { $"If you prefer to use the chat commands for whatever reason, here they are:\n\n" },

                { $"All chat commands must begin with " }, { "“/bv2”", highlight }, { "and are not case-sensitive. The arguments following " }, { "“/bv2”", highlight }, 
                { " can be separated either by whitespace, a comma, semicolon or pipe character. Whatever floats your boat; just make sure there’s something between them.\n\n" },

                { $"• help:\t\t\tYou are here.\n" },
                { $"• printBinds:\t\tPrints your current key binds to chat.\n" },
                { $"• save:\t\t\tSaves the current configuration\n" },
                { $"• load:\t\t\tLoads configuration from the config file\n" },
                { $"• resetBinds:\t\tResets all keybinds\n" },
                { $"• resetConfig:\tResets all settings to default\n" },
                { $"• bind\t\t\t\t[bindName] [control1] [control2] [control3]\n\n" },

                { $"Example: \"/bv2 bind open control shift\"\n\n" },

                { $"For more information, see the Build Vision 2 workshop page." }
            };
        }

        public static string GetPrintBindsMessage()
        {
            string bindHelp =
                "\n---Build Vision 2 Binds---\n" +
                $"Peek: [{GetBindString(BvBinds.Peek)}]\n" +
                $"Open: [{GetBindString(BvBinds.Open)}]\n" +
                $"Close: [{GetBindString(BvBinds.Hide)}]\n" +
                $"Select: [{GetBindString(BvBinds.Select)}]\n" +
                $"Scroll Up: [{GetBindString(BvBinds.ScrollUp)}]\n" +
                $"Scroll Down: [{GetBindString(BvBinds.ScrollDown)}]\n" +
                $"---Multipliers---\n" +
                $"MultX: [{GetBindString(BvBinds.MultX)}]\n" +
                $"MultY: [{GetBindString(BvBinds.MultY)}]\n" +
                $"MultZ: [{GetBindString(BvBinds.MultZ)}]\n" +
                $"---Copy/Paste---\n" +
                $"ToggleSelectMode: [{GetBindString(BvBinds.ToggleSelectMode)}]\n" +
                $"SelectAll: [{GetBindString(BvBinds.SelectAll)}]\n" +
                $"CopySelection: [{GetBindString(BvBinds.CopySelection)}]\n" +
                $"PasteProperties: [{GetBindString(BvBinds.PasteProperties)}]\n" +
                $"UndoPaste: [{GetBindString(BvBinds.UndoPaste)}]";

            return bindHelp;
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