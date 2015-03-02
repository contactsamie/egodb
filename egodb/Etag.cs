using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace egodb
{
    public class Etag
    {
        public Etag()
        {
            Created = DateTime.Now;
            LastSave = DateTime.Now;
            Id = Guid.NewGuid();
        }

        public Guid Id { set; get; }
        public DateTime Created { set; get; }
        public DateTime LastSave { set; get; }

        public string Value
        {
            set
            {
                var parts = value.Split('-');
                var dates = parts[0] + "-" + parts[1] + "-";
                Created = new DateTime(long.Parse(parts[0]));
                LastSave = new DateTime(long.Parse(parts[1]));
                Id = Guid.Parse(value.Replace(dates, ""));
            }
            get { return LastSave.Ticks + "-" + Created.Ticks + "-" + Id; }
        }
    }
}