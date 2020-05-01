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
        using Client;

        public enum BindGroupAccessors : int
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

        /// <summary>
        /// A collection of unique keybinds.
        /// </summary>
        public interface IBindGroup : IIndexedCollection<IBind>
        {
            /// <summary>
            /// Bind group name
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Unique identifer
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Updates input state
            /// </summary>
            void HandleInput();

            /// <summary>
            /// Returns true if the group contains a bind with the given name.
            /// </summary>
            bool DoesBindExist(string name);

            /// <summary>
            /// Returns true if the given list of controls conflicts with any existing binds.
            /// </summary>
            bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null);

            /// <summary>
            /// Attempts to load bind combinations from bind data. Will not register new binds.
            /// </summary>
            bool TryLoadBindData(IList<BindDefinition> bindData);

            /// <summary>
            /// Registers a list of binds using the names given paired with associated control indices.
            /// </summary>
            void RegisterBinds(IEnumerable<MyTuple<string, IList<int>>> bindData);

            /// <summary>
            /// Registers a list of binds using the names given.
            /// </summary>
            void RegisterBinds(IList<string> bindNames);

            /// <summary>
            /// Registers and loads bind combinations from BindDefinitions.
            /// </summary>
            void RegisterBinds(IList<BindDefinition> bindData);

            /// <summary>
            /// Returns the bind with the name given, if it exists.
            /// </summary>
            IBind GetBind(string name);

            /// <summary>
            /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
            /// </summary>
            IBind AddBind(string bindName, IList<string> combo);

            /// <summary>
            /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
            /// </summary>
            IBind AddBind(string bindName, IList<ControlData> combo = null);

            /// <summary>
            /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
            /// </summary>
            IBind AddBind(string bindName, IList<int> combo);

            /// <summary>
            /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
            /// </summary>
            IBind AddBind(string bindName, IList<IControl> combo = null);

            /// <summary>
            /// Tries to register a bind using the given name and the given key combo.
            /// </summary>
            bool TryRegisterBind(string bindName, out IBind bind, IList<string> combo = null);

            /// <summary>
            /// Tries to register a bind using the given name and the given key combo. Shows an error message in chat upon failure.
            /// </summary>
            bool TryRegisterBind(string bindName, IList<IControl> combo, out IBind newBind);

            /// <summary>
            /// Tries to register a bind using the given name and the given key combo. Shows an error message in chat upon failure.
            /// </summary>
            bool TryRegisterBind(string bindName, IList<int> combo, out IBind newBind);

            /// <summary>
            /// Retrieves the set of key binds as an array of BindDefinitions.
            /// </summary>
            BindDefinition[] GetBindDefinitions();

            /// <summary>
            /// Retreives information needed to access the BindGroup via the API.
            /// </summary>
            BindGroupMembers GetApiData();
        }
    }
}