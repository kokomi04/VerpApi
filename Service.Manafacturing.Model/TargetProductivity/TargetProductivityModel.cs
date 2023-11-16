using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model
{
    public class TargetProductivityModel : IMapFrom<TargetProductivity>
    {
        public int TargetProductivityId { get; set; }
        public string TargetProductivityCode { get; set; }
        public long TargetProductivityDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; } = false;
        public string Note { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public decimal? EstimateProductionDays { get; set; }
        public decimal? EstimateProductionQuantity { get; set; }

        public IList<TargetProductivityDetailModel> TargetProductivityDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TargetProductivity, TargetProductivityModel>()
            .ForMember(m => m.TargetProductivityDate, v => v.MapFrom(m => m.TargetProductivityDate.GetUnix()))
            .ForMember(m => m.TargetProductivityDetail, v => v.MapFrom(m => m.TargetProductivityDetail))
            .ReverseMapCustom()
            .ForMember(m => m.TargetProductivityDetail, v => v.Ignore());
        }
    }

    [Display(Name = "Chi tiết năng suất mục tiêu")]
    public class TargetProductivityDetailModel : MappingDataRowAbstract, IMapFrom<TargetProductivityDetail>
    {
        [FieldDataIgnore]
        public int TargetProductivityDetailId { get; set; }
        [FieldDataIgnore]
        public int TargetProductivityId { get; set; }
        [Display(Name = "Công đoạn")]
        public int ProductionStepId { get; set; }
        [Display(Name = "Năng suất mục tiêu")]
        public decimal TargetProductivity { get; set; }
        [Display(Name = "Đơn vị thời gian")]
        public EnumProductivityTimeType ProductivityTimeTypeId { get; set; }
        [Display(Name = "Đối tượng tính năng suất")]
        public EnumProductivityResourceType ProductivityResourceTypeId { get; set; }
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
        [Display(Name = "Cách tính KLCV")]
        public EnumWorkloadType WorkLoadTypeId { get; set; }
        [Display(Name = "Số giờ phân công tối thiểu")]
        public decimal MinAssignHours { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<TargetProductivityDetail, TargetProductivityDetailModel>()
            .ForMember(m => m.ProductivityTimeTypeId, v => v.MapFrom(m => (EnumProductivityTimeType)m.ProductivityTimeTypeId))
            .ForMember(m => m.ProductivityResourceTypeId, v => v.MapFrom(m => (EnumProductivityResourceType)m.ProductivityResourceTypeId))
            .ForMember(m => m.WorkLoadTypeId, v => v.MapFrom(m => (EnumWorkloadType)m.WorkLoadTypeId))
            .ReverseMapCustom()
            .ForMember(m => m.ProductivityTimeTypeId, v => v.MapFrom(m => (int)m.ProductivityTimeTypeId))
            .ForMember(m => m.ProductivityResourceTypeId, v => v.MapFrom(m => (int)m.ProductivityResourceTypeId))
            .ForMember(m => m.WorkLoadTypeId, v => v.MapFrom(m => (int)m.WorkLoadTypeId));
        }
    }
}
