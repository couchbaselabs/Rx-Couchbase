using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Base class for exceptions wrapping errors returned by view requests via the Reactive engine.
    /// </summary>
    public abstract class CouchbaseViewQueryException : CouchbaseReactiveException
    {
        /// <summary>
        /// The HTTP Status Code for the request.
        /// </summary>
        public abstract HttpStatusCode StatusCode { get; }

        /// <summary>
        /// A View engine specific error message if one occurred.
        /// </summary>
        public abstract string Error { get; }

        internal CouchbaseViewQueryException(string message)
            : base(message)
        {
        }

        internal CouchbaseViewQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
