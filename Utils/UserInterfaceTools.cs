using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GridSetter.Controls;
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
		/// <param name="window">Defines the binding.</param>
		/// <param name="grid">Defines the grid to attach to button into.</param>
		/// <param name="rowId">The row where the gridSplitter will be created.</param>
		/// <param name="colId">The column where the gridSplitter will be created.</param>
		/// <param name="direction">The direction for the gridSplitter.</param>
		public static void AddGridSplitter(Views.Grid window, Grid grid, int rowId, int colId, DirectionsEnum direction)
		{
			GridSplitter gridSplitter;
			if (direction == DirectionsEnum.Vertical)
				gridSplitter = new GridSplitter
				{
					Width = 5,
					Cursor = Cursors.SizeWE,
					Background = new SolidColorBrush(Colors.DarkGray),
					HorizontalAlignment = HorizontalAlignment.Stretch,
					ForceCursor = true
				};
			else
				gridSplitter = new GridSplitter
				{
					Height = 5,
					Cursor = Cursors.SizeNS,
					Background = new SolidColorBrush(Colors.DarkGray),
					HorizontalAlignment = HorizontalAlignment.Stretch,
					ForceCursor = true
				};

			gridSplitter.DragCompleted += window.GridSplitterDragEnd;

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
			Grid imageGrid = new Grid
			{
				Name = "imageGrid",
				Visibility = Visibility.Collapsed,
				AllowDrop = true,
                ClipToBounds = true,
                Background = new SolidColorBrush(Colors.Transparent)
			};
			imageGrid.Drop += window.ImageGridOnDrop;
			imageGrid.MouseWheel += window.ImageDisplayOnMouseWheel;
			imageGrid.SizeChanged += window.ImageGridSizeChanged;

		    imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
		    imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

		    imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
		    imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Image imageDisplay = new Image
			{
				Name = "image",
                ClipToBounds = true
			};

		    var layoutTransformGroup = new TransformGroup();
			var scaleTransform = new ScaleTransform();
		    layoutTransformGroup.Children.Add(scaleTransform);
		    var renderTransformGroup = new TransformGroup();
            var transflateTransform = new TranslateTransform();
		    var renderScaleTransform = new ScaleTransform();
            renderTransformGroup.Children.Add(transflateTransform);
		    renderTransformGroup.Children.Add(renderScaleTransform);

            imageDisplay.LayoutTransform = layoutTransformGroup;
			imageDisplay.RenderTransform = renderTransformGroup;
			imageDisplay.MouseLeftButtonDown += window.ImageDisplayOnMouseLeftButtonDown;
			imageDisplay.MouseLeftButtonUp += window.ImageDisplayOnMouseLeftButtonUp;
			imageDisplay.MouseMove += window.ImageDisplayOnMouseMove;
            imageDisplay.SizeChanged += window.ImageDisplaySizeChanged;

            Grid.SetColumn(imageDisplay, 1);
			Grid.SetRow(imageDisplay, 1);
			imageGrid.Children.Add(imageDisplay);
			Grid.SetColumn(imageGrid, colId);
			Grid.SetRow(imageGrid, rowId);
			Grid.SetColumnSpan(imageGrid, colSpan);
			Grid.SetRowSpan(imageGrid, rowSpan);
			Panel.SetZIndex(imageGrid, 10);
			window.MainGrid.Children.Add(imageGrid);

			AddImageControlButtons(window, imageGrid, rowSpan, colSpan);
		}

		/// <summary>
		/// Add the buttons to control the image.
		/// </summary>
		/// <param name="window">Used to bind to the methods.</param>
		/// <param name="grid">The grid to add the buttons into.</param>
		/// <param name="rowSpan">Defines the rowspan of the parent.</param>
		/// <param name="colSpan">Defines the colspan of the parent.</param>
		private static void AddImageControlButtons(Views.Grid window, Grid grid, int rowSpan = 1, int colSpan = 1)
		{
			Grid controlGrid = new Grid
			{
                Name = "imageControlGrid",
				Background = new SolidColorBrush(Colors.Transparent),
                ClipToBounds = true,
				MaxWidth = 195,
                VerticalAlignment = VerticalAlignment.Top
			};
		    controlGrid.MouseEnter += window.ImageControlGridMouseEnter;
		    controlGrid.MouseLeave += window.ImageControlGridMouseLeave;

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
                Name = "takeWidthButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeWidthImage"] as BitmapImage
            };
		    takeWidthButton.Click += window.ImageControlButtonClick;

            Button takeHeightButton = new Button
		    {
		        Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "takeHeightButton",
		        Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeHeightImage"] as BitmapImage
            };
		    takeHeightButton.Click += window.ImageControlButtonClick;

		    Button resizeButton = new Button
		    {
		        Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "resizeButton",
		        Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["ResizeImage"] as BitmapImage
            };
		    resizeButton.Click += window.ImageControlButtonClick;

            Grid.SetColumn(takeHeightButton, 1);
			Grid.SetRow(takeHeightButton, 0);
			Grid.SetColumn(takeWidthButton, 2);
			Grid.SetRow(takeWidthButton, 0);
			Grid.SetColumn(resizeButton, 3);
			Grid.SetRow(resizeButton, 0);
		    controlGrid.Children.Add(takeHeightButton);
		    controlGrid.Children.Add(takeWidthButton);
		    controlGrid.Children.Add(resizeButton);

            Grid.SetColumn(controlGrid, 1);
			Grid.SetRow(controlGrid, 1);
			Grid.SetColumnSpan(controlGrid, colSpan);
			Grid.SetRowSpan(controlGrid, rowSpan);

            grid.Children.Add(controlGrid);
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
			Grid insideGrid = new Grid
			{
				Name = "insideGrid",
				Background = new SolidColorBrush(Colors.Transparent)
			};

			var splitButton = new Button { Style = Application.Current.Resources["RoundButtonStyle"] as Style, Content = "Split", Name = "splitButton" };
			splitButton.Click += window.SplitButton_OnClick;
			var addUpRowButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Top" };
			addUpRowButton.Click += window.AddUpRowButtonOnClick;
			var addRightColButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Right" };
			addRightColButton.Click += window.AddRightColButtonOnClick;
			var addDownRowButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Down" };
			addDownRowButton.Click += window.AddDownRowButtonOnClick;
			var addLeftColButton = new Button { Style = Application.Current.Resources["NoStyleButton"] as Style, Content = "Left" };
			addLeftColButton.Click += window.AddLeftColButtonOnClick;
			var radialPanel = new RadialPanel { Style = Application.Current.Resources["RadialStyle"] as Style };

			radialPanel.Children.Add(addUpRowButton);
			radialPanel.Children.Add(addRightColButton);
			radialPanel.Children.Add(addDownRowButton);
			radialPanel.Children.Add(addLeftColButton);

			var mergeTopButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeTopButton", Visibility = Visibility.Collapsed};
			mergeTopButton.MouseLeftButtonUp += window.MergeTopButtonOnClick;
			var mergeDownButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeDownButton", Visibility = Visibility.Collapsed };
			mergeDownButton.MouseLeftButtonUp += window.MergeDownButtonOnClick;
			var mergeLeftButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeLeftButton", Visibility = Visibility.Collapsed };
			mergeLeftButton.MouseLeftButtonUp += window.MergeLeftButtonOnClick;
			var mergeRightButton = new Image { Style = Application.Current.Resources["MergeButtonStyle"] as Style, Name = "mergeRightButton", Visibility = Visibility.Collapsed };
			mergeRightButton.MouseLeftButtonUp += window.MergeRightButtonOnClick;

			insideGrid.Children.Add(splitButton);
			insideGrid.Children.Add(radialPanel);
			insideGrid.Children.Add(mergeTopButton);
			insideGrid.Children.Add(mergeDownButton);
			insideGrid.Children.Add(mergeLeftButton);
			insideGrid.Children.Add(mergeRightButton);

			insideGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			insideGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			insideGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			insideGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			insideGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

			insideGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			insideGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			insideGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			insideGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			insideGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

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

			Grid.SetColumn(insideGrid, colId);
			Grid.SetRow(insideGrid, rowId);
			Grid.SetColumnSpan(insideGrid, colSpan);
			Grid.SetRowSpan(insideGrid, rowSpan);
			Panel.SetZIndex(insideGrid, 100);
			window.MainGrid.Children.Add(insideGrid);
		}

		#endregion // Controls

		/// <summary>
		/// Helps to resolve the parent for a given child.
		/// </summary>
		/// <param name="child">The parent to return.</param>
		public static Grid FindParent(DependencyObject child)
		{
			var result = VisualTreeHelper.GetParent(child);
			if (result is RadialPanel)
				return FindParent(result);
			return result as Grid;
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
					if (c is Grid grid && grid.Name.Equals("insideGrid"))
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
			var gridList = mainGrid.Children.Cast<UIElement>().Where(e => e is Grid).ToList();
			foreach (var uiElement in gridList)
			{
				var grid = (Grid)uiElement;
			    if (grid.Name == "imageGrid")
			    {
			        var imageControl = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
			        if (imageControl is Image image && image.Source != null)
			            grid.Opacity = grid.Opacity < 1 ? 1 : 0.3;
			        else
			            grid.Visibility = grid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                }
                else
			        grid.Visibility = grid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
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
			// TODO gérer les vidéo aussi.
			if (isMain)
			{
				foreach (var item in grid.Children.Cast<UIElement>().Where(e => e is Grid && e.Visibility == Visibility.Visible).ToList())
				{
					var itemGrid = item as Grid;
					var image = itemGrid?.Children.Cast<UIElement>().FirstOrDefault(e => e is Image && e.Visibility == Visibility.Visible);

					if (image == null)
						continue;

                    var scaleTransform = (ScaleTransform)((TransformGroup)((Image)image).RenderTransform).Children.First(tr => tr is ScaleTransform);
				    scaleTransform.ScaleX = 1;
				    scaleTransform.ScaleY = 1;
                    ((Image)image).Source = null;
                    ((Image)image).Visibility = Visibility.Hidden;
				}
			}
			else
			{
				foreach (var item in grid.Children)
				{
					if (item is Button)
						continue;

					if (item is Image)
					{
						var image = item as Image;
						image.Source = null;
						image.Visibility = Visibility.Hidden;
					}
					else if (item is VideoDrawing)
					{
						throw new NotImplementedException();
					}
				}
			}
		}

		#endregion // Public methods
	}
}
