using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
	public class BlockFinder
	{
		public struct TargetCandidate
		{
			public IMyTerminalBlock block;
			public float distance;
		}

		/// <summary>
		/// The line of the raycast for the last successful update
		/// </summary>
		public LineD LastTargetLine { get; private set; }

		/// <summary>
		/// Returns a list of the last successfully acquired targets sorted by distance priority
		/// </summary>
		public IReadOnlyList<TargetCandidate> SortedTargets => sortedTargets;

		private readonly HashSet<IMySlimBlock> blockHashBuffer;
		private readonly List<IMySlimBlock> targetBuffer;

		private List<TargetCandidate> sortedTargets;
		private List<TargetCandidate> candidateBuffer;
		private readonly List<uint> sortBuffer;

		public BlockFinder()
		{
			blockHashBuffer = new HashSet<IMySlimBlock>();
			targetBuffer = new List<IMySlimBlock>();
			sortedTargets = new List<TargetCandidate>();
			candidateBuffer = new List<TargetCandidate>();
			sortBuffer = new List<uint>();
		}

		public void Clear()
		{
			LastTargetLine = default(LineD);
			blockHashBuffer.Clear();
			targetBuffer.Clear();
			sortedTargets.Clear();
			candidateBuffer.Clear();
			sortBuffer.Clear();
		}

		/// <summary>
		/// Attempts to acquire terminal blocks by casting a ray along the given line segment. Returns true 
		/// on success and writes results to SortedTargets.
		/// </summary>
		public bool TryUpdateTargets(LineD targetLine)
		{
			IMyCubeGrid cubeGrid;
			IHitInfo rayInfo;

			if (LocalPlayer.TryGetTargetedGrid(targetLine, out cubeGrid, out rayInfo))
			{
				// Retrieve blocks within about half a block of the ray intersection point.
				targetLine = new LineD(targetLine.From, rayInfo.Position);
				var sphere = new BoundingSphereD(targetLine.To, (cubeGrid.GridSizeEnum == MyCubeSize.Large) ? 1.3 : .3);

				targetBuffer.Clear();
				TerminalGrid.GetBlocksInsideSphere(cubeGrid, targetBuffer, blockHashBuffer, ref sphere);
				GetTargetCandidates(targetBuffer, candidateBuffer);

				// Check distances to bounding boxes and sort by closest
				GetSortedTargets(targetLine);

				LastTargetLine = targetLine;
				return sortedTargets.Count > 0;
			}
			else
				return false;
		}

		private static void GetTargetCandidates(IReadOnlyList<IMySlimBlock> targetBuffer, List<TargetCandidate> candidates)
		{
			candidates.Clear();

			foreach (IMySlimBlock slimBlock in targetBuffer)
			{
				IMyCubeBlock cubeBlock = slimBlock?.FatBlock;

				if (cubeBlock != null)
				{
					var topBlock = cubeBlock as IMyAttachableTopBlock;

					if (topBlock != null)
						cubeBlock = topBlock.Base;
				}

				var tBlock = cubeBlock as IMyTerminalBlock;

				if (tBlock != null)
					candidates.Add(new TargetCandidate { block = tBlock });
			}
		}

		private void GetSortedTargets(LineD targetLine)
		{
			sortBuffer.Clear();

			double currentDist = double.PositiveInfinity, currentCenterDist = double.PositiveInfinity;
			ushort distKey = ushort.MaxValue;

			for (ushort i = 0; i < candidateBuffer.Count; i++)
			{
				TargetCandidate candidate = candidateBuffer[i];
				// Project ray intersection point into object local coordinate space for precise localAABB intersection
				MatrixD invWorldMatrix = candidate.block.WorldMatrixNormalizedInv;
				Vector3D worldCenter = candidate.block.WorldAABB.Center;
				Vector3 localLineEnd = Vector3D.TransformNormal(targetLine.To - worldCenter, ref invWorldMatrix);

				// Find shortest dist between the bb and the intersection.
				// Clamp minimum dimensions for small blocks
				Vector3 dim = Vector3.Max(candidate.block.LocalAABB.Size, new Vector3(0.25f));
				Vector3 halfSize = 0.5f * dim;

				BoundingBox box = new BoundingBox(-halfSize, halfSize);
				double newDist = Math.Round(box.DistanceSquared(localLineEnd), 3),
					newCenterDist = Math.Round(localLineEnd.LengthSquared(), 3);

				// If this is a terminal block, check to see if this block is any closer than the last.
				// If the distance to the bb is zero, use the center dist, favoring smaller blocks.
				if ((currentDist > 0d && newDist < currentDist)
					|| (Math.Abs(currentDist - newDist) < 0.02 && newCenterDist < currentCenterDist)
				)
				{
					currentDist = newDist;
					currentCenterDist = newCenterDist;
					distKey--;
				}

				candidate.distance = (float)newDist;
				candidateBuffer[i] = candidate;

				uint sortKey = (uint)distKey << 16 | i;
				sortBuffer.Add(sortKey);
			}

			sortBuffer.Sort();
			sortedTargets.Clear();

			for (int i = 0; i < sortBuffer.Count; i++)
			{
				int keyIndex = (int)(sortBuffer[i] & 0xFFFFu);
				sortedTargets.Add(candidateBuffer[keyIndex]);
			}
		}
	}
}
