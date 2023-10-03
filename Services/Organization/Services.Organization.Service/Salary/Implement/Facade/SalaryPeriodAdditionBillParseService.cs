using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using Verp.Resources.Organization;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.Facade;
using Microsoft.EntityFrameworkCore;
using VErp.Infrastructure.ServiceCore.Extensions;
using VErp.Commons.Constants;
using VErp.Services.Organization.Model.Salary;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Verp.Resources.Organization.Salary;
using DocumentFormat.OpenXml.InkML;
using Verp.Resources.Organization.Salary.Validation;
using System.Reflection;
using DocumentFormat.OpenXml.Drawing;
using OpenXmlPowerTools;
using NPOI.SS.Formula.Functions;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using VErp.Commons.Library.Utilities;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;

namespace VErp.Services.Organization.Service.Salary.Implement.Facade
{

    public class SalaryPeriodAdditionBillParseService : SalaryPeriodAdditionBillFieldAbstract, ISalaryPeriodAdditionBillParseService
    {

        private readonly OrganizationDBContext _organizationDBContext;

        private readonly ISalaryPeriodAdditionTypeService _salaryPeriodAdditionTypeService;

        public SalaryPeriodAdditionBillParseService(OrganizationDBContext organizationDBContext, ICategoryHelperService categoryHelperService, ISalaryPeriodAdditionTypeService salaryPeriodAdditionTypeService)
            : base(categoryHelperService)
        {
            _organizationDBContext = organizationDBContext;
            _salaryPeriodAdditionTypeService = salaryPeriodAdditionTypeService;
        }

        public async Task<CategoryNameModel> GetFieldDataMappingForParse(int salaryPeriodAdditionTypeId)
        {
            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            var result = new CategoryNameModel()
            {
                //CategoryId = inputTypeInfo.HrTypeId,
                CategoryCode = typeInfo.SalaryPeriodAdditionTypeId + "",
                CategoryTitle = typeInfo.Title,
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            result.Fields = new List<CategoryFieldNameModel>();

            foreach (var d in await GetFieldDetailsForMapping(typeInfo))
            {
                result.Fields.Add(d);
            }

            return result;
        }

        public async Task<IList<SalaryPeriodAdditionBillEmployeeParseInfo>> ParseExcel(int salaryPeriodAdditionTypeId, ImportExcelMapping mapping, Stream stream)
        {

            var typeInfo = await _salaryPeriodAdditionTypeService.GetFullEntityInfo(salaryPeriodAdditionTypeId);

            if (typeInfo == null)
            {
                throw GeneralCode.ItemNotFound.BadRequest();
            }

            if (!typeInfo.IsActived)
            {
                throw SalaryPeriodAdditionTypeValidationMessage.TypeInActived.BadRequestFormat(typeInfo.Title);
            }

            var parseFacade = new SalaryPeriodAdditionBillParseFacadeContext(mapping, categoryHelperService, typeInfo);


            var reader = new ExcelReader(stream);

            var excelRows = ReadExcelData(mapping, reader);


            await parseFacade.LoadEmployees(_organizationDBContext, excelRows);

            int count = excelRows.Count();

            var result = new List<SalaryPeriodAdditionBillEmployeeParseInfo>();
            var details = new List<SalaryPeriodAdditionBillEmployeeModel>();
            for (int rowIndex = 0; rowIndex < count; rowIndex++)
            {
                var row = excelRows.ElementAt(rowIndex);
                var detailInfo = parseFacade.MapAndLoadRowToModel(row, "", details);
                result.Add(detailInfo);
            }

            return result;

        }


    }
}
