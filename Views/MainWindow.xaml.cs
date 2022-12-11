﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GridSetter.ViewModels;
using Cursors = System.Windows.Input.Cursors;
using static System.Windows.Application;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.IO;
using System.Windows.Controls;

namespace GridSetter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Properties

        /// <summary>
        /// Icon in the system tray.
        /// </summary>
        public NotifyIcon NotifyIcon { get; set; }

        /// <summary>
        /// The main viewmodel reference.
        /// </summary>
        private GridSetterViewModel ViewModel { get; }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// <see cref="MainWindow"/> default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new GridSetterViewModel(this);
            ViewModel.NewCreatedEvent += (sender, args) => SetNotifyIconMenuItems();
            ViewModel.ToggleStateChangedEvent += (sender, args) => SetNotifyIconMenuItems();

            DataContext = ViewModel;
            DropDownImport.DropDownContent = ViewModel.GetDropDownContent();

            #region System Tray Icon

            NotifyIcon = new NotifyIcon
            {
                Icon = GetNumberedIcon(),
                Visible = false
            };

            NotifyIcon.DoubleClick += delegate
            {
                Show();
                WindowState = WindowState.Normal;
                NotifyIcon.Visible = false;
            };

            SetNotifyIconMenuItems();

            #endregion // System Tray Icon
        }

        #endregion // Constructors

        #region Events

        /// <summary>
        /// Event raised on reset/new click.
        /// </summary>
        private void ResetNewClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Event raised when the user click somewhere on the window and won't release click.
        /// </summary>
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                //((MainWindow)sender).Cursor = Cursors.None;
                DragMove();
            }
        }

        /// <summary>
        /// Event raised when the user release the mouse click.
        /// </summary>
        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised when the mouse enter the window.
        /// </summary>
        private void WindowMouseEnter(object sender, MouseEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised when the mouse leave the window.
        /// </summary>
        private void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            ((MainWindow)sender).Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Event raised on state changed.
        /// </summary>
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                NotifyIcon.Visible = true;
                Hide();
            }

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Event raised when the use double click on the window.
        /// </summary>
        private void WindowMouseDouble(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
                SetNotifyIconMenuItems();
            }
        }

        #endregion // Events

        #region Methods

        /// <summary>
        /// Defines the NotifyIcon.ContextMenu.MenuItems.
        /// </summary>
        private void SetNotifyIconMenuItems()
        {
            if (NotifyIcon.ContextMenuStrip == null)
                NotifyIcon.ContextMenuStrip = new ContextMenuStrip();

            var bitmapMaximize = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\maximize.png");
            var bitmapEmptyAll = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\eraser.png");
            var bitmapExit = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\exit.png");

            NotifyIcon.ContextMenuStrip.Items.Clear();
            NotifyIcon.ContextMenuStrip.Items.Add("Maximize", bitmapMaximize, (s, e) => { Show(); WindowState = WindowState.Normal; NotifyIcon.Visible = false; });
            if (ViewModel.ResetNewLabel == "New")
            {
                var bitmapNew = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\new.png");
                NotifyIcon.ContextMenuStrip.Items.Add("New", bitmapNew, (s, e) => { ViewModel.ResetGrid(); });
                bitmapNew = null;
            }
            else
            {
                var bitmapReset = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\reset.png");
                NotifyIcon.ContextMenuStrip.Items.Add("Reset", bitmapReset, (s, e) => { ViewModel.ResetGrid(); });
                bitmapReset = null;
            }
            NotifyIcon.ContextMenuStrip.Items.Add("Empty all", bitmapEmptyAll, (s, e) => { ViewModel.EmptyAllContent(); });
            if (ViewModel.ToggleLockLabel == "Lock")
            {
                var bitmapLock = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\lock.png");
                NotifyIcon.ContextMenuStrip.Items.Add("Lock", bitmapLock, (s, e) => { ViewModel.ToggleLockGrid(); SetNotifyIconMenuItems(); });
                bitmapLock = null;
            }
            else
            {
                var bitmapUnlock = Bitmap.FromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Images\\unlock.png");
                NotifyIcon.ContextMenuStrip.Items.Add("Unlock", bitmapUnlock, (s, e) => { ViewModel.ToggleLockGrid(); SetNotifyIconMenuItems(); });
                bitmapUnlock = null;
            }
            NotifyIcon.ContextMenuStrip.Items.Add("Exit", bitmapExit, (s, e) => Current.Shutdown());

            bitmapMaximize = null;
            bitmapEmptyAll = null;
            bitmapExit = null;
        }

        /// <summary>
        /// Defines the number on the system tray icon and set it.
        /// </summary>
        private static Icon GetNumberedIcon()
        {
            //var resource = GetResourceStream(new Uri("pack://application:,,,/Resources/Images/cube.ico"));
            var resource = GetResourceStream(new Uri("pack://application:,,,/Resources/Images/grid.ico"));
            if (resource == null) throw new ArgumentNullException($"Resource not found!");

            var bitmap = new Bitmap(100, 100);
            var count = Process.GetProcesses().Count(p => p.ProcessName == "GridSetter");
            var icon = new Icon(resource.Stream);
            var font = new Font("Tahoma", 47, System.Drawing.FontStyle.Bold);
            var brush = new SolidBrush(Color.Black);
            var graphics = Graphics.FromImage(bitmap);

            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            graphics.DrawIcon(icon, 0, 0);
            graphics.DrawString(count == 1 ? string.Empty : count.ToString(), font, brush, 21, 10);

            var createdIcon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());

            resource.Stream.Dispose();
            font.Dispose();
            brush.Dispose();
            graphics.Dispose();
            bitmap.Dispose();

            return createdIcon;
        }

        /// <summary>
        /// Command for the import dropdownbutton.
        /// </summary>
        public class FileSelectionCommand : ICommand
        {
            public GridSetterViewModel ViewModel { get; set; }

            public void Execute(object parameter)
            {
                ViewModel.ImportGrid(parameter.ToString());
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }

        /// <summary>
        /// Command for the delete dropdownbutton.
        /// </summary>
        public class FileDeletionCommand : ICommand
        {
            public GridSetterViewModel ViewModel { get; set; }

            public void Execute(object parameter)
            {
                ViewModel.DeleteJson(parameter.ToString());
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }

        #endregion // Methods
    }
}