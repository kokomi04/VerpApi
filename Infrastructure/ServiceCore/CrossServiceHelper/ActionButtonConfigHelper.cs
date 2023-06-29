using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IActionButtonConfigHelper
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs();


        Task<ActionButtonModel> AddActionButton(ActionButtonUpdateModel model);

        Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonUpdateModel model);

        Task<bool> DeleteActionButton(int actionButtonId);

        Task<int> AddActionButtonBillType(int actionButtonId, int billTypeObjectId);

        Task<bool> RemoveActionButtonBillType(int actionButtonId, int billTypeObjectId, string billTypeObjectTitle);

        Task<bool> RemoveAllByBillType(int billTypeObjectId, string billTypeObjectTitle);

        Task<IList<ActionButtonBillTypeMapping>> GetActionButtonBillType(int? billTypeObjectId);

    }

    public abstract class ActionButtonConfigHelperServiceAbstract : IActionButtonConfigHelper
    {
        private IActionButtonConfigHelperService _ActionButtonConfigHelperService;
        private EnumObjectType _billTypeObjectTypeId;
        private string _billTypeObjectTypeTitle;
        private IMapper _mapper;

        public ActionButtonConfigHelperServiceAbstract(IMapper mapper, IActionButtonConfigHelperService ActionButtonConfigHelperService, EnumObjectType billTypeObjectTypeId, string billTypeObjectTypeTitle)
        {
            _mapper = mapper;
            _ActionButtonConfigHelperService = ActionButtonConfigHelperService;
            _billTypeObjectTypeId = billTypeObjectTypeId;
            _billTypeObjectTypeTitle = billTypeObjectTypeTitle;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs()
        {
            return await _ActionButtonConfigHelperService.GetActionButtonConfigs(_billTypeObjectTypeId);
        }


        public async Task<ActionButtonModel> AddActionButton(ActionButtonUpdateModel model)
        {
            var data = _mapper.Map<ActionButtonModel>(model);
            data.BillTypeObjectTypeId = _billTypeObjectTypeId;
            return await _ActionButtonConfigHelperService.AddActionButton(data, _billTypeObjectTypeTitle);
        }

        public async Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonUpdateModel model)
        {
            var data = _mapper.Map<ActionButtonModel>(model);

            data.BillTypeObjectTypeId = _billTypeObjectTypeId;
            return await _ActionButtonConfigHelperService.UpdateActionButton(actionButtonId, data, _billTypeObjectTypeTitle);
        }

        public async Task<bool> DeleteActionButton(int actionButtonId)
        {
            return await _ActionButtonConfigHelperService.DeleteActionButton(actionButtonId, _billTypeObjectTypeId, _billTypeObjectTypeTitle);
        }

        public async Task<int> AddActionButtonBillType(int actionButtonId, int billTypeObjectId)
        {
            var title = await GetObjectTitle(billTypeObjectId);

            return await _ActionButtonConfigHelperService.AddActionButtonBillType(actionButtonId, _billTypeObjectTypeId, billTypeObjectId, title);
        }

        public async Task<bool> RemoveActionButtonBillType(int actionButtonId, int billTypeObjectId, string billTypeObjectTitle)
        {
            var title = billTypeObjectTitle;
            if (string.IsNullOrWhiteSpace(title))
                title = await GetObjectTitle(billTypeObjectId);

            return await _ActionButtonConfigHelperService.RemoveActionButtonBillType(actionButtonId, _billTypeObjectTypeId, billTypeObjectId, title);
        }


        public async Task<bool> RemoveAllByBillType(int billTypeObjectId, string billTypeObjectTitle)
        {
            var title = billTypeObjectTitle;
            if (string.IsNullOrWhiteSpace(title))
                title = await GetObjectTitle(billTypeObjectId);

            return await _ActionButtonConfigHelperService.RemoveAllByBillType(_billTypeObjectTypeId, billTypeObjectId, title);
        }

        public async Task<IList<ActionButtonBillTypeMapping>> GetActionButtonBillType(int? billTypeObjectId)
        {

            return await _ActionButtonConfigHelperService.GetActionButtonBillType(_billTypeObjectTypeId, billTypeObjectId);
        }

        protected abstract Task<string> GetObjectTitle(int objectId);

    }

    public interface IActionButtonConfigHelperService
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType billTypeObjectTypeId);
        Task<ActionButtonModel> AddActionButton(ActionButtonModel data, string billTypeObjectTypeTitle);
        Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data, string billTypeObjectTypeTitle);
        Task<bool> DeleteActionButton(int actionButtonId, EnumObjectType billTypeObjectTypeId, string billTypeObjectTypeTitle);
        Task<int> AddActionButtonBillType(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string billTypeObjectTitle);
        Task<bool> RemoveActionButtonBillType(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string billTypeObjectTitle);
        Task<bool> RemoveAllByBillType(EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string billTypeObjectTitle);
        Task<IList<ActionButtonBillTypeMapping>> GetActionButtonBillType(EnumObjectType billTypeObjectTypeId, int? billTypeObjectId);
    }

    public class ActionButtonConfigHelperService : IActionButtonConfigHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        public ActionButtonConfigHelperService(IHttpCrossService httpCrossService,
            IOptions<AppSetting> appSetting,
            ILogger<ProductHelperService> logger)
        {
            _httpCrossService = httpCrossService;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType billTypeObjectTypeId)
        {

            var queries = new
            {
                billTypeObjectTypeId = (int)billTypeObjectTypeId,
            };
            return await _httpCrossService.Get<IList<ActionButtonModel>>($"api/internal/InternalActionButtonConfig", queries);
        }


        public async Task<ActionButtonModel> AddActionButton(ActionButtonModel data, string typeTitle)
        {
            return await _httpCrossService.Post<ActionButtonModel>($"api/internal/InternalActionButtonConfig", data, new { typeTitle });
        }

        public async Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data, string typeTitle)
        {
            return await _httpCrossService.Put<ActionButtonModel>($"api/internal/InternalActionButtonConfig/{actionButtonId}", data, new { typeTitle });
        }


        public async Task<bool> DeleteActionButton(int actionButtonId, EnumObjectType billTypeObjectTypeId, string typeTitle)
        {
            var data = new
            {
                billTypeObjectTypeId = (int)billTypeObjectTypeId
            };
            return await _httpCrossService.Deleted<bool>($"api/internal/InternalActionButtonConfig/{actionButtonId}", data, new { typeTitle });
        }


        public async Task<int> AddActionButtonBillType(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string objectTitle)
        {
            var data = new ActionButtonBillTypeMapping
            {
                ActionButtonId = actionButtonId,
                BillTypeObjectTypeId = billTypeObjectTypeId,
                BillTypeObjectId = billTypeObjectId
            };
            return await _httpCrossService.Post<int>($"api/internal/InternalActionButtonConfig/ActionButtonBillType", data, new { objectTitle });
        }


        public async Task<bool> RemoveActionButtonBillType(int actionButtonId, EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string objectTitle)
        {
            var data = new ActionButtonBillTypeMapping
            {
                ActionButtonId = actionButtonId,
                BillTypeObjectTypeId = billTypeObjectTypeId,
                BillTypeObjectId = billTypeObjectId
            };
            return await _httpCrossService.Deleted<bool>($"api/internal/InternalActionButtonConfig/ActionButtonBillType", data, new { objectTitle });
        }

        public async Task<bool> RemoveAllByBillType(EnumObjectType billTypeObjectTypeId, int billTypeObjectId, string objectTitle)
        {
            var data = new ActionButtonBillTypeMapping
            {
                //ActionButtonId = actionButtonId,
                BillTypeObjectTypeId = billTypeObjectTypeId,
                BillTypeObjectId = billTypeObjectId
            };
            return await _httpCrossService.Deleted<bool>($"api/internal/InternalActionButtonConfig/ActionButtonBillType/RemoveAllByBillType", data, new { objectTitle });
        }

        public async Task<IList<ActionButtonBillTypeMapping>> GetActionButtonBillType(EnumObjectType billTypeObjectTypeId, int? billTypeObjectId)
        {
            var queries = new
            {
                //ActionButtonId = actionButtonId,
                BillTypeObjectTypeId = billTypeObjectTypeId,
                BillTypeObjectId = billTypeObjectId
            };

            return await _httpCrossService.Get<IList<ActionButtonBillTypeMapping>>($"api/internal/InternalActionButtonConfig/ActionButtonBillType", queries);
        }
    }
}
