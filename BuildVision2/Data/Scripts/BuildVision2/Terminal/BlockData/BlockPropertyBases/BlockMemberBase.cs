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
            /// Retrieves the name of the block property
            /// </summary>
            public virtual StringBuilder Name { get; protected set; }

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public abstract StringBuilder Display { get; }

            /// <summary>
            /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
            /// </summary>
            public virtual StringBuilder Value => Display;

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public abstract StringBuilder Status { get; }

            /// <summary>
            /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
            /// </summary>
            public virtual bool Enabled { get; protected set; }

            public abstract void Reset();

            public abstract void Return();
        }

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