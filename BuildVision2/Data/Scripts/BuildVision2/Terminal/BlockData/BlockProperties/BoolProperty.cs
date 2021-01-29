using Sandbox.Game.Localization;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using VRage;
using VRageMath;
using VRage.Utils;
using RichHudFramework;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : BvTerminalProperty<ITerminalProperty<bool>, bool>, IBlockAction
        {
            public override string Display => GetPropStateText();
            public override string Status => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private Func<string> GetPostfixFunc, GetPowerDisplayFunc, GetTankFillFunc;
            private MyStringId OnText, OffText;
            protected BvPropPool<BoolProperty> poolParent;

            public BoolProperty()
            {
                GetPowerDisplayFunc = GetPowerDisplay;
                GetTankFillFunc = GetGasTankFillPercent;
            }

            public override void SetProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, PropertyBlock block)
            {
                base.SetProperty(name, property, control, block);

                if (poolParent == null)
                    poolParent = block.boolPropPool;

                if (property.Id == "OnOff" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Powered)) // Insert power draw / output info
                    GetPostfixFunc = GetPowerDisplayFunc;
                else if (property.Id == "Stockpile" && block.SubtypeId.UsesSubtype(TBlockSubtypes.GasTank)) // Insert gas tank info
                    GetPostfixFunc = GetTankFillFunc;

                if (property is IMyTerminalControlOnOffSwitch)
                {
                    IMyTerminalControlOnOffSwitch onOffSwitch = (IMyTerminalControlOnOffSwitch)property;

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

            public static BoolProperty GetProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, PropertyBlock block)
            {
                BoolProperty prop = block.boolPropPool.Get();
                prop.SetProperty(name, property, control, block);

                return prop;
            }

            private string GetPowerDisplay()
            {
                PowerAccessor power = block.Power;
                float? input = power.Input, output = power.Output;

                return $"({PowerAccessor.GetPowerDisplay(input, output)})";
            }

            private string GetGasTankFillPercent() =>
                $"({Math.Round(block.GasTank.FillRatio * 100d, 1)}%)";

            public void Action() =>
                SetValue(!GetValue());

            public override bool TryParseValue(string valueData, out bool value) =>
                bool.TryParse(valueData, out value);

            /// <summary>
            /// Retrieves the on/off state of given property of a given block as a string.
            /// </summary>
            private string GetPropStateText()
            {
                if (GetValue())
                    return MyTexts.Get(OnText).ToString();
                else
                    return MyTexts.Get(OffText).ToString();
            }
        }
    }
}