using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionAssignmentModel : IMapFrom<ProductionAssignmentEntity>
    {
        public long? ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public int DepartmentId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        //public decimal Productivity { get; set; }
        public long? StartDate { get; set; }
        public long? EndDate { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public bool IsManualFinish { get; set; }
        public bool IsManualSetDate { get; set; }
        public decimal? RateInPercent { get; set; }


     

        public EnumAssignedProgressStatus? AssignedProgressStatus { get; set; }
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
                .ForMember(s => s.AssignedProgressStatus, d => d.MapFrom(m => (EnumAssignedProgressStatus)m.AssignedProgressStatus))
                .ReverseMap()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.UnixToDateTime()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.UnixToDateTime()))
                .ForMember(s => s.CreatedDatetimeUtc, d => d.Ignore())
                .ForMember(s => s.IsManualFinish, d => d.Ignore())
                .ForMember(s => s.AssignedProgressStatus, d => d.Ignore());
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

        private decimal? _assignmentWorkload;
        private decimal? _assignmentWorkHour;

        public decimal? AssignmentWorkload { get { return _assignmentWorkload; } }
        public decimal? AssignmentWorkHour { get { return _assignmentWorkHour; } }

        public void SetAssignmentWorkload(decimal? assignmentWorkload)
        {
            _assignmentWorkload = assignmentWorkload;
        }

        public void SetAssignmentWorkHour(decimal? assignmentWorkHour)
        {
            _assignmentWorkHour = assignmentWorkHour;
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

        private decimal? _workloadPerDay;
        private decimal? _workHourPerDay;

        public decimal? WorkloadPerDay { get { return _workloadPerDay; } }
        public decimal? WorkHourPerDay { get { return _workHourPerDay; } }

        public void SetWorkloadPerDay(decimal? workloadPerDay)
        {
            _workloadPerDay = workloadPerDay;
        }

        public void SetWorkHourPerDay(decimal? workHourPerDay)
        {
            _workHourPerDay = workHourPerDay;
        }
    }

    public class ProductionAssignmentInputModel
    {
        public ProductionAssignmentModel[] ProductionAssignments { get; set; }
        public ProductionStepWorkInfoInputModel ProductionStepWorkInfo { get; set; }

        //public DepartmentTimeTableModel[] DepartmentTimeTable { get; set; }
    }

    public class GeneralProductionStepAssignmentModel
    {
        public long ProductionStepId { get; set; }
        public ProductionAssignmentModel[] ProductionAssignments { get; set; }
        public ProductionStepWorkInfoInputModel ProductionStepWorkInfo { get; set; }
    }

    public class GeneralAssignmentModel
    {
        public GeneralProductionStepAssignmentModel[] ProductionStepAssignment { get; set; }
        //public DepartmentTimeTableModel[] DepartmentTimeTable { get; set; }
    }

    public class DepartmentAssignUpdateDateModel
    {
        public long ProductionStepId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public bool IsManualSetDate { get; set; }
        public decimal RateInPercent { get; set; }
        public IList<ProductionAssignmentDetailModel> Details { get; set; }
    }
}
