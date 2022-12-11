using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GridSetter.Utils;
using GridSetter.Views;
using Newtonsoft.Json;
using WpfAnimatedGif;
using static System.Net.WebRequestMethods;
using static GridSetter.Views.MainWindow;
using GGrid = GridSetter.Views.Grid;
using WGrid = System.Windows.Controls.Grid;

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
		/// Export grid on command.
		/// </summary>
		public ActionCommand ExportGridCommand { get; set; }

		/// <summary>
		/// Command to remove all content from every cells.
		/// </summary>
		public ActionCommand EmptyContentCommand { get; set; }

		/// <summary>
		/// Get the MainWindow ref.
		/// </summary>
		public MainWindow ParentRef { get; set; }

		/// <summary>
		/// Keep in memory the grid.
		/// </summary>
		public GGrid CurrentGrid { get; set; }

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
		public GridSetterViewModel(MainWindow parentRef)
		{
			AppTitle = "Grid Setter";
#if DEBUG
			Version = "1.5d";
#else
			Version = "1.5r";
#endif

			ParentRef = parentRef;
			ToggleLockLabel = "Lock";
			ResetNewLabel = "New";

			ExitCommand = new ActionCommand(() => Application.Current.Shutdown());
			ResetCommand = new ActionCommand(ResetGrid);
			ToggleLockCommand = new ActionCommand(() => ToggleLockGrid());
			ExportGridCommand = new ActionCommand(ExportGrid);
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
				CurrentGrid = new GGrid(this);
				CurrentGrid.WindowState = WindowState.Minimized;
				CurrentGrid.Show();
				CurrentGrid.WindowState = WindowState.Maximized;
			}
			else if (ResetNewLabel.Equals("Reset"))
			{
				ToggleLockLabel = "Lock";
				CurrentGrid.Close();
				CurrentGrid = new GGrid(this);
				CurrentGrid.WindowState = WindowState.Minimized;
				CurrentGrid.Show();
				CurrentGrid.WindowState = WindowState.Maximized;
			}

			OnGridCreatedEvent(EventArgs.Empty);
		}

		/// <summary>
		/// Toggle on/off the lock of the grid.
		/// </summary>
		public void ToggleLockGrid(bool force = false)
		{
			if (ResetNewLabel.Equals("New") && !force)
				return;

			if (ToggleLockLabel.Equals("Lock") || force)
			{
				UserInterfaceTools.ToggleLockControlButtons(CurrentGrid.MainGrid, true);
				ToggleLockLabel = "Unlock";

				if (force)
					ResetNewLabel = "Reset";
			}
			else if (ToggleLockLabel.Equals("Unlock"))
			{
				UserInterfaceTools.ToggleLockControlButtons(CurrentGrid.MainGrid, false);
				ToggleLockLabel = "Lock";
			}

			ToggleStateChangedEvent?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Export grid to a file.
		/// </summary>
		public void ExportGrid()
		{
			if (CurrentGrid == null) return;

			var i = 0;
			var exists = false;
			var filename = $"{System.Reflection.Assembly.GetEntryAssembly().Location.Replace("GridSetter.exe", "")}\\Imports\\grid-template.json";
			do
			{
				if (System.IO.File.Exists(filename))
				{
					exists = true;
					filename = $"{filename.Replace($"grid-template{(i == 0 ? "" : i.ToString())}.json", $"grid-template{++i}.json")}";
				}
				else
					exists = false;
			}
			while (exists);

			var mediaCells = CurrentGrid.MainGrid.Children.Cast<FrameworkElement>().Where(e => e is Canvas).ToList();
			using (StreamWriter sw = System.IO.File.CreateText(filename))
			{
				var jsonCells = new GridViewModel(CurrentGrid.MainGrid.ColumnDefinitions.Count, CurrentGrid.MainGrid.RowDefinitions.Count);
				foreach (Canvas cell in mediaCells.OrderBy(x => WGrid.GetRow(x)).ThenBy(x => WGrid.GetColumn(x)))
				{
					var width = cell.ActualWidth;
					var height = cell.ActualHeight;
					var col = WGrid.GetColumn(cell);
					var row = WGrid.GetRow(cell);
					var colSpan = WGrid.GetColumnSpan(cell);
					var rowSpan = WGrid.GetRowSpan(cell);

					var source = string.Empty;
					var image = (Image)cell.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
					var video = (MediaElement)cell.Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement);
					if (image?.Source != null)
						source = ImageBehavior.GetAnimatedSource(image).ToString();
					if (video?.Source != null)
						source = video?.Source.ToString();
					if (width == 0.0 || height == 0.0)
					{
						var setupGrid = CurrentGrid.MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => WGrid.GetRow(u) == row && WGrid.GetColumn(u) == col && u is WGrid _control && _control.Name.Equals("SetupGrid")) as WGrid;
						width = setupGrid.ActualWidth;
						height = setupGrid.ActualHeight;
					}

					var innerCells = new List<InnerCellViewModel>();
					for (var z = col; z < col + colSpan; z++)
						for (var w = row; w < row + rowSpan; w++)
                            if (z % 2 == 0 && w % 2 == 0)
                                innerCells.Add(new InnerCellViewModel(CurrentGrid.MainGrid.ColumnDefinitions[z].ActualWidth, CurrentGrid.MainGrid.RowDefinitions[w].ActualHeight, z, w));

                    jsonCells.Cells.Add(new CellViewModel(width, height, col, row, colSpan, rowSpan, source, innerCells));
				}

				sw.WriteLine(JsonConvert.SerializeObject(jsonCells, Formatting.Indented));
			}

			ParentRef.DropDownImport.DropDownContent = GetDropDownContent();
		}

		/// <summary>
		/// Setup the list for the Import grid dropdown button.
		/// </summary>
		public Canvas GetDropDownContent()
		{
			var exeDir = new DirectoryInfo($"{System.Reflection.Assembly.GetEntryAssembly().Location.Replace("GridSetter.exe", "")}\\Imports");
			var files = exeDir.GetFiles("grid-template*.json");

			var brushConverter = new System.Windows.Media.BrushConverter();
			var canvas = new Canvas()
			{
				Width = 197,
				Height = files.Length * 22,
				Background = (System.Windows.Media.Brush)brushConverter.ConvertFrom("#dddddddd")
			};

			for (var i = 0; i < files.Length; i++)
			{
				var button = new Button()
				{
                    Style = ParentRef.FindResource("DropDownButtonImageBase") as Style,
                    Tag = Application.Current.Resources["CrossImage"] as BitmapImage
                };
				var textBlock = new TextBlock
				{
					Text = files[i].Name,
					Style = ParentRef.FindResource("DropDownTextBlockStyle") as Style
				};

				var margin = textBlock.Margin;
				margin.Top = i == 0 ? 2 : 22 * i;
				textBlock.Margin = margin;
                var marginBtn = button.Margin;
                marginBtn.Top = i == 0 ? 2 : 22 * i;
                button.Margin = marginBtn;

                var mouseBinding = new MouseBinding(new FileSelectionCommand() { ViewModel = this }, new MouseGesture(MouseAction.LeftClick)) { CommandParameter = files[i].Name };
				textBlock.InputBindings.Add(mouseBinding);
				var mouseBindingBtn = new MouseBinding(new FileDeletionCommand() { ViewModel = this }, new MouseGesture(MouseAction.LeftClick)) { CommandParameter = files[i].Name };
				button.InputBindings.Add(mouseBindingBtn);

                canvas.Children.Add(button);
				canvas.Children.Add(textBlock);
			}

			return canvas;
		}

		/// <summary>
		/// Import grid from file and create it accordingly.
		/// </summary>
		/// <param name="fileName">The exported grid.</param>
		public void ImportGrid(string fileName)
		{
			var fullPath = $"{System.Reflection.Assembly.GetEntryAssembly().Location.Replace("GridSetter.exe", "")}\\Imports\\{fileName}";
			if (!System.IO.File.Exists(fullPath))
				return;

			GridViewModel json;
			using (StreamReader sr = System.IO.File.OpenText(fullPath))
			{
				json = JsonConvert.DeserializeObject<GridViewModel>(sr.ReadToEnd());
			}

			if (CurrentGrid != null)
				CurrentGrid.Close();

			CurrentGrid = new GGrid(this, json);
			CurrentGrid.WindowState = WindowState.Minimized;
			CurrentGrid.Show();
			CurrentGrid.WindowState = WindowState.Maximized;
			ToggleLockGrid(true);

			foreach (var cell in json.Cells) //Add content and resize rows/cols
			{
                foreach (var innerCell in cell.InnerCells)
                {
                    CurrentGrid.MainGrid.ColumnDefinitions[innerCell.Col].Width = new GridLength(innerCell.Width, GridUnitType.Star);
                    CurrentGrid.MainGrid.RowDefinitions[innerCell.Row].Height = new GridLength(innerCell.Height, GridUnitType.Star);
                }

				if (!string.IsNullOrWhiteSpace(cell.Source))
				{
					var mediaCanvas = CurrentGrid.MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => WGrid.GetRow(u) == cell.Row && WGrid.GetColumn(u) == cell.Col && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
					MediaControlTools.AddMedia(mediaCanvas, fileName: cell.Source, grid: CurrentGrid);
				}
			}
		}

        /// <summary>
        /// Delete the json file used to import.
        /// </summary>
        /// <param name="fileName">The exported grid to delete.</param>
        public void DeleteJson(string fileName)
		{
            var fullPath = $"{System.Reflection.Assembly.GetEntryAssembly().Location.Replace("GridSetter.exe", "")}\\Imports\\{fileName}";
			System.IO.File.Delete(fullPath);
            ParentRef.DropDownImport.DropDownContent = GetDropDownContent();
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
