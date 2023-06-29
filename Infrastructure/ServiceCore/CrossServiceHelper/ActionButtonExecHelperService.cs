using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IActionButtonExecHelper
    {
        Task<IList<ActionButtonModel>> GetActionButtons(int billTypeObjectId);

        Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, int billTypeObjectId);

        Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data);

    }

    public abstract class ActionButtonExecHelperServiceAbstract : IActionButtonExecHelper
    {
        private IActionButtonExecHelperService _actionButtonExecHelperService;
        private EnumObjectType _billTypeObjectTypeId;
        public ActionButtonExecHelperServiceAbstract(IActionButtonExecHelperService actionButtonExecHelperService, EnumObjectType objectTypeId)
        {
            _actionButtonExecHelperService = actionButtonExecHelperService;
            _billTypeObjectTypeId = objectTypeId;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtons(int billTypeObjectId)
        {
            return await _actionButtonExecHelperService.GetActionButtons(_billTypeObjectTypeId, billTypeObjectId);
        }

        public async Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, int billTypeObjectId)
        {
            return await _actionButtonExecHelperService.ActionButtonInfo(actionButtonId, _billTypeObjectTypeId, billTypeObjectId);
        }

        public abstract Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data);
    }

    public interface IActionButtonExecHelperService
    {
        Task<IList<ActionButtonModel>> GetActionButtons(EnumObjectType billTypeObjectTypeId, int billTypeObjectId);
        Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId);
    }

    public class ActionButtonExecHelperService : IActionButtonExecHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public ActionButtonExecHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }


        public async Task<IList<ActionButtonModel>> GetActionButtons(EnumObjectType billTypeObjectTypeId, int billTypeObjectId)
        {
            var queries = new
            {
                billTypeObjectTypeId = (int)billTypeObjectTypeId,
                billTypeObjectId
            };
            return await _httpCrossService.Get<IList<ActionButtonModel>>($"api/internal/InternalActionButtonExec", queries);
        }


        public async Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId)
        {
            var queries = new
            {
                billTypeObjectTypeId = (int)billTypeObjectTypeId,
                billTypeObjectId
            };
            return await _httpCrossService.Get<ActionButtonModel>($"api/internal/InternalActionButtonExec/{actionButtonId}", queries);
        }


    }
}
