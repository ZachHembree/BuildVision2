using RichHudFramework.Game;
using System;
using System.Collections.Generic;
using VRage;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using BindMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    System.Func<bool> // IsReleased
>;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;

namespace RichHudFramework
{
    using BindGroupMembers = MyTuple<
        string, // Name                
        BindMembers[],// Binds
        Func<IList<int>, int, bool>, // DoesComboConflict
        Func<string, int[], bool, BindMembers?>, // TryRegisterBind
        Func<IList<BindDefinitionData>, BindMembers[]>, // TryLoadBindData
        MyTuple<
            Func<string, string[], bool, BindMembers?>, // TryRegisterBind2
            Func<BindDefinitionData[]>, // GetBindData
            Action, // HandleInput
            Action // ClearSubscribers
        >
    >;

    namespace UI.Server
    {
        public sealed partial class BindManager
        {
            /// <summary>
            /// Contains a set of keybinds independent of other groups and determines when/if those binds can be pressed. 
            /// While a group's own binds cannot conflict with oneanother, binds in other groups may.
            /// </summary>
            private partial class BindGroup : IBindGroup
            {
                public const int maxBindLength = 3;
                private const long holdTime = TimeSpan.TicksPerMillisecond * 500;

                public string Name { get; }
                public IBind this[int index] => keyBinds[index];
                public int Count => keyBinds.Count;

                private readonly List<Bind> keyBinds;
                private readonly List<IBind>[] controlMap; // X = controls; Y = associated binds
                private List<IControl> usedControls;
                private List<List<IBind>> bindMap; // X = used controls; Y = associated binds

                public BindGroup(string name)
                {
                    Name = name.ToLower();
                    controlMap = new List<IBind>[Controls.Count];

                    for (int n = 0; n < controlMap.Length; n++)
                        controlMap[n] = new List<IBind>();

                    keyBinds = new List<Bind>();
                    usedControls = new List<IControl>();
                    bindMap = new List<List<IBind>>();
                }

                public void ClearSubscribers()
                {
                    foreach (Bind bind in keyBinds)
                        bind.ClearSubscribers();
                }

                /// <summary>
                /// Updates bind presses each time its called. Key binds will not work if this isn't being run.
                /// </summary>
                public void HandleInput()
                {
                    if (keyBinds.Count > 0)
                    {
                        int bindsPressed = GetPressedBinds();

                        if (bindsPressed > 1)
                            DisambiguatePresses();

                        foreach (Bind bind in keyBinds)
                            bind.UpdatePress(bind.bindHits == bind.length && !bind.beingReleased);
                    }
                }

                /// <summary>
                /// Finds and counts number of pressed key binds.
                /// </summary>
                private int GetPressedBinds()
                {
                    int bindsPressed = 0;

                    foreach (Bind bind in keyBinds)
                        bind.bindHits = 0;

                    foreach (IControl con in usedControls)
                    {
                        if (con.IsPressed)
                        {
                            foreach (Bind bind in controlMap[con.Index])
                                bind.bindHits++;
                        }
                    }

                    // Partial presses on previously pressed binds count as full presses.
                    foreach (Bind bind in keyBinds)
                    {
                        if (bind.IsPressed || bind.beingReleased)
                        {
                            if (bind.bindHits > 0 && bind.bindHits < bind.length)
                            {
                                bind.bindHits = bind.length;
                                bind.beingReleased = true;
                            }
                            else if (bind.beingReleased)
                                bind.beingReleased = false;
                        }

                        if (bind.bindHits == bind.length)
                            bindsPressed++;
                        else
                            bind.bindHits = 0;
                    }

                    return bindsPressed;
                }

                /// <summary>
                /// Resolves conflicts between pressed binds with shared controls.
                /// </summary>
                private void DisambiguatePresses()
                {
                    Bind first, longest;
                    int controlHits;

                    // If more than one pressed bind shares the same control, the longest
                    // binds take precedence. Any binds shorter than the longest will not
                    // be counted as being pressed.
                    foreach (IControl con in usedControls)
                    {
                        first = null;
                        controlHits = 0;
                        longest = GetLongestBindPressForControl(con);

                        foreach (Bind bind in controlMap[con.Index])
                        {
                            if (bind.bindHits > 0 && (bind != longest))
                            {
                                if (controlHits > 0)
                                    bind.bindHits--;
                                else if (controlHits == 0)
                                    first = bind;

                                controlHits++;
                            }
                        }

                        if (controlHits > 0)
                            first.bindHits--;
                    }
                }

                /// <summary>
                /// Determines the length of the longest bind pressed for a given control on the bind map.
                /// </summary>
                private Bind GetLongestBindPressForControl(IControl con)
                {
                    Bind longest = null;

                    foreach (Bind bind in controlMap[con.Index])
                    {
                        if (bind.bindHits > 0 && (longest == null || bind.length > longest.length))
                            longest = bind;
                    }

                    return longest;
                }

