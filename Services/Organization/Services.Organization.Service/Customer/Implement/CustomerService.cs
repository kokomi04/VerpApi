﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.Customer;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Service.Customer.Implement.Facade;
using static Verp.Resources.Organization.Customer.CustomerValidationMessage;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;

namespace VErp.Services.Organization.Service.Customer.Implement
{
    public class CustomerService : ICustomerService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly IMapper _mapper;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IUserHelperService _userHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ObjectActivityLogFacade _customerActivityLog;

        public CustomerService(OrganizationDBContext organizationContext
            , IMapper mapper
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , ICategoryHelperService httpCategoryHelperService
            , IUserHelperService userHelperService
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _organizationContext = organizationContext;
            _mapper = mapper;
            _httpCategoryHelperService = httpCategoryHelperService;
            _userHelperService = userHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _customerActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Customer);
        }

        public async Task<int> AddCustomer(int updatedUserId, CustomerModel data)
        {
            var result = await AddBatchCustomers(new[] { data });




            return result.First().Key.CustomerId;

            //using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            //{
            //    var customerId = await AddCustomerToDb(data);
            //    trans.Commit();
            //    await _activityLogService.CreateLog(EnumObjectType.Customer, customerId, $"Thêm đối tác {data.CustomerName}", data.JsonSerialize());
            //    return customerId;
            //}
        }


        //public async Task<int> AddCustomerToDb(CustomerModel data)
        //{
        //    var existedCustomer = await _organizationContext.Customer.FirstOrDefaultAsync(s => s.CustomerCode == data.CustomerCode || s.CustomerName == data.CustomerName);

        //    if (existedCustomer != null)
        //    {
        //        if (string.Compare(existedCustomer.CustomerCode, data.CustomerCode, StringComparison.OrdinalIgnoreCase) == 0)
        //        {
        //            throw new BadRequestException(CustomerErrorCode.CustomerCodeAlreadyExisted, $"Mã đối tác \"{data.CustomerCode}\" đã tồn tại");
        //        }

        //        throw new BadRequestException(CustomerErrorCode.CustomerNameAlreadyExisted, $"Tên đối tác \"{data.CustomerName}\" đã tồn tại");
        //    }

        //    var customer = new CustomerEntity()
        //    {
        //        CustomerCode = data.CustomerCode,
        //        CustomerName = data.CustomerName,
        //        CustomerTypeId = (int)data.CustomerTypeId,
        //        Address = data.Address,
        //        TaxIdNo = data.TaxIdNo,
        //        PhoneNumber = data.PhoneNumber,
        //        Website = data.Website,
        //        Email = data.Email,
        //        Description = data.Description,
        //        IsActived = data.IsActived,
        //        IsDeleted = false,
        //        LegalRepresentative = data.LegalRepresentative,
        //        CreatedDatetimeUtc = DateTime.UtcNow,
        //        UpdatedDatetimeUtc = DateTime.UtcNow,
        //        CustomerStatusId = (int)data.CustomerStatusId,
        //        Identify = data.Identify
        //    };

        //    await _organizationContext.Customer.AddAsync(customer);
        //    await _organizationContext.SaveChangesAsync();

        //    if (data.Contacts != null && data.Contacts.Count > 0)
        //    {
        //        await _organizationContext.CustomerContact.AddRangeAsync(data.Contacts.Select(c => new CustomerContact()
        //        {
        //            CustomerId = customer.CustomerId,
        //            FullName = c.FullName,
        //            GenderId = (int?)c.GenderId,
        //            Position = c.Position,
        //            PhoneNumber = c.PhoneNumber,
        //            Email = c.Email,
        //            IsDeleted = false,
        //            CreatedDatetimeUtc = DateTime.UtcNow,
        //            UpdatedDatetimeUtc = DateTime.UtcNow
        //        }));

        //        await _organizationContext.SaveChangesAsync();
        //    }

        //    if (data.BankAccounts != null && data.BankAccounts.Count > 0)
        //    {
        //        await _organizationContext.CustomerBankAccount.AddRangeAsync(data.BankAccounts.Select(ba => new CustomerBankAccount()
        //        {
        //            CustomerId = customer.CustomerId,
        //            BankName = ba.BankName,
        //            AccountNumber = ba.AccountNumber,
        //            SwiffCode = ba.SwiffCode,
        //            UpdatedUserId = _currentContextService.UserId,
        //            IsDeleted = false,
        //            CreatedDatetimeUtc = DateTime.UtcNow,
        //            UpdatedDatetimeUtc = DateTime.UtcNow
        //        }));

        //        await _organizationContext.SaveChangesAsync();
        //    }
        //    return customer.CustomerId;

        //}

        public async Task<Dictionary<CustomerEntity, CustomerModel>> AddBatchCustomers(IList<CustomerModel> customers)
        {
            using (var transaction = _organizationContext.Database.BeginTransaction())
            {
                Dictionary<CustomerEntity, CustomerModel> originData = await AddBatchCustomersBase(customers);
                transaction.Commit();

                return originData;
            }
        }

        public async Task<Dictionary<CustomerEntity, CustomerModel>> AddBatchCustomersBase(IList<CustomerModel> customers)
        {
            var genCodeContexts = new List<GenerateCodeContext>();
            var baseValueChains = new Dictionary<string, int>();

            var cates = await _organizationContext.CustomerCate.ToListAsync();

            foreach (var c in customers)
                genCodeContexts.Add(await GenerateCustomerCode(cates, null, c, baseValueChains));

            await ValidateCustomerModels(customers);

            var (customerEntities, originData, contacts, bankAccounts, attachments) = ConvertToCustomerEntities(customers);

            await _organizationContext.InsertByBatch(customerEntities);

            var contactEntities = new List<CustomerContact>();
            var bankAccountEntities = new List<CustomerBankAccount>();
            var customerAttachments = new List<CustomerAttachment>();

            foreach (var entity in customerEntities)
            {
                entity.PartnerId = string.Concat("KH", entity.CustomerId);

                foreach (var contact in contacts[entity])
                {
                    contact.CustomerId = entity.CustomerId;
                }

                foreach (var bacnkAcc in bankAccounts[entity])
                {
                    bacnkAcc.CustomerId = entity.CustomerId;
                }

                foreach (var attach in attachments[entity])
                {
                    attach.CustomerId = entity.CustomerId;
                }


                contactEntities.AddRange(contacts[entity]);
                bankAccountEntities.AddRange(bankAccounts[entity]);
                customerAttachments.AddRange(attachments[entity]);
            }

            await _organizationContext.UpdateByBatch(customerEntities, false);
            await _organizationContext.InsertByBatch(contactEntities, false);
            await _organizationContext.InsertByBatch(bankAccountEntities, false);
            await _organizationContext.InsertByBatch(customerAttachments, false);

            _organizationContext.SaveChanges();

            foreach (var ctx in genCodeContexts)
                await ctx.ConfirmCode();
            foreach (var c in originData)
            {
                await _customerActivityLog.LogBuilder(() => CustomerActivityLogMessage.Create)
                  .MessageResourceFormatDatas(c.Key.CustomerCode)
                  .ObjectId(c.Key.CustomerId)
                  .JsonData(c.Value.JsonSerialize())
                  .CreateLog();
            }

            return originData;
        }

        public async Task<bool> DeleteCustomer(int customerId)
        {

            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                throw new BadRequestException(CustomerErrorCode.CustomerNotFound);
            }

            var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var checkParams = new[]
            {
                new SqlParameter("@CustomerId",customerId),
                isInUsed
            };

            await _organizationContext.ExecuteStoreProcedure("asp_Customer_CheckUsed", checkParams);

            if (isInUsed.Value as bool? == true)
            {
                throw CanNotDeleteCustomerWhichIsInUse.BadRequest(ProductErrorCode.ProductInUsed);
            }

            var customerContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            foreach (var c in customerContacts)
            {
                c.IsDeleted = true;
                c.UpdatedDatetimeUtc = DateTime.UtcNow;
            }

            var customerBanks = await _organizationContext.CustomerBankAccount.Where(c => c.CustomerId == customerId).ToListAsync();
            foreach (var c in customerBanks)
            {
                c.IsDeleted = true;
                c.UpdatedDatetimeUtc = DateTime.UtcNow;
            }


            var customerFiles = await _organizationContext.CustomerAttachment.Where(c => c.CustomerId == customerId).ToListAsync();
            foreach (var c in customerFiles)
            {
                c.IsDeleted = true;
                c.UpdatedDatetimeUtc = DateTime.UtcNow;
            }


            customerInfo.IsDeleted = true;
            customerInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _organizationContext.SaveChangesAsync();

            await _customerActivityLog.LogBuilder(() => CustomerActivityLogMessage.Delete)
            .MessageResourceFormatDatas(customerInfo.CustomerCode)
            .ObjectId(customerInfo.CustomerId)
            .JsonData(customerInfo.JsonSerialize())
            .CreateLog();
            return true;
        }

        public async Task<CustomerModel> GetCustomerInfo(int customerId)
        {
            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                throw new BadRequestException(CustomerErrorCode.CustomerNotFound);
            }
            var customerContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            var bankAccounts = await _organizationContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();
            var customerAttachments = await _organizationContext.CustomerAttachment.Where(at => at.CustomerId == customerId).ProjectTo<CustomerAttachmentModel>(_mapper.ConfigurationProvider).ToListAsync();


            return new CustomerModel()
            {
                CustomerName = customerInfo.CustomerName,
                CustomerCode = customerInfo.CustomerCode,
                CustomerCateId = customerInfo.CustomerCateId,
                LogoFileId = customerInfo.LogoFileId,
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

                DebtDays = customerInfo.DebtDays,
                DebtLimitation = customerInfo.DebtLimitation,
                DebtBeginningTypeId = (EnumBeginningType)customerInfo.DebtBeginningTypeId,
                DebtManagerUserId = customerInfo.DebtManagerUserId,
                LoanDays = customerInfo.LoanDays,
                LoanLimitation = customerInfo.LoanLimitation,
                LoanBeginningTypeId = (EnumBeginningType)customerInfo.LoanBeginningTypeId,
                LoanManagerUserId = customerInfo.LoanManagerUserId,


                Contacts = customerContacts.Select(c => new CustomerContactModel()
                {
                    CustomerContactId = c.CustomerContactId,
                    FullName = c.FullName,
                    GenderId = (EnumGender?)c.GenderId,
                    Position = c.Position,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email
                }).ToList(),
                BankAccounts = bankAccounts.Select(ba => TransformBankAccModel(ba)).ToList(),
                CustomerAttachments = customerAttachments
            };
        }



        public async Task<PageData<CustomerListOutput>> GetList(string keyword, int? customerCateId, IList<int> customerIds, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null)
        {
            var lst = await GetListEntity(keyword, customerCateId, customerIds, customerStatusId, page, size, filters);
            var lstData = _mapper.Map<List<CustomerListOutput>>(lst.List);
            return (lstData, lst.Total);
        }

        private async Task<PageData<CustomerEntity>> GetListEntity(string keyword, int? customerCateId, IList<int> customerIds, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _organizationContext.Customer.AsQueryable();
            if (customerCateId > 0)
            {
                query = query.Where(c => c.CustomerCateId == customerCateId);
            }
            if (customerIds != null && customerIds.Count > 0)
            {
                query = query.Where(c => customerIds.Contains(c.CustomerId));
            }

            query = query.InternalFilter(filters);

            if (customerStatusId.HasValue)
            {
                query = from u in query
                        where
                        u.CustomerStatusId == (int)customerStatusId.Value
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


        public async Task<(Stream stream, string fileName, string contentType)> ExportList(IList<string> fieldNames, string keyword, int? customerCateId, IList<int> customerIds, EnumCustomerStatus? customerStatusId, int page, int size, Clause filters = null)
        {
            var lst = await GetListEntity(keyword, customerCateId, customerIds, customerStatusId, page, size, filters);
            var bomExport = new CusomerExportFacade(_organizationContext, _httpCategoryHelperService, _userHelperService, fieldNames);
            return await bomExport.Export(lst.List);
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
                    CustomerCateId = c.CustomerCateId,
                    LogoFileId = c.LogoFileId,
                    CustomerTypeId = (EnumCustomerType)c.CustomerTypeId,
                    Address = c.Address,
                    TaxIdNo = c.TaxIdNo,
                    PhoneNumber = c.PhoneNumber,
                    Website = c.Website,
                    Email = c.Email,
                    Identify = c.Identify,
                    DebtDays = c.DebtDays,
                    DebtLimitation = c.DebtLimitation,
                    DebtBeginningTypeId = (EnumBeginningType)c.DebtBeginningTypeId,
                    DebtManagerUserId = c.DebtManagerUserId,
                    LoanDays = c.LoanDays,
                    LoanLimitation = c.LoanLimitation,
                    LoanBeginningTypeId = (EnumBeginningType)c.LoanBeginningTypeId,
                    LoanManagerUserId = c.LoanManagerUserId,
                    CustomerStatusId = (EnumCustomerStatus)c.CustomerStatusId
                }
            ).ToListAsync();
        }

        public async Task<bool> UpdateCustomer(int customerId, CustomerModel data)
        {

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
                    CustomerEntity customerInfo = await UpdateCustomerBase(customerId, data);
                    trans.Commit();


                    return true;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }
        }

        public async Task<CustomerEntity> UpdateCustomerBase(int customerId, CustomerModel data, bool igDeleteRef = false)
        {
            var customerInfo = await _organizationContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customerInfo == null)
            {
                throw new BadRequestException(CustomerErrorCode.CustomerNotFound);
            }

            var checkExisted = _organizationContext.Customer.Any(q => q.CustomerId != customerId && q.CustomerCode == data.CustomerCode);
            if (checkExisted)
                throw new BadRequestException(CustomerErrorCode.CustomerCodeAlreadyExisted);

            var dbContacts = await _organizationContext.CustomerContact.Where(c => c.CustomerId == customerId).ToListAsync();
            var dbBankAccounts = await _organizationContext.CustomerBankAccount.Where(ba => ba.CustomerId == customerId).ToListAsync();
            var dbCustomerAttachments = await _organizationContext.CustomerAttachment.Where(a => a.CustomerId == customerId).ToListAsync();


            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.LegalRepresentative))
                customerInfo.LegalRepresentative = data.LegalRepresentative;

            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.CustomerCode))
                customerInfo.CustomerCode = data.CustomerCode;

            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.CustomerName))
                customerInfo.CustomerName = data.CustomerName;
            if (!igDeleteRef || data.CustomerCateId > 0)
                customerInfo.CustomerCateId = data.CustomerCateId;

            if (data.CustomerTypeId.HasValue)
                customerInfo.CustomerTypeId = (int)data.CustomerTypeId;

            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.Address))
                customerInfo.Address = data.Address;

            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.TaxIdNo))
                customerInfo.TaxIdNo = data.TaxIdNo;
            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.PhoneNumber))
                customerInfo.PhoneNumber = data.PhoneNumber;
            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.Website))
                customerInfo.Website = data.Website;
            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.Email))
                customerInfo.Email = data.Email;
            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.Identify))
                customerInfo.Identify = data.Identify;
            if (!igDeleteRef || data.DebtDays.HasValue)
                customerInfo.DebtDays = data.DebtDays;
            if (!igDeleteRef || data.DebtDays.HasValue)
                customerInfo.DebtLimitation = data.DebtLimitation;
            if (!igDeleteRef || data.DebtBeginningTypeId.HasValue)
                customerInfo.DebtBeginningTypeId = (int)data.DebtBeginningTypeId;
            if (!igDeleteRef || data.DebtManagerUserId.HasValue)
                customerInfo.DebtManagerUserId = data.DebtManagerUserId;
            if (!igDeleteRef || data.LoanDays.HasValue)
                customerInfo.LoanDays = data.LoanDays;
            if (!igDeleteRef || data.LoanLimitation.HasValue)
                customerInfo.LoanLimitation = data.LoanLimitation;
            customerInfo.LoanBeginningTypeId = (int)data.LoanBeginningTypeId;
            if (!igDeleteRef || data.LoanManagerUserId.HasValue)
                customerInfo.LoanManagerUserId = data.LoanManagerUserId;
            if (!igDeleteRef || !string.IsNullOrWhiteSpace(data.Description))
                customerInfo.Description = data.Description;
            if (!igDeleteRef || data.IsActived.HasValue)
                customerInfo.IsActived = data.IsActived.Value;
            //customerInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
            if (!igDeleteRef || data.CustomerStatusId.HasValue)
                customerInfo.CustomerStatusId = (int)data.CustomerStatusId;

            if (!igDeleteRef || data.LogoFileId.HasValue)
                customerInfo.LogoFileId = data.LogoFileId;

            if (data.Contacts == null)
            {
                data.Contacts = new List<CustomerContactModel>();
            }
            if (data.BankAccounts == null)
            {
                data.BankAccounts = new List<CustomerBankAccountModel>();
            }

            if (data.CustomerAttachments == null)
            {
                data.CustomerAttachments = new List<CustomerAttachmentModel>();
            }

            //1. contacts
            if (!igDeleteRef)
            {
                var deletedContacts = dbContacts.Where(c => !data.Contacts.Any(s => s.CustomerContactId == c.CustomerContactId)).ToList();
                foreach (var c in deletedContacts)
                {
                    c.IsDeleted = true;
                }
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

            //2. banks
            if (!igDeleteRef)
            {
                var deletedBankAccounts = dbBankAccounts.Where(ba => !data.BankAccounts.Any(s => s.BankAccountId == ba.CustomerBankAccountId)).ToList();

                foreach (var ba in deletedBankAccounts)
                {
                    ba.IsDeleted = true;
                }
            }


            var newBankAccounts = data.BankAccounts
                .Where(ba => ba.BankAccountId <= 0)
                .Select(ba => TransformBankAccEntity(customerId, ba));
            await _organizationContext.CustomerBankAccount.AddRangeAsync(newBankAccounts);

            foreach (var ba in dbBankAccounts)
            {
                var reqBankAccount = data.BankAccounts.FirstOrDefault(s => s.BankAccountId == ba.CustomerBankAccountId);
                if (reqBankAccount != null)
                {
                    ba.AccountNumber = reqBankAccount.AccountNumber;
                    ba.SwiffCode = reqBankAccount.SwiffCode;
                    ba.BankName = reqBankAccount.BankName;
                    ba.BankAddress = reqBankAccount.BankAddress;
                    ba.BankBranch = reqBankAccount.BankBranch;
                    ba.BankCode = reqBankAccount.BankCode;
                    ba.AccountName = reqBankAccount.AccountName;
                    ba.Province = reqBankAccount.Province;
                    ba.CurrencyId = reqBankAccount.CurrencyId;
                    ba.UpdatedDatetimeUtc = DateTime.UtcNow;
                }
            }

            //3. Attachments
            foreach (var attach in dbCustomerAttachments)
            {
                var change = data.CustomerAttachments.FirstOrDefault(x => x.CustomerAttachmentId == attach.CustomerAttachmentId);
                if (change != null)
                    _mapper.Map(change, attach);
                else
                {
                    if (!igDeleteRef)
                        attach.IsDeleted = true;
                }
            }
            var newAttachment = data.CustomerAttachments.AsQueryable()
                .Where(x => !(x.CustomerAttachmentId > 0))
                .ProjectTo<CustomerAttachment>(_mapper.ConfigurationProvider).ToList();
            newAttachment.ForEach(x => x.CustomerId = customerInfo.CustomerId);
            await _organizationContext.CustomerAttachment.AddRangeAsync(newAttachment);


            await _organizationContext.SaveChangesAsync();

            await _customerActivityLog.LogBuilder(() => CustomerActivityLogMessage.Update)
                  .MessageResourceFormatDatas(customerInfo.CustomerCode)
                  .ObjectId(customerInfo.CustomerId)
                  .JsonData(customerInfo.JsonSerialize())
                  .CreateLog();

            return customerInfo;
        }

        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "Customer",
                CategoryTitle = "Đối tác",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = ExcelUtils.GetFieldNameModels<BaseCustomerImportModel>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportCustomerFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var importFacade = new CusomerImportFacade(this, _customerActivityLog, _mapper, _httpCategoryHelperService, _organizationContext);

            var customerModels = await importFacade.ParseCustomerFromMapping(mapping, stream);

            // var insertedData = await AddBatchCustomers(customerModels);


            // foreach (var item in insertedData)
            // {
            //     await _activityLogService.CreateLog(EnumObjectType.Customer, item.Key.CustomerId, $"Import đối tác {item.Value.CustomerName}", item.Value.JsonSerialize());
            // }

            return true;

        }


        private async Task ValidateCustomerModels(IList<CustomerModel> customers)
        {
            foreach (var c in customers)
            {
                if (string.IsNullOrWhiteSpace(c.CustomerCode))
                {
                    throw CustomerCodeIsRequired.BadRequest();
                }
            }
            var customerCodes = customers.Select(c => c.CustomerCode).ToList();

            var customerNames = customers.Select(c => c.CustomerName).ToList();

            var existedCustomers = await _organizationContext.Customer.Where(s => customerCodes.Contains(s.CustomerCode) || customerNames.Contains(s.CustomerName)).ToListAsync();

            if (existedCustomers != null && existedCustomers.Count > 0)
            {
                var existedCodes = existedCustomers.Select(c => c.CustomerCode).ToList();
                var existingCodes = existedCodes.Intersect(customerCodes, StringComparer.OrdinalIgnoreCase);

                if (existingCodes.Count() > 0)
                {
                    throw CustomerCodeAlreadyExists.BadRequestFormat(string.Join(", ", existingCodes));
                }

                throw CustomerNameAlreadyExists.BadRequestFormat(string.Join(", ", existedCustomers.Select(c => c.CustomerName)));
            }


        }

        private (IList<CustomerEntity> customerEntities,
            Dictionary<CustomerEntity, CustomerModel> originData,
            Dictionary<CustomerEntity, List<CustomerContact>> contacts,
            Dictionary<CustomerEntity, List<CustomerBankAccount>> bankAccounts,
            Dictionary<CustomerEntity, List<CustomerAttachment>> attachments
            )
            ConvertToCustomerEntities(IList<CustomerModel> customers)
        {
            var customerEntities = new List<CustomerEntity>();
            var originData = new Dictionary<CustomerEntity, CustomerModel>();
            var contacts = new Dictionary<CustomerEntity, List<CustomerContact>>();
            var bankAccounts = new Dictionary<CustomerEntity, List<CustomerBankAccount>>();
            var attachments = new Dictionary<CustomerEntity, List<CustomerAttachment>>();


            foreach (var data in customers)
            {
                var customer = new CustomerEntity()
                {
                    CustomerCode = data.CustomerCode,
                    CustomerName = data.CustomerName,
                    CustomerCateId = data.CustomerCateId,
                    LogoFileId = data.LogoFileId,
                    CustomerTypeId = (int)(data.CustomerTypeId ?? EnumCustomerType.Organization),
                    Address = data.Address,
                    TaxIdNo = data.TaxIdNo,
                    PhoneNumber = data.PhoneNumber,
                    Website = data.Website,
                    Email = data.Email,
                    Description = data.Description,
                    IsActived = data.IsActived ?? false,
                    IsDeleted = false,
                    LegalRepresentative = data.LegalRepresentative,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    CustomerStatusId = (int)(data.CustomerStatusId ?? EnumCustomerStatus.Actived),
                    Identify = data.Identify,
                    DebtDays = data.DebtDays,
                    DebtLimitation = data.DebtLimitation,
                    DebtBeginningTypeId = (int)(data.DebtBeginningTypeId ?? EnumBeginningType.BillDate),
                    DebtManagerUserId = data.DebtManagerUserId,
                    LoanDays = data.LoanDays,
                    LoanLimitation = data.LoanLimitation,
                    LoanBeginningTypeId = (int)data.LoanBeginningTypeId,
                    LoanManagerUserId = data.LoanManagerUserId
                };
                customerEntities.Add(customer);

                originData.Add(customer, data);
                contacts.Add(customer, new List<CustomerContact>());
                bankAccounts.Add(customer, new List<CustomerBankAccount>());
                attachments.Add(customer, new List<CustomerAttachment>());


                if (data.Contacts != null && data.Contacts.Count > 0)
                {
                    contacts[customer].AddRange(data.Contacts.Select(c => new CustomerContact()
                    {
                        //CustomerId = customer.CustomerId,
                        FullName = c.FullName,
                        GenderId = (int?)c.GenderId,
                        Position = c.Position,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        IsDeleted = false,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow
                    }));
                }

                if (data.BankAccounts != null && data.BankAccounts.Count > 0)
                {
                    bankAccounts[customer].AddRange(data.BankAccounts.Select(ba => TransformBankAccEntity(0, ba)));
                }

                if (data.CustomerAttachments != null && data.CustomerAttachments.Count > 0)
                {
                    attachments[customer].AddRange(data.CustomerAttachments.Select(attach => new CustomerAttachment()
                    {
                        Title = attach.Title,
                        AttachmentFileId = attach.AttachmentFileId,
                        IsDeleted = false,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                    }));
                }

            }

            return (customerEntities, originData, contacts, bankAccounts, attachments);
        }

        private CustomerBankAccountModel TransformBankAccModel(CustomerBankAccount entity)
        {
            if (entity == null) return null;
            return new CustomerBankAccountModel()
            {
                BankAccountId = entity.CustomerBankAccountId,
                BankName = entity.BankName,
                AccountNumber = entity.AccountNumber,
                SwiffCode = entity.SwiffCode,
                BankAddress = entity.BankAddress,
                BankBranch = entity.BankBranch,
                BankCode = entity.BankCode,
                AccountName = entity.AccountName,
                Province = entity.Province,
                CurrencyId = entity.CurrencyId
            };
        }

        private CustomerBankAccount TransformBankAccEntity(int customerId, CustomerBankAccountModel model)
        {
            if (model == null) return null;
            return new CustomerBankAccount()
            {
                CustomerId = customerId,
                CustomerBankAccountId = model.BankAccountId,
                BankName = model.BankName,
                AccountNumber = model.AccountNumber,
                SwiffCode = model.SwiffCode,
                BankAddress = model.BankAddress,
                BankBranch = model.BankBranch,
                BankCode = model.BankCode,
                AccountName = model.AccountName,
                Province = model.Province,
                CurrencyId = model.CurrencyId,
                IsDeleted = false,
            };
        }

        private async Task<GenerateCodeContext> GenerateCustomerCode(IList<CustomerCate> cates, int? customerId, CustomerModel model, Dictionary<string, int> baseValueChains)
        {
            model.CustomerCode = (model.CustomerCode ?? "").Trim();

            var cateInfo = cates.FirstOrDefault(c => c.CustomerCateId == model.CustomerCateId);
            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

            var code = await ctx
                .SetConfig(EnumObjectType.Customer)
                .SetConfigData(customerId ?? 0, null, cateInfo?.CustomerCateCode)
                .TryValidateAndGenerateCode(_organizationContext.Customer, model.CustomerCode, (s, code) => s.CustomerId != customerId && s.CustomerCode == code);

            model.CustomerCode = code;

            return ctx;

        }
    }
}
