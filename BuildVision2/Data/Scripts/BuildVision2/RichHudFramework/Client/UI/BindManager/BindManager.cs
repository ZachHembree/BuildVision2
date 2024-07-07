using RichHudFramework.Client;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage;
using VRageMath;
using VRage.Input;
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
            public const int MaxBindLength = 3;

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

                    lastBlacklist = value;
                    _instance.GetOrSetMemberFunc(value, (int)BindClientAccessors.RequestBlacklistMode);
                }
            }

            /// <summary>
            /// MyAPIGateway.Gui.ChatEntryVisible, but actually usable for input polling
            /// </summary>
            public static bool IsChatOpen => (bool)_instance.GetOrSetMemberFunc(null, (int)BindClientAccessors.IsChatOpen);

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
            private readonly List<int> conIDbuf;
            private readonly List<List<int>> aliasIDbuf;

            private static SeBlacklistModes lastBlacklist, tmpBlacklist;

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

                conIDbuf = new List<int>();
                aliasIDbuf = new List<List<int>>();
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
                UnloadAction?.Invoke();
                _instance = null;
            }

            /// <summary>
            /// Sets a temporary control blacklist cleared after every frame. Blacklists set via
            /// property will persist regardless.
            /// </summary>
            public static void RequestTempBlacklist(SeBlacklistModes mode)
            {
                tmpBlacklist |= mode;
            }

            public override void Draw()
            {
                GetOrSetMemberFunc(lastBlacklist | tmpBlacklist, (int)BindClientAccessors.RequestBlacklistMode);
                tmpBlacklist = SeBlacklistModes.None;
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
            public static ControlHandle GetControl(string name)
            {
                var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetControlByName);
                return new ControlHandle(index);
            }

            /// <summary>
            /// Returns control name for the corresponding handle
            /// </summary>
            public static string GetControlName(ControlHandle con)
            {
                return Instance.GetOrSetMemberFunc(con.id, (int)BindClientAccessors.GetControlName) as string;
            }

            /// <summary>
            /// Returns control name for the corresponding int ID
            /// </summary>
            public static string GetControlName(int conID)
            {
                return Instance.GetOrSetMemberFunc(conID, (int)BindClientAccessors.GetControlName) as string;
            }

            /// <summary>
            /// Returns control names for the corresponding int IDs
            /// </summary>
            public static string[] GetControlNames(IReadOnlyList<int> conIDs)
            {
                return Instance.GetOrSetMemberFunc(conIDs, (int)BindClientAccessors.GetControlName) as string[];
            }

            /// <summary>
            /// Returns the control associated with the given <see cref="ControlHandle"/>
            /// </summary>
            public static IControl GetControl(ControlHandle handle) =>
                Controls[handle.id];

            /// <summary>
            /// Generates a list of control indices from a list of <see cref="ControlHandle"/>s.
            /// </summary>
            public static void GetComboIndices(IReadOnlyList<ControlHandle> controls, List<int> combo, bool sanitize = true)
            {
                combo.Clear();

                for (int n = 0; n < controls.Count; n++)
                    combo.Add(controls[n].id);

                if (sanitize)
                    SanitizeCombo(combo);
            }

            private static IReadOnlyList<int> GetComboIndicesTemp(IReadOnlyList<ControlHandle> controls, bool sanitize = true)
            {
                var buf = _instance.conIDbuf;
                GetComboIndices(controls, buf, sanitize);
                return buf;
            }

            private static IReadOnlyList<int> GetSanitizedComboTemp(IEnumerable<int> combo)
            {
                var buf = _instance.conIDbuf;

                if (buf != combo)
                {
                    buf.Clear();
                    buf.AddRange(combo);
                }

                SanitizeCombo(buf);
                return buf;
            }

            /// <summary>
            /// Sorts ControlID buffer and removes duplicates and invalid indices
            /// </summary>
            private static void SanitizeCombo(List<int> combo)
            {
                combo.Sort();

                for (int i = combo.Count - 1; i > 0; i--)
                {
                    if (combo[i] == combo[i - 1] || combo[i] <= 0)
                        combo.RemoveAt(i);
                }

                if (combo.Count > 0 && combo[0] == 0)
                    combo.RemoveAt(0);
            }
        }
    }
}