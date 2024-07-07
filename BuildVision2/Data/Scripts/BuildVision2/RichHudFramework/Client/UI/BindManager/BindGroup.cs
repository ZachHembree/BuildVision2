using System.Collections.Generic;
using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    using BindDefinitionData = MyTuple<string, string[], string[][]>;

    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            // <summary>
            /// A collection of unique keybinds.
            /// </summary>
            private partial class BindGroup : ReadOnlyApiCollection<IBind>, IBindGroup
            {
                /// <summary>
                /// Returns the bind with the name given, if it exists.
                /// </summary>
                public IBind this[string name] 
                { 
                    get 
                    {
                        IBind bind = GetBind(name);

                        if (bind == null)
                            throw new Exception($"Bind: {name} was not found in bind group {Name}.");
                        else
                            return bind;
                    } 
                }

                /// <summary>
                /// Bind group name
                /// </summary>
                public string Name => _instance.GetOrSetGroupMemberFunc(Index, null, (int)BindGroupAccessors.Name) as string;

                /// <summary>
                /// Index of the bind group in its associated client
                /// </summary>
                public int Index { get; }

                /// <summary>
                /// Unique identifer
                /// </summary>
                public object ID => _instance.GetOrSetGroupMemberFunc(Index, null, (int)BindGroupAccessors.ID);

                public BindGroup(int index) 
                    : base(x => new Bind(new Vector2I(index, x)), () => _instance.GetBindCountFunc(index))
                {
                    Index = index;
                }

                /// <summary>
                /// Returns true if the group contains a bind with the given name.
                /// </summary>
                public bool DoesBindExist(string name) =>
                    (bool)_instance.GetOrSetGroupMemberFunc(Index, name, (int)BindGroupAccessors.DoesBindExist);

                /// <summary>
                /// Returns true if the given list of controls conflicts with any existing binds.
                /// </summary>
                public bool DoesComboConflict(IReadOnlyList<ControlHandle> newCombo, IBind currentBind = null, int alias = 0)
                {
                    var data = new MyTuple<IReadOnlyList<int>, int, int>(GetComboIndicesTemp(newCombo), currentBind?.Index ?? -1, alias);
                    return (bool)_instance.GetOrSetGroupMemberFunc(Index, data, (int)BindGroupAccessors.DoesComboConflict);
                }

                /// <summary>
                /// Determines if given combo is equivalent to any existing binds.
                /// </summary>
                public bool DoesComboConflict(IReadOnlyList<int> newCombo, IBind currentBind = null, int alias = 0)
                {
                    var data = new MyTuple<IReadOnlyList<int>, int, int>(newCombo, currentBind?.Index ?? -1, alias);
                    return (bool)_instance.GetOrSetGroupMemberFunc(Index, data, (int)BindGroupAccessors.DoesComboConflict);
                }

                /// <summary>
                /// Replaces current bind combos with combos based on the given <see cref="BindDefinition"/>[]. Does not register new binds.
                /// </summary>
                public bool TryLoadBindData(IReadOnlyList<BindDefinitionData> bindData) =>
                    (bool)_instance.GetOrSetGroupMemberFunc(Index, bindData, (int)BindGroupAccessors.TryLoadBindData);

                /// <summary>
                /// Attempts to load bind combinations from bind data. Will not register new binds.
                /// </summary>
                public bool TryLoadBindData(IReadOnlyList<BindDefinition> bindData)
                {
                    var defData = new BindDefinitionData[bindData.Count];

                    for (int i = 0; i < bindData.Count; i++)
                    {
                        string[][] aliasData = null;

                        if (bindData[i].aliases != null)
                        {
                            aliasData = new string[bindData[i].aliases.Length][];

                            for (int j = 0; j < aliasData.Length; j++)
                                aliasData[j] = bindData[i].aliases[j];
                        }

                        defData[i] = new BindDefinitionData(bindData[i].name, bindData[i].controlNames, aliasData);
                    }

                    return (bool)_instance.GetOrSetGroupMemberFunc(Index, defData, (int)BindGroupAccessors.TryLoadBindData);
                }

                /// <summary>
                /// Attempts to register a set of binds with the given names.
                /// </summary>
                public void RegisterBinds(BindGroupInitializer bindData)
                {
                    foreach (var bind in bindData)
                        _instance.GetOrSetGroupMemberFunc(Index, bind, (int)BindGroupAccessors.AddBindWithIndices);
                }

                /// <summary>
                /// Registers a list of binds using the names given.
                /// </summary>
                public void RegisterBinds(IReadOnlyList<string> bindNames) =>
                    _instance.GetOrSetGroupMemberFunc(Index, bindNames, (int)BindGroupAccessors.RegisterBindNames);

                /// <summary>
                /// Returns the bind with the name given, if it exists.
                /// </summary>
                public IBind GetBind(string name)
                {
                    var index = (Vector2I)_instance.GetOrSetGroupMemberFunc(Index, name, (int)BindGroupAccessors.GetBindFromName);
                    return index.Y != -1 ? this[index.Y] : null;
                }

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IReadOnlyList<int> newConIDs, IReadOnlyList<IReadOnlyList<int>> aliases = null)
                {
                    var bindData = new MyTuple<string, IReadOnlyList<int>, IReadOnlyList<IReadOnlyList<int>>>();
                    bindData.Item1 = bindName;
                    bindData.Item2 = newConIDs;
                    bindData.Item3 = aliases;

                    var index = (Vector2I)_instance.GetOrSetGroupMemberFunc(Index, bindData, (int)BindGroupAccessors.AddBindWithIndices);                        
                    return this[index.Y];
                }

                /// <summary>
                /// Adds a bind with the given name and the given key combo. Throws an exception if the bind is invalid.
                /// </summary>
                public IBind AddBind(string bindName, IReadOnlyList<ControlHandle> combo, IReadOnlyList<IReadOnlyList<ControlHandle>> aliases = null)
                {
                    var bindData = new MyTuple<string, IReadOnlyList<int>, IReadOnlyList<IReadOnlyList<int>>>();
                    bindData.Item1 = bindName;
                    bindData.Item2 = GetComboIndicesTemp(combo);

                    var aliasData = (aliases.Count > 0) ? new List<int>[aliases.Count] : null;
                    bindData.Item3 = aliasData;

                    if (aliases.Count > 0)
                    {
                        for (int i = 0; i < aliases.Count; i++)
                        {
                            var alias = aliases[i];
                            aliasData[i] = new List<int>();
                            GetComboIndices(alias, aliasData[i]);
                        }
                    }

                    var index = (Vector2I)_instance.GetOrSetGroupMemberFunc(Index, bindData, (int)BindGroupAccessors.AddBindWithIndices);
                    return this[index.Y];
                }

                /// <summary>
                /// Tries to register an empty bind using the given name.
                /// </summary>
                public bool TryRegisterBind(string bindName, out IBind newBind)
                {
                    int index = (int)_instance.GetOrSetGroupMemberFunc(Index, bindName, (int)BindGroupAccessors.TryRegisterBindName);

                    if (index != -1)
                    {
                        newBind = this[index];
                        return true;
                    }
                    else
                    {
                        newBind = null;
                        return false;
                    }
                }

                /// <summary>
                /// Tries to register a bind using the given name and the given key combo.
                /// </summary>
                public bool TryRegisterBind(string bindName, out IBind newBind, IReadOnlyList<int> combo, IReadOnlyList<IReadOnlyList<int>> aliases = null)
                {
                    var bindData = new MyTuple<string, IReadOnlyList<int>, IReadOnlyList<IReadOnlyList<int>>>();
                    bindData.Item1 = bindName;
                    bindData.Item2 = combo;
                    bindData.Item3 = aliases;

                    int index = (int)_instance.GetOrSetGroupMemberFunc(Index, bindData, (int)BindGroupAccessors.TryRegisterBindWithIndices);

                    if (index != -1)
                    {
                        newBind = this[index];
                        return true;
                    }
                    else
                    {
                        newBind = null;
                        return false;
                    }
                }

                /// <summary>
                /// Tries to register a bind using the given name and the given key combo.
                /// </summary>
                public bool TryRegisterBind(string bindName, out IBind newBind, IReadOnlyList<ControlHandle> combo, IReadOnlyList<IReadOnlyList<ControlHandle>> aliases = null)
                {
                    var bindData = new MyTuple<string, IReadOnlyList<int>, IReadOnlyList<IReadOnlyList<int>>>();
                    bindData.Item1 = bindName;
                    bindData.Item2 = GetComboIndicesTemp(combo);

                    var aliasData = (aliases.Count > 0) ? new List<int>[aliases.Count] : null;
                    bindData.Item3 = aliasData;

                    int index = (int)_instance.GetOrSetGroupMemberFunc(Index, bindData, (int)BindGroupAccessors.TryRegisterBindWithIndices);

                    if (index != -1)
                    {
                        newBind = this[index];
                        return true;
                    }
                    else
                    {
                        newBind = null;
                        return false;
                    }
                }

                /// <summary>
                /// Retrieves the set of key binds as an array of BindDefinition
                /// </summary>
                public BindDefinition[] GetBindDefinitions()
                {
                    var bindData = _instance.GetOrSetGroupMemberFunc(Index, null, (int)BindGroupAccessors.GetBindData) as BindDefinitionData[];
                    var definitions = new BindDefinition[bindData.Length];

                    for (int n = 0; n < bindData.Length; n++)
                        definitions[n] = new BindDefinition(bindData[n].Item1, bindData[n].Item2);

                    return definitions;
                }

                /// <summary>
                /// Retrieves the set of key binds as an array of BindDefinition
                /// </summary>
                public BindDefinitionData[] GetBindData() =>
                    _instance.GetOrSetGroupMemberFunc(Index, null, (int)BindGroupAccessors.GetBindData) as BindDefinitionData[];

                /// <summary>
                /// Clears bind subscribers for the entire group
                /// </summary>
                public void ClearSubscribers() =>
                    _instance.GetOrSetGroupMemberFunc(Index, null, (int)BindGroupAccessors.ClearSubscribers);

                public override bool Equals(object obj)
                {
                    return Index.Equals(obj);
                }

                public override int GetHashCode()
                {
                    return Index.GetHashCode();
                }
            }
        }
    }
}