using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

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
        }

        internal enum ControlContainers : int
        {
            Tile = 1,
            Category = 2,
            Page = 3,
        }

        internal enum TerminalControlAccessors : int
        {
            /// <summary>
            /// MyTuple<bool, Action>
            /// </summary>
            OnSettingChanged = 1,

            /// <summary>
            /// RichStringMembers[]
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// object
            /// </summary>
            Value = 8,
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

        public interface ITerminalValue<T> : ITerminalControl
        {
            /// <summary>
            /// Current value of the control
            /// </summary>
            T Value { get; set; }
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
            /// RichStringMembers[]
            /// </summary>
            HeaderText = 1,

            /// <summary>
            /// RichStringMembers[]
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

        internal enum ControlPageAccessors : int
        {
            /// <summary>
            /// RichStringMembers[]
            /// </summary>
            Name = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,

            /// <summary>
            /// MemberAccessor
            /// </summary>
            AddCategory = 3,
        }

        /// <summary>
        /// Vertically scrolling collection of control categories.
        /// </summary>
        public interface IControlPage : IEnumerable<IControlCategory>
        {
            /// <summary>
            /// Name of the <see cref="IControlPage"/> as it appears in the dropdown of the <see cref="IModControlRoot"/>.
            /// </summary>
            RichText Name { get; set; }

            /// <summary>
            /// Read only collection of <see cref="IControlCategory"/>s assigned to this object.
            /// </summary>
            IReadOnlyCollection<IControlCategory> Categories { get; }

            IControlPage CategoryContainer { get; }

            /// <summary>
            /// Determines whether or not the <see cref="IControlPage"/> will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds a given <see cref="IControlCategory"/> to the page
            /// </summary>
            void Add(ControlCategory category);

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }

        internal enum ModControlRootAccessors : int
        {
            /// <summary>
            /// MyTuple<bool, Action>
            /// </summary>
            OnSelectionChanged = 1,

            /// <summary>
            /// RichStringMembers[]
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

        public interface IModControlRoot : IEnumerable<IControlPage>
        {
            /// <summary>
            /// Raised when a new page is selected
            /// </summary>
            event Action OnSelectionChanged;

            /// <summary>
            /// Name of the mod as it appears in the <see cref="ModMenu"/> mod list
            /// </summary>
            RichText Name { get; set; }

            /// <summary>
            /// Read only collection of <see cref="IControlPage"/>s assigned to this object.
            /// </summary>
            IReadOnlyCollection<IControlPage> Pages { get; }

            IModControlRoot PageContainer { get; }

            /// <summary>
            /// The currently selected <see cref="IControlPage"/>.
            /// </summary>
            IControlPage Selection { get; }

            /// <summary>
            /// Determines whether or not the element will appear in the list.
            /// Disabled by default.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Adds the given <see cref="ControlPage"/> to the object.
            /// </summary>
            void Add(ControlPage page);

            /// <summary>
            /// Retrieves data used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}