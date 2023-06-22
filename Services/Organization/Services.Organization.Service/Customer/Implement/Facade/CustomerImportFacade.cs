using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.GlobalObject.Org;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Customer;
using static Verp.Resources.Organization.Customer.CustomerValidationMessage;
using static VErp.Commons.Constants.CategoryFieldConstants;
using static VErp.Commons.Constants.ConditionsConstants;
using static VErp.Commons.Constants.CurrencyCateConstants;

namespace VErp.Services.Organization.Service.Customer.Implement.Facade
{
    public class CustomerImportFacade
    {
        private readonly IMapper _mapper;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ICustomerService _customerService;
        private readonly OrganizationDBContext _organizationContext;
        private readonly IList<CustomerModel> lstAddCustomer = new List<CustomerModel>();
        private readonly IList<CustomerModel> lstUpdateCustomer = new List<CustomerModel>();
        private readonly IUserHelperService _userHelperService;
        private readonly ObjectActivityLogFacade _customerActivityLog;

        private IList<CustomerBankAccount> _bankAccounts;
        private IList<CustomerContact> _customerContact;
        private IList<ReferFieldModel> _refFields;
        private IList<NonCamelCaseDictionary> _payConditionData;
        private IList<NonCamelCaseDictionary> _deliveryConditionData;
        private IList<EmployeeBasicNameModel> _users;
        public CustomerImportFacade(ICustomerService customerService, ObjectActivityLogFacade customerActivityLog, IMapper mapper, ICategoryHelperService httpCategoryHelperService, OrganizationDBContext organizationContext, IUserHelperService userHelperService)
        {
            _customerService = customerService;
            _customerActivityLog = customerActivityLog;
            _mapper = mapper;
            _httpCategoryHelperService = httpCategoryHelperService;
            _organizationContext = organizationContext;
            _userHelperService = userHelperService;
        }


