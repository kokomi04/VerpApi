namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class InternalProductCateOutput
    {
        public int ProductCateId { get; set; }
        public string ProductCateName { get; set; }
        public int? ParentProductCateId { get; set; }
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
    }
}
