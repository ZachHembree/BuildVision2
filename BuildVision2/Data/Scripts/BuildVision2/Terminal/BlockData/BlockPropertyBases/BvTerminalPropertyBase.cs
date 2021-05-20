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
        private abstract class BvTerminalPropertyBase : BlockMemberBase, IBlockProperty
        {
            /// <summary>
            /// Unique identifier associated with the property
            /// </summary>
            public abstract StringBuilder PropName { get; }

            /// <summary>
            /// Cached hashcode of StringID
            /// </summary>
            public abstract int ID { get; }

            /// <summary>
            /// Returns a serializable representation of the property.
            /// </summary>
            public abstract PropertyData GetPropertyData();

            /// <summary>
            /// Attempts to apply the given property data.
            /// </summary>
            public abstract bool TryImportPropertyValue(PropertyData data);
        }

        /// <summary>
        /// Base class for Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BvTerminalPropertyBase<TProp, TValue> : BvTerminalPropertyBase where TProp : class, ITerminalProperty
        {
            public override StringBuilder PropName { get; }

            public sealed override int ID
            {
                get 
                {
                    if (id == int.MinValue)
                        id = PropName.GetHashCode();

                    return id;
                }
            }

            public override bool Enabled 
            { 
                get 
                {
                    try
                    {
                        return control.Enabled(block.TBlock) && control.Visible(block.TBlock);
                    }
                    catch { }

                    return false;
                } 
            }

            protected TProp property;
            protected IMyTerminalControl control;
            protected PropertyBlock block;

            private Func<IMyTerminalBlock, TValue> Getter;
            private Action<IMyTerminalBlock, TValue> Setter;

            private int id;

            public BvTerminalPropertyBase()
            {
                Name = new StringBuilder();
                PropName = new StringBuilder();
            }

            protected virtual void SetPropertyInternal(StringBuilder name, TProp property, IMyTerminalControl control, PropertyBlock block, Func<IMyTerminalBlock, TValue> Getter, Action<IMyTerminalBlock, TValue> Setter)
            {
                Name.Clear();
                Name.Append(name);

                PropName.Clear();
                PropName.Append(property.Id);

                id = int.MinValue;

                this.property = property;
                this.control = control;
                this.block = block;

                this.Getter = Getter;
                this.Setter = Setter;
            }

            public override void Reset()
            {
                Name.Clear();

                this.property = null;
                this.control = null;
                this.block = null;

                this.Getter = null;
                this.Setter = null;
            }

            /// <summary>
            /// Safely retrieves the current value of the property. Will return default if the property is not enabled
            /// or if an error occurs.
            /// </summary>
            public virtual TValue GetValue()
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
            /// Safely sets the current value of the property to the one given if the property is enabled.
            /// </summary>
            public virtual void SetValue(TValue value)
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

            public override bool TryImportPropertyValue(PropertyData data)
            {
                TValue value;

                if (TryParseValue(data.valueData, out value))
                {
                    SetValue(value);
                    return true;
                }
                else
                    return false;
            }

            public override PropertyData GetPropertyData() =>
                new PropertyData(PropName.ToString(), ID, GetValue().ToString());

            public abstract bool TryParseValue(string valueData, out TValue value);
        }

        private abstract class BvTerminalProperty<TProp, TValue> : BvTerminalPropertyBase<TProp, TValue> where TProp : class, ITerminalProperty<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, IMyTerminalControl control, PropertyBlock block)
            {
                SetPropertyInternal(name, property, control, block, property.GetValue, property.SetValue);
            }
        }

        private abstract class BvTerminalValueControl<TProp, TValue> : BvTerminalPropertyBase<TProp, TValue> where TProp : class, IMyTerminalValueControl<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, IMyTerminalControl control, PropertyBlock block)
            {
                SetPropertyInternal(name, property, control, block, property.Getter, property.Setter);
            }
        }
    }
}