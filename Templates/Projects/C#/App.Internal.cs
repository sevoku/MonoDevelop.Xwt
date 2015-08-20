using System;
using System.Runtime.InteropServices;
using Xwt;

namespace ${Namespace}
{
	public static partial class App
	{
		static ToolkitType toolkitWindows = ToolkitType.Wpf;
		static ToolkitType toolkitLinux = ToolkitType.Gtk;
		static ToolkitType toolkitMacOS = ToolkitType.XamMac;

		public static ToolkitType ToolkitWindows {
			get { return toolkitWindows; }
			set { toolkitWindows = value; }
		}

		public static ToolkitType ToolkitLinux {
			get { return toolkitLinux; }
			set { toolkitLinux = value; }
		}

		public static ToolkitType ToolkitMacOS {
			get { return toolkitMacOS; }
			set { toolkitMacOS = value; }
		}
		
		public static void Run (string[] args)
		{
			InitializeXwt ();

			OnRun  (args);

			Application.Run ();

			OnExit ();
			Application.Dispose ();
		}

		static void InitializeXwt ()
		{
			switch (Environment.OSVersion.Platform) {
				case PlatformID.MacOSX:
					Application.Initialize (ToolkitMacOS);
					break;
				case PlatformID.Unix:
					if (IsRunningOnMac ())
						Application.Initialize (ToolkitMacOS);
					else
						Application.Initialize (ToolkitLinux);
					break;
				default:
					Application.Initialize (ToolkitWindows);
					break;
			}
		}

		// from Xwt.GtkBackend.Platform
		static bool IsRunningOnMac ()
		{
			IntPtr buf = IntPtr.Zero;
			try {
				buf = Marshal.AllocHGlobal (8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname (buf) == 0) {
					string os = Marshal.PtrToStringAnsi (buf);
					if (os.ToUpper () == "DARWIN")
						return true;
				}
				// Analysis disable once EmptyGeneralCatchClause
			} catch {
			} finally {
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal (buf);
			}
			return false;
		}

		[DllImport ("libc")]
		static extern int uname (IntPtr buf);
	}
}

