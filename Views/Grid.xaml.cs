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
using GridSetter.ViewModels;
using WpfAnimatedGif;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using GGrid = System.Windows.Controls.Grid;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable PossibleInvalidOperationException

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
	    private const UInt32 SlowerMouseSpeed = 3;

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
        /// Keep a ref of the GridSetterViewModel.
        /// </summary>
	    private GridSetterViewModel GridSetterRef { get; }

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
        public Grid(GridSetterViewModel parentRef)
		{
			InitializeComponent();

		    GridSetterRef = parentRef;
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
			UserInterfaceTools.AddGridButtons(this);
			UserInterfaceTools.AddMediaCanvas(this);
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
	        GridSetterRef.ToggleLockGrid();
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
                        UserInterfaceTools.AddGridButtons(this, currentRow + i, currentCol + y);
                        UserInterfaceTools.AddMediaCanvas(this, currentRow + i, currentCol + y);
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
                    UserInterfaceTools.AddGridButtons(this, i, currentCol);
                    UserInterfaceTools.AddMediaCanvas(this, i, currentCol);
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
                    UserInterfaceTools.AddGridButtons(this, i, currentCol);
                    UserInterfaceTools.AddMediaCanvas(this, i, currentCol);
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
                    UserInterfaceTools.AddGridButtons(this, currentRow, i);
                    UserInterfaceTools.AddMediaCanvas(this, currentRow, i);
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
                    UserInterfaceTools.AddGridButtons(this, currentRow, i);
                    UserInterfaceTools.AddMediaCanvas(this, currentRow, i);
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

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
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

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
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

            UserInterfaceTools.AddGridButtons(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
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

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.UpdateControlButtons(MainGrid);
        }

        #endregion // Merge events

        #endregion // Setup

        #region Media events

        /// <summary>
        /// Event when something is dropped on a grid.
        /// </summary>
        /// <param name="sender">The canvas impacted, I hope.</param>
        /// <param name="dragEventArgs">Explicit.</param>
        public void MediaCanvas_OnDrop(object sender, DragEventArgs dragEventArgs)
		{
			if (dragEventArgs.Data.GetDataPresent(DataFormats.FileDrop))
				MediaControlTools.MediaDrop(sender, dragEventArgs);
		}

		/// <summary>
		/// Event raised on mouse wheel changed on every media elements.
		/// </summary>
		/// <param name="sender">The media hopefully.</param>
		/// <param name="mouseWheelEventArgs">The mouse wheel events.</param>
		public void MediaCanvas_OnMouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
		{
		    if (!(sender is Canvas canvas)) return;
			var fElements = canvas.Children.Cast<FrameworkElement>().Where(i => i.Name.Equals("Image") || i.Name.Equals("Video"));
			foreach (var fElement in fElements)
			{
				if (fElement.Visibility != Visibility.Visible) continue;
				MediaControlTools.MediaZoom(fElement, mouseWheelEventArgs);
				return;
			}
		}

		/// <summary>
		/// Pause or play the gif/video on double click.
		/// </summary>
		/// <param name="sender">Dunno.</param>
		/// <param name="args">Dontcare.</param>
		public void Media_MouseDown(object sender, MouseButtonEventArgs args)
        {
	        switch (args.Source)
	        {
		        case Image image:
			        if (image.Source == null) return;
			        if (args.ChangedButton != MouseButton.Left || args.ClickCount != 2) return;

			        var controller = ImageBehavior.GetAnimationController(image);
			        if (controller.IsPaused)
				        controller.Play();
			        else
				        controller.Pause();
			        break;
		        case MediaElement video:
			        if (video.Source == null) return;
			        if (args.ChangedButton != MouseButton.Left || args.ClickCount != 2) return;

			        if ((bool) video.Tag)
				        video.Pause();
			        else
				        video.Play();

			        video.Tag = !(bool) video.Tag;
					break;
	        }
        }

		/// <summary>
		/// Defines the media element to drag.
		/// </summary>
		/// <param name="sender">The media dragged.</param>
		/// <param name="e">Events.</param>
		public void Media_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image) && !(e.Source is MediaElement)) return;
			var fElement = (FrameworkElement) e.Source;
			if (!(UserInterfaceTools.FindParent(fElement) is Canvas canvas)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var elementHeight = fElement.ActualHeight * scaleTransform.ScaleY;
			var elementWidth = fElement.ActualWidth * scaleTransform.ScaleX;

			if (elementWidth <= canvas.ActualWidth && elementHeight <= canvas.ActualHeight) return;

			fElement.Cursor = Cursors.SizeAll;
			fElement.CaptureMouse();

			var translateTransform = (TranslateTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is TranslateTransform);
			MousePosition = e.GetPosition(canvas);
			OriginPosition = new Point(Math.Round(translateTransform.X, MidpointRounding.AwayFromZero),
				Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero));

		    OriginMouseSpeed = (uint) SystemInformation.MouseSpeed;
            SystemParametersInfo(SPI_SETMOUSESPEED, 0, SlowerMouseSpeed, 0);
		}

		/// <summary>
		/// Release the dragged media element.
		/// </summary>
		/// <param name="sender">The media dragged.</param>
		/// <param name="e">Events.</param>
		public void Media_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!(e.Source is Image) && !(e.Source is MediaElement)) return;
			var fElement = (FrameworkElement) e.Source;

			fElement.ReleaseMouseCapture();
			fElement.Cursor = Cursors.Arrow;
		    SystemParametersInfo(SPI_SETMOUSESPEED, 0, OriginMouseSpeed, 0);
        }

		/// <summary>
		/// Move the media element with the mouse.
		/// </summary>
		/// <param name="sender">The element dragged.</param>
		/// <param name="e">Events.</param>
		public void Media_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (!(e.Source is FrameworkElement fElement) || !fElement.IsMouseCaptured) return;
			if (!(UserInterfaceTools.FindParent(fElement) is Canvas canvas)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is ScaleTransform);
		    var controlHeight = fElement.ActualHeight * scaleTransform.ScaleY;
		    var controlWidth = fElement.ActualWidth * scaleTransform.ScaleX;

		    Point relativePoint = fElement.TransformToAncestor(canvas).Transform(new Point(0, 0));
		    var mathRelativeY = Math.Round(relativePoint.Y, MidpointRounding.AwayFromZero);
		    var mathRelativeX = Math.Round(relativePoint.X, MidpointRounding.AwayFromZero);
		    var mathMouseY = Math.Round(MousePosition.Y, MidpointRounding.AwayFromZero);
		    var mathMouseX = Math.Round(MousePosition.X, MidpointRounding.AwayFromZero);

		    Vector vector = new Vector { X = 0, Y = 0 };
		    if (controlHeight >= canvas.ActualHeight)
		    {
		        var vectorY = Math.Round(e.GetPosition(canvas).Y, MidpointRounding.AwayFromZero) - mathMouseY;
		        if (mathRelativeY <= 0 && mathRelativeY + vectorY <= 0 && vectorY > 0 ||
		            mathRelativeY + controlHeight >= canvas.ActualHeight && mathRelativeY + controlHeight + vectorY >= canvas.ActualHeight && vectorY < 0)
		            vector.Y = vectorY;
		    }
		    if (controlWidth >= canvas.ActualWidth)
		    {
		        var vectorX = Math.Round(e.GetPosition(canvas).X, MidpointRounding.AwayFromZero) - mathMouseX;
		        if (mathRelativeX <= 0 && mathRelativeX + vectorX <= 0 && vectorX > 0 ||
		            mathRelativeX + controlWidth >= canvas.ActualWidth && mathRelativeX + controlWidth + vectorX >= canvas.ActualWidth && vectorX < 0)
		            vector.X = vectorX;
		    }

		    var translateTransform = (TranslateTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is TranslateTransform);
		    translateTransform.X = OriginPosition.X + vector.X;
		    translateTransform.Y = OriginPosition.Y + vector.Y;

		    MousePosition = e.GetPosition(canvas);
		    OriginPosition = new Point(Math.Round(translateTransform.X, MidpointRounding.AwayFromZero),
		        Math.Round(translateTransform.Y, MidpointRounding.AwayFromZero));
        }

		/// <summary>
		/// Translate the media content on canvas resized.
		/// </summary>
		/// <param name="sender">The canvas.</param>
		/// <param name="e">Events.</param>
		public void MediaCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		    if (!(sender is Canvas canvas)) return;
			if (!(canvas.Children.Cast<UIElement>().FirstOrDefault(u => u is FrameworkElement) is FrameworkElement fElement)) return;

		    var scaleTransform = (ScaleTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is ScaleTransform);
		    var translateTransform = (TranslateTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is TranslateTransform);
		    var imageHeight = fElement.ActualHeight * scaleTransform.ScaleY;
		    var imageWidth = fElement.ActualWidth * scaleTransform.ScaleX;
		    var relativePoint = fElement.TranslatePoint(new Point(0, 0), canvas);

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
        /// Show the media control buttons on enter.
        /// </summary>
        /// <param name="sender">The media control buttons grid.</param>
        /// <param name="args">Who cares?</param>
        public void MediaButtons_OnMouseEnter(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "MediaButtons") return;
	        if (!(UserInterfaceTools.FindParent(grid) is Canvas parent)) return;

	        var image = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
	        var video = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement);
			if ((image as Image)?.Source == null && (video as MediaElement)?.Source == null) return;

            foreach (var child in grid.Children)
                if (child is Button button)
                    button.Visibility = Visibility.Visible;
        }

	    /// <summary>
        /// Hide the media control buttons on leave.
        /// </summary>
        /// <param name="sender">The grid where the buttons are.</param>
        /// <param name="args">Who cares?</param>
	    public void MediaButtons_OnMouseLeave(object sender, MouseEventArgs args)
	    {
	        if (!(sender is GGrid grid)) return;
	        if (grid.Name != "MediaButtons") return;

            foreach (var child in grid.Children)
                if (child is Button button)
                    button.Visibility = Visibility.Hidden;
        }

		/// <summary>
		/// Update the size of the media control on the click of a button.
		/// </summary>
		/// <param name="sender">.</param>
		/// <param name="args">!</param>
		public void MediaControl_OnClick(object sender, RoutedEventArgs args)
		{
			if (!(sender is Button child)) return;

			var grid = UserInterfaceTools.FindParent(child);
			var canvas = UserInterfaceTools.FindParent(grid);
			var image = ((Canvas)canvas).Children.Cast<UIElement>().FirstOrDefault(e => e is Image) as Image;
			var video = ((Canvas)canvas).Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement) as MediaElement;

			FrameworkElement fElement;
			if (image?.Source == null && video?.Source != null)
				fElement = video;
			else
				fElement = image;

			var scaleTransform = (ScaleTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is ScaleTransform);
			var translateTransform = (TranslateTransform)((TransformGroup)fElement.RenderTransform).Children.First(tr => tr is TranslateTransform);
			translateTransform.X = 0;
			translateTransform.Y = 0;
			switch (child.Name)
			{
				case "TakeHeightButton":
					var scaleHeight = Math.Round(canvas.ActualHeight / fElement.ActualHeight, 2);
					scaleTransform.ScaleY = scaleHeight;
					scaleTransform.ScaleX = scaleHeight;
					break;

				case "TakeWidthButton":
					var scaleWidth = Math.Round(canvas.ActualWidth / fElement.ActualWidth, 2);
					scaleTransform.ScaleY = scaleWidth;
					scaleTransform.ScaleX = scaleWidth;
					break;

				case "ResizeButton":
					scaleTransform.ScaleY = 1;
					scaleTransform.ScaleX = 1;
					break;

				case "MuteButton":

					break;
			}
		}

		#region Video events

		/// <summary>
		/// Replay video on end.
		/// </summary>
		/// <param name="sender">Blabla.</param>
		/// <param name="routedEventArgs">Bla.</param>
		public void Video_OnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
		{
			if (!(sender is MediaElement video)) return;
			video.Position = TimeSpan.FromMilliseconds(1);
		}

		#endregion // Video events

		#endregion // Media events

		#endregion // Events
	}
}
