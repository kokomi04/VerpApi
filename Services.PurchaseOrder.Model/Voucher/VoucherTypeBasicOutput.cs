using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.Voucher
{
    public class VoucherTypeBasicOutput : IMapFrom<VoucherType>
    {
        public string Title { get; set; }
        public string VoucherTypeCode { get; set; }
        public IList<VoucherAreaBasicOutput> Areas { get; set; }
        public IList<VoucherTypeViewModelList> Views { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherType, VoucherTypeBasicOutput>()
                .ForMember(d => d.Areas, m => m.Ignore())
                .ForMember(d => d.Views, m => m.Ignore());
        }
    }

    public class VoucherAreaBasicOutput : IMapFrom<VoucherArea>
    {
        public int VoucherAreaId { get; set; }
        public string Title { get; set; }
        public string InputAreaCode { get; set; }
        public bool IsMultiRow { get; set; }
        public int Columns { get; set; }
        public string ColumnStyles { get; set; }
        public IList<VoucherAreaFieldBasicOutput> Fields { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherArea, VoucherAreaBasicOutput>()
                .ForMember(d => d.Fields, m => m.Ignore());
        }
    }

    public class VoucherAreaFieldBasicOutput
    {
        public int VoucherAreaId { get; set; }
        public int VoucherAreaFieldId { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DataSize { get; set; }
        public EnumFormType FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
    }
}
