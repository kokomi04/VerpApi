using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Services.Organization.Model.Customer;
using CustomerEntity = VErp.Infrastructure.EF.OrganizationDB.Customer;
using static VErp.Commons.GlobalObject.InternalDataInterface.BaseCustomerImportModelExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using static VErp.Commons.Constants.CurrencyCateConstants;
using static VErp.Commons.Constants.CategoryFieldConstants;


namespace VErp.Services.Organization.Service.Customer.Implement.Facade
{
    public class CusomerExportFacade
    {
        private readonly OrganizationDBContext _organizationContext;
        private ISheet sheet = null;
        private int currentRow = 0;

        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IUserHelperService _userHelperService;

        private readonly IList<CategoryFieldNameModel> fields;
        private readonly IList<string> groups;

        private IDictionary<int, string> users;
        private IDictionary<int, string> currencies;

        public CusomerExportFacade(
            OrganizationDBContext organizationContext,
            ICategoryHelperService httpCategoryHelperService,
            IUserHelperService userHelperService,
            IList<string> fieldNames)
        {
            _organizationContext = organizationContext;
            _httpCategoryHelperService = httpCategoryHelperService;
            _userHelperService = userHelperService;

            fields = Utils.GetFieldNameModels<BaseCustomerImportModel>().Where(f => fieldNames == null || fieldNames.Count == 0 || fieldNames.Contains(f.FieldName)).ToList();
            groups = fields.Select(g => g.GroupName).Distinct().ToList();
        }


        public async Task<(Stream stream, string fileName, string contentType)> Export(IList<CustomerEntity> customers)
        {
            var currencyData = await _httpCategoryHelperService.GetDataRows(CurrencyCategoryCode, new CategoryFilterModel());
            currencies = currencyData.List.ToDictionary(c => Convert.ToInt32(c[F_Id]), c => c[CurrencyCode]?.ToString());

            var userIds = customers.SelectMany(c => new[] { c.DebtManagerUserId, c.LoanManagerUserId }).Where(c => c.HasValue).Select(c => c.Value).ToList();

            var userInfos = await _userHelperService.GetByIds(userIds);
            users = userInfos.ToDictionary(u => u.UserId, u => u.FullName);

             var xssfwb = new XSSFWorkbook();
            sheet = xssfwb.CreateSheet();


            await WriteTable(customers);

            var currentRowTmp = currentRow;

            if (sheet.LastRowNum < 1000)
            {
                for (var i = 0; i < fields.Count + 1; i++)
                {
                    sheet.AutoSizeColumn(i, false);
                }
            }
            else
            {
                for (var i = 0; i < fields.Count + 1; i++)
                {
                    sheet.ManualResize(i, columnMaxLineLength[i]);
                }
            }

            currentRow = currentRowTmp;


            var stream = new MemoryStream();
            xssfwb.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"customer-list-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            return (stream, fileName, contentType);
        }

        private async Task<string> WriteTable(IList<CustomerEntity> customers)
        {
            currentRow = 1;

            var fRow = currentRow;
            var sRow = currentRow;

            sheet.EnsureCell(fRow, 0).SetCellValue($"STT");
            sheet.SetHeaderCellStyle(fRow, 0);

            var sColIndex = 1;
            if (groups.Count > 0)
            {
                sRow = fRow + 1;
            }

            columnMaxLineLength = new List<int>(fields.Count + 1);
            columnMaxLineLength.Add(5);
            foreach (var g in groups)
            {
                var groupCols = fields.Where(f => f.GroupName == g);
                sheet.EnsureCell(fRow, sColIndex).SetCellValue(g);
                sheet.SetHeaderCellStyle(fRow, sColIndex);

                if (groupCols.Count() > 1)
                {
                    var region = new CellRangeAddress(fRow, fRow, sColIndex, sColIndex + groupCols.Count() - 1);
                    sheet.AddMergedRegion(region);
                    RegionUtil.SetBorderBottom(1, region, sheet);
                    RegionUtil.SetBorderLeft(1, region, sheet);
                    RegionUtil.SetBorderRight(1, region, sheet);
                    RegionUtil.SetBorderTop(1, region, sheet);
                }

                foreach (var f in groupCols)
                {
                    sheet.EnsureCell(sRow, sColIndex).SetCellValue(f.FieldTitle);
                    columnMaxLineLength.Add(f.FieldTitle?.Length ?? 10);

                    sheet.SetHeaderCellStyle(sRow, sColIndex);
                    sColIndex++;
                }
            }

            currentRow = sRow + 1;

            return await WriteTableDetailData(customers);
        }


