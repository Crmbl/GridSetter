using System.Threading;
using System.Windows;
using GridSetter.Utils;
using GridSetter.Utils.Interfaces;

namespace GridSetter
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			RegisterInstance();
		}

		public static void RegisterInstance()
		{
			DependencyInjectionUtil.RegisterInstance<IDispatcher>(new Dispatcher(Thread.CurrentThread));
		}
	}
}
