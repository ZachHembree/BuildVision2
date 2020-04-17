using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class GasTankData
        {
            /// <summary>
            /// Returns the capacity of the tank
            /// </summary>
            public double Capacity => gasTank.Capacity;

            /// <summary>
            /// Returns the fill ratio of the tank
            /// </summary>
            public double FillRatio => gasTank.FilledRatio;

            private readonly IMyGasTank gasTank;

            public GasTankData(IMyTerminalBlock tBlock)
            {
                gasTank = tBlock as IMyGasTank;
            }
        }
    }
}