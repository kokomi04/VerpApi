﻿namespace VErp.Commons.GlobalObject.InternalDataInterface.Stock
{
    public class ProductTypeOutput
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public string IdentityCode { get; set; }
        public int? ParentProductTypeId { get; set; }
        public int SortOrder { get; set; }
    }
}
