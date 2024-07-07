using System;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    using EventData = MyTuple<bool, Action>;

    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private partial class BindGroup
            {
                public class Bind : IBind
                {
                    /// <summary>
                    /// Name of the keybind
                    /// </summary>
                    public string Name => _instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.Name) as string;

                    /// <summary>
                    /// Index of the bind within its group
                    /// </summary>
                    public int Index => index.Y;

                    /// <summary>
                    /// Number of key combinations registered to the given bind. If AliasCount == 1, then only
                    /// the main combo is set. If greater, then it is aliased.
                    /// </summary>
                    public int AliasCount => (int)_instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.AliasCount);

                    /// <summary>
                    /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
                    /// </summary>
                    public bool Analog => (bool)_instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.Analog);

                    /// <summary>
                    /// Analog value of the bind, if it has one. Returns the sum of all analog values in
                    /// key combo. Multiple analog controls per bind are not recommended.
                    /// </summary>
                    public float AnalogValue => (float)_instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.AnalogValue);

                    /// <summary>
                    /// True if currently pressed.
                    /// </summary>
                    public bool IsPressed => _instance.IsBindPressedFunc(index, (int)BindAccesssors.IsPressed);

                    /// <summary>
                    /// True after being held for more than 500ms.
                    /// </summary>
                    public bool IsPressedAndHeld => _instance.IsBindPressedFunc(index, (int)BindAccesssors.IsPressedAndHeld);

                    /// <summary>
                    /// True if just pressed.
                    /// </summary>
                    public bool IsNewPressed => _instance.IsBindPressedFunc(index, (int)BindAccesssors.IsNewPressed);

                    /// <summary>
                    /// True if just released.
                    /// </summary>
                    public bool IsReleased => _instance.IsBindPressedFunc(index, (int)BindAccesssors.IsReleased);

                    /// <summary>
                    /// Invoked when the bind is first pressed.
                    /// </summary>
                    public event EventHandler NewPressed;

                    /// <summary>
                    /// Invoked after the bind has been held and pressed for at least 500ms.
                    /// </summary>
                    public event EventHandler PressedAndHeld;

                    /// <summary>
                    /// Invoked after the bind has been released.
                    /// </summary>
                    public event EventHandler Released;

                    private readonly Vector2I index;

                    public Bind(Vector2I index)
                    {
                        this.index = index;
                        _instance.GetOrSetBindMemberFunc(index, new EventData(true, OnNewPressed), (int)BindAccesssors.OnNewPress);
                        _instance.GetOrSetBindMemberFunc(index, new EventData(true, OnPressedAndHeld), (int)BindAccesssors.OnPressAndHold);
                        _instance.GetOrSetBindMemberFunc(index, new EventData(true, OnReleased), (int)BindAccesssors.OnRelease);
                    }

                    private void OnNewPressed()
                    {
                        NewPressed?.Invoke(this, EventArgs.Empty);
                    }

                    private void OnPressedAndHeld()
                    {
                        PressedAndHeld?.Invoke(this, EventArgs.Empty);
                    }

                    private void OnReleased()
                    {
                        Released?.Invoke(this, EventArgs.Empty);
                    }

                    /// <summary>
                    /// Returns a list of controls representing the key combinaton for the bind
                    /// </summary>
                    public List<ControlHandle> GetCombo(int alias = 0)
                    {
                        var indices = _instance.GetOrSetBindMemberFunc(index, alias, (int)BindAccesssors.GetCombo) as List<int>;
                        var combo = new List<ControlHandle>(indices.Count);

                        for (int n = 0; n < indices.Count; n++)
                            combo.Add((ControlHandle)indices[n]);

                        return combo;
                    }

                    /// <summary>
                    /// Returns a list of control indices representing the key combinaton for the bind
                    /// </summary>
                    public List<int> GetConIDs(int alias = 0) =>
                        _instance.GetOrSetBindMemberFunc(index, alias, (int)BindAccesssors.GetCombo) as List<int>;

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<ControlHandle> combo, int alias = 0, bool isStrict = true, bool isSilent = true)
                    {
                        var comboData = new MyTuple<IReadOnlyList<int>, int, bool, bool>(GetComboIndicesTemp(combo), alias, isStrict, isSilent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithIndices);
                    }

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<int> combo, int alias = 0, bool isStrict = true, bool isSilent = true)
                    {
                        var comboData = new MyTuple<IReadOnlyList<int>, int, bool, bool>(combo, alias, isStrict, isSilent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithIndices);
                    }

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<string> combo, int alias = 0, bool isStrict = true, bool isSilent = true)
                    {
                        var comboData = new MyTuple<IReadOnlyList<string>, int, bool, bool>(combo, alias, isStrict, isSilent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithNames);
                    }

                    /// <summary>
                    /// Clears the current key combination.
                    /// </summary>
                    public void ClearCombo(int alias = 0) =>
                        _instance.GetOrSetBindMemberFunc(index, alias, (int)BindAccesssors.ClearCombo);

                    /// <summary>
                    /// Clears all event subscibers for this bind.
                    /// </summary>
                    public void ClearSubscribers() =>
                        _instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.ClearSubscribers);

                    public override bool Equals(object obj)
                    {
                        return ((Bind)obj).index == index;
                    }

                    public override int GetHashCode()
                    {
                        return index.GetHashCode();
                    }
                }
            }
        }
    }
}