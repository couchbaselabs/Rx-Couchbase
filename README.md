## Couchbase Reactive Extensions

NOTE: Not supported under Couchbase Enterprise Support Subscriptions!

[![Join the chat at https://gitter.im/couchbaselabs/Rx-Couchbase](https://badges.gitter.im/couchbaselabs/Rx-Couchbase.svg)](https://gitter.im/couchbaselabs/Rx-Couchbase?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Rx-Couchbase is an experimental library designed to extend the [Couchbase .Net SDK](https://github.com/couchbase/couchbase-net-client) to provide a Reactive implementation of bucket get and query operations.

The intention is to have this adopted as a part the Couchbase .Net SDK, or alternatively adopted as a supported Nuget package which extends the SDK.  This decision may vary based on the need to include a dependency on the Reactive Extensions in the Couchbase .Net SDK.

##Getting Documents

Documents can be requested using bucket.GetObservable or bucket.GetDocumentObservable.  Both support getting a single document or a list of documents by their ID.

GetObservable returns an IObservable<KeyValuePair<string, IOperationResult<T>>>, where T is the type of document you are getting.  If you subscribe to this IObservable instance, it will initiate an asynchronous query get the the document or documents, and you will be pushed a KeyValuePair for each requested document.  The key is the document key you requested, and the IOperationResult<T> has status/error information, and the actual document if the Get operation was successful.

GetDocumentObservable functions in the same manner as GetObservable, but the Value of the KeyValuePair is an IDocumentResult<T> instead.

When performing a multiple Get operation using a list of keys, you may optionally provide a ParallelOptions object which controls how the Get operations are run in parallel.  By default, the MaxDegreeOfParallelism will be the MaxSize from the Couchbase connection pool for the bucket.

Note that when performing multiple Get operations using a list of keys, the return order of the documents is not deterministic.

##Querying Views

To query a view, you may use bucket.QueryObservable<T>(viewQuery).  The view query may be created using bucket.CreateQuery.

The returned IObservable will execute the view query once it is subscribed.  It will then push each ViewRow<T> result to the subscriber.  Because of the serialized nature of the view, these results will be pushed in the order received.

If the view query returns an error, it will be pushed to the subscriber via an OnError call.  If the view query returned an exception, this will be pushed directly.  If the view query simply returned an error response, then the error will be wrapped in a CouchbaseViewQueryException.

##Querying With N1QL

To run a N1QL query, you may use bucket.QueryObservable<T>(queryRequest).

The returned IObservable will execute the N1QL query once it is subscribed.  It will then push each result row to the subscriber.  Because of the serialized nature of the query, these results will be pushed in the order received.

If the query returns an error, it will be pushed to the subscriber via an OnError call.  If the query returned an exception, this will be pushed directly.  If the query simply returned an error response, then the error will be wrapped in a CouchbaseN1QlQueryException.

##Asynchronous Handling

All queries are executed asynchronously, and the results are delivered to the subscriber asynchronously.  Developers should be sure that the asynchronous nature of the subscription is handled in their design.

However, view and N1QL queries don't currently deliver streamed results.  The entire asynchronous operation to the Couchbase server is completed, after which the result set which was received is streamed to the observer.
