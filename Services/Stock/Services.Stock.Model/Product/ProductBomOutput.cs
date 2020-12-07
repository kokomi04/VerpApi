using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductBomOutputBase
    {
        public long? ProductBomId { get; set; }
        public int Level { get; set; }
        public int ProductId { get; set; }
        public int? ChildProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }
        public string Specification { get; set; }

        public decimal Quantity { get; set; }
        public decimal Wastage { get; set; }
        public string Description { get; set; }
        public string UnitName { get; set; }
        public bool IsMaterial { get; set; }
        public string NumberOrder { get; set; }
    }
    public class ProductBomEntity : ProductBomOutputBase
    {
        public string PathProductIds { get; set; }
    }

    public class ProductBomOutput : ProductBomOutputBase, IMapFrom<ProductBomEntity>
    {
        public int[] PathProductIds { get; set; }
        public virtual void Mapping(Profile profile)
        {
            profile.CreateMap<ProductBomEntity, ProductBomOutput>()
                .ForMember(m => m.PathProductIds, v => v.Ignore());
        }
    }
}
