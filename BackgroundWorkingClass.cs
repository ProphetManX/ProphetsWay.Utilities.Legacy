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

		public void StartWork(object args = null)
		{
			Logger.Info("Starting: " + _workName);
			Working = true;
			Cancel = false;

			var th = new Thread(DoWork);
			th.Start(args);
		}

		public void AbortWork()
		{
			if (Working)
			{
				Logger.Info("Cancelling: " + _workName);
				Cancel = true;
			}
			else
				Logger.Info("Cancel invoked when not currently working.");
		}

		protected abstract void DoWork(object args);

		public EventHandler<WorkFinishedArgs> WorkFinished;
		public EventHandler<WorkProgressArgs> ProgressUpdate;
		public EventHandler<WorkStatusArgs> StatusUpdate;
	}

	public class WorkFinishedArgs : WorkProgressArgs
	{
		public WorkFinishedArgs(object output, string message = null, int progress = 100, int max = 100)
			: base(progress, max, message)
		{
			Results = output;
		}

		public object Results { get; set; }
	}

	public class WorkProgressArgs : WorkStatusArgs
	{
		public WorkProgressArgs(int progress = 100, int max = 100, string message = null) : base(message)
		{
			Progress = progress;
			MaxProgress = max;
		}

		public int Progress { get; private set; }
		public int MaxProgress { get; private set; }
	}

	public class WorkStatusArgs : EventArgs
	{
		public WorkStatusArgs(string message)
		{
			Message = message;
		}

		public string Message { get; private set; }
	}
}
