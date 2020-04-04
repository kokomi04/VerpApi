using System;
using AutoMapper;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Services.Accountant.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErpApi.Controllers.Accountant
{
    public class AccountantMapping : Profile
    {
        public AccountantMapping()
        {
            CreateMap<AccountingAccount, AccountingAccountOutputModel>();
            CreateMap<AccountingAccountInputModel, AccountingAccount>();

            CreateMap<CategoryEntity, CategoryModel>();
            CreateMap<CategoryModel, CategoryEntity>().ForMember(c => c.SubCategories, act => act.Ignore());
            CreateMap<CategoryEntity, CategoryFullModel>();
            CreateMap<DataType, DataTypeModel>().ReverseMap();
            CreateMap<FormType, FormTypeModel>().ReverseMap();
            CreateMap<CategoryField, CategoryFieldOutputModel>();
            CreateMap<CategoryField, CategoryFieldOutputFullModel>();
            CreateMap<CategoryFieldInputModel, CategoryField>();
            CreateMap<CategoryValueModel, CategoryValue>();
        }
    }
}
