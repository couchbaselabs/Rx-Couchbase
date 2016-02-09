using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Views;

namespace Couchbase.Reactive
{
    /// <summary>
    /// Exception wrapping an error returned by a view request via the Reactive engine.
    /// </summary>
    public class CouchbaseViewQueryException<T> : CouchbaseViewQueryException
    {
        private readonly IViewResult<T> _viewResult;

        /// <summary>
        /// Result of the view request.
        /// </summary>
        public IViewResult<T> ViewResult
        {
            get { return _viewResult; }
        }

        public override HttpStatusCode StatusCode
        {
            get { return _viewResult.StatusCode; }
        }

        public override string Error
        {
            get { return _viewResult.Error; }
        }

        internal CouchbaseViewQueryException(IViewResult<T> viewResult)
            : base("Couchbase view query returned an error: " + (viewResult.Error ?? "Unknown"),
                  viewResult.Exception)
        {
            if (viewResult == null)
            {
                throw new ArgumentNullException("viewResult");
            }

            _viewResult = viewResult;
        }
    }
}
