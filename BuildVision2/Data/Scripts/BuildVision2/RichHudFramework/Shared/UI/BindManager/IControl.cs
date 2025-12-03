namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

		/// <summary>
		/// Interface for controls used by the <see cref="BindManager"/>
		/// </summary> 
		public interface IControl
        {
            /// <summary>
            /// Name of the control
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Name of the control as displayed in bind menu
            /// </summary>
            string DisplayName { get; }

            /// <summary>
            /// Index of the control in the <see cref="BindManager"/>
            /// </summary>
            int Index { get; }

            /// <summary>
            /// Returns true if the control is being pressed
            /// </summary>
            bool IsPressed { get; }

            /// <summary>
            /// Returns true if the control was just pressed
            /// </summary>
            bool IsNewPressed { get; }

            /// <summary>
            /// Returns true if the control was just released
            /// </summary>
            bool IsReleased { get; }

            /// <summary>
            /// Returns true if the control doesn't represent a boolean value.
            /// </summary>
            bool Analog { get; }

            /// <summary>
            /// Returns analog value of the control, if it has one
            /// </summary>
            float AnalogValue { get; }
        }

        /// <summary>
        /// Internal API member accessor enums
        /// </summary>
        /// <exclude/>
        public enum ControlAccessors : int
        {
            /// <summary>
            /// out: string
            /// </summary>
            Name = 1,

            /// <summary>
            /// out: string
            /// </summary>
            DisplayName = 2,

            /// <summary>
            /// out: int
            /// </summary>
            Index = 3,

            /// <summary>
            /// out: bool
            /// </summary>
            IsPressed = 4,

            /// <summary>
            /// out: bool
            /// </summary>
            Analog = 5,

            /// <summary>
            /// out: bool
            /// </summary>
            IsNewPressed = 6,

            /// <summary>
            /// out: bool
            /// </summary>
            IsReleased = 7,

            /// <summary>
            /// out: float
            /// </summary>
            AnalogValue = 8
        }
    }
}