using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Customer;
using VErp.Services.Master.Service.Activity;
using CustomerEntity = VErp.Infrastructure.EF.MasterDB.Customer;

namespace VErp.Services.Master.Service.Customer.Implement
{
    public class CustomerService : ICustomerService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public CustomerService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<CustomerService> logger
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<int>> AddCustomer(int updatedUserId, CustomerModel data)
        {
            var existedCustomer = await _masterContext.Customer.FirstOrDefaultAsync(s => s.CustomerCode == data.CustomerCode || s.CustomerName == data.CustomerName);

            if (existedCustomer != null)
            {
                if (string.Compare(existedCustomer.CustomerCode, data.CustomerCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CustomerErrorCode.CustomerCodeAlreadyExisted;
                }

                return CustomerErrorCode.CustomerNameAlreadyExisted;
            }
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                try
                {
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
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        CustomerStatusId = (int)data.CustomerStatusId
                    };

                    await _masterContext.Customer.AddAsync(customer);
                    await _masterContext.SaveChangesAsync();

                    if (data.Contacts != null && data.Contacts.Count > 0)
                    {
                        await _masterContext.CustomerContact.AddRangeAsync(data.Contacts.Select(c => new CustomerContact()
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

                        await _masterContext.SaveChangesAsync();
                    }

                    if (data.BankAccounts != null && data.BankAccounts.Count > 0)
                    {
                        await _masterContext.CustomerBankAccount.AddRangeAsync(data.BankAccounts.Select(ba => new CustomerBankAccount()
                        {
                            CustomerId = customer.CustomerId,
                            BankName = ba.BankName,
                            AccountNumber = ba.AccountNumber,
                            SwiffCode = ba.SwiffCode,
                            UpdatedUserId = updatedUserId,
                            IsDeleted = false,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow
                        }));

                        await _masterContext.SaveChangesAsync();
                    }
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Customer, customer.CustomerId, $"Thêm đối tác {customer.CustomerName}", data.JsonSerialize());
                    return customer.CustomerId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeleteCustomer(int customerId)
        {
            var customerInfo = await _masterContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }

            var customerContacts = await _masterContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            foreach (var c in customerContacts)
            {
                c.IsDeleted = true;
                c.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            customerInfo.IsDeleted = true;
            customerInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Customer, customerInfo.CustomerId, $"Xóa đối tác {customerInfo.CustomerName}", customerInfo.JsonSerialize());

            return GeneralCode.Success;
        }

        public async Task<ServiceResult<CustomerModel>> GetCustomerInfo(int customerId)
        {
            var customerInfo = await _masterContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }
            var customerContacts = await _masterContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            var bankAccounts = await _masterContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();

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

                Description = customerInfo.Description,
                IsActived = customerInfo.IsActived,
                CustomerStatusId = (EnumCustomerStatus)customerInfo.CustomerStatusId,
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

        public async Task<PageData<CustomerListOutput>> GetList(string keyword, EnumCustomerStatus? customerStatusId, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from c in _masterContext.Customer
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
                     CustomerStatusId = (EnumCustomerStatus)c.CustomerStatusId
                 }
             );
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
                from c in _masterContext.Customer
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
                    CustomerStatusId = (EnumCustomerStatus)c.CustomerStatusId
                }
            ).ToListAsync();
        }

        public async Task<Enum> UpdateCustomer(int updatedUserId, int customerId, CustomerModel data)
        {
            var customerInfo = await _masterContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                return CustomerErrorCode.CustomerNotFound;
            }

            var checkExisted = _masterContext.Customer.Any(q => q.CustomerId != customerId && q.CustomerCode == data.CustomerCode);
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
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var dbContacts = await _masterContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
                    var dbBankAccounts = await _masterContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();

                    customerInfo.CustomerCode = data.CustomerCode;
                    customerInfo.CustomerName = data.CustomerName;
                    customerInfo.CustomerTypeId = (int)data.CustomerTypeId;
                    customerInfo.Address = data.Address;
                    customerInfo.TaxIdNo = data.TaxIdNo;
                    customerInfo.PhoneNumber = data.PhoneNumber;
                    customerInfo.Website = data.Website;
                    customerInfo.Email = data.Email;
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
                    await _masterContext.CustomerContact.AddRangeAsync(newContacts);

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
                    await _masterContext.CustomerBankAccount.AddRangeAsync(newBankAccounts);

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

                    await _masterContext.SaveChangesAsync();
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
    }
}
