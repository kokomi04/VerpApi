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
            CreateMap<CategoryEntity, CategoryModel>().ReverseMap();
            CreateMap<CategoryEntity, CategoryFullModel>();
            CreateMap<DataType, DataTypeModel>().ReverseMap();
            CreateMap<FormType, FormTypeModel>().ReverseMap();
            CreateMap<CategoryField, CategoryFieldOutputModel>();
            CreateMap<CategoryFieldInputModel, CategoryField>();
            CreateMap<CategoryValueInputModel, CategoryValue>();
        }
    }
}
