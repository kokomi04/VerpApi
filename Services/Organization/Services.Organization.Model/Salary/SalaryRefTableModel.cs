using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryRefTableModel : IMapFrom<SalaryRefTable>
    {
        public int SalaryRefTableId { get; set; }
        public int SortOrder { get; set; }
        public string FromField { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string RefTableCode { get; set; }
        public Clause Filter { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string RefTableField { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(128)]
        [RegularExpression("([a-zA-Z0-9_]){1,127}\\$", ErrorMessage = "Định danh không hợp lệ, định danh chỉ bao gồm chữ cái hoặc số, dấu gạch dưới, kết thúc bởi dấu $")]
        public string Alias { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<SalaryRefTableModel, SalaryRefTable>()
                    .ForMember(d => d.Filter, s => s.MapFrom(f => f.Filter.JsonSerialize()))
                    .ReverseMapCustom()
                    .ForMember(d => d.Filter, s => s.MapFrom(f => f.Filter.JsonDeserialize<Clause>()))
                    ;
        }
    }
}
