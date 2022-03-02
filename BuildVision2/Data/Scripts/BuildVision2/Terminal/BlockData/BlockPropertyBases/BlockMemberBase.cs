using RichHudFramework;
using System.Collections.Generic;
using VRage;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        private abstract class BlockMemberBase : IBlockMember
        {
            /// <summary>
            /// Unique identifier associated with the property
            /// </summary>
            public abstract string PropName { get; }

            /// <summary>
            /// Retrieves the name of the block property
            /// </summary>
            public virtual StringBuilder Name { get; protected set; }

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public abstract StringBuilder FormattedValue { get; }

            /// <summary>
            /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
            /// </summary>
            public virtual StringBuilder ValueText => FormattedValue;

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public abstract StringBuilder StatusText { get; }

            /// <summary>
            /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
            /// </summary>
            public virtual bool Enabled { get; protected set; }

            /// <summary>
            /// Returns the type of data stored by the block member, if any.
            /// </summary>
            public virtual BlockMemberValueTypes ValueType { get; protected set; }

            /// <summary>
            /// Synchronizes the state of the block member with the associated terminal properties
            /// </summary>
            public abstract void Update(bool sync);

            public abstract void Reset();

            public abstract void Return();
        }

        /// <summary>
        /// Manages pool of property block members
        /// </summary>
        private class BvPropPool<T> : ObjectPool<T>
            where T : BlockMemberBase, new()
        {
            public BvPropPool() : base(new BvPropPoolPolicy())
            { }

            private class BvPropPoolPolicy : IPooledObjectPolicy<T>
            {
                public virtual T GetNewObject()
                {
                    return new T();
                }

                public virtual void ResetObject(T obj)
                {
                    obj.Reset();
                }

                public virtual void ResetRange(IReadOnlyList<T> objects, int index, int count)
                {
                    for (int i = 0; (i + index) < objects.Count; i++)
                        objects[i].Reset();
                }

                public virtual void ResetRange<T2>(IReadOnlyList<MyTuple<T, T2>> objects, int index, int count)
                {
                    for (int i = 0; (i + index) < objects.Count; i++)
                        objects[i].Item1.Reset();
                }
            }
        }
    }
}