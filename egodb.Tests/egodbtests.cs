using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace egodb.Tests
{
    [TestClass]
    public class egodbtests
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var store = new DocumentStore())
            {
                store.OpenSession((session) =>
                {
                    var update = session.Store(new TestModel() {TestName = "bla"});

                    session.Store(new TestModel() {TestName = "bla1"});
                    session.Store(new TestModel() {TestName = "bla2"});
                    session.Store(new TestModel() {TestName = "bla3"});


                    Assert.IsNotNull(update);
                    var newObj = session.Load<TestModel>(update.DocumentMetaData.DocumentId);

                    Assert.AreEqual("bla", newObj.TestName);

                    var no1 = session.Update<TestModel>(newObj.DocumentMetaData.Etag, newObj.DocumentMetaData.DocumentId, (ob) =>
                    {
                        ob.TestName = "done";
                        return ob;
                    });
                    Assert.AreEqual("done", no1.TestName);
                    var newObj1 = session.Load<TestModel>(update.DocumentMetaData.DocumentId);

                    Assert.AreEqual("done", newObj1.TestName);


                    var all = session.Query<TestModel>().ToList();

                    Assert.AreEqual(4, all.Count);

                    Assert.IsTrue(all.Exists(x => x.TestName == "bla1"));
                    Assert.IsTrue(all.Exists(x => x.TestName == "bla2"));
                    Assert.IsTrue(all.Exists(x => x.TestName == "bla3"));
                    Assert.IsFalse(all.Exists(x => x.TestName == "bla"));
                    Assert.IsTrue(all.Exists(x => x.TestName == "done"));


                    session.Update<TestModel>(newObj.DocumentMetaData.Etag, newObj.DocumentMetaData.DocumentId, (ob) =>
                    {
                        ob.TestName = "done3";
                        return ob;
                    });
                    // should throw


                    var updatelog = session.Update<TestModel>(newObj.DocumentMetaData.Etag, newObj.DocumentMetaData.DocumentId, (ob) =>
                    {
                        ob.TestName = "done";
                        return ob;
                    });

                    Assert.IsNull(updatelog);


                    var newLoad = session.Load<TestModel>(newObj.DocumentMetaData.DocumentId);
                    session.Update<TestModel>(newLoad.DocumentMetaData.Etag, newObj.DocumentMetaData.DocumentId, (ob) =>
                    {
                        ob.TestName = "done";
                        return ob;
                    });


                    session.Delete<TestModel>(update.DocumentMetaData.Etag, update.DocumentMetaData.DocumentId);
                    var all2 = session.Query<TestModel>().ToList();
                    Assert.AreEqual(4, all2.Count);


                    var newLoad1 = session.Load<TestModel>(update.DocumentMetaData.DocumentId);
                    session.Delete<TestModel>(newLoad1.DocumentMetaData.Etag, update.DocumentMetaData.DocumentId);
                    var all3 = session.Query<TestModel>().ToList();
                    Assert.AreEqual(3, all3.Count);


                    var newLoad11 = session.Load<TestModel>(update.DocumentMetaData.DocumentId,"Default",true);
                    session.UnDelete<TestModel>(newLoad11.DocumentMetaData.Etag, update.DocumentMetaData.DocumentId, "Default");
                    var all32 = session.Query<TestModel>().ToList();
                    Assert.AreEqual(4, all32.Count);


                    var newLoad1a = session.Load<TestModel>(update.DocumentMetaData.DocumentId);
                    session.Delete<TestModel>(newLoad1a.DocumentMetaData.Etag, update.DocumentMetaData.DocumentId);
                    var all3a = session.Query<TestModel>().ToList();
                    Assert.AreEqual(3, all3a.Count);

                  
                    
                    var all3ax = session.Query<TestModel>( "Default", true).ToList( );
                    Assert.AreEqual(4, all3ax.Count);

                    Assert.IsFalse(all3.Exists(x => x.TestName == "done3"));
                    Assert.IsFalse(all3.Exists(x => x.TestName == "done"));

                    var all4 = session.Query_WithPoorPerformance<TestModel, TestModelIndex>((query) => query.Where(x => x.TestName == "bla2")).ToList();

                    Assert.AreEqual(1,all4.Count);
                    Assert.IsTrue(all4.Exists(x => x.TestName == "bla2"));
                });
            }
        }
    }
}