using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Stock.Model.Product
{
    public class PropertyModel : PropertyInfoModel, IMapFrom<Property>
    {
        
    }
}
