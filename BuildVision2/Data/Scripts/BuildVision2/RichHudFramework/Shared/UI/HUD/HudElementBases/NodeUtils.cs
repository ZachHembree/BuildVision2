using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        public abstract partial class HudNodeBase
        {
            /// <summary>
            /// Collection of utilities used internally to manage bulk element registration/unregistration
            /// </summary>
            protected static class NodeUtils
            {
                /// <summary>
                /// Used internally quickly register a list of child nodes to a parent.
                /// </summary>
                public static void RegisterNodes(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, bool preregister, bool canPreload)
                {
                    bool wereFastUnregistered = false;

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n];

                        if ((node.State & HudElementStates.IsRegistered) > 0)
                            throw new Exception("HUD Element already registered!");

                        if ((node.State & HudElementStates.WasFastUnregistered) > 0 && newParent != node.reregParent)
                        {
                            node.reregParent.RemoveChild(node);
                            node.State &= ~HudElementStates.WasFastUnregistered;
                            node.reregParent = null;
                        }

                        if ((node.State & HudElementStates.WasFastUnregistered) > 0)
                            wereFastUnregistered = true;
                    }

                    if (!wereFastUnregistered)
                        children.EnsureCapacity(children.Count + nodes.Count);

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n];

                        if (preregister)
                        {
                            node.reregParent = newParent;
                            node.Parent = null;
                            node.State &= ~HudElementStates.IsRegistered;
                        }
                        else
                        {
                            node.Parent = newParent;
                            node.State |= HudElementStates.IsRegistered;
                            node.layerData.parentZOffset = newParent.ZOffset;
                            node.parentScale = newParent.Scale;
                            node.ParentVisible = newParent.Visible;
                        }

                        if (!((node.State & HudElementStates.WasFastUnregistered) > 0))
                        {
                            children.Add(node);
                        }

                        if (canPreload)
                            node.State |= HudElementStates.CanPreload;
                        else
                            node.State &= ~HudElementStates.CanPreload;

                        if (preregister)
                            node.State |= HudElementStates.WasFastUnregistered;
                        else
                            node.State &= ~HudElementStates.WasFastUnregistered;
                    }
                }

                /// <summary>
                /// Used internally quickly register a list of child nodes to a parent.
                /// </summary>
                public static void RegisterNodes<TCon, TNode>(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<TCon> nodes, bool preregister, bool canPreload)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    bool wereFastUnregistered = false;

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n].Element;

                        if ((node.State & HudElementStates.IsRegistered) > 0)
                            throw new Exception("HUD Element already registered!");

                        if ((node.State & HudElementStates.WasFastUnregistered) > 0 && newParent != node.reregParent)
                        {
                            node.reregParent.RemoveChild(node);
                            node.State &= ~HudElementStates.WasFastUnregistered;
                            node.reregParent = null;
                        }

                        if ((node.State & HudElementStates.WasFastUnregistered) > 0)
                            wereFastUnregistered = true;
                    }

                    if (!wereFastUnregistered)
                        children.EnsureCapacity(children.Count + nodes.Count);
                
                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n].Element;

                        if (preregister)
                        {
                            node.reregParent = newParent;
                            node.Parent = null;
                            node.State &= ~HudElementStates.IsRegistered;
                        }
                        else
                        {
                            node.Parent = newParent;
                            node.State |= HudElementStates.IsRegistered;
                            node.layerData.parentZOffset = newParent.ZOffset;
                            node.parentScale = newParent.Scale;
                            node.ParentVisible = newParent.Visible;
                        }

                        if (!((node.State & HudElementStates.WasFastUnregistered) > 0))
                        {
                            children.Add(node);
                        }

                        if (canPreload)
                            node.State |= HudElementStates.CanPreload;
                        else
                            node.State &= ~HudElementStates.CanPreload;

                        if (preregister)
                            node.State |= HudElementStates.WasFastUnregistered;
                        else
                            node.State &= ~HudElementStates.WasFastUnregistered;
                    }
                }

                /// <summary>
                /// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
                /// specified in the node list from the child list.
                /// </summary>
                public static void UnregisterNodes(HudParentBase parent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, int index, int count, bool fast)
                {
                    if (count > 0)
                    {
                        int conEnd = index + count - 1;

                        if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
                            throw new Exception("Specified indices are out of range.");

                        if (parent == null)
                            throw new Exception("Parent cannot be null");

                        if (!fast)
                        {
                            for (int i = index; i <= conEnd; i++)
                            {
                                int start = 0;

                                while (start < children.Count && children[start] != nodes[i])
                                    start++;

                                if (children[start] == nodes[i])
                                {
                                    int j = start, end = start;

                                    while (j < children.Count && i <= conEnd && children[j] == nodes[i])
                                    {
                                        end = j;
                                        i++;
                                        j++;
                                    }

                                    children.RemoveRange(start, end - start + 1);
                                }
                            }
                        }

                        for (int n = index; n < count; n++)
                        {
                            HudNodeBase node = nodes[n];
                            HudParentBase nodeParent = node._parent ?? node.reregParent;

                            if (nodeParent != parent)
                                throw new Exception("The child node specified is not registered to the parent given.");

                            if (fast)
                            {
                                node.reregParent = node._parent;
                                node.State |= HudElementStates.WasFastUnregistered;
                            }
                            else
                            {
                                node.reregParent = null;
                                node.State &= ~HudElementStates.WasFastUnregistered;
                            }

                            node.Parent = null;
                            node.State &= ~HudElementStates.IsRegistered;
                            node.layerData.parentZOffset = 0;
                            node.ParentVisible = false;
                        }
                    }
                }

                /// <summary>
                /// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
                /// specified in the node list from the child list.
                /// </summary>
                public static void UnregisterNodes<TCon, TNode>(HudParentBase parent, List<HudNodeBase> children, IReadOnlyList<TCon> nodes, int index, int count, bool fast)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    if (count > 0)
                    {
                        int conEnd = index + count - 1;

                        if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
                            throw new Exception("Specified indices are out of range.");

                        if (parent == null)
                            throw new Exception("Parent cannot be null");

                        if (!fast)
                        {
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

                                    children.RemoveRange(start, end - start + 1);
                                }
                            }
                        }

                        for (int n = index; n < count; n++)
                        {
                            HudNodeBase node = nodes[n].Element;
                            HudParentBase nodeParent = node._parent ?? node.reregParent;

                            if (nodeParent != parent)
                                throw new Exception("The child node specified is not registered to the parent given.");

                            if (fast)
                            {
                                node.reregParent = node._parent;
                                node.State |= HudElementStates.WasFastUnregistered;
                            }
                            else
                            {
                                node.reregParent = null;
                                node.State &= ~HudElementStates.WasFastUnregistered;
                            }

                            node.Parent = null;
                            node.State &= ~HudElementStates.IsRegistered;
                            node.layerData.parentZOffset = 0;
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
                                nodes[i].State &= ~state;
                        }
                        else
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].State |= state;
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
                                nodes[i].Element.State &= ~state;
                        }
                        else
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].Element.State |= state;
                        }
                    }
                }
            }
        }
    }
}