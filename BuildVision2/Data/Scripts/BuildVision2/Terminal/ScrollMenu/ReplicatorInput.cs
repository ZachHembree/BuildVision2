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
        private void HandleReplicatorInput()
        {
            if (BvBinds.SelectAll.IsNewPressed)
                SelectAllProperties();

            if (BvBinds.Select.IsNewPressed)
                Selection.Replicating = !Selection.Replicating;
        }

        public List<PropertyData> GetReplicationRange()
        {
            var propertyData = new List<PropertyData>();

            for (int n = 0; n < Count; n++)
            {
                if (scrollBody.List[n].Replicating)
                {
                    var property = scrollBody.List[n].BlockMember as IBlockProperty;

                    if (property != null)
                        propertyData.Add(property.GetPropertyData());
                }
            }

            return propertyData;
        }

        private void ToggleReplicationMode()
        {
            if (MenuMode == ScrollMenuModes.Copy)
            {
                MenuMode = ScrollMenuModes.Control;
                CloseProp();
            }
            else
            {
                MenuMode = ScrollMenuModes.Copy;
                DeselectAllProperties();
            }
        }

        private void SelectAllProperties()
        {
            for (int n = 0; n < Count; n++)
            {
                if (scrollBody.List[n].Enabled)
                    scrollBody.List[n].Replicating = true;
            }
        }

        private void DeselectAllProperties()
        {
            for (int n = 0; n < Count; n++)
                scrollBody.List[n].Replicating = false;
        }
    }
}