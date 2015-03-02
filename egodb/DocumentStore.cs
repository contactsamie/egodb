using System;
using DBreeze;

namespace egodb
{
    public class DocumentStore : IDocumentStore
    {
        private  string OpenedBy = "System";
        private DBreezeEngine _engine;
        private string _filedb { set; get; }

        public DocumentStore(string s = null)
        {
            _filedb = s;
        }

        private Action<ADocumentSession> Operation { set; get; }

        public void OpenSession(Action<ADocumentSession> operation)
        {
            _engine = string.IsNullOrEmpty(_filedb)
                ? new DBreezeEngine(new DBreezeConfiguration() {Storage = DBreezeConfiguration.eStorage.MEMORY})
                : new DBreezeEngine(_filedb);

            Operation = operation;
        }

        public void OpenSession(string openedBy, Action<ADocumentSession> operation)
        {
            OpenedBy = openedBy;
            OpenSession(operation);
        }


        public void Dispose()
        {
            using (_engine)
            {
                Operation.Invoke(new DocumentSession(_engine,OpenedBy));
            }
        }
    }
}