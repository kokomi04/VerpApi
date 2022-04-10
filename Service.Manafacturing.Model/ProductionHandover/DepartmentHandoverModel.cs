using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Services.Manafacturing.Model.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class DepartmentHandoverModel : IMapFrom<DepartmentHandoverEntity>
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepName { get; set; }
        public long GroupId { get; set; }
        public int StepId { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public string InOutType { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoveredQuantity { get; set; }
        public EnumAssignedProgressStatus AssignedProgressStatus { get; set; }

        public long ToProductionStepId { get; set; }
        public long FromProductionStepId { get; set; }
        
        public void Mapping(Profile profile)
        {
            profile.CreateMap<DepartmentHandoverEntity, DepartmentHandoverModel>()
                .ForMember(m => m.StartDate, v => v.MapFrom(m => m.StartDate.GetUnix()))
                .ForMember(m => m.EndDate, v => v.MapFrom(m => m.EndDate.GetUnix()))
                .ForMember(m => m.AssignedProgressStatus, v => v.MapFrom(m => (EnumAssignedProgressStatus)m.AssignedProgressStatus));
        }
    }

    public class DepartmentHandoverEntity
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
        public long ProductionStepId { get; set; }
        public string ProductionStepName { get; set; }
        public long GroupId { get; set; }
        public int StepId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string InOutType { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public decimal HandoveredQuantity { get; set; }
        public int AssignedProgressStatus { get; set; }

        public long ToProductionStepId { get; set; }
        public long FromProductionStepId { get; set; }
    }

    public class DepartmentHandoverDetailModel
    {
        public long ProductionStepId { get; set; }
        public int DepartmentId { get; set; }
        public IList<StepInOutData> InputDatas { get; set; }
        public IList<StepInOutData> OutputDatas { get; set; }
        public IList<ProductionAssignmentModel> AdjacentAssignments { get; set; }
        public DepartmentHandoverDetailModel()
        {
            InputDatas = new List<StepInOutData>();
            OutputDatas = new List<StepInOutData>();
            AdjacentAssignments = new List<ProductionAssignmentModel>();
        }
    }

    public class StepInOutData
    {
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RequireQuantity { get; set; }
        public decimal TotalQuantity { get; set; }
        public string FromStepTitle { get; set; }
        public long? FromStepId { get; set; }
        public string ToStepTitle { get; set; }
        public long? ToStepId { get; set; }
        public long HandoverDatetime { get; set; }

        public long? OutsourceStepRequestId { get; set; }

        public IList<ProductionHandoverModel> HandoverHistories { get; set; }
        public IList<ProductionInventoryRequirementModel> InventoryRequirementHistories { get; set; }
        public IList<ProductionMaterialsRequirementDetailListModel> MaterialsRequirementHistories { get; set; }

        public StepInOutData()
        {
            HandoverHistories = new List<ProductionHandoverModel>();
            InventoryRequirementHistories = new List<ProductionInventoryRequirementModel>();
            MaterialsRequirementHistories = new List<ProductionMaterialsRequirementDetailListModel>();
        }
    }
}
