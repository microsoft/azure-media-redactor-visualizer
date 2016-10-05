using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureMediaRedactor.Extensions.FFmpeg
{
    public class AsyncLock
    {
        class Releaser : IDisposable
        {
            private AsyncLock _owner;

            public Releaser(AsyncLock owner)
            {
                _owner = owner;
            }

            #region IDisposable Support
            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                _owner.Exit();
            }
            #endregion
        }

        private object _mutex;
        private bool _isLocked;
        private Task<IDisposable> _releaser;
        private Queue<TaskCompletionSource<IDisposable>> _waiters;

        public AsyncLock()
        {
            _mutex = new object();
            _releaser = Task.FromResult<IDisposable>(new Releaser(this));
            _waiters = new Queue<TaskCompletionSource<IDisposable>>();
        }

        public Task<IDisposable> AcquireLockAsync(CancellationToken cancellationToken)
        {
            lock (_mutex)
            {
                if (!_isLocked)
                {
                    _isLocked = true;
                    return _releaser;
                }
                else
                {
                    TaskCompletionSource<IDisposable> taskCompletionSource = new TaskCompletionSource<IDisposable>();
                    cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());
                    _waiters.Enqueue(taskCompletionSource);
                    return taskCompletionSource.Task;
                }
            }
        }

        private void Exit()
        {
            TaskCompletionSource<IDisposable> toRelease = null;

            lock (_mutex)
            {
                if (!_isLocked)
                {
                    throw new InvalidOperationException();
                }

                while (_waiters.Count != 0)
                {
                    var waiter = _waiters.Dequeue();
                    if (!waiter.Task.IsCanceled)
                    {
                        toRelease = waiter;
                        break;
                    }
                }

                if (toRelease == null)
                {
                    _isLocked = false;
                }
            }

            if (toRelease != null)
            {
                toRelease.SetResult(new Releaser(this));
            }
        }
    }
}
