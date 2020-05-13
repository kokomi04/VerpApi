using AutoMapper;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryValueModel : IMapFrom<CategoryRowValue>
    {
        public int CategoryValueId { get; set; }
        public int CategoryFieldId { get; set; }
        public string Value { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CategoryValueModel, CategoryRowValue>();
            profile.CreateMap<CategoryRowValue, CategoryValueModel>()
                .ForMember(dest => dest.CategoryValueId, opt => opt.MapFrom(src => src.CategoryRowValueId))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(
                    src => (src.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || src.CategoryField.FormTypeId == (int)EnumFormType.Select)
                    && src.ReferenceCategoryRowValueId.HasValue 
                    ? src.ReferenceCategoryRowValue.Value 
                    : src.Value));
        }
    }


    public class MapTitleInputModel
    {
        public int CategoryFieldId { get; set; }
        public int CategoryFieldTitleId { get; set; }
        public string Value { get; set; }

    }

    public class MapTitleOutputModel : MapTitleInputModel
    {
        public string Title { get; set; }

    }
}
