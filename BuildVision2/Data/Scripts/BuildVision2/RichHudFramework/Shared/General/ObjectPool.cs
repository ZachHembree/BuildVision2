using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using VRage;
using System;

namespace RichHudFramework
{
    public interface IPooledObjectPolicy<T>
    {
        T GetNewObject();

        void ResetObject(T obj);
    }

    public class PooledObjectPolicy<T> : IPooledObjectPolicy<T>
    {
        private readonly Func<T> GetNewObjectFunc;
        private readonly Action<T> ResetObjectAction;

        public PooledObjectPolicy(Func<T> GetNewObjectFunc, Action<T> ResetObjectAction)
        {
            if (GetNewObjectFunc == null || ResetObjectAction == null)
                throw new Exception("Neither GetNewObjectFunc nor ResetObjectAction can be null.");
        
            this.GetNewObjectFunc = GetNewObjectFunc;
            this.ResetObjectAction = ResetObjectAction;
        }

        public T GetNewObject()
        {
            return GetNewObjectFunc();
        }

        public void ResetObject(T obj)
        {
            ResetObjectAction(obj);
        }
    }

    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> pooledObjects;
        private readonly IPooledObjectPolicy<T> objectPolicy;

        public ObjectPool(IPooledObjectPolicy<T> objectPolicy)
        {
            if (objectPolicy == null)
                throw new Exception("Pooled object policy cannot be null.");

            pooledObjects = new ConcurrentBag<T>();
            this.objectPolicy = objectPolicy;
        }

        public ObjectPool(Func<T> GetNewObjectFunc, Action<T> ResetObjectAction)
        {
            if (GetNewObjectFunc == null || ResetObjectAction == null)
                throw new Exception("Neither GetNewObjectFunc nor ResetObjectAction can be null.");

            this.pooledObjects = new ConcurrentBag<T>();
            this.objectPolicy = new PooledObjectPolicy<T>(GetNewObjectFunc, ResetObjectAction);
        }

        /// <summary>
        /// Removes and returns and object from the pool or creates
        /// a new one if none are available.
        /// </summary>
        public T Get()
        {
            T obj;

            if (!pooledObjects.TryTake(out obj))
                obj = objectPolicy.GetNewObject();

            return obj;
        }

        /// <summary>
        /// Adds the given object back to the pool for later reuse.
        /// </summary>
        public void Return(T obj)
        {
            objectPolicy.ResetObject(obj);
            pooledObjects.Add(obj);
        }
    }
}