                public bool TryRegisterBind(string bindName, out IBind bind, string[] combo = null, bool silent = false)
                {
                    string[] uniqueControls = combo?.GetUnique();
                    IControl[] newCombo = null;
                    bind = null;

                    if (combo == null || TryGetCombo(uniqueControls, out newCombo))
                        return TryRegisterBind(bindName, newCombo, out bind, silent);
                    else if (!silent)
                        ModBase.SendChatMessage($"Invalid bind for {Name}.{bindName}. One or more control names were not recognised.");

                    return false;
                }

                public bool TryRegisterBind(string bindName, IControl[] combo, out IBind newBind, bool silent = false)
                {
                    newBind = null;
                    bindName = bindName.ToLower();

                    if (!DoesBindExist(bindName))
                    {
                        Bind bind = new Bind(bindName, keyBinds.Count, this);
                        newBind = bind;
                        keyBinds.Add(bind);

                        if (combo != null)
                            return bind.TrySetCombo(combo, silent);
                        else
                            return true;
                    }
                    else if (!silent)
                        ModBase.SendChatMessage($"Bind {Name}.{bindName} already exists.");

                    return false;
                }

                private BindMembers? TryRegisterBind(string name, string[] combo, bool silent)
                {
                    IBind bind;

                    if (TryRegisterBind(name, out bind, combo, silent))
                        return bind.GetApiData();
                    else
                        return null;
                }

                private BindMembers? TryRegisterBind(string name, int[] combo, bool silent)
                {
                    IBind bind;
                    IControl[] controls = combo != null ? GetCombo(combo) : null;

                    if (TryRegisterBind(name, controls, out bind, silent))
                        return bind.GetApiData();
                    else
                        return null;
                }

                /// <summary>
                /// Replaces current bind combos with combos based on the given <see cref="BindDefinition"/>[].
                /// </summary>
                public bool TryLoadBindData(IList<BindDefinition> bindData)
                {
                    List<IControl> oldUsedControls;
                    List<List<IBind>> oldBindMap;
                    bool bindError = false;

                    if (bindData != null && bindData.Count > 0)
                    {
                        oldUsedControls = usedControls;
                        oldBindMap = bindMap;

                        UnregisterControls();
                        usedControls = new List<IControl>(bindData.Count);
                        bindMap = new List<List<IBind>>(bindData.Count);

                        foreach (BindDefinition bind in bindData)
                            if (!GetBind(bind.name).TrySetCombo(bind.controlNames, false))
                            {
                                bindError = true;
                                break;
                            }

                        if (bindError)
                        {
                            ModBase.SendChatMessage("One or more keybinds in the given configuration were invalid or conflict with oneanother.");
                            UnregisterControls();

                            usedControls = oldUsedControls;
                            bindMap = oldBindMap;
                            ReregisterControls();

                            return false;
                        }
                        else
                            return true;
                    }
                    else
                    {
                        ModBase.SendChatMessage("Bind data cannot be null or empty.");
                        return false;
                    }
                }

                private BindMembers[] TryLoadApiBindData(IList<BindDefinitionData> data)
                {
                    BindDefinition[] definitions = new BindDefinition[data.Count];

                    for (int n = 0; n < data.Count; n++)
                        definitions[n] = data[n];

                    if (TryLoadBindData(definitions))
                    {
                        BindMembers[] binds = new BindMembers[keyBinds.Count];

                        for (int n = 0; n < keyBinds.Count; n++)
                            binds[n] = keyBinds[n].GetApiData();

                        return binds;
                    }
                    else
                        return null;
                }

                public void RegisterBinds(IList<string> bindNames)
                {
                    IBind newBind;

                    foreach (string name in bindNames)
                        TryRegisterBind(name, out newBind, silent: true);
                }

                public void RegisterBinds(IList<BindDefinition> bindData)
                {
                    IBind newBind;

                    foreach (BindDefinition bind in bindData)
                        TryRegisterBind(bind.name, out newBind, bind.controlNames, true);
                }

                private void ReregisterControls()
                {
                    for (int n = 0; n < usedControls.Count; n++)
                        controlMap[usedControls[n].Index] = bindMap[n];
                }

                private void UnregisterControls()
                {
                    foreach (IControl con in usedControls)
                        controlMap[con.Index] = new List<IBind>();
                }

                /// <summary>
                /// Unregisters a given bind from its current key combination and registers it to a
                /// new one.
                /// </summary>
                private void RegisterBindToCombo(Bind bind, IControl[] newCombo)
                {
                    UnregisterBindFromCombo(bind);

                    foreach (IControl con in newCombo)
                    {
                        List<IBind> registeredBinds = controlMap[con.Index];

                        if (registeredBinds.Count == 0)
                        {
                            usedControls.Add(con);
                            bindMap.Add(registeredBinds);
                        }

                        registeredBinds.Add(bind);

                        if (con.Analog)
                            bind.Analog = true;
                    }

                    bind.length = newCombo.Length;
                }

