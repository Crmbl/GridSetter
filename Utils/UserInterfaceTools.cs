using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CustomShapeWpfButton;
using CustomShapeWpfButton.Enums;
using CustomShapeWpfButton.Utils;
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
        /// Add the gridSplitter between two grids.
        /// </summary>
        /// <param name="window">The main window.</param>
        /// <param name="grid">Defines the grid to attach to button into.</param>
        /// <param name="rowId">The row where the gridSplitter will be created.</param>
        /// <param name="colId">The column where the gridSplitter will be created.</param>
        /// <param name="direction">The direction for the gridSplitter.</param>
        public static void AddGridSplitter(Views.Grid window, Grid grid, int rowId, int colId, DirectionsEnum direction)
        {
            GridSplitter gridSplitter;
            if (direction == DirectionsEnum.Vertical)
            {
                gridSplitter = new GridSplitter
                {
                    Width = 5,
                    Cursor = Cursors.SizeWE,
                    Background = new SolidColorBrush(Colors.DarkGray),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ForceCursor = true,
                    DragIncrement = 0.1
                };
            }
            else
            {
                gridSplitter = new GridSplitter
                {
                    Height = 5,
                    Cursor = Cursors.SizeNS,
                    Background = new SolidColorBrush(Colors.DarkGray),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ForceCursor = true,
                    DragIncrement = 0.1
                };
            }

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
        public static void AddMediaCanvas(Views.Grid window, int rowId = 0, int colId = 0, int colSpan = 1, int rowSpan = 1)
        {
            Canvas canvas = new Canvas
            {
                Name = "MediaCanvas",
                Visibility = Visibility.Collapsed,
                AllowDrop = true,
                ClipToBounds = true,
                Background = new SolidColorBrush(Colors.Transparent)
            };
            canvas.Drop += window.MediaCanvas_OnDrop;
            canvas.MouseWheel += window.MediaCanvas_OnMouseWheel;
            canvas.SizeChanged += window.MediaCanvas_SizeChanged;

            #region Image

            Image image = new Image
            {
                Name = "Image",
                ClipToBounds = true,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

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

            image.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new TranslateTransform(),
                    new ScaleTransform()
                }
            };
            image.MouseLeftButtonDown += window.Media_OnMouseLeftButtonDown;
            image.MouseLeftButtonUp += window.Media_OnMouseLeftButtonUp;
            image.MouseMove += window.Media_OnMouseMove;
            image.MouseDown += window.Media_MouseDown;

            #endregion // Image

            #region Video

            MediaElement video = new MediaElement
            {
                Name = "Video",
                ClipToBounds = true,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            video.SetBinding(Canvas.TopProperty, new MultiBinding
            {
                Converter = new CenterConverter(),
                ConverterParameter = "top",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = video },
                    new Binding("ActualHeight") { Source = video }
                }
            });
            video.SetBinding(Canvas.LeftProperty, new MultiBinding
            {
                Converter = new CenterConverter(),
                ConverterParameter = "left",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = video },
                    new Binding("ActualHeight") { Source = video }
                }
            });

            video.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new TranslateTransform(),
                    new ScaleTransform()
                }
            };
            video.MouseLeftButtonDown += window.Media_OnMouseLeftButtonDown;
            video.MouseLeftButtonUp += window.Media_OnMouseLeftButtonUp;
            video.MouseMove += window.Media_OnMouseMove;
            video.MouseDown += window.Media_MouseDown;
            video.MediaEnded += window.Video_OnMediaEnded;
            video.MediaFailed += window.Video_OnMediaFailed;
            video.MediaOpened += window.Video_MediaOpened;

            video.Tag = true;

            #endregion // Video

            canvas.Children.Add(image);
            canvas.Children.Add(video);
            Grid.SetColumn(canvas, colId);
            Grid.SetRow(canvas, rowId);
            Grid.SetColumnSpan(canvas, colSpan);
            Grid.SetRowSpan(canvas, rowSpan);
            Panel.SetZIndex(canvas, 10);
            window.MainGrid.Children.Add(canvas);

            AddMediaButtons(window, canvas);
            AddVideoVolumeSlider(window, canvas);
            AddVideoTimeSlider(window, canvas);
        }

        /// <summary>
        /// Add the buttons to control the image.
        /// </summary>
        /// <param name="window">Used to bind to the methods.</param>
        /// <param name="canvas">The canvas to add the buttons into.</param>
        private static void AddMediaButtons(Views.Grid window, Canvas canvas)
        {
            Grid controlGrid = new Grid
            {
                Name = "MediaButtons",
                Background = new SolidColorBrush(Colors.Transparent),
                ClipToBounds = true,
                MaxWidth = 220,
                VerticalAlignment = VerticalAlignment.Top
            };
            controlGrid.MouseEnter += window.MediaButtons_OnMouseEnter;
            controlGrid.MouseLeave += window.MediaButtons_OnMouseLeave;

            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            Button takeWidthButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "TakeWidthButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeWidthImage"] as BitmapImage
            };
            takeWidthButton.Click += window.MediaControl_OnClick;

            Button takeHeightButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "TakeHeightButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["EnlargeHeightImage"] as BitmapImage
            };
            takeHeightButton.Click += window.MediaControl_OnClick;

            Button resizeButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "ResizeButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["ResizeImage"] as BitmapImage
            };
            resizeButton.Click += window.MediaControl_OnClick;

            Button removeContentButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageBase"] as Style,
                Name = "RemoveContentButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["RecycleImage"] as BitmapImage
            };
            removeContentButton.Click += window.MediaControl_OnClick;

            Grid.SetColumn(takeHeightButton, 0);
            Grid.SetColumn(takeWidthButton, 1);
            Grid.SetColumn(resizeButton, 2);
            Grid.SetColumn(removeContentButton, 3);
            controlGrid.Children.Add(takeHeightButton);
            controlGrid.Children.Add(takeWidthButton);
            controlGrid.Children.Add(resizeButton);
            controlGrid.Children.Add(removeContentButton);
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

            canvas.Children.Add(controlGrid);
        }

        /// <summary>
        /// Add the buttons to control the volume of the video.
        /// </summary>
        /// <param name="window">Used to bind to the methods.</param>
        /// <param name="canvas">The canvas to add the buttons into.</param>
        private static void AddVideoVolumeSlider(Views.Grid window, Canvas canvas)
        {
            Grid controlGrid = new Grid
            {
                Name = "VideoButtons",
                Background = new SolidColorBrush(Colors.Transparent),
                MaxHeight = 325, //195
                Width = 55, //45
                VerticalAlignment = VerticalAlignment.Center
            };
            controlGrid.MouseEnter += window.MediaButtons_OnMouseEnter;
            controlGrid.MouseLeave += window.MediaButtons_OnMouseLeave;

            controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            controlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Slider volumeSlider = new Slider
            {
                Style = Application.Current.Resources["SliderStyle"] as Style,
                Name = "VolumeSlider",
                Visibility = Visibility.Hidden
            };
            volumeSlider.ValueChanged += window.VolumeSlider_OnValueChanged;

            Button muteButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageSide"] as Style,
                Name = "ToggleMuteButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["MuteImage"] as BitmapImage
            };
            muteButton.Click += window.MediaControl_OnClick;

            Button reloadButton = new Button
            {
                Style = Application.Current.Resources["ButtonImageSide"] as Style,
                Name = "ReloadButton",
                Visibility = Visibility.Hidden,
                Tag = Application.Current.Resources["ReloadImage"] as BitmapImage
            };
            reloadButton.Click += window.MediaControl_OnClick;

            Grid.SetRow(volumeSlider, 0);
            Grid.SetRow(muteButton, 1);
            Grid.SetRow(reloadButton, 2);
            controlGrid.Children.Add(volumeSlider);
            controlGrid.Children.Add(muteButton);
            controlGrid.Children.Add(reloadButton);
            controlGrid.SetBinding(Canvas.LeftProperty, new MultiBinding
            {
                Converter = new RightConverter(),
                ConverterParameter = "left",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = controlGrid },
                    new Binding("ActualHeight") { Source = controlGrid }
                }
            });
            controlGrid.SetBinding(Canvas.TopProperty, new MultiBinding
            {
                Converter = new CenterConverter(),
                ConverterParameter = "top",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = controlGrid },
                    new Binding("ActualHeight") { Source = controlGrid }
                }
            });

            canvas.Children.Add(controlGrid);
        }

        /// <summary>
        /// Add the button to control the time of the video.
        /// </summary>
        /// <param name="window">Used to bind to the methods.</param>
        /// <param name="canvas">The canvas to add the buttons into.</param>
        private static void AddVideoTimeSlider(Views.Grid window, Canvas canvas)
        {
            Grid controlGrid = new Grid
            {
                Name = "VideoTimeSlider",
                Background = new SolidColorBrush(Colors.Transparent),
                MaxHeight = 55,
                Width = 450,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            controlGrid.MouseEnter += window.MediaButtons_OnMouseEnter;
            controlGrid.MouseLeave += window.MediaButtons_OnMouseLeave;

            controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            Slider timeSlider = new Slider
            {
                Style = Application.Current.Resources["SliderBottomStyle"] as Style,
                Name = "TimeSlider",
                Visibility = Visibility.Hidden,
                Minimum = 0
            };
            timeSlider.ValueChanged += window.TimeSlider_OnValueChanged;

            Grid.SetRow(timeSlider, 0);
            controlGrid.Children.Add(timeSlider);
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
            controlGrid.SetBinding(Canvas.TopProperty, new MultiBinding
            {
                Converter = new BottomConverter(),
                ConverterParameter = "top",
                Mode = BindingMode.TwoWay,
                Bindings = {
                    new Binding("ActualWidth") { Source = canvas },
                    new Binding("ActualHeight") { Source = canvas },
                    new Binding("ActualWidth") { Source = controlGrid },
                    new Binding("ActualHeight") { Source = controlGrid }
                }
            });

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
        public static void AddGridButtons(Views.Grid window, int rowId = 0, int colId = 0, int colSpan = 1, int rowSpan = 1)
        {
            var controlGrid = new Grid
            {
                Name = "SetupGrid",
                Background = new SolidColorBrush(Colors.Transparent)
            };
            controlGrid.MouseEnter += window.ControlGrid_MouseEnter;
            controlGrid.MouseLeave += window.ControlGrid_MouseLeave;

            var values = new Dictionary<Position, string>
            {
                { Position.Right, "Right" },
                { Position.Left, "Left" },
                { Position.Top, "Top" },
                { Position.Bottom, "Bottom" },
                { Position.Center, "Split" }
            };
            ArcButton arcButton = new ArcButton(230, values, backgroundPressed: "#a29bfe");
            arcButton.Visibility = Visibility.Hidden;
            arcButton.LeftClick += window.AddLeftColButton_OnClick;
            arcButton.RightClick += window.AddRightColButton_OnClick;
            arcButton.TopClick += window.AddUpRowButton_OnClick;
            arcButton.BottomClick += window.AddDownRowButton_OnClick;
            arcButton.CenterClick += window.SplitButton_OnClick;
            if (arcButton.Content is Grid arcButtonGrid)
                arcButtonGrid.LayoutTransform = new ScaleTransform();

            var mergeTopButton = new Button { Style = Application.Current.Resources["MergeTopButtonStyle"] as Style, Name = "mergeTopButton", IsEnabled = false };
            var mergeDownButton = new Button { Style = Application.Current.Resources["MergeBottomButtonStyle"] as Style, Name = "mergeDownButton", IsEnabled = false };
            var mergeLeftButton = new Button { Style = Application.Current.Resources["MergeLeftButtonStyle"] as Style, Name = "mergeLeftButton", IsEnabled = false };
            var mergeRightButton = new Button { Style = Application.Current.Resources["MergeRightButtonStyle"] as Style, Name = "mergeRightButton", IsEnabled = false };
            mergeTopButton.Click += window.MergeTopButton_OnClick;
            mergeDownButton.Click += window.MergeDownButton_OnClick;
            mergeLeftButton.Click += window.MergeLeftButton_OnClick;
            mergeRightButton.Click += window.MergeRightButton_OnClick;

            controlGrid.Children.Add(arcButton);
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

            Grid.SetColumn(arcButton, 2);
            Grid.SetRow(arcButton, 2);
            Panel.SetZIndex(arcButton, 110);

            Grid.SetColumn(mergeTopButton, 0);
            Grid.SetColumnSpan(mergeTopButton, 5);
            Grid.SetRow(mergeTopButton, 0);
            Grid.SetColumn(mergeDownButton, 0);
            Grid.SetColumnSpan(mergeDownButton, 5);
            Grid.SetRow(mergeDownButton, 4);
            Grid.SetColumn(mergeLeftButton, 0);
            Grid.SetRow(mergeLeftButton, 0);
            Grid.SetRowSpan(mergeLeftButton, 5);
            Grid.SetColumn(mergeRightButton, 4);
            Grid.SetRow(mergeRightButton, 0);
            Grid.SetRowSpan(mergeRightButton, 5);

            Grid.SetColumn(controlGrid, colId);
            Grid.SetRow(controlGrid, rowId);
            Grid.SetColumnSpan(controlGrid, colSpan);
            Grid.SetRowSpan(controlGrid, rowSpan);
            Panel.SetZIndex(controlGrid, 100);

            window.MainGrid.Children.Add(controlGrid);
        }

        #endregion // Controls

        #region Tools

        /// <summary>
        /// Helps to resolve the parent for a given child.
        /// </summary>
        /// <param name="child">The parent to return.</param>
        public static FrameworkElement FindParent(DependencyObject child)
        {
            var result = VisualTreeHelper.GetParent(child);
            return result as FrameworkElement;
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

                return new Dictionary<string, int> { { "rowSpan", rowSpan }, { "colSpan", colSpan } };
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

                    if (potentielNeighbor is Grid && Grid.GetRowSpan(potentielNeighbor) == currentRowSpan)
                        return true;
                    return false;

                case DirectionsEnum.Right:
                    if (currentCol == colMax)
                        return false;

                    potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
                        e => Grid.GetRow(e) == currentRow && Grid.GetColumn(e) == currentCol + currentColSpan + 1);

                    if (potentielNeighbor is Grid && Grid.GetRowSpan(potentielNeighbor) == currentRowSpan)
                        return true;
                    return false;

                case DirectionsEnum.Up:
                    if (currentRow == 0)
                        return false;

                    potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
                        e => Grid.GetRow(e) == currentRow - Grid.GetRowSpan(e) - 1 && Grid.GetColumn(e) == currentCol);
                    if (potentielNeighbor is Grid && Grid.GetColumnSpan(potentielNeighbor) == currentColSpan)
                        return true;
                    return false;

                case DirectionsEnum.Down:
                    if (currentRow == rowMax)
                        return false;

                    potentielNeighbor = mainGrid.Children.Cast<UIElement>().FirstOrDefault(
                        e => Grid.GetRow(e) == currentRow + currentRowSpan + 1 && Grid.GetColumn(e) == currentCol);
                    if (potentielNeighbor is Grid && Grid.GetColumnSpan(potentielNeighbor) == currentColSpan)
                        return true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if there are merged cells, prevent from destroying cells management.
        /// </summary>
        private static bool MergedCellsChecker(Grid mainGrid, Grid grid, DirectionsEnum direction)
        {
            var currentRow = Grid.GetRow(grid);
            var currentCol = Grid.GetColumn(grid);
            //Has to minus 1 because Grid.GetRow/GetColumn starts at 0.
            var currentRowSpan = Grid.GetRowSpan(grid) - 1;
            var currentColSpan = Grid.GetColumnSpan(grid) - 1;

            switch (direction)
            {
                case DirectionsEnum.Left:
                    return !mainGrid.Children.Cast<UIElement>().Any(e => Grid.GetColumnSpan(e) > 1 && Grid.GetColumn(e) < currentCol && Grid.GetColumn(e) + Grid.GetColumnSpan(e) - 1 >= currentCol);

                case DirectionsEnum.Right:
                    return !mainGrid.Children.Cast<UIElement>().Any(e => Grid.GetColumnSpan(e) > 1 && Grid.GetColumn(e) <= currentCol + currentColSpan
                            && Grid.GetColumn(e) + Grid.GetColumnSpan(e) - 1 > currentCol + currentColSpan);

                case DirectionsEnum.Up:
                    return !mainGrid.Children.Cast<UIElement>().Any(e => Grid.GetRowSpan(e) > 1 && Grid.GetRow(e) < currentRow && Grid.GetRow(e) + Grid.GetRowSpan(e) - 1 >= currentRow);

                case DirectionsEnum.Down:
                    return !mainGrid.Children.Cast<UIElement>().Any(e => Grid.GetRowSpan(e) > 1 && Grid.GetRow(e) <= currentRow + currentRowSpan
                                && Grid.GetRow(e) + Grid.GetRowSpan(e) - 1 > currentRow + currentRowSpan);

                case DirectionsEnum.None:
                    return currentRowSpan > 0 || currentColSpan > 0;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Lock or unlock the buttons on click.
        /// </summary>
        /// <param name="mainGrid">Defines the main grid to lock the buttons.</param>
        /// <param name="isLocked">Defines the mode.</param>
        public static void ToggleLockControlButtons(Grid mainGrid, bool isLocked)
        {
            var elementList = mainGrid.Children.Cast<FrameworkElement>().Where(e => e.Name == "SetupGrid" || e.Name == "MediaCanvas").ToList();
            foreach (var uiElement in elementList)
            {
                if (uiElement is Canvas canvas && canvas.Name == "MediaCanvas")
                {
                    var image = (Image)canvas.Children.Cast<UIElement>().FirstOrDefault(e => e is Image);
                    var video = (MediaElement)canvas.Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement);

                    if (image?.Source != null || video?.Source != null)
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
                var buttonsList = grid.Children.Cast<UIElement>().Where(x => x is Button || x is ArcButton).ToList();
                var mergeButtonsList = grid.Children.Cast<UIElement>().Where(x => x is Button).ToList();
                var mergeTopButton = mergeButtonsList.Cast<Button>().FirstOrDefault(b => b.Name.Equals("mergeTopButton"));
                var mergeDownButton = mergeButtonsList.Cast<Button>().FirstOrDefault(b => b.Name.Equals("mergeDownButton"));
                var mergeLeftButton = mergeButtonsList.Cast<Button>().FirstOrDefault(b => b.Name.Equals("mergeLeftButton"));
                var mergeRightButton = mergeButtonsList.Cast<Button>().FirstOrDefault(b => b.Name.Equals("mergeRightButton"));
                var splitButton = buttonsList.Cast<ArcButton>().FirstOrDefault();

                if (mergeTopButton != null)
                    mergeTopButton.IsEnabled = NeighborChecker(mainGrid, grid, DirectionsEnum.Up);
                if (mergeDownButton != null)
                    mergeDownButton.IsEnabled = NeighborChecker(mainGrid, grid, DirectionsEnum.Down);
                if (mergeLeftButton != null)
                    mergeLeftButton.IsEnabled = NeighborChecker(mainGrid, grid, DirectionsEnum.Left);
                if (mergeRightButton != null)
                    mergeRightButton.IsEnabled = NeighborChecker(mainGrid, grid, DirectionsEnum.Right);

                if (splitButton == null) continue;
                splitButton.UpdateButtonProperty(Position.Top, "Visibility", MergedCellsChecker(mainGrid, grid, DirectionsEnum.Up));
                splitButton.UpdateButtonProperty(Position.Bottom, "Visibility", MergedCellsChecker(mainGrid, grid, DirectionsEnum.Down));
                splitButton.UpdateButtonProperty(Position.Left, "Visibility", MergedCellsChecker(mainGrid, grid, DirectionsEnum.Left));
                splitButton.UpdateButtonProperty(Position.Right, "Visibility", MergedCellsChecker(mainGrid, grid, DirectionsEnum.Right));
                splitButton.UpdateButtonProperty(Position.Center, "Visibility", MergedCellsChecker(mainGrid, grid, DirectionsEnum.None));
            }
        }

        /// <summary>
        /// Scale the control buttons according to the space available.
        /// </summary>
        public static void ScaleControlButtons(Grid mainGrid)
        {
            foreach (var grid in mainGrid.Children.Cast<FrameworkElement>().Where(x => x.Name == "SetupGrid"))
            {
                var arcButton = ((Grid)grid).Children.Cast<ArcButton>().FirstOrDefault();
                if (!(arcButton?.Content is Grid insideGrid) || (arcButton is ArcButton && arcButton.Visibility == Visibility.Visible)) return;

                var scaleTransform = insideGrid.LayoutTransform as ScaleTransform;
                var bounds = LayoutInformation.GetLayoutClip(grid)?.Bounds;
                if (bounds != null || scaleTransform.ScaleX < 1)
                {
                    var element = bounds != null ? new Size(bounds.Value.Width, bounds.Value.Height) : new Size(grid.ActualWidth, grid.ActualHeight);
                    //30 == merge button height/width * 2 (defined in App.xaml)
                    var coeffH = MathUtil.Round((element.Height - 30) / insideGrid.Height);
                    var coeffW = MathUtil.Round((element.Width - 30) / insideGrid.Width);
                    scaleTransform.ScaleX = coeffH > coeffW ? coeffW : coeffH;
                    scaleTransform.ScaleY = scaleTransform.ScaleX;

                    scaleTransform.CenterX = MathUtil.Round(arcButton.ActualWidth / 2);
                    scaleTransform.CenterY = MathUtil.Round(arcButton.ActualHeight / 2);
                }
                else
                {
                    scaleTransform.ScaleX = 1;
                    scaleTransform.ScaleY = 1;
                    scaleTransform.CenterX = 0;
                    scaleTransform.CenterY = 0;
                }

                arcButton.UpdateButtonsProperty("TextVisibility", scaleTransform.ScaleX >= 0.8);
                insideGrid.LayoutTransform = scaleTransform;
            }
        }

        /// <summary>
        /// Remove the content of a grid, picture/movie...
        /// </summary>
        /// <param name="grid">The grid where we want to delete the content.</param>
        public static void RemoveContentFromGrid(Grid grid)
        {
            foreach (var item in grid.Children.Cast<UIElement>().Where(e => e is Canvas && e.Visibility == Visibility.Visible).ToList())
            {
                var canvas = item as Canvas;
                var image = canvas?.Children.Cast<UIElement>().FirstOrDefault(e => e is Image && e.Visibility == Visibility.Visible) as Image;
                var video = canvas?.Children.Cast<UIElement>().FirstOrDefault(e => e is MediaElement && e.Visibility == Visibility.Visible) as MediaElement;

                if (image != null)
                {
                    var renderScaleTransform = (ScaleTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is ScaleTransform);
                    var translateTransform = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
                    renderScaleTransform.ScaleX = 1;
                    renderScaleTransform.ScaleY = 1;
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                    image.Source = null;
                    image.Visibility = Visibility.Hidden;
                }

                if (video != null)
                {
                    var renderScaleTransform = (ScaleTransform)((TransformGroup)video.RenderTransform).Children.First(tr => tr is ScaleTransform);
                    var translateTransform = (TranslateTransform)((TransformGroup)video.RenderTransform).Children.First(tr => tr is TranslateTransform);
                    renderScaleTransform.ScaleX = 1;
                    renderScaleTransform.ScaleY = 1;
                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                    video.Source = null;
                    video.Visibility = Visibility.Hidden;
                }
            }
        }

        #endregion // Tools

        #endregion // Public methods
    }
}
