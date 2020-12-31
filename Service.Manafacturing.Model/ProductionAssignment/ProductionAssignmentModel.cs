﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;

namespace VErp.Services.Manafacturing.Model.ProductionAssignment
{
    public class ProductionAssignmentModel : IMapFrom<ProductionAssignmentEntity>
    {
        public long? ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int DepartmentId { get; set; }
        public decimal AssignmentQuantity { get; set; }
        public int CompletedQuantity { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal Productivity { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ProductionAssignmentEntity, ProductionAssignmentModel>()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.GetUnix()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.GetUnix()))
                .ReverseMap()
                .ForMember(s => s.StartDate, d => d.MapFrom(m => m.StartDate.UnixToDateTime()))
                .ForMember(s => s.EndDate, d => d.MapFrom(m => m.EndDate.UnixToDateTime()));
        }
    }

    public class ProductionAssignmentInputModel
    {
        public ProductionAssignmentModel[] ProductionAssignments { get; set; }
        public ProductionStepWorkInfoInputModel ProductionStepWorkInfo { get; set; }
    }
}
