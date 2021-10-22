﻿using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class VoucherOrderDetailSimpleModel : VoucherOrderDetailSimple, IMapFrom<VoucherOrderDetailSimpleEntity>
    {
        public long DeliveryDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<VoucherOrderDetailSimpleEntity, VoucherOrderDetailSimpleModel>()
            .ForMember(m => m.DeliveryDate, v => v.MapFrom(m => GetUnix(m.DeliveryDate)))
            .ReverseMap()
            .ForMember(m => m.DeliveryDate, v => v.Ignore());
        }

        public long GetUnix(DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }

    public class VoucherOrderDetailSimple
    {
        public long VoucherTypeId { get; set; }
        public long OrderId { get; set; }
        public string OrderCode { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerId { get; set; }
        public string CustomerPO { get; set; }
        public int ContainerQuantity { get; set; }
    }

    public class VoucherOrderDetailSimpleEntity : VoucherOrderDetailSimple
    {
        public DateTime DeliveryDate { get; set; }
    }


}