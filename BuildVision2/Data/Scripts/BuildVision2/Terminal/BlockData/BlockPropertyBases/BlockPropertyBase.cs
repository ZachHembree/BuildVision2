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
            /// Returns a serializable representation of the property.
            /// </summary>
            public abstract PropertyData GetPropertyData();

            /// <summary>
            /// Attempts to apply the given property data.
            /// </summary>
            public abstract bool TryImportData(PropertyData data);
        }

        /// <summary>
        /// Base class for Build Vision terminal properties that make use of SE's <see cref="ITerminalProperty"/>
        /// </summary>
        private abstract class BlockPropertyBase<TProp, TValue> : BlockPropertyBase 
            where TProp : class, ITerminalProperty
        {
            public override string PropName => property?.Id;

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

            public BlockPropertyBase()
            {
                Name = new StringBuilder();
            }

            protected virtual void SetPropertyInternal(StringBuilder name, TProp property, PropertyBlock block, Func<IMyTerminalBlock, TValue> Getter, Action<IMyTerminalBlock, TValue> Setter)
            {
                Name.Clear();
                Name.Append(name);

                this.property = property;
                this.control = property as IMyTerminalControl;
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

            public override bool TryImportData(PropertyData data)
            {
                TValue value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    SetValue(value);
                    return true;
                }
                else
                    return false;
            }

            public override PropertyData GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(GetValue(), out valueData) == null)
                {
                    return new PropertyData(PropName.ToString(), valueData);
                }
                else
                    return default(PropertyData);
            }

            public abstract bool TryParseValue(string valueData, out TValue value);
        }

        private abstract class BvTerminalProperty<TProp, TValue> : BlockPropertyBase<TProp, TValue> where TProp : class, ITerminalProperty<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, PropertyBlock block)
            {
                SetPropertyInternal(name, property, block, property.GetValue, property.SetValue);
            }
        }

        private abstract class BvTerminalValueControl<TProp, TValue> : BlockPropertyBase<TProp, TValue> where TProp : class, IMyTerminalValueControl<TValue>
        {
            public virtual void SetProperty(StringBuilder name, TProp property, PropertyBlock block)
            {
                SetPropertyInternal(name, property, block, property.Getter, property.Setter);
            }
        }
    }
}