        private IList<int> columnMaxLineLength = new List<int>();
        private (EnumDataType type, object value) GetProductValue(
            CustomerEntity customer,
            IList<CustomerContact> customerContacts,
            IList<CustomerBankAccount> customerBanks,
            string fieldName,
            out bool isFormula
            )
        {
            try
            {
                isFormula = false;

                switch (fieldName)
                {
                    case nameof(BaseCustomerImportModel.CustomerCode):
                        return (EnumDataType.Text, customer.CustomerCode);
                    case nameof(BaseCustomerImportModel.CustomerName):
                        return (EnumDataType.Text, customer.CustomerName);
                    case nameof(BaseCustomerImportModel.CustomerTypeId):
                        return (EnumDataType.Text, customer.CustomerTypeId.GetEnumDescription<EnumCustomerType>());
                    case nameof(BaseCustomerImportModel.Address):
                        return (EnumDataType.Text, customer.Address);
                    case nameof(BaseCustomerImportModel.TaxIdNo):
                        return (EnumDataType.Text, customer.TaxIdNo);
                    case nameof(BaseCustomerImportModel.PhoneNumber):
                        return (EnumDataType.Text, customer.PhoneNumber);
                    case nameof(BaseCustomerImportModel.Website):
                        return (EnumDataType.Text, customer.Website);
                    case nameof(BaseCustomerImportModel.Email):
                        return (EnumDataType.Text, customer.Email);
                    case nameof(BaseCustomerImportModel.Description):
                        return (EnumDataType.Text, customer.Description);
                    case nameof(BaseCustomerImportModel.LegalRepresentative):
                        return (EnumDataType.Text, customer.LegalRepresentative);
                    case nameof(BaseCustomerImportModel.Identify):
                        return (EnumDataType.Text, customer.Identify);
                    case nameof(BaseCustomerImportModel.DebtDays):
                        return (EnumDataType.Int, customer.DebtDays);
                    case nameof(BaseCustomerImportModel.DebtLimitation):
                        return (EnumDataType.Decimal, customer.DebtLimitation);
                    case nameof(BaseCustomerImportModel.DebtBeginningTypeId):
                        return (EnumDataType.Text, customer.DebtBeginningTypeId.GetEnumDescription<EnumBeginningType>());
                    case nameof(BaseCustomerImportModel.DebtManagerUserId):
                        if (customer.DebtManagerUserId.HasValue && users.ContainsKey(customer.DebtManagerUserId.Value))
                        {
                            return (EnumDataType.Text, users[customer.DebtManagerUserId.Value]);
                        }
                        return (EnumDataType.Text, null);
                    case nameof(BaseCustomerImportModel.LoanDays):
                        return (EnumDataType.Int, customer.LoanDays);
                    case nameof(BaseCustomerImportModel.LoanLimitation):
                        return (EnumDataType.Decimal, customer.LoanLimitation);
                    case nameof(BaseCustomerImportModel.LoanBeginningTypeId):
                        return (EnumDataType.Text, customer.LoanBeginningTypeId.GetEnumDescription<EnumBeginningType>());
                    case nameof(BaseCustomerImportModel.LoanManagerUserId):
                        if (customer.LoanManagerUserId.HasValue && users.ContainsKey(customer.LoanManagerUserId.Value))
                        {
                            return (EnumDataType.Text, users[customer.LoanManagerUserId.Value]);
                        }
                        return (EnumDataType.Text, null);

                }

                bool isMatch = false;
                var contact = GetContact(ref isMatch, customer, customerContacts, fieldName);
                if (isMatch) return contact;

                var bankAcc = GetBankAcc(ref isMatch, customer, customerBanks, fieldName);
                if (isMatch) return bankAcc;

            }
            catch (Exception e)
            {

                throw;
            }
            
            return (EnumDataType.Text, "");

        }

        private (EnumDataType type, object value) GetContact(ref bool isMatch, CustomerEntity customer, IList<CustomerContact> contacts, string fieldName)
        {
            var contactField = ContactFieldPrefix.FirstOrDefault(f => fieldName.StartsWith(f));
            if (contactField != null)
            {
                isMatch = true;
                var suffix = int.Parse(fieldName.Substring(contactField.Length));
                var index = suffix - 1;
                if (contacts.Count > index)
                {
                    var contact = contacts[index];

                    if (contactField == ContactName)
                    {
                        return (EnumDataType.Text, contact.FullName);
                    }
                    if (contactField == ContactGender)
                    {
                        return (EnumDataType.Text, contact.GenderId?.GetEnumDescription<EnumGender>());
                    }
                    if (contactField == ContactPosition)
                    {
                        return (EnumDataType.Text, contact.Position);
                    }
                    if (contactField == ContactPhone)
                    {
                        return (EnumDataType.Text, contact.PhoneNumber);
                    }
                    if (contactField == ContactEmail)
                    {
                        return (EnumDataType.Text, contact.Email);
                    }
                }
            }
            return (EnumDataType.Text, null);
        }

