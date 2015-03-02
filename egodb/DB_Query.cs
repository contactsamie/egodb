using System;
using System.Collections.Generic;
using System.Linq;

namespace egodb
{
    public class DBQuery<T>
    {
        private Func<DBQuery<T>, List<T>> _operation { set; get; }

        public DBQuery(Func<DBQuery<T>, List<T>> operation)
        {
            _operation = operation;
        }

        internal bool DescendingOrder = false;


        internal int _Take = 1000;

        public DBQuery<T> Take(int take)
        {
            _Take = take;
            return this;
        }

           
        public DBQuery<T> OrderByDescending()
        {
            DescendingOrder = true;
            return this;
        }

        public DBQuery<T> OrderByAscending()
        {
            DescendingOrder = false;
            return this;
        }

        internal bool getFirst;
        internal bool getFirstOrDefault;

        public List<T> ToList()
        {
            return _operation.Invoke(this);
        }

        public T First()
        {
            getFirst = true;
            return _operation.Invoke(this).First();
        }

        public T FirstOrDefault()
        {
            getFirstOrDefault = true;
            return _operation.Invoke(this).FirstOrDefault();
        }
    }
}