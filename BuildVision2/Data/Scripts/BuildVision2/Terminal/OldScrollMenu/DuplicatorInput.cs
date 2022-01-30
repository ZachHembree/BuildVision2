using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvScrollMenu : HudElementBase
    {
        /// <summary>
        /// Updates input for copy menu mode
        /// </summary>
        private void HandleDuplicatorInput()
        {
            if (BvBinds.SelectAll.IsNewPressed)
                SelectAllProperties();

            if (BvBinds.Select.IsNewPressed)
                Selection.Copying = !Selection.Copying;
        }

        /// <summary>
        /// Returns the range of properties selected as a serialized list of data.
        /// </summary>
        public List<PropertyData> GetDuplicationRange()
        {
            var propertyData = new List<PropertyData>();

            for (int n = 0; n < Count; n++)
            {
                BvPropertyBox propertyBox = scrollBody.Collection[n].Element;

                if (propertyBox.Copying)
                {
                    var property = propertyBox.BlockMember as IBlockProperty;

                    if (property != null)
                        propertyData.Add(property.GetPropertyData());
                }
            }

            return propertyData;
        }

        /// <summary>
        /// Toggles the menu between copying and property control
        /// </summary>
        private void ToggleDuplicationMode()
        {
            if (MenuMode == ScrollMenuModes.Dupe)
            {
                MenuMode = ScrollMenuModes.Control;
                DeselectAllProperties();
            }
            else
            {
                MenuMode = ScrollMenuModes.Dupe;                
                CloseProp();
            }
        }

        /// <summary>
        /// Selects all enabled properties for duplication
        /// </summary>
        private void SelectAllProperties()
        {
            for (int n = 0; n < Count; n++)
            {
                if (scrollBody.Collection[n].Enabled)
                    scrollBody.Collection[n].Element.Copying = true;
            }
        }

        /// <summary>
        /// Deselects all properties for duplication
        /// </summary>
        private void DeselectAllProperties()
        {
            for (int n = 0; n < Count; n++)
                scrollBody.Collection[n].Element.Copying = false;
        }
    }
}