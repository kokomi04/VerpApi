﻿using AutoMapper;
using System.Collections.Generic;
using VErp.Commons.Enums.Stock;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.StockDB;
using InventoryRequirementEntity = VErp.Infrastructure.EF.StockDB.InventoryRequirement;

namespace VErp.Services.Stock.Model.Inventory.InventoryRequirement
{
    public class InventoryRequirementCreateBaseModel
    {
        public string InventoryRequirementCode { get; set; }
        public string Content { get; set; }
        public long Date { get; set; }
        public int? DepartmentId { get; set; }
        public long? ProductionStepId { get; set; }

        //public string Shipper { get; set; }
        public int? CustomerId { get; set; }
        //public string BillForm { get; set; }
        //public string BillCode { get; set; }
        //public string BillSerial { get; set; }
        //public long? BillDate { get; set; }
        public int? ModuleTypeId { get; set; }
        public EnumInventoryRequirementType InventoryRequirementTypeId { get; set; }
        public EnumInventoryOutsideMappingType InventoryOutsideMappingTypeId { get; set; }
        public int? ProductMaterialsConsumptionGroupId { get; set; }
        public long? ProductionOrderMaterialSetId { get; set; }

 
        public long InventoryRequirementId { get; set; }

        public long UpdatedDatetimeUtc { get; set; }

    }

    public class InventoryRequirementBaseModel : InventoryRequirementCreateBaseModel
    {
        public int CreatedByUserId { get; set; }

        //public string Shipper { get; set; }
        //public string BillForm { get; set; }
        //public string BillCode { get; set; }
        //public string BillSerial { get; set; }
        //public long? BillDate { get; set; }
        

        public int? CensorByUserId { get; set; }
        public long? CensorDatetimeUtc { get; set; }
        public EnumInventoryRequirementStatus CensorStatus { get; set; }


    }

    public class InventoryRequirementListModel : InventoryRequirementBaseModel, IMapFrom<InventoryRequirementDetail>
    {


        public int ProductId { get; set; }
        public long InventoryRequirementDetailId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string StockName { get; set; }
        public long? OutsourceStepRequestId { get; set; }

