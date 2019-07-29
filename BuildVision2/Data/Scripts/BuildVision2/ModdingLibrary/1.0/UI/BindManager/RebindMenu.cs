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
        private static readonly List<IControl> blacklist;
        private const long inputWaitTime = 100;

        private readonly List<IControl> newControls;
        private readonly RebindHud menu;
        private readonly Utils.Stopwatch stopwatch;
        private readonly List<string> bodyText;
        private BindManager.Group group;
        private IBind bind;
        private bool open;

        static RebindMenu()
        {
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
            bodyText = new List<string>();
            menu = new RebindHud();

            menu.footer.LeftText = "<color=210,235,245>[Enter] = Accept";
            menu.footer.RightText = "<color=210,235,245>[Delete] = Remove";

            HudUtilities.SharedBinds["enter"].OnNewPress += Confirm;
            HudUtilities.SharedBinds["delete"].OnNewPress += RemoveLast;
            HudUtilities.SharedBinds["escape"].OnNewPress += Exit;
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
            Instance.menu.header.Text = $"<color=210,235,245>Bind Manager: {bind.Name}";
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
            string bindString = "New Bind: ";

            bodyText.Clear();
            bodyText.Add("<color=210,235,245>Press a key to add it to the bind.");
            bodyText.Add(HudUtilities.LineBreak);

            for (int n = 0; n < newControls.Count; n++)
            {
                if (n < newControls.Count - 1)
                    bindString += newControls[n].Name + " + ";
                else
                    bindString += newControls[n].Name;
            }

            bodyText.Add($"<color=210,235,245>{bindString}");

            if (group.DoesComboConflict(newControls, bind))
            {
                bodyText.Add(HudUtilities.LineBreak);
                bodyText.Add("<color=180,20,20>Warning: Current key combination");
                bodyText.Add("<color=180,20,20>conflicts with another bind.");
            }

            menu.body.ListText = bodyText;
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
        private class RebindHud : HudUtilities.ElementBase
        {
            public override bool Visible => Instance.open;
            public override double Scale => HudUtilities.ResScale;
            public override Vector2D UnscaledSize => chain.UnscaledSize;

            public readonly TextBox header;
            public readonly ListBox body;
            public readonly DoubleTextBox footer;
            private readonly TextBoxChain chain;

            public RebindHud()
            {
                header = new TextBox()
                { TextScale = .935, Padding = new Vector2D(48d, 18d), BgColor = new Color(41, 54, 62, 229) };

                body = new ListBox(10)
                { TextAlignment = TextAlignment.Center, TextScale = .85, Padding = new Vector2D(48d, 16d), BgColor = new Color(70, 78, 86, 204) };

                footer = new DoubleTextBox()
                { TextScale = .85, Padding = new Vector2D(48d, 8d), BgColor = new Color(41, 54, 62, 229) };

                chain = new TextBoxChain(new List<TextBoxBase>() { header, body, footer })
                { parent = this };
            }
        }
    }
}