
using System;
using System.Collections.Generic;

#if !WINDOWS_UWP && !(UNITY_2017_OR_NEWER && !(NET_2_0 || NET_2_0_SUBSET))
namespace MySpace.Tasks
{
    public class Task
    {
        private class Worker
        {
            private const int maxThreads = 3;
            private const int initialJobs = 1;
            private volatile bool exit;
            private readonly System.Threading.AutoResetEvent jobEvent;
            private readonly LinkedList<Task> tasks;
            private readonly LinkedList<Task> stock;
            public Worker()
            {
                //UnityEngine.Debug.Log("worker start");
                exit = false;
                tasks = new LinkedList<Task>();
                stock = new LinkedList<Task>();
                for (int i = 0; i < initialJobs; i++)
                {
                    Task empty = null;
                    stock.AddLast(empty);
                }
                jobEvent = new System.Threading.AutoResetEvent(false);
                for (int i = 0; i < maxThreads; i++)
                {
                    var thread = new System.Threading.Thread(ThreadMain);
                    thread.Priority = System.Threading.ThreadPriority.Normal;
                    thread.Start();
                    thread.IsBackground = true;
                }
            }

            private void ThreadMain()
            {
                try
                {
                    while (!exit)
                    {
                        jobEvent.WaitOne();
                        while (tasks.Count != 0)
                        {
                            ProcessJob();
                        }
                    }
                }
                catch //(Exception ec)
                {
                    //UnityEngine.Debug.LogException(ec);
                }
            }
            private void ProcessJob()
            {
                Task task = null;
                lock (tasks)
                {
                    if (tasks.Count != 0)
                    {
                        LinkedListNode<Task> node = tasks.First;
                        tasks.Remove(node);
                        task = node.Value;
                        node.Value = null;
                        stock.AddLast(node);
                    }
                    if (tasks.Count != 0)
                    {
                        jobEvent.Set();
                    }
                }
                if ((task != null) && !exit)
                {
                    try
                    {
                        task.state = TaskState.Running;
                        task.job.Invoke();
                        task.SetResult(TaskState.RanToCompletion);
                    }
                    catch (System.Exception ec)
                    {
                        task.exception = ec;
                        task.SetResult(TaskState.Faulted);
                        if (ec is System.Threading.ThreadAbortException)
                        {
                        }
                        else
                        {
                            UnityEngine.Debug.LogException(ec);
                        }
                    }
                }
            }
            public void Delegate(Task task)
            {
                task.state = TaskState.WaitingToRun;
                lock (tasks)
                {
                    LinkedListNode<Task> node = stock.First;
                    if (node == null)
                    {
                        node = new LinkedListNode<Task>(task);
                    }
                    else
                    {
                        stock.Remove(node);
                        node.Value = task;
                    }
                    tasks.AddLast(node);
                    jobEvent.Set();
                }
            }
        }

        public enum TaskState
        {
            Canceled,
            Created,
            Faulted,
            RanToCompletion,
            Running,
            WaitingForActivation,
            WaitingForChildrenToComplete,
            WaitingToRun,
        }
        private Exception exception;
        private Action job;
        private volatile TaskState state;
        private volatile System.Threading.ManualResetEvent complete;

        private static Worker worker = new Worker();

        public Task(Action job)
        {
            state = TaskState.Created;
            this.job = job;
        }
        private void SetResult(TaskState state)
        {
            this.state = state;
            var mre = complete;
            if (mre != null)
            {
                mre.Set();
            }
        }

        public TaskState Status
        {
            get
            {
                return state;
            }
        }
        public Exception Exception
        {
            get
            {
                return exception;
            }
        }
        public bool IsCompleted
        {
            get
            {
                TaskState st = state;
                return (st == TaskState.Canceled) || (st == TaskState.Faulted) || (st == TaskState.RanToCompletion);
            }
        }
        public static Task Run(Action action)
        {
            Task t = new Task(action);
            t.Start();
            return t;
        }
        public void Start()
        {
            if (state != TaskState.Created)
            {
                throw new System.InvalidOperationException();
            }
            state = TaskState.WaitingForActivation;
            worker.Delegate(this);
        }
        public void Wait()
        {
            if (IsCompleted)
            {
                return;
            }
            var mre = MyFountain<System.Threading.ManualResetEvent>.Get(() => new System.Threading.ManualResetEvent(false));
            mre.Reset();
            complete = mre;
            if (!IsCompleted)
            {
                complete.WaitOne();
            }
            complete = null;
            MyFountain<System.Threading.ManualResetEvent>.Put(mre);
        }
    }
}
#endif
