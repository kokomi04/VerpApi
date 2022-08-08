﻿//using AutoMapper;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;
//using VErp.Commons.GlobalObject;
//using VErp.Infrastructure.EF.ManufacturingDB;

//namespace VErp.Services.Manafacturing.Model.Outsource.Order
//{
//    public class OutsourceStepOrderInput: OutsourceOrderModel, IMapFrom<OutsourceOrder>
//    {
//        public IList<OutsourceStepOrderDetailInput> OutsourceOrderDetail { get; set; }
//        public IList<OutsourceOrderMaterialsModel> OutsourceOrderMaterials { get; set; }
//        public IList<OutsourceOrderExcessModel> OutsourceOrderExcesses { get; set; }
//    }

//    public class OutsourceStepOrderDetailInput: IMapFrom<OutsourceOrderDetail>
//    {
//        public long OutsourceOrderDetailId { get; set; }
//        public long OutsourceOrderId { get; set; }
//        public decimal OutsourceOrderQuantity { get; set; }
//        public decimal OutsourceOrderPrice { get; set; }
//        public decimal OutsourceOrderTax { get; set; }
//        public long ProductionStepLinkDataId { get; set; }

//        public int? OutsourceOrderProductUnitConversionId { get; set; }
//        public decimal? OutsourceOrderProductUnitConversionQuantity { get; set; }
//        public decimal? OutsourceOrderProductUnitConversionPrice { get; set; }

//        public void Mapping(Profile profile)
//        {
//            profile.CreateMapIgnoreNoneExist<OutsourceOrderDetail, OutsourceStepOrderDetailInput>()
//                .ForMember(m => m.ProductionStepLinkDataId, v => v.MapFrom(m => m.ObjectId))
//                .ForMember(m => m.OutsourceOrderPrice, v => v.MapFrom(m => m.Price))
//                .ForMember(m => m.OutsourceOrderQuantity, v => v.MapFrom(m => m.Quantity))
//                .ForMember(m => m.OutsourceOrderTax, v => v.MapFrom(m => m.Tax))
//                .ForMember(m => m.OutsourceOrderProductUnitConversionPrice, v => v.MapFrom(m => m.ProductUnitConversionPrice))
//                .ForMember(m => m.OutsourceOrderProductUnitConversionQuantity, v => v.MapFrom(m => m.ProductUnitConversionQuantity))
//                .ForMember(m => m.OutsourceOrderProductUnitConversionId, v => v.MapFrom(m => m.ProductUnitConversionId))
//                .ReverseMapIgnoreNoneExist()
//                .ForMember(m => m.ObjectId, v => v.MapFrom(m => m.ProductionStepLinkDataId))
//                .ForMember(m => m.Price, v => v.MapFrom(m => m.OutsourceOrderPrice))
//                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.OutsourceOrderQuantity))
//                .ForMember(m => m.Tax, v => v.MapFrom(m => m.OutsourceOrderTax))
//                .ForMember(m => m.ProductUnitConversionPrice, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionPrice))
//                .ForMember(m => m.ProductUnitConversionQuantity, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionQuantity))
//                .ForMember(m => m.ProductUnitConversionId, v => v.MapFrom(m => m.OutsourceOrderProductUnitConversionId));

//        }
//    }
//}
