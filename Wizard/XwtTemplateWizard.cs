//
// XwtTemplateWizard.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.ProgressMonitoring;
using MonoDevelop.Ide.Templates;
using MonoDevelop.Projects;
using MonoDevelop.VersionControl;
using MonoDevelop.VersionControl.Git;
using NGit.Api;
using NGit.Storage.File;

namespace MonoDevelop.Xwt
{
	public class XwtTemplateWizard : TemplateWizard
	{
		
		public override string Id {
			get {
				return "MonoDevelop.Xwt.XwtTemplateWizard";
			}
		}

		public override WizardPage GetPage(int pageNumber)
		{
			return new XwtSourceWizardPage(this);
		}

		public override void ItemsCreated (IEnumerable<IWorkspaceFileObject> items)
		{
			Solution gitSolution = null;

			foreach (var item in items) {
				var solution = item as Solution;

				if (solution == null) {
					var project = item as DotNetProject;
					if (project != null) {
						solution = project.ParentSolution;
						ConfigureProject (project);
					}
				} else foreach (var project in solution.GetAllProjects ())
						ConfigureProject (project as DotNetProject);
				

				if (gitSolution == null)
					gitSolution = solution;
			}
			
			if (gitSolution != null && Parameters ["XwtSourceGithub"] == true.ToString ())
			{
				var monitor = new MessageDialogProgressMonitor (true, true, true, true);
				Task.Run (async delegate {
					await AddXwtFromGithubAsync (
						gitSolution,
						Parameters ["UserDefinedProjectName"],
						Parameters ["XwtGitSubmodule"] == true.ToString (),
						monitor);
				});
			}

			base.ItemsCreated (items);
		}

		void ConfigureProject (DotNetProject project)
		{
			if (project == null)
				return;
			if (project != null && project.LanguageName == "VBNet")
				project.UseMSBuildEngine = false;
			if (project.MSBuildProject != null) {
				if (project.MSBuildProject.ProjectTypeGuids.Select (t => t.ToUpper ()).Contains ("{948B3504-5B70-4649-8FE4-BDE1FB46EC69}")) {
					project.MSBuildProject.GetGlobalPropertyGroup ().SetValue ("SuppressXamMacMigration", true);
					project.MSBuildProject.Save (project.MSBuildProject.FileName);
				}
				else if (project.MSBuildProject.ProjectTypeGuids.Select (t => t.ToUpper ()).Contains ("{A3F8F2AB-B479-4A4A-A458-A89E7DC349F1}")) {
					project.MSBuildProject.GetGlobalPropertyGroup ().SetValue ("UseXamMacFullFramework", true);
					project.MSBuildProject.Save (project.MSBuildProject.FileName);
				}
			}
		}

		void ConfigureProjectIfMonoMac (DotNetProject project)
		{
			
		}

