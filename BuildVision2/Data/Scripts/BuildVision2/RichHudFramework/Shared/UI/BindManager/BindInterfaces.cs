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
        internal enum BindClientAccessors : int
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

        internal enum BindGroupAccessors : int
        {
            /// <summary>
            /// In: MyTuple{IList{int}, int}, Out: bool
            /// </summary>
            DoesComboConflict = 1,

            /// <summary>
            /// In: MyTuple{string, int[], bool}, Out: BindMembers?
            /// </summary>
            TryRegisterBind = 2,

            /// <summary>
            /// In: IList{BindDefinitionData}, Out: BindMembers[]
            /// </summary>
            TryLoadBindData = 3,

            /// <summary>
            /// In: MyTuple{string, string[], bool}, Out: BindMembers?
            /// </summary>
            TryRegisterBind2 = 4,

            /// <summary>
            /// Out: BindDefinitionData[]
            /// </summary>
            GetBindData = 5,

            /// <summary>
            /// Void
            /// </summary>
            ClearSubscribers = 6,

            /// <summary>
            /// object
            /// </summary>
            ID = 7
        }

        public interface IBindGroup : IIndexedCollection<IBind>
        {
            string Name { get; }
            object ID { get; }

            void HandleInput();
            bool DoesBindExist(string name);
            bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null);
            bool TryLoadBindData(IList<BindDefinition> bindData);
            void RegisterBinds(IList<string> bindNames);
            void RegisterBinds(IList<BindDefinition> bindData);
            IBind GetBind(string name);
            bool TryRegisterBind(string bindName, out IBind bind, string[] combo = null, bool silent = false);
            bool TryRegisterBind(string bindName, IControl[] combo, out IBind newBind, bool silent = false);
            BindDefinition[] GetBindDefinitions();
            void ClearSubscribers();
            BindGroupMembers GetApiData();
        }

        public enum BindAccesssors : int
        {
            /// <summary>
            /// out: <see cref="string"/>
            /// </summary>
            Name = 1,

            /// <summary>
            /// out: <see cref="bool"/>
            /// </summary>
            Analog = 2,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnNewPress = 3,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnPressAndHold = 4,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnRelease = 5,

            /// <summary>
            /// out: <see cref="List{T}(int)"/>
            /// </summary>
            GetCombo = 6,

            /// <summary>
            /// in: MyTuple(List(int), bool), out: bool"
            /// </summary>
            SetCombo = 7,

            /// <summary>
            /// void
            /// </summary>
            ClearCombo = 8,

            /// <summary>
            /// void
            /// </summary>
            ClearSubscribers = 9,

            /// <summary>
            /// out: index
            /// </summary>
            Index = 10
        }

        public interface IBind
        {
            string Name { get; }
            int Index { get; }

            /// <summary>
            /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
            /// </summary>
            bool Analog { get; }

            /// <summary>
            /// True if just pressed.
            /// </summary>
            bool IsNewPressed { get; }

            /// <summary>
            /// True if currently pressed.
            /// </summary>
            bool IsPressed { get; }

            /// <summary>
            /// True on new press and after being held for more than 500ms.
            /// </summary>
            bool IsPressedAndHeld { get; }

            /// <summary>
            /// True if just released.
            /// </summary>
            bool IsReleased { get; }

            /// <summary>
            /// Events triggered whenever their corresponding booleans are true.
            /// </summary>
            event Action OnNewPress, OnPressAndHold, OnRelease;

            /// <summary>
            /// Returns a list of the current key combo for this bind.
            /// </summary>
            /// <returns></returns>
            IList<IControl> GetCombo();

            /// <summary>
            /// Attempts to set the binds combo to the given controls. Returns true if successful.
            /// </summary>
            bool TrySetCombo(IControl[] combo, bool strict = true, bool silent = false);

            /// <summary>
            /// Attempts to set the binds combo to the given controls. Returns true if successful.
            /// </summary>
            bool TrySetCombo(IList<string> combo, bool strict = true, bool silent = false);

            /// <summary>
            /// Clears the current key combination.
            /// </summary>
            void ClearCombo();

            /// <summary>
            /// Clears all event subscibers for this bind.
            /// </summary>
            void ClearSubscribers();
            BindMembers GetApiData();
        }

        /// <summary>
        /// Interface for anything used as a control
        /// </summary> 
        public interface IControl
        {
            string Name { get; }
            string DisplayName { get; }
            int Index { get; }
            bool IsPressed { get; }
            bool Analog { get; }
            ControlMembers GetApiData();
        }
    }
}