        public async Task<bool> ParseCustomerFromMapping(ILongTaskResourceLockService longTaskResourceLockService, ImportExcelMapping mapping, Stream stream)
        {
            using (var longTask = await longTaskResourceLockService.Accquire($"Nhập dữ liệu đối tác từ excel"))
            {
                var refProps = mapping.MappingFields.Select(f => f.RefFieldName).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().ToList();
                _refFields = await _httpCategoryHelperService.GetReferFields(new[] { PayConditionCode, DeliveryConditionCode }, refProps);

                var reader = new ExcelReader(stream);
                reader.RegisterLongTaskEvent(longTask);

                var lstData = await ReadExcel(reader, mapping);

                var customersCode = lstData.Select(x => x.CustomerCode);
                var customersName = lstData.Select(x => x.CustomerName);
                var existsCustomers = await _organizationContext.Customer.Where(x => customersCode.Contains(x.CustomerCode) || customersName.Contains(x.CustomerName))
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
                ).AsNoTracking()
                .ToListAsync();

                _customerContact = await _organizationContext.CustomerContact.Where(x => contactNames.Contains(x.FullName)).AsNoTracking().ToListAsync();

                foreach (var customerModel in lstData)
                {

                    var customerInfo = _mapper.Map<CustomerModel>(customerModel);
                    customerInfo.CustomerStatusId = EnumCustomerStatus.Actived;

                    LoadContacts(customerInfo, customerModel);
                    LoadBankAccounts(customerInfo, customerModel);

                    if (customerInfo.CustomerTypeId == 0)
                    {
                        customerInfo.CustomerTypeId = customerInfo.Contacts?.Count > 0 ? EnumCustomerType.Organization : EnumCustomerType.Personal;
                    }

                    var existedCustomers = existsCustomers.Where(x => x.CustomerName == customerInfo.CustomerName || x.CustomerCode == customerInfo.CustomerCode);

                    if (existedCustomers != null && existedCustomers.Any() && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    {
                        var existedCodes = existedCustomers.Select(c => c.CustomerCode).ToList();
                        var existingCodes = existedCodes.Intersect(new[] { customerInfo.CustomerCode }, StringComparer.OrdinalIgnoreCase);

                        if (existingCodes.Any())
                        {
                            throw CustomerCodeAlreadyExists.BadRequestFormat(string.Join(", ", existingCodes));
                        }

                        throw CustomerNameAlreadyExists.BadRequestFormat(string.Join(", ", existedCustomers.Select(c => c.CustomerName)));
                    }

                    var oldCustomer = existedCustomers.FirstOrDefault();

                    if (oldCustomer == null)
                    {
                        if (lstAddCustomer.Any(x => x.CustomerName == customerInfo.CustomerName || x.CustomerCode == customerInfo.CustomerCode))
                            if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                                throw MultipleCustomerFound.BadRequestFormat(customerInfo.CustomerCode);
                            else
                                continue;

                        lstAddCustomer.Add(customerInfo);
                    }
                    else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                    {
                        if (lstUpdateCustomer.Any(x => (!string.IsNullOrEmpty(customerInfo.CustomerName) && x.CustomerName == customerInfo.CustomerName) || (!string.IsNullOrEmpty( customerInfo.CustomerCode) &&  x.CustomerCode == customerInfo.CustomerCode) ))
                            continue;

                        customerInfo.CustomerId = oldCustomer.CustomerId;

                        lstUpdateCustomer.Add(customerInfo);
                    }
                }


                using var activityLog = _customerActivityLog.BeginBatchLog();
                using var @trans = await _organizationContext.Database.BeginTransactionAsync();
                try
                {
                    longTask.SetCurrentStep("Cập nhật đối tác vào cơ sở dữ liệu", lstUpdateCustomer.Count);

                    foreach (var customer in lstUpdateCustomer)
                    {
                        await _customerService.UpdateCustomerBase(customer.CustomerId, customer, true);
                        longTask.IncProcessedRows();
                    }

                    longTask.SetCurrentStep("Thêm đối tác mới vào cơ sở dữ liệu");

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
        }


        private async Task<IList<BaseCustomerImportModel>> ReadExcel(ExcelReader reader, ImportExcelMapping mapping)
        {
            var currencies = await _httpCategoryHelperService.GetDataRows(CurrencyCategoryCode, new CategoryFilterModel());
            _users = await _userHelperService.GetAll();
            _payConditionData = (await _httpCategoryHelperService.GetDataRows(PayConditionCode, new CategoryFilterModel())).List;
            _deliveryConditionData = (await _httpCategoryHelperService.GetDataRows(DeliveryConditionCode, new CategoryFilterModel())).List;
            var lstCates = await _organizationContext.CustomerCate.ToListAsync();
            var cates = lstCates.GroupBy(c => c.Name.NormalizeAsInternalName()).ToDictionary(c => c.Key, c => c.FirstOrDefault());
            var strContactGender = nameof(BaseCustomerImportModel.ContactGender1);
            strContactGender = strContactGender.Substring(0, strContactGender.Length - 1);
            CheckExisCondition(mapping);

            var strBankAccCurrency = nameof(BaseCustomerImportModel.BankAccCurrency1);
            strBankAccCurrency = strBankAccCurrency.Substring(0, strBankAccCurrency.Length - 1);

            return await reader.ReadSheetEntity<BaseCustomerImportModel>(mapping, async (entity, propertyName, value, refObj, refPropertyName, refPropertyPathSeparateByPoint) =>
            {
                await Task.CompletedTask;

                if (propertyName == nameof(BaseCustomerImportModel.PayConditionsId))
                {
                    if (string.IsNullOrWhiteSpace(value)) return true;

                    entity.PayConditionsId = ReadCondition(PayConditionCode, refPropertyName, value, _payConditionData);
                    return true;
                }
                if (propertyName == nameof(BaseCustomerImportModel.DeliveryConditionsId))
                {
                    if (string.IsNullOrWhiteSpace(value)) return true;
                    entity.DeliveryConditionsId = ReadCondition(DeliveryConditionCode, refPropertyName, value, _deliveryConditionData);
                    return true;
                }
                if (propertyName == nameof(BaseCustomerImportModel.DebtManagerUserId))
                {
                    if (string.IsNullOrWhiteSpace(value)) return true;
                    entity.DebtManagerUserId = ReadUser(refPropertyName, value);
                    return true;
                }
                if (propertyName == nameof(BaseCustomerImportModel.LoanManagerUserId))
                {
                    if (string.IsNullOrWhiteSpace(value)) return true;
                    entity.LoanManagerUserId = ReadUser(refPropertyName, value);
                    return true;
                }
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

                if (propertyName == nameof(BaseCustomerImportModel.CustomerCateId))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        var v = value.NormalizeAsInternalName();
                        if (cates.ContainsKey(v))
                        {
                            entity.CustomerCateId = cates[v].CustomerCateId;
                        }
                        else
                        {
                            throw CustomerCateNotFound.BadRequestFormat(value);
                        }
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
                        if (currency == null) throw CurrencyNotFound.BadRequestFormat(value);

                        var id = Convert.ToInt32(currency[F_Id]);

                        entity.SetPropertyValue(propertyName, id);
                    }
                    return true;
                }


                return false;
            });

        }

