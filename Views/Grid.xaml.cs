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
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using GGrid = System.Windows.Controls.Grid;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleInvalidOperationException

/* TODO : 
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
			MainGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 195, Width = new GridLength(1, GridUnitType.Star) });
			MainGrid.RowDefinitions.Add(new RowDefinition { MinHeight = 100, Height = new GridLength(1, GridUnitType.Star) });

			AddChild(MainGrid);
			UserInterfaceTools.AddControlButtons(this);
			UserInterfaceTools.AddImageControl(this);
			UserInterfaceTools.UpdateControlButtons(MainGrid);

		    ToggleLockCommand.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
		}

        #endregion // Constructors

        #region Events

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

        #region Setup

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

        #region Add events

        /// <summary>
        /// Add a column on the left of the grid containing the clicked button.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="routedEventArgs">The routed args.</param>
        public void AddLeftColButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent);
            var rowAmount = MainGrid.RowDefinitions.Count;

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
            for (var i = 0; i < rowAmount; i++)
                UserInterfaceTools.AddGridSplitter(MainGrid, i, currentCol, DirectionsEnum.Vertical);

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { MinWidth = 195, Width = new GridLength(1, GridUnitType.Star) });
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
        public void AddRightColButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent) + GGrid.GetColumnSpan(parent);
            var rowAmount = MainGrid.RowDefinitions.Count;

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { MinWidth = 195, Width = new GridLength(1, GridUnitType.Star) });
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
        public void AddUpRowButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentRow = GGrid.GetRow(parent);
            var colAmount = MainGrid.ColumnDefinitions.Count;

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
            for (var i = 0; i < colAmount; i++)
                UserInterfaceTools.AddGridSplitter(MainGrid, currentRow, i, DirectionsEnum.Horizontal);

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { MinHeight = 100, Height = new GridLength(1, GridUnitType.Star) });
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
        public void AddDownRowButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentRow = GGrid.GetRow(parent) + GGrid.GetRowSpan(parent);
            var colAmount = MainGrid.ColumnDefinitions.Count;

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { MinHeight = 100, Height = new GridLength(1, GridUnitType.Star) });
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
        public void MergeLeftButton_OnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
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
        public void MergeRightButton_OnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
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
        public void MergeTopButton_OnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
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
        public void MergeDownButton_OnClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
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

        #endregion // Setup

        #region Image events

        /// <summary>
        /// Event when something is dropped on a grid.
        /// </summary>
        /// <param name="sender">The grid impacted, I hope.</param>
        /// <param name="dragEventArgs">Explicit.</param>
        public void ImageCanvas_OnDrop(object sender, DragEventArgs dragEventArgs)
		{
			if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
				MediaControlTools.ImageDrop(sender, dragEventArgs);
		}

		/// <summary>
		/// Event raised on mouse wheel changed on every ImageControl.
		/// </summary>
		/// <param name="sender">The image hopefully.</param>
		/// <param name="mouseWheelEventArgs">The mouse wheel events.</param>
		public void ImageCanvas_OnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
		{
		    if (!(sender is Canvas canvas)) return;
			var image = canvas.Children.Cast<Image>().FirstOrDefault(i => i.Name.Equals("Image"));
			MediaControlTools.ImageZoom(image, mouseWheelEventArgs);
		}

        /// <summary>
        /// Pause or play the gif on double click.
        /// </summary>
        /// <param name="sender">Dunno.</param>
        /// <param name="args">Dontcare.</param>
	    public void Image_MouseDown(object sender, MouseButtonEventArgs args)
	    {
	        if (!(args.Source is Image image)) return;
	        if (image.Source == null) return;
	        if (args.ChangedButton != MouseButton.Left || args.ClickCount != 2) return;

	        var controller = ImageBehavior.GetAnimationController(image);
            if (controller == null) return;

	        if (controller.IsPaused)
	            controller.Play();
	        else
	            controller.Pause();
	    }

		/// <summary>
		/// Defines the image to drag.
		/// </summary>
		/// <param name="sender">The image dragged.</param>
		/// <param name="e">Events.</param>
		public void Image_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image image)) return;
		    if (!(UserInterfaceTools.FindParent(image) is Canvas canvas)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
			var imageWidth = image.ActualWidth * scaleTransform.ScaleX;

			if (imageWidth <= canvas.ActualWidth && imageHeight <= canvas.ActualHeight) return;

			image.Cursor = Cursors.SizeAll;
			image.CaptureMouse();

			var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
			MousePosition = e.GetPosition(canvas);
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
		public void Image_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image image)) return;

			image.ReleaseMouseCapture();
			image.Cursor = Cursors.Arrow;
		    SystemParametersInfo(SPI_SETMOUSESPEED, 0, OriginMouseSpeed, 0);
        }

		/// <summary>
		/// Move the image with the mouse.
		/// </summary>
		/// <param name="sender">The image dragged.</param>
		/// <param name="e">Events.</param>
		public void Image_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (!(e.Source is Image image) || !image.IsMouseCaptured) return;
		    if (!(UserInterfaceTools.FindParent(image) is Canvas canvas)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
		    var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
		    var imageWidth = image.ActualWidth * scaleTransform.ScaleX;

		    Point relativePoint = image.TransformToAncestor(canvas).Transform(new Point(0, 0));
		    var mathRelativeY = Math.Round(relativePoint.Y, MidpointRounding.AwayFromZero);
		    var mathRelativeX = Math.Round(relativePoint.X, MidpointRounding.AwayFromZero);
		    var mathMouseY = Math.Round(MousePosition.Y, MidpointRounding.AwayFromZero);
		    var mathMouseX = Math.Round(MousePosition.X, MidpointRounding.AwayFromZero);

		    Vector vector = new Vector { X = 0, Y = 0 };
		    if (imageHeight >= canvas.ActualHeight)
		    {
		        var vectorY = Math.Round(e.GetPosition(canvas).Y, MidpointRounding.AwayFromZero) - mathMouseY;
		        if (mathRelativeY <= 0 && mathRelativeY + vectorY <= 0 && vectorY > 0 ||
		            mathRelativeY + imageHeight >= canvas.ActualHeight && mathRelativeY + imageHeight + vectorY >= canvas.ActualHeight && vectorY < 0)
		            vector.Y = vectorY;
		    }
		    if (imageWidth >= canvas.ActualWidth)
		    {
		        var vectorX = Math.Round(e.GetPosition(canvas).X, MidpointRounding.AwayFromZero) - mathMouseX;
		        if (mathRelativeX <= 0 && mathRelativeX + vectorX <= 0 && vectorX > 0 ||
		            mathRelativeX + imageWidth >= canvas.ActualWidth && mathRelativeX + imageWidth + vectorX >= canvas.ActualWidth && vectorX < 0)
		            vector.X = vectorX;
		    }

		    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
		    translateTransform.X = OriginPosition.X + vector.X;
		    translateTransform.Y = OriginPosition.Y + vector.Y;

		    MousePosition = e.GetPosition(canvas);
		    OriginPosition = new Point(Math.Round(translateTransform.X, MidpointRounding.AwayFromZero),
		        Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero));
        }

		/// <summary>
		/// Prevent the image grid to be resized when the image size change.
		/// </summary>
		/// <param name="sender">The image grid.</param>
		/// <param name="args">Events.</param>
		public void ImageCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		    if (!(sender is Canvas canvas)) return;
		    var child = canvas.Children.Cast<UIElement>().FirstOrDefault(u => u is Image);
		    if (!(child is Image image)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
		    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
		    var imageHeight = image.ActualHeight * scaleTransform.ScaleY;
		    var imageWidth = image.ActualWidth * scaleTransform.ScaleX;
		    var relativePoint = image.TranslatePoint(new Point(0, 0), canvas);

		    var previousHeight = Math.Round(e.PreviousSize.Height, 2);
		    var previousWidth = Math.Round(e.PreviousSize.Width, 2);
		    var newHeight = Math.Round(e.NewSize.Height, 2);
		    var newWidth = Math.Round(e.NewSize.Width, 2);

		    if (e.NewSize.Height > e.PreviousSize.Height)
		    {
		        if (imageHeight < canvas.ActualHeight)
		            translateTransform.Y = 0;
		        else if (imageHeight >= canvas.ActualHeight)
		        {
		            if (relativePoint.Y > 0)
		                translateTransform.Y -= Math.Round(newHeight - previousHeight, 2);
		            else if (relativePoint.Y + imageHeight < canvas.ActualHeight)
		                translateTransform.Y += Math.Round(newHeight - previousHeight, 2);
		        }
		    }

		    if (e.NewSize.Width > e.PreviousSize.Width)
		    {
		        if (imageWidth < canvas.ActualWidth)
		            translateTransform.X = 0;
		        else if (imageWidth >= canvas.ActualWidth)
		        {
		            if (relativePoint.X > 0)
		                translateTransform.X -= Math.Round(newWidth - previousWidth, 2);
		            else if (relativePoint.X + imageWidth < canvas.ActualWidth)
		                translateTransform.X += Math.Round(newWidth - previousWidth, 2);
		        }
		    }
        }

        /// <summary>
        /// Force replace the image control buttons.
        /// </summary>
        /// <param name="sender">Fuuu.</param>
        /// <param name="args">Whatever.</param>
	    public void Image_SizeChanged(object sender, SizeChangedEventArgs args)
	    {
	        //if (!(sender is Image image)) return;

	        //var grid = UserInterfaceTools.FindParent(image);
	        //var tmp = grid.Children.Cast<UIElement>().FirstOrDefault(e => e is GGrid);
	        //if (!(tmp is GGrid controlImage)) return;

	        //var relativePointControl = controlImage.TranslatePoint(new Point(0, 0), grid);
	        //var translateTransformGrid = (TranslateTransform)((TransformGroup)controlImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
	        //translateTransformGrid.Y -= relativePointControl.Y;
	    }

        /// <summary>
        /// Show the image control buttons on enter.
        /// </summary>
        /// <param name="sender">The image control buttons grid.</param>
        /// <param name="args">Who cares?</param>
        public void ImageButtons_OnMouseEnter(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "ImageButtons") return;
	        if (!(UserInterfaceTools.FindParent(grid) is Canvas parent)) return;

	        var image = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
	        if ((image as Image)?.Source == null) return;

            foreach (var child in grid.Children)
                if (child is Button button)
                    button.Visibility = Visibility.Visible;
        }

	    /// <summary>
        /// Hide the image control buttons on leave.
        /// </summary>
        /// <param name="sender">The grid where the buttons are.</param>
        /// <param name="args">Who cares?</param>
	    public void ImageButtons_OnMouseLeave(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "ImageButtons") return;

            foreach (var child in grid.Children)
                if (child is Button button)
                    button.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Update the size of the image on the click of a button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
	    public void ImageControl_OnClick(object sender, RoutedEventArgs args)
	    {
            //if (!(sender is Button child)) return;

            //   var parentGrid = UserInterfaceTools.FindParent(child);
            //   var grandParentGrid = UserInterfaceTools.FindParent(parentGrid);
            //   var imageGrid = grandParentGrid.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
            //   if (!(imageGrid is Image image)) return;

            //         var renderScaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
            //      ScaleTransform scaleTransform;
            //      if (renderScaleTransform.ScaleX != 1 || renderScaleTransform.ScaleY != 1)
            //          scaleTransform = renderScaleTransform;
            //      else
            //          scaleTransform = layoutScaleTransform;

            //         switch (child.Name)
            //   {
            //	case "TakeHeightButton":
            //		var scaleHeight = grandParentGrid.ActualHeight / image.ActualHeight;
            //		MediaControlTools.ForceZoom(image, scaleHeight, scaleTransform.ScaleY);
            //		break;

            //	case "TakeWidthButton":
            //		var scaleWidth = grandParentGrid.ActualWidth / image.ActualWidth;
	        //		MediaControlTools.ForceZoom(image, scaleWidth, scaleTransform.ScaleX);
	        //		break;

	        //	case "ResizeButton":
            //		var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            //		scaleTransform.ScaleY = 1;
            //		scaleTransform.ScaleX = 1;
            //		translateTransform.X = 0;
            //		translateTransform.Y = 0;
            //		break;
            //   }
        }

        #endregion // Image events

        #endregion // Events
    }
}
