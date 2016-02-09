using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.N1QL;

namespace Couchbase.Reactive
{
    internal class N1QlQueryObservable<T> : IObservable<T>
    {
        private readonly IBucket _bucket;
        private readonly IQueryRequest _query;

        public N1QlQueryObservable(IBucket bucket, IQueryRequest query)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            _bucket = bucket;
            _query = query;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var disposed = false;

            try
            {
                var task = _bucket.QueryAsync<T>(_query);

                // Pass exceptions to the observer as an error
                task.ContinueWith(t =>
                {
                    if (!disposed)
                    {
                        observer.OnError((Exception) t.Exception ?? new InvalidOperationException("Unknown Error"));
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);

                // Pass canceled tasks as completion to the observer
                task.ContinueWith(t =>
                {
                    if (!disposed)
                    {
                        observer.OnCompleted();
                    }
                }, TaskContinuationOptions.OnlyOnCanceled);

                // Handle successful task completion
                task.ContinueWith(t =>
                {
                    if (!disposed)
                    {
                        if (t.Result.Success)
                        {
                            // On success, deliver all rows to the observer

                            foreach (var row in t.Result.Rows)
                            {
                                observer.OnNext(row);
                            }

                            observer.OnCompleted();
                        }
                        else
                        {
                            observer.OnError(new CouchbaseN1QlQueryException<T>(t.Result));
                        }
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return Disposable.Create(() => disposed = true);
        }
    }
}
