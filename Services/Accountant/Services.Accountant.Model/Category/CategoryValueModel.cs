using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryValueModel : IMapFrom<CategoryRowValue>
    {
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryRowValue, CategoryValueModel>()
                .ForMember(nameof(Value), opt => opt.MapFrom(src => src.Value ?? src.ValueInNumber.ToString()));
        }

    }

    public class CategoryValueInputModel : CategoryValueModel
    {
        public string TitleValue { get; set; }
    }

    public class MapTitleInputModel
    {
        public int ReferCategoryFieldId { get; set; }
        //public int? CategoryFieldTitleId { get; set; }
        public string Value { get; set; }
    }


    public class MapTitleOutputModel : MapTitleInputModel
    {
        public NonCamelCaseDictionary ReferObject { get; set; }
    }
}
