//
// Git Monitor.cs
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
using MonoDevelop.Core;
using NGit;

namespace MonoDevelop.Xwt
{

	class GitMonitor: NGit.ProgressMonitor, IDisposable
	{
		readonly Core.ProgressMonitor monitor;
		int currentWork;
		int currentStep;
		bool taskStarted;
		bool monitorStarted;

		public GitMonitor (Core.ProgressMonitor monitor)
		{
			this.monitor = monitor;
		}

		public override void Start (int totalTasks)
		{
			monitorStarted = true;
			currentStep = 0;
			currentWork = 4;
			monitor.BeginTask (null, currentWork);
		}


		public override void BeginTask (string title, int totalWork)
		{
			if (taskStarted)
				EndTask ();

			taskStarted = true;
			currentStep = 0;
			currentWork = totalWork > 0 ? totalWork : 1;
			monitor.BeginTask (title, currentWork);
		}


		public override void Update (int completed)
		{
			currentStep += completed;
			if (currentStep >= (currentWork / 100)) {
				monitor.Step (currentStep);
				currentStep = 0;
			}
		}


		public override void EndTask ()
		{
			taskStarted = false;
			monitor.EndTask ();
			monitor.Step (1);
		}


		public override bool IsCancelled ()
		{
			return monitor.CancellationToken.IsCancellationRequested;
		}

		public void Dispose ()
		{
			if (monitorStarted)
				monitor.EndTask ();
		}
	}
}

