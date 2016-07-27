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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.Xwt
{
	public class XwtSourcePageWidget : HBox
	{
		Spinner spinnerRepo;
		Spinner spinnerBranches;
		ComboBox cmbGithubRepo;
		ComboBox cmbGithubBranch;
		Octokit.IRepositoriesClient cGitHub;
		static ConcurrentBag<string> repositories = new ConcurrentBag<string> ();
		static ConcurrentDictionary<string, ConcurrentBag<string>> branches = new ConcurrentDictionary<string, ConcurrentBag<string>> ();
		
		public XwtSourceWizardPage Page { get; private set; }

		public XwtSourcePageWidget (XwtSourceWizardPage page)
		{
			Page = page;

			cGitHub = new Octokit.GitHubClient (new Octokit.ProductHeaderValue ("xwt_addin")).Repository;

			var optBuiltIn = new RadioButton(GettextCatalog.GetString ("Local Package / GAC")) {
				TooltipText = GettextCatalog.GetString (
					"Xwt must be installed to the GAC (Global Assembly Cache),\n" +
					"otherwise you will have to add a HintPath property manually.")
			};
			var optGithub = new RadioButton(GettextCatalog.GetString ("GitHub:")) {
				TooltipText = GettextCatalog.GetString (
					"A separate solution folder named 'Xwt' will be added to the solution.\n" +
					"If the solution already contains the Xwt project,\nit will be referenced instead " +
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
			BackgroundColor = Styles.NewProjectDialog.ProjectConfigurationLeftHandBackgroundColor;

			// use inner table for selection to make it easier to add more options
			var tblSource = new Table ();
			var boxGithub = new HBox ();
			boxGithub.PackStart (optGithub);
			boxGithub.PackStart (linkGithub);
			tblSource.Add (boxGithub, 0, 0);

			var tblGithubRepo = new Table ();
			tblGithubRepo.MarginLeft = 30;

			tblGithubRepo.Add (new Label (GettextCatalog.GetString ("Repository:")), 0, 0);
			cmbGithubRepo = new ComboBox ();
			cmbGithubRepo.Items.Add ("mono/xwt");
			cmbGithubRepo.SelectedIndex = 0;
			cmbGithubRepo.SelectionChanged += (sender, e) => UpdateBranches (cmbGithubRepo.SelectedText);
			tblGithubRepo.Add (cmbGithubRepo, 1, 0);
			spinnerRepo = new Spinner { Visible = false };
			tblGithubRepo.Add (spinnerRepo, 2, 0);


			tblGithubRepo.Add (new Label (GettextCatalog.GetString ("Branch:")), 0, 1);
			cmbGithubBranch = new ComboBox ();
			cmbGithubBranch.Items.Add ("master");
			cmbGithubBranch.SelectedIndex = 0;
			cmbGithubBranch.SelectionChanged += (sender, e) => Page.XwtGithubBranch = cmbGithubBranch.SelectedText;
			tblGithubRepo.Add (cmbGithubBranch, 1, 1);
			spinnerBranches = new Spinner { Visible = false };
			tblGithubRepo.Add (spinnerBranches, 2, 1);

			tblSource.Add (tblGithubRepo, 0, 1);

			if (chkGitSubmodule != null) tblSource.Add (chkGitSubmodule, 0, 2);
			tblSource.Add (optNuGet, 0, 3);
			tblSource.Add (optBuiltIn, 0, 4);

			tbl.Add (new Label (GettextCatalog.GetString ("Xwt reference source:")), 0, 0, vpos: WidgetPlacement.Start, hpos: WidgetPlacement.End);
			tbl.Add (tblSource, 1, 0);

			var rightFrame = new FrameBox ();
			rightFrame.BackgroundColor = Styles.NewProjectDialog.ProjectConfigurationRightHandBackgroundColor; ;
			rightFrame.WidthRequest = 280;

			Spacing = 0;
			PackStart (tbl, true, WidgetPlacement.Center, marginLeft: 30, marginRight: 30);

			PackStart (rightFrame, false, false);
			UpdateForks ();
			UpdateBranches ("mono/xwt");
		}

		async void UpdateBranches (string fullName)
		{
			Page.XwtGithubRepository = fullName;
			try {
				var spl = fullName.Split ('/');
				if (spl.Length != 2)
					throw new InvalidOperationException ();
				Application.Invoke (() => spinnerBranches.Animate = spinnerBranches.Visible = true);
				var repoBranches = new ConcurrentBag<string> ((await cGitHub.GetAllBranches (spl [0], spl [1])).Select (b => b.Name));
				branches [fullName] = repoBranches;
				cmbGithubBranch.Items.Clear ();
				foreach (var branch in repoBranches.OrderBy (f => f)) {
					LoggingService.LogInfo (branch);
					Application.Invoke (() => {
						cmbGithubBranch.Items.Add (branch);
						if (branch == "master")
							cmbGithubBranch.SelectedItem = "master";
					});
				}
			} catch (Octokit.RateLimitExceededException) {
				LoggingService.LogWarning ("Github API rate limit exceeded");
			} catch (Exception ex) {
				LoggingService.LogError ("Failed fetching Xwt repositories", ex);
			} finally {
				Application.Invoke (() => {
					cmbGithubBranch.Items.Clear ();
					ConcurrentBag<string> repoBranches = null;
					if (branches.TryGetValue (fullName, out repoBranches) && repoBranches?.Count > 0) {
						foreach (var branch in repoBranches.OrderBy (f => f)) {
							cmbGithubBranch.Items.Add (branch);
							if (branch == "master")
								cmbGithubBranch.SelectedItem = "master";
						}
					} else {
						cmbGithubBranch.Items.Add ("master");
						cmbGithubBranch.SelectedIndex = 0;
					}
					spinnerBranches.Animate = spinnerBranches.Visible = false;
				});
			}

		}

		async void UpdateForks ()
		{
			Application.Invoke (() => spinnerRepo.Animate = spinnerRepo.Visible = true);

			try {
				var forks = await cGitHub.Forks.GetAll ("mono", "xwt", Octokit.ApiOptions.None);
				repositories = new ConcurrentBag<string> (forks.Select (f => f.FullName));
			} catch (Octokit.RateLimitExceededException) {
				LoggingService.LogWarning ("Github API rate limit exceeded");
			} catch (Exception ex) {
				LoggingService.LogError ("Failed fetching Xwt repositories", ex);
			} finally {
				if (repositories?.Count > 0)
					foreach (var fork in repositories.OrderBy (f => f))
						Application.Invoke (() => cmbGithubRepo.Items.Add (fork));
				Application.Invoke (() => spinnerRepo.Animate = spinnerRepo.Visible = false);
			}
		}
	}
}

