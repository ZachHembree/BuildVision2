using Sandbox.ModAPI;
using System;
using ParallelTasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    internal class TaskPool
    {
        public delegate void ErrorCallback(List<BvException> known, BvAggregateException unknown);

        private readonly ConcurrentQueue<Action> actions;
        private readonly Queue<Action> tasksWaiting;
        private Queue<Task> tasksRunning;
        private readonly int maxTasksRunning;
        private readonly ErrorCallback errorCallback;

        public TaskPool(int maxTasksRunning, ErrorCallback errorCallback)
        {
            this.maxTasksRunning = maxTasksRunning;
            this.errorCallback = errorCallback;

            actions = new ConcurrentQueue<Action>();
            tasksWaiting = new Queue<Action>();
            tasksRunning = new Queue<Task>();
        }

        public void Update()
        {
            TryStartWaitingTasks();
            UpdateRunningTasks();
            RunTaskActions();
        }

        public void EnqueueTask(Action action) =>
            tasksWaiting.Enqueue(action);

        public void EnqueueAction(Action action) =>
            actions.Enqueue(action);

        /// <summary>
        /// Attempts to start any tasks in the waiting queue if the number of tasks running
        /// is below a set threshold.
        /// </summary>
        private void TryStartWaitingTasks()
        {
            Action action;

            if (tasksWaiting.Count > 0 && tasksRunning.Count < 2)
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
            List<BvException> knownExceptions = new List<BvException>(); //known exceptions
            BvException bvException = null;
            BvAggregateException unknownExceptions = null;

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
                unknownExceptions = new BvAggregateException(taskExceptions);

            errorCallback(knownExceptions, unknownExceptions);
        }

        /// <summary>
        /// Attempts to cast an Exception as BvException and returns true if successful.
        /// </summary>
        private static bool TryGetBvException(Exception exception, out BvException bvException)
        {
            bvException = exception as BvException;
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