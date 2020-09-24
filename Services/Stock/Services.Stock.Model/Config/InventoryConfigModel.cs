using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.Config
{
    public class InventoryConfigModel : ModuleConfig, IMapFrom<InventoryConfig>
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMap<InventoryConfig, InventoryConfigModel>()
                .ForMember(m => m.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonDeserialize<FreqClosingDate>()))
                .ReverseMap()
                .ForMember(d => d.FreqClosingDate, m => m.MapFrom(v => v.FreqClosingDate.JsonSerialize()));
        }
    }
}
