using AutoMapper;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{

    public class SalaryGroupModel : IMapFrom<SalaryGroup>
    {
        public int SalaryGroupId { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string Title { get; set; }
        public Clause EmployeeFilter { get; set; }
        public bool IsActived { get; set; }
        public IList<SalaryGroupFieldModel> TableFields { get; set; }

        public virtual void Mapping(Profile profile)
        {
            profile.CreateMapCustom<SalaryGroupModel, SalaryGroup>()
                    .ForMember(d => d.EmployeeFilter, s => s.MapFrom(f => f.EmployeeFilter.JsonSerialize()))
                    .ForMember(d => d.SalaryGroupField, s => s.Ignore())
                    .ReverseMapCustom()
                    .ForMember(d => d.EmployeeFilter, s => s.MapFrom(f => f.EmployeeFilter.JsonDeserialize<Clause>()))
                    .ForMember(d => d.TableFields, s => s.Ignore());
        }
    }

    public class SalaryGroupInfo : SalaryGroupModel
    {
        public int CreatedByUserId { get; set; }
        public long CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public long UpdatedDatetimeUtc { get; set; }

        public override void Mapping(Profile profile)
        {
            profile.CreateMapCustom<SalaryGroupInfo, SalaryGroup>()
                    .IncludeBase<SalaryGroupModel, SalaryGroup>()
                    .ReverseMap()
                    .IncludeBase<SalaryGroup, SalaryGroupModel>();
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
