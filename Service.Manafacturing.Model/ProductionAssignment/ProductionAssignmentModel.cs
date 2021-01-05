using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionAssignmentModel : IMapFrom<ProductionAssignmentEntity>
    {
        public long? ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int DepartmentId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal Productivity { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public long UpdatedDatetimeUtc { get; set; }

        public virtual ICollection<ProductionAssignmentDetailModel> ProductionAssignmentDetail { get; set; }

        public ProductionAssignmentModel()
        {
            ProductionAssignmentDetail = new List<ProductionAssignmentDetailModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionAssignmentEntity, ProductionAssignmentModel>()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.GetUnix()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.GetUnix()))
                .ForMember(s => s.UpdatedDatetimeUtc, d => d.MapFrom(m => m.UpdatedDatetimeUtc.GetUnix()))
                .ReverseMap()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.UnixToDateTime()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.UnixToDateTime()))
                .ForMember(s => s.UpdatedDatetimeUtc, d => d.Ignore());
        }
    }

    public class ProductionAssignmentDetailModel : IMapFrom<ProductionAssignmentDetail>
    {
        public long WorkDate { get; set; }
        public decimal? QuantityPerDay { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionAssignmentDetail, ProductionAssignmentDetailModel>()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.GetUnix()))
                .ReverseMap()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.UnixToDateTime()));
        }
    }

    public class ProductionAssignmentInputModel
    {
        public ProductionAssignmentModel[] ProductionAssignments { get; set; }
        public ProductionStepWorkInfoInputModel ProductionStepWorkInfo { get; set; }
    }
}
