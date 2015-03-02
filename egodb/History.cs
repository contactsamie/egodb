using System;
using Newtonsoft.Json;

namespace egodb
{
    public class History
    {
        public Guid Id { set; get; }
        public object PreviousValue { set; get; }
        public DateTime Updated { set; get; }
        public string OperationName { set; get; }
        public string UpdatedBy { set; get; }

        public T TryGetPreviousValue<T>()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(PreviousValue.ToString());
            }
            catch (Exception)
            {

                return  default(T);
            }
           
        }
    }
}