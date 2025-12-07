using ParallelTasks;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RichHudFramework
{
	/// <summary>
	/// Aggregates multiple exceptions into a single exception with a combined message.
	/// Used by <see cref="TaskPool"/> for clean error reporting when parallel tasks fail.
	/// </summary>
	public class AggregateException : Exception
	{
		public AggregateException(string aggregatedMsg) : base(aggregatedMsg) { }

		public AggregateException(IReadOnlyList<Exception> exceptions) : base(BuildMessage(exceptions)) { }

		public AggregateException(IReadOnlyList<AggregateException> exceptions) : base(BuildMessage(exceptions)) { }

		private static string BuildMessage<T>(IReadOnlyList<T> exceptions) where T : Exception
		{
			var sb = new StringBuilder();

			for (int i = 0; i < exceptions.Count; i++)
			{
				sb.AppendLine(exceptions[i].ToString());
				if (i < exceptions.Count - 1)
					sb.AppendLine();
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Marks an exception as expected/known (e.g., failed file load, invalid user data).
	/// Allows the framework to distinguish between bugs and recoverable errors.
	/// </summary>
	public class KnownException : Exception
	{
		public KnownException() : base() { }
		public KnownException(string message) : base(message) { }
		public KnownException(string message, Exception innerException) : base(message, innerException) { }
	}

	/// <summary>
	/// Global parallel task pool used by RichHudFramework. Limits concurrent background tasks.
	/// Tasks are started from the main thread only; results/actions can be posted back safely.
	/// </summary>
	/// <exclude/>
	public class TaskPool : RichHudComponentBase
	{
		/// <summary>
		/// Global limit on simultaneously running tasks across all <see cref="TaskPool"/> instances.
		/// </summary>
		public static int MaxTasksRunning { get { return maxTasksRunning; } set { maxTasksRunning = MathHelper.Clamp(value, 1, 10); } }
        private static int maxTasksRunning = 1, tasksRunningCount = 0;

		private readonly List<Task> tasksRunning;
		private readonly Queue<Action> tasksWaiting;
		private readonly ConcurrentQueue<Action> actions;
		private readonly Action<List<KnownException>, AggregateException> errorCallback;

		public TaskPool(Action<List<KnownException>, AggregateException> errorCallback) : base(true, true)
		{
			this.errorCallback = errorCallback;

			tasksRunning = new List<Task>();
			actions = new ConcurrentQueue<Action>();
			tasksWaiting = new Queue<Action>();
		}

		/// <summary>
		/// Called when the mod is unloaded
		/// </summary>
		public override void Close() => tasksRunningCount = 0;

		/// <summary>
		/// Main-thread update: starts new tasks if under limit, cleans completed/failed tasks,
		/// executes actions posted from background threads, and reports exceptions.
		/// </summary>
		public override void Draw()
		{
			TryStartWaitingTasks();
			UpdateRunningTasks();
			RunTaskActions();
		}

		/// <summary>
		/// Enqueues a delegate to run on a background thread (via ParallelTasks).
		/// Must be called from the main thread.
		/// </summary>
		public void EnqueueTask(Action action)
		{
			if (Parent == null && RichHudCore.Instance != null)
				RegisterComponent(RichHudCore.Instance);
			else if (ExceptionHandler.Unloading)
				throw new Exception("New tasks cannot be started while the mod is being unloaded.");

			tasksWaiting.Enqueue(action);
		}

		/// <summary>
		/// Enqueues an action to be executed on the next main-thread update.
		/// Thread-safe – intended for background tasks to safely affect game state.
		/// </summary>
		public void EnqueueAction(Action action)
		{
			if (Parent == null && RichHudCore.Instance != null)
				RegisterComponent(RichHudCore.Instance);
			else if (ExceptionHandler.Unloading)
				throw new Exception("New tasks cannot be started while the mod is being unloaded.");

			actions.Enqueue(action);
		}

		/// <summary>
		/// Attempts to start any tasks in the waiting queue if the number of tasks running
		/// is below a set threshold.
		/// </summary>
		private void TryStartWaitingTasks()
		{
			Action action;

			while (tasksRunningCount < maxTasksRunning && (tasksWaiting.Count > 0) && tasksWaiting.TryDequeue(out action))
			{
				tasksRunning.Add(MyAPIGateway.Parallel.Start(action));
				tasksRunningCount++;
			}
		}

		/// <summary>
		/// Checks the task list for invalid tasks and tasks with exceptions then logs and throws exceptions as needed.
		/// </summary>
		private void UpdateRunningTasks()
		{
			List<KnownException> knownExceptions = new List<KnownException>();
			List<Exception> otherExceptions = new List<Exception>(); //unknown exceptions
			AggregateException unknownExceptions = null;

			for (int n = 0; n < tasksRunning.Count; n++)
			{
				Task task = tasksRunning[n];

				if (task.Exceptions != null && task.Exceptions.Length > 0)
				{
					foreach (Exception exception in task.Exceptions)
					{
						if (exception is KnownException)
							knownExceptions.Add((KnownException)exception);
						else
							otherExceptions.Add(exception);
					}
				}

				if (!task.valid || task.IsComplete || (task.Exceptions != null && task.Exceptions.Length > 0))
				{
					tasksRunning.Remove(task);
					tasksRunningCount--;
				}
			}

			if (otherExceptions.Count > 0)
				unknownExceptions = new AggregateException(otherExceptions);

			errorCallback(knownExceptions, unknownExceptions);
		}

		/// <summary>
		/// Checks actions queue for any actions sent from tasks to be executed on the main 
		/// thread and executes them.
		/// </summary>
		private void RunTaskActions()
		{
			Action action;

			while (actions.Count > 0)
				if (actions.TryDequeue(out action))
					action();
		}
	}
}