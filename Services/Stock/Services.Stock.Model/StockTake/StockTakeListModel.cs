using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using StockTakeEntity = VErp.Infrastructure.EF.StockDB.StockTake;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakeListModel : IMapFrom<StockTakeEntity>
    {
        public long StockTakeId { get; set; }
        public long StockTakePeriodId { get; set; }
        public string StockTakeCode { get; set; }
        public long StockTakeDate { get; set; }
        public int StockRepresentativeId { get; set; }
        public string Content { get; set; }
        public int AccountancyRepresentativeId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StockTakeEntity, StockTakeListModel>()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.GetUnix()));
        }
    }


    public class StockTakeModel : StockTakeListModel
    {
        public ICollection<StockTakeDetailModel> StockTakeDetail { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StockTakeEntity, StockTakeModel>()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.GetUnix()))
                .ForMember(dest => dest.StockTakeDetail, opt => opt.MapFrom(x => x.StockTakeDetail))
                .ReverseMap()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.UnixToDateTime()))
                .ForMember(dest => dest.StockTakeDetail, opt => opt.Ignore());
        }
    }

}