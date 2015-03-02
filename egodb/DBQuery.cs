using System;
using System.Collections.Generic;

namespace egodb
{
    public class DBQuery<T, I> : DBQuery<T>
    {
        public DBQuery(Func<DBQuery<T>, List<T>> operation) : base(operation)
        {
        }
    }
}