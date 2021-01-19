﻿using Sandbox.Game.Localization;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using VRage;
using VRageMath;
using VRage.Utils;

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

            private readonly Func<string> GetPostfixFunc;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, SuperBlock block) : base(name, property, control, block)
            {
                if (property.Id == "OnOff" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Powered)) // Insert power draw / output info
                {
                    GetPostfixFunc = () => 
                    {
                        PowerAccessor power = block.Power;
                        float? input = power.Input, output = power.Output;
                        return $"({PowerAccessor.GetPowerDisplay(input, output)})"; 
                    };
                }
                else if (property.Id == "Stockpile" && block.SubtypeId.UsesSubtype(TBlockSubtypes.GasTank)) // Insert gas tank info
                    GetPostfixFunc = GetGasTankFillPercent;

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