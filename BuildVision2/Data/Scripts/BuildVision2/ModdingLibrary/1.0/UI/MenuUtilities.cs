using DarkHelmet.Game;
using DarkHelmet.UI.TextHudApi;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;
using MenuFlag = DarkHelmet.UI.TextHudApi.HudAPIv2.MenuRootCategory.MenuFlag;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Collection of wrapper types and utilities used to simplify the creation of settings menu elements in the Text HUD API Mod Menu
    /// </summary>
    public sealed class MenuUtilities : ModBase.ComponentBase
    {
        public static bool CanAddElements => !wasClosed;

        private static MenuUtilities Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }

        private static MenuUtilities instance = null;
        private static bool wasClosed = false;
        private static readonly List<Action> menuUpdateActions;

        static MenuUtilities()
        {
            menuUpdateActions = new List<Action>();
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new MenuUtilities();

                if (!wasClosed)
                    MenuRoot.Init(ModBase.ModName, $"{ModBase.ModName} Settings");
            }
        }

        public override void Close()
        {
            Instance = null;
            wasClosed = true;
        }

        public override void Update()
        {
            if (HudAPIv2.Heartbeat && MyAPIGateway.Gui.ChatEntryVisible)
            {
                for (int n = 0; n < menuUpdateActions.Count; n++)
                    menuUpdateActions[n]();
            }
        }

        public static void AddMenuElement(IMenuElement newChild)
        {
            if (!wasClosed)
                MenuRoot.Instance.AddChild(newChild);
        }

        public static void AddMenuElements(IList<IMenuElement> newChildren)
        {
            if (!wasClosed)
                MenuRoot.Instance.AddChildren(newChildren);
        }

        /// <summary>
        /// Interface of all menu elements capable of serving as parents of other elements.
        /// </summary>
        public interface IMenuCategory
        {
            string Header { get; }
            HudAPIv2.MenuCategoryBase CategoryBase { get; }
            bool IsRoot { get; }
        }

        /// <summary>
        /// Interface for all menu elements based on HudAPIv2.MenuItemBase
        /// </summary>
        public interface IMenuElement
        {
            string Name { get; }
            IMenuCategory Parent { get; set; }
            void InitElement();
        }

        /// <summary>
        /// Base class for all wrapper types that instantiate HudAPIv2.MenuItemBase mod menu elements
        /// </summary>
        public abstract class MenuElement<T> : IMenuElement where T : HudAPIv2.MenuItemBase
        {
            public virtual IMenuCategory Parent
            {
                get { return parent; }
                set
                {
                    parent = value;

                    if (Element != null && parent != null && parent.CategoryBase != null)
                        Element.Parent = parent.CategoryBase;
                }
            }
            private IMenuCategory parent;

            public virtual string Name
            {
                get { return GetName(); }
                set
                {
                    GetName = () => value;

                    if (Element != null)
                        Element.Text = GetName();
                }
            }

            public virtual Func<string> GetName { get; set; }
            public virtual T Element { get; protected set; }

            public MenuElement(Func<string> GetName, IMenuCategory parent = null)
            {
                Init();
                this.GetName = GetName;
                Parent = parent;
                menuUpdateActions.Add(Update);
            }

            public MenuElement(string name, IMenuCategory parent = null) : this(() => name, parent)
            { }

            /// <summary>
            /// Used to instantiate HudAPIv2.MenuItemBase elements upon initialization of the Text Hud API
            /// </summary>
            public abstract void InitElement();

            /// <summary>
            /// Used to continuously update menu elements
            /// </summary>
            protected virtual void Update()
            {
                if (Element != null)
                    Element.Text = Name;
            }
        }

        /// <summary>
        /// Base class for all menu elements capable of containing other menu elements.
        /// </summary>
        public abstract class MenuCategoryBase<T> : MenuElement<T>, IMenuCategory where T : HudAPIv2.MenuCategoryBase
        {
            public virtual string Header
            {
                get { return header; }
                set
                {
                    header = value;

                    if (Element != null)
                        Element.Header = header;
                }
            }

            public virtual HudAPIv2.MenuCategoryBase CategoryBase { get { return Element as HudAPIv2.MenuCategoryBase; } }
            public virtual bool IsRoot { get; protected set; }

            protected string header;
            protected Queue<IMenuElement> children;

            public MenuCategoryBase(Func<string> GetName, string header, IMenuCategory parent = null, IList<IMenuElement> children = null, bool isRoot = false) : base(GetName, parent)
            {
                this.Header = header;
                this.IsRoot = isRoot;
                this.children = new Queue<IMenuElement>();

                if (children != null)
                    AddChildren(children);                    
            }

            public MenuCategoryBase(string name, string header, IMenuCategory parent = null, IList<IMenuElement> children = null, bool isRoot = false) : this(() => name, header, parent, children, isRoot)
            { }

            public virtual void AddChild(IMenuElement child)
            {
                child.Parent = this;
                children.Enqueue(child);
            }

            public virtual void AddChildren(IList<IMenuElement> newChildren)
            {
                for (int n = 0; n < newChildren.Count; n++)
                {
                    newChildren[n].Parent = this;
                    children.Enqueue(newChildren[n]);
                }
            }

            protected override void Update()
            {
                if (Element != null)
                {
                    IMenuElement child;

                    while (children.Count > 0)
                    {
                        if (children.TryDequeue(out child))
                            child.InitElement();
                    }
                }

                base.Update();
            }
        }

        /// <summary>
        /// Contains all settings menu elements for a given mod; singleton. Must be initalized before any other menu elements.
        /// </summary>
        private sealed class MenuRoot : MenuCategoryBase<HudAPIv2.MenuRootCategory>
        {
            public static MenuRoot Instance { get; private set; }

            /// <summary>
            /// This does nothing; it's only here because I couldn't be bothered to remove it from this type's base classes.
            /// </summary>
            public override IMenuCategory Parent { get { return this; } set { } }

            private MenuRoot(string name, string header) : base(name, header, null, null, true)
            { }

            public static void Init(string name, string header)
            {
                if (Instance == null)
                    Instance = new MenuRoot(name, header);
            }

            public override void InitElement() =>
                Element = new HudAPIv2.MenuRootCategory(Name, MenuFlag.PlayerMenu, Header);

            protected override void Update()
            {
                if (Element == null)
                    InitElement();

                base.Update();
            }
        }

        /// <summary>
        /// Collapsable submenu that can contain other elements, including other submenus
        /// </summary>
        public class MenuCategory : MenuCategoryBase<HudAPIv2.MenuSubCategory>
        {
            public MenuCategory(Func<string> GetName, string header, List<IMenuElement> children = null, IMenuCategory parent = null) : base(GetName, header, parent, children)
            { }

            public MenuCategory(string name, string header, List<IMenuElement> children = null, IMenuCategory parent = null) : base(name, header, parent, children)
            { }

            public override void InitElement() =>
                Element = new HudAPIv2.MenuSubCategory(Name, Parent.CategoryBase, Header);
        }

        /// <summary>
        /// Wrapper base for HudAPIv2.MenuItem based controls (buttons, sliders, text boxes, etc.)
        /// </summary>
        public abstract class MenuSetting<T> : MenuElement<T> where T : HudAPIv2.MenuItemBase
        {
            public MenuSetting(Func<string> GetName, IMenuCategory parent = null) : base(GetName, parent)
            {
                if (typeof(T) == typeof(HudAPIv2.MenuCategoryBase))
                    throw new Exception("Types of HudAPIv2.MenuCategoryBase cannot be used to create MenuSettings.");
            }

            public MenuSetting(string name, IMenuCategory parent = null) : this(() => name, parent)
            { }
        }

        /// <summary>
        /// Creates a draggable box that the user can use to indicate a position on the screen.
        /// </summary>
        public class MenuPositionInput : MenuSetting<HudAPIv2.MenuScreenInput>
        {
            private readonly Action<Vector2D> SetPosition;
            private readonly Func<Vector2D> GetPosition, GetSize;
            private readonly string queryText;
            private Vector2D startPos;

            public MenuPositionInput(Func<string> GetName, string queryText, Func<Vector2D> GetPosition, Action<Vector2D> SetPosition, Func<Vector2D> GetSize = null, IMenuCategory parent = null)
                : base(GetName, parent)
            {
                this.queryText = queryText;
                this.GetPosition = GetPosition;
                this.GetSize = GetSize;
                this.SetPosition = SetPosition;

                if (this.GetSize == null)
                    this.GetSize = () => Vector2D.Zero;
            }

            public MenuPositionInput(string name, string queryText, Func<Vector2D> GetPosition, Action<Vector2D> SetPosition, Func<Vector2D> GetSize = null, IMenuCategory parent = null)
                : this(() => name, queryText, GetPosition, SetPosition, GetSize, parent) { }

            private void OnSelect()
            {
                startPos = GetPosition();
                Element.Size = GetSize();
                Element.Origin = startPos - Element.Size / 2d;
            }

            private void UpdatePos(Vector2D pos)
            {
                Element.Size = GetSize();
                SetPosition(pos + Element.Size / 2d);
            }

            private void OnCancel()
            {
                SetPosition(startPos);
            }

            public override void InitElement() =>
                Element = new HudAPIv2.MenuScreenInput(Name, Parent.CategoryBase, Vector2D.Zero, Vector2D.Zero, queryText, UpdatePos, UpdatePos, OnCancel, OnSelect);
        }

        /// <summary>
        /// Creates a clickable menu button
        /// </summary>
        public class MenuButton : MenuSetting<HudAPIv2.MenuItem>
        {
            private readonly Action OnClickAction;

            public MenuButton(Func<string> GetName, Action OnClick, IMenuCategory parent = null) : base(GetName, parent)
            {
                OnClickAction = OnClick;
            }

            public MenuButton(string name, Action OnClick, IMenuCategory parent = null) : base(name, parent)
            {
                OnClickAction = OnClick;
            }

            private void OnClick() =>
                ModBase.RunSafeAction(OnClickAction);

            public override void InitElement() =>
                Element = new HudAPIv2.MenuItem(Name, Parent.CategoryBase, OnClick);
        }

        /// <summary>
        /// Creates an interactable textbox
        /// </summary>
        public class MenuTextInput : MenuSetting<HudAPIv2.MenuTextInput>
        {
            private readonly string queryText;
            private readonly Action<string> OnSubmitAction;

            public MenuTextInput(Func<string> GetName, string queryText, Action<string> OnSubmit, IMenuCategory parent = null) : base(GetName, parent)
            {
                this.queryText = queryText;
                OnSubmitAction = OnSubmit;
            }

            public MenuTextInput(string name, string queryText, Action<string> OnSubmit, IMenuCategory parent = null) : base(name, parent)
            {
                this.queryText = queryText;
                OnSubmitAction = OnSubmit;
            }

            private void OnClick(string input)
            {
                OnSubmitAction(input);
            }

            public override void InitElement()
            {
                Element = new HudAPIv2.MenuTextInput(Name, Parent.CategoryBase, queryText, OnClick);
            }
        }

        /// <summary>
        /// Creates a slider with a specified minimum and maximum value.
        /// </summary>
        public class MenuSliderInput : MenuSetting<HudAPIv2.MenuSliderInput>
        {
            private readonly string queryText;
            private readonly Func<float> CurrentValueAction;
            private readonly Action<float> OnUpdateAction;
            private readonly float min, max, range;
            private readonly int rounding;
            private float start;

            public MenuSliderInput(Func<string> GetName, string queryText, float min, float max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null)
                : base(GetName, parent)
            {
                this.queryText = queryText;
                this.min = min;
                this.max = max;
                range = max - min;
                this.rounding = 2;

                CurrentValueAction = GetCurrentValue;
                OnUpdateAction = OnUpdate;
            }

            public MenuSliderInput(string name, string queryText, float min, float max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null)
                : this(() => name, queryText, min, max, GetCurrentValue, OnUpdate, parent) { }

            public MenuSliderInput(Func<string> GetName, string queryText, int min, int max, Func<int> GetCurrentValue, Action<int> OnUpdate, IMenuCategory parent = null)
                : base(GetName, parent)
            {
                this.queryText = queryText;
                this.min = min;
                this.max = max;
                range = max - min;
                this.rounding = 0;

                CurrentValueAction = () => GetCurrentValue();
                OnUpdateAction = x => OnUpdate((int)x);
            }

            public MenuSliderInput(string name, string queryText, int min, int max, Func<int> GetCurrentValue, Action<int> OnUpdate, IMenuCategory parent = null)
                : this(() => name, queryText, min, max, GetCurrentValue, OnUpdate, parent) { }

            private void OnSubmit(float percent)
            {
                OnUpdateAction((float)Math.Round(Utils.Math.Clamp(min + (range * percent), min, max), rounding));
                Element.InitialPercent = GetCurrentValue();
                start = GetCurrentValue();
            }

            private void OnCancel()
            {
                OnUpdateAction((float)Math.Round(Utils.Math.Clamp(min + (range * start), min, max), rounding));
                Element.InitialPercent = start;
            }

            private float GetCurrentValue() =>
                (float)Math.Round((CurrentValueAction() - min) / range, 2);

            private object GetSliderValue(float percent)
            {
                OnUpdateAction((float)Math.Round(Utils.Math.Clamp(min + (range * percent), min, max), rounding));
                return $"{Math.Round(min + range * percent, rounding)}";
            }

            public override void InitElement()
            {
                start = GetCurrentValue();
                Element = new HudAPIv2.MenuSliderInput(Name, Parent.CategoryBase, start, queryText, OnSubmit, GetSliderValue, OnCancel);
            }
        }

        /// <summary>
        /// Creates a menu category containing slider controls for each color channel
        /// </summary>
        public class MenuColorInput : IMenuElement
        {
            public string Name { get { return colorRoot.Name; } set { colorRoot.Name = value; } }
            public IMenuCategory Parent { get { return colorRoot.Parent; } set { colorRoot.Parent = value; } }

            private readonly MenuCategory colorRoot;
            private readonly Action<Color> OnUpdate;
            private readonly Func<Color> GetCurrentValue;

            public MenuColorInput(string name, Func<Color> GetCurrentValue, Action<Color> OnUpdate, bool showAlpha = true, IMenuCategory parent = null)
            {
                this.GetCurrentValue = GetCurrentValue;
                this.OnUpdate = OnUpdate;

                List<IMenuElement> colorChannles = new List<IMenuElement>(4)
                {
                    new MenuSliderInput(() => $"R: {GetCurrentValue().R}", "Red Value", 0, 255, () => GetCurrentValue().R, UpdateColorR),
                    new MenuSliderInput(() => $"G: {GetCurrentValue().G}", "Green Value", 0, 255, () => GetCurrentValue().G, UpdateColorG),
                    new MenuSliderInput(() => $"B: {GetCurrentValue().B}", "Blue Value", 0, 255, () => GetCurrentValue().B, UpdateColorB),
                };

                if (showAlpha)
                    colorChannles.Add(new MenuSliderInput(() => $"A: {GetCurrentValue().A}", "Alpha Value", 0, 255, () => GetCurrentValue().A, UpdateColorA));

                colorRoot = new MenuCategory(name, "Colors", colorChannles, parent);
            }

            public void InitElement() { }

            private void UpdateColorR(int R)
            {
                Color current = GetCurrentValue();
                current.R = (byte)R;
                OnUpdate(current);
            }

            private void UpdateColorG(int G)
            {
                Color current = GetCurrentValue();
                current.G = (byte)G;
                OnUpdate(current);
            }

            private void UpdateColorB(int B)
            {
                Color current = GetCurrentValue();
                current.B = (byte)B;
                OnUpdate(current);
            }

            private void UpdateColorA(int A)
            {
                Color current = GetCurrentValue();
                current.A = (byte)A;
                OnUpdate(current);
            }
        }
    }
}