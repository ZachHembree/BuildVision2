using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	namespace UI
	{
		/// <summary>
		/// Internal terminal page member data enums
		/// </summary>
		/// <exclude/>
		public enum TerminalPageAccessors : int
		{
			/// <summary>
			/// string
			/// </summary>
			Name = 1,

			/// <summary>
			/// bool
			/// </summary>
			Enabled = 2,
		}

		/// <summary>
		/// Internal interface for RHF terminal pages. Shared with client and master modules.
		/// </summary>
		/// <exclude/>
		public interface ITerminalPage : IModRootMember
		{
			/// <summary>
			/// Retrieves information used by the Framework API
			/// </summary>
			ControlMembers GetApiData();
		}
	}
}