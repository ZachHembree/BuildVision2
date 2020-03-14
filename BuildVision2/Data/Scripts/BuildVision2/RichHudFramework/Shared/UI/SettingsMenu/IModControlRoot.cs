﻿using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
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
        public enum MenuControls : int
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

        public enum ControlContainers : int
        {
            Tile = 1,
            Category = 2,
        }

        public enum ModPages : int
        {
            ControlPage = 1,
            RebindPage = 2,
        }

        public enum ModControlRootAccessors : int
        {
            /// <summary>
            /// MyTuple<bool, Action>
            /// </summary>
            OnSelectionChanged = 1,

            /// <summary>
            /// string
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

        /// <summary>
        /// Indented dropdown list of terminal pages. Root UI element for all terminal controls
        /// associated with a given mod.
        /// </summary>
        public interface IModControlRoot : IEnumerable<ITerminalPage>
        {
            /// <summary>
            /// Invoked when a new page is selected
            /// </summary>
            event Action OnSelectionChanged;

            /// <summary>
            /// Name of the mod as it appears in the <see cref="RichHudTerminal"/> mod list
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// Read only collection of <see cref="ITerminalPage"/>s assigned to this object.
            /// </summary>
            IReadOnlyCollection<ITerminalPage> Pages { get; }

            IModControlRoot PageContainer { get; }

            /// <summary>
            /// Currently selected <see cref="ITerminalPage"/>.
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