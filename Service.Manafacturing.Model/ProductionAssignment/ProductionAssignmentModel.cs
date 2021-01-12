using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;
using System.Linq;

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
        public long CreatedDatetimeUtc { get; set; }

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
                .ForMember(s => s.CreatedDatetimeUtc, d => d.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ReverseMap()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.UnixToDateTime()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.UnixToDateTime()))
                .ForMember(s => s.CreatedDatetimeUtc, d => d.Ignore());
        }

        public bool IsChange(ProductionAssignmentEntity entity)
        {
            var isChange = entity.AssignmentQuantity != AssignmentQuantity
                        || entity.ProductionStepLinkDataId != ProductionStepLinkDataId
                        || entity.StartDate.GetUnix() != StartDate
                        || entity.EndDate.GetUnix() != EndDate
                        || entity.ProductionAssignmentDetail.Count != ProductionAssignmentDetail.Count;
            if (!isChange)
            {
                isChange = entity.ProductionAssignmentDetail
                    .Any(ad => !ProductionAssignmentDetail
                    .Any(oad => oad.WorkDate == ad.WorkDate.GetUnix() && oad.QuantityPerDay == ad.QuantityPerDay));
            }
            return isChange;
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

        public DepartmentTimeTableModel[] DepartmentTimeTable { get; set; }
    }
}
