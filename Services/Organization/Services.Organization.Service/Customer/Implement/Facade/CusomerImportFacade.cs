﻿using AutoMapper;
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
using VErp.Infrastructure.EF.OrganizationDB;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.Customer.Implement.Facade
{
    public class CusomerImportFacade
    {
        private readonly IMapper _mapper;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ICustomerService _customerService;
        private readonly OrganizationDBContext _organizationContext;

        private IList<CustomerModel> lstAddCustomer = new List<CustomerModel>();
        private IList<CustomerModel> lstUpdateCustomer = new List<CustomerModel>();
        private readonly ObjectActivityLogFacade _customerActivityLog;

        private IList<CustomerBankAccount> _bankAccounts;
        private IList<CustomerContact> _customerContact;

        public CusomerImportFacade(ICustomerService customerService, ObjectActivityLogFacade customerActivityLog, IMapper mapper, ICategoryHelperService httpCategoryHelperService, OrganizationDBContext organizationContext)
        {
            _customerService = customerService;
            _customerActivityLog = customerActivityLog;
            _mapper = mapper;
            _httpCategoryHelperService = httpCategoryHelperService;
            _organizationContext = organizationContext;
        }


        public async Task<bool> ParseCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var currencies = await _httpCategoryHelperService.GetDataRows(CurrencyCategoryCode, new CategoryFilterModel());

            var reader = new ExcelReader(stream);


            var strContactGender = nameof(BaseCustomerImportModel.ContactGender1);
            strContactGender = strContactGender.Substring(0, strContactGender.Length - 1);


            var strBankAccCurrency = nameof(BaseCustomerImportModel.BankAccCurrency1);
            strBankAccCurrency = strBankAccCurrency.Substring(0, strBankAccCurrency.Length - 1);

            var lstData = reader.ReadSheetEntity<BaseCustomerImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (propertyName == nameof(BaseCustomerImportModel.CustomerTypeId))
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

                if (propertyName == nameof(BaseCustomerImportModel.DebtBeginningTypeId))
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

                if (propertyName == nameof(BaseCustomerImportModel.LoanBeginningTypeId))
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

                        var id = Convert.ToInt32(currency[F_Id]);

                        entity.SetPropertyValue(propertyName, id);
                    }
                    return true;
                }


                return false;
            });

            var customersCode = lstData.Select(x => x.CustomerCode);
            var customersName = lstData.Select(x => x.CustomerName);
            var existsCumstomers = await _organizationContext.Customer.Where(x => customersCode.Contains(x.CustomerCode) || customersName.Contains(x.CustomerName))
                .AsNoTracking()
                .Select(x => new { x.CustomerId, x.CustomerCode, x.CustomerName })
                .ToListAsync();

            var contactNames = lstData.SelectMany(x =>
            {
                var rs = new List<string>();

                for (var number = 1; number <= 3; number++)
                {
                    var name = GetValueStringByFieldNumber(x, nameof(BaseCustomerImportModel.ContactName1), number);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        rs.Add(name);
                    }
                }
                return rs;
            }).ToList();

            var backAccounts = lstData.SelectMany(x =>
            {
                var rs = new List<CustomerBankAccountModel>();
                for (var number = 1; number <= 3; number++)
                {
                    var name = GetValueStringByFieldNumber(x, nameof(BaseCustomerImportModel.BankAccAccountName1), number);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        rs.Add(new CustomerBankAccountModel()
                        {
                            BankName = GetValueStringByFieldNumber(x, nameof(BaseCustomerImportModel.BankAccBankName1), number) ?? "",
                            AccountName = name
                        });
                    }
                }
                return rs;
            }).ToList();

            _bankAccounts = await _organizationContext.CustomerBankAccount.Where(x =>
                backAccounts.Select(b => b.BankName).Contains(x.BankName) &&
                backAccounts.Select(b => b.AccountName).Contains(x.AccountName)
            ).AsNoTracking().ToListAsync();
            
            _customerContact = await _organizationContext.CustomerContact.Where(x => contactNames.Contains(x.FullName)).AsNoTracking().ToListAsync();

            foreach (var customerModel in lstData)
            {

                var customerInfo = _mapper.Map<CustomerModel>(customerModel);
                customerInfo.CustomerStatusId = EnumCustomerStatus.Actived;

                LoadContacts(customerInfo, customerModel, mapping);
                LoadBankAccounts(customerInfo, customerModel, mapping);

                if (customerInfo.CustomerTypeId == 0)
                {
                    customerInfo.CustomerTypeId = customerInfo.Contacts?.Count > 0 ? EnumCustomerType.Organization : EnumCustomerType.Personal;
                }

                var existedCustomers = existsCumstomers.Where(x => x.CustomerName == customerInfo.CustomerName || x.CustomerCode == customerInfo.CustomerCode);

                if (existedCustomers != null && existedCustomers.Count() > 0 && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                {
                    var existedCodes = existedCustomers.Select(c => c.CustomerCode).ToList();
                    var existingCodes = existedCodes.Intersect(new[] { customerInfo.CustomerCode }, StringComparer.OrdinalIgnoreCase);

                    if (existingCodes.Count() > 0)
                    {
                        throw new BadRequestException(CustomerErrorCode.CustomerCodeAlreadyExisted, $"Mã đối tác \"{string.Join(", ", existingCodes)}\" đã tồn tại");
                    }

                    throw new BadRequestException(CustomerErrorCode.CustomerNameAlreadyExisted, $"Tên đối tác \"{string.Join(", ", existedCustomers.Select(c => c.CustomerName))}\" đã tồn tại");
                }

                var oldCustomer = existedCustomers.FirstOrDefault();

                if (oldCustomer == null)
                {
                    if (lstAddCustomer.Any(x => x.CustomerName == customerInfo.CustomerName || x.CustomerCode == customerInfo.CustomerCode))
                        if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại nhiều đối tác {customerInfo.CustomerCode} trong file excel");
                        else
                            continue;

                    lstAddCustomer.Add(customerInfo);
                }
                else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                {
                    if (lstUpdateCustomer.Any(x => x.CustomerName == customerInfo.CustomerName || x.CustomerCode == customerInfo.CustomerCode))
                        continue;

                    customerInfo.CustomerId = oldCustomer.CustomerId;

                    lstUpdateCustomer.Add(customerInfo);
                }
            }

            using var activityLog = _customerActivityLog.BeginBatchLog();
            using var @trans = await _organizationContext.Database.BeginTransactionAsync();
            try
            {
                 foreach(var customer in lstUpdateCustomer)
                    await _customerService.UpdateCustomerBase(customer.CustomerId, customer, true);

                await _customerService.AddBatchCustomersBase(lstAddCustomer);

                await @trans.CommitAsync();

                await activityLog.CommitAsync();
            }
            catch (System.Exception)
            {
                await @trans.RollbackAsync();
                throw;
            }

            return true;
        }

        private void LoadContacts(CustomerModel model, BaseCustomerImportModel obj, ImportExcelMapping mapping)
        {
            model.Contacts = new List<CustomerContactModel>();
            for (var number = 1; number <= 3; number++)
            {
                var name = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactName1), number);
                var existsEntity = _customerContact.FirstOrDefault(x => model.CustomerId == x.CustomerId && x.FullName == name);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    model.Contacts.Add(new CustomerContactModel()
                    {
                        CustomerContactId = existsEntity != null ? existsEntity.CustomerContactId : 0,
                        FullName = name,
                        GenderId = GetValueByFieldNumber<EnumGender>(obj, nameof(BaseCustomerImportModel.ContactGender1), number),
                        Position = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactPosition1), number) ?? "",
                        PhoneNumber = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactPhone1), number) ?? "",
                        Email = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.ContactEmail1), number) ?? "",
                    });
                }
            }

        }

        private void LoadBankAccounts(CustomerModel model, BaseCustomerImportModel obj, ImportExcelMapping mapping)
        {
            model.BankAccounts = new List<CustomerBankAccountModel>();
            for (var number = 1; number <= 3; number++)
            {
                var name = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAccountName1), number);
                var bankName = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccBankName1), number);

                var existsEntity = _bankAccounts.FirstOrDefault(x =>model.CustomerId == x.CustomerId && x.BankName == bankName && x.AccountName == name);
               
                if (!string.IsNullOrWhiteSpace(name))
                {
                    model.BankAccounts.Add(new CustomerBankAccountModel()
                    {
                        BankAccountId = existsEntity != null ? existsEntity.CustomerBankAccountId : 0,
                        BankName = bankName ?? "",
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
