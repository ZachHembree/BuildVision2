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
        private readonly HashSet<IMySlimBlock> blockHashBuffer;

        public TerminalGrid()
        {
            groupBuffer = new List<IMyBlockGroup>(10);
            groupBlockBuffer = new List<IMyTerminalBlock>(10);
            blockHashBuffer = new HashSet<IMySlimBlock>();
        }

        private void OnGridClose(IMyEntity entity) =>
            Reset();

        public void SetGrid(IMyCubeGrid grid, bool temp = false)
        {
            if (grid != Grid && !temp)
            {
                if (Grid != null)
                    Grid.OnMarkForClose -= OnGridClose;

                Reset();
                Grid = grid;

                if (Grid != null && !temp)
                {
                    TerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                    Grid.OnMarkForClose += OnGridClose;
                }
            }
        }

        public void GetGroupNamesForBlock(IMyTerminalBlock block, List<string> groups)
        {
            TerminalSystem.GetBlockGroups(groupBuffer);

            for (int g = 0; g < groupBuffer.Count; g++)
            {
                groupBlockBuffer.Clear();
                groupBuffer[g].GetBlocks(groupBlockBuffer, x => x == block);

                if (groupBlockBuffer.Count > 0)
                    groups.Add(groupBuffer[g].Name);
            }

            groupBuffer.Clear();
            groupBlockBuffer.Clear();
        }

        public void GetGroupsForBlock(IMyTerminalBlock block, List<IMyBlockGroup> groups)
        {
            TerminalSystem.GetBlockGroups(groupBuffer);

            for (int g = 0; g < groupBuffer.Count; g++)
            {
                groupBlockBuffer.Clear();
                groupBuffer[g].GetBlocks(groupBlockBuffer, x => x == block);

                if (groupBlockBuffer.Count > 0)
                    groups.Add(groupBuffer[g]);
            }

            groupBuffer.Clear();
            groupBlockBuffer.Clear();
        }

        /// <summary>
        /// Non-allocating version of Sandbox.Game.Entities.MyCubeGrid.GetBlocksInsideSphere()
        /// </summary>
        public void GetBlocksInsideSphere(IMyCubeGrid grid, List<IMySlimBlock> blockList, ref BoundingSphereD sphere)
        {
            if (grid.PositionComp != null)
            {
                BoundingBoxD aabb = BoundingBoxD.CreateFromSphere(sphere);
                MatrixD matrix = grid.PositionComp.WorldMatrixNormalizedInv;
                Vector3D result;

                Vector3D.Transform(ref sphere.Center, ref matrix, out result);
                BoundingSphere localSphere = new BoundingSphere(result, (float)sphere.Radius);
                BoundingBox boundingBox = BoundingBox.CreateFromSphere(localSphere);
                double gridSizeR = 1d / grid.GridSize;

                Vector3I searchMin = new Vector3I
                (
                    (int)Math.Round(boundingBox.Min.X * gridSizeR),
                    (int)Math.Round(boundingBox.Min.Y * gridSizeR),
                    (int)Math.Round(boundingBox.Min.Z * gridSizeR)
                );
                Vector3I searchMax = new Vector3I
                (
                    (int)Math.Round(boundingBox.Max.X * gridSizeR),
                    (int)Math.Round(boundingBox.Max.Y * gridSizeR),
                    (int)Math.Round(boundingBox.Max.Z * gridSizeR)
                );

                int blockCount = (grid.Max - grid.Min).Volume();
                Vector3I start = Vector3I.Max(Vector3I.Min(searchMin, searchMax), grid.Min);
                Vector3I end = Vector3I.Min(Vector3I.Max(searchMin, searchMax), grid.Max);

                var gridIterator = new Vector3I_RangeIterator(ref start, ref end);
                Vector3I next = gridIterator.Current;

                blockHashBuffer.Clear();

                while (gridIterator.IsValid())
                {
                    IMySlimBlock cube = grid.GetCubeBlock(next);
                    float gridSizeHalf = grid.GridSize / 2f;

                    if (cube != null)
                    {
                        var cubeBounds = new BoundingBox((cube.Min * grid.GridSize) - gridSizeHalf, (cube.Max * grid.GridSize) + gridSizeHalf);

                        if (cubeBounds.Intersects(localSphere))
                            blockHashBuffer.Add(cube);
                    }

                    gridIterator.GetNext(out next);
                }

                blockList.Clear();
                blockList.EnsureCapacity(blockHashBuffer.Count);

                foreach (IMySlimBlock block in blockHashBuffer)
                    blockList.Add(block);
            }
        }

        public void Reset()
        {
            Grid = null;
            TerminalSystem = null;
            groupBuffer.Clear();
            groupBlockBuffer.Clear();
            blockHashBuffer.Clear();
        }
    }
}
