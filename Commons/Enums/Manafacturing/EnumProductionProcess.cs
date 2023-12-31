﻿using System.ComponentModel;

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
        public enum EnumProductionStepLinkDataRoleType
        {
            Input = 1,
            Output = 2,
        }

        public enum EnumProductionStepLinkDataObjectType
        {
            [Description("Sản phẩm")]
            Product = 1,
            [Description("Bán thành phẩm")]
            ProductSemi = 2
        }

        public enum EnumOutsourceType
        {
            [Description("Gia công chi tiết")]
            OutsourcePart = 1,
            [Description("Gia công công đoạn")]
            OutsourceStep = 2,
            [Description("Gia công nguyên vật liệu")]
            OutsourceMaterial = 3
        }

        public enum EnumProductionStepLinkDataType
        {
            None = 0,
            [Description("LinkData gia công chi tiết")]
            StepLinkDataOutsourcePart = 1,
            [Description("LinkData gia công công đoạn")]
            StepLinkDataOutsourceStep = 2,
            Others = 99,
        }

        public enum EnumOutsourceRequestStatusType : int
        {
            [Description("Chưa xử lý")]
            Unprocessed = 1,
            [Description("Đang xử lý")]
            Processing = 2,
            [Description("Đã xử lý")]
            Processed = 3
        }

        public enum EnumProductionStepLinkType
        {
            [Description("Bàn giao công đoạn")]
            Handover = 1,
            [Description("Trung gian qua kho")]
            Intermediate = 2
        }
    }
}
