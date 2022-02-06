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
                if (BvBinds.Select.IsReleased)
                {
                    var shortcutEntry = selection as PropertyWheelShortcutEntry;
                    shortcutEntry.ShortcutAction();
                }
            }

            /// <summary>
            /// Opens property list in duplication mode
            /// </summary>
            private void OpenDupeList()
            {
                quickActionMenu.OpenPropertyList();
                MenuState |= QuickActionMenuState.PropertyDuplication;
            }

            /// <summary>
            /// Selects all properties for duplication and opens list in duplication mode
            /// </summary>
            private void OpenDupeListAndSelectAll()
            {
                quickActionMenu.OpenPropertyList();
                Target.Duplicator.SelectAllProperties();
                MenuState |= QuickActionMenuState.PropertyDuplication;
            }

            /// <summary>
            /// Attempts to restore original property settings after paste
            /// </summary>
            private void UndoPropertyPaste()
            {
                int copiedProperties = Target.Duplicator.TryUndoPaste();

                if (copiedProperties > 0)
                    menuBody.ShowNotification($"Paste undone");
            }

            /// <summary>
            /// Copies properties selected for duplication to clipboard
            /// </summary>
            private void CopySelectedProperties()
            {
                int copiedProperties = Target.Duplicator.CopySelectedProperties();
                menuBody.ShowNotification($"Copied {copiedProperties} properties");
            }

            /// <summary>
            /// Clears duplication selection
            /// </summary>
            private void ClearSelection()
            {
                Target.Duplicator.ClearSelection();
            }

            /// <summary>
            /// Selects all enabled properties in the current target and copies them to the clipboard
            /// </summary>
            private void CopyAllProperties(bool includeName = true)
            {
                int copiedProperties = Target.Duplicator.CopyAllProperties(includeName);
                menuBody.ShowNotification($"Copied {copiedProperties} properties");
            }

            /// <summary>
            /// Writes previously copied properties to target block
            /// </summary>
            private void PasteCopiedProperties()
            {
                int pastedProperties = Target.Duplicator.TryPasteCopiedProperties();
                
                if (pastedProperties == -1)
                    menuBody.ShowNotification($"Copied properties incompatible");
                else if (pastedProperties > 0)
                    menuBody.ShowNotification($"Pasted {pastedProperties} properties");
            }
        }
    }
}