using Sandbox.Game.Localization;
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
            public override string Value => GetPropStateText();
            public override string Status => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly Func<string> GetPostfixFunc;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, SuperBlock block) : base(name, property, control, block)
            {
                if (property.Id == "OnOff" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Powered)) // Insert power draw / output info
                    GetPostfixFunc = GetBlockPowerInfo;
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

            private string GetBlockPowerInfo()
            {
                string disp = "", suffix;
                float powerDraw = block.Power.Input,
                    powerOut = block.Power.Out,
                    total = MathHelper.Max(powerDraw, 0f) + MathHelper.Max(powerOut, 0f),
                    scale;

                if (total >= 1000f)
                {
                    scale = .001f;
                    suffix = "GW";
                }
                else if (total >= 1f)
                {
                    scale = 1f;
                    suffix = "MW";
                }
                else if (total >= .001f)
                {
                    scale = 1000f;
                    suffix = "KW";
                }
                else
                {
                    scale = 1000000f;
                    suffix = "W";
                }

                if (powerDraw >= 0f)
                    disp += "-" + Math.Round(powerDraw * scale, 1);

                if (powerOut >= 0f)
                {
                    if (powerDraw >= 0f)
                        disp += " / ";

                    disp += "+" + Math.Round(powerOut * scale, 1);
                }

                return $"({disp} {suffix})";
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