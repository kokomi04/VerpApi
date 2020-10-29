using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Enums.MasterEnum;
using AutoMapper;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherActionSimpleProjectMappingModel : VoucherActionSimpleModel, IMapFrom<VoucherAction>
    {
       
    }

    public class VoucherActionModel : VoucherActionSimpleProjectMappingModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã chức năng")]
        [MaxLength(45, ErrorMessage = "Mã chức năng quá dài")]
        [RegularExpression(@"(^[a-zA-Z0-9_]*$)", ErrorMessage = "Mã chức năng chỉ gồm các ký tự chữ, số và ký tự _.")]
        public string VoucherActionCode { get; set; }
      
        public string SqlAction { get; set; }
        public string JsAction { get; set; }
        public string IconName { get; set; }
        public string Style { get; set; }
        public EnumPosition Position { set; get; }

        protected void MappingBase<T>(Profile profile) where T : VoucherActionModel
        {
            profile.CreateMap<VoucherAction, T>()
                .ForMember(a => a.Position, m => m.MapFrom(a => (EnumPosition)a.Position))
                .ReverseMap()
                .ForMember(a => a.Position, m => m.MapFrom(a => (int)a.Position));

        }

        public void Mapping(Profile profile)
        {
            MappingBase<VoucherActionModel>(profile);
        }
    }
}
