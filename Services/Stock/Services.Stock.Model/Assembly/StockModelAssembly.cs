﻿using AutoMapper;
using System.Reflection;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Infrastructure.EF.StockDB;
using static VErp.Commons.GlobalObject.InternalDataInterface.Stock.ProductModel;
using ProductEntity = VErp.Infrastructure.EF.StockDB.Product;

namespace VErp.Services.Stock.Model
{
    public static class StockModelAssembly
    {
        public static Assembly Assembly => typeof(StockModelAssembly).Assembly;
    }

    public class CustomMappingModel : ICustomMapping
    {
        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<ProductEntity, ProductModel>()
                .ForMember(d => d.StockInfo, s => s.Ignore())
                .ForMember(d => d.Extra, s => s.Ignore())
                .ForMember(d => d.ProductCustomers, s => s.Ignore())
                .ForMember(d => d.BarcodeStandardId, s => s.MapFrom(s => (EnumBarcodeStandard?)s.BarcodeStandardId))
                .ForMember(d => d.QuantitativeUnitTypeId, s => s.MapFrom(s => (EnumQuantitativeUnitType?)s.QuantitativeUnitTypeId))
                .ReverseMapCustom()
                .ForMember(d => d.ProductCate, s => s.Ignore())
                .ForMember(d => d.ProductType, s => s.Ignore())
                .ForMember(d => d.ProductExtraInfo, s => s.Ignore())
                .ForMember(d => d.ProductStockInfo, s => s.Ignore())
                .ForMember(d => d.InventoryDetail, s => s.Ignore())
                .ForMember(d => d.InventoryRequirementDetail, s => s.Ignore())
                .ForMember(d => d.ProductAttachment, s => s.Ignore())
                .ForMember(d => d.ProductBomChildProduct, s => s.Ignore())
                .ForMember(d => d.ProductBomProduct, s => s.Ignore())
                .ForMember(d => d.ProductCustomer, s => s.Ignore())
                .ForMember(d => d.ProductMaterialsConsumptionMaterialsConsumption, s => s.Ignore())
                .ForMember(d => d.ProductMaterialsConsumptionProduct, s => s.Ignore())
                .ForMember(d => d.ProductStockValidation, s => s.Ignore())
                .ForMember(d => d.ProductUnitConversion, s => s.Ignore())
                .ForMember(d => d.BarcodeStandardId, s => s.MapFrom(s => (int?)s.BarcodeStandardId))
                .ForMember(d => d.QuantitativeUnitTypeId, s => s.MapFrom(s => (int?)s.QuantitativeUnitTypeId));


            profile.CreateMapCustom<ProductExtraInfo, ProductModelExtra>()
               .ReverseMapCustom();


            profile.CreateMapCustom<ProductStockInfo, ProductModelStock>()
                .ForMember(d => d.UnitConversions, s => s.Ignore())
                .ForMember(d => d.StockIds, s => s.Ignore())
                .ForMember(d => d.TimeWarningTimeTypeId, s => s.MapFrom(v => (EnumTimeType?)v.TimeWarningTimeTypeId))
                .ForMember(d => d.StockOutputRuleId, s => s.MapFrom(v => (EnumStockOutputRule?)v.StockOutputRuleId))
                .ForMember(d => d.ExpireTimeTypeId, s => s.MapFrom(v => (EnumTimeType?)v.ExpireTimeTypeId))
                .ReverseMapCustom()
                .ForMember(d => d.Product, s => s.Ignore())
                .ForMember(d => d.TimeWarningTimeTypeId, s => s.MapFrom(v => (int?)v.TimeWarningTimeTypeId))
                .ForMember(d => d.StockOutputRuleId, s => s.MapFrom(v => (int?)v.StockOutputRuleId))
                .ForMember(d => d.ExpireTimeTypeId, s => s.MapFrom(v => (int?)v.ExpireTimeTypeId));

            profile.CreateMapCustom<ProductCustomer, ProductModelCustomer>()
               .ReverseMapCustom();

            profile.CreateMapCustom<ProductUnitConversion, ProductModelUnitConversion>()
             .ReverseMapCustom();

        }
    }
}
