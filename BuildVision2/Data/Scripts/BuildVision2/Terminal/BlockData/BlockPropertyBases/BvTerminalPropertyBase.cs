using System;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        private abstract class BvTerminalPropertyBase : BlockMemberBase, IBlockProperty
        {
            /// <summary>
            /// Unique identifier associated with the property
            /// </summary>
            public abstract string PropName { get; }

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
        private abstract class BvTerminalPropertyBase<TProp, TValue> : BvTerminalPropertyBase where TProp : ITerminalProperty
        {
            public override string PropName => property.Id;

            public override int ID { get; }

            public override bool Enabled => control.Enabled(block) && control.Visible(block);

            protected readonly TProp property;
            protected readonly IMyTerminalControl control;
            protected readonly IMyTerminalBlock block;

            private readonly Func<IMyTerminalBlock, TValue> Getter;
            private readonly Action<IMyTerminalBlock, TValue> Setter;

            protected BvTerminalPropertyBase(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block, Func<IMyTerminalBlock, TValue> Getter, Action<IMyTerminalBlock, TValue> Setter)
            {
                Name = name;

                this.property = property;
                this.control = control;
                this.block = block;
                
                this.Getter = Getter;
                this.Setter = Setter;

                ID = property.Id.GetHashCode();
            }

            /// <summary>
            /// Safely retrieves the current value of the property. Will return default if the property is not enabled
            /// or if an error occurs.
            /// </summary>
            public TValue GetValue()
            {
                try
                {
                    // Because some custom blocks don't have their properties set up correctly
                    // and I have to live with it
                    if (control.Enabled(block) && control.Visible(block))
                        return Getter(block);
                }
                catch
                { }

                return default(TValue);
            }

            /// <summary>
            /// Safely sets the current value of the property to the one given if the property is enabled.
            /// </summary>
            public void SetValue(TValue value)
            {
                try
                {
                    if (control.Enabled(block) && control.Visible(block))
                        Setter(block, value);
                }
                catch { }
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
                new PropertyData(PropName, ID, GetValue().ToString());

            public abstract bool TryParseValue(string valueData, out TValue value);
        }

        private abstract class BvTerminalProperty<TProp, TValue> : BvTerminalPropertyBase<TProp, TValue> where TProp : ITerminalProperty<TValue>
        {         
            protected BvTerminalProperty(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) 
                : base(name, property, control, block, property.GetValue, property.SetValue) { }          
        }

        private abstract class BvTerminalValueControl<TProp, TValue> : BvTerminalPropertyBase<TProp, TValue> where TProp : IMyTerminalValueControl<TValue>
        {
            protected BvTerminalValueControl(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) 
                : base(name, property, control, block, property.Getter, property.Setter) { }
        }
    }
}