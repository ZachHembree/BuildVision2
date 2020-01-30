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

namespace RichHudFramework
{
    using EventData = MyTuple<bool, Action>;

    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private partial class BindGroup
            {
                private class Bind : IBind
                {
                    /// <summary>
                    /// Name of the keybind
                    /// </summary>
                    public string Name { get; }

                    /// <summary>
                    /// Index of the bind within its group
                    /// </summary>
                    public int Index { get; }

                    /// <summary>
                    /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
                    /// </summary>
                    public bool Analog => (bool)GetOrSetMemberFunc(null, (int)BindAccesssors.Analog);

                    /// <summary>
                    /// True if currently pressed.
                    /// </summary>
                    public bool IsPressed => IsPressedFunc();

                    /// <summary>
                    /// True after being held for more than 500ms.
                    /// </summary>
                    public bool IsPressedAndHeld => IsPressedAndHeldFunc();

                    /// <summary>
                    /// True if just pressed.
                    /// </summary>
                    public bool IsNewPressed => IsNewPressedFunc();

                    /// <summary>
                    /// True if just released.
                    /// </summary>
                    public bool IsReleased => IsReleasedFunc();

                    /// <summary>
                    /// Invoked when the bind is first pressed.
                    /// </summary>
                    public event Action OnNewPress
                    {
                        add { GetOrSetMemberFunc(new EventData(true, value), (int)BindAccesssors.OnNewPress); }
                        remove { GetOrSetMemberFunc(new EventData(false, value), (int)BindAccesssors.OnNewPress); }
                    }

                    /// <summary>
                    /// Invoked after the bind has been held and pressed for at least 500ms.
                    /// </summary>
                    public event Action OnPressAndHold
                    {
                        add { GetOrSetMemberFunc(new EventData(true, value), (int)BindAccesssors.OnPressAndHold); }
                        remove { GetOrSetMemberFunc(new EventData(false, value), (int)BindAccesssors.OnPressAndHold); }
                    }

                    /// <summary>
                    /// Invoked after the bind has been released.
                    /// </summary>
                    public event Action OnRelease
                    {
                        add { GetOrSetMemberFunc(new EventData(true, value), (int)BindAccesssors.OnRelease); }
                        remove { GetOrSetMemberFunc(new EventData(false, value), (int)BindAccesssors.OnRelease); }
                    }

                    private readonly Func<object, int, object> GetOrSetMemberFunc;
                    private readonly Func<bool> IsPressedFunc, IsPressedAndHeldFunc, IsNewPressedFunc, IsReleasedFunc;

                    public Bind(BindMembers data)
                    {
                        GetOrSetMemberFunc = data.Item1;
                        IsPressedFunc = data.Item2;
                        IsNewPressedFunc = data.Item3;
                        IsPressedAndHeldFunc = data.Item4;
                        IsReleasedFunc = data.Item5;

                        Name = (string)GetOrSetMemberFunc(null, (int)BindAccesssors.Name);
                        Index = (int)GetOrSetMemberFunc(null, (int)BindAccesssors.Index);
                    }

                    /// <summary>
                    /// Returns a list of the current key combo for this bind.
                    /// </summary>
                    public IList<IControl> GetCombo() =>
                        BindManager.GetCombo((IList<int>)GetOrSetMemberFunc(null, (int)BindAccesssors.GetCombo));

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IList<string> combo, bool strict = true, bool silent = false) =>
                        TrySetCombo(GetComboIndices(combo), strict, silent);

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IList<IControl> combo, bool strict = true, bool silent = false) =>
                        TrySetCombo(GetComboIndices(combo), strict, silent);

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IList<int> combo, bool strict = true, bool silent = false) =>
                        (bool)GetOrSetMemberFunc(new MyTuple<IList<int>, bool, bool>(combo, strict, silent), (int)BindAccesssors.SetCombo);

                    /// <summary>
                    /// Clears the current key combination.
                    /// </summary>
                    public void ClearCombo() =>
                        GetOrSetMemberFunc(null, (int)BindAccesssors.ClearCombo);

                    /// <summary>
                    /// Clears all event subscibers for this bind.
                    /// </summary>
                    public void ClearSubscribers() =>
                        GetOrSetMemberFunc(null, (int)BindAccesssors.ClearSubscribers);

                    public BindMembers GetApiData()
                    {
                        return new BindMembers();
                    }
                }
            }
        }
    }
}