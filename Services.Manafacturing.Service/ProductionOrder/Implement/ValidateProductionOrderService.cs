using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ValidateProductionOrderService: IValidateProductionOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ValidateProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<string>> ValidateProductionOrder(long productionOrderId)
        {
            var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderId = @ProductionOrderId";
            var parammeters = new SqlParameter[]
            {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
            };
            var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters);

            var productionOrderDetail =  resultData.ConvertData<ProductionOrderDetailOutputModel>();

            return await GetWarningProductionOrder(productionOrderId, productionOrderDetail);
        }

        public  async Task<IList<string>> GetWarningProductionOrder(long productionOrderId, IList<ProductionOrderDetailOutputModel> productionOrderDetail)
        {
            var lsWarning = new List<string>();

            var stepFinal = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => x.IsFinish && x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (stepFinal != null)
            {
                var linkData = stepFinal.ProductionStepLinkDatas.Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input);

                foreach (var detail in productionOrderDetail)
                {
                    var totalQuantity = detail.Quantity + detail.ReserveQuantity;

                    var ld = linkData.FirstOrDefault(x => x.ObjectId == detail.ProductId);
                    if (ld == null)
                    {
                        lsWarning.Add($"Sản phẩm \"{detail.ProductTitle}\" chưa được thiết lập trong QTSX");
                    }
                    else if (totalQuantity != ld.QuantityOrigin)
                    {
                        lsWarning.Add($"Số lượng của sản phẩm \"{detail.ProductTitle}\" không khớp với QTSX ({totalQuantity}/{ld.QuantityOrigin})");
                    }
                }

                var ldNotProcess = linkData.Where(x => productionOrderDetail.Select(p => (long)p.ProductId).Contains(x.ObjectId)).Select(x => x.ObjectId);
                if (ldNotProcess.Count() > 0)
                    lsWarning.Add($"Số lượng sản phẩm đầu ra của QTSX lệch với LSX");
            }
            else
            {
                lsWarning.Add($"Chưa thiết lập QTSX");
            }

            return lsWarning;
        }
    }
}
