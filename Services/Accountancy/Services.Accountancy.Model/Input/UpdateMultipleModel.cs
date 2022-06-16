namespace VErp.Services.Accountancy.Model.Input

{
    public class UpdateMultipleModel
    {
        public string FieldName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public long[] BillIds { get; set; }
        public long[] DetailIds { get; set; }
    }
}
