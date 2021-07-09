using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace VErp.Services.Stock.Model.Product
{
    public class ProductPropertyModel: IMapFrom<ProductProperty>
    {
        public int ProductPropertyId { get; set; }
        public string ProductPropertyName { get; set; }
    }
}
