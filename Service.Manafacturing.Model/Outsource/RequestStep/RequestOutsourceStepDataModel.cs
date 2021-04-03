﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Model.Outsource.RequestStep
{
    public class OutsourceStepRequestDataModel : IMapFrom<OutsourceStepRequestData>
    {
        public long OutsourceStepRequestId { get; set; }
        [Required]
        public long ProductionStepLinkDataId { get; set; }
        [Required]
        public decimal? OutsourceStepRequestDataQuantity { get; set; }
        [Required]
        public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
        public long ProductionStepId { get; set; }
        public bool? IsImportant { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OutsourceStepRequestData, OutsourceStepRequestDataModel>()
                .ForMember(m => m.OutsourceStepRequestDataQuantity, v => v.MapFrom(m => m.Quantity))
                .ReverseMap()
                .ForMember(m => m.Quantity, v => v.MapFrom(m => m.OutsourceStepRequestDataQuantity));
        }
    }

    public class OutsourceStepRequestDataExtraInfo: OutsourceStepRequestDataModel
    {
        public string ProductionStepTitle { get; set; }
        public string ProductionStepLinkDataTitle { get; set; }
        public int ProductionStepLinkDataUnitId { get; set; }
        //public decimal? OutsourceStepRequestDataQuantityProcessed { get; set; }
    }
}
