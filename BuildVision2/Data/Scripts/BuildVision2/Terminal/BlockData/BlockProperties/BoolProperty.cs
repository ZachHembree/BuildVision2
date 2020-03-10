using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace DarkHelmet.BuildVision2
{
    internal partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property of a Boolean
        /// </summary>
        private class BoolProperty : BvTerminalProperty<ITerminalProperty<bool>, bool>, IBlockAction
        {
            public override string Value => GetPropStateText();
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly Func<string> GetPostfixFunc;
            private readonly MyStringId OnText, OffText;

            public BoolProperty(string name, ITerminalProperty<bool> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                if (property.Id == "OnOff" && (block.ResourceSink != null || block is IMyPowerProducer)) // Insert power draw / output info
                {
                    MyDefinitionId definitionId = MyDefinitionId.FromContent(block.SlimBlock.GetObjectBuilder());
                    var sink = block.ResourceSink;
                    var producer = block as IMyPowerProducer;

                    GetPostfixFunc = () => GetBlockPowerInfo(sink, producer, definitionId);
                }
                else if (property.Id == "Stockpile" && block is IMyGasTank) // Insert gas tank info
                {
                    GetPostfixFunc = () => GetGasTankFillPercent((IMyGasTank)block);
                }

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

            private static string GetBlockPowerInfo(MyResourceSinkComponentBase sink, IMyPowerProducer producer, MyDefinitionId definitionId)
            {
                string disp = "", suffix;
                float powerDraw = sink != null ? sink.CurrentInputByType(definitionId) : 0f,
                    powerOut = producer != null ? producer.CurrentOutput : 0f,
                    total = (powerDraw + powerOut), scale;

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

                if (sink != null)
                    disp += "-" + Math.Round(powerDraw * scale, 1);

                if (producer != null)
                {
                    if (sink != null)
                        disp += " / ";

                    disp += "+" + Math.Round(powerOut * scale, 1);
                }

                return $"({disp} {suffix})";
            }

            private static string GetGasTankFillPercent(IMyGasTank gasTank)
            {
                return $"({Math.Round(gasTank.FilledRatio * 100d, 1)}%)";
            }

            public void Action()
            {
                SetValue(!GetValue());
            }

            public override PropertyData GetPropertyData() =>
                new PropertyData(PropName, ID, GetValue().ToString());

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