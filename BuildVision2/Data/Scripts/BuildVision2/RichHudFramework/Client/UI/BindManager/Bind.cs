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
                    public string Name { get; }
                    public int Index { get; }
                    public bool Analog => (bool)GetOrSetMemberFunc(null, (int)BindAccesssors.Analog);
                    public bool IsPressed => IsPressedFunc();
                    public bool IsPressedAndHeld => IsPressedAndHeldFunc();
                    public bool IsNewPressed => IsNewPressedFunc();
                    public bool IsReleased => IsReleasedFunc();

                    public event Action OnNewPress
                    {
                        add { GetOrSetMemberFunc(new EventData(true, value), (int)BindAccesssors.OnNewPress); }
                        remove { GetOrSetMemberFunc(new EventData(false, value), (int)BindAccesssors.OnNewPress); }
                    }
                    public event Action OnPressAndHold
                    {
                        add { GetOrSetMemberFunc(new EventData(true, value), (int)BindAccesssors.OnPressAndHold); }
                        remove { GetOrSetMemberFunc(new EventData(false, value), (int)BindAccesssors.OnPressAndHold); }
                    }
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

                    public IList<IControl> GetCombo() =>
                        BindManager.GetCombo((IList<int>)GetOrSetMemberFunc(null, (int)BindAccesssors.GetCombo));

                    public bool TrySetCombo(IList<string> combo, bool silent = false) =>
                        (bool)GetOrSetMemberFunc(new MyTuple<IList<int>, bool>(GetComboIndices(combo), silent), (int)BindAccesssors.SetCombo);

                    public bool TrySetCombo(IControl[] combo, bool silent = false) =>
                        (bool)GetOrSetMemberFunc(new MyTuple<IList<int>, bool>(GetComboIndices(combo), silent), (int)BindAccesssors.SetCombo);

                    public void ClearCombo() =>
                        GetOrSetMemberFunc(null, (int)BindAccesssors.ClearCombo);

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