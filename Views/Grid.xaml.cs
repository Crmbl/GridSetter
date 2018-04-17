using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using GridSetter.Utils;
using GridSetter.Utils.Enums;
using Button = System.Windows.Controls.Button;
using Control = System.Windows.Forms.Control;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using GGrid = System.Windows.Controls.Grid;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
// ReSharper disable CompareOfFloatsByEqualityOperator

/* TODO : 
 *
 * Gestion IMAGES
 * {
 *  Lors de la réduction de taille dû aux gridsplitter, la taille s'accorde pas bien
 *  Les boutons ne se centrent pas quand on réduit la taille w/ gridsplitter
 *  ====> piste de réflexion
 *         La desiredSize se met bien à jour, l'utiliser pour force move ?.. Event maybe ?
 * }
 *
 * Autre
 * {
 *  Si on lock/unlock avec le shortcut, le libellé ce met pas à jour
 *  Changer la gueule du curseur en fonction des éléments
 *  Améliorer la préhension des boutons left/right/top/bottom
 * }
 *
 * Gestion VIDEOS
 * {
 *  Tout à faire
 * }
 */

namespace GridSetter.Views
{
	/// <summary>
	/// Interaction logic for Grid.xaml
	/// </summary>
	public sealed partial class Grid
	{
        #region DLL imports 

	    [DllImport("User32.dll")]
	    static extern Boolean SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, UInt32 pvParam, UInt32 fWinIni);

        #endregion // DLL imports

        #region Constants

	    /// <summary>
	    /// Defines the mouse speed when dragging.
	    /// </summary>
	    private const UInt32 SlowerMouseSpeed = 4;

	    /// <summary>
	    /// Constant to set the mouse speed.
	    /// </summary>
	    // ReSharper disable once InconsistentNaming
	    private const UInt32 SPI_SETMOUSESPEED = 0x0071;

        #endregion // Constants

        #region Properties

        /// <summary>
        /// Defines the main grid of the app.
        /// </summary>
        public GGrid MainGrid { get; }

		/// <summary>
		/// The mouse position.
		/// </summary>
		private Point MousePosition { get; set; }
		
		/// <summary>
		/// The origin position.
		/// </summary>
		private Point OriginPosition { get; set; }

		/// <summary>
		/// The mouse when dragging the gridsplitter.
		/// </summary>
		private Point MouseSplitterPosition { get; set; }

        /// <summary>
        /// Defines the mouse speed when starting the drag event.
        /// </summary>
        private UInt32 OriginMouseSpeed { get; set; }

        /// <summary>
        /// Determines the state of the grid.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Defines the position before drag start.
        /// </summary>
        private Point OriginGridSplitter { get; set; }

		/// <summary>
		/// Defines the gridSplitter draggingDirection.
		/// </summary>
		private bool[] DraggingDirection { get; set; }

        #region Static

        /// <summary>
        /// The routed command for the shortcut.
        /// </summary>
        public static readonly RoutedCommand ToggleLockCommand = new RoutedCommand();

