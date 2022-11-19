namespace VErp.Commons.Constants
{
    public static class CategoryFieldConstants
    {
        public static string ParentId = "ParentId";
        public static int ParentId_FiledId = -1;
        public const string ParentId_Title = "Đối tượng cha";

        public static string F_Id = "F_Id";

    }

    public static class CurrencyCateConstants
    {
        public const string CurrencyCategoryCode = "_Currency";

        public const string CurrencyCode = "CurrencyCode";
        public const string CurrencyName = "CurrencyName";
        public const string IsPrimary = "IsPrimary";
        
        public const string DecimalPlace = "DecimalPlace";
        
    }

    public class CurrencyData
    {
        public long? F_Id { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public int? DecimalPlace { get; set; }
        public bool? IsPrimary { get; set; }
    }
}
