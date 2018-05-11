using System;
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
			ViewModel = new GridSetterViewModel();
			ViewModel.NewCreatedEvent += (sender, args) => SetNotifyIconMenuItems();
		    ViewModel.ToggleStateChangedEvent += (sender, args) => SetNotifyIconMenuItems();

			DataContext = ViewModel;

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
				((MainWindow) sender).Cursor = Cursors.None;
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
			if (NotifyIcon.ContextMenu == null)
				NotifyIcon.ContextMenu = new ContextMenu();

			NotifyIcon.ContextMenu.MenuItems.Clear();
			NotifyIcon.ContextMenu.MenuItems.Add("Maximize", (s, e) => { Show(); WindowState = WindowState.Normal; NotifyIcon.Visible = false; });
			NotifyIcon.ContextMenu.MenuItems.Add(ViewModel.ResetNewLabel == "New" ? "New" : "Reset", (s, e) => { ViewModel.ResetGrid(); });
			NotifyIcon.ContextMenu.MenuItems.Add("Empty all", (s, e) => { ViewModel.EmptyAllContent(); });
			NotifyIcon.ContextMenu.MenuItems.Add(ViewModel.ToggleLockLabel == "Lock" ? "Lock" : "Unlock", (s, e) => { ViewModel.ToggleLockGrid(); SetNotifyIconMenuItems(); });
			NotifyIcon.ContextMenu.MenuItems.Add("Exit", (s, e) => Current.Shutdown());
		}

		/// <summary>
		/// Defines the number on the system tray icon and set it.
		/// </summary>
		/// <returns></returns>
		private static Icon GetNumberedIcon()
		{
			var resource = GetResourceStream(new Uri("pack://application:,,,/Resources/Images/ice-cream.ico"));
			if (resource == null) throw new ArgumentNullException($"Resource not found!");

			var bitmap = new Bitmap(32, 32);
			var count = Process.GetProcesses().Count(p => p.ProcessName == "GridSetter");
			var icon = new Icon(resource.Stream);
			var font = new Font("Tahoma", 16, System.Drawing.FontStyle.Bold);
			var brush = new SolidBrush(Color.White);
			var graphics = Graphics.FromImage(bitmap);

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
			graphics.DrawIcon(icon, 0, 0);
			graphics.DrawString(count == 1 ? string.Empty : count.ToString(), font, brush, 15, 8);

			var createdIcon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());

			resource.Stream.Dispose();
			font.Dispose();
			brush.Dispose();
			graphics.Dispose();
			bitmap.Dispose();

			return createdIcon;
		}

		#endregion // Methods
	}
}