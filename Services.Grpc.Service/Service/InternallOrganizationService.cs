using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcProto.Protos;
using GrpcProto.Protos.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Grpc.Service
{
    public class InternallOrganizationService : OrganizationProvider.OrganizationProviderBase
    {
        private readonly OrganizationDBContext _organizationDBContext;

        public InternallOrganizationService(ILogger<InternallOrganizationService> logger,
            OrganizationDBContext organizationDBContext)
        {
            _organizationDBContext = organizationDBContext;
        }

        public async override Task<BusinessInfoModel> BusinessInfo(Empty request, ServerCallContext context)
        {
            var businessInfo = await _organizationDBContext.BusinessInfo.FirstOrDefaultAsync();
            BusinessInfoModel result = new BusinessInfoModel(); ;
            if (businessInfo != null)
            {
                result = new BusinessInfoModel
                {
                    CompanyName = businessInfo.CompanyName,
                    LegalRepresentative = businessInfo.LegalRepresentative,
                    Address = businessInfo.Address,
                    TaxIdNo = businessInfo.TaxIdNo,
                    Website = businessInfo.Website,
                    PhoneNumber = businessInfo.PhoneNumber,
                    Email = businessInfo.Email,
                    LogoFileId = (int)businessInfo.LogoFileId
                };
            }

            return result;
        }

        public async override Task<CustomerModel> CustomerInfo(CustomerInfoRequest request, ServerCallContext context)
        {
            var customerInfo = await _organizationDBContext.Customer.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);
            if (customerInfo == null)
            {
                throw new BadRequestException(CustomerErrorCode.CustomerNotFound);
            }

            var customerContacts = await _organizationDBContext.CustomerContact.Where(c => c.CustomerId == request.CustomerId).ToListAsync();
            var bankAccounts = await _organizationDBContext.CustomerBankAccount.Where(ba => ba.CustomerId == request.CustomerId).ToListAsync();

            var responses = new CustomerModel
            {
                CustomerName = customerInfo.CustomerName,
                CustomerCode = customerInfo.CustomerCode,
                CustomerTypeId = customerInfo.CustomerTypeId,
                Address = customerInfo.Address,
                TaxIdNo = customerInfo.TaxIdNo,
                PhoneNumber = customerInfo.PhoneNumber,
                Website = customerInfo.Website,
                Email = customerInfo.Email,
                LegalRepresentative = customerInfo.LegalRepresentative,
                Description = customerInfo.Description,
                IsActived = customerInfo.IsActived,
                CustomerStatusId = customerInfo.CustomerStatusId,
                Identify = customerInfo.Identify,
                DebtDays = customerInfo.DebtDays ?? 0
            };
            responses.Contacts.Add(customerContacts.Select(c => new CustomerContactModel()
            {
                CustomerContactId = c.CustomerContactId,
                FullName = c.FullName,
                GenderId = (int)c.GenderId,
                Position = c.Position,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            }).ToList());
            responses.BankAccounts.Add(bankAccounts.Select(ba => new CustomerBankAccountModel()
            {
                CustomerBankAccountId = ba.CustomerBankAccountId,
                BankName = ba.BankName,
                AccountNumber = ba.AccountNumber,
                SwiffCode = ba.SwiffCode

            }).ToList());

            return responses;
        }
    }
}
