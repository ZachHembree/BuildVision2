using System;
using System.Collections.Generic;
using VRage;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using BindMembers = VRage.MyTuple<
    string, // Name
    System.Func<bool>, // Analog
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    VRage.MyTuple<
        System.Func<bool>, // IsReleased
        VRage.MyTuple<System.Action<System.Action>, System.Action<System.Action>>, // OnNewPress
        VRage.MyTuple<System.Action<System.Action>, System.Action<System.Action>>, // OnPressAndHold
        VRage.MyTuple<System.Action<System.Action>, System.Action<System.Action>>, // OnRelease
        System.Func<System.Collections.Generic.List<int>>, // GetCombo
        VRage.MyTuple<
            System.Func<System.Collections.Generic.IList<int>, bool, bool>, // SetCombo
            System.Action, // ClearCombo
            System.Action, // ClearSubscribers
            int // Index
        >
    >
>;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;
using EventTuple = VRage.MyTuple<System.Action<System.Action>, System.Action<System.Action>>;

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

    namespace UI
    {
        using BismiteClient;
        using BindClientMembers = MyTuple<
            MyTuple<Func<int, ControlMembers>, Func<int>>, // Control List
            Func<IList<string>, int[]>, // GetComboIndices
            Func<string, ControlMembers>, // GetControlByName
            Func<string, BindGroupMembers>, // GetOrCreateGroup
            Func<BindGroupMembers[]>, // GetGroupData
            Action // Unload
        >;

        public sealed class BindManager : BismiteClient.ApiComponentBase
        {
            public static IReadOnlyCollection<IBindGroup> Groups => Instance.groups;
            public static IReadOnlyCollection<IControl> Controls => Instance.controls;

            private static BindManager Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static BindManager instance;

            private readonly IReadOnlyCollection<IBindGroup> groups;
            private readonly IReadOnlyCollection<IControl> controls;

            private readonly List<IBindGroup> bindGroups;
            private readonly Func<IList<string>, int[]> GetComboIndicesFunc;
            private readonly Func<string, ControlMembers> GetControlFunc;
            private readonly Func<string, BindGroupMembers> GetOrCreateGroupFunc;
            private readonly Func<BindGroupMembers[]> GetGroupData;
            private readonly Action UnloadAction;

            private BindManager() : base(ApiComponentTypes.BindManager, false, true)
            {
                var clientData = (BindClientMembers)GetApiData();

                Func<int, ControlMembers> conData = clientData.Item1.Item1;
                Func<int> ConCount = clientData.Item1.Item2;

                Func<int, IControl> ControlGetter = x => new Control(conData(x));
                controls = new ReadOnlyCollectionData<IControl>(ControlGetter, ConCount);

                GetComboIndicesFunc = clientData.Item2;
                GetControlFunc = clientData.Item3;
                GetOrCreateGroupFunc = clientData.Item4;
                GetGroupData = clientData.Item5;
                UnloadAction = clientData.Item6;

                bindGroups = new List<IBindGroup>();
                this.groups = new ReadOnlyCollection<IBindGroup>(bindGroups);

                BindGroupMembers[] groups = GetGroupData();

                foreach (BindGroupMembers group in groups)
                    AddGroupData(group);
            }

            private static void Init()
            {
                if (instance == null)
                {
                    instance = new BindManager();
                }
            }

            public override void Close()
            {
                UnloadAction();
                instance = null;
            }

            public static IBindGroup GetOrCreateGroup(string name)
            {
                name = name.ToLower();
                IBindGroup group = Instance.bindGroups.Find(x => (x.Name == name));

                if (group == null)
                    group = Instance.AddGroupData(Instance.GetOrCreateGroupFunc(name));

                return group;
            }

            private IBindGroup AddGroupData(BindGroupMembers groupData)
            {
                IBindGroup group = new BindGroup(groupData);
                bindGroups.Add(group);

                return group;
            }

            public static IControl GetControl(string name) =>
                new Control(Instance.GetControlFunc(name));

            public static IBindGroup GetBindGroup(string name)
            {
                name = name.ToLower();
                return Instance.bindGroups.Find(x => (x.Name == name));
            }

            public static IControl[] GetCombo(IList<string> names)
            {
                IControl[] combo = new IControl[names.Count];

                for (int n = 0; n < names.Count; n++)
                    combo[n] = GetControl(names[n]);

                return combo;
            }

            public static IControl[] GetCombo(IList<int> indices)
            {
                IControl[] controls = new IControl[indices.Count];

                for (int n = 0; n < indices.Count; n++)
                    controls[n] = Controls[indices[n]];

                return controls;
            }

            public static int[] GetComboIndices(IList<string> controlNames) =>
                Instance.GetComboIndicesFunc(controlNames);

            public static int[] GetComboIndices(IList<IControl> controls)
            {
                int[] indices = new int[controls.Count];

                for (int n = 0; n < controls.Count; n++)
                    indices[n] = controls[n].Index;

                return indices;
            }

            private class Control : IControl
            {
                public string Name { get; }
                public int Index { get; }
                public bool IsPressed => IsPressedFunc();
                public bool Analog { get; }

                private readonly Func<bool> IsPressedFunc;

                public Control(ControlMembers data)
                {
                    Name = data.Item1;
                    Index = data.Item2;
                    IsPressedFunc = data.Item3;
                    Analog = data.Item4;
                }

                public ControlMembers GetApiData()
                {
                    return new ControlMembers()
                    {
                        Item1 = Name,
                        Item2 = Index,
                        Item3 = IsPressedFunc,
                        Item4 = Analog
                    };
                }
            }

            private class BindGroup : IBindGroup
            {
                public string Name { get; }
                public IBind this[int index] => binds[index];
                public int Count => binds.Count;

                private readonly List<IBind> binds;
                private readonly Func<IList<int>, int, bool> DoesComboConflictFunc;
                private readonly Func<string, int[], bool, BindMembers?> TryRegisterBindFunc;
                private readonly Func<IList<BindDefinitionData>, BindMembers[]> TryLoadBindDataFunc;
                private readonly Func<string, string[], bool, BindMembers?> TryRegisterBindFunc2;
                private readonly Func<BindDefinitionData[]> GetBindDataFunc;
                private readonly Action HandleInputAction;
                private readonly Action ClearSubscribersAction;

                public BindGroup(BindGroupMembers groupData)
                {
                    binds = new List<IBind>();
                    AddBinds(groupData.Item2);

                    Name = groupData.Item1;
                    DoesComboConflictFunc = groupData.Item3;
                    TryRegisterBindFunc = groupData.Item4;
                    TryLoadBindDataFunc = groupData.Item5;

                    var data2 = groupData.Item6;
                    TryRegisterBindFunc2 = data2.Item1;
                    GetBindDataFunc = data2.Item2;
                    HandleInputAction = data2.Item3;
                    ClearSubscribersAction = data2.Item4;
                }

                private void AddBinds(BindMembers[] data)
                {
                    for (int n = 0; n < data.Length; n++)
                        binds.Add(new Bind(data[n]));
                }

                public void HandleInput() =>
                    HandleInputAction();

                public IBind GetBind(string name)
                {
                    name = name.ToLower();
                    return binds.Find(x => x.Name == name);
                }

                public bool DoesBindExist(string name) =>
                    GetBind(name) != null;

                public bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null)
                {
                    int[] indices = new int[newCombo.Count];

                    for (int n = 0; n < newCombo.Count; n++)
                        indices[n] = newCombo[n].Index;

                    return DoesComboConflictFunc(indices, exception.Index);
                }

                public bool TryLoadBindData(IList<BindDefinition> bindData)
                {
                    BindMembers[] newBinds = TryLoadBindDataFunc(GetBindDefinitionData(bindData));

                    if (newBinds != null)
                    {
                        binds.Clear();
                        AddBinds(newBinds);
                        return true;
                    }
                    else
                        return false;
                }

                public void RegisterBinds(IList<BindDefinition> bindData)
                {
                    IBind newBind;

                    for (int n = 0; n < bindData.Count; n++)
                    {
                        TryRegisterBind(bindData[n].name, out newBind, bindData[n].controlNames);
                    }
                }

                public void RegisterBinds(IList<string> bindNames)
                {
                    IBind newBind;

                    for (int n = 0; n < bindNames.Count; n++)
                    {
                        TryRegisterBind(bindNames[n], out newBind);
                    }
                }

                public bool TryRegisterBind(string bindName, out IBind newBind, string[] combo = null, bool silent = false)
                {
                    BindMembers? bindData = TryRegisterBindFunc2(bindName, combo, silent);

                    return TryRegisterBind(bindData, out newBind);
                }

                public bool TryRegisterBind(string bindName, IControl[] combo, out IBind newBind, bool silent = false)
                {
                    BindMembers? bindData = TryRegisterBindFunc(bindName, BindManager.GetComboIndices(combo), silent);

                    return TryRegisterBind(bindData, out newBind);
                }

                private bool TryRegisterBind(BindMembers? bindData, out IBind newBind)
                {
                    if (bindData != null)
                    {
                        newBind = new Bind(bindData.Value);
                        binds.Add(newBind);
                        return true;
                    }
                    else
                    {
                        newBind = null;
                        return false;
                    }
                }

                public BindDefinition[] GetBindDefinitions()
                {
                    BindDefinitionData[] data = GetBindDataFunc();
                    BindDefinition[] definitions = new BindDefinition[data.Length];

                    for (int n = 0; n < data.Length; n++)
                        definitions[n] = data[n];

                    return definitions;
                }

                public void ClearSubscribers() =>
                    ClearSubscribersAction();

                private BindMembers[] GetBindMembers()
                {
                    BindMembers[] bindData = new BindMembers[binds.Count];

                    for (int n = 0; n < binds.Count; n++)
                        bindData[n] = binds[n].GetApiData();

                    return bindData;
                }

                public BindGroupMembers GetApiData()
                {
                    return new BindGroupMembers()
                    {
                        Item1 = Name,
                        Item2 = GetBindMembers(),
                        Item3 = DoesComboConflictFunc,
                        Item4 = TryRegisterBindFunc,
                        Item5 = TryLoadBindDataFunc,
                        Item6 = new MyTuple<Func<string, string[], bool, BindMembers?>, Func<BindDefinitionData[]>, Action, Action>()
                        {
                            Item1 = TryRegisterBindFunc2,
                            Item2 = GetBindDataFunc,
                            Item3 = HandleInputAction,
                            Item4 = ClearSubscribersAction
                        }
                    };
                }

                private static BindDefinitionData[] GetBindDefinitionData(IList<BindDefinition> binds)
                {
                    BindDefinitionData[] data = new BindDefinitionData[binds.Count];

                    for (int n = 0; n < binds.Count; n++)
                        data[n] = binds[n];

                    return data;
                }

                private class Bind : IBind
                {
                    public string Name { get; }
                    public int Index { get; }
                    public bool Analog => IsAnalog();
                    public bool IsPressed => IsPressedFunc();
                    public bool IsPressedAndHeld => IsPressedAndHeldFunc();
                    public bool IsNewPressed => IsNewPressedFunc();
                    public bool IsReleased => IsReleasedFunc();

                    public event Action OnNewPress { add { OnNewPressAdd(value); } remove { OnNewPressRemove(value); } }
                    public event Action OnPressAndHold { add { OnPressAndHoldAdd(value); } remove { OnPressAndHoldRemove(value); } }
                    public event Action OnRelease { add { OnReleaseAdd(value); } remove { OnReleaseRemove(value); } }

                    private readonly Func<bool> IsAnalog, IsPressedFunc, IsPressedAndHeldFunc, IsNewPressedFunc, IsReleasedFunc;
                    private readonly Action<Action>
                        OnNewPressAdd, OnPressAndHoldAdd, OnReleaseAdd,
                        OnNewPressRemove, OnPressAndHoldRemove, OnReleaseRemove;

                    private readonly Func<List<int>> GetComboFunc;
                    private readonly Func<IList<int>, bool, bool> SetComboFunc;
                    private readonly Action ClearComboAction;
                    private readonly Action ClearSubscribersAction;

                    public Bind(BindMembers data)
                    {
                        Name = data.Item1;
                        IsAnalog = data.Item2;
                        IsPressedFunc = data.Item3;
                        IsNewPressedFunc = data.Item4;
                        IsPressedAndHeldFunc = data.Item5;

                        var data2 = data.Item6;
                        IsReleasedFunc = data2.Item1;

                        OnNewPressAdd = data2.Item2.Item1;
                        OnNewPressRemove = data2.Item2.Item2;

                        OnPressAndHoldAdd = data2.Item3.Item1;
                        OnPressAndHoldRemove = data2.Item3.Item2;

                        OnReleaseAdd = data2.Item4.Item1;
                        OnReleaseRemove = data2.Item4.Item2;
                        GetComboFunc = data2.Item5;

                        var data3 = data2.Item6;
                        SetComboFunc = data3.Item1;
                        ClearComboAction = data3.Item2;

                        ClearSubscribersAction = data3.Item3;
                        Index = data3.Item4;
                    }

                    public IList<IControl> GetCombo() =>
                        BindManager.GetCombo(GetComboFunc());

                    public bool TrySetCombo(IList<string> combo, bool silent = false) =>
                        SetComboFunc(GetComboIndices(combo), silent);

                    public bool TrySetCombo(IControl[] combo, bool silent = false) =>
                        SetComboFunc(GetComboIndices(combo), silent);

                    public void ClearCombo() =>
                        ClearComboAction();

                    public void ClearSubscribers() =>
                        ClearSubscribersAction();

                    public BindMembers GetApiData()
                    {
                        return new BindMembers()
                        {
                            Item1 = Name,
                            Item2 = IsAnalog,
                            Item3 = IsPressedFunc,
                            Item4 = IsPressedAndHeldFunc,
                            Item5 = IsNewPressedFunc,
                            Item6 = new MyTuple<Func<bool>, EventTuple, EventTuple, EventTuple, Func<List<int>>, MyTuple<Func<IList<int>, bool, bool>, Action, Action, int>>()
                            {
                                Item1 = IsReleasedFunc,
                                Item2 = new EventTuple(OnNewPressAdd, OnNewPressRemove),
                                Item3 = new EventTuple(OnPressAndHoldAdd, OnPressAndHoldRemove),
                                Item4 = new EventTuple(OnReleaseAdd, OnReleaseRemove),
                                Item5 = GetComboFunc,
                                Item6 = new MyTuple<Func<IList<int>, bool, bool>, Action, Action, int>()
                                {
                                    Item1 = SetComboFunc,
                                    Item2 = ClearComboAction,
                                    Item3 = ClearSubscribersAction,
                                    Item4 = Index
                                }
                            }
                        };
                    }
                }
            }
        }
    }
}