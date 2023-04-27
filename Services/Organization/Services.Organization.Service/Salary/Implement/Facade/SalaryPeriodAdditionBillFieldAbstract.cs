using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using Verp.Resources.Organization;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.Constants;
using VErp.Services.Organization.Model.Salary;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{
    public abstract class SalaryPeriodAdditionBillFieldAbstract
    {
        private static IList<CategoryFieldNameModel> detailFields = ExcelUtils.GetFieldNameModels<SalaryPeriodAdditionBillEmployeeModel>();        
        private readonly CategoryFieldNameModel desField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.Description));

        protected readonly CategoryFieldNameModel employeeField = detailFields.First(f => f.FieldName == nameof(SalaryPeriodAdditionBillEmployeeModel.EmployeeId));
        protected readonly ICategoryHelperService categoryHelperService;
        public SalaryPeriodAdditionBillFieldAbstract(ICategoryHelperService categoryHelperService)
        {
            this.categoryHelperService = categoryHelperService;

        }
        protected async Task<IList<CategoryFieldNameModel>> GetFieldDetailsForMapping(SalaryPeriodAdditionType typeFullInfo)
        {

            var referTableNames = new List<string>() { OrganizationConstants.EMPLOYEE_CATEGORY_CODE };

            var referFields = await categoryHelperService.GetReferFields(referTableNames, null);
            var refCategoryFields = referFields.GroupBy(f => f.CategoryCode).ToDictionary(f => f.Key, f => f.ToList());

            if (!refCategoryFields.TryGetValue(OrganizationConstants.EMPLOYEE_CATEGORY_CODE, out var refCategory))
            {
                throw HrDataValidationMessage.RefTableNotFound.BadRequestFormat(OrganizationConstants.EMPLOYEE_CATEGORY_CODE);
            }

            var detailFields = new List<CategoryFieldNameModel>();
            const string detailGroupName = "Chi tiết";
            detailFields.Add(new CategoryFieldNameModel()
            {
                //CategoryFieldId = field.HrAreaFieldId,
                FieldName = employeeField.FieldName,
                FieldTitle = employeeField.FieldTitle,
                IsRequired = employeeField.IsRequired,
                GroupName = detailGroupName,
                RefCategory = new CategoryNameModel()
                {
                    //CategoryId = 0,
                    CategoryCode = refCategory.FirstOrDefault()?.CategoryCode,
                    CategoryTitle = refCategory.FirstOrDefault()?.CategoryTitle,
                    IsTreeView = false,
                    Fields = refCategory
                        .Select(f => new CategoryFieldNameModel()
                        {
                            //CategoryFieldId = f.id,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = f.GetTitleCategoryField(),
                            RefCategory = null,
                            IsRequired = false
                        }).ToList()
                }
            });

            foreach (var field in typeFullInfo.SalaryPeriodAdditionTypeField)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    //CategoryFieldId = field.HrAreaFieldId,
                    FieldName = field.SalaryPeriodAdditionField.FieldName,
                    FieldTitle = field.SalaryPeriodAdditionField.Title,
                    RefCategory = null,
                    IsRequired = false,
                    GroupName = detailGroupName
                };

                detailFields.Add(fileData);
            }


            detailFields.Add(new CategoryFieldNameModel()
            {
                //CategoryFieldId = field.HrAreaFieldId,
                FieldName = desField.FieldName,
                FieldTitle = desField.FieldTitle,
                IsRequired = desField.IsRequired,
                GroupName = detailGroupName,
                RefCategory = null
            });

            detailFields.Add(new CategoryFieldNameModel
            {
                FieldName = ImportStaticFieldConsants.CheckImportRowEmpty,
                FieldTitle = "Cột kiểm tra",
            });

            return detailFields;
        }

        protected IList<SalaryAdditionBilExcelRow> ReadExcelData(ImportExcelMapping mapping, ExcelReader reader)
        {
            var dataExcel = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var ignoreIfEmptyColumns = mapping.MappingFields.Where(f => f.IsIgnoredIfEmpty).Select(f => f.Column).ToList();


            return dataExcel.Rows
                .Select((r, i) => new SalaryAdditionBilExcelRow
                {
                    Data = r,
                    Index = i + mapping.FromRow
                })
                .Where(r => !ignoreIfEmptyColumns.Any(c => !r.Data.ContainsKey(c) || string.IsNullOrWhiteSpace(r.Data[c])))//not any empty ignore column
                .ToList();
        }

        public class SalaryAdditionBilExcelRow
        {
            public int Index { get; set; }
            public NonCamelCaseDictionary<string> Data { get; set; }
        }
    }
}
