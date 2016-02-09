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
    /// Exception wrapping an error returned by a N1QL query request via the Reactive engine.
    /// </summary>
    public class CouchbaseN1QlQueryException<T> : CouchbaseN1QlQueryException
    {
        private readonly IQueryResult<T> _queryResult;

        public IQueryResult<T> QueryResult
        {
            get { return _queryResult; }
        }

        public override QueryStatus Status
        {
            get { return _queryResult.Status; }
        }

        public override IList<Error> Errors
        {
            get { return _queryResult.Errors; }
        }

        internal CouchbaseN1QlQueryException(IQueryResult<T> queryResult)
            : base(ExtractErrorMessage(queryResult), queryResult.Exception)
        {
            if (queryResult == null)
            {
                throw new ArgumentNullException("queryResult");
            }

            _queryResult = queryResult;
        }

        private static string ExtractErrorMessage(IQueryResult<T> queryResult)
        {
            const string messageWrapper = "Couchbase N1QL query returned an error: {0}";

            if (queryResult.Errors != null)
            {
                if (queryResult.Errors.Count == 1)
                {
                    // Only one error, return that message

                    return string.Format(messageWrapper, queryResult.Errors.First().Message);
                }
                else if (queryResult.Errors.Count > 1)
                {
                    // More than one error, so return a generic message

                    return string.Format(messageWrapper, "see Errors property for details.");
                }
            }

            if (queryResult.Exception != null)
            {
                // Use the message from the exception

                return string.Format(messageWrapper, queryResult.Exception.Message);
            }

            // No error message available
            return string.Format(messageWrapper, "Unknown error.");
        }
    }
}
