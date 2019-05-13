using Sandbox.ModAPI;
using System;
using ParallelTasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using DarkHelmet.Game;

namespace DarkHelmet
{
    /// <summary>
    /// This class should not exist. I should have access to the .NET framework
    /// AggregateException class, but here we are.
    /// </summary>
    internal class AggregateException : Exception
    {
        public AggregateException(string aggregatedMsg) : base(aggregatedMsg)
        { }

        public AggregateException(IList<Exception> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public AggregateException(IList<IOException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public AggregateException(IList<AggregateException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        private static string GetExceptionMessages<T>(IList<T> exceptions) where T : Exception
        {
            StringBuilder sb = new StringBuilder();

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append($"{exceptions[n].ToString()}\n");
                else
                    sb.Append($"{exceptions[n].ToString()}");

            return sb.ToString();
        }
    }

    internal class IOException : Exception
    {
        public IOException() : base()
        { }

        public IOException(string message) : base(message)
        { }

        public IOException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    internal class TaskPool
    {
        private readonly ConcurrentQueue<Action> actions;
        private readonly Queue<Action> tasksWaiting;
        private Queue<Task> tasksRunning;
        private readonly int maxTasksRunning;
        private readonly Action<List<IOException>, AggregateException> errorCallback;

        public TaskPool(int maxTasksRunning, Action<List<IOException>, AggregateException> errorCallback)
        {
            this.maxTasksRunning = maxTasksRunning;
            this.errorCallback = errorCallback;

            actions = new ConcurrentQueue<Action>();
            tasksWaiting = new Queue<Action>();
            tasksRunning = new Queue<Task>();
        }

        /// <summary>
        /// Updates internal queues
        /// </summary>
        public void Update()
        {
            TryStartWaitingTasks();
            UpdateRunningTasks();
            RunTaskActions();
        }

        /// <summary>
        /// Enqueues an action to run in parallel. Not thread safe; must be called from the main thread.
        /// </summary>
        public void EnqueueTask(Action action) =>
            tasksWaiting.Enqueue(action);

        /// <summary>
        /// Enqueues an action to run on the main thread. Meant to be used by threads other than the main.
        /// </summary>
        public void EnqueueAction(Action action) =>
            actions.Enqueue(action);

        /// <summary>
        /// Attempts to start any tasks in the waiting queue if the number of tasks running
        /// is below a set threshold.
        /// </summary>
        private void TryStartWaitingTasks()
        {
            Action action;

            if (tasksWaiting.Count > 0 && tasksRunning.Count <= maxTasksRunning)
            {
                if (tasksWaiting.TryDequeue(out action))
                    tasksRunning.Enqueue(MyAPIGateway.Parallel.Start(action));
            }
        }

        /// <summary>
        /// Checks a task queue for invalid tasks and tasks with exceptions, logs and throws exceptions
        /// as needed and rebuilds queue with remaining valid tasks.
        /// </summary>
        private void UpdateRunningTasks()
        {
            Task task;
            Queue<Task> currentTasks = new Queue<Task>();
            List<Exception> taskExceptions = new List<Exception>(); //unknown exceptions
            List<IOException> knownExceptions = new List<IOException>(); 
            IOException bvException = null;
            AggregateException unknownExceptions = null;

            while (tasksRunning.Count > 0)
            {
                if (tasksRunning.TryDequeue(out task) && task.valid)
                    if (task.Exceptions != null)
                    {
                        if (task.Exceptions.Length > 0)
                        {
                            foreach (Exception exception in task.Exceptions)
                            {
                                if (TryGetBvException(exception, out bvException))
                                    knownExceptions.Add(bvException);
                                else
                                    taskExceptions.Add(exception);
                            }
                        }
                    }
                    else if (!task.IsComplete)
                        currentTasks.Enqueue(task);
            }

            tasksRunning = currentTasks;

            if (taskExceptions.Count > 0)
                unknownExceptions = new AggregateException(taskExceptions);

            errorCallback(knownExceptions, unknownExceptions);
        }

        /// <summary>
        /// Attempts to cast an Exception as BvException and returns true if successful.
        /// </summary>
        private static bool TryGetBvException(Exception exception, out IOException bvException)
        {
            bvException = exception as IOException;
            return bvException != null;
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