using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingWorkSheetModel : IMapFrom<CuttingWorkSheet>
    {
        public long CuttingWorkSheetId { get; set; }
        public long PropertyCalcId { get; set; }
        public int InputProductId { get; set; }
        public decimal InputQuantity { get; set; }
        public ICollection<CuttingWorkSheetDestModel> CuttingWorkSheetDest { get; set; }
        public ICollection<CuttingExcessMaterialModel> CuttingExcessMaterial { get; set; }
        public ICollection<CuttingWorkSheetFileModel> CuttingWorkSheetFile { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuttingWorkSheet, CuttingWorkSheetModel>()
                .ForMember(dest => dest.CuttingWorkSheetDest, opt => opt.MapFrom(x => x.CuttingWorkSheetDest))
                .ForMember(dest => dest.CuttingExcessMaterial, opt => opt.MapFrom(x => x.CuttingExcessMaterial))
                .ForMember(dest => dest.CuttingWorkSheetFile, opt => opt.MapFrom(x => x.CuttingWorkSheetFile))
                .ReverseMap()
                .ForMember(dest => dest.CuttingWorkSheetDest, opt => opt.Ignore())
                .ForMember(dest => dest.CuttingWorkSheetFile, opt => opt.Ignore());
        }
    }
}
