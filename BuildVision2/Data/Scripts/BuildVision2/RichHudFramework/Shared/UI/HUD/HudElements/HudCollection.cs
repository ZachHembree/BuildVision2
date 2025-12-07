using System;
using System.Collections;
using System.Collections.Generic;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Generic collection of HUD elements, each wrapped in a decorator container.
		/// Elements inside containers are parented directly to this collection (not to their containers).
		/// Supports full IList-like manipulation while ensuring proper registration/unregistration with the HUD tree.
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		/// <typeparam name="TElement">
		/// Actual HUD element type stored in each container.
		/// </typeparam>
		public class HudCollection<TElementContainer, TElement> : HudElementBase, IHudCollection<TElementContainer, TElement>
			where TElementContainer : IHudNodeContainer<TElement>, new()
			where TElement : HudNodeBase
		{
			/// <summary>
			/// Read-only access to the list of element containers in this collection.
			/// </summary>
			public IReadOnlyList<TElementContainer> Collection { get; }

			/// <summary>
			/// Enables collection-initializer syntax (e.g., new HudCollection { element1, element2 }).
			/// Returns this instance so initializers can chain with container additions.
			/// </summary>
			public HudCollection<TElementContainer, TElement> CollectionContainer => this;

			/// <summary>
			/// Indexer providing access to the container at the specified position.
			/// </summary>
			public TElementContainer this[int index]
			{
				get
				{
					if (hudCollectionList.Count == 0 || index < 0 || index >= hudCollectionList.Count)
						throw new Exception($"Collection index out of range. Index: {index} Count: {hudCollectionList.Count}");

					return hudCollectionList[index];
				}
			}

			/// <summary>
			/// Number of containers currently in the collection.
			/// </summary>
			public int Count => hudCollectionList.Count;

			/// <summary>
			/// Always false — this collection is mutable.
			/// </summary>
			public bool IsReadOnly { get; }

			/// <summary>
			/// Internal backing list holding the containers.
			/// </summary>
			/// <exclude/>
			protected readonly List<TElementContainer> hudCollectionList;

			public HudCollection(HudParentBase parent = null) : base(parent)
			{
				IsReadOnly = false;
				hudCollectionList = new List<TElementContainer>();
				Collection = hudCollectionList;
			}

			public IEnumerator<TElementContainer> GetEnumerator() => hudCollectionList.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			/// <summary>
			/// Adds a raw <typeparamref name="TElement"/> by automatically wrapping it in a new <typeparamref name="TElementContainer"/>.
			/// </summary>
			public virtual void Add(TElement element)
			{
				var container = new TElementContainer();
				container.SetElement(element);
				Add(container);
			}

			/// <summary>
			/// Adds a pre-constructed container to the end of the collection.
			/// The container's element will be registered as a child of this collection.
			/// </summary>
			/// <exception cref="Exception">Thrown if the element is already registered elsewhere or registration fails.</exception>
			public virtual void Add(TElementContainer container)
			{
				if (container.Element.Registered)
					throw new Exception("HUD element is already registered to another parent.");

				if (!container.Element.Register(this))
					throw new Exception("Failed to register HUD element with this collection.");

				hudCollectionList.Add(container);
			}

			/// <summary>
			/// Adds multiple pre-constructed containers to the end of the collection in a single operation.
			/// All elements are registered as children of this collection.
			/// </summary>
			public virtual void AddRange(IReadOnlyList<TElementContainer> newContainers)
			{
				NodeUtils.RegisterNodes<TElementContainer, TElement>(this, newContainers);
				hudCollectionList.AddRange(newContainers);
			}

			/// <summary>
			/// Inserts a container at the specified index.
			/// </summary>
			public virtual void Insert(int index, TElementContainer container)
			{
				if (!container.Element.Register(this))
					throw new Exception("Failed to register HUD element with this collection.");

				hudCollectionList.Insert(index, container);
			}

			/// <summary>
			/// Inserts a range of containers starting at the specified index.
			/// </summary>
			public virtual void InsertRange(int index, IReadOnlyList<TElementContainer> newContainers)
			{
				NodeUtils.RegisterNodes<TElementContainer, TElement>(this, newContainers);
				hudCollectionList.InsertRange(index, newContainers);
			}

			/// <summary>
			/// Removes the specified container if it belongs to this collection.
			/// </summary>
			/// <returns>true if the container was removed and its element unregistered successfully.</returns>
			public virtual bool Remove(TElementContainer entry)
			{
				if (entry?.Element.Parent != this || hudCollectionList.Count == 0)
					return false;

				if (hudCollectionList.Remove(entry))
					return entry.Element.Unregister();

				return false;
			}

			/// <summary>
			/// Removes the first container that matches the given predicate.
			/// </summary>
			/// <returns>true if a matching container was found and removed.</returns>
			public virtual bool Remove(Func<TElementContainer, bool> predicate)
			{
				int index = hudCollectionList.FindIndex(x => predicate(x));

				if (index == -1)
					return false;

				var element = hudCollectionList[index].Element;
				hudCollectionList.RemoveAt(index);
				return element.Unregister();
			}

			/// <summary>
			/// Removes the container at the specified index.
			/// </summary>
			/// <returns>true if removal and unregistration succeeded.</returns>
			public virtual bool RemoveAt(int index)
			{
				if (index < 0 || index >= hudCollectionList.Count || hudCollectionList[index].Element.Parent != this)
					return false;

				var element = hudCollectionList[index].Element;
				hudCollectionList.RemoveAt(index);
				return element.Unregister();
			}

			/// <summary>
			/// Removes a contiguous range of containers starting at <paramref name="index"/>.
			/// Only affects collection members — regular (non-collection) children are untouched.
			/// </summary>
			public virtual void RemoveRange(int index, int count)
			{
				NodeUtils.UnregisterNodes<TElementContainer, TElement>(this, hudCollectionList, index, count);
				hudCollectionList.RemoveRange(index, count);
			}

			/// <summary>
			/// Removes all containers from the collection.
			/// Regular child elements (added directly via normal parenting) are not affected.
			/// </summary>
			public virtual void Clear()
			{
				NodeUtils.UnregisterNodes<TElementContainer, TElement>(this, hudCollectionList, 0, hudCollectionList.Count);
				hudCollectionList.Clear();
			}

			/// <summary>
			/// Returns the first container matching the predicate, or default(<typeparamref name="TElementContainer"/>) if none found.
			/// </summary>
			public virtual TElementContainer Find(Func<TElementContainer, bool> predicate)
				=> hudCollectionList.Find(x => predicate(x));

			/// <summary>
			/// Returns the index of the first container matching the predicate, or -1 if none found.
			/// </summary>
			public virtual int FindIndex(Func<TElementContainer, bool> predicate)
				=> hudCollectionList.FindIndex(x => predicate(x));

			/// <summary>
			/// Determines whether the collection contains the specified container.
			/// </summary>
			public virtual bool Contains(TElementContainer item) => hudCollectionList.Contains(item);

			/// <summary>
			/// Copies the collection's containers to an array starting at the specified index.
			/// </summary>
			public virtual void CopyTo(TElementContainer[] array, int arrayIndex)
				=> hudCollectionList.CopyTo(array, arrayIndex);

			/// <summary>
			/// Overrides <see cref="HudParentBase.RemoveChild"/> to handle removal of elements that were added via the collection.
			/// Ensures both the container list and regular child tracking stay in sync.
			/// </summary>
			public override bool RemoveChild(HudNodeBase child)
			{
				if (child.Parent == this)
				{
					bool success = child.Unregister();
					if (success)
						RemoveChild(child);
					return success;
				}
				else if (child.Parent == null && children.Remove(child))
				{
					childHandles.Remove(child.DataHandle);

					for (int n = 0; n < hudCollectionList.Count; n++)
					{
						if (hudCollectionList[n].Element == child)
						{
							hudCollectionList.RemoveAt(n);
							break;
						}
					}

					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Generic collection of HUD elements, each wrapped in a decorator container.
		/// Elements inside containers are parented directly to this collection (not to their containers).
		/// Supports full IList-like manipulation while ensuring proper registration/unregistration with the HUD tree.
		/// <para>
		/// Alias for <see cref="HudCollection{TElementContainer, TElement}"/> where the element type is 
		/// <see cref="HudElementBase"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		public class HudCollection<TElementContainer> : HudCollection<TElementContainer, HudElementBase>
			where TElementContainer : IHudNodeContainer<HudElementBase>, new()
		{
			public HudCollection(HudParentBase parent = null) : base(parent) { }
		}

		/// <summary>
		/// Generic collection of HUD elements, each wrapped in a decorator container.
		/// Elements inside containers are parented directly to this collection (not to their containers).
		/// Supports full IList-like manipulation while ensuring proper registration/unregistration with the HUD tree.
		/// <para>
		/// Alias for <see cref="HudCollection{TElementContainer, TElement}"/> using 
		/// <see cref="HudElementContainer"/> as the wrapper and <see cref="HudElementBase"/> as the element type.
		/// </para>
		/// </summary>
		public class HudCollection : HudCollection<HudNodeContainer, HudNodeBase>
		{
			public HudCollection(HudParentBase parent = null) : base(parent) { }
		}
	}
}