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
    internal sealed class HudUtilities
    {
        public static HudUtilities Instance { get; private set; }
        public UiTestPattern TestPattern { get; private set; }
        public bool Heartbeat { get { return textHudApi.Heartbeat; } }

        private static BvMain Main { get { return BvMain.Instance; } }
        private HudAPIv2 textHudApi;
        private double screenWidth, screenHeight, aspectRatio, fov, fovScale;
        private List<Action> hudElementsDraw;
        private Queue<Action> menuElementsInit;

        private HudUtilities()
        {
            textHudApi = new HudAPIv2();

            screenWidth = (double)MyAPIGateway.Session.Config.ScreenWidth;
            screenHeight = (double)MyAPIGateway.Session.Config.ScreenHeight;
            aspectRatio = screenWidth / screenHeight;
            fov = MyAPIGateway.Session.Camera.FovWithZoom;
            fovScale = 0.1 * Math.Tan(fov / 2d);

            hudElementsDraw = new List<Action>();
            menuElementsInit = new Queue<Action>();
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new HudUtilities();
                Instance.TestPattern = new UiTestPattern();
                Instance.TestPattern.Hide();
            }
        }

        public void Draw()
        {
            if (Heartbeat)
            {
                if (fov != MyAPIGateway.Session.Camera.FovWithZoom)
                {
                    fov = MyAPIGateway.Session.Camera.FovWithZoom;
                    fovScale = 0.1 * Math.Tan(fov / 2d);
                }

                foreach (Action Draw in hudElementsDraw)
                    Draw();

                Action InitElement;

                while (menuElementsInit.Count > 0)
                {
                    if (menuElementsInit.TryDequeue(out InitElement))
                        InitElement();
                }
            }
        }

        public void Close()
        {
            textHudApi?.Close();
            Instance = null;
        }

        public interface IMenuElement
        {
            MenuCategoryBase Parent { get; set; }
        }

        public abstract class MenuElement<T> : IMenuElement where T : HudAPIv2.MenuItemBase
        {
            public abstract MenuCategoryBase Parent { get; set; }
            public T Element { get; protected set; }

            public MenuElement(MenuCategoryBase parent = null)
            {
                if (Instance == null)
                    throw new Exception("Menu Elements cannot be created without initializing HudUtilities.");

                Parent = parent;
                Instance.menuElementsInit.Enqueue(InitElement);
            }

            protected abstract void InitElement();
        }
        
        public abstract class MenuCategoryBase : MenuElement<HudAPIv2.MenuCategoryBase>
        {
            public override MenuCategoryBase Parent { get; set; }
            protected MenuCategoryBase parent;
            protected readonly string name, header;
            protected readonly List<IMenuElement> children;

            public MenuCategoryBase(string name, string header, IList<IMenuElement> children = null)
            {
                this.name = name;
                this.header = header;
                this.children = (List<IMenuElement>)children;

                if (children != null)
                {
                    foreach (IMenuElement child in children)
                        child.Parent = this;
                }
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

        public class MenuRoot : MenuCategoryBase
        {
            public MenuRoot(string name, string header, IList<IMenuElement> children = null) : base(name, header, children)
            { }

            protected override void InitElement() =>
                Element = new HudAPIv2.MenuRootCategory(name, MenuFlag.PlayerMenu, header);
        }

        public class MenuCategory : MenuCategoryBase
        {
            public override MenuCategoryBase Parent
            {
                get { return parent; }
                set
                {
                    parent = value;

                    if (category != null)
                        category.Parent = parent.Element; //but y tho
                }
            }

            private HudAPIv2.MenuSubCategory category;

            public MenuCategory(string name, string header, IList<IMenuElement> children = null, MenuCategoryBase parent = null) : base(name, header, children)
            {
                Parent = parent;
            }

            protected override void InitElement()
            {
                category = new HudAPIv2.MenuSubCategory(name, Parent?.Element, header);
                Element = category;
            }
        }

        /// <summary>
        /// Wrapper base for HudAPIv2.MenuItem controls
        /// </summary>
        public abstract class MenuSetting<T> : MenuElement<T> where T : HudAPIv2.MenuItemBase
        {
            protected readonly Func<string> GetDisplay;
            protected MenuCategoryBase parent;

            public MenuSetting(Func<string> GetDisplay, MenuCategoryBase parent = null)
            {
                if (typeof(T) == typeof(HudAPIv2.MenuCategoryBase))
                    throw new Exception("Types of HudAPIv2.MenuCategoryBase cannot be used to create MenuSettings.");
                
                this.parent = parent;
                this.GetDisplay = GetDisplay;
            }
        }

        /// <summary>
        /// Wrapper used to simplify usage of HudAPIv2.MenuItem
        /// </summary>
        public class MenuButton : MenuSetting<HudAPIv2.MenuItem>
        {
            public override MenuCategoryBase Parent
            {
                get { return parent; }
                set
                {
                    parent = value;

                    if (Element != null)
                        Element.Parent = parent.Element;
                }
            }

            private readonly Action OnClickAction;

            public MenuButton(Func<string> GetDisplay, Action OnClick, MenuCategoryBase parent = null) : base(GetDisplay, parent)
            {
                OnClickAction = OnClick;
            }

            private void OnClick()
            {
                OnClickAction();
                Element.Text = GetDisplay();
            }

            protected override void InitElement() =>
                Element = new HudAPIv2.MenuItem(GetDisplay(), Parent?.Element, OnClick);
        }

        /// <summary>
        /// Wrapper used to simplify usage of HudAPIv2.MenuTextInput
        /// </summary>
        public class MenuTextInput : MenuSetting<HudAPIv2.MenuTextInput>
        {
            public override MenuCategoryBase Parent
            {
                get { return parent; }
                set
                {
                    parent = value;

                    if (Element != null)
                        Element.Parent = parent.Element;
                }
            }

            private readonly string queryText;
            private readonly Action<string> OnSubmitAction;

            public MenuTextInput(Func<string> GetDisplay, string queryText, Action<string> OnSubmit, MenuCategoryBase parent = null) : base(GetDisplay, parent)
            {
                this.queryText = queryText;
                OnSubmitAction = OnSubmit;
            }

            private void OnClick(string input)
            {
                OnSubmitAction(input);
                Element.Text = GetDisplay();
            }

            protected override void InitElement() =>
                Element = new HudAPIv2.MenuTextInput(GetDisplay(), Parent?.Element, queryText, OnClick);
        }

        /// <summary>
        /// Wrapper used to simplify usage of HudAPIv2.MenuSliderInput
        /// </summary>
        public class MenuSliderInput : MenuSetting<HudAPIv2.MenuSliderInput>
        {
            public override MenuCategoryBase Parent
            {
                get { return parent; }
                set
                {
                    parent = value;

                    if (Element != null)
                        Element.Parent = parent.Element;
                }
            }

            private readonly string queryText;
            private readonly Func<float> CurrentValueAction;
            private readonly Action<float> OnUpdateAction;
            private readonly float min, max, range;
            private readonly int rounding;
            private float start;

            public MenuSliderInput(float min, float max, Func<float> GetCurrentValue, Func<string> GetDisplay, string queryText, Action<float> OnUpdate, MenuCategoryBase parent = null) : base(GetDisplay, parent)
            {
                this.min = min;
                this.max = max;
                range = max - min;
                rounding = 2;

                this.queryText = queryText;
                CurrentValueAction = GetCurrentValue;
                OnUpdateAction = OnUpdate;
            }

            public MenuSliderInput(int min, int max, Func<float> GetCurrentValue, Func<string> GetDisplay, string queryText, Action<float> OnUpdate, MenuCategoryBase parent = null) : base(GetDisplay, parent)
            {
                this.min = min;
                this.max = max;
                range = max - min;
                rounding = 0;

                this.queryText = queryText;
                CurrentValueAction = GetCurrentValue;
                OnUpdateAction = OnUpdate;
            }

            private void OnSubmit(float percent)
            {
                OnUpdateAction((float)Math.Round(Utilities.Clamp(min + (range * percent), min, max), rounding));
                Element.InitialPercent = GetCurrentValue();
                Element.Text = GetDisplay();
                start = GetCurrentValue();
            }

            private void OnCancel()
            {
                OnUpdateAction((float)Math.Round(Utilities.Clamp(min + (range * start), min, max), rounding));
                Element.InitialPercent = start;
                Element.Text = GetDisplay();
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
                Element = new HudAPIv2.MenuSliderInput(GetDisplay(), Parent?.Element, start, queryText, OnSubmit, GetSliderValue, OnCancel);
            }
        }

        public class MenuColorInput : IMenuElement
        {
            public MenuCategoryBase Parent
            {
                get { return parent; }
                set
                {
                    parent = value;
                    colorRoot.Parent = value;
                }
            }

            private MenuCategoryBase parent;
            private readonly MenuCategory colorRoot;
            private readonly MenuSliderInput r, g, b, a;

            private readonly Action<Color> OnUpdate;
            private readonly Func<Color> GetCurrentValue;

            public MenuColorInput(string name, Func<Color> GetCurrentValue, Action<Color> OnUpdate, bool showAlpha = true, MenuCategoryBase parent = null)
            {
                Parent = parent;
                this.GetCurrentValue = GetCurrentValue;
                this.OnUpdate = OnUpdate;

                List<IMenuElement> colorChannles = new List<IMenuElement>(4)
                {
                    new MenuSliderInput(0, 255, () => GetCurrentValue().R, () => "R", "Red Value", UpdateColorR),
                    new MenuSliderInput(0, 255, () => GetCurrentValue().G, () => "G", "Green Value", UpdateColorG),
                    new MenuSliderInput(0, 255, () => GetCurrentValue().B, () => "B", "Blue Value", UpdateColorB),
                };

                if (showAlpha)
                    colorChannles.Add(new MenuSliderInput(0, 255, () => GetCurrentValue().A, () => "A", "Alpha Value", UpdateColorA));

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

        public enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        /// <summary>
        /// Abstract base for hud elements
        /// </summary>
        public abstract class HudElement
        {
            public HudElement()
            {
                if (Instance == null)
                    throw new Exception("Hud Elements cannot be created without initializing HudUtilities.");

                Instance.hudElementsDraw.Add(Draw);
            }

            /// <summary>
            /// If a parent is set, the hud element's position will be centered around it.
            /// </summary>
            public virtual HudElement Parent { get; set; } = null;

            /// <summary>
            /// Position using scaled coordinate system
            /// </summary>
            public virtual Vector2D ScaledPos { get; set; } = Vector2D.Zero; // behavior of ScaledPos is not consistent; do something about that

            /// <summary>
            /// If set to true, the hud element will be visible. Parented elements will be hidden if the parent is not visible.
            /// </summary>
            public virtual bool Visible
            {
                get { return Parent == null ? visible : Parent.Visible && visible; }
                set { visible = value; }
            }

            protected bool visible = true;

            /// <summary>
            /// Sizing scale corrected to be consistent across both HUD Message based elements and Billboard based elements.
            /// </summary>
            public virtual double Scale { get; set; } = 1d;

            /// <summary>
            /// Current position from the center of the screen or parent in pixels. Includes parented positions.
            /// </summary>
            public virtual Vector2I Origin
            {
                get { return Parent == null ? origin : (origin + Parent.Origin + Parent.Offset); }
                set { origin = value; }
            }

            protected Vector2I origin = Vector2I.Zero;

            /// <summary>
            /// Determines location of the HUD element relative to the origin.
            /// </summary>
            public virtual Vector2I Offset { get; set; } = Vector2I.Zero;

            /// <summary>
            /// Updates settings and draws hud element.
            /// </summary>
            protected abstract void Draw();
        }

        /// <summary>
        /// Scrollable list menu; the selection box position is based on the selction index.
        /// </summary>
        public class ScrollMenu : HudElement
        {
            public StringBuilder HeaderText { get { return header.Message; } set { header.Message = value; } }
            public StringBuilder FooterLeftText { get { return footerLeft.Message; } set { footerLeft.Message = value; } }
            public StringBuilder FooterRightText { get { return footerRight.Message; } set { footerRight.Message = value; } }
            public StringBuilder[] ListText
            {
                get { return listText; }
                set
                {
                    listText = value;

                    if (list == null || list.Length < listText.Length)
                    {
                        list = new TextHudMessage[listText.Length];

                        for (int n = 0; n < list.Length; n++)
                            list[n] = new TextHudMessage(background, TextAlignment.Left);
                    }

                    for (int n = 0; n < listText.Length; n++)
                        list[n].Message = listText[n];
                }
            }

            public int SelectionIndex
            {
                get { return selectionIndex; }
                set { selectionIndex = Utilities.Clamp(value, 0, (ListText != null ? list.Length - 1 : 0)); }
            }

            public Vector2D Size { get; private set; }
            public Color BodyColor { get { return background.color; } set { background.color = value; } }
            public Color SelectionBoxColor { get { return highlightBox.color; } set { highlightBox.color = value; } }
            public Color HeaderColor
            {
                get { return headerColor; }
                set
                {
                    headerBg.color = value;
                    footerBg.color = value;
                    headerColor = value;
                }
            }

            private static Vector2I padding;
            private StringBuilder[] listText;
            private Color headerColor;

            private readonly TexturedBox headerBg, footerBg, background, highlightBox, tab;
            private readonly TextHudMessage header, footerLeft, footerRight;
            private TextHudMessage[] list;
            private double currentScale = 0d;
            private int selectionIndex = 0;

            public ScrollMenu(int maxListLength)
            {
                background = new TexturedBox(this);

                headerBg = new TexturedBox(background);
                header = new TextHudMessage(headerBg);                

                footerBg = new TexturedBox(background);
                footerLeft = new TextHudMessage(footerBg, TextAlignment.Left);
                footerRight = new TextHudMessage(footerBg, TextAlignment.Right);

                highlightBox = new TexturedBox(background);
                tab = new TexturedBox(highlightBox, new Color(225, 225, 240, 255));

                list = new TextHudMessage[maxListLength];

                for (int n = 0; n < list.Length; n++)
                    list[n] = new TextHudMessage(background, TextAlignment.Left);
            }

            protected override void Draw()
            {
                if (Visible && ListText != null)
                {
                    padding = new Vector2I((int)(72f * Scale), (int)(32f * Scale));

                    if (Scale != currentScale)
                        SetScale(Scale);

                    Vector2I listSize = GetListSize(), textOffset = listSize / 2, pos;
                    Origin = Instance.GetPixelPos(Utilities.Round(ScaledPos, 3));

                    background.Size = listSize + padding;

                    headerBg.Size = new Vector2I(background.Width, header.TextSize.Y + (int)(28d * Scale));
                    headerBg.Offset = new Vector2I(0, (headerBg.Height + background.Height) / 2);

                    pos = new Vector2I(-textOffset.X, textOffset.Y - list[0].TextSize.Y / 2);

                    for (int n = 0; n < ListText.Length && n < list.Length; n++)
                    {
                        list[n].Offset = pos;
                        list[n].Visible = true;
                        pos.Y -= list[n].TextSize.Y;
                    }

                    for (int n = ListText.Length; n < list.Length; n++)
                        list[n].Visible = false;

                    highlightBox.Size = new Vector2I(listSize.X + 16, (int)(24d * Scale));
                    highlightBox.Offset = new Vector2I(0, list[SelectionIndex].Offset.Y);

                    tab.Size = new Vector2I(4, highlightBox.Height);
                    tab.Offset = new Vector2I(-highlightBox.Width / 2 + 1, 0);

                    footerBg.Size = new Vector2I(background.Width, footerLeft.TextSize.Y + (int)(12d * Scale));
                    footerBg.Offset = new Vector2I(0, -(background.Height + footerBg.Height) / 2);
                    footerLeft.Offset = new Vector2I((-footerBg.Width + padding.X) / 2, 0);
                    footerRight.Offset = new Vector2I((footerBg.Width - padding.X) / 2, 0);

                    Offset = -new Vector2I(0, headerBg.Height - footerBg.Height) / 2;
                    Size = Instance.GetScaledPos(background.Size + new Vector2I(0, headerBg.Height + footerBg.Height));
                }
            }

            private Vector2I GetListSize()
            {
                Vector2I listSize, lineSize;
                int maxLineWidth = 0, footerWidth;
                listSize = Vector2I.Zero;

                for (int n = 0; (n < listText.Length && n < list.Length); n++)
                {
                    lineSize = list[n].TextSize;
                    listSize.Y += lineSize.Y;

                    if (lineSize.X > maxLineWidth)
                        maxLineWidth = lineSize.X;
                }

                if (header.TextSize.X > maxLineWidth)
                    maxLineWidth = header.TextSize.X;

                footerWidth = footerLeft.TextSize.X + footerRight.TextSize.X + padding.X;

                if (footerWidth > maxLineWidth)
                    maxLineWidth = footerWidth;

                listSize.X = maxLineWidth;
                return listSize;
            }

            private void SetScale(double scale)
            {
                header.Scale = scale * 1.1;
                footerLeft.Scale = scale;
                footerRight.Scale = scale;

                foreach (TextHudMessage element in list)
                    element.Scale = scale;

                currentScale = scale;
            }
        }

        /// <summary>
        /// Wrapper used to make precise pixel-level manipluation of Text HUD API messages easier.
        /// </summary>
        public class TextHudMessage : HudElement
        {
            public StringBuilder Message
            {
                get { return message; }
                set
                {
                    message = value;

                    if (hudMessage != null)
                        UpdateMessage();
                }
            }

            public override Vector2D ScaledPos
            {
                get { return hudMessage != null ? hudMessage.Origin : Vector2D.Zero; }
                set { hudMessage.Origin = value; }
            }

            public override double Scale
            {
                get { return scale; }
                set
                {
                    scale = value * (278d / (500d - 138.75 * Instance.aspectRatio));

                    if (hudMessage != null)
                        hudMessage.Scale = scale;
                }
            }

            public Vector2D ScaledTextSize { get; private set; }
            public Vector2I TextSize { get; private set; }
            public TextAlignment alignment;

            private HudAPIv2.HUDMessage hudMessage;
            private StringBuilder message;
            private Vector2I alignmentOffset;
            private double scale;

            public TextHudMessage(HudElement Parent = null, TextAlignment alignment = TextAlignment.Center)
            {
                this.Parent = Parent;
                this.alignment = alignment;                
            }

            /// <summary>
            /// Updates settings of underlying Text HUD API type.
            /// </summary>
            protected override void Draw()
            {
                if (hudMessage == null)
                {
                    hudMessage = new HudAPIv2.HUDMessage
                    {
                        Blend = BlendTypeEnum.LDR,
                        Scale = Scale,
                        Options = HudAPIv2.Options.Fixed,
                        Visible = false,
                    };

                    UpdateMessage();
                }

                if (Visible)
                {
                    hudMessage.Origin = Instance.GetScaledPos(Origin + Offset + alignmentOffset);
                    hudMessage.Draw();
                }
            }

            private void UpdateMessage()
            {
                if (Message != null)
                {
                    Vector2D length;

                    hudMessage.Message = Message;
                    length = hudMessage.GetTextLength();
                    ScaledTextSize = length;
                    TextSize = Utilities.Abs(Instance.GetPixelPos(ScaledTextSize));
                    GetAlignmentOffset();
                }
            }

            private void GetAlignmentOffset()
            {
                alignmentOffset = TextSize / 2;
                alignmentOffset.X *= -1;

                if (alignment == TextAlignment.Right)
                {
                    alignmentOffset.X -= TextSize.X / 2;
                }
                else if (alignment == TextAlignment.Left)
                {
                    alignmentOffset.X += TextSize.X / 2;
                }
            }
        }

        /// <summary>
        /// Creates a colored box of a given width and height with a given mateiral. The default material is just a plain color.
        /// </summary>
        public class TexturedBox : HudElement
        {
            public Vector2D ScaledSize { get; private set; }
            public Vector2I Size { get { return size; } set { size = Utilities.Abs(value); } }
            public int Height { get { return Size.Y; } set { Size = new Vector2I(value, Size.Y); } }
            public int Width { get { return Size.X; } set { Size = new Vector2I(Size.X, value); ; } }

            public MyStringId material;
            public Color color;
            private Vector2I size;

            public TexturedBox(HudElement Parent = null, Color color = default(Color), MyStringId material = default(MyStringId))
            {
                this.color = color;
                this.Parent = Parent;

                if (material == default(MyStringId))
                    this.material = MyStringId.GetOrCompute("Square");
                else
                    this.material = material;
            }

            protected override void Draw()
            {
                if (Visible)
                {
                    MatrixD cameraMatrix;
                    Quaternion rotquad;
                    Vector3D boardPos;
                    Vector2D boardOrigin, boardSize;

                    ScaledSize = Instance.GetScaledSize(Size, Scale);
                    ScaledPos = Instance.GetScaledPos(Origin + Offset);

                    boardSize = ScaledSize * Instance.fovScale * 16d;

                    boardOrigin = ScaledPos;
                    boardOrigin.X *= Instance.fovScale * Instance.aspectRatio;
                    boardOrigin.Y *= Instance.fovScale;

                    cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    boardPos = Vector3D.Transform(new Vector3D(boardOrigin.X, boardOrigin.Y, -0.1), cameraMatrix);

                    rotquad = Quaternion.CreateFromAxisAngle(cameraMatrix.Forward, 0f);
                    cameraMatrix = MatrixD.Transform(cameraMatrix, rotquad);

                    MyTransparentGeometry.AddBillboardOriented
                    (
                        material,
                        color,
                        boardPos,
                        cameraMatrix.Left,
                        cameraMatrix.Up,
                        (float)boardSize.X,
                        (float)boardSize.Y,
                        Vector2.Zero,
                        BlendTypeEnum.LDR
                    );
                }
            }
        }

        /// <summary>
        /// Converts text Hud API sizing scale to pixels
        /// </summary>
        public Vector2I GetPixelSize(Vector2D scaledSize, double scale = 1d)
        {
            return new Vector2I
            (
                (int)(scaledSize.X * screenHeight * 16d / Math.Sqrt(scale)),
                (int)(scaledSize.Y * screenHeight * 16d / Math.Sqrt(scale))
            );
        }

        /// <summary>
        /// Converts from a size given in pixels to the scale used by the text Hud API
        /// </summary>
        public Vector2D GetScaledSize(Vector2I pixelSize, double scale = 1d)
        {
            return new Vector2D
            (
                pixelSize.X,
                pixelSize.Y
            ) * (Math.Sqrt(scale) / screenHeight / 16d);
        }

        /// <summary>
        /// Converts from a coordinate in the scaled coordinate system to a concrete coordinate in pixels.
        /// Also useful for converting text block sizes to pixels for some reason.
        /// </summary>
        public Vector2I GetPixelPos(Vector2D scaledPos)
        {
            scaledPos /= 2d;

            return new Vector2I
            (
                (int)(scaledPos.X * screenWidth),
                (int)(scaledPos.Y * screenHeight)
            );
        }

        /// <summary>
        /// Converts from a coordinate given in pixels to the API's scaled coordinate system.
        /// </summary>
        public Vector2D GetScaledPos(Vector2I pixelPos)
        {
            pixelPos *= 2;

            return new Vector2D
            (
                pixelPos.X / screenWidth,
                pixelPos.Y / screenHeight
            );
        }

        /// <summary>
        /// Pattern of textured boxes used to test scaling and positioning.
        /// </summary>
        public class UiTestPattern
        {
            private readonly TexturedBox[] testPattern;
            private bool visible;

            public UiTestPattern()
            {
                visible = false;

                testPattern = new TexturedBox[]
                {
                    new TexturedBox() // red
                    {
                        color = new Color(255, 0, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(300, 0)
                    },
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-300, 0)
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, 300)
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, -300)
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, 0)
                    },
                    // sqrt(50) x sqrt(50)
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-200, -200),
                        Scale = .5d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(200, -200),
                        Scale = .5d
                    },
                    // 50 x 50
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-400, -400),
                        Scale = .25d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(400, -400),
                        Scale = .25d
                    },
                };
            }

            public void Toggle()
            {
                if (visible)
                    Hide();
                else
                    Show();
            }

            public void Show()
            {
                foreach (TexturedBox box in testPattern)
                    box.Visible = true;

                visible = true;
            }

            public void Hide()
            {
                foreach (TexturedBox box in testPattern)
                    box.Visible = false;

                visible = false;
            }
        }

    }
}
