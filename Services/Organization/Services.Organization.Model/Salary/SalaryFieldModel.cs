using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Model.Salary
{
    public class SalaryFieldModel : IMapFrom<SalaryField>
    {
        [MaxLength(128)]
        public string GroupName { get; set; }
        public int SalaryFieldId { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string SalaryFieldName { get; set; }
        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string Title { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
        public EnumDataType DataTypeId { get; set; }
        public int DecimalPlace { get; set; }        
        public int SortOrder { get; set; }
        public IList<SalaryFieldExpressionModel> Expression { get; set; }
        public bool IsEditable { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisplayRefData { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMapCustom<SalaryFieldModel, SalaryField>()
                    .ForMember(d => d.Expression, s => s.MapFrom(f => f.Expression.JsonSerialize()))   
                    .ReverseMapCustom()
                    .ForMember(d => d.Expression, s => s.MapFrom(f => f.Expression.JsonDeserialize<IList<SalaryFieldExpressionModel>>()))
                    ;
        }
    }

    public class SalaryFieldExpressionModel
    {
        public string Name { get; set; }
        public Clause Filter { get; set; }
        public string ValueExpression { get; set; }
    }
}
