using AutoMapper;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHandoverModel : ProductionHandoverInputModel
    {
        public required long ProductionHandoverReceiptId { get; set; }
        public required string ProductionHandoverReceiptCode { get; set; }
        public required EnumHandoverStatus HandoverStatusId { get; set; }
               
        public required int CreatedByUserId { get; set; }
        public required int? AcceptByUserId { get; set; }
               
        public required long? InventoryRequirementDetailId { get; set; }
        public required long? InventoryDetailId { get; set; }
        public required int? InventoryProductId { get; set; }
        public required bool IsAuto { get; set; }
        public required long? InventoryId { get; set; }
        public required string InventoryCode { get; set; }
        public required decimal? InventoryQuantity { get; set; }
    }

    public class ProductionHandoverReceiptModel : IMapFrom<ProductionHandoverReceipt>
    {
        public long? ProductionHandoverReceiptId { get; set; }
        public string ProductionHandoverReceiptCode { get; set; }
        public EnumHandoverStatus HandoverStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int? AcceptByUserId { get; set; }
        public IList<ProductionHandoverInputModel> Handovers { get; set; }
        public IList<ProductionHistoryInputModel> Histories { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionHandoverReceiptModel, ProductionHandoverReceipt>()
                .ForMember(m => m.ProductionHandover, v => v.Ignore())
                .ForMember(m => m.ProductionHistory, v => v.Ignore())
                .ReverseMap()
                .ForMember(m => m.Handovers, v => v.Ignore())
                .ForMember(m => m.Histories, v => v.Ignore());
        }

    }

    public class ProductionHandoverInputModel : IMapFrom<ProductionHandoverEntity>
    {
        public required long? ProductionHandoverId { get; set; }
        public required decimal HandoverQuantity { get; set; }
        public required long ObjectId { get; set; }
        public required EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public required int FromDepartmentId { get; set; }
        public required long FromProductionStepId { get; set; }
        public required int ToDepartmentId { get; set; }
        public required long ToProductionStepId { get; set; }
        public required long? HandoverDatetime { get; set; }
        public required string Note { get; set; }
               
        public required long ProductionOrderId { get; set; }


    }

    public class ProductionHandoverHistoryReceiptModel
    {
        public long ProductionHandoverReceiptId { get; set; }
        public string ProductionHandoverReceiptCode { get; set; }
        public EnumHandoverStatus HandoverStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserFullName { get; set; }
        public int? AcceptByUserId { get; set; }
        public string AcceptByUserFullName { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public long Date { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepTitle { get; set; }
        public long? StepId { get; set; }
        public string StepName { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string HandoverNote { get; set; }
        public string ProductionNote { get; set; }

        public decimal? HandoverQuantity { get; set; }
        public decimal? ProductionQuantity { get; set; }
        public decimal? OvertimeProductionQuantity { get; set; }
    }
}
