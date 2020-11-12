using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using AutoMapper;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionOrder
{

    public class ProductionOrderListModel : ProductionOrderDetailOutputModel, IMapFrom<ProductionOrderListEntity>
    {
        public string ProductionOrderCode { get; set; }
        public long VoucherDate { get; set; }
        public long? FinishDate { get; set; }
        public string Description { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionOrderListEntity, ProductionOrderListModel>()
                .ForMember(dest => dest.ProductionOrderCode, opt => opt.MapFrom(source => source.ProductionOrderCode))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(source => source.Description))
                .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(source => source.VoucherDate.GetUnix()))
                .ForMember(dest => dest.FinishDate, opt => opt.MapFrom(source => source.FinishDate.GetUnix()));
        }
    }

    public class ProductionOrderListEntity : ProductionOrderDetailOutputModel
    {
        public string ProductionOrderCode { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Description { get; set; }
    }
}
