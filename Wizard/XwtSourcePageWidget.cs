//
// XwtSourcePageWidget.cs
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
using MonoDevelop.Core;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.Xwt
{
	public class XwtSourcePageWidget : HBox
	{
		public XwtSourceWizardPage Page { get; private set; }

		public XwtSourcePageWidget (XwtSourceWizardPage page)
		{
			Page = page;

			var optBuiltIn = new RadioButton(GettextCatalog.GetString ("Local Package / GAC")) {
				TooltipText = GettextCatalog.GetString (
					"Xwt must be installed to the GAC (Global Assembly Cache),\n" +
					"otherwise you will have to add a HintPath property manually.")
			};
			var optGithub = new RadioButton(GettextCatalog.GetString ("GitHub repository")) {
				TooltipText = GettextCatalog.GetString (
					"A separate solution folder named 'Xwt' will be added to the solution.\n" +
					"If the solution already contains the Xwt project,\nit will be referenced instead" +
					"and the git checkout will be skipped.")
			};
			var linkGithub = new Label {
				Markup = "(<a href='https://github.com/mono/xwt'>Official Repository</a>)",
				TooltipText = "https://github.com/mono/xwt"
			};
			var optNuGet = new RadioButton(GettextCatalog.GetString ("NuGet package")) {
				TooltipText = GettextCatalog.GetString ("All registered NuGet repositories will be searched.")
			};
			                               
			var sourceGroup = optBuiltIn.Group = optGithub.Group = optNuGet.Group;
			sourceGroup.ActiveRadioButtonChanged += (sender, e) => {
				if (sourceGroup.ActiveRadioButton == optGithub)
					Page.XwtReferenceSource = XwtSource.Github;
				else if (sourceGroup.ActiveRadioButton == optNuGet)
					Page.XwtReferenceSource = XwtSource.NuGet;
				else
					Page.XwtReferenceSource = XwtSource.Local;
			};
			optGithub.Active = true;

			CheckBox chkGitSubmodule = null;

			if (page.Wizard.Parameters["CreateSolution"] == true.ToString ()) {
				chkGitSubmodule = new CheckBox(GettextCatalog.GetString ("Register Git Submodule\n(will be committed automatically)")) {
					TooltipText = GettextCatalog.GetString (
						"Only if you enable git version control for the new project in the last creation step.\n" +
						"The Xwt submodule will be registered and initialized during the creation process.")
				};
				chkGitSubmodule.MarginLeft = 30;
				chkGitSubmodule.Toggled += (sender, e) => Page.XwtGitSubmodule = chkGitSubmodule.Active;

				sourceGroup.ActiveRadioButtonChanged += (sender, e) => {
					if (sourceGroup.ActiveRadioButton == optGithub)
						chkGitSubmodule.Sensitive = true;
					else
						chkGitSubmodule.Sensitive = false;
				};
			}

			var tbl = new Table ();
			BackgroundColor = Color.FromBytes (225, 228, 232);

			// use inner table for selection to make it easier to add more options
			var tblSource = new Table ();
			var boxGithub = new HBox ();
			boxGithub.PackStart (optGithub);
			boxGithub.PackStart (linkGithub);
			tblSource.Add (boxGithub, 0, 0);
			if (chkGitSubmodule != null) tblSource.Add (chkGitSubmodule, 0, 1);
			tblSource.Add (optNuGet, 0, 2);
			tblSource.Add (optBuiltIn, 0, 3);

			tbl.Add (new Label (GettextCatalog.GetString ("Xwt reference source:")), 0, 0, vpos: WidgetPlacement.Start, hpos: WidgetPlacement.End);
			tbl.Add (tblSource, 1, 0);

			var rightFrame = new FrameBox ();
			rightFrame.BackgroundColor = Colors.White;
			rightFrame.WidthRequest = 280;

			Spacing = 0;
			PackStart (tbl, true, WidgetPlacement.Center, marginLeft: 30, marginRight: 30);

			PackStart (rightFrame, false, false);
		}
	}
}

