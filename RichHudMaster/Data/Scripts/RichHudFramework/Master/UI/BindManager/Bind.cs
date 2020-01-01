using RichHudFramework.Game;
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
    namespace UI.Server
    {
        public sealed partial class BindManager
        {
            private partial class BindGroup
            {
                /// <summary>
                /// Logic and data for individual keybinds
                /// </summary>
                private class Bind : IBind
                {
                    public event Action OnNewPress, OnPressAndHold, OnRelease;

                    public string Name { get; }
                    public int Index { get; }
                    public bool Analog { get; set; }
                    public bool IsPressed { get; private set; }
                    public bool IsNewPressed { get { return IsPressed && (!wasPressed || Analog); } }
                    public bool IsPressedAndHeld { get; private set; }
                    public bool IsReleased { get { return !IsPressed && wasPressed; } }

                    public bool beingReleased;
                    public int length, bindHits;

                    private bool wasPressed;
                    private readonly Utils.Stopwatch stopwatch;
                    private readonly BindGroup group;

                    public Bind(string name, int index, BindGroup group)
                    {
                        Name = name;
                        Index = index;
                        stopwatch = new Utils.Stopwatch();
                        this.group = group;

                        IsPressedAndHeld = false;
                        wasPressed = false;

                        bindHits = 0;
                        Analog = false;
                        beingReleased = false;
                        length = 0;
                    }

                    /// <summary>
                    /// Used to update the key bind with each tick of the Binds.Update function. 
                    /// </summary>
                    public void UpdatePress(bool isPressed)
                    {
                        wasPressed = IsPressed;
                        IsPressed = isPressed;

                        if (IsNewPressed)
                        {
                            OnNewPress?.Invoke();
                            stopwatch.Start();
                        }

                        IsPressedAndHeld = IsNewPressed || (IsPressed && stopwatch.ElapsedTicks > holdTime);

                        if (IsReleased)
                            OnRelease?.Invoke();

                        if (IsPressedAndHeld)
                            OnPressAndHold?.Invoke();
                    }

                    public IList<IControl> GetCombo()
                    {
                        List<IControl> combo = new List<IControl>();

                        foreach (IControl con in group.usedControls)
                        {
                            if (group.BindUsesControl(this, con))
                                combo.Add(con);
                        }

                        return combo;
                    }

                    private List<int> GetComboIndices()
                    {
                        List<int> combo = new List<int>();

                        foreach (IControl con in group.usedControls)
                        {
                            if (group.BindUsesControl(this, con))
                                combo.Add(con.Index);
                        }

                        return combo;
                    }

                    public bool TrySetCombo(IList<string> combo, bool silent = false) =>
                        TrySetCombo(BindManager.GetCombo(combo), silent);

                    /// <summary>
                    /// Tries to update a key bind using the names of the controls to be bound.
                    /// </summary>
                    public bool TrySetCombo(IControl[] combo, bool silent = false)
                    {
                        if (combo.Length <= maxBindLength && combo.Length > 0)
                        {
                            if (!group.DoesComboConflict(combo, this))
                            {
                                group.RegisterBindToCombo(this, combo);
                                return true;
                            }
                            else if (!silent)
                                ModBase.SendChatMessage($"Invalid bind for {group.Name}.{Name}. One or more of the given controls conflict with existing binds.");
                        }
                        else if (!silent)
                        {
                            if (combo.Length > 0)
                                ModBase.SendChatMessage($"Invalid key bind. No more than {maxBindLength} keys in a bind are allowed.");
                            else
                                ModBase.SendChatMessage("Invalid key bind. There must be at least one control in a key bind.");
                        }

                        return false;
                    }

                    private bool TrySetCombo(IList<int> indices, bool silent = false)
                    {
                        IControl[] combo = new IControl[indices.Count];

                        for (int n = 0; n < indices.Count; n++)
                            combo[n] = BindManager.Controls[indices[n]];

                        return TrySetCombo(combo);
                    }

                    public void ClearCombo() =>
                        group.UnregisterBindFromCombo(this);

                    public void ClearSubscribers()
                    {
                        OnNewPress = null;
                        OnPressAndHold = null;
                        OnRelease = null;
                    }

                    private object GetOrSetMember(object data, int memberEnum)
                    {
                        var member = (BindAccesssors)memberEnum;

                        if (member == BindAccesssors.Name)
                        {
                            return Name;
                        }
                        else if (member == BindAccesssors.Analog)
                        {
                            return Analog;
                        }
                        else if (member == BindAccesssors.Index)
                        {
                            return Index;
                        }
                        else if (member == BindAccesssors.OnNewPress)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnNewPress += eventData.Item2;
                            else
                                OnNewPress -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.OnPressAndHold)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnPressAndHold += eventData.Item2;
                            else
                                OnPressAndHold -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.OnRelease)
                        {
                            var eventData = (MyTuple<bool, Action>)data;

                            if (eventData.Item1)
                                OnRelease += eventData.Item2;
                            else
                                OnRelease -= eventData.Item2;
                        }
                        else if (member == BindAccesssors.GetCombo)
                            return GetComboIndices();
                        else if (member == BindAccesssors.SetCombo)
                        {
                            var comboData = (MyTuple<List<int>, bool>)data;
                            return TrySetCombo(comboData.Item1, comboData.Item2);
                        }
                        else if (member == BindAccesssors.ClearCombo)
                        {
                            ClearCombo();
                        }
                        else if (member == BindAccesssors.ClearSubscribers)
                            ClearSubscribers();

                        return null;
                    }

                    public BindMembers GetApiData()
                    {
                        return new BindMembers()
                        {
                            Item1 = GetOrSetMember,
                            Item2 = () => IsPressed,
                            Item3 = () => IsNewPressed,
                            Item4 = () => IsPressedAndHeld,
                            Item5 = () => IsReleased
                        };
                    }
                }
            }
        }
    }
}