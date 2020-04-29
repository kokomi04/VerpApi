using System;
using AutoMapper;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Services.Accountant.Model.Category;
using VErp.Services.Accountant.Model.Input;
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
            CreateMap<CategoryValueModel, CategoryValue>().ReverseMap();
            CreateMap<OutSideDataConfig, OutSideDataConfigModel>().ReverseMap();
            CreateMap<CategoryRow, CategoryRowOutputModel>();
            CreateMap<CategoryRow, CategoryRowListOutputModel>();
            CreateMap<CategoryRowInputModel, CategoryRow>();
            CreateMap<CategoryRowValue, CategoryValueModel>().ReverseMap();
            

            CreateMap<InputType, InputTypeModel>();
            CreateMap<InputTypeModel, InputType>();
            CreateMap<InputType, InputTypeFullModel>();
            CreateMap<InputArea, InputAreaOutputModel>();
            CreateMap<InputAreaInputModel, InputArea>();
            CreateMap<InputAreaField, InputAreaFieldOutputFullModel>();
            CreateMap<InputAreaFieldInputModel, InputAreaField>();

            CreateMap<InputAreaFieldStyle, InputAreaFieldStyleOutputModel>();
            CreateMap<InputAreaFieldStyleInputModel, InputAreaFieldStyle>();

            CreateMap<InputValueBill, InputValueBillOutputModel>();
            CreateMap<InputValueBillInputModel, InputValueBill>().ForMember(b => b.InputValueRows, act => act.Ignore());

            CreateMap<InputValueRow, InputValueRowOutputModel>();
            CreateMap<InputValueRowInputModel, InputValueRow>().ForMember(r => r.InputValueRowVersions, act => act.Ignore());

            CreateMap<InputValueRowVersion, InputValueRowVersionOutputModel>();
            CreateMap<InputValueRowVersionInputModel, InputValueRowVersion>();
        }
    }
}