        public IList<InventorySimpleInfo> InventoryInfo { set; get; }

        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal InventoryQuantity { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal? ProductUnitConversionQuantity { get; set; }

        public string OrderCode { get; set; }
        public string PoCode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }


        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<InventoryRequirementDetail, InventoryRequirementListModel>()
                .ForMember(dest => dest.InventoryRequirementCode, otp => otp.MapFrom(source => source.InventoryRequirement.InventoryRequirementCode))
                .ForMember(dest => dest.Content, otp => otp.MapFrom(source => source.InventoryRequirement.Content))
                .ForMember(dest => dest.Date, otp => otp.MapFrom(source => source.InventoryRequirement.Date.GetUnix()))
                .ForMember(dest => dest.DepartmentId, otp => otp.MapFrom(source => source.DepartmentId))
                .ForMember(dest => dest.ProductionStepId, otp => otp.MapFrom(source => source.ProductionStepId))
                .ForMember(dest => dest.CreatedByUserId, otp => otp.MapFrom(source => source.InventoryRequirement.CreatedByUserId))
                .ForMember(dest => dest.ProductionOrderCode, otp => otp.MapFrom(source => source.ProductionOrderCode))
                //.ForMember(dest => dest.Shipper, otp => otp.MapFrom(source => source.InventoryRequirement.Shipper))
                .ForMember(dest => dest.CustomerId, otp => otp.MapFrom(source => source.InventoryRequirement.CustomerId))
                //.ForMember(dest => dest.BillForm, otp => otp.MapFrom(source => source.InventoryRequirement.BillForm))
                //.ForMember(dest => dest.BillCode, otp => otp.MapFrom(source => source.InventoryRequirement.BillCode))
                //.ForMember(dest => dest.BillSerial, otp => otp.MapFrom(source => source.InventoryRequirement.BillSerial))
                //.ForMember(dest => dest.BillDate, otp => otp.MapFrom(source => source.InventoryRequirement.BillDate.GetUnix()))
                .ForMember(dest => dest.ModuleTypeId, otp => otp.MapFrom(source => source.InventoryRequirement.ModuleTypeId))
                .ForMember(dest => dest.InventoryRequirementId, otp => otp.MapFrom(source => source.InventoryRequirement.InventoryRequirementId))
                .ForMember(dest => dest.CensorByUserId, otp => otp.MapFrom(source => source.InventoryRequirement.CensorByUserId))
                .ForMember(dest => dest.CensorDatetimeUtc, otp => otp.MapFrom(source => source.InventoryRequirement.CensorDatetimeUtc.GetUnix()))
                .ForMember(dest => dest.CensorStatus, otp => otp.MapFrom(source => (EnumInventoryRequirementStatus)source.InventoryRequirement.CensorStatus))
                .ForMember(dest => dest.ProductCode, otp => otp.MapFrom(source => source.Product.ProductCode))
                .ForMember(dest => dest.ProductName, otp => otp.MapFrom(source => source.Product.ProductName))
                .ForMember(dest => dest.ProductTitle, otp => otp.MapFrom(source => $"{source.Product.ProductCode} / {source.Product.ProductName}"))
                .ForMember(dest => dest.StockName, otp => otp.MapFrom(source => source.AssignStock.StockName))
                .ForMember(dest => dest.OutsourceStepRequestId, otp => otp.MapFrom(source => source.OutsourceStepRequestId))
                .ForMember(dest => dest.PrimaryQuantity, otp => otp.MapFrom(source => source.PrimaryQuantity))
                .ForMember(dest => dest.ProductUnitConversionQuantity, otp => otp.MapFrom(source => source.ProductUnitConversionQuantity));
        }
    }

    public class InventoryRequirementInputModel : InventoryRequirementCreateBaseModel, IMapFrom<InventoryRequirementEntity>
    {
        public virtual ICollection<InventoryRequirementDetailInputModel> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFileInputModel> InventoryRequirementFile { get; set; }

        public InventoryRequirementInputModel()
        {
            InventoryRequirementFile = new List<InventoryRequirementFileInputModel>();
            InventoryRequirementDetail = new List<InventoryRequirementDetailInputModel>();
            OutsideImportMappingData = new OutsideImportMappingData();
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<InventoryRequirementInputModel, InventoryRequirementEntity>()
                .ForMember(dest => dest.InventoryRequirementDetail, opt => opt.Ignore())
                .ForMember(dest => dest.InventoryRequirementFile, opt => opt.Ignore())
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.UnixToDateTime()));
                //.ForMember(dest => dest.BillDate, opt => opt.MapFrom(source => source.BillDate.UnixToDateTime()));
        }

        public OutsideImportMappingData OutsideImportMappingData { get; set; }        
    }

    public class OutsideImportMappingData
    {
        public string MappingFunctionKey { get; set; }
        public string ObjectId { get; set; }
    }

    public class InventoryRequirementOutputModel : InventoryRequirementBaseModel, IMapFrom<InventoryRequirementEntity>
    {
        public virtual ICollection<InventoryRequirementDetailOutputModel> InventoryRequirementDetail { get; set; }
        public virtual ICollection<InventoryRequirementFileOutputModel> InventoryRequirementFile { get; set; }

        public InventoryRequirementOutputModel()
        {
            InventoryRequirementFile = new List<InventoryRequirementFileOutputModel>();
            InventoryRequirementDetail = new List<InventoryRequirementDetailOutputModel>();
        }


        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<InventoryRequirementEntity, InventoryRequirementOutputModel>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(source => source.Date.GetUnix()))
                .ForMember(dest => dest.CensorDatetimeUtc, opt => opt.MapFrom(source => source.CensorDatetimeUtc.GetUnix()))
                //.ForMember(dest => dest.BillDate, opt => opt.MapFrom(source => source.BillDate.GetUnix()))
                .ForMember(dest => dest.CensorStatus, opt => opt.MapFrom(source => (EnumInventoryRequirementStatus)source.CensorStatus));
        }
    }
}
