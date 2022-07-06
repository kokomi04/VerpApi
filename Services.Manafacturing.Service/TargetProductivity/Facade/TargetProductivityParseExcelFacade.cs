using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.TargetProductivity;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Services.Manafacturing.Model;
using VErp.Services.Manafacturing.Service.Step;

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
            var productivityTimeType = EnumExtensions.GetEnumMembers<EnumProductivityTimeType>();
            var productivityResourceType = EnumExtensions.GetEnumMembers<EnumProductivityResourceType>();
            var workloadType = EnumExtensions.GetEnumMembers<EnumWorkloadType>();
            return reader.ReadSheetEntity<TargetProductivityDetailModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(TargetProductivityDetailModel.ProductionStepId):
                        var normalizeValue = value.NormalizeAsInternalName();
                        if (stepNames.ContainsKey(normalizeValue))
                        {
                            entity.ProductionStepId = stepNames[normalizeValue].OrderByDescending(s => s.StepName == value).First().StepId;
                            return true;
                        }
                        else
                        {
                            throw TargetProductivityValidation.StepNotFound.BadRequestFormat(value);
                        }
                    case nameof(TargetProductivityDetailModel.ProductivityTimeTypeId):
                        var productivityTimeTypeId = productivityTimeType.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (productivityTimeTypeId != null) entity.ProductivityTimeTypeId = productivityTimeTypeId.Enum;
                        return true;
                    case nameof(TargetProductivityDetailModel.ProductivityResourceTypeId):
                        var productivityResourceTypeId = productivityResourceType.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (productivityResourceTypeId != null) entity.ProductivityResourceTypeId = productivityResourceTypeId.Enum;
                        return true;
                    case nameof(TargetProductivityDetailModel.WorkLoadTypeId):
                        var workloadTypeId = workloadType.FirstOrDefault(r => r.Description.NormalizeAsInternalName() == value.NormalizeAsInternalName());
                        if (workloadTypeId != null) entity.WorkLoadTypeId = workloadTypeId.Enum;
                        return true;
                }

                return false;
            });
        }


    }
}
