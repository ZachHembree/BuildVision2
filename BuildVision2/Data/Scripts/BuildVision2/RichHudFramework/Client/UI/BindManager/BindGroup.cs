using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using BindMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    System.Func<bool> // IsReleased
>;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using BindGroupMembers = MyTuple<
        string, // Name                
        BindMembers[], // Binds
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMember
    >;

    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private partial class BindGroup : IBindGroup
            {
                public string Name { get; }
                public IBind this[int index] => binds[index];
                public int Count => binds.Count;
                public object ID => GetOrSetMemberFunc(null, (int)BindGroupAccessors.ID);

                private readonly List<IBind> binds;
                private readonly Action HandleInputAction;
                private readonly ApiMemberAccessor GetOrSetMemberFunc;

                public BindGroup(BindGroupMembers groupData)
                {
                    binds = new List<IBind>();
                    AddBinds(groupData.Item2);

                    Name = groupData.Item1;
                    HandleInputAction = groupData.Item3;
                    GetOrSetMemberFunc = groupData.Item4;
                }

                private void AddBinds(BindMembers[] data)
                {
                    for (int n = 0; n < data.Length; n++)
                        binds.Add(new Bind(data[n]));
                }

                /// <summary>
                /// Updates input state
                /// </summary>
                public void HandleInput() =>
                    HandleInputAction();

                /// <summary>
                /// Returns the bind with the name given, if it exists.
                /// </summary>
                public IBind GetBind(string name)
                {
                    name = name.ToLower();
                    return binds.Find(x => x.Name.ToLower() == name);
                }

                /// <summary>
                /// Returns true if the group contains a bind with the given name.
                /// </summary>
                public bool DoesBindExist(string name) =>
                    GetBind(name) != null;

                /// <summary>
                /// Returns true if the given list of controls conflicts with any existing binds.
                /// </summary>
                public bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null)
                {
                    int[] indices = new int[newCombo.Count];

                    for (int n = 0; n < newCombo.Count; n++)
                        indices[n] = newCombo[n].Index;

                    var args = new MyTuple<IList<int>, int>(indices, exception.Index);
                    return (bool)GetOrSetMemberFunc(args, (int)BindGroupAccessors.DoesComboConflict);
                }

                /// <summary>
                /// Attempts to load bind combinations from bind data. Will not register new binds.
                /// </summary>
                public bool TryLoadBindData(IList<BindDefinition> bindData)
                {
                    var newBinds = GetOrSetMemberFunc(GetBindDefinitionData(bindData), (int)BindGroupAccessors.TryLoadBindData) as BindMembers[];

                    if (newBinds != null)
                    {
                        binds.Clear();
                        AddBinds(newBinds);
                        return true;
                    }
                    else
                        return false;
                }

                /// <summary>
                /// Converts a list of BindDefinitions into an array of BindDefinitionData to allow bind registration via the API
                /// </summary>
                private static BindDefinitionData[] GetBindDefinitionData(IList<BindDefinition> binds)
                {
                    BindDefinitionData[] data = new BindDefinitionData[binds.Count];

                    for (int n = 0; n < binds.Count; n++)
                        data[n] = binds[n];

                    return data;
                }

                /// <summary>
                /// Attempts to register a set of binds with the given names.
                /// </summary>
                public void RegisterBinds(IEnumerable<MyTuple<string, IList<int>>> bindData)
                {
                    foreach (var bind in bindData)
                        AddBind(bind.Item1, bind.Item2);
                }

                /// <summary>
                /// Registers and loads bind combinations from BindDefinitions.
                /// </summary>
                public void RegisterBinds(IList<BindDefinition> bindData)
                {
                    IBind newBind;

                    for (int n = 0; n < bindData.Count; n++)
                    {
                        TryRegisterBind(bindData[n].name, out newBind, bindData[n].controlNames);
                    }
                }

                /// <summary>
                /// Registers a list of binds using the names given.
                /// </summary>
                public void RegisterBinds(IList<string> bindNames)
                {
                    IBind newBind;

                    for (int n = 0; n < bindNames.Count; n++)
                    {
                        TryRegisterBind(bindNames[n], out newBind);
                    }
                }

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IList<string> combo)
                {
                    IBind bind;

                    if (TryRegisterBind(bindName, out bind, combo, true))
                        return bind;
                    else
                        throw new Exception($"Bind {Name}.{bindName} is invalid. Bind names and key combinations must be unique.");
                }

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IList<ControlData> combo = null) =>
                    AddBind(bindName, (combo != null) ? GetComboIndices(combo) : null);

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IList<IControl> combo = null) =>
                    AddBind(bindName, (combo != null) ? GetComboIndices(combo) : null);

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IList<int> combo)
                {
                    IBind bind;

                    if (TryRegisterBind(bindName, combo, out bind, true))
                        return bind;
                    else
                        throw new Exception($"Bind {Name}.{bindName} is invalid. Bind names and key combinations must be unique.");
                }

                /// <summary>
                /// Tries to register a bind using the given name and the given key combo. Shows an error message in chat upon failure.
                /// </summary>
                public bool TryRegisterBind(string bindName, out IBind newBind, IList<string> combo = null, bool silent = false)
                {
                    var args = new MyTuple<string, IList<string>, bool>(bindName, combo, silent);
                    var bindData = (BindMembers?)GetOrSetMemberFunc(args, (int)BindGroupAccessors.TryRegisterBind2);

                    return TryRegisterBind(bindData, out newBind);
                }

                /// <summary>
                /// Tries to register a bind using the given name and the given key combo. Shows an error message in chat upon failure.
                /// </summary>
                public bool TryRegisterBind(string bindName, IList<IControl> combo, out IBind newBind, bool silent = false) =>
                    TryRegisterBind(bindName, GetComboIndices(combo), out newBind, silent);

                /// <summary>
                /// Tries to register a bind using the given name and the given key combo. Shows an error message in chat upon failure.
                /// </summary>
                public bool TryRegisterBind(string bindName, IList<int> combo, out IBind newBind, bool silent = false)
                {
                    var args = new MyTuple<string, IList<int>, bool>(bindName, combo, silent);
                    BindMembers? bindData = (BindMembers?)GetOrSetMemberFunc(args, (int)BindGroupAccessors.TryRegisterBind);

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

                /// <summary>
                /// Retrieves the set of key binds as an array of BindDefinitions.
                /// </summary>
                public BindDefinition[] GetBindDefinitions()
                {
                    var data = GetOrSetMemberFunc(null, (int)BindGroupAccessors.GetBindData) as BindDefinitionData[];
                    BindDefinition[] definitions = new BindDefinition[data.Length];

                    for (int n = 0; n < data.Length; n++)
                        definitions[n] = data[n];

                    return definitions;
                }

                /// <summary>
                /// Clears all event subscribers from binds.
                /// </summary>
                private void ClearSubscribers() =>
                    GetOrSetMemberFunc(null, (int)BindGroupAccessors.ClearSubscribers);

                /// <summary>
                /// Builds a collection Bind API accessors for all cached keybinds.
                /// </summary>
                private BindMembers[] GetBindMembers()
                {
                    BindMembers[] bindData = new BindMembers[binds.Count];

                    for (int n = 0; n < binds.Count; n++)
                        bindData[n] = binds[n].GetApiData();

                    return bindData;
                }

                /// <summary>
                /// Retreives information needed to access the BindGroup via the API.
                /// </summary>
                public BindGroupMembers GetApiData()
                {
                    return new BindGroupMembers()
                    {
                        Item1 = Name,
                        Item2 = GetBindMembers(),
                        Item3 = HandleInputAction,
                        Item4 = GetOrSetMemberFunc,
                    };
                }
            }
        }
    }
}