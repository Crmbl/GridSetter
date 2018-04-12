using System;
using System.ComponentModel;

namespace GridSetter.Utils.Interfaces
{
	/// <summary>
	/// IDispatcher class.
	/// </summary>
	public interface IDispatcher
	{
		/// <summary>
		/// Invoke OnPropertyChanged on the UI Thread using the platform specific dispatcher.
		/// </summary>
		/// <param name="source">sender</param>
		/// <param name="handler">action</param>
		/// <param name="propertyName">name of the property which has changed</param>
		void Invoke(object source, PropertyChangedEventHandler handler, string propertyName);

		/// <summary>
		/// Executes the specified delegate synchronously on the thread the Dispatcher is associated with.
		/// </summary>
		/// <param name="action">The action to execute.</param>
		void Invoke(Action action);
	}
}
