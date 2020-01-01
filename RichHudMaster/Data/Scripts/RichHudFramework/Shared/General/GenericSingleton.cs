using System;

namespace RichHudFramework
{
    /// <summary>
    /// Generic singleton. Types inheriting from this class cannot be instantiated externally (public constructor or not).
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T Instance
        {
            get { Init(); return instance; }
            protected set { instance = value; }
        }

        private static T instance;
        private static bool initializing;

        protected Singleton()
        {
            if (instance != null)
                throw new Exception("Types of Singleton<T> cannot be instantiated externally.");
        }

        public static void Init()
        {
            if (instance == null && !initializing)
            {
                initializing = true;
                instance = new T();
                instance.AfterInit();
                initializing = false;
            }
        }

        protected virtual void AfterInit() { }
    }
}
