namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InternalObjectDataMail<T> where T : class
    {
        public string CreatedByUser { get; set; }
        public string UpdatedByUser { get; set; }
        public string CheckedByUser { get; set; }
        public string CensoredByUser { get; set; }

        public T Data { get; set; }
    }
}