using System;

namespace egodb
{
    public interface IDocumentStore : IDisposable
    {
        void OpenSession(Action<ADocumentSession> operation);
        void OpenSession(string openedBy,Action<ADocumentSession> operation);
    }
}