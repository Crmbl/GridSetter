using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GridSetter.Controls;
using GridSetter.Utils.Converters;
using GridSetter.Utils.Enums;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Panel = System.Windows.Controls.Panel;

namespace GridSetter.Utils
{
	/// <summary>
	/// Defines all the methods to manage the UI.
	/// </summary>
	public static class UserInterfaceTools
	{
		#region Public methods

		#region Controls

		/// <summary>
		/// Add the gridSplitter between to grids.
		/// </summary>
		/// <param name="grid">Defines the grid to attach to button into.</param>
		/// <param name="rowId">The row where the gridSplitter will be created.</param>
		/// <param name="colId">The column where the gridSplitter will be created.</param>
		/// <param name="direction">The direction for the gridSplitter.</param>
		public static void AddGridSplitter(Grid grid, int rowId, int colId, DirectionsEnum direction)
		{
			GridSplitter gridSplitter;
			if (direction == DirectionsEnum.Vertical)
				gridSplitter = new GridSplitter
				{
					Width = 5,
					Cursor = Cursors.SizeWE,
					Background = new SolidColorBrush(Colors.DarkGray),
					HorizontalAlignment = HorizontalAlignment.Stretch,
					ForceCursor = true,
					DragIncrement = 0.1
				};
			else
				gridSplitter = new GridSplitter
				{
					Height = 5,
					Cursor = Cursors.SizeNS,
					Background = new SolidColorBrush(Colors.DarkGray),
					HorizontalAlignment = HorizontalAlignment.Stretch,
					ForceCursor = true,
					DragIncrement = 0.1
				};

			Grid.SetColumn(gridSplitter, colId);
			Grid.SetRow(gridSplitter, rowId);
			Panel.SetZIndex(gridSplitter, 1000);
			grid.Children.Add(gridSplitter);
		}

		/// <summary>
		/// Add an image control to the main grid, at a given position.
		/// </summary>
		/// <param name="window">Keep the grid in memory to bind the methods, not really mvc pattern.</param>
		/// <param name="rowId">Defines where to add the grid.</param>
		/// <param name="colId">Defines where to add the grid.</param>
		/// <param name="colSpan">Defines the colSpan.</param>s
		/// <param name="rowSpan">Defines the rowSpan.</param>
		public static void AddImageControl(Views.Grid window, int rowId = 0, int colId = 0, int colSpan = 1, int rowSpan = 1)
		{
			Canvas canvas = new Canvas
            {
				Name = "ImageCanvas",
				Visibility = Visibility.Collapsed,
				AllowDrop = true,
                ClipToBounds = true,
                Background = new SolidColorBrush(Colors.Transparent)
			};
		    canvas.Drop += window.ImageCanvas_OnDrop;
		    canvas.MouseWheel += window.ImageCanvas_OnMouseWheel;
		    canvas.SizeChanged += window.ImageCanvas_SizeChanged;

            Image image = new Image
			{
				Name = "Image",
                ClipToBounds = true,
                RenderTransformOrigin = new Point(0.5, 0.5)
			};

		    var renderTransformGroup = new TransformGroup();
            var transflateTransform = new TranslateTransform();
		    var scaleTransform = new ScaleTransform();
            renderTransformGroup.Children.Add(transflateTransform);
		    renderTransformGroup.Children.Add(scaleTransform);

		    image.SetBinding(Canvas.TopProperty, new MultiBinding
		    {
		        Converter = new CenterConverter(),
		        ConverterParameter = "top",
		        Mode = BindingMode.TwoWay,
		        Bindings = {
		            new Binding("ActualWidth") { Source = canvas },
		            new Binding("ActualHeight") { Source = canvas },
		            new Binding("ActualWidth") { Source = image },
		            new Binding("ActualHeight") { Source = image }
		        }
		    });
            image.SetBinding(Canvas.LeftProperty, new MultiBinding
            {
                Converter = new CenterConverter(),
                ConverterParameter = "left",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = image },
                    new Binding("ActualHeight") { Source = image }
                }
            });

            image.RenderTransform = renderTransformGroup;
		    image.MouseLeftButtonDown += window.Image_OnMouseLeftButtonDown;
		    image.MouseLeftButtonUp += window.Image_OnMouseLeftButtonUp;
		    image.MouseMove += window.Image_OnMouseMove;
		    image.MouseDown += window.Image_MouseDown;
            image.SizeChanged += window.Image_SizeChanged;

