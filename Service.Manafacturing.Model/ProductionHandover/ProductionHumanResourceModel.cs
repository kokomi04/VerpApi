using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHumanResourceModel : ProductionHumanResourceInputModel
    {
        public int CreatedByUserId { get; set; }
    }

    public class ProductionHumanResourceInputModel : IMapFrom<ProductionHumanResource>
    {
        public long? ProductionHumanResourceId { get; set; }
        public decimal OfficeWorkDay { get; set; }
        public decimal OvertimeWorkDay { get; set; }
        public int DepartmentId { get; set; }
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public string ProductionStepTitle { get; set; }
        public string ProductionOrderCode { get; set; }
        public long? Date { get; set; }
        public string Note { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionHumanResourceInputModel, ProductionHumanResource>()
                .ForMember(d => d.ProductionOrder, s => s.Ignore())
                .ForMember(d => d.ProductionStep, s => s.Ignore())
                .ReverseMap();
        }
    }
}
