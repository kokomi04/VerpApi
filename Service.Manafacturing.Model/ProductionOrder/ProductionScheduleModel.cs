﻿//using AutoMapper;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using VErp.Commons.Enums.Manafacturing;
//using VErp.Commons.GlobalObject;
//using VErp.Infrastructure.EF.ManufacturingDB;
//using VErp.Commons.Library;

//namespace VErp.Services.Manafacturing.Model.ProductionOrder
//{
//    public class ProductionScheduleModel : ProductionPlanningOrderDetailModel, IMapFrom<ProductionScheduleEntity>
//    {
//        public long ProductionScheduleId { get; set; }
//        public long StartDate { get; set; }
//        public long EndDate { get; set; }
//        public EnumScheduleStatus ProductionScheduleStatus { get; set; }
//        public decimal ProductionScheduleQuantity { get; set; }
//        public string ProductionOrderCode { get; set; }
//        public long? ScheduleTurnId { get; set; }
//        public bool HasAssignment { get; set; }
//        public string ScheduleCode { get; set; }
//        public void Mapping(Profile profile)
//        {
//            profile.CreateMapIgnoreNoneExist<ProductionScheduleEntity, ProductionScheduleModel>()
//                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.GetUnix()))
//                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.GetUnix()))
//                .ForMember(dest => dest.ProductionScheduleStatus, opt => opt.MapFrom(source => (EnumScheduleStatus)source.ProductionScheduleStatus));
//        }
//    }

//    public class ProductionScheduleEntity : ProductionPlanningOrderDetailModel
//    {
//        public long ProductionScheduleId { get; set; }
//        public DateTime StartDate { get; set; }
//        public DateTime EndDate { get; set; }
//        public int ProductionScheduleStatus { get; set; }
//        public decimal ProductionScheduleQuantity { get; set; }
//        public string ProductionOrderCode { get; set; }
//        public long? ScheduleTurnId { get; set; }
//        public bool HasAssignment { get; set; }
//        public string ScheduleCode { get; set; }
//    }

//    public class ProductionScheduleInputModel : IMapFrom<ProductionSchedule>
//    {
//        public long? ProductionScheduleId { get; set; }
//        public long ProductionOrderDetailId { get; set; }
//        public long StartDate { get; set; }
//        public long EndDate { get; set; }
//        public decimal ProductionScheduleQuantity { get; set; }
//        public long? ScheduleTurnId { get; set; }
//        public EnumScheduleStatus ProductionScheduleStatus { get; set; }
//        public string ScheduleCode { get; set; }
//        public void Mapping(Profile profile)
//        {
//            profile.CreateMapIgnoreNoneExist<ProductionScheduleInputModel, ProductionSchedule>()
//                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.StartDate.UnixToDateTime()))
//                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(source => source.EndDate.UnixToDateTime()))
//                .ForMember(dest => dest.ProductionScheduleStatus, opt => opt.MapFrom(source => (int)source.ProductionScheduleStatus));
//        }
//    }

//    public class ProductionScheduleStatusModel 
//    {
//        public EnumScheduleStatus ProductionScheduleStatus { get; set; }
//    }

//}
