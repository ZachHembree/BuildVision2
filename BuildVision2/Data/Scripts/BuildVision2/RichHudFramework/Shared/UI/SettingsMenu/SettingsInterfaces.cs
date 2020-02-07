using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    namespace UI
    {
        using Server;
        using Client;

        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;

        /// <summary>
        /// Used by the API to specify to request a given type of settings menu control
        /// </summary>
        internal enum MenuControls : int
        {
            Checkbox = 1,
            ColorPicker = 2,
            OnOffButton = 3,
            SliderSetting = 4,
            TerminalButton = 5,
            TextField = 6,
            DropdownControl = 7,
            ListControl = 8,
            DragBox = 9,
        }

        internal enum ControlContainers : int
        {
            Tile = 1,
            Category = 2,
        }

        internal enum ModPages : int
        {
            ControlPage = 1,
            RebindPage = 2,
        }

        internal enum TerminalControlAccessors : int
        {
            /// <summary>
            /// MyTuple<bool, Action>
            /// </summary>
            OnSettingChanged = 1,

            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// T
            /// </summary>
            Value = 8,

            /// <summary>
            /// Func{T}
            /// </summary>
            ValueGetter = 9,

            /// <summary>
            /// Action{T}
            /// </summary>
            ValueSetter = 10,
        }

        /// <summary>
        /// Clickable control used in conjunction with the settings menu
        /// </summary>
        public interface ITerminalControl
        {
            /// <summary>
            /// Raised whenever the control's value is changed.
            /// </summary>
            event Action OnControlChanged;

            /// <summary>
            /// Name of the control.
            /// </summary>
            RichText Name { get; set; }

            /// <summary>
            /// Non functional.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifer.
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            ControlMembers GetApiData();
        }

        /// <summary>
        /// Clickable control used in conjunction with the settings menu
        /// </summary>
        public interface ITerminalControl<T> : ITerminalControl where T : ITerminalControl<T>
        {
            Action<T> ControlChangedAction { get; set; }
        }

        public interface ITerminalValue<TValue, TCon> : ITerminalControl<TCon> where TCon : ITerminalControl<TCon>
        {
            /// <summary>
            /// Current value of the control
            /// </summary>
            TValue Value { get; set; }

            Func<TValue> CustomValueGetter { get; set; }

            Action<TValue> CustomValueSetter { get; set; }
        }

        internal enum ControlTileAccessors : int
        {
            /// <summary>
            /// out: MemberAccessor
            /// </summary>
            AddControl = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,
        }

        /// <summary>
        /// Small collection of terminal controls organized into a single block. No more than 1-3
        /// controls should be added to a tile. If a group of controls can't fit on a tile, then they
        /// will be draw outside its bounds.
        /// </summary>
        public interface IControlTile : IEnumerable<ITerminalControl>
        {
            /// <summary>
            /// Read only collection of <see cref="TerminalControlBase"/>s attached to the tile
            /// </summary>
            IReadOnlyCollection<ITerminalControl> Controls { get; }

            IControlTile ControlContainer { get; }

            /// <summary>
            /// Determines whether or not the tile will be rendered in the list.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds a <see cref="TerminalControlBase"/> to the tile
            /// </summary>
            void Add(TerminalControlBase control);

            /// <summary>
            /// Retrieves information needed by the Framework API 
            /// </summary>
            ControlContainerMembers GetApiData();
        }

        internal enum ControlCatAccessors : int
        {
            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            HeaderText = 1,

            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            SubheaderText = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// out: MemberAccessor
            /// </summary>
            AddTile = 4,
        }

        /// <summary>
        /// Horizontally scrolling group of control tiles.
        /// </summary>
        public interface IControlCategory : IEnumerable<IControlTile>
        {
            /// <summary>
            /// Category name
            /// </summary>
            RichText HeaderText { get; set; }

            /// <summary>
            /// Category information
            /// </summary>
            RichText SubheaderText { get; set; }

            /// <summary>
            /// Read only collection of <see cref="IControlTile"/>s assigned to this category
            /// </summary>
            IReadOnlyCollection<IControlTile> Tiles { get; }

            IControlCategory TileContainer { get; }

            /// <summary>
            /// Determines whether or not the element will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier.
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds a <see cref="IControlTile"/>
            /// </summary>
            void Add(ControlTile tile);

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }

        internal enum TerminalPageAccessors : int
        {
            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            Name = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,
        }

        public interface ITerminalPage
        {
            /// <summary>
            /// Name of the <see cref="ITerminalPage"/> as it appears in the dropdown of the <see cref="IModControlRoot"/>.
            /// </summary>
            RichText Name { get; set; }

            /// <summary>
            /// Determines whether or not the <see cref="ITerminalPage"/> will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlMembers GetApiData();
        }

        public interface ITextPage : ITerminalPage
        {
            RichText Text { get; set; }
        }

        internal enum RebindPageAccessors : int
        {
            Add = 10,
        }

        public interface IRebindPage : ITerminalPage, IEnumerable<IBindGroup>
        {
            IReadOnlyCollection<IBindGroup> BindGroups { get; }

            void Add(IBindGroup bindGroup);
        }

        internal enum ControlPageAccessors : int
        {
            /// <summary>
            /// MemberAccessor
            /// </summary>
            AddCategory = 10,

            CategoryData = 11,
        }

        /// <summary>
        /// Vertically scrolling collection of control categories.
        /// </summary>
        public interface IControlPage : ITerminalPage, IEnumerable<IControlCategory>
        {
            /// <summary>
            /// Read only collection of <see cref="IControlCategory"/>s assigned to this object.
            /// </summary>
            IReadOnlyCollection<IControlCategory> Categories { get; }

            IControlPage CategoryContainer { get; }

            /// <summary>
            /// Adds a given <see cref="IControlCategory"/> to the page
            /// </summary>
            void Add(ControlCategory category);
        }

        internal enum ModControlRootAccessors : int
        {
            /// <summary>
            /// MyTuple<bool, Action>
            /// </summary>
            OnSelectionChanged = 1,

            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// MemberAccessor
            /// </summary>
            Selection = 4,

            /// <summary>
            /// MemberAccessor
            /// </summary>
            AddPage = 5,
        }

        public interface IModControlRoot : IEnumerable<ITerminalPage>
        {
            /// <summary>
            /// Raised when a new page is selected
            /// </summary>
            event Action OnSelectionChanged;

            /// <summary>
            /// Name of the mod as it appears in the <see cref="RichHudTerminal"/> mod list
            /// </summary>
            RichText Name { get; set; }

            /// <summary>
            /// Read only collection of <see cref="ITerminalPage"/>s assigned to this object.
            /// </summary>
            IReadOnlyCollection<ITerminalPage> Pages { get; }

            IModControlRoot PageContainer { get; }

            /// <summary>
            /// The currently selected <see cref="ITerminalPage"/>.
            /// </summary>
            ITerminalPage Selection { get; }

            /// <summary>
            /// Determines whether or not the element will appear in the list.
            /// Disabled by default.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Adds the given <see cref="TerminalPageBase"/> to the object.
            /// </summary>
            void Add(TerminalPageBase page);

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}