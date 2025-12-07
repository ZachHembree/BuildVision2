namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// Internal API member accessor indices
	/// </summary>
	/// <exclude/>
	public enum SliderSettingsAccessors : int
	{
		/// <summary>
		/// Float
		/// </summary>
		Min = 16,

		/// <summary>
		/// Float
		/// </summary>
		Max = 17,

		/// <summary>
		/// Float
		/// </summary>
		Percent = 18,

		/// <summary>
		/// RichStringMembers[]
		/// </summary>
		ValueText = 19,
	}

	/// <summary>
	/// A labeled slider for selecting float values within a specific range. For <see cref="ControlTile"/>s.
	/// <para>Mimics the appearance of the slider in the SE terminal.</para>
	/// </summary>
	public class TerminalSlider : TerminalValue<float>
	{
		/// <summary>
		/// The minimum allowable value for the slider.
		/// </summary>
		public float Min
		{
			get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Min); }
			set { GetOrSetMember(value, (int)SliderSettingsAccessors.Min); }
		}

		/// <summary>
		/// The maximum allowable value for the slider.
		/// </summary>
		public float Max
		{
			get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Max); }
			set { GetOrSetMember(value, (int)SliderSettingsAccessors.Max); }
		}

		/// <summary>
		/// The current slider value expressed as a normalized percentage (0.0 to 1.0) between Min and Max.
		/// </summary>
		public float Percent
		{
			get { return (float)GetOrSetMember(null, (int)SliderSettingsAccessors.Percent); }
			set { GetOrSetMember(value, (int)SliderSettingsAccessors.Percent); }
		}

		/// <summary>
		/// Custom text suffix or label indicating the current value. 
		/// <para>Note: This must be updated manually if dynamic text reflecting the value is desired.</para>
		/// </summary>
		public string ValueText
		{
			get { return GetOrSetMember(null, (int)SliderSettingsAccessors.ValueText) as string; }
			set { GetOrSetMember(value, (int)SliderSettingsAccessors.ValueText); }
		}

		public TerminalSlider() : base(MenuControls.SliderSetting)
		{ }
	}
}