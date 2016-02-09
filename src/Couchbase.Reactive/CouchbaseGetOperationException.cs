using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Exception wrapping an error returned by a Get request via the Reactive engine.
    /// </summary>
    class CouchbaseGetOperationException<T> : CouchbaseGetException
    {
        private readonly IOperationResult<T> _operationResult;

        /// <summary>
        /// <see cref="IOperationResult{T}"/> returned by the Couchbase SDK.
        /// </summary>
        public IOperationResult<T> OperationResult
        {
            get { return _operationResult; }
        }

        internal CouchbaseGetOperationException(IOperationResult<T> operationResult)
            : base(operationResult.Status, "Couchbase Get operation returned an error: " + operationResult.Message, operationResult.Exception)
        {
            if (operationResult == null)
            {
                throw new ArgumentNullException("operationResult");
            }

            _operationResult = operationResult;
        }
    }
}