		async Task AddXwtFromGithubAsync (Solution solution, string newProjectName, bool createSubmodule, ProgressMonitor monitor)
		{
			try {
				var gitRepo = VersionControlService.GetRepository (solution) as GitRepository;
				var xwt_proj = solution.FindProjectByName ("Xwt") as DotNetProject;
				var xwt_path = xwt_proj == null ? solution.BaseDirectory.Combine ("Xwt") : xwt_proj.BaseDirectory.ParentDirectory;

				monitor.BeginTask ("Configuring Xwt References...", 3);
				monitor.BeginTask ("Cloning Xwt into " + xwt_path + "...", 1);

				if (xwt_proj == null && !Directory.Exists (xwt_path)) {
					if (createSubmodule && gitRepo != null) {
						monitor.BeginTask ("Initializing Xwt submodule in " + xwt_path + "...", 1);

						var repo = new FileRepository (gitRepo.RootPath.Combine (".git"));
						var git = new Git (repo);

						try {
							using (var gm = new GitMonitor (monitor)) {
								var add_submodule = git.SubmoduleAdd ();
								add_submodule.SetPath ("Xwt");
								add_submodule.SetURI ("git://github.com/mono/xwt");
								add_submodule.SetProgressMonitor (gm);
								add_submodule.Call ();
							}
						} catch {
							Directory.Delete (xwt_path, true);
							throw;
						}

						monitor.EndTask ();
						monitor.BeginTask ("Comitting changes...", 1);

						var commit = git.Commit ();
						commit.SetMessage ("Add Xwt Submodule");
						commit.Call ();

						monitor.EndTask ();
					} else {

						var repo = new GitRepository ();
						repo.Url = "git://github.com/mono/xwt";
						repo.Checkout (xwt_path, true, monitor);
					}
				}

				SolutionFolder xwt_folder;
				if (xwt_proj != null)
					xwt_folder = xwt_proj.ParentFolder;
				else {
					xwt_folder = new SolutionFolder ();
					xwt_folder.Name = "Xwt";
				}
				solution.RootFolder.Items.Add (xwt_folder);

				monitor.EndTask ();
				monitor.Step (1);

				monitor.BeginTask ("Adding Xwt Projects to Solution...", 7);

				if (xwt_proj == null && File.Exists (xwt_path.Combine ("Xwt", "Xwt.csproj"))) {
					xwt_proj = await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt", "Xwt.csproj")
					) as DotNetProject;
				}
				if (xwt_proj == null)
					throw new InvalidOperationException ("Xwt project not found");

				monitor.Step (1);

				var xwt_gtk_proj = solution.FindProjectByName ("Xwt.Gtk") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.Gtk", "Xwt.Gtk.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_gtk_win_proj = solution.FindProjectByName ("Xwt.Gtk.Windows") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.Gtk.Windows", "Xwt.Gtk.Windows.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_gtk_mac_proj = solution.FindProjectByName ("Xwt.Gtk.Mac") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.Gtk.Mac", "Xwt.Gtk.Mac.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_gtk3_proj = solution.FindProjectByName ("Xwt.Gtk3") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.Gtk", "Xwt.Gtk3.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_wpf_proj = solution.FindProjectByName ("Xwt.WPF") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.WPF", "Xwt.WPF.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_mac_proj = solution.FindProjectByName ("Xwt.Mac") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.Mac", "Xwt.Mac.csproj")
					) as DotNetProject;

				monitor.Step (1);

				var xwt_xammac_proj = solution.FindProjectByName ("Xwt.XamMac") ??
					await IdeApp.ProjectOperations.AddSolutionItem (
						xwt_folder,
						xwt_path.Combine ("Xwt.XamMac", "Xwt.XamMac.csproj")
					) as DotNetProject;


				monitor.EndTask ();
				monitor.Step (1);
				monitor.BeginTask ("Adding Xwt References...", solution.Items.Count);

				foreach (var item in solution.Items) {
					var project = item as DotNetProject;
					if (project != null) {
						if (project.Name == newProjectName ||
						    project.Name.StartsWith (newProjectName + ".", StringComparison.Ordinal))
							project.References.Add (ProjectReference.CreateProjectReference (xwt_proj));
						
						if (project.Name == newProjectName + ".Desktop") {
							if (Platform.IsWindows) {
								project.References.Add (ProjectReference.CreateProjectReference (xwt_wpf_proj));
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_proj));
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_win_proj));
							} else if (Platform.IsLinux) {
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_proj));
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk3_proj));
							} else if (Platform.IsMac) {
								project.References.Add (ProjectReference.CreateProjectReference (xwt_xammac_proj));
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_proj));
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_mac_proj));
							}
						}

						if (project.Name == newProjectName + ".Gtk2") {
							project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_proj));
							if (Platform.IsWindows) {
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_win_proj));
							} else if (Platform.IsMac) {
								project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk_mac_proj));
							}
						}

						if (project.Name == newProjectName + ".Wpf") {
							project.References.Add (ProjectReference.CreateProjectReference (xwt_wpf_proj));
						}

						if (project.Name == newProjectName + ".Gtk3") {
							project.References.Add (ProjectReference.CreateProjectReference (xwt_gtk3_proj));
						}

						if (project.Name == newProjectName + ".Mac") {
							project.References.Add (ProjectReference.CreateProjectReference (xwt_mac_proj));
						}

						if (project.Name == newProjectName + ".XamMac") {
							project.References.Add (ProjectReference.CreateProjectReference (xwt_xammac_proj));
						}
					}
					monitor.Step (1);
				}

				monitor.EndTask ();
				monitor.EndTask ();
				monitor.ReportSuccess ("Xwt Repository initialized successfully");

				await IdeApp.Workspace.SaveAsync (monitor);
			} catch (Exception e) {
				string msg = GettextCatalog.GetString ("Adding Xwt reference failed: ");
				monitor.ReportError (msg, e);
				MessageService.ShowError (msg, e);
			} finally {
				monitor.Dispose ();
			}
		}
	}
}

