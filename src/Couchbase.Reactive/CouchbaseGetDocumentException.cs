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
    class CouchbaseGetDocumentException<T> : CouchbaseGetException
    {
        private readonly IDocumentResult<T> _documentResult;

        /// <summary>
        /// <see cref="IDocumentResult{T}"/> returned by the Couchbase SDK.
        /// </summary>
        public IDocumentResult<T> DocumentResult
        {
            get { return _documentResult; }
        }

        internal CouchbaseGetDocumentException(IDocumentResult<T> documentResult)
            : base(documentResult.Status, "Couchbase Get operation returned an error: " + documentResult.Message, documentResult.Exception)
        {
            if (documentResult == null)
            {
                throw new ArgumentNullException("documentResult");
            }

            _documentResult = documentResult;
        }
    }
}
