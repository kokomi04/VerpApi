using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakePeriotListModel : IMapFrom<StockTakePeriod>
    {

        public long StockTakePeriodId { get; set; }
        public string StockTakePeriodCode { get; set; }
        public long StockTakePeriodDate { get; set; }
        public long FinishDate { get; set; }
        public int StockId { get; set; }
        public int Status { get; set; }
        public string Content { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMap<StockTakePeriod, StockTakePeriotListModel>()
                .ForMember(dest => dest.StockTakePeriodDate, opt => opt.MapFrom(x => x.StockTakePeriodDate.GetUnix()));
        }
    }

    public class StockTakePeriotModel : StockTakePeriotListModel
    {
        public ICollection<StockTakeListModel> StockTake { get; set; }
        public ICollection<StockTakeRepresentativeModel> StockTakeRepresentative { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StockTakePeriod, StockTakePeriotModel>()
                .ForMember(dest => dest.StockTakePeriodDate, opt => opt.MapFrom(x => x.StockTakePeriodDate.GetUnix()))
                .ForMember(dest => dest.StockTake, opt => opt.MapFrom(x => x.StockTake))
                .ForMember(dest => dest.StockTakeRepresentative, opt => opt.MapFrom(x => x.StockTakeRepresentative))
                .ReverseMap()
                .ForMember(dest => dest.StockTakePeriodDate, opt => opt.MapFrom(x => x.StockTakePeriodDate.UnixToDateTime()))
                .ForMember(dest => dest.StockTake, opt => opt.Ignore())
                .ForMember(dest => dest.StockTakeRepresentative, opt => opt.MapFrom(x => x.StockTakeRepresentative));
        }

    }
}
