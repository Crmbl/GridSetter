using System;
using Unity;
using Unity.Injection;

namespace GridSetter.Utils
{
    /// <summary>
    /// DependencyInjectionUtil static class is used to register and resolve application dependencies.
    /// </summary>
    public static class DependencyInjectionUtil
    {
        #region Static variables

        /// <summary>
        /// The 
        /// </summary>
        private static readonly IUnityContainer Container;

        #endregion // Static variables 

        #region Static constructor

        /// <summary>
        /// DependencyInjectionUtil static constructor.
        /// </summary>
        static DependencyInjectionUtil()
        {
            // Initialization
            Container = new UnityContainer();
        }

        #endregion // Static constructor

        #region Static methods

        #region Bindings

        /// <summary>
        /// Bind TFrom base class or interface to TTo inherited or implementation class.
        /// For each TFrom resolution a new instance of TTo will be created.
        /// </summary>
        /// <typeparam name="TFrom">The base class or interface type.</typeparam>
        /// <typeparam name="TTo">An inherited or implementation class of TFrom.</typeparam>
        public static void Register<TFrom, TTo>() where TTo : TFrom, new()
        {
            Register<TFrom, TTo>(() => new TTo());
        }

        /// <summary>
        /// Bind TFrom base class or interface to TTo inherited or implementation class.
        /// For each TFrom resolution a new instance of TTo will be created. 
        /// The dependencies container used the specified creation method to create new instances of TTo.
        /// </summary>
        /// <typeparam name="TFrom">The base class or interface type.</typeparam>
        /// <typeparam name="TTo">An inherited or implementation class of TFrom.</typeparam>
        /// <param name="createMethod">The function used to create an instance of TTo.</param>
        private static void Register<TFrom, TTo>(Func<TTo> createMethod) where TTo : TFrom
        {
            Container.RegisterType<TFrom, TTo>(typeof(TFrom).Name, new InjectionFactory(container => createMethod()));
        }

        /// <summary>
        /// Register an instance using type name as default identifier
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public static void RegisterInstance<T>(T instance) where T : class
        {
            RegisterInstance(typeof(T).Name, instance);
        }

        /// <summary>
        /// Register an instance using specific string identifier
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="instance"></param>
        private static void RegisterInstance<T>(string identifier, T instance) where T : class
        {
            Container.RegisterInstance(identifier, instance);
        }

        /// <summary>
        /// Indicates that the dependency resolution of T must return the unique instance of T create by the specified creation smethode.
        /// </summary>
        /// <typeparam name="T">The type of the element to register.</typeparam>
        /// <param name="createMethod">The method used to create the unique instance of T.</param>
        public static void RegisterInstance<T>(Func<T> createMethod)
        {
            Container.RegisterInstance(createMethod());
        }

        #endregion // Bindings

        /// <summary>
        /// Resolve using type name as default identifier
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>() where T : class
        {
            return Container.Resolve<T>(typeof(T).Name);
        }

        /// <summary>
        /// Resolve the dependency identified by the specified identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static T Resolve<T>(string identifier) where T : class
        {
            return Container.Resolve<T>(identifier);
        }

        #endregion // Static methods
    }
}
