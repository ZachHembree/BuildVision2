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
    using BindGroupMembers = MyTuple<
        string, // Name                
        BindMembers[], // Binds
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMember
    >;

    namespace UI
    {
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
    }
}