                /// <summary>
                /// Unregisters a bind from its key combo if it has one.
                /// </summary>
                private void UnregisterBindFromCombo(Bind bind)
                {
                    for (int n = 0; n < usedControls.Count; n++)
                    {
                        List<IBind> registeredBinds = controlMap[usedControls[n].Index];
                        registeredBinds.Remove(bind);

                        if (registeredBinds.Count == 0)
                        {
                            bindMap.Remove(registeredBinds);
                            usedControls.Remove(usedControls[n]);
                        }
                    }

                    bind.Analog = false;
                    bind.length = 0;
                }

                /// <summary>
                /// Retrieves key bind using its name.
                /// </summary>
                public IBind GetBind(string name)
                {
                    name = name.ToLower();

                    foreach (Bind bind in keyBinds)
                        if (bind.Name.ToLower() == name)
                            return bind;

                    ModBase.SendChatMessage($"{name} is not a valid bind name.");
                    return null;
                }

                /// <summary>
                /// Retrieves the set of key binds as an array of KeyBindData
                /// </summary>
                public BindDefinition[] GetBindDefinitions()
                {
                    BindDefinition[] bindData = new BindDefinition[keyBinds.Count];
                    string[][] combos = new string[keyBinds.Count][];

                    for (int x = 0; x < keyBinds.Count; x++)
                    {
                        IList<IControl> combo = keyBinds[x].GetCombo();
                        combos[x] = new string[combo.Count];

                        for (int y = 0; y < combo.Count; y++)
                            combos[x][y] = combo[y].Name;
                    }

                    for (int n = 0; n < keyBinds.Count; n++)
                        bindData[n] = new BindDefinition(keyBinds[n].Name, combos[n]);

                    return bindData;
                }

                /// <summary>
                /// Retrieves the set of key binds as an array of KeyBindData
                /// </summary>
                private BindDefinitionData[] GetBindData()
                {
                    BindDefinitionData[] bindData = new BindDefinitionData[keyBinds.Count];
                    string[][] combos = new string[keyBinds.Count][];

                    for (int x = 0; x < keyBinds.Count; x++)
                    {
                        IList<IControl> combo = keyBinds[x].GetCombo();
                        combos[x] = new string[combo.Count];

                        for (int y = 0; y < combo.Count; y++)
                            combos[x][y] = combo[y].Name;
                    }

                    for (int n = 0; n < keyBinds.Count; n++)
                        bindData[n] = new BindDefinitionData(keyBinds[n].Name, combos[n]);

                    return bindData;
                }

                public BindGroupMembers GetApiData()
                {
                    BindMembers[] bindData = new BindMembers[keyBinds.Count];

                    for (int n = 0; n < keyBinds.Count; n++)
                        bindData[n] = keyBinds[n].GetApiData();

                    BindGroupMembers apiData = new BindGroupMembers()
                    {
                        Item1 = Name,
                        Item2 = bindData,
                        Item3 = DoesComboConflict,
                        Item4 = TryRegisterBind,
                        Item5 = TryLoadApiBindData,
                        Item6 = new MyTuple<Func<string, string[], bool, BindMembers?>, Func<BindDefinitionData[]>, Action, Action>()
                        {
                            Item1 = TryRegisterBind,
                            Item2 = GetBindData,
                            Item3 = HandleInput,
                            Item4 = ClearSubscribers
                        }
                    };

                    return apiData;
                }

                public bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null) =>
                    DoesComboConflict(BindManager.GetComboIndices(newCombo), (exception != null) ? exception.Index : -1);

                /// <summary>
                /// Determines if given combo is equivalent to any existing binds.
                /// </summary>
                private bool DoesComboConflict(IList<int> newCombo, int exception = -1)
                {
                    int matchCount;

                    for (int n = 0; n < keyBinds.Count; n++)
                        if (keyBinds[n].Index != exception && keyBinds[n].length == newCombo.Count)
                        {
                            matchCount = 0;

                            foreach (int con in newCombo)
                                if (BindUsesControl(keyBinds[n], BindManager.Controls[con]))
                                    matchCount++;
                                else
                                    break;

                            if (matchCount == newCombo.Count)
                                return true;
                        }

                    return false;
                }

                /// <summary>
                /// Returns true if a keybind with the given name exists.
                /// </summary>
                public bool DoesBindExist(string name)
                {
                    name = name.ToLower();

                    foreach (Bind bind in keyBinds)
                        if (bind.Name.ToLower() == name)
                            return true;

                    return false;
                }

                /// <summary>
                /// Determines whether or not a bind with a given index uses a given control.
                /// </summary>
                private bool BindUsesControl(Bind bind, IControl con) =>
                    controlMap[con.Index].Contains(bind);
            }
        }
    }
}