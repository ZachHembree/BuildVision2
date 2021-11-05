using RichHudFramework;
using Sandbox.Game.Localization;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Utils;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : BvTerminalProperty<ITerminalProperty<bool>, bool>, IBlockAction
        {
            public override StringBuilder Display => GetPropStateText();

            public override StringBuilder Status 
            {
                get 
                {
                    if (GetPostfixFunc != null)
                    {
                        GetPostfixFunc(statusBuilder);
                        return statusBuilder;
                    }
                    else
                        return null;
                }
            }

            private Action<StringBuilder> GetPostfixFunc, GetPowerDisplayFunc, GetTankFillFunc;
            private MyStringId OnText, OffText;
            protected BvPropPool<BoolProperty> poolParent;
            private readonly StringBuilder statusBuilder;

            public BoolProperty()
            {
                GetPowerDisplayFunc = GetPowerDisplay;
                GetTankFillFunc = GetGasTankFillPercent;
                statusBuilder = new StringBuilder();
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<bool> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.boolPropPool;

                if (property.Id == "OnOff" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Powered)) // Insert power draw / output info
                    GetPostfixFunc = GetPowerDisplayFunc;
                else if (property.Id == "Stockpile" && block.SubtypeId.UsesSubtype(TBlockSubtypes.GasTank)) // Insert gas tank info
                    GetPostfixFunc = GetTankFillFunc;

                if (property is IMyTerminalControlOnOffSwitch)
                {
                    var onOffSwitch = property as IMyTerminalControlOnOffSwitch;

                    OnText = onOffSwitch.OnText;
                    OffText = onOffSwitch.OffText;
                }
                else
                {
                    OnText = MySpaceTexts.SwitchText_On;
                    OffText = MySpaceTexts.SwitchText_Off;
                }
            }

            public override void Reset()
            {
                base.Reset();
                GetPostfixFunc = null;
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static BoolProperty GetProperty(StringBuilder name, ITerminalProperty<bool> property, PropertyBlock block)
            {
                BoolProperty prop = block.boolPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            private void GetPowerDisplay(StringBuilder sb)
            {
                PowerAccessor power = block.Power;
                float? input = power.Input, output = power.Output;

                sb.Clear();
                sb.Append('(');
                PowerAccessor.GetPowerDisplay(input, output, sb);
                sb.Append(')');
            }

            private void GetGasTankFillPercent(StringBuilder sb)
            {
                sb.Clear();
                sb.Append('(');
                sb.Append(Math.Round(block.GasTank.FillRatio * 100d, 1));
                sb.Append("%)");
            }

            public void Action() =>
                SetValue(!GetValue());

            public override bool TryParseValue(string valueData, out bool value) =>
                bool.TryParse(valueData, out value);

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private StringBuilder GetPropStateText()
            {
                if (GetValue())
                    return MyTexts.Get(OnText);
                else
                    return MyTexts.Get(OffText);
            }
        }
    }
}