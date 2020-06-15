﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Model
{
    public class PurchasingRequestOutputList : IMapFrom<PurchasingRequest>
    {
        public long PurchasingRequestId { get; set; }
        public string PurchasingRequestCode { get; set; }
        public long Date { get; set; }
        public string Content { get; set; }
        public int RejectCount { get; set; }
        public EnumPurchasingRequestStatus PurchasingRequestStatusId { get; set; }
        public bool? IsApproved { get; set; }
        public EnumPoProcessStatus? PoProcessStatusId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }
        public int? CensorByUserId { get; set; }

        public long CreatedDatetimeUtc { get; set; }
        public long UpdatedDatetimeUtc { get; set; }
        public long? CensorDatetimeUtc { get; set; }

        protected IMappingExpression<PurchasingRequest, T> MappingBase<T>(Profile profile) where T : PurchasingRequestOutputList => profile.CreateMap<PurchasingRequest, T>()
          .ForMember(m => m.Date, m => m.MapFrom(v => v.Date.GetUnix()))
          .ForMember(m => m.PurchasingRequestStatusId, m => m.MapFrom(v => (EnumPurchasingRequestStatus)v.PurchasingRequestStatusId))
          .ForMember(m => m.PoProcessStatusId, m =>
              m.MapFrom(v => v.PoProcessStatusId.HasValue ? (EnumPoProcessStatus?)v.PoProcessStatusId : null)
          )
          .ForMember(m => m.CreatedDatetimeUtc, m => m.MapFrom(v => v.CreatedDatetimeUtc.GetUnix()))
          .ForMember(m => m.UpdatedDatetimeUtc, m => m.MapFrom(v => v.UpdatedDatetimeUtc.GetUnix()))
          .ForMember(m => m.CensorDatetimeUtc, m => m.MapFrom(v => v.CensorDatetimeUtc.GetUnix()));

        public void Mapping(Profile profile)
        {
            MappingBase<PurchasingRequestOutputList>(profile);
        }
    }


    public class PurchasingRequestOutput : PurchasingRequestOutputList
    {
        public IList<long> FileIds { get; set; }
        public List<PurchasingRequestOutputDetail> Details { set; get; }

        public new void Mapping(Profile profile)
        {
            MappingBase<PurchasingRequestOutput>(profile)
                .ForMember(m => m.Details, m => m.Ignore());
        }
    }


    public class PurchasingRequestOutputDetail : PurchasingRequestInputDetail
    {
        public long PurchasingRequestDetailId { get; set; }
        public new void Mapping(Profile profile) => MappingBase<PurchasingRequestOutputDetail>(profile);        
    }
}