        #endregion // Static

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Default parameterless constructor.
        /// </summary>
        public Grid()
		{
			InitializeComponent();

			WindowStyle = WindowStyle.None;
			ResizeMode = ResizeMode.NoResize;
			Left = 0;
			Top = 0;
		    Width = Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top)).WorkingArea.Width;
		    Height = Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top)).WorkingArea.Height;

            MainGrid = new GGrid { ShowGridLines = false };
			MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

			AddChild(MainGrid);
			UserInterfaceTools.AddControlButtons(this);
			UserInterfaceTools.AddImageControl(this);
			UserInterfaceTools.UpdateControlButtons(MainGrid);

		    ToggleLockCommand.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
		}

        #endregion // Constructors

        #region Events

	    /// <summary>
	    /// Triggered when the drag starts.
	    /// </summary>
	    /// <param name="sender">Don't care.</param>
	    /// <param name="args">Don't know.</param>
	    public void GridSplitterDragStart(object sender, DragStartedEventArgs args)
	    {
	        if (!(sender is GridSplitter gridSplitter)) return;
            OriginGridSplitter = gridSplitter.TranslatePoint(new Point(0, 0), MainGrid);

		    MouseSplitterPosition = new Point(Control.MousePosition.X, Control.MousePosition.Y);
		    DraggingDirection = new[] {true, true, true, true};
			const int value = 1;
		    if (gridSplitter.ActualWidth == 5)
		    {
			    var columnRight = GGrid.GetColumn(gridSplitter) + 1;
			    var columnLeft = GGrid.GetColumn(gridSplitter) - 1;
			    if (Math.Round(MainGrid.ColumnDefinitions[columnLeft].ActualWidth, MidpointRounding.AwayFromZero) - value <= 195 && args.VerticalOffset > 0)
				    DraggingDirection[0] = false;
			    if (Math.Round(MainGrid.ColumnDefinitions[columnRight].ActualWidth, MidpointRounding.AwayFromZero) - value <= 195 && args.VerticalOffset < 0)
				    DraggingDirection[1] = false;
			}
		    else if (gridSplitter.ActualHeight == 5)
		    {
			    var rowBottom = GGrid.GetRow(gridSplitter) + 1;
			    var rowTop = GGrid.GetRow(gridSplitter) - 1;

			    if (Math.Round(MainGrid.RowDefinitions[rowBottom].ActualHeight, MidpointRounding.AwayFromZero) - value <= 100 && args.HorizontalOffset > 0)
				    DraggingDirection[2] = false;
				if (Math.Round(MainGrid.RowDefinitions[rowTop].ActualHeight, MidpointRounding.AwayFromZero) - value <= 100 && args.HorizontalOffset < 0)
					DraggingDirection[3] = false;
			}
		}

		/// <summary>
		/// Triggered when the mouse move over the gridsplitter.
		/// </summary>
		/// <param name="sender">YO!</param>
		/// <param name="args">WADDUP.</param>
		public void GridSplitterMouseMove(object sender, MouseEventArgs args)
		{
			if (!(sender is GridSplitter gridSplitter && gridSplitter.IsDragging) || args.LeftButton != MouseButtonState.Pressed) return;

			var newMousePos = new Point(Control.MousePosition.X, Control.MousePosition.Y);
			var delta = newMousePos - MouseSplitterPosition;
			if (gridSplitter.ActualWidth == 5)
			{
				if (delta.X < 0)
				{
					if (!DraggingDirection[0])
						gridSplitter.CancelDrag();
				}
				else
				{
					if (!DraggingDirection[1])
						gridSplitter.CancelDrag();
				}
			}
			else if (gridSplitter.ActualHeight == 5)
			{
				if (delta.Y > 0)
				{
					if (!DraggingDirection[2])
						gridSplitter.CancelDrag();
				}
				else
				{
					if (!DraggingDirection[3])
						gridSplitter.CancelDrag();
				}
			}

			Console.WriteLine($"Y : {delta.Y} /// X : {delta.X}");
		}

        /// <summary>
        /// Triggered when is dragging.
        /// </summary>
        /// <param name="sender">Fart powder.</param>
        /// <param name="args">Yup.</param>
	    public void GridSplitterDragDelta(object sender, DragDeltaEventArgs args)
	    {
			if (!(sender is GridSplitter gridSplitter)) return;

			var delta = gridSplitter.TranslatePoint(new Point(0, 0), MainGrid) - OriginGridSplitter;
			if (delta.X != 0)
			{
				int column;
				if (delta.X > 0) // vers la droite
					column = GGrid.GetColumn(gridSplitter) + 1;
				else // vers la gauche
					column = GGrid.GetColumn(gridSplitter) - 1;

				if (Math.Round(MainGrid.ColumnDefinitions[column].ActualWidth, MidpointRounding.AwayFromZero) <= 195)
					gridSplitter.CancelDrag();
			}
			else if (delta.Y != 0)
			{
				int row;
				if (delta.Y > 0) // vers le bas
					row = GGrid.GetRow(gridSplitter) + 1;
				else // vers le haut
					row = GGrid.GetRow(gridSplitter) - 1;

				if (Math.Round(MainGrid.RowDefinitions[row].ActualHeight, MidpointRounding.AwayFromZero) <= 100)
					gridSplitter.CancelDrag();
			}
		}

	    /// <summary>
        /// Triggered when the drag ends.
        /// </summary>
        /// <param name="sender">Don't care.</param>
        /// <param name="args">Don't know.</param>
        public void GridSplitterDragEnd(object sender, DragCompletedEventArgs args)
        {
            foreach (var child in MainGrid.Children.Cast<UIElement>().Where(e => e is GGrid grid && grid.Name == "imageGrid"))
            {
                if (!(child is GGrid grid)) continue;
                if (!(sender is GridSplitter gridSplitter)) continue;
                if (GGrid.GetRow(gridSplitter) - 1 != GGrid.GetRow(grid) &&
                    GGrid.GetRow(gridSplitter) + 1 != GGrid.GetRow(grid) &&
                    GGrid.GetColumn(gridSplitter) - 1 != GGrid.GetColumn(grid) &&
                    GGrid.GetColumn(gridSplitter) + 1 != GGrid.GetColumn(grid)) continue;
                var imageControl = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
                if (!(imageControl is Image image) || image.Source == null) continue;

                var renderScaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
                var layoutScaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
                ScaleTransform scaleTransform;
                if (renderScaleTransform.ScaleX != 1 || renderScaleTransform.ScaleY != 1)
                    scaleTransform = renderScaleTransform;
                else
                    scaleTransform = layoutScaleTransform;

                var imageHeight = Math.Round(image.ActualHeight * scaleTransform.ScaleY, MidpointRounding.AwayFromZero);
                var imageWidth = Math.Round(image.ActualWidth * scaleTransform.ScaleX, MidpointRounding.AwayFromZero);
                var gridHeight = Math.Round(grid.ActualHeight, MidpointRounding.AwayFromZero);
                var gridWidth = Math.Round(grid.ActualWidth, MidpointRounding.AwayFromZero);
                var newPos = gridSplitter.TranslatePoint(new Point(0, 0), MainGrid);
                var delta = newPos - OriginGridSplitter;

                if (imageHeight == gridHeight || imageWidth == gridWidth)
                {
                    UIElement oppositeGridSplitter;
                    if (delta.X != 0)
                    {
                        int column;
                        if (GGrid.GetColumn(gridSplitter) > GGrid.GetColumn(grid))
                            column = GGrid.GetColumn(grid) - 1;
                        else
                            column = GGrid.GetColumn(grid) + 1;

                        oppositeGridSplitter = MainGrid.Children.Cast<UIElement>().FirstOrDefault(e =>
                            GGrid.GetColumn(e) == column && GGrid.GetRow(e) == GGrid.GetRow(gridSplitter) && e is GridSplitter);
                    }
                    else if (delta.Y != 0)
                    {
                        int row;
                        if (GGrid.GetRow(gridSplitter) > GGrid.GetRow(grid))
                            row = GGrid.GetRow(grid) - 1;
                        else
                            row = GGrid.GetRow(grid) + 1;

                        oppositeGridSplitter = MainGrid.Children.Cast<UIElement>().FirstOrDefault(e =>
                            GGrid.GetRow(e) == row && GGrid.GetColumn(e) == GGrid.GetColumn(gridSplitter) && e is GridSplitter);
                    }
                    else
                        return;

                    var posOpposite = oppositeGridSplitter?.TranslatePoint(new Point(0, 0), MainGrid);
                    Vector value;
                    if (GGrid.GetColumn(gridSplitter) > GGrid.GetColumn(grid))
                        // ReSharper disable once PossibleInvalidOperationException
                        value = (Vector) (newPos - posOpposite);
                    else
                        // ReSharper disable once PossibleInvalidOperationException
                        value = (Vector) (posOpposite - newPos);

                    var scale = (value.X - 5) / image.ActualWidth;
                    scaleTransform.ScaleX = Math.Round(scale, 2);
                    scaleTransform.ScaleY = Math.Round(scale, 2);
                }
                else if (imageHeight < ((Rect) image.Tag).Height && grid.ActualHeight > imageHeight ||
                         imageWidth < ((Rect)image.Tag).Width && grid.ActualWidth > imageWidth)
                {
                    double scale;
                    if (delta.X != 0)
                        scale = grid.ActualWidth / image.ActualWidth;
                    else if (delta.Y != 0)
                        scale = grid.ActualHeight / image.ActualHeight;
                    else
                        return;

                    if (image.ActualHeight * scale > ((Rect) image.Tag).Height
                        || image.ActualWidth * scale > ((Rect) image.Tag).Width)
                    {
                        scaleTransform.ScaleX = 1;
                        scaleTransform.ScaleY = 1;
                    }
                    else
                    {
                        scaleTransform.ScaleX = Math.Round(scale, 2);
                        scaleTransform.ScaleY = Math.Round(scale, 2);
                    }
                }
                else if (imageHeight > ((Rect)image.Tag).Height && grid.ActualHeight > imageHeight ||
                         imageWidth > ((Rect)image.Tag).Width && grid.ActualWidth > imageWidth)
                {
                    scaleTransform.ScaleX = 1;
                    scaleTransform.ScaleY = 1;
                }
            }
        }

        /// <summary>
        /// Remove a column on click.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="e">The routed args.</param>
        public void SplitButton_OnClick(object sender, RoutedEventArgs e)
		{
			var parent = UserInterfaceTools.FindParent((Button)sender);
			var currentCol = GGrid.GetColumn(parent);
			var currentRow = GGrid.GetRow(parent);
			var actualColSpan = GGrid.GetColumnSpan(parent);
			var actualRowSpan = GGrid.GetRowSpan(parent);

			UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.None);
			for (var i = 0; i < actualRowSpan; i++)
			{
				for (var y = 0; y < actualColSpan; y++)
				{
					if (y % 2 != 0)
						UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow + i, currentCol + y, DirectionsEnum.Vertical);
					else if (i % 2 != 0)
						UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow + i, currentCol + y, DirectionsEnum.Horizontal);
					else
					{
						UserInterfaceTools.AddControlButtons(this, currentRow + i, currentCol + y);
						UserInterfaceTools.AddImageControl(this, currentRow + i, currentCol + y);
					}
				}
			}

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		#region Add events

		/// <summary>
		/// Add a column on the left of the grid containing the clicked button.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="routedEventArgs">The routed args.</param>
		public void AddLeftColButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Button)sender);
			var currentCol = GGrid.GetColumn(parent);
			var rowAmount = MainGrid.RowDefinitions.Count;

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
			for (var i = 0; i < rowAmount; i++)
				UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Vertical);

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
			for (var i = 0; i < rowAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Horizontal);
				else
				{
					UserInterfaceTools.AddControlButtons(this, i, currentCol);
					UserInterfaceTools.AddImageControl(this, i, currentCol);
				}
			}

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Add a column on the right of the grid containing the clicked button.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="routedEventArgs">The routed args.</param>
		public void AddRightColButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Button)sender);
			var currentCol = GGrid.GetColumn(parent) + GGrid.GetColumnSpan(parent);
			var rowAmount = MainGrid.RowDefinitions.Count;

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Right, currentCol);
			for (var i = 0; i < rowAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Horizontal);
				else
				{
					UserInterfaceTools.AddControlButtons(this, i, currentCol);
					UserInterfaceTools.AddImageControl(this, i, currentCol);
				}
			}

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Right, currentCol);
			for (var i = 0; i < rowAmount; i++)
				UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Vertical);

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Add a row on the top of the grid containing the clicked button.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="routedEventArgs">The routed args.</param>
		public void AddUpRowButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Button)sender);
			var currentRow = GGrid.GetRow(parent);
			var colAmount = MainGrid.ColumnDefinitions.Count;

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
			for (var i = 0; i < colAmount; i++)
				UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Horizontal);

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
			for (var i = 0; i < colAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Vertical);
				else
				{
					UserInterfaceTools.AddControlButtons(this, currentRow, i);
					UserInterfaceTools.AddImageControl(this, currentRow, i);
				}
			}

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Add a row under the grid containing the clicked button.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="routedEventArgs">The routed args.</param>
		public void AddDownRowButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Button)sender);
			var currentRow = GGrid.GetRow(parent) + GGrid.GetRowSpan(parent);
			var colAmount = MainGrid.ColumnDefinitions.Count;

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Down, currentRow);
			for (var i = 0; i < colAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Vertical);
				else
				{
					UserInterfaceTools.AddControlButtons(this, currentRow, i);
					UserInterfaceTools.AddImageControl(this, currentRow, i);
				}
			}

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Down, currentRow);
			for (var i = 0; i < colAmount; i++)
				UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Horizontal);

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		#endregion // Add events

		#region Merge events

		/// <summary>
		/// Merge the cell from left with the current one.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="mouseButtonEventArgs">The mouse args.</param>
		public void MergeLeftButtonOnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Image)sender);
			var currentCol = GGrid.GetColumn(parent);
			var currentRow = GGrid.GetRow(parent);
			var currentColSpan = GGrid.GetColumnSpan(parent);
			var currentRowSpan = GGrid.GetRowSpan(parent);

			var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol - 2, DirectionsEnum.Left);
			for (var i = 0; i < currentRowSpan; i++)
				UserInterfaceTools.DeleteContent(MainGrid, currentRow + i, currentCol - 1, DirectionsEnum.Left);
			UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Left);

			UserInterfaceTools.AddControlButtons(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
			UserInterfaceTools.AddImageControl(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Merge the cell from right with the current one.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="mouseButtonEventArgs">The mouse args.</param>
		public void MergeRightButtonOnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Image)sender);
			var currentCol = GGrid.GetColumn(parent);
			var currentRow = GGrid.GetRow(parent);
			var currentColSpan = GGrid.GetColumnSpan(parent);
			var currentRowSpan = GGrid.GetRowSpan(parent);

			UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Right);
			for (var i = 0; i < currentRowSpan; i++)
				UserInterfaceTools.DeleteContent(MainGrid, currentRow + i, currentCol + currentColSpan, DirectionsEnum.Right);
			var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol + currentColSpan + 1, DirectionsEnum.Right);

			UserInterfaceTools.AddControlButtons(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
			UserInterfaceTools.AddImageControl(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Merge the cell from top with the current one.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="mouseButtonEventArgs">The mouse args.</param>
		public void MergeTopButtonOnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Image)sender);
			var currentCol = GGrid.GetColumn(parent);
			var currentRow = GGrid.GetRow(parent);
			var currentRowSpan = GGrid.GetRowSpan(parent);
			var currentColSpan = GGrid.GetColumnSpan(parent);

			var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow - 2, currentCol, DirectionsEnum.Up);
			for (var i = 0; i < currentColSpan; i++)
				UserInterfaceTools.DeleteContent(MainGrid, currentRow - 1, currentCol + i, DirectionsEnum.Up);
			UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Up);

			UserInterfaceTools.AddControlButtons(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
			UserInterfaceTools.AddImageControl(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		/// <summary>
		/// Merge the cell from under with the current one.
		/// </summary>
		/// <param name="sender">The button clicked.</param>
		/// <param name="mouseButtonEventArgs">The mouse args.</param>
		public void MergeDownButtonOnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			var parent = UserInterfaceTools.FindParent((Image)sender);
			var currentCol = GGrid.GetColumn(parent);
			var currentRow = GGrid.GetRow(parent);
			var currentRowSpan = GGrid.GetRowSpan(parent);
			var currentColSpan = GGrid.GetColumnSpan(parent);

			UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Down);
			for (var i = 0; i < currentColSpan; i++)
				UserInterfaceTools.DeleteContent(MainGrid, currentRow + currentRowSpan, currentCol + i, DirectionsEnum.Down);
			var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow + currentRowSpan + 1, currentCol, DirectionsEnum.Down);

			UserInterfaceTools.AddControlButtons(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
			UserInterfaceTools.AddImageControl(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

        #endregion // Merge events

        #region Media events

        #region Image events

        /// <summary>
        /// Event when something is dropped on a grid.
        /// </summary>
        /// <param name="sender">The grid impacted, I hope.</param>
        /// <param name="dragEventArgs">Explicit.</param>
        public void ImageGridOnDrop(object sender, DragEventArgs dragEventArgs)
		{
			if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
				MediaControlTools.ImageDrop(sender, dragEventArgs);
		}

		/// <summary>
		/// Event raised on mouse wheel changed on every ImageControl.
		/// </summary>
		/// <param name="sender">The image hopefully.</param>
		/// <param name="mouseWheelEventArgs">The mouse wheel events.</param>
		public void ImageDisplayOnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
		{
			if (sender is Image image)
				MediaControlTools.ImageZoom(image, mouseWheelEventArgs);
			else
			{
				if (sender is GGrid grid)
				{
					image = grid.Children.Cast<Image>().FirstOrDefault(i => i.Name.Equals("image"));
					MediaControlTools.ImageZoom(image, mouseWheelEventArgs);
				}
			}
		}

		/// <summary>
		/// Defines the image to drag.
		/// </summary>
		/// <param name="sender">The image dragged.</param>
		/// <param name="e">Events.</param>
		public void ImageDisplayOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image image))
				return;

			var grid = UserInterfaceTools.FindParent(image);
		    var scaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
            var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
			var imageWidth = image.ActualWidth * scaleTransform.ScaleX;

			if (imageWidth <= grid.ActualWidth && imageHeight <= grid.ActualHeight)
				return;

			image.Cursor = Cursors.SizeAll;
			image.CaptureMouse();

			var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			MousePosition = e.GetPosition(grid);
			OriginPosition = new Point(Math.Round(translateTransform.X, MidpointRounding.AwayFromZero),
				Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero));

		    OriginMouseSpeed = (uint) SystemInformation.MouseSpeed;
            SystemParametersInfo(SPI_SETMOUSESPEED, 0, SlowerMouseSpeed, 0);
		}

		/// <summary>
		/// Release the dragged image.
		/// </summary>
		/// <param name="sender">The image dragged.</param>
		/// <param name="e">Events.</param>
		public void ImageDisplayOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image image))
				return;

			image.ReleaseMouseCapture();
			image.Cursor = Cursors.Arrow;

		    SystemParametersInfo(SPI_SETMOUSESPEED, 0, OriginMouseSpeed, 0);
        }

		/// <summary>
		/// Move the image with the mouse.
		/// </summary>
		/// <param name="sender">The image dragged.</param>
		/// <param name="e">Events.</param>
		public void ImageDisplayOnMouseMove(object sender, MouseEventArgs e)
		{
			if (!(e.Source is Image image) || !image.IsMouseCaptured)
				return;

			var grid = UserInterfaceTools.FindParent(image);
		    var layoutScaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
		    var renderScaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
		    ScaleTransform scaleTransform;
		    if (renderScaleTransform.ScaleX != 0 || renderScaleTransform.ScaleY != 0)
		        scaleTransform = renderScaleTransform;
		    else
		        scaleTransform = layoutScaleTransform;

		    var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
		    var imageWidth = image.ActualWidth * scaleTransform.ScaleX;

            Point relativePoint = image.TransformToAncestor(grid).Transform(new Point(0, 0));
			var mathRelativeY = Math.Round(relativePoint.Y, MidpointRounding.AwayFromZero);
			var mathRelativeX = Math.Round(relativePoint.X, MidpointRounding.AwayFromZero);
			var mathMouseY = Math.Round(MousePosition.Y, MidpointRounding.AwayFromZero);
			var mathMouseX = Math.Round(MousePosition.X, MidpointRounding.AwayFromZero);

			Vector vector = new Vector { X = 0, Y = 0 };
			if (imageHeight >= grid.ActualHeight)
			{
				var vectorY = Math.Round(e.GetPosition(grid).Y, MidpointRounding.AwayFromZero) - mathMouseY;
			    if (mathRelativeY <= 0 && mathRelativeY + vectorY <= 0 && vectorY > 0 ||
			        mathRelativeY + imageHeight >= grid.ActualHeight && mathRelativeY + imageHeight + vectorY >= grid.ActualHeight && vectorY < 0)
                    vector.Y = vectorY;
            }
            if (imageWidth >= grid.ActualWidth)
			{
				var vectorX = Math.Round(e.GetPosition(grid).X, MidpointRounding.AwayFromZero) - mathMouseX;
			    if (mathRelativeX <= 0 && mathRelativeX + vectorX <= 0 && vectorX > 0 ||
			        mathRelativeX + imageWidth >= grid.ActualWidth && mathRelativeX + imageWidth + vectorX >= grid.ActualWidth && vectorX < 0)
			        vector.X = vectorX;
			}

			var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			translateTransform.X = OriginPosition.X + vector.X;
			translateTransform.Y = OriginPosition.Y + vector.Y;

			MousePosition = e.GetPosition(grid);
			OriginPosition = new Point(Math.Round(translateTransform.X, MidpointRounding.AwayFromZero),
				Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero));
		}

		/// <summary>
		/// Prevent the image grid to be resized when the image size change.
		/// </summary>
		/// <param name="sender">The image grid.</param>
		/// <param name="args">Events.</param>
		public void ImageGridSizeChanged(object sender, SizeChangedEventArgs args)
		{
			if (sender is GGrid grid)
			{
			    var child = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
			    var image = child as Image;
			    if (image?.Source == null)
			        return;

			    var scaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
                var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
			    var imageWidth = image.ActualWidth * scaleTransform.ScaleX;
			    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			    var relativePoint = image.TranslatePoint(new Point(0, 0), grid);
			    var mathRelativePositionY = Math.Round(args.NewSize.Height - args.PreviousSize.Height, MidpointRounding.AwayFromZero);
			    var mathRelativePositionX = Math.Round(args.NewSize.Width - args.PreviousSize.Width, MidpointRounding.AwayFromZero);
			    var mathTranslateY = Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero);
			    var mathTranslateX = Math.Round(translateTransform.X, MidpointRounding.AwayFromZero);

                if (imageHeight < grid.ActualHeight)
			        translateTransform.Y = 0;
			    else if (relativePoint.Y > 0)
			        translateTransform.Y = mathTranslateY - mathRelativePositionY;
			    else if (relativePoint.Y + imageHeight < grid.ActualHeight)
			        translateTransform.Y = mathTranslateY + mathRelativePositionY;

			    if (imageWidth < grid.ActualWidth)
			        translateTransform.X = 0;
			    else if (relativePoint.X > 0)
			        translateTransform.X = mathTranslateX - mathRelativePositionX;
			    else if (relativePoint.X + imageWidth < grid.ActualWidth)
			        translateTransform.X = mathTranslateX + mathRelativePositionX;

			    var tmp = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is GGrid);
			    if (!(tmp is GGrid controlImage)) return;
			    var relativePointControl = controlImage.TranslatePoint(new Point(0, 0), grid);
			    var translateTransformGrid = (TranslateTransform)((TransformGroup)controlImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
			    translateTransformGrid.Y -= relativePointControl.Y;
            }
		}

        /// <summary>
        /// Force replace the image control buttons.
        /// </summary>
        /// <param name="sender">Fuuu.</param>
        /// <param name="args">Whatever.</param>
	    public void ImageDisplaySizeChanged(object sender, SizeChangedEventArgs args)
	    {
	        if (!(sender is Image image)) return;

	        var grid = UserInterfaceTools.FindParent(image);
	        var tmp = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is GGrid);
	        if (!(tmp is GGrid controlImage)) return;

	        var relativePointControl = controlImage.TranslatePoint(new Point(0, 0), grid);
	        var translateTransformGrid = (TranslateTransform)((TransformGroup)controlImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
	        translateTransformGrid.Y -= relativePointControl.Y;
	    }

        /// <summary>
        /// Show the image control buttons on enter.
        /// </summary>
        /// <param name="sender">The image control buttons grid.</param>
        /// <param name="args">Who cares?</param>
        public void ImageControlGridMouseEnter(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "imageControlGrid") return;

	        var parent = UserInterfaceTools.FindParent(grid);
	        var image = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
	        if ((image as Image)?.Source == null) return;

            foreach (var button in grid.Children)
                if (button is Button)
                    ((Button)button).Visibility = Visibility.Visible;
        }

	    /// <summary>
        /// Hide the image control buttons on leave.
        /// </summary>
        /// <param name="sender">The grid where the buttons are.</param>
        /// <param name="args">Who cares?</param>
	    public void ImageControlGridMouseLeave(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "imageControlGrid") return;

            foreach (var button in grid.Children)
                if (button is Button)
                    ((Button)button).Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Enlarge the image to make it take the full width of the parent grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    public void ImageControlButtonClick(object sender, RoutedEventArgs args)
	    {
			if (!(sender is Button child)) return;

		    var parentGrid = UserInterfaceTools.FindParent(child);
		    var grandParentGrid = UserInterfaceTools.FindParent(parentGrid);
		    var imageGrid = grandParentGrid.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
		    if (!(imageGrid is Image image)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)image.LayoutTransform).Children.First(tr => tr is ScaleTransform);
			switch (child.Name)
		    {
				case "takeHeightButton":
					var scaleHeight = grandParentGrid.ActualHeight / image.ActualHeight;
					MediaControlTools.ForceZoom(image, scaleHeight, scaleTransform.ScaleY);
					break;

				case "takeWidthButton":
					var scaleWidth = grandParentGrid.ActualWidth / image.ActualWidth;
					MediaControlTools.ForceZoom(image, scaleWidth, scaleTransform.ScaleX);
					break;

				case "resizeButton":
					var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
					scaleTransform.ScaleY = 1;
					scaleTransform.ScaleX = 1;
					translateTransform.X = 0;
					translateTransform.Y = 0;
					break;
		    }
		}

        /// <summary>
        /// Toggle lock on shortcut press (ctrl + a).
        /// </summary>
        /// <param name="sender">Who cares yo ?</param>
        /// <param name="e">Same.</param>
	    private void ShortcutToggleLock(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO : won't update the LOCK/UNLOCK button label.
            IsLocked = !IsLocked;
            UserInterfaceTools.ToggleLockControlButtons(MainGrid, IsLocked);
        }

        #endregion // Image events

        #region Video events

        // EMPTY AS FUCK YOOO

        #endregion // Video events

        #endregion // Media events

        #endregion // Events
    }
}
