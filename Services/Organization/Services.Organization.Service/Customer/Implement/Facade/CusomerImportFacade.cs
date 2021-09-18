using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Organization.Model.Customer;
using static VErp.Commons.Constants.CurrencyCateConstants;
using static VErp.Commons.Constants.CategoryFieldConstants;

namespace VErp.Services.Organization.Service.Customer.Implement.Facade
{
    public class CusomerImportFacade
    {
        private readonly IMapper _mapper;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        public CusomerImportFacade(IMapper mapper, ICategoryHelperService httpCategoryHelperService)
        {
            _mapper = mapper;
            _httpCategoryHelperService = httpCategoryHelperService;
        }

        public async Task<List<CustomerModel>> ParseCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var currencies = await _httpCategoryHelperService.GetDataRows(CurrencyCategoryCode, new CategoryFilterModel());

            var reader = new ExcelReader(stream);


            var strContactGender = nameof(CustomerModel.ContactGender1);
            strContactGender = strContactGender.Substring(0, strContactGender.Length - 1);


            var strBankAccCurrency = nameof(CustomerModel.BankAccCurrency1);
            strBankAccCurrency = strBankAccCurrency.Substring(0, strBankAccCurrency.Length - 1);

            var lstData = reader.ReadSheetEntity<BaseCustomerImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (propertyName == nameof(CustomerModel.CustomerTypeId))
                {
                    if (value.NormalizeAsInternalName().Equals(EnumCustomerType.Personal.GetEnumDescription().NormalizeAsInternalName()))
                    {
                        entity.CustomerTypeId = EnumCustomerType.Personal;
                    }
                    else
                    {
                        entity.CustomerTypeId = EnumCustomerType.Organization;
                    }

                    return true;
                }

                if (propertyName == nameof(CustomerModel.DebtBeginningTypeId))
                {
                    if (value.NormalizeAsInternalName().Equals(((int)EnumBeginningType.EndOfMonth).ToString().NormalizeAsInternalName()))
                    {
                        entity.DebtBeginningTypeId = EnumBeginningType.EndOfMonth;
                    }
                    else
                    {
                        entity.DebtBeginningTypeId = EnumBeginningType.BillDate;
                    }

                    return true;
                }

                if (propertyName == nameof(CustomerModel.LoanBeginningTypeId))
                {
                    if (value.NormalizeAsInternalName().Equals(((int)EnumBeginningType.EndOfMonth).ToString().NormalizeAsInternalName()))
                    {
                        entity.LoanBeginningTypeId = EnumBeginningType.EndOfMonth;
                    }
                    else
                    {
                        entity.LoanBeginningTypeId = EnumBeginningType.BillDate;
                    }

                    return true;
                }

                if (propertyName.StartsWith(strContactGender))
                {
                    entity.SetPropertyValue(propertyName, value.GetEnumValue<EnumGender>());

                    return true;
                }


                if (propertyName.StartsWith(strBankAccCurrency))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var val = value?.NormalizeAsInternalName();

                        var currency = currencies.List.FirstOrDefault(c => c.ContainsKey(CurrencyCode) && c[CurrencyCode]?.ToString()?.NormalizeAsInternalName() == val
                         || c.ContainsKey(CurrencyName) && c[CurrencyName]?.ToString()?.NormalizeAsInternalName() == val
                        );
                        if (currency == null) throw new BadRequestException("Không tìm thấy tiền tệ " + value);

                        var id = Convert.ToInt64(currency[F_Id]);

                        entity.SetPropertyValue(propertyName, id);
                    }
                    return true;
                }


                return false;
            });

            var customerModels = new List<CustomerModel>();

            foreach (var customerModel in lstData)
            {

                var customerInfo = _mapper.Map<CustomerModel>(customerModel);
                customerInfo.CustomerStatusId = EnumCustomerStatus.Actived;

                LoadContacts(customerInfo, customerModel);
                LoadBankAccounts(customerInfo, customerModel);

                customerModels.Add(customerInfo);
            }


            return customerModels;

        }

        private void LoadContacts(CustomerModel model, BaseCustomerImportModel obj)
        {
            model.Contacts = new List<CustomerContactModel>();
            for (var number = 1; number <= 3; number++)
            {
                var name = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactName1), number);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    model.Contacts.Add(new CustomerContactModel()
                    {
                        FullName = name,
                        GenderId = GetValueByFieldNumber<EnumGender>(obj, nameof(BaseCustomerImportModel.ContactGender1), number),
                        Position = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactPosition1), number) ?? "",
                        PhoneNumber = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactPhone1), number) ?? "",
                        Email = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactEmail1), number) ?? "",
                    });
                }
            }

        }

        private void LoadBankAccounts(CustomerModel model, BaseCustomerImportModel obj)
        {
            model.BankAccounts = new List<CustomerBankAccountModel>();
            for (var number = 1; number <= 3; number++)
            {
                var name = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAccountName1), number);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    model.BankAccounts.Add(new CustomerBankAccountModel()
                    {
                        BankName = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccBankName1), number) ?? "",
                        AccountNumber = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAccountNo1), number) ?? "",
                        SwiffCode = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccSwiffCode1), number) ?? "",
                        BankCode = "",
                        BankBranch = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccBrach1), number) ?? "",
                        BankAddress = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAddress1), number) ?? "",
                        AccountName = name ?? "",
                        CurrencyId = GetValueByFieldNumber<int>(obj, nameof(BaseCustomerImportModel.BankAccCurrency1), number),
                        Province = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAddress1), number) ?? ""
                    });
                }
            }

        }

        private T? GetValueByFieldNumber<T>(BaseCustomerImportModel obj, string propertyName, int number) where T : unmanaged
        {
            var fieldPrefix = GetFieldNameWithoutNumber(propertyName);
            var filedName = fieldPrefix + number;
            return (T?)obj.GetType().GetProperty(filedName).GetValue(obj);
        }

        private string GetValueStringByFieldNumber(BaseCustomerImportModel obj, string propertyName, int number)
        {
            var fieldPrefix = GetFieldNameWithoutNumber(propertyName);
            var filedName = fieldPrefix + number;
            return obj.GetType().GetProperty(filedName).GetValue(obj)?.ToString();
        }

        Dictionary<string, string> FieldPrefixs = new Dictionary<string, string>();
        private string GetFieldNameWithoutNumber(string propertyName)
        {
            if (FieldPrefixs.ContainsKey(propertyName)) return FieldPrefixs[propertyName];

            var val = propertyName.Substring(0, propertyName.Length - 1);
            FieldPrefixs.TryAdd(propertyName, val);
            return val;
        }
    }
}
