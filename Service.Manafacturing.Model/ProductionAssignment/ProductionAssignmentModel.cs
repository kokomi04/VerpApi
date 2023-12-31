﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.DataAnnotationsExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionAssignmentModel : IMapFrom<ProductionAssignmentEntity>
    {
        public long? ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public int DepartmentId { get; set; }
        [GreaterThan(0, ErrorMessage = "Số lượng phân công phải >0")]
        public decimal AssignmentQuantity { get; set; }
        public decimal? AssignmentHours { get; set; }
        
        public long ProductionStepLinkDataId { get; set; }
        
        public long? StartDate { get; set; }
        public long? EndDate { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public bool IsManualFinish { get; set; }
        public bool IsManualSetStartDate { get; set; }
        public bool IsManualSetEndDate { get; set; }
        public decimal? RateInPercent { get; set; }
        [MaxLength(512)]
        public string Comment { get; set; }

        public EnumAssignedProgressStatus? AssignedProgressStatus { get; set; }

        public EnumAssignedProgressStatus? AssignedInputStatus { get; set; }

        public bool? IsUseMinAssignHours { get; set; }

        public virtual ICollection<ProductionAssignmentDetailModel> ProductionAssignmentDetail { get; set; }

        public ProductionAssignmentModel()
        {
            ProductionAssignmentDetail = new List<ProductionAssignmentDetailModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionAssignmentEntity, ProductionAssignmentModel>()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.GetUnix()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.GetUnix()))
                .ForMember(s => s.CreatedDatetimeUtc, d => d.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(s => s.AssignedProgressStatus, d => d.MapFrom(m => (EnumAssignedProgressStatus)m.AssignedProgressStatus))
                .ForMember(s => s.AssignedInputStatus, d => d.MapFrom(m => (EnumAssignedProgressStatus)m.AssignedInputStatus))
                .ReverseMapCustom()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.UnixToDateTime()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.UnixToDateTime()))
                .ForMember(s => s.CreatedDatetimeUtc, d => d.Ignore())
                .ForMember(s => s.IsManualFinish, d => d.Ignore())
                .ForMember(s => s.AssignedProgressStatus, d => d.Ignore())
                .ForMember(s => s.AssignedInputStatus, d => d.Ignore());
        }

        public bool IsChange(ProductionAssignmentEntity _entity)
#pragma warning disable S125 // Sections of code should not be commented out
        {
            return true;
            /*
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
            */
        }
#pragma warning restore S125 // Sections of code should not be commented out

        public decimal? AssignmentWorkload { get; set; }
    }

    public class ProductionAssignmentDetailModel : IMapFrom<ProductionAssignmentDetail>
    {
        public long WorkDate { get; set; }
        [GreaterThan(0, ErrorMessage = "Số lượng phân công theo ngày phải >0")]
        public decimal? QuantityPerDay { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductionAssignmentDetail, ProductionAssignmentDetailModel>()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.GetUnix()))
                .ForMember(s => s.QuantityPerDay, d => d.MapFrom(m => m.QuantityPerDay))
                .ForMember(s => s.WorkloadPerDay, d => d.MapFrom(m => m.WorkloadPerDay))
                .ForMember(s => s.WorkHourPerDay, d => d.MapFrom(m => m.HoursPerDay))
                .ForMember(s => s.DetailLinkDatas, d => d.MapFrom(m => m.ProductionAssignmentDetailLinkData))
                .ReverseMapCustom()
                .ForMember(s => s.WorkDate, d => d.MapFrom(m => m.WorkDate.UnixToDateTime()))
                .ForMember(s => s.QuantityPerDay, d => d.MapFrom(m => m.QuantityPerDay))
                .ForMember(s => s.WorkloadPerDay, d => d.MapFrom(m => m.WorkloadPerDay))
                .ForMember(s => s.HoursPerDay, d => d.MapFrom(m => m.WorkHourPerDay))
                .ForMember(s => s.ProductionAssignmentDetailLinkData, d => d.MapFrom(m => m.DetailLinkDatas));
        }

        //private decimal? _workloadPerDay;
        //private decimal? _workHourPerDay;

        public decimal? WorkloadPerDay { get; set; }
        public decimal? WorkHourPerDay { get; set; }
        public decimal? MinAssignHours { get; set; }
        public bool? IsUseMinAssignHours { get; set; }

        //public void SetWorkloadPerDay(decimal? workloadPerDay)
        //{
        //    _workloadPerDay = workloadPerDay;
        //}

        //public void SetWorkHourPerDay(decimal? workHourPerDay)
        //{
        //    _workHourPerDay = workHourPerDay;
        //}
        public IList<ProductionAssignmentDetailLinkDataModel> DetailLinkDatas { get; set; }
    }

    public class ProductionAssignmentDetailLinkDataModel : IMapFrom<ProductionAssignmentDetailLinkData>
    {
        public long ProductionStepLinkDataId { get; set; }

        public decimal QuantityPerDay { get; set; }

        public decimal WorkloadPerDay { get; set; }

        public decimal HoursPerDay { get; set; }

        public decimal MinAssignHours { get; set; }

        public bool IsUseMinAssignHours { get; set; }
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
        public bool IsManualSetStartDate { get; set; }
        public bool IsManualSetEndDate { get; set; }
        public decimal RateInPercent { get; set; }
        public IList<ProductionAssignmentDetailModel> Details { get; set; }
    }

    public class CapacityAssignInfo
    {
        public int DepartmentId { get; set; }
        public decimal AssignQuantity { get; set; }
        public decimal AssignWorkloadQuantity { get; set; }
        public decimal AssignWorkHour { get; set; }
        public long? StartDate { get; set; }
        public long? EndDate { get; set; }
        public bool IsSelectionAssign { get; set; }
        public bool? IsUseMinAssignHours { get; set; }
        
        public bool IsManualSetStartDate { get; set; }
        public bool IsManualSetEndDate { get; set; }
        public decimal? RateInPercent { get; set; }
        public IList<ProductionAssignmentDetailModel> ByDates { get; set; }
    }
}
