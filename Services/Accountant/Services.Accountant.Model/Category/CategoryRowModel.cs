
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category

{
    public class CategoryRowInputModel : IMapFrom<CategoryRow>
    {
        public CategoryRowInputModel()
        {
            CategoryRowValues = new HashSet<CategoryValueModel>();
        }
        public int? ParentCategoryRowId { get; set; }
        public ICollection<CategoryValueModel> CategoryRowValues { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap(GetType(), typeof(CategoryRow))
                .ForMember(nameof(CategoryRow.CategoryRowValue), opt => opt.MapFrom(nameof(CategoryRowValues))).ReverseMap();
        }
    }

    public class CategoryRowListOutputModel : CategoryRowInputModel
    {
        public int CategoryRowId { get; set; }
        public int CategoryRowLevel { get; set; }
    }

    public class CategoryRowOutputModel : CategoryRowListOutputModel
    {
        public CategoryRowOutputModel ParentCategoryRow { get; set; }
    }
}
