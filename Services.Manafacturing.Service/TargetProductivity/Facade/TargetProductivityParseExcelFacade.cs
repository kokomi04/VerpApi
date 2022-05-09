
using System;
using System.Linq;
using System.Collections.Generic;
using VErp.Commons.Library;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using System.IO;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Verp.Resources.PurchaseOrder.PurchasingRequest;
using VErp.Commons.Library.Model;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service.Step;
using Verp.Resources.Manafacturing.TargetProductivity;
using System.Threading.Tasks;

namespace VErp.Services.Manafacturing.Service.Facade
{
    public class TargetProductivityParseExcelFacade
    {
        private readonly IStepService _stepService;
        public TargetProductivityParseExcelFacade(IStepService stepService)
        {
            _stepService = stepService;

        }
        public async Task<IList<TargetProductivityDetailModel>> ParseInvoiceDetails(ImportExcelMapping mapping, Stream stream)
        {
            var steps = await _stepService.GetListStep(null, 1, 0);

            var stepNames = steps.List.GroupBy(s => s.StepName.NormalizeAsInternalName()).ToDictionary(s => s.Key, s => s.ToList());

            var reader = new ExcelReader(stream);

            return reader.ReadSheetEntity<TargetProductivityDetailModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                if (propertyName == nameof(TargetProductivityDetailModel.ProductionStepId))
                {
                    var normalizeValue = value.NormalizeAsInternalName();
                    if (stepNames.ContainsKey(normalizeValue))
                    {
                        entity.ProductionStepId = stepNames[normalizeValue].OrderByDescending(s => s.StepName == value).First().StepId;
                    }
                    else
                    {
                        throw TargetProductivityValidation.StepNotFound.BadRequestFormat(value);
                    }
                }

                return false;
            });
        }


    }
}
