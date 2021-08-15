using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Stock;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;

namespace VErp.Services.Stock.Model.StockTake
{
    public class StockTakeAcceptanceCertificateModel : IMapFrom<StockTakeAcceptanceCertificate>
    {
        public long StockTakePeriodId { get; set; }
        public string StockTakeAcceptanceCertificateCode { get; set; }
        public long StockTakeAcceptanceCertificateDate { get; set; }
        public EnumStockTakeAcceptanceCertificateStatus StockTakeAcceptanceCertificateStatus { get; set; }
        public string Content { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<StockTakeAcceptanceCertificate, StockTakeAcceptanceCertificateModel>()
                .ForMember(dest => dest.StockTakeAcceptanceCertificateDate, opt => opt.MapFrom(x => x.StockTakeAcceptanceCertificateDate.GetUnix()))
                .ForMember(dest => dest.StockTakeAcceptanceCertificateStatus, opt => opt.MapFrom(x => (EnumStockTakeAcceptanceCertificateStatus)x.StockTakeAcceptanceCertificateStatus))
                .ReverseMap()
                .ForMember(dest => dest.StockTakeAcceptanceCertificateDate, opt => opt.MapFrom(x => x.StockTakeAcceptanceCertificateDate.UnixToDateTime()))
                .ForMember(dest => dest.StockTakeAcceptanceCertificateStatus, opt => opt.MapFrom(x => (int)x.StockTakeAcceptanceCertificateStatus));
        }
    }

    public class ConfirmAcceptanceCertificateModel
    {
        public EnumStockTakeAcceptanceCertificateStatus StockTakeAcceptanceCertificateStatus { get; set; }

    }
}
