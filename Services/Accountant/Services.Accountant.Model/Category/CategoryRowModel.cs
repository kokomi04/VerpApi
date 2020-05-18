
using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.AccountingDB;

namespace VErp.Services.Accountant.Model.Category
{
    public class CategoryRowFilterModel
    {
        public int CategoryRowId { get; set; }
    }

    public class CategoryRowInputModel : IMapFrom<CategoryRow>
    {
        public CategoryRowInputModel()
        {
            CategoryRowValues = new HashSet<CategoryValueInputModel>();
        }
        public int? ParentCategoryRowId { get; set; }
        public ICollection<CategoryValueInputModel> CategoryRowValues { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap(GetType(), typeof(CategoryRow))
                .ForMember(nameof(CategoryRow.CategoryRowValue), opt => opt.MapFrom(nameof(CategoryRowValues)));
        }
    }

    public class CategoryRowListOutputModel : IMapFrom<CategoryRow>
    {
        public CategoryRowListOutputModel()
        {
            CategoryRowValues = new HashSet<CategoryValueModel>();
        }
        public int CategoryRowId { get; set; }
        public int CategoryRowLevel { get; set; }
        public int? ParentCategoryRowId { get; set; }
        public ICollection<CategoryValueModel> CategoryRowValues { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap(typeof(CategoryRow), GetType())
                .ForMember(nameof(CategoryRowValues), opt => opt.MapFrom(nameof(CategoryRow.CategoryRowValue)));
        }
    }

    public class CategoryRowOutputModel : CategoryRowListOutputModel
    {
        public CategoryRowOutputModel ParentCategoryRow { get; set; }
    }
}
