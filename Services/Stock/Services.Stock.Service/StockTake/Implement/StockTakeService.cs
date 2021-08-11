using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.StockTake;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace VErp.Services.Stock.Service.StockTake.Implement
{
    public class StockTakeService : IStockTakeService
    {
        private readonly StockDBContext _stockContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IRoleHelperService _roleHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IMapper _mapper;
        public StockTakeService(
            StockDBSubsidiaryContext stockContext
            , IActivityLogService activityLogService
            , IRoleHelperService roleHelperService
            , ICurrentContextService currentContextService
            , IMapper mapper
            )
        {
            _stockContext = stockContext;
            _activityLogService = activityLogService;
            _roleHelperService = roleHelperService;
            _currentContextService = currentContextService;
            _mapper = mapper;
        }

        public async Task<PageData<StockTakePeriotListModel>> GetStockTakePeriods(string keyword, int page, int size)
        {
            keyword = keyword.Trim();
            var stockTakePeriods = _stockContext.StockTakePeriod.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                stockTakePeriods = stockTakePeriods.Where(stp => stp.StockTakePeriodCode.Contains(keyword) || stp.Content.Contains(keyword));
            }
            var total = await stockTakePeriods.CountAsync();
            var paged = (size > 0 ? stockTakePeriods.Skip((page - 1) * size).Take(size) : stockTakePeriods).ProjectTo<StockTakePeriotListModel>(_mapper.ConfigurationProvider).ToList();
            return (paged, total);
        }
    }
}
