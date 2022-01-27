﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Organization;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using LeaveBill = VErp.Infrastructure.EF.OrganizationDB.Leave;

namespace VErp.Services.Organization.Model.Leave
{
    public class LeaveModel : IMapFrom<LeaveBill>
    {
        public long LeaveId { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long DateStart { get; set; }
        public bool DateStartIsHalf { get; set; }
        public long DateEnd { get; set; }
        public bool DateEndIsHalf { get; set; }
        public decimal TotalDays { get; set; }
        public long? FileId { get; set; }
        public int AbsenceTypeSymbolId { get; set; }
        public EnumLeaveStatus LeaveStatusId { get; set; }
        public int? CheckedByUserId { get; set; }
        public int? CensoredByUserId { get; set; }
        public long? CheckedDatetimeUtc { get; set; }
        public long? CensoredDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<LeaveModel, LeaveBill>()
                .ForMember(d => d.LeaveStatusId, s => s.MapFrom(m => (int)m.LeaveStatusId))
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(m => m.CreatedDatetimeUtc.UnixToDateTime()))
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.MapFrom(m => m.UpdatedDatetimeUtc.UnixToDateTime()))
                .ForMember(d => d.CheckedDatetimeUtc, s => s.MapFrom(m => m.CheckedDatetimeUtc.UnixToDateTime()))
                .ForMember(d => d.CensoredDatetimeUtc, s => s.MapFrom(m => m.CensoredDatetimeUtc.UnixToDateTime()))
                .ReverseMap()
                .ForMember(d => d.LeaveStatusId, s => s.MapFrom(m => (EnumLeaveStatus)m.LeaveStatusId))
                .ForMember(d => d.CreatedDatetimeUtc, s => s.MapFrom(m => m.CreatedDatetimeUtc.GetUnix()))
                .ForMember(d => d.UpdatedDatetimeUtc, s => s.MapFrom(m => m.UpdatedDatetimeUtc.GetUnix()))
                .ForMember(d => d.CheckedDatetimeUtc, s => s.MapFrom(m => m.CheckedDatetimeUtc.GetUnix()))
                .ForMember(d => d.CensoredDatetimeUtc, s => s.MapFrom(m => m.CensoredDatetimeUtc.GetUnix()));

        }

    }

}
