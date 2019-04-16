using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRageMath;
using VRage.Utils;
using VRage.Game;
using System;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using MenuFlag = DarkHelmet.BuildVision2.HudAPIv2.MenuRootCategory.MenuFlag;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Collection of tools used to make working with the Text Hud API and general GUI stuff easier.
    /// </summary>
    internal sealed partial class HudUtilities
    {
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
        }

        /// <summary>
        /// Base class for all wrapper types that instantiate HudAPIv2.MenuItemBase mod menu elements
        /// </summary>
        public abstract class MenuElement<T> : IMenuElement where T : HudAPIv2.MenuItemBase
        {
            public abstract IMenuCategory Parent { get; set; }
            public virtual T Element { get; protected set; }

            public virtual string Name
            {
                get { return GetName(); }
                set
                {
                    name = value;

                    if (Element != null)
                        Element.Text = GetName();
                }
            }

            public virtual Func<string> GetName { get; set; }
            protected string name;
            private int requeueCount;

            public MenuElement(string name, IMenuCategory parent = null)
            {
                this.name = name;
                Init(() => this.name, parent);
            }

            public MenuElement(Func<string> GetName, IMenuCategory parent = null)
            {
                Init(GetName, parent);
            }

            private void Init(Func<string> GetName, IMenuCategory parent)
            {
                if (Instance == null)
                    throw new Exception("Menu Elements cannot be created without initializing HudUtilities.");

                requeueCount = 0;
                this.GetName = GetName;
                Parent = parent;
                Instance.menuElementsInit.Enqueue(TryInitElement);
            }

            private void TryInitElement()
            {
                if (Parent != null && (Parent.CategoryBase != null || Parent.IsRoot))
                    InitElement();
                else if (requeueCount < 5)
                {
                    Instance.menuElementsInit.Enqueue(TryInitElement);
                    requeueCount++;
                }
            }

            /// <summary>
            /// Used to instantiate HudAPIv2.MenuItemBase elements upon initialization of the Text Hud API
            /// </summary>
            protected abstract void InitElement();
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
            protected List<IMenuElement> children;

            public MenuCategoryBase(string name, string header, IMenuCategory parent = null, List<IMenuElement> children = null, bool IsRoot = false) : base(name, parent)
            {
                Init(header, children, IsRoot);
            }

            public MenuCategoryBase(Func<string> GetName, string header, IMenuCategory parent = null, List<IMenuElement> children = null, bool IsRoot = false) : base(GetName, parent)
            {
                Init(header, children, IsRoot);
            }

            private void Init(string header, List<IMenuElement> children, bool IsRoot)
            {
                this.Header = header;
                this.IsRoot = IsRoot;
                this.children = children;

                if (this.children != null)
                {
                    foreach (IMenuElement child in this.children)
                        child.Parent = this;
                }
                else
                    this.children = new List<IMenuElement>();
            }

            public virtual void AddChild(IMenuElement child)
            {
                children.Add(child);
                child.Parent = this;
            }

            public virtual void AddChildren(IEnumerable<IMenuElement> newChildren)
            {
                foreach (IMenuElement child in newChildren)
                    child.Parent = this;

                children.AddRange(newChildren);
            }
        }

        /// <summary>
        /// Contains all settings menu elements for a given mod; singleton. Must be initalized before any other menu elements.
        /// </summary>
        public sealed class MenuRoot : MenuCategoryBase<HudAPIv2.MenuRootCategory>
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

            public void Close()
            {
                Instance = null;
            }

            protected override void InitElement() =>
                Element = new HudAPIv2.MenuRootCategory(Name, MenuFlag.PlayerMenu, Header);
        }

        /// <summary>
        /// Collapsable submenu that can contain other elements, including other submenus
        /// </summary>
        public class MenuCategory : MenuCategoryBase<HudAPIv2.MenuSubCategory>
        {
            public override IMenuCategory Parent
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

            public MenuCategory(Func<string> GetName, string header, List<IMenuElement> children = null, IMenuCategory parent = null) : base(GetName, header, parent, children)
            { }

            public MenuCategory(string name, string header, List<IMenuElement> children = null, IMenuCategory parent = null) : base(name, header, parent, children)
            { }

            protected override void InitElement()
            {
                Element = new HudAPIv2.MenuSubCategory(Name, Parent.CategoryBase, Header);
            }
        }

        /// <summary>
        /// Wrapper base for HudAPIv2.MenuItem based controls (buttons, sliders, text boxes, etc.)
        /// </summary>
        public abstract class MenuSetting<T> : MenuElement<T> where T : HudAPIv2.MenuItemBase
        {
            public MenuSetting(string name, IMenuCategory parent = null) : base(name, parent)
            {
                if (typeof(T) == typeof(HudAPIv2.MenuCategoryBase))
                    throw new Exception("Types of HudAPIv2.MenuCategoryBase cannot be used to create MenuSettings.");
            }

            public MenuSetting(Func<string> GetName, IMenuCategory parent = null) : base(GetName, parent)
            {
                if (typeof(T) == typeof(HudAPIv2.MenuCategoryBase))
                    throw new Exception("Types of HudAPIv2.MenuCategoryBase cannot be used to create MenuSettings.");
            }
        }

        /// <summary>
        /// Creates a clickable menu button
        /// </summary>
        public class MenuButton : MenuSetting<HudAPIv2.MenuItem>
        {
            public override IMenuCategory Parent
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
            private readonly Action OnClickAction;

            public MenuButton(Func<string> GetName, Action OnClick, IMenuCategory parent = null) : base(GetName, parent)
            {
                OnClickAction = OnClick;
            }

            public MenuButton(string name, Action OnClick, IMenuCategory parent = null) : base(name, parent)
            {
                OnClickAction = OnClick;
            }

            private void OnClick()
            {
                OnClickAction();
                Element.Text = Name;
            }

            protected override void InitElement()
            {
                Element = new HudAPIv2.MenuItem(Name, Parent.CategoryBase, OnClick);
            }
        }

        /// <summary>
        /// Creates an interactable textbox
        /// </summary>
        public class MenuTextInput : MenuSetting<HudAPIv2.MenuTextInput>
        {
            public override IMenuCategory Parent
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
                Element.Text = Name;
            }

            protected override void InitElement()
            {
                Element = new HudAPIv2.MenuTextInput(Name, Parent.CategoryBase, queryText, OnClick);
            }
        }

        /// <summary>
        /// Creates a slider with a specified minimum and maximum value.
        /// </summary>
        public class MenuSliderInput : MenuSetting<HudAPIv2.MenuSliderInput>
        {
            public override IMenuCategory Parent
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
            private string queryText;
            private Func<float> CurrentValueAction;
            private Action<float> OnUpdateAction;
            private float min, max, range;
            private int rounding;
            private float start;

            public MenuSliderInput(string name, string queryText, float min, float max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null) : base(name, parent)
            {
                Init(queryText, min, max, 2, GetCurrentValue, OnUpdate, parent);
            }

            public MenuSliderInput(string name, string queryText, int min, int max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null) : base(name, parent)
            {
                Init(queryText, min, max, 0, GetCurrentValue, OnUpdate, parent);
            }

            public MenuSliderInput(Func<string> GetName, string queryText, float min, float max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null) : base(GetName, parent)
            {
                Init(queryText, min, max, 2, GetCurrentValue, OnUpdate, parent);
            }

            public MenuSliderInput(Func<string> GetName, string queryText, int min, int max, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null) : base(GetName, parent)
            {
                Init(queryText, min, max, 0, GetCurrentValue, OnUpdate, parent);
            }

            private void Init(string queryText, float min, float max, int rounding, Func<float> GetCurrentValue, Action<float> OnUpdate, IMenuCategory parent = null)
            {
                this.queryText = queryText;
                this.min = min;
                this.max = max;
                range = max - min;
                this.rounding = rounding;

                CurrentValueAction = GetCurrentValue;
                OnUpdateAction = OnUpdate;
            }

            private void OnSubmit(float percent)
            {
                OnUpdateAction((float)Math.Round(Utilities.Clamp(min + (range * percent), min, max), rounding));
                Element.InitialPercent = GetCurrentValue();
                Element.Text = Name;
                start = GetCurrentValue();
            }

            private void OnCancel()
            {
                OnUpdateAction((float)Math.Round(Utilities.Clamp(min + (range * start), min, max), rounding));
                Element.InitialPercent = start;
                Element.Text = Name;
            }

            private float GetCurrentValue() =>
                (float)Math.Round((CurrentValueAction() - min) / range, 2);

            private object GetSliderValue(float percent)
            {
                OnUpdateAction((float)Math.Round(Utilities.Clamp(min + (range * percent), min, max), rounding));
                return $"{Math.Round(min + range * percent, rounding)}";
            }

            protected override void InitElement()
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

            private void UpdateColorR(float R)
            {
                Color current = GetCurrentValue();
                current.R = (byte)R;
                OnUpdate(current);
            }

            private void UpdateColorG(float G)
            {
                Color current = GetCurrentValue();
                current.G = (byte)G;
                OnUpdate(current);
            }

            private void UpdateColorB(float B)
            {
                Color current = GetCurrentValue();
                current.B = (byte)B;
                OnUpdate(current);
            }

            private void UpdateColorA(float A)
            {
                Color current = GetCurrentValue();
                current.A = (byte)A;
                OnUpdate(current);
            }
        }
    }
}