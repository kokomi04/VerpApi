﻿using AutoMapper;
using System;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProcessingOrderListModel : IMapFrom<ProcessingOrderListEntity>
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductTitle { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }

        public IList<StepListModel> Steps { get; set; }

        public ProcessingOrderListModel()
        {
            Steps = new List<StepListModel>();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProcessingOrderListEntity, ProcessingOrderListModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()));
        }
    }

    public class StepReportModel : StepListModel
    {
        public decimal StepProgressPercent { get; set; }
        public IList<DepartmentProgress> DepartmentProgress { get; set; }

        public StepReportModel()
        {
            DepartmentProgress = new List<DepartmentProgress>();
        }
    }

    public class DepartmentProgress
    {
        public decimal DepartmentProgressPercent { get; set; }
        public int DepartmentId { get; set; }
    }

    public class StepListModel
    {
        public string StepName { get; set; }
        public int StepId { get; set; }
    }

    public class ProcessingOrderListEntity
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public string ProductTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
