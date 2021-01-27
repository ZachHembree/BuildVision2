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
                public static void RegisterNodes(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, bool preregister)
                {
                    bool wereFastUnregistered = false;

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n];

                        if (node._registered)
                            throw new Exception("HUD Element already registered!");

                        if (node.wasFastUnregistered && newParent != node.reregParent)
                        {
                            node.reregParent.RemoveChild(node);
                            node.wasFastUnregistered = false;
                            node.reregParent = null;
                        }

                        if (node.wasFastUnregistered)
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
                            node._registered = false;
                        }
                        else
                        {
                            node.parentZOffset = newParent.ZOffset;
                            node.parentScale = newParent.Scale;
                            node.parentVisible = newParent.Visible;
                        }

                        if (!node.wasFastUnregistered)
                        {
                            HudMain.RefreshDrawList = true;
                            children.Add(node);
                        }

                        node.wasFastUnregistered = preregister;
                    }
                }

                /// <summary>
                /// Used internally quickly register a list of child nodes to a parent.
                /// </summary>
                public static void RegisterNodes<TCon, TNode>(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<TCon> nodes, bool preregister)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    bool wereFastUnregistered = false;

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n].Element;

                        if (node._registered)
                            throw new Exception("HUD Element already registered!");

                        if (node.wasFastUnregistered && newParent != node.reregParent)
                        {
                            node.reregParent.RemoveChild(node);
                            node.wasFastUnregistered = false;
                            node.reregParent = null;
                        }

                        if (node.wasFastUnregistered)
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
                            node._registered = false;
                        }
                        else
                        {
                            node.parentZOffset = newParent.ZOffset;
                            node.parentScale = newParent.Scale;
                            node.parentVisible = newParent.Visible;
                        }

                        if (!node.wasFastUnregistered)
                        {
                            HudMain.RefreshDrawList = true;
                            children.Add(node);
                        }

                        node.wasFastUnregistered = preregister;
                    }
                }

                /// <summary>
                /// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
                /// specified in the node list from the child list.
                /// </summary>
                public static void UnregisterNodes(HudParentBase parent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, int index, int count, bool fast)
                {
                    int conEnd = index + count - 1;

                    if (!(index >= 0 && count >= 0 && index < nodes.Count && conEnd <= nodes.Count))
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

                        HudMain.RefreshDrawList = true;
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
                            node.wasFastUnregistered = true;
                        }
                        else
                        {
                            node.reregParent = null;
                            node.wasFastUnregistered = false;
                        }

                        node.Parent = null;
                        node._registered = false;
                        node.parentZOffset = 0;
                        node.parentVisible = false;
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
                    int conEnd = index + count - 1;

                    if (!(index >= 0 && count >= 0 && index < nodes.Count && conEnd <= nodes.Count))
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

                        HudMain.RefreshDrawList = true;
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
                            node.wasFastUnregistered = true;
                        }
                        else
                        {
                            node.reregParent = null;
                            node.wasFastUnregistered = false;
                        }

                        node.Parent = null;
                        node._registered = false;
                        node.parentZOffset = 0;
                        node.parentVisible = false;
                    }
                }
            }
        }
    }
}