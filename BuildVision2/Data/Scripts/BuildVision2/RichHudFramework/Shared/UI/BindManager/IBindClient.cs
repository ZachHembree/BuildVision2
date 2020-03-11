using System;
using System.Collections.Generic;
using VRage;
using BindMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    System.Func<bool> // IsReleased
>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using ControlMembers = MyTuple<string, string, int, Func<bool>, bool, ApiMemberAccessor>;
    using BindGroupMembers = MyTuple<
        string, // Name                
        BindMembers[], // Binds
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMember
    >;

    namespace UI
    {
        using BindClientMembers = MyTuple<
            MyTuple<Func<int, ControlMembers?>, Func<int>>, // Control List
            Action, // HandleInput
            ApiMemberAccessor, // GetOrSetMember
            Action // Unload
        >;

        public enum BindClientAccessors : int
        {
            /// <summary>
            /// In: IList{string}, Out: int[]
            /// </summary>
            GetComboIndices = 1,

            /// <summary>
            /// In: string, Out: Controlmembers
            /// </summary>
            GetControlByName = 2,

            /// <summary>
            /// In: string, Out: BindGroupMembers
            /// </summary>
            GetOrCreateGroup = 3,

            /// <summary>
            /// Out: BindGroupMembers[]
            /// </summary>
            GetGroupData = 4,

            /// <summary>
            /// void
            /// </summary>
            Unload = 5,
        }

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
    }
}