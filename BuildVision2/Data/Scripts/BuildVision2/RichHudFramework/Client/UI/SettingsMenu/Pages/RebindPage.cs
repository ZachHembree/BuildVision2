using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework
{
    using BindDefinitionData = MyTuple<string, string[], string[][]>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;

    namespace UI.Client
    {
        /// <summary>
        /// Scrollable list of bind group controls.
        /// </summary>
        public class RebindPage : TerminalPageBase, IRebindPage
        {
            /// <summary>
            /// List of bind groups registered to the page.
            /// </summary>
            public IReadOnlyList<IBindGroup> BindGroups => bindGroups;

            /// <summary>
            /// Allows addition of bind groups using collection-initializer syntax
            /// </summary>
            public RebindPage GroupContainer => this;

            private readonly List<IBindGroup> bindGroups;

            public RebindPage() : base(ModPages.RebindPage)
            {
                bindGroups = new List<IBindGroup>();
            }

            /// <summary>
            /// Adds the given bind group to the page.
            /// </summary>
            /// <param name="isAliased">Exposes bind aliases for group if true</param>
            public void Add(IBindGroup bindGroup, bool isAliased = false)
            {
                GetOrSetMemberFunc(new MyTuple<object, BindDefinitionData[], bool>(bindGroup.ID, null, isAliased), (int)RebindPageAccessors.Add);
                bindGroups.Add(bindGroup);
            }

            /// <summary>
            /// Adds the given bind group to the page along with its associated default configuration.
            /// </summary>
            /// <param name="isAliased">Exposes bind aliases for group if true</param>
            public void Add(IBindGroup bindGroup, BindDefinition[] defaultBinds, bool isAliased = false)
            {
                BindDefinitionData[] data = new BindDefinitionData[defaultBinds.Length];

                for (int n = 0; n < defaultBinds.Length; n++)
                    data[n] = (BindDefinitionData)defaultBinds[n];

                GetOrSetMemberFunc(new MyTuple<object, BindDefinitionData[], bool>(bindGroup.ID, data, isAliased), (int)RebindPageAccessors.Add);
                bindGroups.Add(bindGroup);
            }

            public IEnumerator<IBindGroup> GetEnumerator() =>
                bindGroups.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                bindGroups.GetEnumerator();
        }
    }
}