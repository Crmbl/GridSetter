using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using GridSetter.Utils;
using GridSetter.Utils.Enums;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using GGrid = System.Windows.Controls.Grid;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

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
        /// Defines the mouse speed when starting the drag event.
        /// </summary>
        private UInt32 OriginMouseSpeed { get; set; }

        /// <summary>
        /// Determines the state of the grid.
        /// </summary>
        public bool IsLocked { get; set; }

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

        #region Split event

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
						UserInterfaceTools.AddGridSplitter(MainGrid, currentRow + i, currentCol + y, DirectionsEnum.Vertical);
					else if (i % 2 != 0)
						UserInterfaceTools.AddGridSplitter(MainGrid, currentRow + i, currentCol + y, DirectionsEnum.Horizontal);
					else
					{
						UserInterfaceTools.AddControlButtons(this, currentRow + i, currentCol + y);
						UserInterfaceTools.AddImageControl(this, currentRow + i, currentCol + y);
					}
				}
			}

			UserInterfaceTools.UpdateControlButtons(MainGrid);
		}

		#endregion // Split event

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
				UserInterfaceTools.AddGridSplitter(MainGrid, i, currentCol, DirectionsEnum.Vertical);

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
			for (var i = 0; i < rowAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(MainGrid, i, currentCol, DirectionsEnum.Horizontal);
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
					UserInterfaceTools.AddGridSplitter(MainGrid, i, currentCol, DirectionsEnum.Horizontal);
				else
				{
					UserInterfaceTools.AddControlButtons(this, i, currentCol);
					UserInterfaceTools.AddImageControl(this, i, currentCol);
				}
			}

			MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Right, currentCol);
			for (var i = 0; i < rowAmount; i++)
				UserInterfaceTools.AddGridSplitter(MainGrid, i, currentCol, DirectionsEnum.Vertical);

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
				UserInterfaceTools.AddGridSplitter(MainGrid, currentRow, i, DirectionsEnum.Horizontal);

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
			for (var i = 0; i < colAmount; i++)
			{
				if (i % 2 != 0)
					UserInterfaceTools.AddGridSplitter(MainGrid, currentRow, i, DirectionsEnum.Vertical);
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
					UserInterfaceTools.AddGridSplitter(MainGrid, currentRow, i, DirectionsEnum.Vertical);
				else
				{
					UserInterfaceTools.AddControlButtons(this, currentRow, i);
					UserInterfaceTools.AddImageControl(this, currentRow, i);
				}
			}

			MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
			UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Down, currentRow);
			for (var i = 0; i < colAmount; i++)
				UserInterfaceTools.AddGridSplitter(MainGrid, currentRow, i, DirectionsEnum.Horizontal);

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
		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
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
		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
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

			    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
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

		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
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
