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

        public sealed partial class BindManager
        {
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
        }
    }
}