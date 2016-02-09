using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.N1QL;
using Couchbase.Reactive.UnitTests.Documents;
using Couchbase.Reactive.UnitTests.Utils;
using Couchbase.Views;
using Moq;
using NUnit.Framework;

namespace Couchbase.Reactive.UnitTests
{
    [TestFixture]
    public class BucketExtensionTests : TestBase
    {
        #region GetObservable

        [Test]
        public void GetObservable_SingleDocument_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAsync<Beer>(It.IsAny<string>())).Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetObservable_SingleDocument_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            operationResult.Setup(m => m.Message).Returns("error message");
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetObservable_SingleDocument_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetObservable_SingleDocument_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            // Act

            var results = bucket.Object.GetObservable<Beer>("key").ToEnumerable();

            // Assert

            Assert.IsEmpty(results);
        }

        [Test]
        public void GetObservable_MultiDocument_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Get<Beer>(It.IsAny<string>())).Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetObservable_MultiDocument_OperationReturnsException_CallsOnErrorWithException()
        {
            var keys = new[] { "key1", "key2", "key3" };

            // Arrange

            var successOperationResult = new Mock<IOperationResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Value).Returns(new Beer { Name = "Beer" });

            var errorOperationResult = new Mock<IOperationResult<Beer>>();
            errorOperationResult.Setup(m => m.Success).Returns(false);
            errorOperationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            errorOperationResult.Setup(m => m.Message).Returns("error message");
            errorOperationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Get<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[2])).Returns(errorOperationResult.Object);

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(errorOperationResult.Object.Message));
            Assert.AreEqual(errorOperationResult.Object.Status, ex.Status);
            Assert.AreEqual(errorOperationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetObservable_MultiDocument_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var successOperationResult = new Mock<IOperationResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Value).Returns(new Beer { Name = "Beer" });

            var errorOperationResult = new Mock<IOperationResult<Beer>>();
            errorOperationResult.Setup(m => m.Success).Returns(false);
            errorOperationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            errorOperationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Get<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[2])).Returns(errorOperationResult.Object);

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(errorOperationResult.Object.Message));
            Assert.AreEqual(errorOperationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetObservable_MultiDocument_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var successOperationResult = new Mock<IOperationResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Value).Returns(new Beer { Name = "Beer" });

            var notFoundOperationResult = new Mock<IOperationResult<Beer>>();
            notFoundOperationResult.Setup(m => m.Success).Returns(false);
            notFoundOperationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            notFoundOperationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.Get<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.Get<Beer>(keys[2])).Returns(notFoundOperationResult.Object);


            // Act

            var results = bucket.Object.GetObservable<Beer>(keys).ToEnumerable();

            // Assert

            Assert.AreEqual(2, results.Count());
        }

        #endregion

        #region GetAndTouchObservable

        [Test]
        public void GetAndTouchObservable_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetAndTouchObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetAndTouchObservable_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            operationResult.Setup(m => m.Message).Returns("error message");
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetAndTouchObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetAndTouchObservable_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<KeyValuePair<string, Beer>>();

            // Act

            bucket.Object.GetAndTouchObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetAndTouchObservable_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var operationResult = new Mock<IOperationResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            // Act

            var results = bucket.Object.GetAndTouchObservable<Beer>("key", TimeSpan.Zero).ToEnumerable();

            // Assert

            Assert.IsEmpty(results);
        }

        #endregion

        #region GetDocumentObservable

        [Test]
        public void GetDocumentObservable_SingleDocument_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocumentAsync<Beer>(It.IsAny<string>())).Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetDocumentObservable_SingleDocument_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            operationResult.Setup(m => m.Message).Returns("error message");
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocumentAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetDocumentObservable_SingleDocument_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocumentAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>("key").Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetDocumentObservable_SingleDocument_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocumentAsync<Beer>(It.IsAny<string>())).Returns(Task.FromResult(operationResult.Object));

            // Act

            var results = bucket.Object.GetDocumentObservable<Beer>("key").ToEnumerable();

            // Assert

            Assert.IsEmpty(results);
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocument<Beer>(It.IsAny<string>())).Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_OperationReturnsException_CallsOnErrorWithException()
        {
            var keys = new[] { "key1", "key2", "key3" };

            // Arrange

            var successOperationResult = new Mock<IDocumentResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Content).Returns(new Beer { Name = "Beer" });

            var errorOperationResult = new Mock<IDocumentResult<Beer>>();
            errorOperationResult.Setup(m => m.Success).Returns(false);
            errorOperationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            errorOperationResult.Setup(m => m.Message).Returns("error message");
            errorOperationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocument<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[2])).Returns(errorOperationResult.Object);

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(errorOperationResult.Object.Message));
            Assert.AreEqual(errorOperationResult.Object.Status, ex.Status);
            Assert.AreEqual(errorOperationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var successOperationResult = new Mock<IDocumentResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Content).Returns(new Beer { Name = "Beer" });

            var errorOperationResult = new Mock<IDocumentResult<Beer>>();
            errorOperationResult.Setup(m => m.Success).Returns(false);
            errorOperationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            errorOperationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocument<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[2])).Returns(errorOperationResult.Object);

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetDocumentObservable<Beer>(keys).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(errorOperationResult.Object.Message));
            Assert.AreEqual(errorOperationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var keys = new[] { "key1", "key2", "key3" };

            var successOperationResult = new Mock<IDocumentResult<Beer>>();
            successOperationResult.Setup(m => m.Success).Returns(true);
            successOperationResult.Setup(m => m.Status).Returns(ResponseStatus.Success);
            successOperationResult.Setup(m => m.Content).Returns(new Beer { Name = "Beer" });

            var notFoundOperationResult = new Mock<IDocumentResult<Beer>>();
            notFoundOperationResult.Setup(m => m.Success).Returns(false);
            notFoundOperationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            notFoundOperationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetDocument<Beer>(keys[0])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[1])).Returns(successOperationResult.Object);
            bucket.Setup(m => m.GetDocument<Beer>(keys[2])).Returns(notFoundOperationResult.Object);


            // Act

            var results = bucket.Object.GetDocumentObservable<Beer>(keys).ToEnumerable();

            // Assert

            Assert.AreEqual(2, results.Count());
        }

        #endregion

        #region GetAndTouchDocumentObservable

        [Test]
        public void GetAndTouchDocumentObservable_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchDocumentAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetAndTouchDocumentObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void GetAndTouchDocumentObservable_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.InternalError);
            operationResult.Setup(m => m.Message).Returns("error message");
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchDocumentAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetAndTouchDocumentObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void GetAndTouchDocumentObservable_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.AuthenticationError);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchDocumentAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<IDocument<Beer>>();

            // Act

            bucket.Object.GetAndTouchDocumentObservable<Beer>("key", TimeSpan.Zero).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseGetException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
        }

        [Test]
        public void GetAndTouchDocumentObservable_OperationReturnsKeyNotFound_ReturnsEmptyResult()
        {
            // Arrange

            var operationResult = new Mock<IDocumentResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(ResponseStatus.KeyNotFound);
            operationResult.Setup(m => m.Message).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.GetAndTouchDocumentAsync<Beer>(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(operationResult.Object));

            // Act

            var results = bucket.Object.GetAndTouchDocumentObservable<Beer>("key", TimeSpan.Zero).ToEnumerable();

            // Assert

            Assert.IsEmpty(results);
        }

        #endregion

        #region QueryObservable

        [Test]
        public void QueryObservable_ViewQuery_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IViewQueryable>()))
                .Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<ViewRow<Beer>>();

            // Act

            bucket.Object.QueryObservable<Beer>(new ViewQuery()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void QueryObservable_ViewQuery_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IViewResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.StatusCode).Returns(HttpStatusCode.InternalServerError);
            operationResult.Setup(m => m.Error).Returns("error message");
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IViewQueryable>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<ViewRow<Beer>>();

            // Act

            bucket.Object.QueryObservable<Beer>(new ViewQuery()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseViewQueryException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Error));
            Assert.AreEqual(operationResult.Object.Error, ex.Error);
            Assert.AreEqual(operationResult.Object.StatusCode, ex.StatusCode);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void QueryObservable_ViewQuery_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IViewResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.StatusCode).Returns(HttpStatusCode.InternalServerError);
            operationResult.Setup(m => m.Error).Returns("error message");

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IViewQueryable>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<ViewRow<Beer>>();

            // Act

            bucket.Object.QueryObservable<Beer>(new ViewQuery()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseViewQueryException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Error));
            Assert.AreEqual(operationResult.Object.Error, ex.Error);
            Assert.AreEqual(operationResult.Object.StatusCode, ex.StatusCode);
        }

        [Test]
        public void QueryObservable_QueryRequest_OperationThrowsException_CallsOnErrorWithException()
        {
            // Arrange

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IQueryRequest>()))
                .Throws(new ObjectDisposedException("test"));

            var observer = new AssertThrowsObserver<Beer>();

            // Act

            bucket.Object.QueryObservable<Beer>(new QueryRequest()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<ObjectDisposedException>();
            Assert.AreEqual("test", ex.ObjectName);
        }

        [Test]
        public void QueryObservable_QueryRequest_OperationReturnsException_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IQueryResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(QueryStatus.Fatal);
            operationResult.Setup(m => m.Exception).Returns(new ApplicationException("exception message"));

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IQueryRequest>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<Beer>();

            // Act

            bucket.Object.QueryObservable<Beer>(new QueryRequest()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseN1QlQueryException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Exception.Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
            Assert.AreEqual(operationResult.Object.Exception, ex.InnerException);
        }

        [Test]
        public void QueryObservable_QueryRequest_OperationReturnsError_CallsOnErrorWithException()
        {
            // Arrange

            var operationResult = new Mock<IQueryResult<Beer>>();
            operationResult.Setup(m => m.Success).Returns(false);
            operationResult.Setup(m => m.Status).Returns(QueryStatus.Errors);
            operationResult.Setup(m => m.Errors).Returns(new List<Error> { new Error { Message = "error message" } });

            var bucket = new Mock<IBucket>();
            bucket.Setup(m => m.QueryAsync<Beer>(It.IsAny<IQueryRequest>()))
                .Returns(Task.FromResult(operationResult.Object));

            var observer = new AssertThrowsObserver<Beer>();

            // Act

            bucket.Object.QueryObservable<Beer>(new QueryRequest()).Subscribe(observer);

            // Assert

            var ex = observer.Assert<CouchbaseN1QlQueryException>();
            Assert.True(ex.Message.Contains(operationResult.Object.Errors.First().Message));
            Assert.AreEqual(operationResult.Object.Status, ex.Status);
        }

        #endregion
    }
}
