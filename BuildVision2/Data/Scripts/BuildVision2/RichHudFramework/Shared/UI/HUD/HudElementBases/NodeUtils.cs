using System;
using System.Collections.Generic;

namespace RichHudFramework
{
	namespace UI
	{
		using static RichHudFramework.UI.NodeConfigIndices;

		public abstract partial class HudNodeBase
		{
			/// <summary>
			/// Collection of utilities used internally to manage HUD nodes
			/// </summary>
			protected static class NodeUtils
			{
				/// <summary>
				/// Used internally quickly register a list of child nodes to a parent.
				/// </summary>
				public static void RegisterNodes(HudParentBase newParent, IReadOnlyList<HudNodeBase> nodes)
				{
					ParentUtils.RegisterNodes(newParent, nodes);

					for (int n = 0; n < nodes.Count; n++)
					{
						HudNodeBase node = nodes[n];
						node.Parent = newParent;

						node.Config[StateID] |= (uint)HudElementStates.IsRegistered;
						node.Config[StateID] &= ~(uint)HudElementStates.WasParentVisible;
					}
				}

				/// <summary>
				/// Used internally quickly register a list of child nodes to a parent.
				/// </summary>
				public static void RegisterNodes<TCon, TNode>(HudParentBase newParent, IReadOnlyList<TCon> nodes)
					where TCon : IHudElementContainer<TNode>, new()
					where TNode : HudNodeBase
				{
					ParentUtils.RegisterNodes<TCon, TNode>(newParent, nodes);

					for (int n = 0; n < nodes.Count; n++)
					{
						HudNodeBase node = nodes[n].Element;
						node.Parent = newParent;

						node.Config[StateID] |= (uint)HudElementStates.IsRegistered;
						node.Config[StateID] &= ~(uint)HudElementStates.WasParentVisible;
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
						ParentUtils.UnregisterNodes(parent, nodes, index, count);

						for (int n = index; n < count; n++)
						{
							HudNodeBase node = nodes[n];
							HudParentBase nodeParent = node.Parent;

							if (nodeParent != parent)
								throw new Exception("The child node specified is not registered to the parent given.");

							node.Parent = null;
							node._dataHandle[0].Item4 = null;
							node.Config[StateID] &= (uint)~(HudElementStates.IsRegistered | HudElementStates.WasParentVisible);
						}
					}
				}

				/// <summary>
				/// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
				/// specified in the node list from the child list.
				/// </summary>
				public static void UnregisterNodes<TCon, TNode>(HudParentBase parent, IReadOnlyList<TCon> nodes, int index, int count)
					where TCon : IHudElementContainer<TNode>, new()
					where TNode : HudNodeBase
				{
					if (count > 0)
					{
						ParentUtils.UnregisterNodes<TCon, TNode>(parent, nodes, index, count);

						for (int n = index; n < count; n++)
						{
							HudNodeBase node = nodes[n].Element;
							HudParentBase nodeParent = node.Parent;

							if (nodeParent != parent)
								throw new Exception("The child node specified is not registered to the parent given.");

							node.Parent = null;
							node._dataHandle[0].Item4 = null;
							node.Config[StateID] &= (uint)~(HudElementStates.IsRegistered | HudElementStates.WasParentVisible);
						}
					}
				}

				/// <summary>
				/// Used internally to modify the state of hud nodes
				/// </summary>
				public static void SetNodesState(HudElementStates state, bool mask, IReadOnlyList<HudNodeBase> nodes, int index, int count)
				{
					if (count > 0)
					{
						int end = index + count - 1;
						Utils.Debug.Assert(index >= 0 && end < nodes.Count, $"Range out of bounds. Index: {index}, End: {end}");

						if (mask)
						{
							for (int i = index; i <= end; i++)
							{
								nodes[i].Config[StateID] &= (uint)~state;
							}
						}
						else
						{
							for (int i = index; i <= end; i++)
							{
								nodes[i].Config[StateID] |= (uint)state;
							}
						}
					}
				}

				/// <summary>
				/// Used internally to modify the state of hud nodes
				/// </summary>
				public static void SetNodesState<TCon, TNode>(HudElementStates state, bool mask, IReadOnlyList<TCon> nodes, int index, int count)
					where TCon : IHudElementContainer<TNode>, new()
					where TNode : HudNodeBase
				{
					if (count > 0)
					{
						int end = index + count - 1;
						Utils.Debug.Assert(index >= 0 && end < nodes.Count, $"Range out of bounds. Index: {index}, End: {end}");

						if (mask)
						{
							for (int i = index; i <= end; i++)
							{
								nodes[i].Element.Config[StateID] &= (uint)~state;
							}
						}
						else
						{
							for (int i = index; i <= end; i++)
							{
								nodes[i].Element.Config[StateID] |= (uint)state;
							}
						}
					}
				}
			}
		}
	}
}