using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Couchbase.N1QL;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Base class for exceptions wrapping errors returned by N1QL query requests via the Reactive engine.
    /// </summary>
    public abstract class CouchbaseN1QlQueryException : CouchbaseReactiveException
    {
        /// <summary>
        /// Gets the status of the request.
        /// </summary>
        public abstract QueryStatus Status { get; }

        /// <summary>
        /// Gets a list of zero or more error objects.
        /// </summary>
        public abstract IList<Error> Errors { get; }

        internal CouchbaseN1QlQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
