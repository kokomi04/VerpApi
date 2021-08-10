using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model.ProductPrice
{
    public class ProductPriceConfigVersionModel : IMapFrom<ProductPriceConfigVersion>
    {

        public int ProductPriceConfigVersionId { get; set; }
        public int? ProductPriceConfigId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EvalSourceCodeJs { get; set; }
        public string Fields { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }

        public IList<ProductPriceConfigItemModel> Items { get; set; }

        public bool IsActived { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductPriceConfigVersionModel, ProductPriceConfigVersion>()
                .ForMember(d => d.ProductPriceConfigItem, s => s.Ignore())
                .ForMember(d => d.UpdatedByUserId, s => s.Ignore())
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.Ignore())
                .ReverseMap()
                .ForMember(d => d.Items, s => s.Ignore())
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.MapFrom(v=>v.UpdatedDatetimeUtc.GetUnix()));
        }
    }

    public class ProductPriceConfigItemModel : IMapFrom<ProductPriceConfigItem>
    {
        public int ProductPriceConfigItemId { get; set; }
        public int ProductPriceConfigVersionId { get; set; }
        public string ItemKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public bool isTable { get; set; }
        public string TableConfig { get; set; }
        public bool? IsEditable { get; set; }
        public string OnChange { get; set; }


    }
}
