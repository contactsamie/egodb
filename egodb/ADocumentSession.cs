using System;
using System.Collections.Generic;

namespace egodb
{
    public abstract class ADocumentSession : IDisposable
    {
        public const string DefaultTableName = "Default";

        public abstract T Store<T>(T obj, string name = DefaultTableName) where T : BaseIdentity;

        public abstract T Update<T>(string etag, long id, Func<T, T> operation, string name = DefaultTableName)
            where T : BaseIdentity;

        public abstract T Load<T>(long id, string name = DefaultTableName,bool includeDeleted=false) where T : BaseIdentity;
       
        public abstract void Delete<T>(string etag, long id, string name = DefaultTableName, bool deletePermanently=false) where T : BaseIdentity;

        public abstract void UnDelete<T>(string etag, long id, string name ) where T : BaseIdentity;

        public abstract DBQuery<T> Query<T>(string name = DefaultTableName,bool includeDeleted=false)   where T : BaseIdentity;
        public abstract DBQuery<T, I> Query_WithPoorPerformance<T, I>(string name = DefaultTableName, Func<IEnumerable<T>, IEnumerable<T>> queryUpdate = null,bool includeDeleted=false)
            where I:DBIndex<T> where T : BaseIdentity;


        public abstract DBQuery<T, I> Query_WithPoorPerformance<T, I>(Func<IEnumerable<T>, IEnumerable<T>> queryUpdate = null, bool includeDeleted = false)
            where I : DBIndex<T>
            where T : BaseIdentity;



        public abstract void SaveChanges();

        public void Dispose()
        {
            
        }
    }
}