        private void CheckExisCondition(ImportExcelMapping mapping)
        {
            var payFieldInfo = _refFields.FirstOrDefault(f => f.CategoryCode == PayConditionCode);
            var payPropertyName = mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(BaseCustomerImportModel.PayConditionsId));
            if (payPropertyName != null && payFieldInfo == null)
            {
                var payFieldName = string.IsNullOrEmpty(payPropertyName.RefFieldName) ? payPropertyName.FieldName : payPropertyName.RefFieldName;
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {payFieldName} trong bảng {PayConditionCode}");
            }
            var deliveryFieldInfo = _refFields.FirstOrDefault(f=> f.CategoryCode == DeliveryConditionCode);
            var deliveryPropertyName = mapping.MappingFields.FirstOrDefault(f => f.FieldName == nameof(BaseCustomerImportModel.DeliveryConditionsId));
            if (deliveryPropertyName != null&& deliveryFieldInfo == null)
            {
                var deliveryFieldName = string.IsNullOrEmpty(deliveryPropertyName.RefFieldName) ? deliveryPropertyName.FieldName : deliveryPropertyName.RefFieldName;
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu {deliveryFieldName} trong bảng {DeliveryConditionCode}");
            }

        }
        private int ReadCondition(string category, string refPropertyName, string value, IList<NonCamelCaseDictionary> data)
        {
            var condition = data.Where(f => f.ContainsKey(refPropertyName) && f[refPropertyName].ToString() == value).ToList();
            if (condition.Count == 0)
            {
                throw CustomerConditionNotFound.BadRequestFormat(category, value);
            }

            if (condition.Count > 1)
            {
                throw CustomerConditionFoundMoreThanOne.BadRequestFormat(category, value);
            }
            return Convert.ToInt32(condition[0][F_Id]);
        }
        private int ReadUser(string refPropertyName, string value)
        {
            var user = _users.Where(u => u.GetType().GetProperty(refPropertyName) != null && u.GetPropertyValue<string>(refPropertyName) == value).ToList();
            if (user.Count == 0)
            {
                throw CustomerConditionNotFound.BadRequestFormat(UserManager.UserMangerCode, value);
            }

            if (user.Count > 1)
            {
                throw CustomerConditionFoundMoreThanOne.BadRequestFormat(UserManager.UserMangerCode, value);
            }
            return user[0].UserId;
        }
        private void LoadContacts(CustomerModel model, BaseCustomerImportModel obj)
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

        private void LoadBankAccounts(CustomerModel model, BaseCustomerImportModel obj)
        {
            model.BankAccounts = new List<CustomerBankAccountModel>();
            for (var number = 1; number <= 3; number++)
            {
                var name = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccAccountName1), number);
                var bankName = GetValueStringByFieldNumber(obj, nameof(BaseCustomerImportModel.BankAccBankName1), number);

                var existsEntity = _bankAccounts.FirstOrDefault(x => model.CustomerId == x.CustomerId && x.BankName == bankName && x.AccountName == name);

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

        readonly Dictionary<string, string> FieldPrefixs = new Dictionary<string, string>();
        private string GetFieldNameWithoutNumber(string propertyName)
        {
            if (FieldPrefixs.ContainsKey(propertyName)) return FieldPrefixs[propertyName];

            var val = propertyName.Substring(0, propertyName.Length - 1);
            FieldPrefixs.TryAdd(propertyName, val);
            return val;
        }
    }
}
