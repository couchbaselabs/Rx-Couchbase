using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Reactive.IntegrationTests.Documents;
using NUnit.Framework;
using System.Configuration;
using Couchbase.Reactive.IntegrationTests.Utils;

namespace Couchbase.Reactive.IntegrationTests
{
    [TestFixture]
    public class BucketExtensionsTests
    {

        #region GetObservable

        [Test]
        public void GetObservable_SingleDocument_KeyExists_GetsDocument()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    const string key = "beer-test";

                    var testDocument = new Beer()
                    {
                        Type = "beer",
                        Name = "test"
                    };

                    try
                    {
                        // Create the document for the test
                        bucket.Upsert(new Document<Beer>
                        {
                            Id = key,
                            Content = testDocument
                        });

                        var observable = bucket.GetObservable<Beer>(key);

                        var document = observable.ToEnumerable().Single();

                        Assert.AreEqual(key, document.Key);
                        Assert.AreEqual(testDocument.Name, document.Value.Name);
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(key);
                    }
                }
            }
        }

        [Test]
        public void GetObservable_SingleDocument_KeyDoesNotExist_NoResult()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    const string key = "beer-test";

                    // Ensure the key doesn't exist first
                    bucket.Remove(key);

                    var observable = bucket.GetObservable<Beer>(key);

                    Assert.False(observable.ToEnumerable().Any());
                }
            }
        }

        [Test]
        public void GetObservable_MultiDocument_GetsDocuments()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var testDocuments = new List<Document<Beer>>()
                    {
                        new Document<Beer>()
                        {
                            Id = "test-beer",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer2",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test2"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer3",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test3"
                            }
                        }
                    };

                    try
                    {
                        // Create the documents for the test
                        testDocuments.ForEach(document =>
                        {
                            bucket.Upsert(document);
                        });

                        var observable = bucket.GetObservable<Beer>(testDocuments.Select(p => p.Id).ToArray());

                        var documents = observable.ToEnumerable().OrderBy(p => p.Key).ToList();

                        Assert.AreEqual(testDocuments.Count, documents.Count);

                        var zipped = documents.Zip(testDocuments, (result, test) => new { Result = result, Test = test }).ToList();

                        zipped.ForEach(zip =>
                        {
                            Assert.AreEqual(zip.Test.Id, zip.Result.Key);
                            Assert.AreEqual(zip.Test.Content.Name, zip.Result.Value.Name);
                        });
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(testDocuments.Select(p => p.Id).ToList());
                    }
                }
            }
        }

        [Test]
        public void GetObservable_MultiDocument_SkipsMissingKeys()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var testDocuments = new List<Document<Beer>>()
                    {
                        new Document<Beer>()
                        {
                            Id = "test-beer",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer2",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test2"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer3",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test3"
                            }
                        }
                    };

                    try
                    {
                        // Create the documents for the test
                        testDocuments.Take(2).ToList().ForEach(document =>
                        {
                            bucket.Upsert(document);
                        });

                        bucket.Remove(testDocuments.Skip(2).Select(p => p.Id).ToList());

                        var observable = bucket.GetObservable<Beer>(testDocuments.Select(p => p.Id).ToArray());

                        var documents = observable.ToEnumerable().ToList();

                        Assert.AreEqual(2, documents.Count);
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(testDocuments.Select(p => p.Id).ToList());
                    }
                }
            }
        }

        #endregion

        #region GetDocumentObservable

        [Test]
        public void GetDocumentObservable_SingleDocument_KeyExists_GetsDocument()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var testDocument = new Document<Beer>()
                    {
                        Id = "beer-test",
                        Content = new Beer()
                        {
                            Type = "beer",
                            Name = "test"
                        }
                    };

                    try
                    {
                        // Create the document for the test
                        bucket.Upsert(testDocument);

                        var observable = bucket.GetDocumentObservable<Beer>(testDocument.Id);

                        var document = observable.ToEnumerable().Single();

                        Assert.AreEqual(testDocument.Id, document.Id);
                        Assert.AreEqual(testDocument.Content.Name, document.Content.Name);
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(testDocument.Id);
                    }
                }
            }
        }

        [Test]
        public void GetDocumentObservable_SingleDocument_KeyDoesNotExist_NoResult()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    const string key = "beer-test";

                    // Ensure the key doesn't exist first
                    bucket.Remove(key);

                    var observable = bucket.GetDocumentObservable<Beer>(key);

                    Assert.False(observable.ToEnumerable().Any());
                }
            }
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_GetsDocuments()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var testDocuments = new List<Document<Beer>>()
                    {
                        new Document<Beer>()
                        {
                            Id = "test-beer",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer2",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test2"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer3",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test3"
                            }
                        }
                    };

                    try
                    {
                        // Create the documents for the test
                        testDocuments.ForEach(document =>
                        {
                            bucket.Upsert(document);
                        });

                        var observable = bucket.GetDocumentObservable<Beer>(testDocuments.Select(p => p.Id).ToArray());

                        var documents = observable.ToEnumerable().OrderBy(p => p.Id).ToList();

                        Assert.AreEqual(testDocuments.Count, documents.Count);

                        var zipped = documents.Zip(testDocuments, (result, test) => new {Result = result, Test = test}).ToList();

                        zipped.ForEach(zip =>
                        {
                            Assert.AreEqual(zip.Test.Id, zip.Result.Id);
                            Assert.AreEqual(zip.Test.Content.Name, zip.Result.Content.Name);
                        });
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(testDocuments.Select(p => p.Id).ToList());
                    }
                }
            }
        }

        [Test]
        public void GetDocumentObservable_MultiDocument_SkipsMissingKeys()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var testDocuments = new List<Document<Beer>>()
                    {
                        new Document<Beer>()
                        {
                            Id = "test-beer",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer2",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test2"
                            }
                        },
                        new Document<Beer>()
                        {
                            Id = "test-beer3",
                            Content = new Beer()
                            {
                                Type = "beer",
                                Name = "test3"
                            }
                        }
                    };

                    try
                    {
                        // Create the documents for the test
                        testDocuments.Take(2).ToList().ForEach(document =>
                        {
                            bucket.Upsert(document);
                        });

                        bucket.Remove(testDocuments.Skip(2).Select(p => p.Id).ToList());

                        var observable = bucket.GetDocumentObservable<Beer>(testDocuments.Select(p => p.Id).ToArray());

                        var documents = observable.ToEnumerable().ToList();

                        Assert.AreEqual(2, documents.Count);
                    }
                    finally
                    {
                        // Cleanup
                        bucket.Remove(testDocuments.Select(p => p.Id).ToList());
                    }
                }
            }
        }

        #endregion

        #region QueryObservable

        [Test]
        public void QueryObservable_View()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var query = bucket.CreateQuery("beer", "brewery_beers").Limit(10);

                    var observable = bucket.QueryObservable<Beer>(query);

                    var lockObj = new object();
                    observable.ForEachAsync(p =>
                    {
                        lock (lockObj)
                        {
                            Console.WriteLine(p.Key);
                            Console.WriteLine(p.Id);
                        }
                    }).Wait();
                }
            }
        }

        [Test]
        public void QueryObservable_View_ExpectError()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var query = bucket.CreateQuery("beer", "brewery_beers_fake_view").Limit(10);

                    var observable = bucket.QueryObservable<Beer>(query);

                    var lockObj = new object();
                    Exception gotException = null;

                    lock (lockObj)
                    {
                        observable.Subscribe(
                            p => // OnNext
                            {
                                lock (lockObj)
                                {
                                    Console.WriteLine(p.Key);
                                    Console.WriteLine(p.Id);
                                }
                            },
                            ex => // OnError
                            {
                                lock (lockObj)
                                {
                                    gotException = ex;

                                    Monitor.Pulse(lockObj);
                                }
                            },
                            () => // OnComplete
                            {
                                lock (lockObj)
                                {
                                    Monitor.Pulse(lockObj);
                                }
                            });

                        // Wait for a pulse which indicates that the observable is complete
                        Monitor.Wait(lockObj);
                    }

                    Assert.IsInstanceOf<CouchbaseViewQueryException>(gotException);
                    Console.WriteLine(gotException.Message);
                }
            }
        }

        [Test]
        public void QueryObservable_N1Ql()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var observable = bucket.QueryObservable<Beer>("SELECT `beer-sample`.* FROM `beer-sample` WHERE type = 'beer' LIMIT 10");

                    var lockObj = new object();
                    observable.ForEachAsync(p =>
                    {
                        lock (lockObj)
                        {
                            Console.WriteLine(p.Name);
                        }
                    }).Wait();
                }
            }
        }

        [Test]
        public void QueryObservable_N1Ql_ExpectError()
        {
            using (var cluster = new Cluster(TestConfiguration.GetConfiguration("default")))
            {
                using (var bucket = cluster.OpenBucket("beer-sample"))
                {
                    var observable = bucket.QueryObservable<Beer>("SELECT `beer-sample`.* FROM `beer - sample` WHERE type = 'beer' LIMIT 10 THIS IS BAD SYNTAX");

                    var lockObj = new object();
                    Exception gotException = null;

                    lock (lockObj)
                    {
                        observable.Subscribe(
                            p => // OnNext
                            {
                                lock (lockObj)
                                {
                                    Console.WriteLine(p.Name);
                                }
                            },
                            ex => // OnError
                            {
                                lock (lockObj)
                                {
                                    gotException = ex;

                                    Monitor.Pulse(lockObj);
                                }
                            },
                            () => // OnComplete
                            {
                                lock (lockObj)
                                {
                                    Monitor.Pulse(lockObj);
                                }
                            });

                        // Wait for a pulse which indicates that the observable is complete
                        Monitor.Wait(lockObj);
                    }

                    Assert.IsInstanceOf<CouchbaseN1QlQueryException>(gotException);
                    Console.WriteLine(gotException.Message);
                }
            }
        }

        #endregion
    }
}
