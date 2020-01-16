using RichHudFramework.Game;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Client
{
    /// <summary>
    /// GUI used to change binds in <see cref="BindManager.Group"/>s.
    /// </summary>
    internal sealed class RebindMenu : ModBase.ComponentBase
    {
        private static RebindMenu Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static RebindMenu instance;
        private static readonly GlyphFormat defaultFormat, errorFormat;
        private static readonly List<int> blacklist;
        private const long inputWaitTime = 100;

        private readonly List<int> controlIndices;
        private readonly List<IControl> newCombo;
        private readonly RebindHud menu;
        private readonly Utils.Stopwatch stopwatch;
        private IBindGroup group;
        private IBind bind;
        private bool open;

        static RebindMenu()
        {
            defaultFormat = new GlyphFormat(color: new Color(210, 235, 245));
            errorFormat = new GlyphFormat(color: new Color(180, 20, 20));

            blacklist = new List<int>
            {
                BindManager.GetControl("enter").Index,
                BindManager.GetControl("delete").Index,
                BindManager.GetControl("escape").Index,
            };
        }

        private RebindMenu() : base(false, true)
        {
            controlIndices = new List<int>();
            newCombo = new List<IControl>();
            stopwatch = new Utils.Stopwatch();
            menu = new RebindHud();

            menu.footer.LeftTextBoard.SetText(new RichString("[Enter] = Accept", defaultFormat));
            menu.footer.RightTextBoard.SetText(new RichString("[Delete] = Remove", defaultFormat));

            SharedBinds.Enter.OnNewPress += Confirm;
            SharedBinds.Delete.OnNewPress += RemoveLast;
            SharedBinds.Escape.OnNewPress += Exit;
        }

        private static void Init()
        {
            if (instance == null)
                instance = new RebindMenu();
        }

        public override void Close()
        {
            Exit();
            instance = null;
        }

        public static void UpdateBind(IBindGroup group, IBind bind)
        {
            Instance.stopwatch.Start();
            Instance.controlIndices.Clear();
            Instance.open = true;

            Instance.group = group;
            Instance.bind = bind;
            Instance.UpdateCombo();
            Instance.menu.header.Text = $"Bind Manager: {bind.Name}";
        }

        public static void UpdateBind(IBindGroup group, string bindName)
        {
            UpdateBind(group, group.GetBind(bindName));
        }

        public override void HandleInput()
        {
            if (stopwatch.Enabled && stopwatch.ElapsedMilliseconds > inputWaitTime)
                stopwatch.Stop();

            if ((stopwatch.ElapsedMilliseconds > inputWaitTime) && group != null && bind.Name != null)
            {
                for (int n = 0; (n < BindManager.Controls.Count && controlIndices.Count < 3); n++)
                {
                    if (BindManager.Controls[n].IsPressed)
                    {
                        if (!blacklist.Contains(BindManager.Controls[n].Index) && !controlIndices.Contains(BindManager.Controls[n].Index))
                        {
                            controlIndices.Add(BindManager.Controls[n].Index);
                            UpdateCombo();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the body text with the names of the currently selected controls and informs
        /// the user if the combination conflicts with any other binds in the same group.
        /// </summary>
        private void UpdateCombo()
        {
            RichText bindString = new RichText(defaultFormat);

            newCombo.Clear();
            newCombo.AddRange(BindManager.GetCombo(controlIndices));
            bindString += "New Bind: ";

            menu.body.Text = new RichText("Press a key to add it to the bind.\n", defaultFormat);

            for (int n = 0; n < newCombo.Count; n++)
            {
                if (n < newCombo.Count - 1)
                    bindString += newCombo[n].Name + " + ";
                else
                    bindString += newCombo[n].Name;
            }

            menu.body.TextBoard.Append(bindString);

            if (group.DoesComboConflict(newCombo, bind))
            {
                menu.body.TextBoard.Append(new RichText("\nWarning: Current key combination", errorFormat));
                menu.body.TextBoard.Append(new RichText("conflicts with another bind.", errorFormat));
            }
        }

        /// <summary>
        /// Removes the last entry in the new control list and updates the body text accordingly.
        /// If there are no controls in the list, it will close the menu instead.
        /// </summary>
        private void RemoveLast()
        {
            if (open)
            {
                if (controlIndices.Count > 0)
                {
                    controlIndices.RemoveAt(controlIndices.Count - 1);
                    UpdateCombo();
                }
                else
                    Exit();
            }
        }

        /// <summary>
        /// Clears the control list and closes the menu.
        /// </summary>
        private void Exit()
        {
            if (open)
            {
                controlIndices.Clear();
                newCombo.Clear();
                open = false;
            }
        }

        /// <summary>
        /// Attempts to update a given bind using its name and the names of the controls
        /// in the new combo and closes the menu.
        /// </summary>
        private void Confirm()
        {
            if (open && newCombo.Count > 0)
            {
                string[] controlNames = new string[newCombo.Count];

                for (int n = 0; n < newCombo.Count; n++)
                    controlNames[n] = newCombo[n].Name;

                group.GetBind(bind.Name).TrySetCombo(controlNames);
                Exit();
            }
        }

        /// <summary>
        /// Generates the UI for the <see cref="RebindMenu"/>
        /// </summary>
        private class RebindHud : HudElementBase
        {
            public override bool Visible => Instance.open;
            public override Vector2 Size => chain.Size;

            public readonly TextBox header, body;
            public readonly DoubleLabelBox footer;
            private readonly HudChain<HudElementBase> chain;

            public RebindHud() : base(HudMain.Root)
            {
                header = new TextBox();
                //{ TextScale = .935f, Padding = new Vector2(48f, 18f), Color = new Color(41, 54, 62, 229), Format = new GlyphFormat(color: new Color(210, 235, 245)) };

                body = new TextBox();
                //{ TextScale = .85f, Padding = new Vector2(48f, 16f), Color = new Color(70, 78, 86, 204) };

                footer = new DoubleLabelBox();
                //{ TextScale = .85f, Padding = new Vector2(48f, 8f), Color = new Color(41, 54, 62, 229) };

                chain = new HudChain<HudElementBase>(this)
                {
                    ChildContainer =
                    {
                        header,
                        body,
                        footer
                    }
                };
            }
        }
    }
}