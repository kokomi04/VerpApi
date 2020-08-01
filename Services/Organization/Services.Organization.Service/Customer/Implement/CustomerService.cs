using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Customer;
using VErp.Infrastructure.EF.EFExtensions;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;
using System.IO;
using VErp.Commons.GlobalObject;
using System.ComponentModel.DataAnnotations;

namespace VErp.Services.Organization.Service.Customer.Implement
{
    public class CustomerService : ICustomerService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        public CustomerService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<CustomerService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public async Task<ServiceResult<int>> AddCustomer(int updatedUserId, CustomerModel data)
        {
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                var customerId = await AddCustomerToDb(data);
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Customer, customerId, $"Thêm đối tác {data.CustomerName}", data.JsonSerialize());
                return customerId;
            }
        }


        public async Task<int> AddCustomerToDb(CustomerModel data)
        {
            var existedCustomer = await _organizationContext.Customer.FirstOrDefaultAsync(s => s.CustomerCode == data.CustomerCode || s.CustomerName == data.CustomerName);

            if (existedCustomer != null)
            {
                if (string.Compare(existedCustomer.CustomerCode, data.CustomerCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(CustomerErrorCode.CustomerCodeAlreadyExisted);
                }

                throw new BadRequestException(CustomerErrorCode.CustomerNameAlreadyExisted);
            }

            var customer = new CustomerEntity()
            {
                CustomerCode = data.CustomerCode,
                CustomerName = data.CustomerName,
                CustomerTypeId = (int)data.CustomerTypeId,
                Address = data.Address,
                TaxIdNo = data.TaxIdNo,
                PhoneNumber = data.PhoneNumber,
                Website = data.Website,
                Email = data.Email,
                Description = data.Description,
                IsActived = data.IsActived,
                IsDeleted = false,
                LegalRepresentative = data.LegalRepresentative,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                CustomerStatusId = (int)data.CustomerStatusId,
                Identify = data.Identify
            };

            await _organizationContext.Customer.AddAsync(customer);
            await _organizationContext.SaveChangesAsync();

            if (data.Contacts != null && data.Contacts.Count > 0)
            {
                await _organizationContext.CustomerContact.AddRangeAsync(data.Contacts.Select(c => new CustomerContact()
                {
                    CustomerId = customer.CustomerId,
                    FullName = c.FullName,
                    GenderId = (int?)c.GenderId,
                    Position = c.Position,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    IsDeleted = false,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                }));

                await _organizationContext.SaveChangesAsync();
            }

            if (data.BankAccounts != null && data.BankAccounts.Count > 0)
            {
                await _organizationContext.CustomerBankAccount.AddRangeAsync(data.BankAccounts.Select(ba => new CustomerBankAccount()
                {
                    CustomerId = customer.CustomerId,
                    BankName = ba.BankName,
                    AccountNumber = ba.AccountNumber,
                    SwiffCode = ba.SwiffCode,
                    UpdatedUserId = _currentContextService.UserId,
                    IsDeleted = false,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                }));

                await _organizationContext.SaveChangesAsync();
            }
            return customer.CustomerId;

        }

        public async Task<Enum> DeleteCustomer(int customerId)
        {
            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }

            var customerContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            foreach (var c in customerContacts)
            {
                c.IsDeleted = true;
                c.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            customerInfo.IsDeleted = true;
            customerInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Customer, customerInfo.CustomerId, $"Xóa đối tác {customerInfo.CustomerName}", customerInfo.JsonSerialize());

            return GeneralCode.Success;
        }

        public async Task<ServiceResult<CustomerModel>> GetCustomerInfo(int customerId)
        {
            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }
            var customerContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            var bankAccounts = await _organizationContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();

            return new CustomerModel()
            {
                CustomerName = customerInfo.CustomerName,
                CustomerCode = customerInfo.CustomerCode,
                CustomerTypeId = (EnumCustomerType)customerInfo.CustomerTypeId,
                Address = customerInfo.Address,
                TaxIdNo = customerInfo.TaxIdNo,
                PhoneNumber = customerInfo.PhoneNumber,
                Website = customerInfo.Website,
                Email = customerInfo.Email,
                LegalRepresentative = customerInfo.LegalRepresentative,
                Description = customerInfo.Description,
                IsActived = customerInfo.IsActived,
                CustomerStatusId = (EnumCustomerStatus)customerInfo.CustomerStatusId,
                Identify = customerInfo.Identify,
                Contacts = customerContacts.Select(c => new CustomerContactModel()
                {
                    CustomerContactId = c.CustomerContactId,
                    FullName = c.FullName,
                    GenderId = (EnumGender?)c.GenderId,
                    Position = c.Position,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email
                }).ToList(),
                BankAccounts = bankAccounts.Select(ba => new CustomerBankAccountModel()
                {
                    CustomerBankAccountId = ba.CustomerBankAccountId,
                    BankName = ba.BankName,
                    AccountNumber = ba.AccountNumber,
                    SwiffCode = ba.SwiffCode

                }).ToList()
            };
        }

        public async Task<PageData<CustomerListOutput>> GetList(string keyword, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from c in _organizationContext.Customer
                 select new CustomerListOutput()
                 {
                     CustomerCode = c.CustomerCode,
                     CustomerId = c.CustomerId,
                     CustomerName = c.CustomerName,
                     CustomerTypeId = (EnumCustomerType)c.CustomerTypeId,
                     Address = c.Address,
                     TaxIdNo = c.TaxIdNo,
                     PhoneNumber = c.PhoneNumber,
                     Website = c.Website,
                     Email = c.Email,
                     Identify = c.Identify,
                     CustomerStatusId = (EnumCustomerStatus)c.CustomerStatusId
                 }
             );

            query = query.InternalFilter(filters);

            if (customerStatusId.HasValue)
            {
                query = from u in query
                        where
                        u.CustomerStatusId == customerStatusId
                        select u;
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from u in query
                        where
                        u.CustomerCode.Contains(keyword)
                        || u.CustomerName.Contains(keyword)
                        || u.Address.Contains(keyword)
                        || u.TaxIdNo.Contains(keyword)
                        || u.PhoneNumber.Contains(keyword)
                        || u.Website.Contains(keyword)
                        || u.Email.Contains(keyword)

                        select u;
            }

            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<IList<CustomerListOutput>> GetListByIds(IList<int> customerIds)
        {
            if (customerIds == null || customerIds.Count == 0)
            {
                return new List<CustomerListOutput>();
            }
            return await (
                from c in _organizationContext.Customer
                where customerIds.Contains(c.CustomerId)
                select new CustomerListOutput()
                {
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    CustomerTypeId = (EnumCustomerType)c.CustomerTypeId,
                    Address = c.Address,
                    TaxIdNo = c.TaxIdNo,
                    PhoneNumber = c.PhoneNumber,
                    Website = c.Website,
                    Email = c.Email,
                    Identify = c.Identify,
                    CustomerStatusId = (EnumCustomerStatus)c.CustomerStatusId
                }
            ).ToListAsync();
        }

        public async Task<Enum> UpdateCustomer(int updatedUserId, int customerId, CustomerModel data)
        {
            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }

            var checkExisted = _organizationContext.Customer.Any(q => q.CustomerId != customerId && q.CustomerCode == data.CustomerCode);
            if (checkExisted)
                return CustomerErrorCode.CustomerCodeAlreadyExisted;
            //var existedCustomer = await _masterContext.Customer.FirstOrDefaultAsync(s => s.CustomerId != customerId && s.CustomerCode == data.CustomerCode || s.CustomerName == data.CustomerName);

            //if (existedCustomer != null)
            //{
            //    if (string.Compare(existedCustomer.CustomerCode, data.CustomerCode, StringComparison.OrdinalIgnoreCase) == 0)
            //    {
            //        return CustomerErrorCode.CustomerCodeAlreadyExisted;
            //    }

            //    return CustomerErrorCode.CustomerNameAlreadyExisted;
            //}
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
                    var dbBankAccounts = await _organizationContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();
                    customerInfo.LegalRepresentative = data.LegalRepresentative;
                    customerInfo.CustomerCode = data.CustomerCode;
                    customerInfo.CustomerName = data.CustomerName;
                    customerInfo.CustomerTypeId = (int)data.CustomerTypeId;
                    customerInfo.Address = data.Address;
                    customerInfo.TaxIdNo = data.TaxIdNo;
                    customerInfo.PhoneNumber = data.PhoneNumber;
                    customerInfo.Website = data.Website;
                    customerInfo.Email = data.Email;
                    customerInfo.Identify = data.Identify;
                    customerInfo.Description = data.Description;
                    customerInfo.IsActived = data.IsActived;
                    customerInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                    customerInfo.CustomerStatusId = (int)data.CustomerStatusId;

                    if (data.Contacts == null)
                    {
                        data.Contacts = new List<CustomerContactModel>();
                    }
                    if (data.Contacts == null)
                    {
                        data.BankAccounts = new List<CustomerBankAccountModel>();
                    }

                    var deletedContacts = dbContacts.Where(c => !data.Contacts.Any(s => s.CustomerContactId == c.CustomerContactId)).ToList();
                    foreach (var c in deletedContacts)
                    {
                        c.IsDeleted = true;
                        c.UpdatedDatetimeUtc = DateTime.UtcNow;
                    }

                    var newContacts = data.Contacts.Where(c => !(c.CustomerContactId > 0)).Select(c => new CustomerContact()
                    {
                        CustomerId = customerInfo.CustomerId,
                        FullName = c.FullName,
                        GenderId = (int?)c.GenderId,
                        Position = c.Position,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        IsDeleted = false,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow
                    });
                    await _organizationContext.CustomerContact.AddRangeAsync(newContacts);

                    foreach (var c in dbContacts)
                    {
                        var reqContact = data.Contacts.FirstOrDefault(s => s.CustomerContactId == c.CustomerContactId);
                        if (reqContact != null)
                        {
                            c.FullName = reqContact.FullName;
                            c.GenderId = (int?)reqContact.GenderId;
                            c.Position = reqContact.Position;
                            c.PhoneNumber = reqContact.PhoneNumber;
                            c.Email = reqContact.Email;
                            c.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }
                    }

                    var deletedBankAccounts = dbBankAccounts.Where(ba => !data.BankAccounts.Any(s => s.CustomerBankAccountId == ba.CustomerBankAccountId)).ToList();
                    foreach (var ba in deletedBankAccounts)
                    {
                        ba.IsDeleted = true;
                        ba.UpdatedDatetimeUtc = DateTime.UtcNow;
                        ba.UpdatedUserId = updatedUserId;
                    }

                    var newBankAccounts = data.BankAccounts.Where(ba => !(ba.CustomerBankAccountId > 0)).Select(ba => new CustomerBankAccount()
                    {
                        CustomerId = customerInfo.CustomerId,
                        BankName = ba.BankName,
                        UpdatedUserId = updatedUserId,
                        SwiffCode = ba.SwiffCode,
                        AccountNumber = ba.AccountNumber,
                        IsDeleted = false,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow
                    });
                    await _organizationContext.CustomerBankAccount.AddRangeAsync(newBankAccounts);

                    foreach (var ba in dbBankAccounts)
                    {
                        var reqBankAccount = data.BankAccounts.FirstOrDefault(s => s.CustomerBankAccountId == ba.CustomerBankAccountId);
                        if (reqBankAccount != null)
                        {
                            ba.AccountNumber = reqBankAccount.AccountNumber;
                            ba.SwiffCode = reqBankAccount.SwiffCode;
                            ba.BankName = reqBankAccount.BankName;
                            ba.UpdatedUserId = updatedUserId;
                            ba.UpdatedDatetimeUtc = DateTime.UtcNow;
                        }
                    }

                    await _organizationContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Customer, customerInfo.CustomerId, $"Cập nhật đối tác {customerInfo.CustomerName}", data.JsonSerialize());
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Update");
                    return GeneralCode.InternalError;
                }
            }
        }


        public async Task<bool> ImportCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var fields = typeof(CustomerModel).GetProperties();

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var rowDatas = new List<List<ImportExcelRowData>>();

            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new List<ImportExcelRowData>();
                bool isIgnoreRow = false;
                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count && !isIgnoreRow; fieldIndx++)
                {
                    var mappingField = mapping.MappingFields[fieldIndx];

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire)
                    {
                        isIgnoreRow = true;
                        continue;
                    }

                    var field = fields.FirstOrDefault(f => f.Name == mappingField.FieldName);

                    if (field == null) throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy field {mappingField.FieldName}");



                    rowData.Add(new ImportExcelRowData()
                    {
                        FieldMapping = mappingField,
                        PropertyInfo = field,
                        CellValue = value
                    });
                }

                if (!isIgnoreRow)
                    rowDatas.Add(rowData);
            }


            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var insertedData = new Dictionary<int, CustomerModel>();

                    // Insert data
                    foreach (var rowData in rowDatas)
                    {
                        var rowInput = new Dictionary<string, string>();

                        var customerInfo = new CustomerModel()
                        {
                            CustomerStatusId = EnumCustomerStatus.Actived
                        };

                        foreach (var cellData in rowData)
                        {
                            if (string.IsNullOrWhiteSpace(cellData.FieldMapping.FieldName) || string.IsNullOrWhiteSpace(cellData.CellValue)) continue;

                            if (cellData.PropertyInfo.Name == nameof(CustomerModel.CustomerTypeId))
                            {
                                if (cellData.CellValue.NormalizeAsInternalName().Equals(EnumCustomerType.Personal.GetEnumDescription().NormalizeAsInternalName()))
                                {
                                    customerInfo.CustomerTypeId = EnumCustomerType.Personal;
                                }
                                else
                                {
                                    customerInfo.CustomerTypeId = EnumCustomerType.Organization;
                                }
                            }
                            else
                            {
                                cellData.PropertyInfo.SetValue(customerInfo, cellData.CellValue.ConvertValueByType(cellData.PropertyInfo.PropertyType));
                            }
                        }

                        var context = new ValidationContext(customerInfo);
                        ICollection<ValidationResult> results = new List<ValidationResult>();
                        bool isValid = Validator.TryValidateObject(customerInfo, context, results, true);
                        if (!isValid)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, string.Join(", ", results.FirstOrDefault()?.MemberNames) + ": " + results.FirstOrDefault()?.ErrorMessage);
                        }

                        var customerId = await AddCustomerToDb(customerInfo);

                        insertedData.Add(customerId, customerInfo);
                    }

                    trans.Commit();

                    foreach (var item in insertedData)
                    {
                        await _activityLogService.CreateLog(EnumObjectType.Customer, item.Key, $"Import đối tác {item.Value.CustomerName}", item.Value.JsonSerialize());
                    }

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "ImportCustomerFromMapping");
                    throw;
                }
            }

            return true;

        }

    }
}
