
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeModel: IMapFrom<VoucherType>
    {
        public VoucherTypeModel()
        {
        }

        public int VoucherTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên chứng từ")]
        [MaxLength(256, ErrorMessage = "Tên chứng từ quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã chứng từ")]
        [MaxLength(45, ErrorMessage = "Mã chứng từ quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chứng từ chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string VoucherTypeCode { get; set; }

        public int SortOrder { get; set; }
        public int? VoucherTypeGroupId { get; set; }
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
