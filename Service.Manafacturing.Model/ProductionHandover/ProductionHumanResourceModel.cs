using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHumanResourceModel : ProductionHumanResourceInputModel
    {
        public int CreatedByUserId { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHumanResource, ProductionHumanResourceModel>()
                .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.GetUnix()));
        }
    }

    public class ProductionHumanResourceInputModel : IMapFrom<ProductionHumanResource>
    {
        public long? ProductionHumanResourceId { get; set; }
        public decimal OfficeWorkDay { get; set; }
        public decimal OvertimeWorkDay { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long? Date { get; set; }
        public string Note { get; set; }
        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHumanResourceInputModel, ProductionHumanResource>()
                .ForMember(m => m.Date, v => v.MapFrom(m => m.Date.UnixToDateTime()));
        }
    }
}
