﻿using AutoMapper;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryGroupModel : IMapFrom<SalaryGroup>
    {
        public int SalaryGroupId { get; set; }
        public string Title { get; set; }
        public Clause EmployeeFilter { get; set; }

        public IList<SalaryGroupFieldModel> TableFields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<SalaryGroupModel, SalaryGroup>()
                    .ForMember(d => d.EmployeeFilter, s => s.MapFrom(f => f.EmployeeFilter.JsonSerialize()))
                    .ForMember(d => d.SalaryGroupField, s => s.Ignore())
                    .ReverseMapCustom()
                    .ForMember(d => d.EmployeeFilter, s => s.MapFrom(f => f.EmployeeFilter.JsonDeserialize<Clause>()))
                    .ForMember(d => d.TableFields, s => s.Ignore());
        }
    }


    public class SalaryGroupFieldModel : IMapFrom<SalaryGroupField>
    {
        public int SalaryFieldId { get; set; }
        public bool IsEditable { get; set; }
        public bool IsHidden { get; set; }
        public int SortOrder { get; set; }
    }

}
