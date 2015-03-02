using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DBreeze;
using DBreeze.DataTypes;
using Newtonsoft.Json;
namespace egodb.Tests
{
    [TestClass]
    public class dbreezzetest
    {
        [TestMethod]
        public void TestMethod1()
        {

            DBreeze.Utils.CustomSerializator.Serializator = JsonConvert.SerializeObject;
            DBreeze.Utils.CustomSerializator.Deserializator = JsonConvert.DeserializeObject;
            
           // using (var engine = new DBreezeEngine( AppDomain.CurrentDomain.BaseDirectory+ "/" +  "egodb"))
            using (var engine = new DBreezeEngine(new DBreezeConfiguration() { Storage = DBreezeConfiguration.eStorage.MEMORY }))
            {
                try
                {
                    using (var tran = engine.GetTransaction())
                    {
                        tran.Insert<int, DbMJSON<DateTime>>("t1", 1, DateTime.Now);
                        tran.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                try
                {
                    using (var tran = engine.GetTransaction())
                    {
                        //Note, if one of threads needs, inside of the transaction, to read data from the tables and it wants to be sure that till the end of transaction other threads will not modify the data, this thread must reserve tables for synchronized read.
                        //If you think that there is no necessity to block table(s) and other threads could write data in parallel just don’t use tran.SynchronizeTables.
                        tran.SynchronizeTables("t1");
                        var row = tran.Select<int, DbMJSON<DateTime>>("t1", 1);

                        if (row.Exists)
                        {
                            var key = row.Key;
                            var res = row.Value;
                            //btRes will be null, because we have only 4 bytes
                            var btRes = row.GetValuePart(12);
                            //btRes will be null, because we have only 4 bytes
                            btRes = row.GetValuePart(12, 1);
                            //will return 4 bytes
                            btRes = row.GetValuePart(0);
                            //will return 4 bytes
                            btRes = row.GetValuePart(0, 4);

                            Console.WriteLine(btRes);

                            var val = row.Value;

                            var act = row.Value.Get;

                            var ser = row.Value.SerializedObject;
                        }

                        var rows = tran.SelectBackward<int, DbMJSON<DateTime>>("t1");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                try
                {
                    using (var tran = engine.GetTransaction())
                    {
                        //Note, if one of threads needs, inside of the transaction, to read data from the tables and it wants to be sure that till the end of transaction other threads will not modify the data, this thread must reserve tables for synchronized read.
                        //If you think that there is no necessity to block table(s) and other threads could write data in parallel just don’t use tran.SynchronizeTables.
                        tran.SynchronizeTables("t1");
                        var row = tran.Select<int, DbMJSON<DateTime>>("t1", 1);

                        if (row.Exists)
                        {
                            tran.RemoveKey<int>("t1", row.Key);
                        }

                        tran.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                try
                {
                    using (var tran = engine.GetTransaction())
                    {
                        //Note, if one of threads needs, inside of the transaction, to read data from the tables and it wants to be sure that till the end of transaction other threads will not modify the data, this thread must reserve tables for synchronized read.
                        //If you think that there is no necessity to block table(s) and other threads could write data in parallel just don’t use tran.SynchronizeTables.
                        tran.SynchronizeTables("t1");
                        var row = tran.Select<int, DbMJSON<DateTime>>("t1", 1);

                        if (row.Exists)
                        {
                            tran.RemoveKey<int>("t1", row.Key);

                            var val = row.Value;

                            var act = row.Value.Get;

                            var ser = row.Value.SerializedObject;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

         
            }
        }
    }
}
