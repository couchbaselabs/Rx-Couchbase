using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using System.Collections.Concurrent;
using Couchbase.IO;
using Couchbase.N1QL;
using Couchbase.Views;

namespace Couchbase.Reactive
{
    public static class BucketExtensions
    {
        public static IObservable<KeyValuePair<string, T>> GetObservable<T>(this IBucket bucket, string key)
        {
            return Observable.FromAsync(() => bucket.GetAsync<T>(key))
                .Where(p => p.Status != ResponseStatus.KeyNotFound)
                .Select(p =>
                {
                    CheckResultForError(p);

                    return new KeyValuePair<string, T>(key, p.Value);
                });
        }

        public static IObservable<KeyValuePair<string, T>> GetObservable<T>(this IBucket bucket, IList<string> keys)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            // Create an IObservable to get the IOperationResult for all keys
            IObservable<KeyValuePair<string, IOperationResult<T>>> operationObserver =
                new MultiGetObservable<string, KeyValuePair<string, IOperationResult<T>>>(
                    keys, key => new KeyValuePair<string, IOperationResult<T>>(key, bucket.Get<T>(key)));

            // Filter out KeyNotFound results
            operationObserver = operationObserver.Where(p => p.Value.Status != ResponseStatus.KeyNotFound);

            return operationObserver.Select(p =>
            {
                CheckResultForError(p.Value);

                return new KeyValuePair<string, T>(p.Key, p.Value.Value);
            });
        }

        public static IObservable<KeyValuePair<string, T>> GetAndTouchObservable<T>(this IBucket bucket, string key, TimeSpan expiration)
        {
            return Observable.FromAsync(() => bucket.GetAndTouchAsync<T>(key, expiration))
                .Where(p => p.Status != ResponseStatus.KeyNotFound)
                .Select(p =>
                {
                    CheckResultForError(p);

                    return new KeyValuePair<string, T>(key, p.Value);
                });
        }

        public static IObservable<IDocument<T>> GetDocumentObservable<T>(this IBucket bucket, string id)
        {
            return Observable.FromAsync(() => bucket.GetDocumentAsync<T>(id))
                .Where(p => p.Status != ResponseStatus.KeyNotFound)
                .Select(p =>
                {
                    CheckResultForError(p);

                    return p.Document;
                });
        }

        public static IObservable<IDocument<T>> GetAndTouchDocumentObservable<T>(this IBucket bucket, string id, TimeSpan expiration)
        {
            return Observable.FromAsync(() => bucket.GetAndTouchDocumentAsync<T>(id, expiration))
               .Where(p => p.Status != ResponseStatus.KeyNotFound)
               .Select(p =>
               {
                   CheckResultForError(p);

                   return p.Document;
               });
        }

        public static IObservable<IDocument<T>> GetDocumentObservable<T>(this IBucket bucket, IList<string> ids)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }

            // Create an IObservable to get the IOperationResult for all keys
            IObservable<KeyValuePair<string, IDocumentResult<T>>> operationObserver =
                new MultiGetObservable<string, KeyValuePair<string, IDocumentResult<T>>>(
                    ids, id => new KeyValuePair<string, IDocumentResult<T>>(id, bucket.GetDocument<T>(id)));

            // Filter out KeyNotFound results
            operationObserver = operationObserver.Where(p => p.Value.Status != ResponseStatus.KeyNotFound);

            return operationObserver.Select(p =>
            {
                CheckResultForError(p.Value);

                return p.Value.Document;
            });
        }

        public static IObservable<ViewRow<T>> QueryObservable<T>(this IBucket bucket, IViewQueryable query)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return new ViewQueryObservable<T>(bucket, query);
        }

        public static IObservable<T> QueryObservable<T>(this IBucket bucket, string query)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return bucket.QueryObservable<T>(new QueryRequest(query));
        }

        public static IObservable<T> QueryObservable<T>(this IBucket bucket, IQueryRequest query)
        {
            if (bucket == null)
            {
                throw new ArgumentNullException("bucket");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return new N1QlQueryObservable<T>(bucket, query);
        }

        #region Helpers

        private static void CheckResultForError<T>(IOperationResult<T> result)
        {
            if (!result.Success)
            {
                throw new CouchbaseGetOperationException<T>(result);
            }
        }

        private static void CheckResultForError<T>(IDocumentResult<T> result)
        {
            if (!result.Success)
            {
                throw new CouchbaseGetDocumentException<T>(result);
            }
        }

        #endregion
    }
}
