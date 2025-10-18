using RichHudFramework.UI;
using Sandbox.Definitions;
using Sandbox.Game.GameSystems.Conveyors;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyTextSurface = Sandbox.ModAPI.Ingame.IMyTextSurface;
using IMyTextSurfaceProvider = Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider;
using ContentType = VRage.Game.GUI.TextPanel.ContentType;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public TextSurfaceAccessor TextSurfaceProvider => _textSurfaceProvider;

		private TextSurfaceAccessor _textSurfaceProvider;

		public class TextSurfaceAccessor : SubtypeAccessor<IMyTextSurfaceProvider>
		{
			/// <summary>
			/// Returns a list of interfaces to LCD surfaces owned by the block
			/// </summary>
			public IReadOnlyList<TextSurface> Surfaces => surfaces;

			/// <summary>
			/// Returns true if the surface provider is a regular LCD block
			/// </summary>
			public bool IsTextPanel { get; private set; }

			private readonly List<TextSurface> surfaces;

			public TextSurfaceAccessor()
			{
				surfaces = new List<TextSurface>();
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.TextSurfaceProvider);

				if (subtype != null)
				{
					IsTextPanel = subtype is IMyTextPanel;

					for (int i = 0; i < subtype.SurfaceCount; i++)
					{
						var surface = subtype.GetSurface(i);

						if (surface != null)
							surfaces.Add(new TextSurface(surface, block.textBuffer));
					}
				}
			}

			public override void Reset()
			{
				base.Reset();
				surfaces.Clear();
				IsTextPanel = false;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{ }

			/// <summary>
			/// Wrapper around a IMyTextSurface attached to an IMyTextSurfaceProvider
			/// </summary>
			public struct TextSurface
			{
				/// <summary>
				/// Returns the localized name of the surface
				/// </summary>
				public string DisplayName => surface.DisplayName;

				/// <summary>
				/// Returns the name of the script/app if one is set
				/// </summary>
				public string ScriptName => surface.Script;

				/// <summary>
				/// LCD content type. None, text/image or script.
				/// </summary>
				public ContentType ContentType => surface.ContentType;

				/// <summary>
				/// Gets/sets text written on the LCD
				/// </summary>
				public StringBuilder Text
				{
					get
					{
						surface.ReadText(textBuffer);
						return textBuffer;
					}

					set { surface.WriteText(value); }
				}

				private readonly IMyTextSurface surface;
				private readonly StringBuilder textBuffer;

				public TextSurface(IMyTextSurface surface, StringBuilder textBuffer)
				{
					this.surface = surface;
					this.textBuffer = textBuffer;
				}

				/// <summary>
				/// Returns the localized name of the current content mode enum
				/// </summary>
				public string GetLocalizedContentTypeName()
				{
					switch (ContentType)
					{
						case ContentType.TEXT_AND_IMAGE:
							return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_TextAndImageContent);
						case ContentType.SCRIPT:
							return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_ScriptContent);
						default:
							return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_NoContent);
					}
				}
			}
		}
	}
}
