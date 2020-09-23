using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.Config;

namespace VErp.Services.Stock.Service.Stock.Implement
{
    public class InventoryConfigService : IInventoryConfigService
    {
        private readonly StockDBContext _stockDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public InventoryConfigService(StockDBContext stockDBContext, ICurrentContextService currentContextService, IMapper mapper, IActivityLogService activityLogService)
        {
            _stockDBContext = stockDBContext;
            _currentContextService = currentContextService;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<bool> UpdateConfig(InventoryConfigModel req)
        {
            var info = await _stockDBContext.InventoryConfig.FirstOrDefaultAsync();
            if (info == null)
            {
                info = _mapper.Map<InventoryConfig>(req);
                info.SubsidiaryId = _currentContextService.SubsidiaryId;
                await _stockDBContext.InventoryConfig.AddAsync(info);
            }
            else
            {
                _mapper.Map(req, info);
            }

            await _stockDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(Commons.Enums.MasterEnum.EnumObjectType.InventoryConfig, info.SubsidiaryId, $"Cập nhật thiết lập xuất/nhập kho", req.JsonSerialize());

            return true;
        }
    }
}
