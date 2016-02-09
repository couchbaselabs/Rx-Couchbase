using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Couchbase.Reactive.UnitTests.Utils
{
    class AssertThrowsObserver<TElement> : IObserver<TElement>
    {
        private readonly ManualResetEvent _event = new ManualResetEvent(false);
        private Exception _error = null;

        public void OnNext(TElement value)
        {
        }

        public void OnError(Exception error)
        {
            _error = error;
            _event.Set();
        }

        public void OnCompleted()
        {
            _event.Set();
        }

        public TException Assert<TException>() where TException : Exception
        {
            return Assert< TException>(Timeout.InfiniteTimeSpan);
        }

        public TException Assert<TException>(TimeSpan timeout) where TException : Exception
        {
            var fired = _event.WaitOne(timeout, true);

            if (!fired)
            {
                throw new AssertionException("AssertThrowsException timed out waiting for a result.");
            }

            if (_error == null)
            {
                throw new AssertionException(
                    string.Format("AssertThrowsException expected {0}, but did not receive an exception.", typeof(TException).Name));
            }

            var typedError = _error as TException;
            if (typedError == null)
            {
                throw new AssertionException(
                    string.Format("AssertThrowsException expected {0}, but received {1}.", typeof(TException).Name, _error.GetType().Name), _error);
            }

            return typedError;
        }
    }
}