        private (EnumDataType type, object value) GetBankAcc(ref bool isMatch, CustomerEntity customer, IList<CustomerBankAccount> bankAccs, string fieldName)
        {
            var field = BankAccFieldPrefix.FirstOrDefault(f => fieldName.StartsWith(f));
            if (field != null)
            {
                isMatch = true;
                var suffix = int.Parse(fieldName.Substring(field.Length));
                var index = suffix - 1;
                if (bankAccs.Count > index)
                {
                    var bankAcc = bankAccs[index];

                    if (field == BankAccAccountName)
                    {
                        return (EnumDataType.Text, bankAcc.AccountName);
                    }
                    if (field == BankAccBankName)
                    {
                        return (EnumDataType.Text, bankAcc.BankName);
                    }
                    if (field == BankAccAccountNo)
                    {
                        return (EnumDataType.Text, bankAcc.AccountNumber);
                    }
                    if (field == BankAccSwiffCode)
                    {
                        return (EnumDataType.Text, bankAcc.SwiffCode);
                    }
                    if (field == BankAccBrach)
                    {
                        return (EnumDataType.Text, bankAcc.BankBranch);
                    }
                    if (field == BankAccAddress)
                    {
                        return (EnumDataType.Text, bankAcc.BankAddress);
                    }
                    if (field == BankAccCurrency)
                    {
                        if (bankAcc.CurrencyId.HasValue && currencies.ContainsKey(bankAcc.CurrencyId.Value))
                        {
                            return (EnumDataType.Text, currencies[bankAcc.CurrencyId.Value]);
                        }

                        return (EnumDataType.Text, null);
                    }
                }
            }
            return (EnumDataType.Text, null);
        }


        private async Task<string> WriteTableDetailData(IList<CustomerEntity> customers)
        {
            var stt = 1;
            var customerIdPages = new List<IList<int>>();
            var idx = 0;
            var productIdPage = new List<int>();
            foreach (var p in customers.Select(p => p.CustomerId).ToList())
            {
                productIdPage.Add(p);
                idx++;

                if (idx % 1000 == 0)
                {
                    customerIdPages.Add(productIdPage);
                    productIdPage = new List<int>();
                }

            }

            if (productIdPage.Count > 0)
                customerIdPages.Add(productIdPage);


            var contacts = new Dictionary<int, IList<CustomerContact>>();
            var bankAccs = new Dictionary<int, IList<CustomerBankAccount>>();
            foreach (var customerIds in customerIdPages)
            {
                var sValidations = (await _organizationContext.CustomerContact.Where(s => customerIds.Contains(s.CustomerId)).ToListAsync())
                    .GroupBy(s => s.CustomerId)
                    .ToDictionary(s => s.Key, s => s.ToList());
                foreach (var s in sValidations)
                {
                    contacts.Add(s.Key, s.Value);
                }

                var banks = (await _organizationContext.CustomerBankAccount.Where(s => customerIds.Contains(s.CustomerId)).ToListAsync())
                    .GroupBy(s => s.CustomerId)
                    .ToDictionary(s => s.Key, s => s.ToList());
                foreach (var s in banks)
                {
                    bankAccs.Add(s.Key, s.Value);
                }
            }


            var textStyle = sheet.GetCellStyle(isBorder: true);
            var intStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,###");
            var decimalStyle = sheet.GetCellStyle(isBorder: true, hAlign: HorizontalAlignment.Right, dataFormat: "#,##0.00###");

            foreach (var p in customers)
            {
                var sColIndex = 1;
                sheet.EnsureCell(currentRow, 0, intStyle).SetCellValue(stt);

                foreach (var g in groups)
                {
                    var groupCols = fields.Where(f => f.GroupName == g);

                    foreach (var f in groupCols)
                    {
                        contacts.TryGetValue(p.CustomerId, out var customerContacts);
                        if (customerContacts == null) customerContacts = new List<CustomerContact>();

                        bankAccs.TryGetValue(p.CustomerId, out var customerBankAcc);
                        if (customerBankAcc == null) customerBankAcc = new List<CustomerBankAccount>();

                        var v = GetProductValue(p, customerContacts, customerBankAcc, f.FieldName, out var isFormula);
                        switch (v.type)
                        {
                            case EnumDataType.BigInt:
                            case EnumDataType.Int:
                                if (!v.IsNullObject())
                                {
                                    if (isFormula)
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                            .SetCellFormula(v.value?.ToString());
                                    }
                                    else
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, intStyle)
                                            .SetCellValue(Convert.ToDouble(v.value));
                                    }
                                }
                                else
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, intStyle);
                                }
                                break;
                            case EnumDataType.Decimal:
                                if (!v.IsNullObject())
                                {
                                    if (isFormula)
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                            .SetCellFormula(v.value?.ToString());
                                    }
                                    else
                                    {
                                        sheet.EnsureCell(currentRow, sColIndex, decimalStyle)
                                            .SetCellValue(Convert.ToDouble(v.value));
                                    }
                                }
                                else
                                {
                                    sheet.EnsureCell(currentRow, sColIndex, decimalStyle);
                                }
                                break;
                            default:
                                sheet.EnsureCell(currentRow, sColIndex, textStyle).SetCellValue(v.value?.ToString());
                                break;
                        }
                        if (v.value?.ToString()?.Length > columnMaxLineLength[sColIndex])
                        {
                            columnMaxLineLength[sColIndex] = v.value?.ToString()?.Length ?? 10;
                        }

                        sColIndex++;
                    }
                }
                currentRow++;
                stt++;
            }


            return "";
        }


    }
}
