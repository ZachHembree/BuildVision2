using RichHudFramework.Client;
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
        using RichHudFramework.Client;
        using BindClientMembers = MyTuple<
            MyTuple<Func<int, ControlMembers>, Func<int>>, // Control List
            Action, // HandleInput
            ApiMemberAccessor, // GetOrSetMember
            Action // Unload
        >;

        public sealed partial class BindManager : RichHudClient.ApiModule<BindClientMembers>
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
            private readonly ApiMemberAccessor GetOrSetMemberFunc;
            private readonly Action HandleInputAction, UnloadAction;

            private BindManager() : base(ApiModuleTypes.BindManager, false, true)
            {
                var clientData = GetApiData();

                Func<int, ControlMembers> conData = clientData.Item1.Item1;
                Func<int> ConCount = clientData.Item1.Item2;

                Func<int, IControl> ControlGetter = x => new Control(conData(x));
                controls = new ReadOnlyCollectionData<IControl>(ControlGetter, ConCount);

                HandleInputAction = clientData.Item2;
                GetOrSetMemberFunc = clientData.Item3;
                UnloadAction = clientData.Item4;

                bindGroups = new List<IBindGroup>();
                this.groups = new ReadOnlyCollection<IBindGroup>(bindGroups);

                var groups = GetOrSetMemberFunc(null, (int)BindClientAccessors.GetGroupData) as BindGroupMembers[];

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

            public override void HandleInput()
            {
                HandleInputAction();
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
                {
                    var groupData = (BindGroupMembers)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetOrCreateGroup);
                    group = Instance.AddGroupData(groupData);
                }

                return group;
            }

            private IBindGroup AddGroupData(BindGroupMembers groupData)
            {
                IBindGroup group = new BindGroup(groupData);
                bindGroups.Add(group);

                return group;
            }

            public static IControl GetControl(string name)
            {
                var controlData = (ControlMembers)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetControlByName);
                return new Control(controlData);
            }

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
                Instance.GetOrSetMemberFunc(controlNames, (int)BindClientAccessors.GetComboIndices) as int[];

            public static int[] GetComboIndices(IList<IControl> controls)
            {
                int[] indices = new int[controls.Count];

                for (int n = 0; n < controls.Count; n++)
                    indices[n] = controls[n].Index;

                return indices;
            }
        }
    }
}