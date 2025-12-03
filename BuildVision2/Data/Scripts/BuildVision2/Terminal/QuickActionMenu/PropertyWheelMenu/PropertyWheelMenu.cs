using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
	public sealed partial class QuickActionMenu : HudElementBase
	{
		private sealed partial class PropertyWheelMenu : HudElementBase
		{
			/// <summary>
			/// Parent of the wheel menu
			/// </summary>
			public readonly QuickActionMenu quickActionMenu;

			/// <summary>
			/// Gets/sets the menu's state
			/// </summary>
			private QuickActionMenuState MenuState
			{
				get { return quickActionMenu.MenuState; }
				set { quickActionMenu.MenuState = value; }
			}

			/// <summary>
			/// Returns the current target block
			/// </summary>
			private IPropertyBlock Target => quickActionMenu.Target;

			/// <summary>
			/// Returns true if the menu is open and visible
			/// </summary>
			public bool IsOpen { get; set; }

			/// <summary>
			/// Returns true if a property widget is currently open
			/// </summary>
			public bool IsWidgetOpen => wheelBody.IsWidgetOpen;

			/// <summary>
			/// Normalized inner diameter of the wheel, [0, 1]
			/// </summary>
			public float InnerDiam => propertyWheel.InnerRadius;

			/// <summary>
			/// Returns true if the menu is closing
			/// </summary>
			public bool IsHiding { get; private set; }

			private readonly RadialSelectionBox<PropertyWheelEntryBase, Label> propertyWheel, dupeWheel;
			private readonly PropertyWheelMenuBody wheelBody;
			private readonly Label debugText;

			private readonly ObjectPool<object> propertyEntryPool;
			private readonly List<PropertyWheelShortcutEntry> shortcutEntries;
			private RadialSelectionBox<PropertyWheelEntryBase, Label> activeWheel;
			private int textUpdateTick;

			public PropertyWheelMenu(QuickActionMenu parent) : base(parent)
			{
				this.quickActionMenu = parent;
				wheelBody = new PropertyWheelMenuBody(this) { };

				// Selection wheel for block properties
				propertyWheel = new RadialSelectionBox<PropertyWheelEntryBase, Label>(wheelBody)
				{
					Visible = false,
					BackgroundColor = BodyColor,
					HighlightColor = HighlightColor,
					SelectionColor = HighlightFocusColor,
					Size = new Vector2(WheelOuterDiam),
					ZOffset = -1,
				};
				propertyWheel.InnerRadius = WheelInnerDiamScale;

				// Selection wheel for dupe shortcuts
				dupeWheel = new RadialSelectionBox<PropertyWheelEntryBase, Label>(wheelBody)
				{
					Visible = false,
					BackgroundColor = BodyColor,
					HighlightColor = HighlightColor,
					SelectionColor = HighlightFocusColor,
					ZOffset = -1,
					CollectionContainer =
					{
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Back"),
							ShortcutAction = quickActionMenu.StopPropertyDuplication,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Open List"),
							ShortcutAction = quickActionMenu.OpenDupeList,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Open List and Select All"),
							ShortcutAction = quickActionMenu.OpenDupeListAndSelectAll,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Clear Selection"),
							ShortcutAction = quickActionMenu.ClearSelection,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Copy Selected"),
							ShortcutAction = quickActionMenu.CopySelectedProperties,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Copy All but Name"),
							ShortcutAction = () => quickActionMenu.CopyAllProperties(false),
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Copy All"),
							ShortcutAction = () => quickActionMenu.CopyAllProperties(true),
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Paste"),
							ShortcutAction = quickActionMenu.PasteCopiedProperties,
						},
						new PropertyWheelShortcutEntry()
						{
							Text = MyTexts.TrySubstitute("Undo"),
							ShortcutAction = quickActionMenu.UndoPropertyPaste,
						},
					}
				};

				// Shortcuts to be added to the property wheel later
				shortcutEntries = new List<PropertyWheelShortcutEntry>()
				{
					new PropertyWheelShortcutEntry()
					{
						Text = MyTexts.TrySubstitute("Copy Settings"),
						ShortcutAction = () =>
						{
							MenuState = QuickActionMenuState.WheelMenuControl;
							quickActionMenu.StartPropertyDuplication();
						},
					}
				};

				// I'm using a generic pool because I'm using two types of entry in the same list, but only
				// one is pooled, and I can't be arsed to do this more neatly.
				propertyEntryPool = new ObjectPool<object>(
					() => new PropertyWheelEntry(),
					x => (x as PropertyWheelEntry).Reset()
				);

				debugText = new Label(this)
				{
					Visible = false,
					BuilderMode = TextBuilderModes.Lined,
					ParentAlignment = ParentAlignments.Right
				};
			}

			/// <summary>
			/// Adds a permanent shortcut to the property wheel
			/// </summary>
			public void RegisterShortcut(PropertyWheelShortcutEntry shortcut)
			{
				shortcutEntries.Add(shortcut);
			}

			/// <summary>
			/// Opens the menu and populates it with properties from the given property block
			/// </summary>
			public void OpenMenu()
			{
				if (!IsOpen)
				{
					Clear();

					// Add entries for block members
					for (int i = 0; i < Target.BlockMembers.Count; i++)
					{
						var entry = propertyEntryPool.Get() as PropertyWheelEntry;
						entry.SetMember(i, Target);
						propertyWheel.Add(entry);
					}

					// Update shortcut font
					IFontMin cfgFont = FontManager.GetFont(BvConfig.Current.genUI.fontName);

					if (cfgFont != null)
					{
						foreach (PropertyWheelShortcutEntry entry in shortcutEntries)
						{
							entry.TextBoard.SetFormatting(entry.TextBoard.Format.WithFont(cfgFont));
						}

						foreach (PropertyWheelShortcutEntry entry in dupeWheel)
						{
							entry.TextBoard.SetFormatting(entry.TextBoard.Format.WithFont(cfgFont));
						}
					}

					// Append registered shortcuts to end
					propertyWheel.AddRange(shortcutEntries);
					propertyWheel.SetHighlightAt(0);
					dupeWheel.SetHighlightAt(0);
				}

				propertyWheel.InputEnabled = true;
				propertyWheel.Visible = true;
				dupeWheel.Visible = false;
				IsOpen = true;
				Visible = true;
				IsHiding = false;
			}

			public void OpenSummary()
			{
				propertyWheel.Visible = false;
				dupeWheel.Visible = false;
				Visible = true;
				IsOpen = false;
				IsHiding = false;
			}

			public void HideMenu()
			{
				IsHiding = true;
				propertyWheel.Visible = false;
				dupeWheel.Visible = false;
			}

			public void CloseMenu()
			{
				Clear();
				HideMenu();
				IsOpen = false;
			}

			public void ShowNotification(StringBuilder text, bool continuous) =>
				wheelBody.ShowNotification(text, continuous);

			/// <summary>
			/// Clears all entries from the menu and closes any open widgets
			/// </summary>
			private void Clear()
			{
				wheelBody.CloseWidget();

				propertyEntryPool.ReturnRange(propertyWheel.EntryList, 0,
					propertyWheel.EntryList.Count - shortcutEntries.Count);

				dupeWheel.ClearSelection();
				propertyWheel.Clear();
			}

			protected override void Layout()
			{
				float opacity = BvConfig.Current.genUI.hudOpacity;
				wheelBody.background.Color = wheelBody.background.Color.SetAlphaPct(opacity);
				propertyWheel.BackgroundColor = propertyWheel.BackgroundColor.SetAlphaPct(opacity);
				dupeWheel.BackgroundColor = dupeWheel.BackgroundColor.SetAlphaPct(opacity);

				if (IsHiding && wheelBody.AnimPos < 0.01f)
				{
					Visible = false;
					IsHiding = false;
				}

				if (MenuState == QuickActionMenuState.WheelPeek)
				{
					Size = wheelBody.Size;
				}
				else
				{
					Size = propertyWheel.Size;

					if (activeWheel != null)
						activeWheel.CursorSensitivity = BvConfig.Current.genUI.cursorSensitivity;

					if (textUpdateTick == 0 && (MenuState & QuickActionMenuState.PropertyDuplication) == 0)
					{
						foreach (PropertyWheelEntryBase baseEntry in propertyWheel)
						{
							var entry = baseEntry as PropertyWheelEntry;

							if (entry != null && entry.Enabled)
								entry.UpdateText(entry == propertyWheel.Selection
									&& (MenuState & QuickActionMenuState.WidgetControl) == QuickActionMenuState.WidgetControl);
						}
					}
				}

				if (textUpdateTick == 0)
				{
					debugText.Visible = DrawDebug;

					if (DrawDebug)
						UpdateDebugText();
				}

				textUpdateTick++;
				textUpdateTick %= TextTickDivider;
			}

			private void UpdateDebugText()
			{
				PropertyWheelEntryBase selection = activeWheel?.Selection;

				if (selection != null)
				{
					var propertyEntry = selection as PropertyWheelEntry;
					ITextBuilder textBuilder = debugText.TextBoard;
					textBuilder.Clear();

					textBuilder.Append($"Prioritized Members: {Target.Prioritizer.PrioritizedMemberCount}\n");
					textBuilder.Append($"Enabled Members: {Target.EnabledMemberCount}\n");
					textBuilder.Append($"Selection: {selection.Element.TextBoard}\n");

					if (propertyEntry != null)
					{
						textBuilder.Append($"ID: {propertyEntry.BlockMember.PropName}\n");
						textBuilder.Append($"Type: {propertyEntry.BlockMember.GetType().Name}\n");
						textBuilder.Append($"Entry Enabled: {propertyEntry.Enabled}\n");
						textBuilder.Append($"Prop Enabled: {propertyEntry.BlockMember.Enabled}\n");
						textBuilder.Append($"Value Text: {propertyEntry.BlockMember.ValueText}\n");
						textBuilder.Append($"Is Duplicating: {propertyEntry.IsSelectedForDuplication}\n");
					}
				}
			}
		}
	}
}