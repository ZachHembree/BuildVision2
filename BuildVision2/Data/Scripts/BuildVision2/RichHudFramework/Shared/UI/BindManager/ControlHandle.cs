using System;
using VRage.Input;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Server;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// A unified handle for referencing UI controls. 
		/// <para>Abstracts the differences between <see cref="MyKeys"/> (Keyboard), <see cref="RichHudControls"/> (Mouse/Custom), 
		/// and <see cref="MyJoystickButtonsEnum"/> (Gamepad) by wrapping them in a unique integer ID.</para>
		/// </summary>
		public struct ControlHandle
		{
			/// <summary>
			/// The integer index offset where gamepad keys begin.
			/// </summary>
			public const int GPKeysStart = (int)RichHudControls.ReservedEnd + 1;

			/// <summary>
			/// Retrieves the interface to the underlying <see cref="IControl"/> object via the <see cref="BindManager"/>.
			/// </summary>
			public IControl Control => BindManager.GetControl(this);

			/// <summary>
			/// Returns the <see cref="RichHudControls"/> enum corresponding to this handle.
			/// </summary>
			public RichHudControls ControlEnum => (RichHudControls)id;

			/// <summary>
			/// The unique internal integer ID for this control.
			/// </summary>
			public readonly int id;

			/// <summary>
			/// Creates a handle from a string name (looks up ID via BindManager).
			/// </summary>
			public ControlHandle(string controlName)
			{
				this.id = BindManager.GetControl(controlName);
			}

			public ControlHandle(int id)
			{
				this.id = id;
			}

			public ControlHandle(MyKeys id)
			{
				this.id = (int)id;
			}

			public ControlHandle(IControl con)
			{
				this.id = con.Index;
			}

			public ControlHandle(RichHudControls id)
			{
				this.id = (int)id;
			}

			public ControlHandle(MyJoystickButtonsEnum id)
			{
				this.id = GPKeysStart + (int)id;
			}

			public static explicit operator ControlHandle(int con)
			{
				return new ControlHandle(con);
			}

			public static implicit operator ControlHandle(string controlName)
			{
				return new ControlHandle(controlName);
			}

			public static implicit operator ControlHandle(MyKeys id)
			{
				return new ControlHandle(id);
			}

			/// <summary>
			/// Implicit conversion to MyKeys. Throws exception if the handle does not represent a valid MyKey.
			/// </summary>
			public static implicit operator MyKeys(ControlHandle handle)
			{
				var id = (MyKeys)handle.id;

				if (Enum.IsDefined(typeof(MyKeys), id))
					return id;
				else
				{
					throw new Exception($"ControlHandle index {handle.id} cannot be converted to MyKeys.");
				}
			}

			public static implicit operator ControlHandle(RichHudControls id)
			{
				return new ControlHandle(id);
			}

			public static implicit operator RichHudControls(ControlHandle handle)
			{
				var id = (RichHudControls)handle.id;

				if (Enum.IsDefined(typeof(RichHudControls), id))
					return id;
				else
				{
					throw new Exception($"ControlHandle index {handle.id} cannot be converted to RichHudControls.");
				}
			}

			public static implicit operator ControlHandle(MyJoystickButtonsEnum id)
			{
				return new ControlHandle(id);
			}

			public static implicit operator MyJoystickButtonsEnum(ControlHandle handle)
			{
				var id = (MyJoystickButtonsEnum)handle.id;

				if (Enum.IsDefined(typeof(MyJoystickButtonsEnum), id))
					return id;
				else
				{
					throw new Exception($"ControlHandle index {handle.id} cannot be converted to MyJoystickButtonsEnum.");
				}
			}

			public static implicit operator int(ControlHandle handle)
			{
				return handle.id;
			}

			public override int GetHashCode()
			{
				return id.GetHashCode();
			}
		}
	}
}