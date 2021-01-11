using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Enums.MasterEnum;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Commons.Library;

namespace VErp.Services.Manafacturing.Model.Report
{
    public class ProductionOrderStepModel 
    {
        public IList<StepInfoModel> Steps { get; set; }
        public IList<ProductionScheduleInfoModel> ProductionSchedules { get; set; }

        public IDictionary<int, Dictionary<long, List<ProductionOrderStepProgressModel>>> ProductionOrderStepProgress { get; set; }

        public ProductionOrderStepModel()
        {
            Steps = new List<StepInfoModel>();
            ProductionSchedules = new List<ProductionScheduleInfoModel>();
            ProductionOrderStepProgress = new Dictionary<int, Dictionary<long, List<ProductionOrderStepProgressModel>>>();
        }

    }

    public class ProductionOrderStepProgressModel
    {
        public decimal ProgressPercent { get; set; }
        public IList<StepScheduleProgressDataModel> InputData { get; set; }
        public IList<StepScheduleProgressDataModel> OutputData { get; set; }
        public ProductionOrderStepProgressModel()
        {
            InputData = new List<StepScheduleProgressDataModel>();
            OutputData = new List<StepScheduleProgressDataModel>();
        }
    }

    public class StepInfoModel
    {
        public int StepId { get; set; }
        public string StepName { get; set; }
    }

    public class ProductionScheduleInfoModel : IMapFrom<ProductionScheduleEntity>
    {
        public long ProductionScheduleId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal ProductionScheduleQuantity { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionScheduleEntity, ProductionScheduleInfoModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()));
        }
    }
}
