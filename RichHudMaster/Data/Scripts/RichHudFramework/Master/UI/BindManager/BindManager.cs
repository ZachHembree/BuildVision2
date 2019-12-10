using DarkHelmet.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Input;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using BindMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    System.Func<bool> // IsReleased
>;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;

namespace DarkHelmet
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
        using BindClientMembers = MyTuple<
            MyTuple<Func<int, ControlMembers>, Func<int>>, // Control List
            Func<IList<string>, int[]>, // GetComboIndices
            Func<string, ControlMembers>, // GetControlByName
            Func<string, BindGroupMembers>, // GetOrCreateGroup
            Func<BindGroupMembers[]>, // GetGroupData 
            MyTuple<
                Action, // HandleInput
                Action // Unload
            >
        >;

        public interface IBindClient
        {
            ReadOnlyCollection<IControl> Controls { get; }
            ReadOnlyCollection<IBindGroup> Groups { get; }

            BindClientMembers GetApiData();
            IBindGroup GetBindGroup(string name);
            IBindGroup[] GetBindGroups();
            IControl[] GetCombo(IList<int> indices);
            int[] GetComboIndices(IList<IControl> controls);
            IControl GetControl(string name);
            BindGroupMembers[] GetGroupData();
            IBindGroup GetOrCreateGroup(string name);
            void HandleInput();
            void Unload();
        }

        /// <summary>
        /// Manages custom keybinds; singleton
        /// </summary>
        public sealed class BindManager : ModBase.ComponentBase
        {
            public static IReadOnlyCollection<IBindGroup> Groups => Instance.mainClient.Groups;
            public static ReadOnlyCollection<IControl> Controls { get; }

            private static BindManager Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static BindManager instance;

            private static readonly Control[] controls;
            private static readonly Dictionary<string, IControl> controlDict;
            private static readonly List<MyKeys> controlBlacklist;

            private readonly BindClient mainClient;

            static BindManager()
            {
                controlBlacklist = new List<MyKeys>()
                {
                    MyKeys.None,
                    MyKeys.LeftAlt,
                    MyKeys.RightAlt,
                    MyKeys.LeftShift,
                    MyKeys.RightShift,
                    MyKeys.LeftControl,
                    MyKeys.RightControl,
                    MyKeys.LeftWindows,
                    MyKeys.RightWindows
                };

                controlDict = new Dictionary<string, IControl>(200);
                controls = GenerateControls();
                Controls = new ReadOnlyCollection<IControl>(controls as IControl[]);
            }

            private BindManager() : base(false, true)
            {
                mainClient = new BindClient();
            }

            public static void Init()
            {
                if (instance == null)
                    instance = new BindManager();
            }

            public override void HandleInput()
            {
                mainClient.HandleInput();
            }

            public override void Close()
            {
                Instance = null;
            }

            public static IBindClient GetNewBindClient()
            {
                return new BindClient();
            }

            public static IBindGroup GetOrCreateGroup(string name) =>
                Instance.mainClient.GetOrCreateGroup(name);

            public static IBindGroup GetBindGroup(string name) =>
                Instance.mainClient.GetBindGroup(name);

            public static IControl GetControl(string name) =>
                controlDict[name.ToLower()];

            /// <summary>
            /// Builds dictionary of controls from the set of MyKeys enums and a couple custom controls for the mouse wheel.
            /// </summary>
            private static Control[] GenerateControls()
            {
                List<Control> controlList = new List<Control>(200);

                controlList.Add(new Control("MousewheelUp", controlList.Count,
                    () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() > 0, true));
                controlList.Add(new Control("MousewheelDown", controlList.Count,
                    () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() < 0, true));

                controlDict.Add("mousewheelup", controlList[0]);
                controlDict.Add("mousewheeldown", controlList[1]);

                foreach (MyKeys seKey in Enum.GetValues(typeof(MyKeys)))
                {
                    if (!controlBlacklist.Contains(seKey))
                    {
                        Control con = new Control(seKey, controlList.Count);
                        string name = con.Name.ToLower();

                        if (!controlDict.ContainsKey(name))
                        {
                            controlDict.Add(name, con);
                            controlList.Add(con);
                        }
                    }
                }

                return controlList.ToArray();
            }

            public static IControl[] GetCombo(IList<string> names)
            {
                IControl[] combo = new IControl[names.Count];

                for (int n = 0; n < names.Count; n++)
                    combo[n] = GetControl(names[n]);

                return combo;
            }

            public static int[] GetComboIndices(IList<string> names)
            {
                int[] combo = new int[names.Count];

                for (int n = 0; n < names.Count; n++)
                    combo[n] = GetControl(names[n]).Index;

                return combo;
            }

            public static IControl[] GetCombo(IList<int> indices)
            {
                IControl[] combo = new IControl[indices.Count];

                for (int n = 0; n < indices.Count; n++)
                    combo[n] = Controls[indices[n]];

                return combo; ;
            }

            public static int[] GetComboIndices(IList<IControl> controls)
            {
                int[] indices = new int[controls.Count];

                for (int n = 0; n < controls.Count; n++)
                    indices[n] = controls[n].Index;

                return indices;
            }

            /// <summary>
            /// Tries to get a key combo using a list of control names.
            /// </summary>
            public static bool TryGetCombo(IList<string> controlNames, out IControl[] newCombo)
            {
                IControl con;
                newCombo = new IControl[controlNames.Count];

                for (int n = 0; n < controlNames.Count; n++)
                    if (controlDict.TryGetValue(controlNames[n].ToLower(), out con))
                        newCombo[n] = con;
                    else
                        return false;

                return true;
            }

            /// <summary>
            /// General purpose button wrapper for MyKeys and anything else associated with a name and an IsPressed method.
            /// </summary>
            private class Control : IControl
            {
                public string Name { get; }
                public bool IsPressed { get { return isPressedFunc(); } }
                public bool Analog { get; }
                public int Index { get; }

                private readonly Func<bool> isPressedFunc;

                public Control(MyKeys seKey, int index, bool Analog = false)
                    : this(seKey.ToString(), index, () => MyAPIGateway.Input.IsKeyPress(seKey), Analog)
                { }

                public Control(string name, int index, Func<bool> IsPressed, bool Analog = false)
                {
                    Name = name;
                    Index = index;
                    isPressedFunc = IsPressed;
                    this.Analog = Analog;
                }

                public ControlMembers GetApiData()
                {
                    return new ControlMembers()
                    {
                        Item1 = Name,
                        Item2 = Index,
                        Item3 = isPressedFunc,
                        Item4 = Analog
                    };
                }
            }

            private class BindClient : IBindClient
            {
                public ReadOnlyCollection<IBindGroup> Groups { get; }
                public ReadOnlyCollection<IControl> Controls => BindManager.Controls;

                private readonly List<IBindGroup> bindGroups;

                public BindClient()
                {
                    bindGroups = new List<IBindGroup>();
                    Groups = new ReadOnlyCollection<IBindGroup>(bindGroups);
                }

                public void HandleInput()
                {
                    foreach (IBindGroup group in bindGroups)
                        group.HandleInput();
                }

                public IControl GetControl(string name) =>
                    BindManager.GetControl(name);

                public IBindGroup GetOrCreateGroup(string name)
                {
                    name = name.ToLower();
                    IBindGroup group = GetBindGroup(name);

                    if (group == null)
                    {
                        group = new BindGroup(name);
                        bindGroups.Add(group);
                    }

                    return group;
                }

                /// <summary>
                /// Retrieves a copy of the list of all registered groups.
                /// </summary>
                public IBindGroup[] GetBindGroups() =>
                    bindGroups.ToArray();

                /// <summary>
                /// Retrieves a bind group using its name.
                /// </summary>
                public IBindGroup GetBindGroup(string name)
                {
                    name = name.ToLower();
                    return bindGroups.Find(x => (x.Name == name));
                }

                public BindGroupMembers[] GetGroupData()
                {
                    BindGroupMembers[] groupData = new BindGroupMembers[bindGroups.Count];

                    for (int n = 0; n < groupData.Length; n++)
                        groupData[n] = bindGroups[n].GetApiData();

                    return groupData;
                }

                public IControl[] GetCombo(IList<int> indices) =>
                    BindManager.GetCombo(indices);

                public int[] GetComboIndices(IList<IControl> controls) =>
                    BindManager.GetComboIndices(controls);

                public void Unload()
                {
                    foreach (BindGroup group in bindGroups)
                        group.ClearSubscribers();

                    bindGroups.Clear();
                }

                public BindClientMembers GetApiData()
                {
                    BindClientMembers apiData = new BindClientMembers()
                    {
                        Item1 = new MyTuple<Func<int, ControlMembers>, Func<int>>(x => BindManager.Controls[x].GetApiData(), () => BindManager.Controls.Count),
                        Item2 = BindManager.GetComboIndices,
                        Item3 = x => BindManager.GetControl(x).GetApiData(),
                        Item4 = x => GetOrCreateGroup(x).GetApiData(),
                        Item5 = GetGroupData,
                        Item6 = new MyTuple<Action, Action>()
                        {
                            Item1 = HandleInput,
                            Item2 = Unload
                        }
                    };

                    return apiData;
                }
            }

            /// <summary>
            /// Contains a set of keybinds independent of other groups and determines when/if those binds can be pressed. 
            /// While a group's own binds cannot conflict with oneanother, binds in other groups may.
            /// </summary>
            private class BindGroup : IBindGroup
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

                /// <summary>
                /// Logic and data for individual keybinds
                /// </summary>
                private class Bind : IBind
                {
                    public event Action OnNewPress, OnPressAndHold, OnRelease;

                    public string Name { get; }
                    public int Index { get; }
                    public bool Analog { get; set; }
                    public bool IsPressed { get; private set; }
                    public bool IsNewPressed { get { return IsPressed && (!wasPressed || Analog); } }
                    public bool IsPressedAndHeld { get; private set; }
                    public bool IsReleased { get { return !IsPressed && wasPressed; } }

                    public bool beingReleased;
                    public int length, bindHits;

                    private bool wasPressed;
                    private readonly Utils.Stopwatch stopwatch;
                    private readonly BindGroup group;

                    public Bind(string name, int index, BindGroup group)
                    {
                        Name = name;
                        Index = index;
                        stopwatch = new Utils.Stopwatch();
                        this.group = group;

                        IsPressedAndHeld = false;
                        wasPressed = false;

                        bindHits = 0;
                        Analog = false;
                        beingReleased = false;
                        length = 0;
                    }

                    /// <summary>
                    /// Used to update the key bind with each tick of the Binds.Update function. 
                    /// </summary>
                    public void UpdatePress(bool isPressed)
                    {
                        wasPressed = IsPressed;
                        IsPressed = isPressed;

                        if (IsNewPressed)
                        {
                            OnNewPress?.Invoke();
                            stopwatch.Start();
                        }

                        IsPressedAndHeld = IsNewPressed || (IsPressed && stopwatch.ElapsedTicks > holdTime);

                        if (IsReleased)
                            OnRelease?.Invoke();

                        if (IsPressedAndHeld)
                            OnPressAndHold?.Invoke();
                    }

                    public IList<IControl> GetCombo()
                    {
                        List<IControl> combo = new List<IControl>();

                        foreach (IControl con in group.usedControls)
                        {
                            if (group.BindUsesControl(this, con))
                                combo.Add(con);
                        }

                        return combo;
                    }

                    private List<int> GetComboIndices()
                    {
                        List<int> combo = new List<int>();

                        foreach (IControl con in group.usedControls)
                        {
                            if (group.BindUsesControl(this, con))
                                combo.Add(con.Index);
                        }

                        return combo;
                    }

                    public bool TrySetCombo(IList<string> combo, bool silent = false) =>
                        TrySetCombo(BindManager.GetCombo(combo), silent);

                    /// <summary>
                    /// Tries to update a key bind using the names of the controls to be bound.
                    /// </summary>
                    public bool TrySetCombo(IControl[] combo, bool silent = false)
                    {
                        if (combo.Length <= maxBindLength && combo.Length > 0)
                        {
                            if (!group.DoesComboConflict(combo, this))
                            {
                                group.RegisterBindToCombo(this, combo);
                                return true;
                            }
                            else if (!silent)
                                ModBase.SendChatMessage($"Invalid bind for {group.Name}.{Name}. One or more of the given controls conflict with existing binds.");
                        }
                        else if (!silent)
                        {
                            if (combo.Length > 0)
                                ModBase.SendChatMessage($"Invalid key bind. No more than {maxBindLength} keys in a bind are allowed.");
                            else
                                ModBase.SendChatMessage("Invalid key bind. There must be at least one control in a key bind.");
                        }

                        return false;
                    }

                    private bool TrySetCombo(IList<int> indices, bool silent = false)
                    {
                        IControl[] combo = new IControl[indices.Count];

                        for (int n = 0; n < indices.Count; n++)
                            combo[n] = BindManager.Controls[indices[n]];

                        return TrySetCombo(combo);
                    }

                    public void ClearCombo() =>
                        group.UnregisterBindFromCombo(this);

                    public void ClearSubscribers()
                    {
                        OnNewPress = null;
                        OnPressAndHold = null;
                        OnRelease = null;
                    }

                    private object GetOrSetMember(object data, int memberEnum)
                    {
                        var member = (BindAccesssors)memberEnum;

                        if (member == BindAccesssors.Name)
                        {
                            return Name;
                        }
                        else if (member == BindAccesssors.Analog)
                        {
                            return Analog;
                        }
                        else if (member == BindAccesssors.Index)
                        {
                            return Index;
                        }
                        else if (member == BindAccesssors.OnNewPress)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnNewPress += eventData.Item2;
                            else
                                OnNewPress -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.OnPressAndHold)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnPressAndHold += eventData.Item2;
                            else
                                OnPressAndHold -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.OnRelease)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnRelease += eventData.Item2;
                            else
                                OnRelease -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.GetCombo)
                            return GetComboIndices();
                        else if (member == BindAccesssors.SetCombo)
                        {
                            var comboData = (MyTuple<List<int>, bool>)data;
                            return TrySetCombo(comboData.Item1, comboData.Item2);
                        }
                        else if (member == BindAccesssors.ClearCombo)
                        {
                            ClearCombo();
                        }
                        else if (member == BindAccesssors.ClearSubscribers)
                            ClearSubscribers();

                        return null;
                    }

                    public BindMembers GetApiData()
                    {
                        return new BindMembers()
                        {
                            Item1 = GetOrSetMember,
                            Item2 = () => IsPressed,
                            Item3 = () => IsNewPressed,
                            Item4 = () => IsPressedAndHeld,
                            Item5 = () => IsReleased
                        };
                    }
                }
            }
        }
    }
}