using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeViewModelList : IMapFrom<VoucherTypeView>
    {
        public int VoucherTypeViewId { get; set; }
        public string VoucherTypeViewName { get; set; }
        public bool IsDefault { get; set; }
        public int Columns { get; set; }
        public int InputTypeGroupId { get; set; }
    }

    public class VoucherTypeViewModel : VoucherTypeViewModelList
    {
        public IList<VoucherTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherTypeView, VoucherTypeViewModel>()
                .ForMember(d => d.Fields, m => m.Ignore())
                .ReverseMap()
                .ForMember(d => d.VoucherTypeViewField, m => m.Ignore())
                .ForMember(d => d.VoucherType, m => m.Ignore());
        }
    }

    public class VoucherTypeViewFieldModel : IMapFrom<VoucherTypeViewField>
    {
        public int VoucherTypeViewFieldId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string SelectFilters { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<VoucherTypeViewField, VoucherTypeViewFieldModel>()
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (EnumDataType)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (EnumFormType)s.FormTypeId))
            .ReverseMap()
            .ForMember(m => m.VoucherTypeView, m => m.Ignore())
            .ForMember(m => m.DataTypeId, m => m.MapFrom(s => (int)s.DataTypeId))
            .ForMember(m => m.FormTypeId, m => m.MapFrom(s => (int)s.FormTypeId));
    }
}
