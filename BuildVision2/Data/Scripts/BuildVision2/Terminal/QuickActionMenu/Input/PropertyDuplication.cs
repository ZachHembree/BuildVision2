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
        private void StartPropertyDuplication()
        {
            MenuState = QuickActionMenuState.PropertyDuplication;
            propertyWheel.Visible = false;
            dupeWheel.Visible = true;
        }

        private void StopPropertyDuplication()
        {
            MenuState = QuickActionMenuState.PropertySelection;
            propertyWheel.Visible = true;
            dupeWheel.Visible = false;
        }

        private void HandleDupeSelection(QuickActionEntryBase selection)
        {
            var shortcutEntry = selection as QuickActionShortcutEntry;
            shortcutEntry.ShortcutAction();
        }

        private void CopyAllProperties()
        {
            SelectAllProperties();
            CopySelectedProperties();
            ClearPropertySelection();
        }

        private void SelectAllProperties()
        {
            foreach (QuickActionEntryBase baseEntry in propertyWheel)
            {
                var entry = baseEntry as QuickBlockPropertyEntry;

                if (entry != null && entry.Enabled)
                    entry.IsSelectedForCopy = true;
            }
        }

        private void ClearPropertySelection()
        {
            foreach (QuickActionEntryBase baseEntry in propertyWheel)
            {
                var entry = baseEntry as QuickBlockPropertyEntry;

                if (entry != null)
                    entry.IsSelectedForCopy = false;
            }
        }

        private void CopySelectedProperties()
        {
            MyUtils.Swap(ref copiedProperties, ref lastCopiedProperties);
            var propertyList = copiedProperties.propertyList;

            copiedProperties.blockTypeID = block.TypeID;
            propertyList.Clear();

            foreach (QuickActionEntryBase baseEntry in propertyWheel)
            {
                var entry = baseEntry as QuickBlockPropertyEntry;
                var property = entry?.BlockMember as IBlockProperty;

                if (property != null && entry.Enabled && entry.IsSelectedForCopy)
                {
                    propertyList.Add(property.GetPropertyData());
                }
            }

            ExceptionHandler.SendChatMessage($"Copied {propertyList.Count} block properties.");
        }

        private void PasteCopiedProperties()
        {
            if (copiedProperties.blockTypeID == block.TypeID)
            {
                int importCount = block.ImportSettings(copiedProperties);
                ExceptionHandler.SendChatMessage($"Pasted {importCount} block properties.");
            }
            else
                ExceptionHandler.SendChatMessage("Pasted block properties incompatible.");
        }
    }
}