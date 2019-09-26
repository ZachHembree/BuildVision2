using DarkHelmet.Game;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI
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
        private static readonly List<IControl> blacklist;
        private const long inputWaitTime = 100;

        private readonly List<IControl> newControls;
        private readonly RebindHud menu;
        private readonly Utils.Stopwatch stopwatch;
        private BindManager.Group group;
        private IBind bind;
        private bool open;

        static RebindMenu()
        {
            defaultFormat = new GlyphFormat(color: new Color(210, 235, 245));
            errorFormat = new GlyphFormat(color: new Color(180, 20, 20));

            blacklist = new List<IControl>
            {
                BindManager.GetControlByName("enter"),
                BindManager.GetControlByName("delete"),
                BindManager.GetControlByName("escape"),
            };
        }

        private RebindMenu()
        {
            newControls = new List<IControl>();
            stopwatch = new Utils.Stopwatch();
            menu = new RebindHud();

            menu.footer.LeftText.SetText(new RichString("[Enter] = Accept", defaultFormat));
            menu.footer.RightText.SetText(new RichString("[Delete] = Remove", defaultFormat));

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

        public static void UpdateBind(BindManager.Group group, IBind bind)
        {
            Instance.stopwatch.Start();
            Instance.newControls.Clear();
            Instance.open = true;

            Instance.group = group;
            Instance.bind = bind;
            Instance.UpdateBodyText();
            Instance.menu.header.Text.SetText($"Bind Manager: {bind.Name}");
        }

        public static void UpdateBind(BindManager.Group group, string bindName)
        {
            UpdateBind(group, group[bindName]);
        }

        public override void HandleInput()
        {
            if (stopwatch.Enabled && stopwatch.ElapsedMilliseconds > inputWaitTime)
                stopwatch.Stop();

            if ((stopwatch.ElapsedMilliseconds > inputWaitTime) && group != null && bind.Name != null)
            {
                for (int n = 0; (n < BindManager.Controls.Count && newControls.Count < BindManager.maxBindLength); n++)
                {
                    if (BindManager.Controls[n].IsPressed)
                    {
                        if (!blacklist.Contains(BindManager.Controls[n]) && !newControls.Contains(BindManager.Controls[n]))
                        {
                            newControls.Add(BindManager.Controls[n]);
                            UpdateBodyText();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the body text with the names of the currently selected controls and informs
        /// the user if the combination conflicts with any other binds in the same group.
        /// </summary>
        private void UpdateBodyText()
        {
            RichText bindString = new RichText(defaultFormat);
            bindString += "New Bind: ";

            menu.body.Clear();
            menu.body.Add(new RichText("Press a key to add it to the bind.\n", defaultFormat));
            //bodyText.Add(HudMain.LineBreak);

            for (int n = 0; n < newControls.Count; n++)
            {
                if (n < newControls.Count - 1)
                    bindString += newControls[n].Name + " + ";
                else
                    bindString += newControls[n].Name;
            }

            menu.body.Add(bindString);

            if (group.DoesComboConflict(newControls, bind))
            {
                menu.body.Add(new RichText("\nWarning: Current key combination", errorFormat));
                menu.body.Add(new RichText("conflicts with another bind.", errorFormat));
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
                if (newControls.Count > 0)
                {
                    newControls.RemoveAt(newControls.Count - 1);
                    UpdateBodyText();
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
                newControls.Clear();
                open = false;
            }
        }

        /// <summary>
        /// Attempts to update a given bind using its name and the names of the controls
        /// in the new combo and closes the menu.
        /// </summary>
        private void Confirm()
        {
            if (open && newControls.Count > 0)
            {
                string[] controlNames = new string[newControls.Count];

                for (int n = 0; n < newControls.Count; n++)
                    controlNames[n] = newControls[n].Name;

                group.TryUpdateBind(bind.Name, controlNames);
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
            public override float Scale { get { return chain.Scale; } set { chain.Scale = value; } }

            public readonly TextBox header;
            public readonly ListBox body;
            public readonly DoubleTextBox footer;
            private readonly BoxChain chain;

            public RebindHud() : base(HudMain.Root)
            {
                header = new TextBox()
                { TextScale = .935f, Padding = new Vector2(48f, 18f), BgColor = new Color(41, 54, 62, 229), DefaultFormat = new GlyphFormat(color: new Color(210, 235, 245)) };

                body = new ListBox(10)
                { TextAlignment = TextAlignment.Center, TextScale = .85f, Padding = new Vector2(48f, 16f), BgColor = new Color(70, 78, 86, 204) };

                footer = new DoubleTextBox()
                { TextScale = .85f, Padding = new Vector2(48f, 8f), BgColor = new Color(41, 54, 62, 229) };

                chain = new BoxChain(new List<ResizableElementBase>() { header, body, footer }, this);
            }
        }
    }
}