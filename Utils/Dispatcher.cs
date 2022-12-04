using System;
using System.ComponentModel;
using System.Threading;
using GridSetter.Utils.Interfaces;

namespace GridSetter.Utils
{
    /// <summary>
    /// Dispatcher class.
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        #region Instance variables

        /// <summary>
        /// The WPF UI dispatcher used to execute the specified actions on the UI thread. 
        /// </summary>
        private readonly System.Windows.Threading.Dispatcher _dispatcher;

        #endregion // Instance variables

        #region Constructors

        /// <summary>
        /// WpfDispatcher constructor.
        /// </summary>
        /// <param name="uiThread">The UI thread.</param>
        public Dispatcher(Thread uiThread)
        {
            _dispatcher = System.Windows.Threading.Dispatcher.FromThread(uiThread);
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The handler used to launch the event.</param>
        /// <param name="propertyName">The name of the property which has been modified.</param>
        public void Invoke(object source, PropertyChangedEventHandler handler, string propertyName)
        {
            _dispatcher.Invoke(() =>
            {
                handler(source, new PropertyChangedEventArgs(propertyName));
            });
        }

        /// <summary>
        /// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void Invoke(Action action)
        {
            _dispatcher.Invoke(action);
        }

        #endregion //Methods
    }
}
