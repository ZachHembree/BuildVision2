using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using VRageMath;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using System;

namespace DarkHelmet.BuildVision2
{
    public class TerminalGrid
    {
        public IMyCubeGrid Grid { get; private set; }

        public IMyGridTerminalSystem TerminalSystem { get; private set; }

        private readonly List<IMyBlockGroup> groupBuffer;
        private readonly List<IMyTerminalBlock> groupBlockBuffer;

        public TerminalGrid()
        {
            groupBuffer = new List<IMyBlockGroup>(10);
            groupBlockBuffer = new List<IMyTerminalBlock>(10);
        }

        private void OnGridClose(IMyEntity entity) =>
            Clear();

        public void SetGrid(IMyCubeGrid grid)
        {
            if (grid != Grid)
            {
                if (Grid != null)
                    Grid.OnMarkForClose -= OnGridClose;

                Clear();
                Grid = grid;

                if (Grid != null)
                {
                    TerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                    Grid.OnMarkForClose += OnGridClose;
                }
            }
        }

        public void GetGroupsForBlock(IMyTerminalBlock block, List<IMyBlockGroup> groups)
        {
            TerminalSystem.GetBlockGroups(groupBuffer);

            for (int g = 0; g < groupBuffer.Count; g++)
            {
                groupBlockBuffer.Clear();
                groupBuffer[g].GetBlocks(groupBlockBuffer);

                if (groupBlockBuffer.Contains(block))
                    groups.Add(groupBuffer[g]);
            }

            groupBuffer.Clear();
            groupBlockBuffer.Clear();
        }

        public void Clear()
        {
            Grid = null;
            TerminalSystem = null;
            groupBuffer.Clear();
            groupBlockBuffer.Clear();
        }
    }
}
