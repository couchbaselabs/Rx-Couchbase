using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Base class for all exceptions related to Rx-Couchbase.
    /// </summary>
    public abstract class CouchbaseReactiveException : ApplicationException
    {
        internal CouchbaseReactiveException(string message)
            : base(message)
        {
        }

        internal CouchbaseReactiveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
