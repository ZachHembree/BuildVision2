using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using ControlMembers = MyTuple<string, string, int, Func<bool>, bool, ApiMemberAccessor>;

    namespace UI
    {
        /// <summary>
        /// Interface for anything used as a control
        /// </summary> 
        public interface IControl
        {
            string Name { get; }
            string DisplayName { get; }
            int Index { get; }
            bool IsPressed { get; }
            bool Analog { get; }
            ControlMembers GetApiData();
        }
    }
}