using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CustomShapeWpfButton;
using GridSetter.Utils;
using GridSetter.Utils.Enums;
using GridSetter.ViewModels;
using WpfAnimatedGif;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using GGrid = System.Windows.Controls.Grid;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace GridSetter.Views
{
    /// <summary>
    /// Interaction logic for Grid.xaml
    /// </summary>
    public sealed partial class Grid
    {
        #region DLL imports 

        [DllImport("User32.dll")]
        public static extern Boolean SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, UInt32 pvParam, UInt32 fWinIni);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        #endregion // DLL imports

        #region Constants

        /// <summary>
        /// Defines the mouse speed when dragging.
        /// </summary>
        private const UInt32 SlowerMouseSpeed = 3;

        /// <summary>
        /// Constant to set the mouse speed.
        /// </summary>
        private const UInt32 SPI_SETMOUSESPEED = 0x0071;

        /// <summary>
        /// Constant for nomove flag.
        /// </summary>
	    private const UInt32 SWP_NOMOVE = 0x0002;

        /// <summary>
        /// Constant for nosize flag.
        /// </summary>
	    private const UInt32 SWP_NOSIZE = 0x0001;

        /// <summary>
        /// Constant for noZorder flag.
        /// </summary>
	    private const UInt32 SWP_NOZORDER = 0x0004;

        /// <summary>
        /// Constant for showwindow flag.
        /// </summary>
	    private const UInt32 SWP_SHOWWINDOW = 0x0040;

        /// <summary>
        /// Constant for the minimum cell height.
        /// </summary>
	    private const int CellMinHeight = 0; // 100

        /// <summary>
        /// Constant for the minimum cell width.
        /// </summary>
	    private const int CellMinWidth = 0; // 195

        #endregion // Constants

        #region Properties

        /// <summary>
        /// Defines the main grid of the app.
        /// </summary>
        public GGrid MainGrid { get; }

        /// <summary>
        /// Defines the handle for the windows Taskbar.
        /// </summary>
        private IntPtr TaskbarHandle { get; set; }

        /// <summary>
        /// Tells if this is the primary monitor.
        /// </summary>
        private bool IsPrimaryMonitor { get; set; }

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

        /// <summary>
        /// Is switching monitor ?
        /// </summary>
        private bool IsSwitching { get; set; }

        /// <summary>
        /// Defines the mouse timer handling hide/show.
        /// </summary>
        private System.Timers.Timer MouseTimer { get; set; }

        /// <summary>
        /// Used to block timeSlider value change event triggering.
        /// </summary>
        private bool BlockTimeValueChangeEvent { get; set; }

        #region Static

        /// <summary>
        /// The routed command for the shortcut.
        /// </summary>
        public static readonly RoutedCommand ToggleLockCommand = new RoutedCommand();

        /// <summary>
        /// The routed command for the shortcut.
        /// </summary>
        public static readonly RoutedCommand ToDesktopCommand = new RoutedCommand();

        /// <summary>
        /// The routed command for the shortcut.
        /// </summary>
        public static readonly RoutedCommand ToggleTaskbarCommand = new RoutedCommand();

        /// <summary>
        /// The routed command for the shortcut
        /// </summary>
        public static readonly RoutedCommand SwitchMonitorCommand = new RoutedCommand();

        /// <summary>
        /// The routed command for the shortcut
        /// </summary>
        public static readonly RoutedCommand ExportGridCommand = new RoutedCommand();

        #endregion // Static

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Grid(GridSetterViewModel parentRef, GridViewModel importGrid = null)
        {
            InitializeComponent();

            GridSetterRef = parentRef;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Left = 0;
            Top = 0;

            var mousePosition = System.Windows.Forms.Control.MousePosition;
            Screen currentScreen = Screen.FromPoint(new System.Drawing.Point(mousePosition.X, mousePosition.Y));
            Width = currentScreen.Bounds.Width;
            Height = currentScreen.Bounds.Height;
            Left = currentScreen.Bounds.Left;
            Top = currentScreen.Bounds.Top;
            IsPrimaryMonitor = Screen.FromPoint(new System.Drawing.Point(mousePosition.X, mousePosition.Y)).Primary;
            MainGrid = new GGrid { ShowGridLines = false };
            SetWindowPos(new WindowInteropHelper(this).Handle, IntPtr.Zero, (int)Left, (int)Top, (int)Width, (int)Height, SWP_NOSIZE);

            if (importGrid == null)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = CellMinWidth, Width = new GridLength(1, GridUnitType.Star) });
                MainGrid.RowDefinitions.Add(new RowDefinition { MinHeight = CellMinHeight, Height = new GridLength(1, GridUnitType.Star) });

                AddChild(MainGrid);
                UserInterfaceTools.AddGridButtons(this);
                UserInterfaceTools.AddMediaCanvas(this);
            }
            else
            {
                for (var z = 0; z < importGrid.NbRow; z++)
                {
                    if (z % 2 == 0)
                        MainGrid.RowDefinitions.Add(new RowDefinition { MinHeight = CellMinHeight, Height = new GridLength(1, GridUnitType.Star) });
                    else
                    {
                        MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
                        for (var i = 0; i < importGrid.NbCol; i++)
                            UserInterfaceTools.AddGridSplitter(this, MainGrid, z, i, DirectionsEnum.Horizontal);
                    }
                }

                for (var z = 0; z < importGrid.NbCol; z++)
                {
                    if (z % 2 == 0)
                    {
                        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = CellMinWidth, Width = new GridLength(1, GridUnitType.Star) });
                        for (var i = 0; i < importGrid.NbRow; i += 2)
                        {
                            UserInterfaceTools.AddGridButtons(this, i, z);
                            UserInterfaceTools.AddMediaCanvas(this, i, z);
                        }
                    }
                    else
                    {
                        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
                        for (var i = 0; i < importGrid.NbRow; i += 2)
                            UserInterfaceTools.AddGridSplitter(this, MainGrid, i, z, DirectionsEnum.Vertical);
                    }
                }

                UserInterfaceTools.UpdateControlButtons(MainGrid);
                UserInterfaceTools.ScaleControlButtons(MainGrid);

                foreach (var cell in importGrid.Cells)
                {
                    var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == cell.Row && GGrid.GetColumn(u) == cell.Col && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                    UserInterfaceTools.ResetMedia(this, mediaCanvas);
                    UserInterfaceTools.DeleteContent(MainGrid, cell.Row, cell.Col, DirectionsEnum.Down);

                    if (cell.ColSpan > 1 && cell.RowSpan == 1)
                    {
                        for (var i = 1; i < cell.ColSpan; i++)
                        {
                            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == cell.Row && GGrid.GetColumn(u) == (cell.Col + i) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                            UserInterfaceTools.ResetMedia(this, mediaCanvas);
                            UserInterfaceTools.DeleteContent(MainGrid, cell.Row, cell.Col + i, DirectionsEnum.Right);
                        }
                    }
                    else if (cell.RowSpan > 1 && cell.ColSpan == 1)
                    {
                        for (var i = 1; i < cell.RowSpan; i++)
                        {
                            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (cell.Row + i) && GGrid.GetColumn(u) == cell.Col && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                            UserInterfaceTools.ResetMedia(this, mediaCanvas);
                            UserInterfaceTools.DeleteContent(MainGrid, cell.Row + i, cell.Col, DirectionsEnum.Down);
                        }
                    }
                    else if (cell.ColSpan > 1 && cell.ColSpan > 1)
                    {
                        for (var x = 0; x < cell.RowSpan; x++)
                        {
                            for (var i = 0; i < cell.ColSpan; i++)
                            {
                                mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (cell.Row + x) && GGrid.GetColumn(u) == (cell.Col + i) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                                UserInterfaceTools.ResetMedia(this, mediaCanvas);
                                UserInterfaceTools.DeleteContent(MainGrid, cell.Row + x, cell.Col + i, DirectionsEnum.Right);
                            }
                        }
                    }

                    UserInterfaceTools.AddGridButtons(this, cell.Row, cell.Col, rowSpan: cell.RowSpan, colSpan: cell.ColSpan);
                    UserInterfaceTools.AddMediaCanvas(this, cell.Row, cell.Col, rowSpan: cell.RowSpan, colSpan: cell.ColSpan);
                }

                AddChild(MainGrid);
            }

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);

            ToggleLockCommand.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            ToDesktopCommand.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            ToggleTaskbarCommand.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            ExportGridCommand.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control));
            SwitchMonitorCommand.InputGestures.Add(new KeyGesture(Key.D1, ModifierKeys.Control));

            Loaded += Grid_Loaded;
            StateChanged += Grid_StateChanged;
            MouseMove += Grid_MouseMove;

            MouseTimer = new System.Timers.Timer(3000) { AutoReset = false };
            MouseTimer.Elapsed += delegate { Application.Current.Dispatcher.Invoke(new Action(() => { Mouse.OverrideCursor = Cursors.None; })); };
        }

        #endregion // Constructors

        #region Events

        /// <summary>
        /// Show the mouse cursor and reset the timer on movement.
        /// </summary>
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (GridSetterRef.ParentRef.WindowState != WindowState.Minimized && MouseTimer != null)
            {
                MouseTimer.Stop();
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            else if (MouseTimer != null)
            {
                MouseTimer.Stop();
                Mouse.OverrideCursor = null;
                MouseTimer.Start();
            }
        }

        /// <summary>
        /// Set the right flag on window loaded.
        /// </summary>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Grid senderWindow)
                senderWindow.WindowState = WindowState.Maximized;

            TaskbarHandle = IsPrimaryMonitor ? FindWindow("Shell_TrayWnd", null) : FindWindow("Shell_SecondaryTrayWnd", null);
        }

        /// <summary>
        /// Set Maximized after quick hack minimized.
        /// </summary>
        private void Grid_StateChanged(object sender, EventArgs e)
        {
            if (!IsSwitching) return;
            IsSwitching = false;
            SwitchMonitor();
            WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Toggle lock on shortcut press (ctrl + a).
        /// </summary>
        private void ShortcutToggleLock(object sender, ExecutedRoutedEventArgs e)
        {
            GridSetterRef.ToggleLockGrid();
        }

        /// <summary>
        /// Switch on another monitor (ctrl + 1).
        /// </summary>
        private void SwitchMonitor()
        {
            var handle = new WindowInteropHelper(this).Handle;
            var currentScreen = Screen.FromHandle(handle);
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.DeviceName == currentScreen.DeviceName)
                    continue;

                IsPrimaryMonitor = screen.Primary;
                Width = screen.Bounds.Width;
                Height = screen.Bounds.Height;
                Left = screen.Bounds.Left;
                Top = screen.Bounds.Top;

                SetWindowPos(handle, IntPtr.Zero, (int)Left, (int)Top, (int)Width, (int)Height, SWP_NOSIZE);
                break;
            }
        }

        /// <summary>
        /// Export grid to a file on shortcut press (ctrl + x).
        /// </summary>
        private void ShortcutExportGrid(object sender, ExecutedRoutedEventArgs e)
        {
            GridSetterRef.ExportGrid();
        }

        /// <summary>
        /// Init switch on another monitor (ctrl + 1).
        /// </summary>
        private void InitSwitchMonitor(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            IsSwitching = true;
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// To desktop on shortcut press (ctrl + q).
        /// </summary>
        private void ShortcutToDesktop(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Toggle visibility of taskbar on shortcut press (ctrl + w).
        /// </summary>
	    private void ShortcutToggleTaskbar(object sender, ExecutedRoutedEventArgs e)
        {
            SetWindowPos(TaskbarHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        #region Setup

        /// <summary>
        /// Event when mouse leaves the bounds of ControlGrid
        /// </summary>
        public void ControlGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            var grid = (GGrid)sender;
            var arcButton = grid.Children.Cast<UIElement>().FirstOrDefault(x => x is ArcButton);
            if (arcButton == null || !(arcButton is ArcButton) || (arcButton is ArcButton && arcButton.Visibility == Visibility.Hidden))
                return;

            arcButton.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Event when mouse enters the bounds of ControlGrid
        /// </summary>
        public void ControlGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            UserInterfaceTools.ScaleControlButtons(MainGrid);

            var grid = (GGrid)sender;
            var arcButton = grid.Children.Cast<UIElement>().FirstOrDefault(x => x is ArcButton);
            if (arcButton == null || !(arcButton is ArcButton) || (arcButton is ArcButton && arcButton.Visibility == Visibility.Visible))
                return;

            arcButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Remove a column on click.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="e">The routed args.</param>
        public void SplitButton_OnClick(object sender, RoutedEventArgs e)
        {
            var parent = UserInterfaceTools.FindParent((ArcButton)sender);
            var currentCol = GGrid.GetColumn(parent);
            var currentRow = GGrid.GetRow(parent);
            var actualColSpan = GGrid.GetColumnSpan(parent);
            var actualRowSpan = GGrid.GetRowSpan(parent);

            var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
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
                        UserInterfaceTools.AddGridButtons(this, currentRow + i, currentCol + y);
                        UserInterfaceTools.AddMediaCanvas(this, currentRow + i, currentCol + y);
                    }
                }
            }

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        #region Add events

        /// <summary>
        /// Add a column on the left of the grid containing the clicked button.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="routedEventArgs">The routed args.</param>
        public void AddLeftColButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((ArcButton)sender);
            var currentCol = GGrid.GetColumn(parent);
            var rowAmount = MainGrid.RowDefinitions.Count;

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
            for (var i = 0; i < rowAmount; i++)
                UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Vertical);

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { MinWidth = CellMinWidth, Width = new GridLength(1, GridUnitType.Star) });
            UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Left, currentCol);
            for (var i = 0; i < rowAmount; i++)
            {
                if (i % 2 != 0)
                    UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Horizontal);
                else
                {
                    UserInterfaceTools.AddGridButtons(this, i, currentCol);
                    UserInterfaceTools.AddMediaCanvas(this, i, currentCol);
                }
            }

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Add a column on the right of the grid containing the clicked button.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="routedEventArgs">The routed args.</param>
        public void AddRightColButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((ArcButton)sender);
            var currentCol = GGrid.GetColumn(parent) + GGrid.GetColumnSpan(parent);
            var rowAmount = MainGrid.RowDefinitions.Count;

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { MinWidth = CellMinWidth, Width = new GridLength(1, GridUnitType.Star) });
            UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Right, currentCol);
            for (var i = 0; i < rowAmount; i++)
            {
                if (i % 2 != 0)
                    UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Horizontal);
                else
                {
                    UserInterfaceTools.AddGridButtons(this, i, currentCol);
                    UserInterfaceTools.AddMediaCanvas(this, i, currentCol);
                }
            }

            MainGrid.ColumnDefinitions.Insert(currentCol, new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsColumn(MainGrid, DirectionsEnum.Right, currentCol);
            for (var i = 0; i < rowAmount; i++)
                UserInterfaceTools.AddGridSplitter(this, MainGrid, i, currentCol, DirectionsEnum.Vertical);

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Add a row on the top of the grid containing the clicked button.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="routedEventArgs">The routed args.</param>
        public void AddUpRowButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((ArcButton)sender);
            var currentRow = GGrid.GetRow(parent);
            var colAmount = MainGrid.ColumnDefinitions.Count;

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
            for (var i = 0; i < colAmount; i++)
                UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Horizontal);

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { MinHeight = CellMinHeight, Height = new GridLength(1, GridUnitType.Star) });
            UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Up, currentRow);
            for (var i = 0; i < colAmount; i++)
            {
                if (i % 2 != 0)
                    UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Vertical);
                else
                {
                    UserInterfaceTools.AddGridButtons(this, currentRow, i);
                    UserInterfaceTools.AddMediaCanvas(this, currentRow, i);
                }
            }

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Add a row under the grid containing the clicked button.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="routedEventArgs">The routed args.</param>
        public void AddDownRowButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((ArcButton)sender);
            var currentRow = GGrid.GetRow(parent) + GGrid.GetRowSpan(parent);
            var colAmount = MainGrid.ColumnDefinitions.Count;

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { MinHeight = CellMinHeight, Height = new GridLength(1, GridUnitType.Star) });
            UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Down, currentRow);
            for (var i = 0; i < colAmount; i++)
            {
                if (i % 2 != 0)
                    UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Vertical);
                else
                {
                    UserInterfaceTools.AddGridButtons(this, currentRow, i);
                    UserInterfaceTools.AddMediaCanvas(this, currentRow, i);
                }
            }

            MainGrid.RowDefinitions.Insert(currentRow, new RowDefinition { Height = new GridLength(5, GridUnitType.Pixel) });
            UserInterfaceTools.MoveUiElementsRow(MainGrid, DirectionsEnum.Down, currentRow);
            for (var i = 0; i < colAmount; i++)
                UserInterfaceTools.AddGridSplitter(this, MainGrid, currentRow, i, DirectionsEnum.Horizontal);

            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        #endregion // Add events

        #region Merge events

        /// <summary>
        /// Merge the cell from left with the current one.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="buttonEventArgs">The args.</param>
        public void MergeLeftButton_OnClick(object sender, RoutedEventArgs buttonEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent);
            var currentRow = GGrid.GetRow(parent);
            var currentColSpan = GGrid.GetColumnSpan(parent);
            var currentRowSpan = GGrid.GetRowSpan(parent);

            var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == (currentCol - 2) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol - 2, DirectionsEnum.Left);
            for (var i = 0; i < currentRowSpan; i++)
            {
                mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow + i) && GGrid.GetColumn(u) == (currentCol - 1) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                UserInterfaceTools.ResetMedia(this, mediaCanvas);
                UserInterfaceTools.DeleteContent(MainGrid, currentRow + i, currentCol - 1, DirectionsEnum.Left);
            }
            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Left);

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol - dicSpan["colSpan"] - 1, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Merge the cell from right with the current one.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="buttonEventArgs">The args.</param>
        public void MergeRightButton_OnClick(object sender, RoutedEventArgs buttonEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent);
            var currentRow = GGrid.GetRow(parent);
            var currentColSpan = GGrid.GetColumnSpan(parent);
            var currentRowSpan = GGrid.GetRowSpan(parent);

            var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Right);
            for (var i = 0; i < currentRowSpan; i++)
            {
                mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow + i) && GGrid.GetColumn(u) == (currentCol + currentColSpan) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                UserInterfaceTools.ResetMedia(this, mediaCanvas);
                UserInterfaceTools.DeleteContent(MainGrid, currentRow + i, currentCol + currentColSpan, DirectionsEnum.Right);
            }
            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == (currentCol + currentColSpan + 1) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol + currentColSpan + 1, DirectionsEnum.Right);

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol, currentColSpan + dicSpan["colSpan"] + 1, dicSpan["rowSpan"]);
            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Merge the cell from top with the current one.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="buttonEventArgs">The args.</param>
        public void MergeTopButton_OnClick(object sender, RoutedEventArgs buttonEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent);
            var currentRow = GGrid.GetRow(parent);
            var currentRowSpan = GGrid.GetRowSpan(parent);
            var currentColSpan = GGrid.GetColumnSpan(parent);

            var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow - 2) && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow - 2, currentCol, DirectionsEnum.Up);
            for (var i = 0; i < currentColSpan; i++)
            {
                mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow - 1) && GGrid.GetColumn(u) == (currentCol + i) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                UserInterfaceTools.ResetMedia(this, mediaCanvas);
                UserInterfaceTools.DeleteContent(MainGrid, currentRow - 1, currentCol + i, DirectionsEnum.Up);
            }
            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Up);

            UserInterfaceTools.AddGridButtons(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow - dicSpan["rowSpan"] - 1, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
        }

        /// <summary>
        /// Merge the cell from under with the current one.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        /// <param name="buttonEventArgs">The args.</param>
        public void MergeDownButton_OnClick(object sender, RoutedEventArgs buttonEventArgs)
        {
            var parent = UserInterfaceTools.FindParent((Button)sender);
            var currentCol = GGrid.GetColumn(parent);
            var currentRow = GGrid.GetRow(parent);
            var currentRowSpan = GGrid.GetRowSpan(parent);
            var currentColSpan = GGrid.GetColumnSpan(parent);

            var mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == currentRow && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            UserInterfaceTools.DeleteContent(MainGrid, currentRow, currentCol, DirectionsEnum.Down);
            for (var i = 0; i < currentColSpan; i++)
            {
                mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow + currentRowSpan) && GGrid.GetColumn(u) == (currentCol + i) && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
                UserInterfaceTools.ResetMedia(this, mediaCanvas);
                UserInterfaceTools.DeleteContent(MainGrid, currentRow + currentRowSpan, currentCol + i, DirectionsEnum.Down);
            }
            mediaCanvas = MainGrid.Children.Cast<UIElement>().FirstOrDefault(u => GGrid.GetRow(u) == (currentRow + currentRowSpan + 1) && GGrid.GetColumn(u) == currentCol && u is Canvas _canvas && _canvas.Name.Equals("MediaCanvas")) as Canvas;
            UserInterfaceTools.ResetMedia(this, mediaCanvas);
            var dicSpan = UserInterfaceTools.DeleteContent(MainGrid, currentRow + currentRowSpan + 1, currentCol, DirectionsEnum.Down);

            UserInterfaceTools.AddGridButtons(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.AddMediaCanvas(this, currentRow, currentCol, rowSpan: currentRowSpan + dicSpan["rowSpan"] + 1, colSpan: dicSpan["colSpan"]);
            UserInterfaceTools.UpdateControlButtons(MainGrid);
            UserInterfaceTools.ScaleControlButtons(MainGrid);
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
                    if (controller == null) return;

                    if (controller.IsPaused)
                        controller.Play();
                    else
                        controller.Pause();
                    break;
                case MediaElement video:
                    if (video.Source == null) return;
                    if (args.ChangedButton != MouseButton.Left || args.ClickCount != 2) return;

                    if ((bool)video.Tag)
                        video.Pause();
                    else
                        video.Play();

                    video.Tag = !(bool)video.Tag;
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
            var fElement = (FrameworkElement)e.Source;
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

            OriginMouseSpeed = (uint)SystemInformation.MouseSpeed;
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
            var fElement = (FrameworkElement)e.Source;

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
            if (grid.Name != "MediaButtons" && grid.Name != "VideoButtons" && grid.Name != "VideoTimeSlider") return;
            if (!(UserInterfaceTools.FindParent(grid) is Canvas parent)) return;

            var image = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
            var video = parent.Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement);

            if ((grid.Name == "VideoButtons" || grid.Name == "VideoTimeSlider") && (video as MediaElement)?.Source == null) return;
            if ((image as Image)?.Source == null && (video as MediaElement)?.Source == null) return;

            foreach (var child in grid.Children)
                if (child is FrameworkElement fElement)
                {
                    fElement.Visibility = Visibility.Visible;

                    if (video != null && video is MediaElement videoElement && videoElement.NaturalDuration.HasTimeSpan
                        && fElement is Slider timeSlider && timeSlider.Name == "TimeSlider"
                        && fElement.Parent is GGrid ggrid && ggrid.Name == "VideoTimeSlider")
                    {
                        BlockTimeValueChangeEvent = true;
                        var videoPosition = Math.Round(videoElement.Position.TotalMilliseconds);
                        var videoDuration = Math.Round(videoElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                        timeSlider.Value = videoPosition / videoDuration;
                        BlockTimeValueChangeEvent = false;
                    }
                }
        }

        /// <summary>
        /// Hide the media control buttons on leave.
        /// </summary>
        /// <param name="sender">The grid where the buttons are.</param>
        /// <param name="args">Who cares?</param>
        public void MediaButtons_OnMouseLeave(object sender, MouseEventArgs args)
        {
            if (!(sender is GGrid grid)) return;
            if (grid.Name != "MediaButtons" && grid.Name != "VideoButtons" && grid.Name != "VideoTimeSlider") return;

            foreach (var child in grid.Children)
                if (child is FrameworkElement fElement)
                    fElement.Visibility = Visibility.Hidden;
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

            if (fElement == null) return;
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

                case "ToggleMuteButton":
                    if (video == null) return;

                    var parentGrid = UserInterfaceTools.FindParent(child);
                    if (!(parentGrid is GGrid ggrid)) return;

                    var sliderControl = ggrid.Children.Cast<FrameworkElement>().FirstOrDefault(e => e.Name == "VolumeSlider");
                    if (!(sliderControl is Slider slider)) return;

                    if (video.Volume == 0)
                    {
                        child.Tag = Application.Current.Resources["VolumeImage"] as BitmapImage;
                        if ((double)slider.Tag != default(double))
                        {
                            video.Volume = (double)slider.Tag;
                            slider.Value = (double)slider.Tag;
                        }
                        else
                        {
                            video.Volume = 0.5;
                            slider.Value = 0.5;
                        }
                    }
                    else
                    {
                        child.Tag = Application.Current.Resources["MuteImage"] as BitmapImage;
                        slider.Tag = video.Volume;
                        video.Volume = 0;
                        slider.Value = 0;
                    }
                    break;

                case "ReloadButton":
                    var videoSource = video.Source;
                    var videoVolume = video.Volume;

                    video.Stop();
                    video.Source = null;

                    video.BeginInit();
                    video.Source = videoSource;
                    video.Volume = videoVolume;
                    video.EndInit();

                    if ((bool)video.Tag == false)
                        video.Tag = true;

                    video.Play();
                    break;

                case "RemoveContentButton":
                    var mediaButtons = UserInterfaceTools.FindParent(child);
                    var mediaCanvas = UserInterfaceTools.FindParent(mediaButtons);
                    UserInterfaceTools.ResetMedia(this, mediaCanvas as Canvas);
                    break;
            }
        }

        #region Video events

        /// <summary>
        /// Handles crash for now only if default sound device changes (i.e. if bluetooth headset disconnected and there is no other default device)
        /// </summary>
        /// <param name="sender">Blabla.</param>
        /// <param name="routedEventArgs">Bla.</param>
        public void Video_OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (!e.ErrorException.Message.Contains("0xC00D11E5")) return;
            if (!(sender is MediaElement video)) return;
            var videoSource = video.Source;
            video.Stop();
            video.Source = null;

            video.BeginInit();
            video.Source = videoSource;
            video.EndInit();

            video.Volume = 0;
            video.Play();
        }

        /// <summary>
        /// Set time slider value on media opened.
        /// </summary>
        /// <param name="sender">.</param>
        /// <param name="ev">.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Video_MediaOpened(object sender, RoutedEventArgs ev)
        {
            if (!(sender is MediaElement video)) return;

            var parentCanvas = UserInterfaceTools.FindParent(video);
            if (!(parentCanvas is Canvas canvas)) return;

            var timeGrid = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is GGrid && e.Name == "VideoTimeSlider") as GGrid;
            if (timeGrid?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Slider) is Slider timeSlider)
            {
                if (video.NaturalDuration.HasTimeSpan)
                    timeSlider.Maximum = video.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
        }

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

        /// <summary>
        /// Update the volume of the concerned video.
        /// </summary>
        /// <param name="sender">.</param>
        /// <param name="args">.</param>
        public void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            var parentGrid = UserInterfaceTools.FindParent(sender as Slider);
            var parentCanvas = UserInterfaceTools.FindParent(parentGrid);

            if (!(parentCanvas is Canvas canvas)) return;
            if (!(canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e.Name == "Video") is MediaElement video)) return;
            var volumeGrid = canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is GGrid && e.Name == "VideoButtons") as GGrid;
            if (volumeGrid?.Children.Cast<FrameworkElement>().FirstOrDefault(e => e is Button && e.Name == "ToggleMuteButton") is Button volumeButton)
            {
                if (args.NewValue > 0 && Equals(volumeButton.Tag, Application.Current.Resources["MuteImage"] as BitmapImage))
                    volumeButton.Tag = Application.Current.Resources["VolumeImage"] as BitmapImage;
                else if (args.NewValue == 0)
                    volumeButton.Tag = Application.Current.Resources["MuteImage"] as BitmapImage;
            }

            video.Volume = args.NewValue > 1.5 ? 1.5 : args.NewValue < 0 ? 0 : args.NewValue;
        }

        /// <summary>
        /// Update the time of the concerned video.
        /// </summary>
        /// <param name="sender">.</param>
        /// <param name="args">.</param>
        public void TimeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (BlockTimeValueChangeEvent) return;

            var slider = sender as Slider;
            var parentGrid = UserInterfaceTools.FindParent(slider);
            var parentCanvas = UserInterfaceTools.FindParent(parentGrid);

            if (!(parentCanvas is Canvas canvas)) return;
            if (!(canvas.Children.Cast<FrameworkElement>().FirstOrDefault(e => e.Name == "Video") is MediaElement video)) return;

            var newPosition = (int)Math.Round(slider.Maximum * slider.Value);
            video.Position = new TimeSpan(0, 0, 0, 0, newPosition);
        }

        #endregion // Video events

        #endregion // Media events

        #endregion // Events
    }
}