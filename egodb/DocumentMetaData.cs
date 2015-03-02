using System;
using System.Collections.Generic;

namespace egodb
{
    public class DocumentMetaData:ICloneable
    {
        public DocumentMetaData()
        {
            Name = "";
            Tag = new Etag();
            Histories=new List<History>();
        }

        public bool Deleted { set; get; }
        public List<History> Histories { set; get; }

        public string Name { set; get; }

        public long DocumentId { set; get; }

        

        public Etag Tag { set; get; }

        public string Etag
        {
            set { Tag.Value = value; }
            get { return Tag.Value; }
        }

        public object Clone()
        {
          return  this.MemberwiseClone();
        }
    }
}