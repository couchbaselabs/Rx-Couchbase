using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.IO;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Base class for exceptions wrapping errors returned by Get requests via the Reactive engine.
    /// </summary>
    public abstract class CouchbaseGetException : CouchbaseReactiveException
    {
        private readonly ResponseStatus _status;

        /// <summary>
        /// The response status returned by the server when fulfilling the request.
        /// </summary>
        public ResponseStatus Status
        {
            get { return _status; }
        }

        internal CouchbaseGetException(ResponseStatus status, string message)
            : base(message)
        {
            _status = status;
        }

        internal CouchbaseGetException(ResponseStatus status, string message, Exception innerException)
            : base(message, innerException)
        {
            _status = status;
        }
    }
}
