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
    /// Dead simple aggregate exception class. Feed it a list of exceptions and out pops a new exception with their contents
    /// crammed into one message.
    /// </summary>
    public class AggregateException : Exception
    {
        public AggregateException(string aggregatedMsg) : base(aggregatedMsg)
        { }

        public AggregateException(IList<Exception> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        public AggregateException(IList<AggregateException> exceptions) : base(GetExceptionMessages(exceptions))
        { }

        private static string GetExceptionMessages<T>(IList<T> exceptions) where T : Exception
        {
            StringBuilder sb = new StringBuilder(exceptions[0].Message.Length * exceptions.Count);

            for (int n = 0; n < exceptions.Count; n++)
                if (n != exceptions.Count - 1)
                    sb.Append(exceptions[n].ToString() + "\n");
                else
                    sb.Append(exceptions[n].ToString());

            return sb.ToString();
        }
    }

    /// <summary>
    /// Used to separate exceptions thrown manually in a task from random unhandled exceptions that weren't planned for
    /// in the application.
    /// </summary>
    public class KnownException : Exception
    {
        public KnownException() : base()
        { }

        public KnownException(string message) : base(message)
        { }

        public KnownException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    public sealed class TaskPool : ModBase.Component<TaskPool>
    {
        public static int MaxTasksRunning { get { return maxTasksRunning; } set { maxTasksRunning = Utilities.Clamp(value, 1, 10); } }
        private static List<Client> TaskPoolClients { get { return Instance.taskPoolClients; } }
        private static int maxTasksRunning = 1;

        public int TasksRunning
        {
            get
            {
                int total = 0;

                for (int n = 0; n < taskPoolClients.Count; n++)
                    total += taskPoolClients[n].tasksRunning.Count;

                return total;
            }
        }

        public int TasksWaiting
        {
            get
            {
                int total = 0;

                for (int n = 0; n < taskPoolClients.Count; n++)
                    total += taskPoolClients[n].tasksWaiting.Count;

                return total;
            }
        }

        private int currentClient;
        private readonly List<Client> taskPoolClients;

        public TaskPool()
        {
            currentClient = 0;
            taskPoolClients = new List<Client>();
        }

        protected override void BeforeClose()
        {
            foreach (Client client in taskPoolClients)
                client.registered = false;
        }

        public static IClient GetTaskPoolClient(Action<List<KnownException>, AggregateException> errorCallback)
        {
            return new Client(errorCallback);
        }

        protected override void Update()
        {
            while (TasksRunning < maxTasksRunning && TasksWaiting > 0)
            {
                taskPoolClients[currentClient].TryStartWaitingTask();
                currentClient++;

                if (currentClient >= taskPoolClients.Count)
                    currentClient = 0;
            }

            for (int n = 0; n < taskPoolClients.Count; n++)
            {
                taskPoolClients[n].UpdateRunningTasks();
                taskPoolClients[n].RunTaskActions();
            }
        }

        public interface IClient
        {
            void UnregisterClient();

            void EnqueueTask(Action action);

            void EnqueueAction(Action action);
        }

        private class Client : IClient
        {
            public readonly LinkedList<Task> tasksRunning;
            public readonly Queue<Action> tasksWaiting;
            public readonly Action<List<KnownException>, AggregateException> errorCallback;
            public bool registered;

            private readonly ConcurrentQueue<Action> actions;

            public Client(Action<List<KnownException>, AggregateException> errorCallback)
            {
                this.errorCallback = errorCallback;
                tasksRunning = new LinkedList<Task>();
                actions = new ConcurrentQueue<Action>();
                tasksWaiting = new Queue<Action>();
                registered = false;
            }

            public void TryRegisterClient()
            {
                if (!registered && !TaskPoolClients.Contains(this))
                {
                    TaskPoolClients.Add(this);
                    registered = true;
                }
            }

            public void UnregisterClient()
            {
                TaskPoolClients.Remove(this);
            }

            /// <summary>
            /// Enqueues an action to run in parallel. Not thread safe; must be called from the main thread.
            /// </summary>
            public void EnqueueTask(Action action)
            {
                TryRegisterClient();
                tasksWaiting.Enqueue(action);
            }

            /// <summary>
            /// Enqueues an action to run on the main thread. Meant to be used by threads other than the main.
            /// </summary>
            public void EnqueueAction(Action action)
            {
                TryRegisterClient();
                actions.Enqueue(action);
            }

            /// <summary>
            /// Attempts to start any tasks in the waiting queue if the number of tasks running
            /// is below a set threshold.
            /// </summary>
            public bool TryStartWaitingTask()
            {
                Action action;

                if (tasksWaiting.Count > 0)
                {
                    if (tasksWaiting.TryDequeue(out action))
                    {
                        tasksRunning.AddLast(MyAPIGateway.Parallel.Start(action));
                        return true;
                    }
                    else
                        return false;  
                }
                else
                    return false;
            }

            /// <summary>
            /// Checks a task queue for invalid tasks and tasks with exceptions, logs and throws exceptions
            /// as needed and rebuilds queue with remaining valid tasks.
            /// </summary>
            public void UpdateRunningTasks()
            {
                List<KnownException> knownExceptions = new List<KnownException>();
                List<Exception> otherExceptions = new List<Exception>(); //unknown exceptions
                AggregateException unknownExceptions = null;
                LinkedListNode<Task> node = tasksRunning.First;

                while (node != null)
                {
                    Task task = node.Value;

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
                        tasksRunning.Remove(node);

                    node = node.Next;
                }

                if (otherExceptions.Count > 0)
                    unknownExceptions = new AggregateException(otherExceptions);

                errorCallback(knownExceptions, unknownExceptions);
            }

            /// <summary>
            /// Checks actions queue for any actions sent from tasks to be executed on the main 
            /// thread and executes them.
            /// </summary>
            public void RunTaskActions()
            {
                Action action;

                while (actions.Count > 0)
                    if (actions.TryDequeue(out action))
                        action();
            }
        }
    }
}