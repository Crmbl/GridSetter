using System;
using GridSetter.Utils;
using GridSetter.Views;

namespace GridSetter.ViewModels
{
	/// <summary>
	/// MainViewModel of the app.
	/// </summary>
	public class GridSetterViewModel : BaseViewModel
	{
		#region Instance Variables

		/// <summary>
		/// Private variable for <see cref="AppTitle"/>.
		/// </summary>
		private string _appTitle;

		/// <summary>
		/// Private variable for <see cref="Version"/>.
		/// </summary>
		private string _version;

		/// <summary>
		/// Private variable for the reset/new grid button.
		/// </summary>
		private string _resetNewLabel;

		/// <summary>
		/// Private variable for the toggle lock button.
		/// </summary>
		private string _toggleLockLabel;

		#endregion // Instance Variables

		#region Properties

		/// <summary>
		/// Defines the app title.
		/// </summary>
		public string AppTitle
		{
			get => _appTitle;
			set
			{
				if (_appTitle == value) return;
				_appTitle = value;
				NotifyPropertyChanged("AppTitle");
			}
		}

		/// <summary>
		/// The version of the app.
		/// </summary>
		public string Version
		{
			get => _version;
			set
			{
				if (_version == value) return;
				_version = value;
				NotifyPropertyChanged("Version");
			}
		}

		/// <summary>
		/// The label of the toggle lock button.
		/// </summary>
		public string ToggleLockLabel
		{
			get => _toggleLockLabel;
			set
			{
				if (_toggleLockLabel == value) return;
				_toggleLockLabel = value;
				NotifyPropertyChanged("ToggleLockLabel");
			}
		}

		/// <summary>
		/// The label of the reset/new grid button.
		/// </summary>
		public string ResetNewLabel
		{
			get => _resetNewLabel;
			set
			{
				if (_resetNewLabel == value) return;
				_resetNewLabel = value;
				NotifyPropertyChanged("ResetNewLabel");
			}
		}

		/// <summary>
		/// Exit the application on command.
		/// </summary>
		public ActionCommand ExitCommand { get; set; }

		/// <summary>
		/// Reset the grid on command.
		/// </summary>
		public ActionCommand ResetCommand { get; set; }

		/// <summary>
		/// Toggle the grid lock on command.
		/// </summary>
		public ActionCommand ToggleLockCommand { get; set; }

		/// <summary>
		/// Command to remove all content from every cells.
		/// </summary>
		public ActionCommand EmptyContentCommand { get; set; }

		/// <summary>
		/// Keep in memory the grid.
		/// </summary>
		public Grid CurrentGrid { get; set; }

		/// <summary>
		/// Event when the grid has been created.
		/// </summary>
		public event EventHandler NewCreatedEvent;

        /// <summary>
        /// Event when the toggle state changed.
        /// </summary>
	    public event EventHandler ToggleStateChangedEvent;

		#endregion // Properties

		#region Constructors

		/// <summary>
		/// Default parameterless constructor.
		/// </summary>
		public GridSetterViewModel()
		{
			AppTitle = "Grid Setter";
			#if DEBUG
			Version = "1.0d";
			#else
			Version = "1.0r";
			#endif

			ToggleLockLabel = "Lock";
			ResetNewLabel = "New";

			ExitCommand = new ActionCommand(() => System.Windows.Application.Current.Shutdown());
			ResetCommand = new ActionCommand(ResetGrid);
			ToggleLockCommand = new ActionCommand(ToggleLockGrid);
			EmptyContentCommand = new ActionCommand(EmptyAllContent);
		}

		#endregion // Constructors

		#region Events

		/// <summary>
		/// Raised when grid value changed.
		/// </summary>
		public virtual void OnGridCreatedEvent(EventArgs e)
		{
		    NewCreatedEvent?.Invoke(this, e);
		}

		#endregion // Events

		#region Methods

		/// <summary>
		/// Create or reset the grid on click.
		/// </summary>
		public void ResetGrid()
		{
			if (ResetNewLabel.Equals("New"))
			{
				ResetNewLabel = "Reset";
				CurrentGrid = new Grid(this);
				CurrentGrid.Show();
			}
			else if (ResetNewLabel.Equals("Reset"))
			{
				ToggleLockLabel = "Lock";
				CurrentGrid.Close();
				CurrentGrid = new Grid(this);
				CurrentGrid.Show();
			}

			OnGridCreatedEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Toggle on/off the lock of the grid.
		/// </summary>
		public void ToggleLockGrid()
		{
			if (ResetNewLabel.Equals("New"))
				return;

			if (ToggleLockLabel.Equals("Lock"))
			{
				UserInterfaceTools.ToggleLockControlButtons(CurrentGrid.MainGrid, true);
				ToggleLockLabel = "Unlock";
			}
			else if (ToggleLockLabel.Equals("Unlock"))
			{
				UserInterfaceTools.ToggleLockControlButtons(CurrentGrid.MainGrid, false);
				ToggleLockLabel = "Lock";
			}

		    ToggleStateChangedEvent?.Invoke(this, EventArgs.Empty);
        }

		/// <summary>
		/// Remove content from every cell on click.
		/// </summary>
		public void EmptyAllContent()
		{
			if (ResetNewLabel.Equals("New"))
				return;

			UserInterfaceTools.RemoveContentFromGrid(CurrentGrid.MainGrid);
		}

		#endregion // Methods
	}
}
