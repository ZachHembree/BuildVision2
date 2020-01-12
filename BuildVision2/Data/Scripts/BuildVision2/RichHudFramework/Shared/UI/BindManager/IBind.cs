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
    namespace UI
    {
        internal enum BindAccesssors : int
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
    }
}