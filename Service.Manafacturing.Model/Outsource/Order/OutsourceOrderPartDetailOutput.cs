﻿//using AutoMapper;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using VErp.Commons.GlobalObject;

//namespace VErp.Services.Manafacturing.Model.Outsource.Order
//{
//    public class OutsourceOrderPartDetailOutput : OutsourceOrderModel, IMapFrom<OutsourceOrderDetailInfo>
//    {
//        public long OutsourceOrderDetailId { get; set; }
//        public long ObjectId { get; set; }
//        public decimal Price { get; set; }
//        public decimal Tax { get; set; }
//        public string ProductTitle { get; set; }
//        public string ProductPartName { get; set; }
//        public string ProductPartCode { get; set; }
//        public string ProductionOrderCode { get; set; }
//        public string OrderCode { get; set; }
//        public string OutsourcePartRequestCode { get; set; }
//        public string UnitName { get; set; }
//        public decimal Quantity { get; set; }
//        public decimal QuantityOrigin { get; set; }
//        public decimal QuantityProcessed { get; set; }
//        public long OutsourcePartRequestFinishDate { get; set; }

//        public new void Mapping(Profile profile)
//        {
//            profile.CreateMapIgnoreNoneExist<OutsourceOrderDetailInfo, OutsourceOrderPartDetailOutput>()
//                .ForMember(m => m.OutsourceOrderDetailId, v => v.MapFrom(m => m.OutsourceOrderDetailId))
//                .ForMember(m => m.OutsourceOrderId, v => v.MapFrom(m => m.OutsourceOrderId))
//                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ObjectId))
//                .ForMember(m => m.Price, v => v.MapFrom(m => m.Price))
//                .ForMember(m => m.Tax, v => v.MapFrom(m => m.Tax))
//                .ForMember(m => m.ProductPartName, v => v.MapFrom(m => m.ProductPartName))
//                .ForMember(m => m.ProductPartCode, v => v.MapFrom(m => m.ProductPartCode))
//                .ForMember(m => m.OutsourcePartRequestCode, v => v.MapFrom(m => m.OutsourcePartRequestCode))
//                .ForMember(m => m.OutsourcePartRequestFinishDate , v => v.MapFrom(m => m.OutsourcePartRequestDetailFinishDate))
//                .ForMember(m => m.UnitName, v => v.MapFrom(m => m.UnitName))
//                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.Quantity))
//                .ReverseMapIgnoreNoneExist();
//        }
//    }
//}
