using Xwt;

namespace ${Namespace}
{
	public static partial class App
	{
		static MainWindow MainWindow;

		static App ()
		{
			ToolkitWindows = ToolkitType.Wpf;
			ToolkitLinux = ToolkitType.Gtk;
			ToolkitMacOS = ToolkitType.XamMac;
		}

		static void OnRun (string[] args)
		{
			MainWindow = new MainWindow ();
			MainWindow.Show ();
		}

		static void OnExit ()
		{
			if (MainWindow != null)
				MainWindow.Dispose ();
		}
	}
}

