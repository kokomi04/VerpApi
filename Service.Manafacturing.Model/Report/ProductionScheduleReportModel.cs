using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProductionScheduleReportModel : ProductionScheduleModel, IMapFrom<ProductionScheduleEntity>
    {
        public decimal CompletedQuantity { get; set; }
        public string UnfinishedStepTitle { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleEntity, ProductionScheduleReportModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
                .ForMember(dest => dest.ProductionScheduleStatus, opt => opt.MapFrom(source => (EnumScheduleStatus)source.ProductionScheduleStatus));
        }
    }
}
