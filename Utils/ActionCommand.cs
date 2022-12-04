using System;
using System.Windows.Input;
using GridSetter.Utils.Interfaces;

namespace GridSetter.Utils
{
    /// <summary>
    /// Implementation of an ActionCommand that can be bounded to a View via a ViewModel
    /// </summary>
    public class ActionCommand : ICommand
    {
        #region Static variables

        /// <summary>
        /// A reference to the platform specific dispatcher.
        /// </summary>
        private static IDispatcher s_Dispatcher;

        #endregion Static variables

        #region Static constructor

        /// <summary>
        /// <see cref="ActionCommand"/> static constructor.
        /// </summary>
        static ActionCommand()
        {
            s_Dispatcher = DependencyInjectionUtil.Resolve<IDispatcher>();
        }


        #endregion // Static constructor

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion // Events

        #region Instance variables

        private readonly Action _handler;

        private bool _isEnabled;

        #endregion // Instance variables

        #region Properties

        /// <summary>
        /// Set or get the execution status of the command
        /// Trigger an event if the value changed in order the caller can process necessary updates
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    if (CanExecuteChanged != null)
                    {
                        s_Dispatcher.Invoke(() => CanExecuteChanged(this, EventArgs.Empty));
                    }
                }
            }
        }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// <see cref="ActionCommand"/> constructor.
        /// </summary>
        /// <param name="mHandler">Action to be executed</param>
        /// <param name="mIsEnabled">Define if the command is enabled or not. Default value is true</param>
        public ActionCommand(Action mHandler, bool mIsEnabled = true)
        {
            _handler = mHandler;
            _isEnabled = mIsEnabled;
        }

        #endregion // Constructors

        #region Methods

        /// <summary>
        /// Define if the command the command can be executed or not
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>return true or false</returns>
        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public void Execute(object parameter)
        {
            IsEnabled = false;
            s_Dispatcher.Invoke(_handler);
            IsEnabled = true;
        }

        #endregion // Methods
    }
}
