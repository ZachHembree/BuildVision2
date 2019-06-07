using System;

namespace DarkHelmet
{
    /// <summary>
    /// Generic singleton. Types inheriting from this class cannot be instantiated externally (public constructor or not).
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T Instance
        {
            get { if (instance == null) Init(); return instance; }
            protected set { instance = value; }
        }

        protected static bool canInstantiate = false;
        private static T instance;

        protected Singleton()
        {
            if (!canInstantiate)
                throw new Exception("Types of Singleton<T> cannot be instantiated externally.");

            canInstantiate = false;
        }

        public static void Init()
        {
            if (instance == null)
            {
                canInstantiate = true;
                instance = new T();
                instance.AfterInit();
            }
        }

        protected virtual void AfterInit() { }

        protected virtual void BeforeClose() { }

        public static void Close()
        {
            if (Instance != null)
            {
                Instance.BeforeClose();
                Instance = null;
            }
        }
    }
}
