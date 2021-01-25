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
                    /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
                    /// </summary>
                    public bool Analog => (bool)_instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.Analog);

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
                    public event Action NewPressed
                    {
                        add { _instance.GetOrSetBindMemberFunc(index, new EventData(true, value), (int)BindAccesssors.OnNewPress); }
                        remove { _instance.GetOrSetBindMemberFunc(index, new EventData(false, value), (int)BindAccesssors.OnNewPress); }
                    }

                    /// <summary>
                    /// Invoked after the bind has been held and pressed for at least 500ms.
                    /// </summary>
                    public event Action PressedAndHeld
                    {
                        add { _instance.GetOrSetBindMemberFunc(index, new EventData(true, value), (int)BindAccesssors.OnPressAndHold); }
                        remove { _instance.GetOrSetBindMemberFunc(index, new EventData(false, value), (int)BindAccesssors.OnPressAndHold); }
                    }

                    /// <summary>
                    /// Invoked after the bind has been released.
                    /// </summary>
                    public event Action Released
                    {
                        add { _instance.GetOrSetBindMemberFunc(index, new EventData(true, value), (int)BindAccesssors.OnRelease); }
                        remove { _instance.GetOrSetBindMemberFunc(index, new EventData(false, value), (int)BindAccesssors.OnRelease); }
                    }

                    private readonly Vector2I index;

                    public Bind(Vector2I index)
                    {
                        this.index = index;
                    }

                    /// <summary>
                    /// Returns a list of the current key combo for this bind.
                    /// </summary>
                    public List<IControl> GetCombo() 
                    {
                        var indices = _instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.GetCombo) as List<int>;
                        var combo = new List<IControl>(indices.Count);

                        for (int n = 0; n < indices.Count; n++)
                            combo.Add(_instance.controls[indices[n]]);

                        return combo;
                    }

                    /// <summary>
                    /// Returns a list of control indices for the current bind combo
                    /// </summary>
                    public List<int> GetComboIndices() =>
                        _instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.GetCombo) as List<int>;

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<IControl> combo, bool strict = true, bool silent = true)
                    {
                        var indices = new int[combo.Count];

                        for (int n = 0; n < combo.Count; n++)
                            indices[n] = combo[n].Index;

                        var comboData = new MyTuple<IReadOnlyList<int>, bool, bool>(indices, strict, silent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithIndices);
                    }

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<int> combo, bool strict = true, bool silent = true)
                    {
                        var comboData = new MyTuple<IReadOnlyList<int>, bool, bool>(combo, strict, silent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithIndices);
                    }

                    /// <summary>
                    /// Attempts to set the binds combo to the given controls. Returns true if successful.
                    /// </summary>
                    public bool TrySetCombo(IReadOnlyList<string> combo, bool strict = true, bool silent = true)
                    {
                        var comboData = new MyTuple<IReadOnlyList<string>, bool, bool>(combo, strict, silent);
                        return (bool)_instance.GetOrSetBindMemberFunc(index, comboData, (int)BindAccesssors.TrySetComboWithNames);
                    }

                    /// <summary>
                    /// Clears the current key combination.
                    /// </summary>
                    public void ClearCombo() =>
                        _instance.GetOrSetBindMemberFunc(index, null, (int)BindAccesssors.ClearCombo);

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