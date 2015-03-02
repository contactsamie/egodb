using System;
using System.Collections.Generic;
using System.Linq;
using DBreeze;
using DBreeze.DataTypes;
using Newtonsoft.Json;

namespace egodb
{
    public class DocumentSession : ADocumentSession
    {
        private DBreezeEngine Engine { set; get; }
        private static string OpenedBy { set; get; }
        public DocumentSession(DBreezeEngine engine, string openedBy)
        {
            OpenedBy = openedBy;
            Engine = engine;
        }

        public void Dispose()
        {
            // throw new NotImplementedException();
        }

        public override T Store<T>(T obj, string name = DefaultTableName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("table name must be supplied or argument left empty");
            }

            try
            {
                using (var tran = Engine.GetTransaction())
                {
                    tran.SynchronizeTables(name);

                    obj.DocumentMetaData.Tag.LastSave = DateTime.Now;
                    var cnt = tran.Count(name);
                    var number = (long) (cnt == 0 ? 1 : cnt + 1);
                    obj.DocumentMetaData.Name = name;
                    obj.DocumentMetaData.DocumentId = number;
                    obj.Id = obj.DocumentMetaData.Name + "/" + (obj.DocumentMetaData.DocumentId + 100000);
                    obj.Number = (obj.DocumentMetaData.DocumentId + 100000);
                    obj.DocumentMetaData.Histories = obj.DocumentMetaData.Histories ?? new List<History>();
                    obj.DocumentMetaData.Histories.Add(new History()
                    {
                        UpdatedBy = OpenedBy,
                        Id=Guid.NewGuid(),
                        PreviousValue = "",
                        OperationName = "Creation",
                        Updated = DateTime.Now
                    });
                    tran.Insert<long, DbMJSON<T>>(name, number, obj);
                    tran.Commit();

                    var row = tran.Select<long, DbMJSON<T>>(name, number);


                    if (!row.Exists)
                    {
                        throw new Exception("could not verify data has been inserted into db");
                    }


                    return row.Value.Get;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override T Update<T>(string etag, long id, Func<T, T> operation, string name = DefaultTableName)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("table name must be supplied or argument left empty");
            }

            try
            {
                using (var tran = Engine.GetTransaction())
                {
                    tran.SynchronizeTables(name);
                    var row = tran.Select<long, DbMJSON<T>>(name, id);
                    if (!row.Exists)
                    {
                        throw new Exception("could not verify data has been inserted into db");
                    }
                    var model = row.Value.Get;

                    if (model.DocumentMetaData.Etag != etag)
                    {
                        throw new Exception("document has changed since last load. Please reload");
                    }
                    if (model.DocumentMetaData.Deleted)
                    {
                         throw new Exception("Document is already deleted");
                    }

                    var updatedModel = UpdatedModelPiece(operation, model);


                    tran.Insert<long, DbMJSON<T>>(name, updatedModel.DocumentMetaData.DocumentId, updatedModel);
                    tran.Commit();
                }

                return Load<T>(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static T UpdatedModelPiece<T>(Func<T, T> operation, T model, string OperationName="Modify") where T : BaseIdentity
        {
            var ser = JsonConvert.SerializeObject(model);

            var cloneDocumentMetaData = (DocumentMetaData) model.DocumentMetaData.Clone();


            var updatedModel = operation.Invoke(model);

            updatedModel.DocumentMetaData = cloneDocumentMetaData;


            updatedModel.DocumentMetaData.Tag.LastSave = DateTime.Now;


            updatedModel.DocumentMetaData.Histories = updatedModel.DocumentMetaData.Histories ?? new List<History>();
            updatedModel.DocumentMetaData.Histories.Add(new History()
            {
                UpdatedBy = OpenedBy,
                Id = Guid.NewGuid(),
                PreviousValue = ser,
                OperationName = OperationName,
                Updated = DateTime.Now
            });
            return updatedModel;
        }

        public override T Load<T>(long id, string name = DefaultTableName, bool includeDeleted = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("table name must be supplied or argument left empty");
            }
            try
            {
                using (var tran = Engine.GetTransaction())
                {
                    var row = tran.Select<long, DbMJSON<T>>(name, id);

                    if (!row.Exists)
                    {
                        throw new Exception("could not verify data has been inserted into db");
                    }

                    var result = row.Value.Get;

                    return includeDeleted ? result : (result.DocumentMetaData.Deleted?null:result);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override void Delete<T>(string etag, long id, string name = DefaultTableName, bool deletePermanently = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("table name must be supplied or argument left empty");
            }

            try
            {
                using (var tran = Engine.GetTransaction())
                {
                    tran.SynchronizeTables(name);
                    var row = tran.Select<long, DbMJSON<T>>(name, id);
                    if (!row.Exists)
                    {
                        throw new Exception("could not verify data exists in db");
                    }

                    var model = row.Value.Get;

                    if (model.DocumentMetaData.Etag != etag)
                    {
                        throw new Exception("document has changed since last load. Please reload");
                    }
                    if (deletePermanently)
                    {
                        tran.RemoveKey<long>(name, id);
                    }
                    else
                    {
                        var updatedModel = UpdatedModelPiece((m) =>m, model,"Delete");
                        model.DocumentMetaData.Deleted = true;
                        tran.Insert<long, DbMJSON<T>>(name, updatedModel.DocumentMetaData.DocumentId, updatedModel);
                    }
                 
                    tran.Commit();
                }

                var data = Load<T>(id);
                if (data != null)
                {
                    throw new Exception("Problem deleting document");
                }
            }
            catch (Exception ex)
            {
            }
        }

        public override void UnDelete<T>(string etag, long id, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("table name must be supplied or argument left empty");
            }

            try
            {
                using (var tran = Engine.GetTransaction())
                {
                    tran.SynchronizeTables(name);
                    var row = tran.Select<long, DbMJSON<T>>(name, id);
                    if (!row.Exists)
                    {
                        throw new Exception("could not verify data exists in db");
                    }

                    var model = row.Value.Get;

                    if (model.DocumentMetaData.Etag != etag)
                    {
                        throw new Exception("document has changed since last load. Please reload");
                    }
                   
                        var updatedModel = UpdatedModelPiece((m) => m, model,"UnDelete");
                        model.DocumentMetaData.Deleted = false;
                        tran.Insert<long, DbMJSON<T>>(name, updatedModel.DocumentMetaData.DocumentId, updatedModel);
                   
                    tran.Commit();
                }

                var data = Load<T>(id);
                if (data != null)
                {
                    throw new Exception("Problem deleting document");
                }
            }
            catch (Exception ex)
            {
            }
        }

        public override DBQuery<T> Query<T>(string name = DefaultTableName, bool includeDeleted = false)
        {
            return new DBQuery<T>((query) =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("table name must be supplied or argument left empty");
                }
                try
                {
                    using (var tran = Engine.GetTransaction())
                    {
                        var rows = query.DescendingOrder
                            ? tran.SelectBackwardStartFrom<long, DbMJSON<T>>(name, (long)tran.Count(name), false)
                                .Take(query._Take)
                            : tran.SelectForwardStartFrom<long, DbMJSON<T>>(name, 0, false).Take(query._Take);


                        var result= rows.Select(x => x.Value.Get).ToList();

                        return includeDeleted ? result : result.FindAll(x =>! x.DocumentMetaData.Deleted);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            });
        }

        public override DBQuery<T, I> Query_WithPoorPerformance<T, I>(string name = DefaultTableName, Func<IEnumerable<T>, IEnumerable<T>> queryUpdate = null,bool includeDeleted = false)
        {
            return new DBQuery<T, I>((query) =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("table name must be supplied or argument left empty");
                }
                try
                {
                    using (var tran = Engine.GetTransaction())
                    {
                        var rows = query.DescendingOrder
                            ? tran.SelectBackwardStartFrom<long, DbMJSON<T>>(name, (long)tran.Count(name), false)
                                .Take(query._Take)
                            : tran.SelectForwardStartFrom<long, DbMJSON<T>>(name, 0, false).Take(query._Take);


                        var result= queryUpdate.Invoke(rows.Select(x => x.Value.Get)).ToList();

                     

                        return includeDeleted ? result : result.FindAll(x => !x.DocumentMetaData.Deleted);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            });
        }

        public override DBQuery<T, I> Query_WithPoorPerformance<T, I>(Func<IEnumerable<T>, IEnumerable<T>> queryUpdate = null, bool includeDeleted = false)
        {
            return Query_WithPoorPerformance<T, I>(DefaultTableName, queryUpdate,includeDeleted);
        }

        public override void SaveChanges()
        {
            // throw new NotImplementedException();
        }
    }
}