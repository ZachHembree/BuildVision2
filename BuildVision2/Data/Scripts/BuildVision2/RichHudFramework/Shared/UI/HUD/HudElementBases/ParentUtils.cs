using System;

namespace RichHudFramework
{
	namespace UI
	{
		using static NodeConfigIndices;
		using RichHudFramework.UI.Server;
		using System.Collections.Generic;
		using Client;

		public abstract partial class HudParentBase
		{
			/// <summary>
			/// Utilities used internally to access parent node members
			/// </summary>
			/// <exclude/>
			protected static partial class ParentUtils
			{
				/// <summary>
				/// Used internally quickly register a list of child nodes to a parent.
				/// </summary>
				public static void RegisterNodes(HudParentBase newParent, IReadOnlyList<HudNodeBase> nodes)
				{
					if (nodes.Count == 0)
						return;

					newParent.children.EnsureCapacity(newParent.children.Count + nodes.Count);

					for (int n = 0; n < nodes.Count; n++)
					{
						HudNodeBase node = nodes[n];
						node._dataHandle[0].Item4 = newParent.DataHandle;
						node.HudSpace = newParent.HudSpace;

						newParent.childHandles.Add(node.DataHandle);
						newParent.children.Add(node);
					}

					if ((newParent.Config[StateID] & newParent.Config[VisMaskID]) == newParent.Config[VisMaskID])
					{
						// Depending on where this is called, the frame number might be off by one
						uint[] rootConfig = HudMain.Instance._root._config;
						bool isActive = Math.Abs((int)newParent.Config[FrameNumberID] - (int)rootConfig[FrameNumberID]) < 2;

						if (isActive && (rootConfig[StateID] & (uint)HudElementStates.IsStructureStale) == 0)
						{
							rootConfig[StateID] |= (uint)HudElementStates.IsStructureStale;
						}
					}
				}

				/// <summary>
				/// Used internally quickly register a list of child nodes to a parent.
				/// </summary>
				public static void RegisterNodes<TCon, TNode>(HudParentBase newParent, IReadOnlyList<TCon> nodes)
					where TCon : IHudNodeContainer<TNode>, new()
					where TNode : HudNodeBase
				{
					if (nodes.Count == 0)
						return;

					newParent.children.EnsureCapacity(newParent.children.Count + nodes.Count);

					for (int n = 0; n < nodes.Count; n++)
					{
						HudNodeBase node = nodes[n].Element;
						node._dataHandle[0].Item4 = newParent.DataHandle;
						node.HudSpace = newParent.HudSpace;

						newParent.childHandles.Add(node.DataHandle);
						newParent.children.Add(node);
					}

					if ((newParent.Config[StateID] & newParent.Config[VisMaskID]) == newParent.Config[VisMaskID])
					{
						// Depending on where this is called, the frame number might be off by one
						uint[] rootConfig = HudMain.Instance._root._config;
						bool isActive = Math.Abs((int)newParent.Config[FrameNumberID] - (int)rootConfig[FrameNumberID]) < 2;

						if (isActive && (rootConfig[StateID] & (uint)HudElementStates.IsStructureStale) == 0)
						{
							rootConfig[StateID] |= (uint)HudElementStates.IsStructureStale;
						}
					}
				}

				/// <summary>
				/// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
				/// specified in the node list from the child list.
				/// </summary>
				public static void UnregisterNodes(HudParentBase parent, IReadOnlyList<HudNodeBase> nodes, int index, int count)
				{
					if (count > 0)
					{
						int conEnd = index + count - 1;

						if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
							throw new Exception("Specified indices are out of range.");

						if (parent == null)
							throw new Exception("Parent cannot be null");

						for (int i = index; i <= conEnd; i++)
						{
							int start = 0;

							while (start < parent.children.Count && parent.children[start] != nodes[i])
								start++;

							if (parent.children[start] == nodes[i])
							{
								int j = start, end = start;

								while (j < parent.children.Count && i <= conEnd && parent.children[j] == nodes[i])
								{
									end = j;
									i++;
									j++;
								}

								parent.childHandles.RemoveRange(start, end - start + 1);
								parent.children.RemoveRange(start, end - start + 1);
							}
						}
					}
				}

				/// <summary>
				/// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
				/// specified in the node list from the child list.
				/// </summary>
				public static void UnregisterNodes<TCon, TNode>(HudParentBase parent, IReadOnlyList<TCon> nodes, int index, int count)
					where TCon : IHudNodeContainer<TNode>, new()
					where TNode : HudNodeBase
				{
					if (count > 0)
					{
						int conEnd = index + count - 1;
						var children = parent.children;

						if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
							throw new Exception("Specified indices are out of range.");

						if (parent == null)
							throw new Exception("Parent cannot be null");

						for (int i = index; i <= conEnd; i++)
						{
							int start = 0;

							while (start < children.Count && children[start] != nodes[i].Element)
								start++;

							if (children[start] == nodes[i].Element)
							{
								int j = start, end = start;

								while (j < children.Count && i <= conEnd && children[j] == nodes[i].Element)
								{
									end = j;
									i++;
									j++;
								}

								parent.childHandles.RemoveRange(start, end - start + 1);
								children.RemoveRange(start, end - start + 1);
							}
						}
					}
				}
			}
		}
	}
}