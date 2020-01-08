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

                public void HandleInput() =>
                    HandleInputAction();

                public IBind GetBind(string name)
                {
                    name = name.ToLower();
                    return binds.Find(x => x.Name.ToLower() == name);
                }

                public bool DoesBindExist(string name) =>
                    GetBind(name) != null;

                public bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null)
                {
                    int[] indices = new int[newCombo.Count];

                    for (int n = 0; n < newCombo.Count; n++)
                        indices[n] = newCombo[n].Index;

                    var args = new MyTuple<IList<int>, int>(indices, exception.Index);
                    return (bool)GetOrSetMemberFunc(args, (int)BindGroupAccessors.DoesComboConflict);
                }

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
                    var args = new MyTuple<string, string[], bool>(bindName, combo, silent);
                    var bindData = (BindMembers?)GetOrSetMemberFunc(args, (int)BindGroupAccessors.TryRegisterBind2);

                    return TryRegisterBind(bindData, out newBind);
                }

                public bool TryRegisterBind(string bindName, IControl[] combo, out IBind newBind, bool silent = false)
                {
                    var args = new MyTuple<string, int[], bool>(bindName, GetComboIndices(combo), silent);
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

                public BindDefinition[] GetBindDefinitions()
                {
                    var data = GetOrSetMemberFunc(null, (int)BindGroupAccessors.GetBindData) as BindDefinitionData[];
                    BindDefinition[] definitions = new BindDefinition[data.Length];

                    for (int n = 0; n < data.Length; n++)
                        definitions[n] = data[n];

                    return definitions;
                }

                public void ClearSubscribers() =>
                    GetOrSetMemberFunc(null, (int)BindGroupAccessors.ClearSubscribers);

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
                        Item3 = HandleInputAction,
                        Item4 = GetOrSetMemberFunc,
                    };
                }

                private static BindDefinitionData[] GetBindDefinitionData(IList<BindDefinition> binds)
                {
                    BindDefinitionData[] data = new BindDefinitionData[binds.Count];

                    for (int n = 0; n < binds.Count; n++)
                        data[n] = binds[n];

                    return data;
                }
            }
        }
    }
}