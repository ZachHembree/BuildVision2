using RichHudFramework;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Text;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        private abstract class BlockPropertyBase : BlockMemberBase, IBlockProperty
        {
            /// <summary>
            /// Returns flags associated with the property
            /// </summary>
            public virtual BlockPropertyFlags Flags { get; protected set; }

            /// <summary>
            /// Returns a serializable representation of the property.
            /// </summary>
            public abstract PropertyData? GetPropertyData();

            /// <summary>
            /// Attempts to apply the given property data.
            /// </summary>
            public abstract bool TryImportData(PropertyData data);

            public BlockPropertyBase()
            {
                Flags = BlockPropertyFlags.CanUseMultipliers;
            }
        }

        /// <summary>
        /// Base class for Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BlockPropertyBase<TProp, TValue> : BlockPropertyBase
            where TProp : class, ITerminalProperty
        {
            private const string BuildAndRepairPrefix = "Build And Repair. ";

            public override string PropName => property?.Id;

            /// <summary>
            /// Get/sets the value associated with the property
            /// </summary>
            public virtual TValue Value
            {
                get { return _value; }
                set
                {
                    if (!_value.Equals(value))
                        valueChanged = true;

                    _value = value;
                }
            }

            /// <summary>
            /// Returns true if the associated terminal control is enabled and visible
            /// </summary>
            public override bool Enabled { get; protected set; }

            protected TProp property;
            protected IMyTerminalControl control;
            protected PropertyBlock block;
            protected Func<IMyTerminalBlock, TValue> Getter;
            protected Action<IMyTerminalBlock, TValue> Setter;
            protected TValue _value;
            protected bool valueChanged;

            /// <summary>
            /// Exception flag for build and repair workarounds
            /// </summary>
            protected bool isBuildAndRepair;

            public BlockPropertyBase()
            {
                Name = new StringBuilder();
            }

            protected virtual void SetPropertyInternal(StringBuilder name, TProp property, PropertyBlock block,
                Func<IMyTerminalBlock, TValue> Getter, Action<IMyTerminalBlock, TValue> Setter)
            {
                // Build and Repair workaround
                if (name.ContainsSubstring(0, BuildAndRepairPrefix))
                {
                    isBuildAndRepair = true;
                    name.Remove(0, BuildAndRepairPrefix.Length);
                }

                Name.Clear();
                Name.Append(name);

                this.property = property;
                this.control = property as IMyTerminalControl;
                this.block = block;

                this.Getter = Getter;
                this.Setter = Setter;
            }

            /// <summary>
            /// Resets the property object for reuse
            /// </summary>
            public override void Reset()
            {
                Name.Clear();

                this.property = null;
                this.control = null;
                this.block = null;

                this.Getter = null;
                this.Setter = null;

                Enabled = false;
                valueChanged = false;
                isBuildAndRepair = false;
                _value = default(TValue);
            }

            public override void Update(bool sync)
            {
                Enabled = GetEnabled();

                if (Enabled)
                {
                    if (valueChanged && sync || !valueChanged)
                    {
                        if (valueChanged)
                            SetValue(_value);

                        TValue newValue = GetValue();
                        _value = newValue;
                        valueChanged = false;
                    }
                    else
                        SetValue(_value);
                }
                else
                    valueChanged = false;
            }

            /// <summary>
            /// Returns the value of the associated block property to the given value, if the corresponding control
            /// is enabled
            /// </summary>
            protected virtual TValue GetValue()
            {
                if (Enabled)
                {
                    try
                    {
                        return Getter(block.TBlock);
                    }
                    catch { }
                }

                return default(TValue);
            }

            /// <summary>
            /// Sets the value of the associated block property to the given value, if the corresponding control
            /// is enabled
            /// </summary>
            protected virtual void SetValue(TValue value)
            {
                if (Enabled)
                {
                    try
                    {
                        Setter(block.TBlock, value);
                    }
                    catch { }
                }
            }

            /// <summary>
            /// Returns true if the associated control is enabled and visible
            /// </summary>
            protected virtual bool GetEnabled()
            {
                try
                {
                    return (DebugVisibility || isBuildAndRepair) 
                        || (control.Enabled(block.TBlock) && control.Visible(block.TBlock));
                }
                catch { }

                return false;
            }

            public override bool TryImportData(PropertyData data)
            {
                TValue value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    Value = value;
                    return true;
                }
                else
                    return false;
            }

            public override PropertyData? GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(Value, out valueData) == null)
                {
                    return new PropertyData(PropName.ToString(), valueData, Enabled, ValueType);
                }
                else
                    return default(PropertyData);
            }

            public abstract bool TryParseValue(string valueData, out TValue value);
        }

        private abstract class BvTerminalProperty<TProp, TValue> : BlockPropertyBase<TProp, TValue> 
            where TProp : class, ITerminalProperty<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, PropertyBlock block)
            {
                SetPropertyInternal(name, property, block, property.GetValue, property.SetValue);
            }
        }

        private abstract class BvTerminalValueControl<TProp, TValue> : BlockPropertyBase<TProp, TValue> 
            where TProp : class, IMyTerminalValueControl<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, PropertyBlock block)
            {
                SetPropertyInternal(name, property, block, property.Getter, property.Setter);
            }
        }
    }
}