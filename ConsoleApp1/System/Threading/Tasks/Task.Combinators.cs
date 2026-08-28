using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public partial class Task
    {
        public static Task FromCanceled(CancellationToken cancellationToken)
        {
            Task task = new Task();
            task.TrySetCanceled();
            return task;
        }

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
            Task<TResult> task = new Task<TResult>();
            task.TrySetCanceled();
            return task;
        }

        public static Task Run(Action action)
        {
            if (action == null)
                return FromException(new ArgumentNullException("The action cannot be null."));
            TaskCompletionSource completion = new TaskCompletionSource();
            QueueWork(() =>
            {
                try
                {
                    action();
                    completion.TrySetResult();
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            });
            return completion.Task;
        }

        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            if (function == null)
                return FromException<TResult>(new ArgumentNullException("The function cannot be null."));
            TaskCompletionSource<TResult> completion = new TaskCompletionSource<TResult>();
            QueueWork(() =>
            {
                try
                {
                    completion.TrySetResult(function());
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            });
            return completion.Task;
        }

        public static Task Run(Func<Task> function)
        {
            if (function == null)
                return FromException(new ArgumentNullException("The function cannot be null."));
            TaskCompletionSource completion = new TaskCompletionSource();
            QueueWork(() =>
            {
                try
                {
                    Task inner = function();
                    if (inner == null)
                    {
                        completion.TrySetException(new InvalidOperationException("The task function returned null."));
                        return;
                    }
                    inner.AddContinuation(() => Complete(completion, inner));
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            });
            return completion.Task;
        }

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            if (function == null)
                return FromException<TResult>(new ArgumentNullException("The function cannot be null."));
            TaskCompletionSource<TResult> completion = new TaskCompletionSource<TResult>();
            QueueWork(() =>
            {
                try
                {
                    Task<TResult> inner = function();
                    if (inner == null)
                    {
                        completion.TrySetException(new InvalidOperationException("The task function returned null."));
                        return;
                    }
                    inner.AddContinuation(() => Complete(completion, inner));
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            });
            return completion.Task;
        }

        public static Task WhenAll(params Task[] tasks) => WhenAll((IEnumerable<Task>)tasks);

        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                return FromException(new ArgumentNullException("The task collection cannot be null."));
            List<Task> list = new List<Task>();
            foreach (Task task in tasks)
            {
                if (task == null)
                    return FromException(new ArgumentException("The task collection cannot contain null."));
                list.Add(task);
            }
            if (list.Count == 0)
                return CompletedTask;

            TaskCompletionSource completion = new TaskCompletionSource();
            int remaining = list.Count;
            bool canceled = false;
            Exception[] exceptions = new Exception[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                Task task = list[i];
                int index = i;
                task.AddContinuation(() =>
                {
                    if (task.IsFaulted)
                        exceptions[index] = task.Exception;
                    else if (task.IsCanceled)
                        canceled = true;
                    if (Interlocked.Decrement(ref remaining) == 0)
                        CompleteWhenAll(completion, exceptions, canceled);
                });
            }
            return completion.Task;
        }

        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
            => WhenAll((IEnumerable<Task<TResult>>)tasks);

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            if (tasks == null)
                return FromException<TResult[]>(new ArgumentNullException("The task collection cannot be null."));
            List<Task<TResult>> list = new List<Task<TResult>>();
            foreach (Task<TResult> task in tasks)
            {
                if (task == null)
                    return FromException<TResult[]>(new ArgumentException("The task collection cannot contain null."));
                list.Add(task);
            }
            if (list.Count == 0)
                return FromResult(new TResult[0]);

            TaskCompletionSource<TResult[]> completion = new TaskCompletionSource<TResult[]>();
            TResult[] results = new TResult[list.Count];
            Exception[] exceptions = new Exception[list.Count];
            int remaining = list.Count;
            bool canceled = false;
            for (int i = 0; i < list.Count; i++)
            {
                Task<TResult> task = list[i];
                int index = i;
                task.AddContinuation(() =>
                {
                    if (task.IsFaulted)
                        exceptions[index] = task.Exception;
                    else if (task.IsCanceled)
                        canceled = true;
                    else
                        results[index] = task.Result;
                    if (Interlocked.Decrement(ref remaining) == 0)
                        CompleteWhenAll(completion, results, exceptions, canceled);
                });
            }
            return completion.Task;
        }

        public static Task<Task> WhenAny(params Task[] tasks) => WhenAny((IEnumerable<Task>)tasks);

        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                return FromException<Task>(new ArgumentNullException("The task collection cannot be null."));
            List<Task> list = new List<Task>();
            foreach (Task task in tasks)
            {
                if (task == null)
                    return FromException<Task>(new ArgumentException("The task collection cannot contain null."));
                list.Add(task);
            }
            if (list.Count == 0)
                return FromException<Task>(new ArgumentException("At least one task is required."));

            TaskCompletionSource<Task> completion = new TaskCompletionSource<Task>();
            int completed = 0;
            foreach (Task task in list)
                task.AddContinuation(() =>
                {
                    if (Interlocked.CompareExchange(ref completed, 1, 0) == 0)
                        completion.TrySetResult(task);
                });
            return completion.Task;
        }

        public static Task Delay(TimeSpan delay)
            => Delay((int)delay.TotalMilliseconds);

        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return FromCanceled(cancellationToken);
            if (!cancellationToken.CanBeCanceled)
                return Delay(millisecondsDelay);

            TaskCompletionSource completion = new TaskCompletionSource();
            Task timer = Delay(millisecondsDelay);
            CancellationTokenRegistration registration = cancellationToken.Register(() => completion.TrySetCanceled());
            timer.AddContinuation(() =>
            {
                registration.Dispose();
                if (timer.IsFaulted)
                    completion.TrySetException(timer.Exception);
                else if (!timer.IsCanceled)
                    completion.TrySetResult();
            });
            return completion.Task;
        }

        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
            => Delay((int)delay.TotalMilliseconds, cancellationToken);

        private static void QueueWork(Action action)
        {
            TaskScheduler.Register(new WorkItem(action));
        }

        private static void Complete(TaskCompletionSource completion, Task inner)
        {
            if (inner.IsFaulted)
                completion.TrySetException(inner.Exception);
            else if (inner.IsCanceled)
                completion.TrySetCanceled();
            else
                completion.TrySetResult();
        }

        private static void Complete<TResult>(TaskCompletionSource<TResult> completion, Task<TResult> inner)
        {
            if (inner.IsFaulted)
                completion.TrySetException(inner.Exception);
            else if (inner.IsCanceled)
                completion.TrySetCanceled();
            else
                completion.TrySetResult(inner.Result);
        }

        private static void CompleteWhenAll(TaskCompletionSource completion, Exception[] exceptions, bool canceled)
        {
            List<Exception> failures = new List<Exception>();
            for (int i = 0; i < exceptions.Length; i++)
                if (exceptions[i] != null)
                    failures.Add(exceptions[i]);
            if (failures.Count != 0)
                completion.TrySetException(new AggregateException(failures.ToArray()));
            else if (canceled)
                completion.TrySetCanceled();
            else
                completion.TrySetResult();
        }

        private static void CompleteWhenAll<TResult>(TaskCompletionSource<TResult[]> completion,
            TResult[] results, Exception[] exceptions, bool canceled)
        {
            List<Exception> failures = new List<Exception>();
            for (int i = 0; i < exceptions.Length; i++)
                if (exceptions[i] != null)
                    failures.Add(exceptions[i]);
            if (failures.Count != 0)
                completion.TrySetException(new AggregateException(failures.ToArray()));
            else if (canceled)
                completion.TrySetCanceled();
            else
                completion.TrySetResult(results);
        }

        private sealed class WorkItem : TaskPoller
        {
            private readonly Action _action;

            internal WorkItem(Action action) => _action = action;

            internal override void Poll()
            {
                TaskScheduler.Unregister(this);
                _action();
            }
        }
    }
}
