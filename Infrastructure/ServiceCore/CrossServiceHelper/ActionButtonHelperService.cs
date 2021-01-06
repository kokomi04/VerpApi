using GrpcProto.Protos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IActionButtonHelper
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs(int objectId);
        Task<IList<ActionButtonSimpleModel>> GetActionButtons(int objectId);

        Task<ActionButtonModel> AddActionButton(int objectId, ActionButtonModel data);

        Task<ActionButtonModel> UpdateActionButton(int objectId, int actionButtonId, ActionButtonModel data);

        Task<bool> DeleteActionButton(int objectId, int inputActionId);

        Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int inputActionId, long billId, BillInfoModel data);
    }

    public abstract class ActionButtonHelperServiceAbstract : IActionButtonHelper
    {
        private IActionButtonHelperService _actionButtonHelperService;
        private EnumObjectType _objectTypeId;
        public ActionButtonHelperServiceAbstract(IActionButtonHelperService actionButtonHelperService, EnumObjectType objectTypeId)
        {
            _actionButtonHelperService = actionButtonHelperService;
            _objectTypeId = objectTypeId;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs(int objectId)
        {
            return await _actionButtonHelperService.GetActionButtonConfigs(_objectTypeId, objectId);
        }
        public async Task<IList<ActionButtonSimpleModel>> GetActionButtons(int objectId)
        {
            return await _actionButtonHelperService.GetActionButtons(_objectTypeId, objectId);
        }

        public async Task<ActionButtonModel> AddActionButton(int objectId, ActionButtonModel data)
        {
            var title = await GetObjectTitle(objectId);

            data.ObjectTypeId = _objectTypeId;
            data.ObjectId = objectId;
            data.ObjectTitle = title;
            return await _actionButtonHelperService.AddActionButton(data);
        }

        public async Task<ActionButtonModel> UpdateActionButton(int objectId, int actionButtonId, ActionButtonModel data)
        {
            var title = await GetObjectTitle(objectId);

            data.ObjectTypeId = _objectTypeId;
            data.ObjectId = objectId;
            data.ObjectTitle = title;
            return await _actionButtonHelperService.UpdateActionButton(actionButtonId, data);
        }

        public async Task<bool> DeleteActionButton(int objectId, int inputActionId)
        {
            var title = await GetObjectTitle(objectId);

            return await _actionButtonHelperService.DeleteActionButton(inputActionId, _objectTypeId, objectId, title);
        }


        protected abstract Task<string> GetObjectTitle(int objectId);

        public abstract Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int inputActionId, long billId, BillInfoModel data);
    }

    public interface IActionButtonHelperService
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType objectTypeId, int? objectId);
        Task<IList<ActionButtonSimpleModel>> GetActionButtons(EnumObjectType objectTypeId, int objectId);
        Task<ActionButtonModel> AddActionButton(ActionButtonModel data);
        Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data);
        Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType objectTypeId, int objectId);
        Task<bool> DeleteActionButtonsByType(EnumObjectType objectTypeId, int objectId, string objectTitle);
        Task<bool> DeleteActionButton(int actionButtonId, EnumObjectType objectTypeId, int objectId, string objectTitle);
    }

    public class ActionButtonHelperService : IActionButtonHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public ActionButtonHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType objectTypeId, int? objectId)
        {

            var queries = new
            {
                objectTypeId = (int)objectTypeId,
                objectId
            };
            return await _httpCrossService.Get<IList<ActionButtonModel>>($"api/internal/InternalActionButton/Configs", queries);
        }

        public async Task<IList<ActionButtonSimpleModel>> GetActionButtons(EnumObjectType objectTypeId, int objectId)
        {
            var queries = new
            {
                objectTypeId = (int)objectTypeId,
                objectId
            };
            return await _httpCrossService.Get<IList<ActionButtonSimpleModel>>($"api/internal/InternalActionButton/ByTypes", queries);
        }

        public async Task<ActionButtonModel> AddActionButton(ActionButtonModel data)
        {
            return await _httpCrossService.Post<ActionButtonModel>($"api/internal/InternalActionButton", data);
        }

        public async Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data)
        {
            return await _httpCrossService.Put<ActionButtonModel>($"api/internal/InternalActionButton/{actionButtonId}", data);
        }

        public async Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType objectTypeId, int objectId)
        {
            var queries = new
            {
                objectTypeId = (int)objectTypeId,
                objectId
            };
            return await _httpCrossService.Get<ActionButtonModel>($"api/internal/InternalActionButton/{actionButtonId}", queries);
        }

        public async Task<bool> DeleteActionButton(int actionButtonId, EnumObjectType objectTypeId, int objectId, string objectTitle)
        {
            var data = new
            {
                objectTypeId = (int)objectTypeId,
                objectId,
                objectTitle
            };
            return await _httpCrossService.Detete<bool>($"api/internal/InternalActionButton/{actionButtonId}", data);
        }

        public async Task<bool> DeleteActionButtonsByType(EnumObjectType objectTypeId, int objectId, string objectTitle)
        {
            var data = new
            {
                objectTypeId = (int)objectTypeId,
                objectId,
                objectTitle
            };
            return await _httpCrossService.Detete<bool>($"api/internal/InternalActionButton/DeleteByType", data);
        }
    }
}
