using System;
using System.Collections.Generic;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Read-only interface for collections of HUD elements wrapped in decorator containers.
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		/// <typeparam name="TElement">
		/// Actual HUD element type stored in each container.
		/// </typeparam>
		public interface IReadOnlyHudCollection<TElementContainer, TElement> : IReadOnlyList<TElementContainer>
			where TElementContainer : IHudNodeContainer<TElement>, new()
			where TElement : HudNodeBase
		{
			/// <summary>
			/// Read-only access to the underlying container list.
			/// </summary>
			IReadOnlyList<TElementContainer> Collection { get; }

			/// <summary>
			/// Returns the first container matching the predicate, or default if none found.
			/// </summary>
			TElementContainer Find(Func<TElementContainer, bool> predicate);

			/// <summary>
			/// Returns the index of the first matching container, or -1 if none found.
			/// </summary>
			int FindIndex(Func<TElementContainer, bool> predicate);
		}

		/// <summary>
		/// Read-only interface for collections of HUD elements wrapped in decorator containers.
		/// <para>
		/// Alias for <see cref="IReadOnlyHudCollection{TElementContainer, TElement}"/> with 
		/// <see cref="HudElementBase"/> as the element type.
		/// </para>
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		public interface IReadOnlyHudCollection<TElementContainer> : IReadOnlyHudCollection<TElementContainer, HudElementBase>
			where TElementContainer : IHudNodeContainer<HudElementBase>, new()
		{ }

		/// <summary>
		/// Default read-only collection interface using standard container and element types.
		/// <para>
		/// Alias for <see cref="IReadOnlyHudCollection{TElementContainer, TElement}"/> with 
		/// <see cref="HudElementBase"/> as the element type and <see cref="HudElementContainer"/> as the wrapper.
		/// </para>
		/// </summary>
		public interface IReadOnlyHudCollection : IReadOnlyHudCollection<HudElementContainer, HudElementBase>
		{ }

		/// <summary>
		/// Mutable interface for HUD collections supporting decorator-wrapped child elements.
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		/// <typeparam name="TElement">
		/// Actual HUD element type stored in each container.
		/// </typeparam>
		public interface IHudCollection<TElementContainer, TElement> : IReadOnlyHudCollection<TElementContainer, TElement>
			where TElementContainer : IHudNodeContainer<TElement>, new()
			where TElement : HudNodeBase
		{
			/// <summary>
			/// Adds a raw element by automatically creating and inserting a new container.
			/// </summary>
			void Add(TElement element);

			/// <summary>
			/// Adds a pre-built container to the end of the collection.
			/// </summary>
			void Add(TElementContainer container);

			/// <summary>
			/// Adds multiple containers in a single batch operation.
			/// </summary>
			void AddRange(IReadOnlyList<TElementContainer> newContainers);

			/// <summary>
			/// Inserts a container at the specified index.
			/// </summary>
			void Insert(int index, TElementContainer container);

			/// <summary>
			/// Inserts a range of containers starting at the given index.
			/// </summary>
			void InsertRange(int index, IReadOnlyList<TElementContainer> newContainers);

			/// <summary>
			/// Removes a specific container if it belongs to this collection.
			/// </summary>
			bool Remove(TElementContainer collectionElement);

			/// <summary>
			/// Removes the first container matching the predicate.
			/// </summary>
			bool Remove(Func<TElementContainer, bool> predicate);

			/// <summary>
			/// Removes the container at the specified index.
			/// </summary>
			bool RemoveAt(int index);

			/// <summary>
			/// Removes a range of containers. Does not affect regular (non-collection) children.
			/// </summary>
			void RemoveRange(int index, int count);

			/// <summary>
			/// Removes all containers from the collection. Regular children remain untouched.
			/// </summary>
			void Clear();
		}

		/// <summary>
		/// Mutable interface for HUD collections supporting decorator-wrapped child elements.
		/// <para>
		/// Alias of <see cref="IHudCollection{TElementContainer, TElement}"/> using 
		/// <see cref="HudElementBase"/> as the element type.
		/// </para>
		/// </summary>
		/// <typeparam name="TElementContainer">
		/// Type of the container/decorator wrapping each element. Must implement <see cref="IHudNodeContainer{TElement}"/>.
		/// </typeparam>
		public interface IHudCollection<TElementContainer> : IHudCollection<TElementContainer, HudElementBase>
			where TElementContainer : IHudNodeContainer<HudElementBase>, new()
		{ }

		/// <summary>
		/// Mutable interface for HUD collections supporting decorator-wrapped child elements.
		/// <para>
		/// Alias of <see cref="IHudCollection{TElementContainer, TElement}"/> using 
		/// <see cref="HudElementBase"/> as the element type and <see cref="HudElementContainer"/> as the wrapper.
		/// </para>
		/// </summary>
		public interface IHudCollection : IHudCollection<HudElementContainer, HudElementBase>
		{ }
	}
}