using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Input
{
    public class InputTypeViewModel : IMapFrom<InputTypeView>
    {
        public int? InputTypeViewId { get; set; }
        public string InputTypeViewName { get; set; }
        public int InputTypeId { get; set; }
        public int? UserId { get; set; }
        public bool IsDefault { get; set; }
        public int Columns { get; set; }

        public IList<InputTypeViewFieldModel> Fields { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap(typeof(InputTypeView), GetType())
                .ForMember(nameof(Fields), m => m.Ignore())
                .ReverseMap()
                .ForMember(nameof(InputTypeViewId), m => m.Ignore());
        }
    }

    public class InputTypeViewFieldModel : IMapFrom<InputTypeViewField>
    {
        public int InputAreaFieldId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string DefaultValue { get; set; }
        public EnumOperator Operator { get; set; }
    }
}
