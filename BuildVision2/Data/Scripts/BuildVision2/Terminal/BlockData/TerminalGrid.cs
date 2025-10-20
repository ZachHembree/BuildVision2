using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

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

		public void SetGrid(IMyCubeGrid grid)
		{
			if (grid != Grid)
			{
				if (Grid != null)
					Grid.OnMarkForClose -= OnGridClose;

				Reset();
				Grid = grid;

				if (Grid != null)
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
		public void GetBlocksInsideSphere(List<IMySlimBlock> blockList, ref BoundingSphereD sphere)
		{
			GetBlocksInsideSphere(Grid, blockList, blockHashBuffer, ref sphere);
		}

		/// <summary>
		/// Non-allocating version of Sandbox.Game.Entities.MyCubeGrid.GetBlocksInsideSphere()
		/// </summary>
		public static void GetBlocksInsideSphere(IMyCubeGrid grid, List<IMySlimBlock> blockList,
			HashSet<IMySlimBlock> blockHashBuffer, ref BoundingSphereD sphere)
		{
			if (grid.PositionComp != null)
			{
				MatrixD matrix = grid.PositionComp.WorldMatrixNormalizedInv;
				Vector3D result;

				Vector3D.Transform(ref sphere.Center, ref matrix, out result);
				BoundingSphere localSphere = new BoundingSphere(result, (float)sphere.Radius);
				BoundingBox boundingBox = BoundingBox.CreateFromSphere(localSphere);
				double rcpGridSize = 1d / grid.GridSize;

				Vector3I searchMin = new Vector3I
				(
					(int)Math.Round(boundingBox.Min.X * rcpGridSize),
					(int)Math.Round(boundingBox.Min.Y * rcpGridSize),
					(int)Math.Round(boundingBox.Min.Z * rcpGridSize)
				);
				Vector3I searchMax = new Vector3I
				(
					(int)Math.Round(boundingBox.Max.X * rcpGridSize),
					(int)Math.Round(boundingBox.Max.Y * rcpGridSize),
					(int)Math.Round(boundingBox.Max.Z * rcpGridSize)
				);

				Vector3I start = Vector3I.Max(Vector3I.Min(searchMin, searchMax), grid.Min);
				Vector3I end = Vector3I.Min(Vector3I.Max(searchMin, searchMax), grid.Max);

				var gridIterator = new Vector3I_RangeIterator(ref start, ref end);
				Vector3I next = gridIterator.Current;

				blockHashBuffer.Clear();

				while (gridIterator.IsValid())
				{
					IMySlimBlock cube = grid.GetCubeBlock(next);
					float gridSizeHalf = grid.GridSize * 0.5f;

					if (cube != null)
					{
						var cubeBounds = new BoundingBox(
							(cube.Min * grid.GridSize) - gridSizeHalf,
							(cube.Max * grid.GridSize) + gridSizeHalf
						);

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
