﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Stock;
using VErp.Commons.GlobalObject;
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
        public EnumStockTakeStatus StockStatus { get; set; }
        public EnumStockTakeStatus AccountancyStatus { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<StockTakeEntity, StockTakeListModel>()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.GetUnix()))
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(x => (EnumStockTakeStatus)x.StockStatus))
                .ForMember(dest => dest.AccountancyStatus, opt => opt.MapFrom(x => (EnumStockTakeStatus)x.AccountancyStatus));
        }
    }


    public class StockTakeModel : StockTakeListModel
    {
        public ICollection<StockTakeDetailModel> StockTakeDetail { get; set; }

        public new void Mapping(Profile profile)
        {
            profile.CreateMapIgnoreNoneExist<StockTakeEntity, StockTakeModel>()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.GetUnix()))
                .ForMember(dest => dest.StockTakeDetail, opt => opt.MapFrom(x => x.StockTakeDetail))
                .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(x => (EnumStockTakeStatus)x.StockStatus))
                .ForMember(dest => dest.AccountancyStatus, opt => opt.MapFrom(x => (EnumStockTakeStatus)x.AccountancyStatus))
                .ReverseMapIgnoreNoneExist()
                .ForMember(dest => dest.StockTakeDate, opt => opt.MapFrom(x => x.StockTakeDate.UnixToDateTime()))
                .ForMember(dest => dest.StockTakeDetail, opt => opt.Ignore())
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore())
                .ForMember(dest => dest.AccountancyStatus, opt => opt.Ignore());
        }
    }

}