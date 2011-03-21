


using System;
using System.Threading;

namespace ProphetsWay.Utilities
{
	public abstract class BackgroundWorkingClass
	{
		protected BackgroundWorkingClass(string name)
		{
			_workName = name;
		}

		protected bool Cancel;

		private readonly string _workName;

		public bool Working { get; protected set; }

		protected void StartWork(object args = null)
		{
			Logger.Info("Starting: " + _workName);
			Working = true;

			var th = new Thread(DoWork);
			th.Start(args);
		}

		public void AbortWork()
		{
			if(Working)
			{
				Logger.Info("Cancelling: " + _workName);
				Cancel = true;
			}
			else
				Logger.Info("Cancel invoked when not currently working.");
		}

		protected abstract void DoWork(object args);

		public abstract event WorkFinished WorkFinished;
		public virtual event ProgressUpdate ProgressUpdate;
		public virtual event StatusUpdate StatusUpdate;
	}

	public delegate void WorkFinished(object sender, WorkProgressArgs args);

	public delegate void ProgressUpdate(object sender, WorkProgressArgs args);

	public delegate void StatusUpdate(object sender, WorkStatusArgs args);
	
	public class WorkProgressArgs : WorkStatusArgs
	{
		public WorkProgressArgs(int progress = 100, int max = 100)
		{
			Progress = progress;
			MaxProgress = max;
		}

		public int Progress { get; private set; }
		public int MaxProgress { get; private set; }
	}

	public class WorkStatusArgs :EventArgs
	{
		
	}
}
