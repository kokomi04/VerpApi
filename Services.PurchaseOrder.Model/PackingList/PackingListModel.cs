using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using PackingListEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PackingList;

namespace VErp.Services.PurchaseOrder.Model.PackingList
{
    public class PackingListModel : IMapFrom<PackingListEntity>
    {
        public int PackingListId { get; set; }
        public int VoucherBillId { get; set; }
        public string ContSealNo { get; set; }
        public string PackingNote { get; set; }
        public IList<PackingListDetailModel> Details { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PackingListEntity, PackingListModel>()
                .ForMember(d => d.Details, m => m.MapFrom(v => v.PackingListDetail))
                .ReverseMap()
                .ForMember(d => d.PackingListDetail, m => m.Ignore());
        }
    }

    public class PackingListInput
    {
        public List<PackingListModel> PackingLists { get; set; }
    }
}
