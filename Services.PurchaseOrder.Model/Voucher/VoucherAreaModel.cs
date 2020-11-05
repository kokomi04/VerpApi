using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherAreaInputModel : IMapFrom<VoucherArea>
    {
        public int VoucherAreaId { get; set; }
        public int VoucherTypeId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên vùng dữ liệu")]
        [MaxLength(256, ErrorMessage = "Tên vùng dữ liệu quá dài")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mã vùng dữ liệu")]
        [MaxLength(45, ErrorMessage = "Vùng dữ liệu quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã vùng dữ liệu chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string VoucherAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng cột hiển thị")]
        public int Columns { get; set; }
        public string ColumnStyles { get; set; }
        public int SortOrder { get; set; }
    }

    public class VoucherAreaModel : VoucherAreaInputModel
    {
        public VoucherAreaModel()
        {
            VoucherAreaFields = new List<VoucherAreaFieldOutputFullModel>();
        }
        public ICollection<VoucherAreaFieldOutputFullModel> VoucherAreaFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherArea, VoucherAreaModel>()
                .ForMember(dest => dest.VoucherAreaFields, opt => opt.MapFrom(src => src.VoucherAreaField));
        }
    }



}
