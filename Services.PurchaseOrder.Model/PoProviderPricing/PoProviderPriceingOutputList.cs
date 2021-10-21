using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using PoProviderPricingEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PoProviderPricing;

namespace VErp.Services.PurchaseOrder.Model.PoProviderPricing
{


    public class PoProviderPricingOutputList : IMapFrom<PoProviderPricingEntity>
    {
        public long PoProviderPricingId { get; set; }
        public string PoProviderPricingCode { get; set; }
        public long? Date { get; set; }
        public int CustomerId { get; set; }
        public DeliveryDestinationModel DeliveryDestination { get; set; }
        public string Content { get; set; }
        public string AdditionNote { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalMoney { get; set; }
        public EnumPoProviderPricingStatus? PoProviderPricingStatusId { get; set; }
        public bool? IsChecked { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public string PoProviderPricingDescription { get; set; }
        public decimal? TaxInPercent { get; set; }
        public decimal? TaxInMoney { get; set; }

        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CheckedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public long? CheckedDatetimeUtc { get; set; }

        public string CreatorFullName { get; set; }
        public string CheckerFullName { get; set; }
        public string CensorFullName { get; set; }


        public long? DeliveryDate { get; set; }

        public long? CurrencyId { get; set; }
        public decimal? ExchangeRate { get; set; }

        protected void MappingBase<T>(Profile profile) where T : PoProviderPricingOutputList
        {
            profile.CreateMap<PoProviderPricingEntity, T>()
                .ForMember(d => d.DeliveryDestination, s => s.MapFrom(f => f.DeliveryDestination.JsonDeserialize<DeliveryDestinationModel>()))
                .ForMember(d => d.PoProviderPricingStatusId, s => s.MapFrom(f => (EnumPoProviderPricingStatus?)f.PoProviderPricingStatusId))
                .ForMember(d => d.PoProcessStatusId, s => s.MapFrom(f => (EnumPoProcessStatus?)f.PoProcessStatusId))
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(f => f.CreatedDatetimeUtc.GetUnix()))
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.MapFrom(f => f.UpdatedDatetimeUtc.GetUnix()))
                .ForMember(d => d.CensorDatetimeUtc, s => s.MapFrom(f => f.CensorDatetimeUtc.GetUnix()))
                .ForMember(d => d.CheckedDatetimeUtc, s => s.MapFrom(f => f.CheckedDatetimeUtc.GetUnix()))
                .ForMember(d => d.DeliveryDate, s => s.MapFrom(f => f.DeliveryDate.GetUnix()))
                .ForMember(d => d.Date, s => s.MapFrom(f => f.Date.GetUnix()));
        }

        public virtual void Mapping(Profile profile)
        {
            MappingBase<PoProviderPricingOutputList>(profile);
        }
    }

    public class PoProviderPricingModel : PoProviderPricingOutputList, IMapFrom<PoProviderPricingEntity>
    {
        public string PaymentInfo { get; set; }

        public int? DeliveryUserId { get; set; }
        public int? DeliveryCustomerId { get; set; }

        public IList<long> FileIds { get; set; }
        public IList<PoProviderPricingOutputDetail> Details { get; set; }

        public override void Mapping(Profile profile)
        {
            MappingBase<PoProviderPricingModel>(profile);

            profile.CreateMap<PoProviderPricingModel, PoProviderPricingEntity>()
               .ForMember(d => d.DeliveryDestination, s => s.MapFrom(f => f.DeliveryDestination.JsonSerialize()))
               .ForMember(d => d.PoProviderPricingStatusId, s => s.MapFrom(f => (int?)f.PoProviderPricingStatusId))
               .ForMember(d => d.PoProcessStatusId, s => s.MapFrom(f => (int?)f.PoProcessStatusId))
               .ForMember(d => d.DeliveryDate, s => s.MapFrom(f => f.DeliveryDate.UnixToDateTime()))
               .ForMember(d => d.Date, s => s.MapFrom(f => f.Date.UnixToDateTime()))

                .ForMember(d => d.PoProcessStatusId, s => s.Ignore())
                .ForMember(d => d.CreatedDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.CensorDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.CheckedDatetimeUtc, s => s.Ignore())
                .ForMember(d => d.PoProviderPricingDetail, s => s.Ignore())
                .ForMember(d => d.PoProviderPricingFile, s => s.Ignore());
        }

    }

    public class PoProviderPricingOutputDetail : IMapFrom<PoProviderPricingDetail>
    {
        public long? PoProviderPricingDetailId { get; set; }

        public string ProviderProductName { get; set; }

        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }

        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }

        public string Description { get; set; }

        public decimal? IntoMoney { get; set; }

        public decimal? ExchangedMoney { get; set; }
        public int? SortOrder { get; set; }
    }

    public class PoProviderPricingOutputListByProduct : PoProviderPricingOutputList
    {
        public long? PoProviderPricingDetailId { get; set; }
        public string ProviderProductName { get; set; }
        public int ProductId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal PrimaryUnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionPrice { get; set; }

        public string OrderCode { get; set; }
        public string ProductionOrderCode { get; set; }

        public string Description { get; set; }

        public decimal? IntoMoney { get; set; }

        public decimal? ExchangedMoney { get; set; }
        public int? SortOrder { get; set; }

    }
}
