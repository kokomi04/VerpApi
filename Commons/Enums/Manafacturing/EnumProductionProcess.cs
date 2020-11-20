using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VErp.Commons.Enums.Manafacturing
{
    public static class EnumProductionProcess
    {
        public enum EnumContainerType
        {
            [Description("Sản phẩm")]
            Product = 1,
            [Description("Lệnh sản xuất")]
            ProductionOrder = 2,
        }
        public enum ProductionStepLinkDataRoleType
        {
            Input = 1,
            Output = 2,
        }

        public enum ProductionStepLinkDataObjectType
        {
            Product = 1,
            ProductSemi = 2
        }

        public enum OutsourceOrderType
        {
            [Description("Gia công chi tiết")]
            OutsourcePart = 1,
            [Description("Gia công công đoạn")]
            OutsourceStep = 2
        }

        public enum OutsourcePartProcessType
        {
            [Description("Chưa xử lý")]
            Unprocessed = 1,
            [Description("Đang xử lý")]
            Processing = 2,
            [Description("Đã xử lý")]
            Processed = 3
        }
    }
}
