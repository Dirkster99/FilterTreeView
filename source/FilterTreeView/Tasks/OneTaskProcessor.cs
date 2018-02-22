namespace FilterTreeView.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a background thread processor that will process
    /// ONLY ONE task at any time.
    /// </summary>
    internal class OneTaskProcessor : IDisposable
    {
        #region fields
        private readonly OneTaskLimitedScheduler _myTaskScheduler;
        private readonly List<TaskItem> _myTaskList;
        private readonly SemaphoreSlim _Semaphore;

        private bool _Disposed;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public OneTaskProcessor()
        {
            _myTaskScheduler = new OneTaskLimitedScheduler();
            _myTaskList = new List<TaskItem>();

            _Semaphore = new SemaphoreSlim(1, 1);
            _Disposed = false;
        }
        #endregion constructors

        #region methods
        /// <summary>
        /// Implements the <see cref="IDisposable"/> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Schedules a function that returns an int value for execution in a
        /// one task at a time background thread framework.
        /// 
        /// All previously scheduled tasks are cancelled (if any).
        /// Their is only one task being executed at any time (via custom <see cref="TaskScheduler"/>.
        /// </summary>
        /// <param name="funcToExecute"></param>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        internal async Task<int> ExecuteOneTask(Func<int> funcToExecute,
                                                CancellationTokenSource tokenSource)
        {
            try
            {
                for (int i = 0; i < _myTaskList.Count; i++)
                {
                    if (_myTaskList[i].Cancellation != null)
                    {
                        _myTaskList[i].Cancellation.Cancel();
                        _myTaskList[i].Cancellation.Dispose();
                    }
                }
            }
            catch (AggregateException e)
            {
                Console.WriteLine("\nAggregateException thrown with the following inner exceptions:");
                // Display information about each exception. 
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        Console.WriteLine("   TaskCanceledException: Task {0}",
                                          ((TaskCanceledException)v).Task.Id);
                    else
                        Console.WriteLine("   Exception: {0}", v.GetType().Name);
                }
                Console.WriteLine();
            }
            finally
            {
                _myTaskList.Clear();
            }

            await _Semaphore.WaitAsync();
            try
            {
                // Do the search and return number of results as int
                var t = Task.Factory.StartNew<int>(funcToExecute,
                                                    tokenSource.Token,
                                                    TaskCreationOptions.LongRunning,
                                                    _myTaskScheduler);

                _myTaskList.Add(new TaskItem(t, tokenSource));

                await t;

                return t.Result;
            }
            finally
            {
                _Semaphore.Release();
            }
        }

        /// <summary>
        /// The bulk of the clean-up code is implemented here.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed == false)
            {
                if (disposing == true)
                {
                    try
                    {
                        for (int i = 0; i < _myTaskList.Count; i++)
                        {
                            if (_myTaskList[i].Cancellation != null)
                            {
                                _myTaskList[i].Cancellation.Cancel();
                                _myTaskList[i].Cancellation.Dispose();
                            }
                        }

                        _Semaphore.Dispose();
                    }
                    catch { }
                }

                _Disposed = true;
            }
        }
        #endregion methods

        #region private classes
        /// <summary>
        /// Implements a taskitem wich consists of a task and its <see cref="CancellationTokenSource"/>.
        /// </summary>
        private class TaskItem
        {
            /// <summary>
            /// Class constructor.
            /// </summary>
            /// <param name="taskToProcess"></param>
            /// <param name="cancellation"></param>
            public TaskItem(Task taskToProcess,
                            CancellationTokenSource cancellation)
                : this()
            {
                TaskToProcess = taskToProcess;
                Cancellation = cancellation;
            }

            /// <summary>
            /// Class constructor.
            /// </summary>
            protected TaskItem()
            {
                TaskToProcess = null;
                Cancellation = null;
            }

            /// <summary>
            /// Gets the task that shoulf be processed.
            /// </summary>
            public Task TaskToProcess { get; }

            /// <summary>
            /// Gets the <seealso cref="CancellationTokenSource"/> that can
            /// be used to cancel this task.
            /// </summary>
            public CancellationTokenSource Cancellation { get; }
        }
        #endregion private Classes
    }
}