            Grid.SetColumn(image, 1);
			Grid.SetRow(image, 1);
			canvas.Children.Add(image);
			Grid.SetColumn(canvas, colId);
			Grid.SetRow(canvas, rowId);
			Grid.SetColumnSpan(canvas, colSpan);
			Grid.SetRowSpan(canvas, rowSpan);
			Panel.SetZIndex(canvas, 10);
			window.MainGrid.Children.Add(canvas);

			AddImageControlButtons(window, canvas, rowSpan, colSpan);
		}

		/// <summary>
		/// Add the buttons to control the image.
		/// </summary>
		/// <param name="window">Used to bind to the methods.</param>
		/// <param name="grid">The grid to add the buttons into.</param>
		/// <param name="rowSpan">Defines the rowspan of the parent.</param>
		/// <param name="colSpan">Defines the colspan of the parent.</param>
		private static void AddImageControlButtons(Views.Grid window, Canvas canvas, int rowSpan = 1, int colSpan = 1)
		{
			Grid controlGrid = new Grid
			{
                Name = "ImageButtons",
				Background = new SolidColorBrush(Colors.Transparent),
                ClipToBounds = true,
				MaxWidth = 195,
                VerticalAlignment = VerticalAlignment.Top
			};
		    controlGrid.MouseEnter += window.ImageButtons_OnMouseEnter;
		    controlGrid.MouseLeave += window.ImageButtons_OnMouseLeave;

            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

		    var transformGroup = new TransformGroup();
		    var transflateTransform = new TranslateTransform();
		    transformGroup.Children.Add(transflateTransform);
		    controlGrid.RenderTransform = transformGroup;

            Button takeWidthButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "TakeWidthButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeWidthImage"] as BitmapImage
            };
		    takeWidthButton.Click += window.ImageControl_OnClick;

            Button takeHeightButton = new Button
		    {
		        Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "TakeHeightButton",
		        Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeHeightImage"] as BitmapImage
            };
		    takeHeightButton.Click += window.ImageControl_OnClick;

		    Button resizeButton = new Button
		    {
		        Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "ResizeButton",
		        Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["ResizeImage"] as BitmapImage
            };
		    resizeButton.Click += window.ImageControl_OnClick;

            Grid.SetColumn(takeHeightButton, 1);
			Grid.SetRow(takeHeightButton, 0);
			Grid.SetColumn(takeWidthButton, 2);
			Grid.SetRow(takeWidthButton, 0);
			Grid.SetColumn(resizeButton, 3);
			Grid.SetRow(resizeButton, 0);
		    controlGrid.Children.Add(takeHeightButton);
		    controlGrid.Children.Add(takeWidthButton);
		    controlGrid.Children.Add(resizeButton);
		    controlGrid.SetBinding(Canvas.LeftProperty, new MultiBinding
		    {
		        Converter = new CenterConverter(),
		        ConverterParameter = "left",
		        Mode = BindingMode.TwoWay,
		        Bindings = {
		            new Binding("ActualWidth") { Source = canvas },
		            new Binding("ActualHeight") { Source = canvas },
		            new Binding("ActualWidth") { Source = controlGrid },
		            new Binding("ActualHeight") { Source = controlGrid }
		        }
		    });

		    Grid.SetColumn(controlGrid, 1);
		    Grid.SetRow(controlGrid, 1);
		    Grid.SetColumnSpan(controlGrid, colSpan);
		    Grid.SetRowSpan(controlGrid, rowSpan);

		    canvas.Children.Add(controlGrid);
        }

        /// <summary>
        /// Add the control buttons for each new col/row defined.
        /// </summary>
        /// <param name="window">Keep the grid in memory to bind the methods, not really mvc pattern.</param>
        /// <param name="rowId">The row number on which to create the control buttons.</param>
        /// <param name="colId">The col number on which to create the control buttons.</param>
        /// <param name="colSpan">Defines the colSpan.</param>s
        /// <param name="rowSpan">Defines the rowSpan.</param>
        public static void AddControlButtons(Views.Grid window, int rowId = 0, int colId = 0, int colSpan = 1, int rowSpan = 1)
		{
			Grid controlGrid = new Grid
			{
				Name = "SetupGrid",
				Background = new SolidColorBrush(Colors.Transparent)
			};

			var splitButton = new Button { Style = Application.Current.Resources["RoundButtonStyle"] as Style, Content = "Split", Name = "splitButton" };
			splitButton.Click += window.SplitButton_OnClick;
			var addUpRowButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Top" };
			addUpRowButton.Click += window.AddUpRowButton_OnClick;
			var addRightColButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Right" };
			addRightColButton.Click += window.AddRightColButton_OnClick;
			var addDownRowButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Down" };
			addDownRowButton.Click += window.AddDownRowButton_OnClick;
			var addLeftColButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Left" };
			addLeftColButton.Click += window.AddLeftColButton_OnClick;
			var radialPanel = new RadialPanel { Style = Application.Current.Resources["RadialStyle"] as Style };

			radialPanel.Children.Add(addUpRowButton);
			radialPanel.Children.Add(addRightColButton);
			radialPanel.Children.Add(addDownRowButton);
			radialPanel.Children.Add(addLeftColButton);

			var mergeTopButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeTopButton", Visibility = Visibility.Collapsed};
			mergeTopButton.MouseLeftButtonUp += window.MergeTopButton_OnClick;
			var mergeDownButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeDownButton", Visibility = Visibility.Collapsed };
			mergeDownButton.MouseLeftButtonUp += window.MergeDownButton_OnClick;
			var mergeLeftButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeLeftButton", Visibility = Visibility.Collapsed };
			mergeLeftButton.MouseLeftButtonUp += window.MergeLeftButton_OnClick;
			var mergeRightButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeRightButton", Visibility = Visibility.Collapsed };
			mergeRightButton.MouseLeftButtonUp += window.MergeRightButton_OnClick;

		    controlGrid.Children.Add(splitButton);
		    controlGrid.Children.Add(radialPanel);
		    controlGrid.Children.Add(mergeTopButton);
		    controlGrid.Children.Add(mergeDownButton);
		    controlGrid.Children.Add(mergeLeftButton);
		    controlGrid.Children.Add(mergeRightButton);

		    controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
		    controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
		    controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

		    controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
		    controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		    controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
		    controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		    controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

			Grid.SetColumn(splitButton, 2);
			Grid.SetRow(splitButton, 2);
			Panel.SetZIndex(splitButton, 110);
			Grid.SetColumn(radialPanel, 2);
			Grid.SetRow(radialPanel, 2);

			Grid.SetColumn(mergeTopButton, 2);
			Grid.SetRow(mergeTopButton, 0);
			Grid.SetColumn(mergeDownButton, 2);
			Grid.SetRow(mergeDownButton, 4);
			Grid.SetColumn(mergeLeftButton, 0);
			Grid.SetRow(mergeLeftButton, 2);
			Grid.SetColumn(mergeRightButton, 4);
			Grid.SetRow(mergeRightButton, 2);

			Grid.SetColumn(controlGrid, colId);
			Grid.SetRow(controlGrid, rowId);
			Grid.SetColumnSpan(controlGrid, colSpan);
			Grid.SetRowSpan(controlGrid, rowSpan);
			Panel.SetZIndex(controlGrid, 100);
			window.MainGrid.Children.Add(controlGrid);
		}

		#endregion // Controls

		/// <summary>
		/// Helps to resolve the parent for a given child.
		/// </summary>
		/// <param name="child">The parent to return.</param>
		public static UIElement FindParent(DependencyObject child)
		{
			var result = VisualTreeHelper.GetParent(child);
			return result is RadialPanel ? FindParent(result) : result as UIElement;
		}

		/// <summary>
		/// Move the controls to the good position.
		/// </summary>
		/// <param name="colId">The col number above which the control must be moved.</param>
		/// <param name="direction">The direction to move the elements.</param>
		/// <param name="grid">The grid to manipulate.</param>
		public static void MoveUiElementsColumn(Grid grid, DirectionsEnum direction, int colId = 0)
		{
			var cellsToMove = grid.Children.Cast<UIElement>().Where(e => Grid.GetColumn(e) >= colId).OrderByDescending(Grid.GetColumn).ToList();
			if (cellsToMove.Any())
			{
				foreach (var cell in cellsToMove)
				{
					if (direction == DirectionsEnum.Left || direction == DirectionsEnum.Right)
						Grid.SetColumn(cell, Grid.GetColumn(cell) + 1);
					else if (direction == DirectionsEnum.None)
						Grid.SetColumn(cell, Grid.GetColumn(cell) - 1 < 0 ? 0 : Grid.GetColumn(cell) - 1);
				}
			}
		}

		/// <summary>
		/// Move the controls to the good position.
		/// </summary>
		/// <param name="rowId">The row number above which the control must be moved.</param>
		/// <param name="direction">The direction to move the elements.</param>
		/// <param name="grid">The grid to manipulate.</param>
		public static void MoveUiElementsRow(Grid grid, DirectionsEnum direction, int rowId = 0)
		{
			var cellsToMove = grid.Children.Cast<UIElement>().Where(e => Grid.GetRow(e) >= rowId).OrderByDescending(Grid.GetRow).ToList();
			if (cellsToMove.Any())
			{
				foreach (var cell in cellsToMove)
				{
					if (direction == DirectionsEnum.Up || direction == DirectionsEnum.Down)
						Grid.SetRow(cell, Grid.GetRow(cell) + 1);
					else if (direction == DirectionsEnum.None)
						Grid.SetRow(cell, Grid.GetRow(cell) - 1 < 0 ? 0 : Grid.GetRow(cell) - 1);
				}
			}
		}

		/// <summary>
		/// Delete the content at the given position.
		/// </summary>
		/// <param name="mainGrid">The grid to search.</param>
		/// <param name="rowId">The row id.</param>
		/// <param name="colId">The col id.</param>
		/// <param name="direction">The direction to look.</param>
		/// <returns>Returns the col/row value.</returns>
		public static Dictionary<string, int> DeleteContent(Grid mainGrid, int rowId, int colId, DirectionsEnum direction)
		{
			var cellsToDelete = mainGrid.Children.Cast<UIElement>().Where(e => Grid.GetRow(e) == rowId && Grid.GetColumn(e) == colId).ToList();
			if (!cellsToDelete.Any())
			{
				switch (direction)
				{
					case DirectionsEnum.Left:
						return DeleteContent(mainGrid, rowId, colId - 1, direction);
					case DirectionsEnum.Up:
						return DeleteContent(mainGrid, rowId - 1, colId, direction);
				}
			}
			else
			{
				var colSpan = 0;
				var rowSpan = 0;
				foreach (var c in cellsToDelete)
				{
					if (c is Grid grid && grid.Name.Equals("SetupGrid"))
					{
						colSpan = Grid.GetColumnSpan(c);
						rowSpan = Grid.GetRowSpan(c);
					}
					mainGrid.Children.Remove(c);
				}

				return new Dictionary<string, int> {{ "rowSpan", rowSpan }, { "colSpan", colSpan } };
			}

			return null;
		}

		/// <summary>
		/// Checks if there is a cell to the current location.
		/// </summary>
		/// <param name="mainGrid">The mainggrid to inspect.</param>
		/// <param name="grid">The grid check.</param>
		/// <param name="direction">Defines the direction to check.</param>
		/// <returns>Returns truc if there is a cell on this position.</returns>
		private static bool NeighborChecker(Grid mainGrid, Grid grid, DirectionsEnum direction)
		{
			var currentRow = Grid.GetRow(grid);
			var currentCol = Grid.GetColumn(grid);
			var currentRowSpan = Grid.GetRowSpan(grid);
			var currentColSpan = Grid.GetColumnSpan(grid);
			var rowMax = mainGrid.RowDefinitions.Count;
			var colMax = mainGrid.ColumnDefinitions.Count;

			UIElement potentielNeighbor;
			switch (direction)
			{
				case DirectionsEnum.Left:
					if (currentCol == 0)
						return false;

					potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
						e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol - Grid.GetColumnSpan(e) - 1);
					if (potentielNeighbor is Grid)
					{
						if (Grid.GetRowSpan(potentielNeighbor) == currentRowSpan)
							return true;
					}
					return false;

				case DirectionsEnum.Right:
					if (currentCol == colMax)
						return false;

					potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
						e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol + currentColSpan + 1);
					if (potentielNeighbor is Grid)
					{
						if (Grid.GetRowSpan(potentielNeighbor) == currentRowSpan)
							return true;
					}
					return false;

				case DirectionsEnum.Up:
					if (currentRow == 0)
						return false;

					potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
						e => Grid.GetRow(e) == currentRow - Grid.GetRowSpan(e) - 1 && Grid.GetColumn(e) == currentCol);
					if (potentielNeighbor is Grid)
					{
						if (Grid.GetColumnSpan(potentielNeighbor) == currentColSpan)
							return true;
					}
					return false;

				case DirectionsEnum.Down:
					if (currentRow == rowMax)
						return false;

					potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
						e => Grid.GetRow(e) == currentRow + currentRowSpan + 1 && Grid.GetColumn(e) == currentCol);
					if (potentielNeighbor is Grid)
					{
						if (Grid.GetColumnSpan(potentielNeighbor) == currentColSpan)
							return true;
					}
					return false;

				default:
					return false;
			}
		}

		/// <summary>
		/// Lock or unlock the buttons on click.
		/// </summary>
		/// <param name="mainGrid">Defines the main grid to lock the buttons.</param>
		/// <param name="isLocked">Defines the mode.</param>
		public static void ToggleLockControlButtons(Grid mainGrid, bool isLocked)
		{
			var elementList = mainGrid.Children.Cast<UIElement>().Where(e => e is Grid || e is Canvas).ToList();
			foreach (var uiElement in elementList)
			{
			    if (uiElement is Canvas canvas && canvas.Name == "ImageCanvas")
			    {
			        var imageControl = canvas.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
			        if (imageControl is Image image && image.Source != null)
			            canvas.Opacity = canvas.Opacity < 1 ? 1 : 0.3;
			        else
			            canvas.Visibility = canvas.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                else
			        uiElement.Visibility = uiElement.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }

			if (!isLocked)
				UpdateControlButtons(mainGrid);
		}

		/// <summary>
		/// Updates the control to show/hide the merge buttons.
		/// </summary>
		public static void UpdateControlButtons(Grid mainGrid)
		{
			var gridList = mainGrid.Children.Cast<UIElement>().Where(e => e is Grid).ToList();
			foreach (var uiElement in gridList)
			{
				var grid = (Grid)uiElement;
				var buttonsList = grid.Children.Cast<UIElement>().Where(x => x is Button).ToList();
				var imageButtonsList = grid.Children.Cast<UIElement>().Where(x => x is Image).ToList();
				var mergeTopButton = imageButtonsList.Cast<Image>().FirstOrDefault(b => b.Name.Equals("mergeTopButton"));
				var mergeDownButton = imageButtonsList.Cast<Image>().FirstOrDefault(b => b.Name.Equals("mergeDownButton"));
				var mergeLeftButton = imageButtonsList.Cast<Image>().FirstOrDefault(b => b.Name.Equals("mergeLeftButton"));
				var mergeRightButton = imageButtonsList.Cast<Image>().FirstOrDefault(b => b.Name.Equals("mergeRightButton"));
				var splitButton = buttonsList.Cast<Button>().FirstOrDefault(b => b.Name.Equals("splitButton"));

				if (mergeTopButton != null)
					mergeTopButton.Visibility = NeighborChecker(mainGrid, grid, DirectionsEnum.Up) ? Visibility.Visible : Visibility.Hidden;
				if (mergeDownButton != null)
					mergeDownButton.Visibility = NeighborChecker(mainGrid, grid, DirectionsEnum.Down) ? Visibility.Visible : Visibility.Hidden;
				if (mergeLeftButton != null)
					mergeLeftButton.Visibility = NeighborChecker(mainGrid, grid, DirectionsEnum.Left) ? Visibility.Visible : Visibility.Hidden;
				if (mergeRightButton != null)
					mergeRightButton.Visibility = NeighborChecker(mainGrid, grid, DirectionsEnum.Right) ? Visibility.Visible : Visibility.Hidden;
				if (splitButton != null)
					splitButton.Visibility = Grid.GetColumnSpan(grid) > 1 || Grid.GetRowSpan(grid) > 1 ? Visibility.Visible : Visibility.Hidden;
			}
		}

		/// <summary>
		/// Remove the content of a grid, picture/movie...
		/// </summary>
		/// <param name="grid">The grid where we want to delete the content.</param>
		/// <param name="isMain">Tells if this is the main grid.</param>
		public static void RemoveContentFromGrid(Grid grid, bool isMain)
		{
			if (isMain)
			{
				foreach (var item in grid.Children.Cast<UIElement>().Where(e => e is Canvas && e.Visibility == Visibility.Visible).ToList())
				{
					var canvas = item as Canvas;
					var image = canvas?.Children.Cast<UIElement>().FirstOrDefault(e => e is Image && e.Visibility == Visibility.Visible);

					if (image == null)
						continue;

				    var renderScaleTransform = (ScaleTransform)((TransformGroup)((Image)image).RenderTransform).Children.First(tr => tr is ScaleTransform);
				    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
				    renderScaleTransform.ScaleX = 1;
				    renderScaleTransform.ScaleY = 1;
				    translateTransform.X = 0;
				    translateTransform.Y = 0;
                    ((Image)image).Source = null;
                    ((Image)image).Visibility = Visibility.Hidden;
				}
			}
			else
			{
				//foreach (var item in grid.Children)
				//{
				//	if (item is Button)
				//		continue;

				//	if (item is Image)
				//	{
				//		var image = item as Image;
				//		image.Source = null;
				//		image.Visibility = Visibility.Hidden;
				//	}
				//	else if (item is VideoDrawing)
				//	{
				//		throw new NotImplementedException();
				//	}
				//}
			}
		}

        #endregion // Public methods
    }
}
