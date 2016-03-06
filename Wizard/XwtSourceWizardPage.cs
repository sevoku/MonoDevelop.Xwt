//
// XwtTemplateWizardSourcePage.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Vsevolod Kukol
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Templates;
using Xwt;

namespace MonoDevelop.Xwt
{
	public enum XwtSource
	{
		Local,
		Github,
		NuGet,
	}

	public class XwtSourceWizardPage : WizardPage
	{
		XwtSourcePageWidget view;
		XwtSource xwtReferenceSource;
		bool xwtGitSubmodule;

		public XwtTemplateWizard Wizard { get; private set; }

		public XwtSource XwtReferenceSource {
			get {
				return xwtReferenceSource;
			}
			set {
				xwtReferenceSource = value;
				Wizard.Parameters ["XwtSourceLocal"] = (value == XwtSource.Local).ToString ();
				Wizard.Parameters ["XwtSourceGithub"] = (value == XwtSource.Github).ToString ();
				Wizard.Parameters ["XwtSourceNuGet"] = (value == XwtSource.NuGet).ToString ();
			}
		}

		public bool XwtGitSubmodule {
			get {
				return xwtGitSubmodule;
			}
			set {
				xwtGitSubmodule = value;
				Wizard.Parameters ["XwtGitSubmodule"] = value.ToString ();
			}
		}

		public override string Title {
			get {
				return GettextCatalog.GetString ("Configure Xwt Reference");
			}
		}

		public XwtSourceWizardPage (XwtTemplateWizard wizard)
		{
			CanMoveToNextPage = true;
			Wizard = wizard;

			Wizard.Parameters ["Xwt.WPF.Installed"] = IsAssemblyInstalled ("Xwt.WPF").ToString ();
			Wizard.Parameters ["Xwt.Gtk.Installed"] = IsAssemblyInstalled ("Xwt.Gtk").ToString ();
			Wizard.Parameters ["Xwt.Gtk3.Installed"] = IsAssemblyInstalled ("Xwt.Gtk3").ToString ();
			Wizard.Parameters ["Xwt.XamMac.Installed"] = IsAssemblyInstalled ("Xwt.XamMac").ToString ();
			Wizard.Parameters ["Xwt.Mac.Installed"] = IsAssemblyInstalled ("Xwt.Mac").ToString ();
		}

		bool IsAssemblyInstalled (string name)
		{
			var assemblies = Runtime.SystemAssemblyService.CurrentRuntime.RuntimeAssemblyContext.GetAssemblies ()
				.Where (asm => asm.Name == name).ToList ();
			if (assemblies.Count == 0)
				return false;
			return true;
		}

		protected override object CreateNativeWidget<T> ()
		{
			if (view == null) {
				view = new XwtSourcePageWidget (this);
			}
			var w = Toolkit.CurrentEngine.GetNativeWidget (view);
			return w;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && view != null)
				view.Dispose ();
			base.Dispose (disposing);
		}
	}
}

