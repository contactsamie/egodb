using System.Configuration;

namespace egodb
{
    public abstract class BaseIdentity<T>
    {
        protected BaseIdentity()
        {
           DocumentMetaData=new DocumentMetaData();
        }

        public abstract T Id {  set; get; }
        public  long Number
        {
             set;
            get;
        }

        public  DocumentMetaData DocumentMetaData
        {
          set; get;
        }
    }
    public class BaseIdentity:BaseIdentity<string>
    {

        public override string Id {
             set;
            get;
        }
    }
}