using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        private sealed partial class PropertyWheelMenu : HudElementBase
        {
            /// <summary>
            /// Swtiches to duplication wheel and enables property duplication controls
            /// </summary>
            private void StartPropertyDuplication()
            {
                MenuState |= QuickActionMenuState.PropertyDuplication;
                propertyWheel.Visible = false;
                dupeWheel.Visible = true;
            }

            /// <summary>
            /// Closes duplication controls
            /// </summary>
            private void StopPropertyDuplication()
            {
                MenuState = QuickActionMenuState.WheelMenuControl;
                propertyWheel.Visible = true;
                dupeWheel.Visible = false;
            }

            /// <summary>
            /// Handles selection for an entry in the duplication selection wheel
            /// </summary>
            private void HandleDupeSelection(PropertyWheelEntryBase selection)
            {
                var shortcutEntry = selection as PropertyWheelShortcutEntry;
                shortcutEntry.ShortcutAction();
            }

            /// <summary>
            /// Selects all enabled properties in the current target and copies them to the clipboard
            /// </summary>
            private void CopyAllProperties()
            {
                Duplicator.CopyAllProperties();
            }

            private void PasteCopiedProperties()
            {
                int pastedProperties = Duplicator.TryPasteCopiedProperties();
                ExceptionHandler.SendChatMessage($"Pasted {pastedProperties} properties");
            }
        }
    }
}