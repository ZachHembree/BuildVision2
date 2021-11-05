using RichHudFramework.Client;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using VRage.Input;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using Client;
    using UI;

    namespace UI.Client
    {
        using BindClientMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember
            MyTuple<Func<int, object, int, object>, Func<int>>, // GetOrSetGroupMember, GetGroupCount
            MyTuple<Func<Vector2I, object, int, object>, Func<int, int>>, // GetOrSetBindMember, GetBindCount
            Func<Vector2I, int, bool>, // IsBindPressed
            MyTuple<Func<int, int, object>, Func<int>>, // GetControlMember, GetControlCount
            Action // Unload
        >;

        /// <summary>
        /// Manages custom keybinds; singleton
        /// </summary>
        public sealed partial class BindManager : RichHudClient.ApiModule<BindClientMembers>
        {
            /// <summary>
            /// Read-only collection of bind groups registered
            /// </summary>
            public static IReadOnlyList<IBindGroup> Groups => Instance.groups;

            /// <summary>
            /// Read-only collection of all available controls for use with key binds
            /// </summary
            public static IReadOnlyList<IControl> Controls => Instance.controls;

            /// <summary>
            /// Specifies blacklist mode for SE controls
            /// </summary>
            public static SeBlacklistModes BlacklistMode
            {
                get 
                { 
                    if (_instance == null) Init(); 
                        return (SeBlacklistModes)_instance.GetOrSetMemberFunc(null, (int)BindClientAccessors.RequestBlacklistMode); 
                }
                set
                {
                    if (_instance == null)
                        Init();

                    _instance.GetOrSetMemberFunc(value, (int)BindClientAccessors.RequestBlacklistMode);
                }
            }

            private static BindManager Instance
            {
                get { Init(); return _instance; }
            }
            private static BindManager _instance;

            // Group list
            private readonly Func<int, object, int, object> GetOrSetGroupMemberFunc;
            private readonly Func<int> GetGroupCountFunc;

            // Bind lists
            private readonly Func<Vector2I, object, int, object> GetOrSetBindMemberFunc;
            private readonly Func<Vector2I, int, bool> IsBindPressedFunc;
            private readonly Func<int, int> GetBindCountFunc;

            // Control list
            private readonly Func<int, int, object> GetControlMember;
            private readonly Func<int> GetControlCountFunc;

            private readonly ApiMemberAccessor GetOrSetMemberFunc;
            private readonly Action UnloadAction;

            private readonly ReadOnlyApiCollection<IBindGroup> groups;
            private readonly ReadOnlyApiCollection<IControl> controls;

            private BindManager() : base(ApiModuleTypes.BindManager, false, true)
            {
                var clientData = GetApiData();

                GetOrSetMemberFunc = clientData.Item1;
                UnloadAction = clientData.Item6;

                // Group list
                GetOrSetGroupMemberFunc = clientData.Item2.Item1;
                GetGroupCountFunc = clientData.Item2.Item2;

                // Bind lists
                IsBindPressedFunc = clientData.Item4;
                GetOrSetBindMemberFunc = clientData.Item3.Item1;
                GetBindCountFunc = clientData.Item3.Item2;

                // Control list
                GetControlMember = clientData.Item5.Item1;
                GetControlCountFunc = clientData.Item5.Item2;

                groups = new ReadOnlyApiCollection<IBindGroup>(x => new BindGroup(x), GetGroupCountFunc);
                controls = new ReadOnlyApiCollection<IControl>(x => new Control(x), GetControlCountFunc);
            }

            public static void Init()
            {
                if (_instance == null)
                {
                    _instance = new BindManager();
                }
            }

            public override void Close()
            {
                UnloadAction();
                _instance = null;
            }

            /// <summary>
            /// Returns the bind group with the given name and/or creates one with the name given
            /// if one doesn't exist.
            /// </summary>
            public static IBindGroup GetOrCreateGroup(string name)
            {
                var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetOrCreateGroup);
                return index != -1 ? Groups[index] : null;
            }

            /// <summary>
            /// Returns the bind group with the name igven.
            /// </summary>
            public static IBindGroup GetBindGroup(string name)
            {
                var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetBindGroup);
                return index != -1 ? Groups[index] : null;
            }

            /// <summary>
            /// Returns the control associated with the given name.
            /// </summary>
            public static IControl GetControl(string name)
            {
                var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetControlByName);
                return index != -1 ? Controls[index] : null;
            }

            /// <summary>
            /// Generates a list of controls from a list of control names.
            /// </summary>
            public static IControl[] GetCombo(IList<string> names)
            {
                IControl[] combo = new IControl[names.Count];

                for (int n = 0; n < names.Count; n++)
                    combo[n] = GetControl(names[n]);

                return combo;
            }

            /// <summary>
            /// Generates a combo array using the corresponding control indices.
            /// </summary>
            public static IControl[] GetCombo(IList<ControlData> indices)
            {
                IControl[] combo = new IControl[indices.Count];

                for (int n = 0; n < indices.Count; n++)
                    combo[n] = Controls[indices[n].index];

                return combo;
            }

            /// <summary>
            /// Generates a combo array using the corresponding control indices.
            /// </summary>
            public static IControl[] GetCombo(IList<int> indices)
            {
                IControl[] combo = new IControl[indices.Count];

                for (int n = 0; n < indices.Count; n++)
                    combo[n] = Controls[indices[n]];

                return combo;
            }

            /// <summary>
            /// Generates a list of control indices using a list of control names.
            /// </summary>
            public static int[] GetComboIndices(IList<string> controlNames) =>
                Instance.GetOrSetMemberFunc(controlNames, (int)BindClientAccessors.GetComboIndices) as int[];

            /// <summary>
            /// Returns the control associated with the given <see cref="MyKeys"/> enum.
            /// </summary>
            public static IControl GetControl(MyKeys seKey) =>
                Controls[(int)seKey];

            /// <summary>
            /// Returns the control associated with the given custom <see cref="RichHudControls"/> enum.
            /// </summary>
            public static IControl GetControl(RichHudControls rhdKey) =>
                Controls[(int)rhdKey];

            /// <summary>
            /// Generates a list of control indices from a list of controls.
            /// </summary>
            public static int[] GetComboIndices(IList<IControl> controls)
            {
                int[] indices = new int[controls.Count];

                for (int n = 0; n < controls.Count; n++)
                    indices[n] = controls[n].Index;

                return indices;
            }

            /// <summary>
            /// Generates a list of control indices from a list of controls.
            /// </summary>
            public static int[] GetComboIndices(IList<ControlData> controls)
            {
                int[] indices = new int[controls.Count];

                for (int n = 0; n < controls.Count; n++)
                    indices[n] = controls[n].index;

                return indices;
            }
        }
    }
}