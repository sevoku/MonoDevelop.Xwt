using Xwt;

namespace ${Namespace}
{
	public static partial class App
	{
		public static ToolkitType ToolkitWindows {
			get { return ToolkitType.Wpf; }
		}

		public static ToolkitType ToolkitLinux {
			get { return ToolkitType.Gtk; }
		}

		public static ToolkitType ToolkitMacOS {
			get { return ToolkitType.XamMac; }
		}

		static MainWindow MainWindow;

		public static void OnRun (string[] args)
		{
			MainWindow = new MainWindow ();
			MainWindow.Show ();
		}

		public static void OnExit ()
		{
			if (MainWindow != null)
				MainWindow.Dispose ();
		}
	}
}

