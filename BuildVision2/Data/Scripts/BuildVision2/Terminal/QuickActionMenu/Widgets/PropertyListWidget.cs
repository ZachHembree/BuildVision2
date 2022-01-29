using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private sealed class PropertyListWidget : QuickActionWidgetBase
        {
            private readonly ScrollBox<PropertyWidgetEntry, Label> propertyScrollBox;
            private readonly ObjectPool<PropertyWidgetEntry> entryPool;

            public PropertyListWidget(HudParentBase parent = null) : base(parent)
            {
                entryPool = new ObjectPool<PropertyWidgetEntry>(() => new PropertyWidgetEntry(), x => x.Reset());
                propertyScrollBox = new ScrollBox<PropertyWidgetEntry, Label>(true) 
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Height = 170f
                };

                propertyScrollBox.Background.Visible = false;
                propertyScrollBox.ScrollBar.Padding = new Vector2(12f, 16f);
                propertyScrollBox.ScrollBar.Width = 4f;

                var layout = new HudChain(true, this)
                {
                    Padding = new Vector2(20f),
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        propertyScrollBox,
                        buttonChain
                    }
                };
            }

            public override void SetData(object blockMembers, Action CloseWidgetCallback)
            {
                this.CloseWidgetCallback = CloseWidgetCallback;

                foreach (IBlockMember member in blockMembers as IReadOnlyList<IBlockMember>)
                {
                    if (member is IBlockProperty)
                    {
                        var entry = entryPool.Get();
                        entry.SetMember(member);
                        entry.UpdateText();

                        propertyScrollBox.Add(entry);
                    }
                }
            }

            public override void Reset()
            {
                entryPool.ReturnRange(propertyScrollBox.Collection, 0, propertyScrollBox.Collection.Count);
                propertyScrollBox.Clear();
            }

            protected override void Cancel()
            {
                CloseWidgetCallback();
            }

            protected override void Confirm()
            {
                CloseWidgetCallback();
            }
        }

        private sealed class PropertyWidgetEntry : ScrollBoxEntry<Label>
        {
            /// <summary>
            /// Text rendered by the label.
            /// </summary>
            public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

            /// <summary>
            /// TextBoard backing the label element.
            /// </summary>
            public ITextBoard TextBoard => Element.TextBoard;

            /// <summary>
            /// Returns true if the entry is enabled and valid
            /// </summary>
            public override bool Enabled
            {
                get { return base.Enabled && BlockMember != null && BlockMember.Enabled; }
            }

            /// <summary>
            /// Flag used to indicate entries selected for duplication
            /// </summary>
            public bool IsSelectedForCopy { get; set; }

            /// <summary>
            /// Returns associated property member wrapper
            /// </summary>
            public IBlockMember BlockMember { get; private set; }

            private readonly RichText textBuf;

            public PropertyWidgetEntry()
            {
                textBuf = new RichText();
                SetElement(new Label()
                {
                    VertCenterText = true,
                    AutoResize = false,
                    Height = 20f,
                    Format = valueText,
                    BuilderMode = TextBuilderModes.Unlined,
                });
            }

            /// <summary>
            /// Sets block property member
            /// </summary>
            public void SetMember(IBlockMember blockMember)
            {
                BlockMember = blockMember;
            }

            /// <summary>
            /// Updates associated text label for the entry in the menu
            /// </summary>
            public void UpdateText()
            {
                StringBuilder name = BlockMember.Name,
                    disp = BlockMember.FormattedValue,
                    status = BlockMember.StatusText;

                textBuf.Clear();

                if (name != null)
                {
                    textBuf.Add(name, bodyText);

                    if (disp != null || status != null)
                        textBuf.Add(":", bodyText);
                }

                if (disp != null)
                {
                    textBuf.Add(' ', valueText);
                    textBuf.Add(disp, valueText);
                }

                Element.Text = textBuf;
            }

            public void Reset()
            {
                IsSelectedForCopy = false;
                BlockMember = null;
            }
        }
    }
}