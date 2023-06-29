namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class SimpleFileInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long? FileLength { get; set; }
        public int FileTypeId { get; set; }
    }
}
