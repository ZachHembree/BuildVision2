using System.Collections.Generic;
using System.Text;
using VRageMath;
using DarkHelmet.Game;

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
        private static List<IControl> blacklist;
        private const long inputWaitTime = 100;

        private List<IControl> newControls;
        private RebindHud menu;
        private bool open;
        private BindManager.Group group;
        private IKeyBind bind;
        private Utils.Stopwatch stopwatch;

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

        public static void UpdateBind(BindManager.Group group, IKeyBind bind)
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
                for (int n = 0; (n < BindManager.controls.Length && newControls.Count < BindManager.maxBindLength); n++)
                {
                    if (BindManager.controls[n].IsPressed)
                    {
                        if (!blacklist.Contains(BindManager.controls[n]) && !newControls.Contains(BindManager.controls[n]))
                        {
                            newControls.Add(BindManager.controls[n]);
                            UpdateBodyText();
                        }
                    }
                }
            }
        }

        private void UpdateBodyText()
        {
            List<string> bodyText = new List<string>(4);
            string bindString = "New Bind: ";

            bodyText.Add("<color=210,235,245>Press a key to add it to the bind.");
            bodyText.Add(TextList.LineBreak);

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
                bodyText.Add(TextList.LineBreak);
                bodyText.Add("<color=180,20,20>Warning: Current key combination");
                bodyText.Add("<color=180,20,20>conflicts with another bind.");
            }

            menu.body.ListText = bodyText;
        }

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

        private void Exit()
        {
            if (open)
            {
                newControls.Clear();
                open = false;
            }
        }

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
                { TextScale = .935, Padding = new Vector2D(48d, 14d), BgColor = new Color(41, 54, 62, 229) };

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