using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.PurchaseOrder
{
    public class CuttingWorkSheetSourceModel : IMapFrom<CuttingWorkSheet>
    {
        public long CuttingWorkSheetId { get; set; }
        public long PropertyCalcId { get; set; }
        public int ProductId { get; set; }
        public decimal ProductQuantity { get; set; }
        public ICollection<CuttingWorkSheetDestModel> CuttingWorkSheetDest { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<CuttingWorkSheet, CuttingWorkSheetSourceModel>()
                .ForMember(dest => dest.CuttingWorkSheetDest, opt => opt.MapFrom(x => x.CuttingWorkSheetDest))
                .ReverseMap()
                .ForMember(dest => dest.CuttingWorkSheetDest, opt => opt.Ignore());
        }
    }
}
