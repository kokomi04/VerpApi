using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class ReferFieldModel
    {
        public string CategoryCode { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryFieldName { get; set; }
        public string CategoryFieldTitle { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public bool IsHidden { get; set; }
        public int SortOrder { get; set; }
        public string GetTitleCategoryField()
        {
            var rangeValue = ((EnumDataType)DataTypeId).GetRangeValue();
            if (rangeValue.Length > 0)
            {
                return $"{CategoryFieldTitle} ({string.Join(", ", ((EnumDataType)DataTypeId).GetRangeValue())})";
            }

            return CategoryFieldTitle;
        }
    }


}
