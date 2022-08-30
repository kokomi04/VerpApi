using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHandoverModel : ProductionHandoverInputModel
    {
        public long ProductionHandoverReceiptId { get; set; }
        public string ProductionHandoverReceiptCode { get; set; }
        public EnumHandoverStatus HandoverStatusId { get; set; }        
        
        public int CreatedByUserId { get; set; }
        public int? AcceptByUserId { get; set; }


    }

    public class ProductionHandoverReceiptModel : IMapFrom<ProductionHandoverReceipt>
    {
        public long? ProductionHandoverReceiptId { get; set; }
        public string ProductionHandoverReceiptCode { get; set; }
        public EnumHandoverStatus HandoverStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int? AcceptByUserId { get; set; }
        public IList<ProductionHandoverInputModel> Handovers { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionHandoverReceiptModel, ProductionHandoverReceipt>()
                .ForMember(m => m.ProductionHandover, v => v.Ignore())
                .ReverseMap()
                .ForMember(m => m.Handovers, v => v.Ignore());
        }

    }

    public class ProductionHandoverInputModel : IMapFrom<ProductionHandoverEntity>
    {
        public long? ProductionHandoverId { get; set; }
        public decimal HandoverQuantity { get; set; }
        public long ObjectId { get; set; }
        public EnumProductionProcess.EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
        public int FromDepartmentId { get; set; }
        public long FromProductionStepId { get; set; }
        public int ToDepartmentId { get; set; }
        public long ToProductionStepId { get; set; }
        public long? HandoverDatetime { get; set; }
        public string Note { get; set; }

        public long ProductionOrderId { get; set; }


    }
}
