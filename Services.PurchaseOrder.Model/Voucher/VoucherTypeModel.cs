
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeSimpleProjectMappingModel : VoucherTypeSimpleModel, IMapFrom<VoucherType>
    {
       
    }

    public class VoucherTypeModel: VoucherTypeSimpleProjectMappingModel
    {
        public VoucherTypeModel()
        {
        }
   
        public string PreLoadAction { get; set; }
        public string PostLoadAction { get; set; }
        public string AfterLoadAction { get; set; }
        public string BeforeSubmitAction { get; set; }
        public string BeforeSaveAction { get; set; }
        public string AfterSaveAction { get; set; }
    }

    public class VoucherTypeFullModel : VoucherTypeModel
    {
        public VoucherTypeFullModel()
        {
            VoucherAreas = new List<VoucherAreaModel>();
        }
        public ICollection<VoucherAreaModel> VoucherAreas { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherType, VoucherTypeFullModel>()
                .ForMember(dest => dest.VoucherAreas, opt => opt.MapFrom(src => src.VoucherArea));
        }
    }
}
