using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceStepRequestService : IOutsourceStepRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public OutsourceStepRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }
        public async Task<OutsourceStepRequestInfo> GetOutsourceStepRequest(long outsourceStepRequestId)
        {
            var requestOutsourceStep = await _manufacturingDBContext.OutsourceStepRequest
                .Include(o => o.OutsourceStepRequestData)
                .ThenInclude(d => d.ProductionStepLinkData)
                .ThenInclude(l => l.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStep)
                .FirstOrDefaultAsync(r => r.OutsourceStepRequestId == outsourceStepRequestId);


            throw new NotImplementedException();
        }

        public async Task<long> CreateOutsourceStepRequest(OutsourceStepRequestInfo req)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                // Get cấu hình sinh mã
                int customGenCodeId = 0;
                var currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceRequest, EnumObjectType.OutsourceRequest, 0);

                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                if (generated == null)
                {
                    throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                }
                customGenCodeId = currentConfig.CustomGenCodeId;

                // Create outsourceStepRequest
                var outsourceStepRequest = _mapper.Map<OutsourceStepRequest>(req);
                outsourceStepRequest.OutsourceStepRequestCode = generated.CustomCode;

                _manufacturingDBContext.OutsourceStepRequest.Add(outsourceStepRequest);
                await _manufacturingDBContext.SaveChangesAsync();

                // Create outsourceStepRequestData
                var outsourceStepRequestDatas = new List<OutsourceStepRequestData>();
                foreach (var data in req.OutsourceStepRequestData)
                {
                    data.OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId;
                    outsourceStepRequestDatas.Add(_mapper.Map<OutsourceStepRequestData>(data));
                }

                await _manufacturingDBContext.OutsourceStepRequestData.AddRangeAsync(outsourceStepRequestDatas);
                await _manufacturingDBContext.SaveChangesAsync();
                if (customGenCodeId > 0)
                    await _customGenCodeHelperService.ConfirmCode(customGenCodeId);

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, outsourceStepRequest.OutsourceStepRequestId,
                    $"Thêm mới yêu cầu gia công công đoạn", req.JsonSerialize());

                return outsourceStepRequest.OutsourceStepRequestId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateRequestOutsourcePart");
                throw;
            }
        }

        public async Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInfo req)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(req, outsourceStepRequest);

                var outsourceStepRequestDataOld = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();
                var outsourceStepRequestDataNew = _mapper.Map<IList<OutsourceStepRequestData>>(req.OutsourceStepRequestData);

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(outsourceStepRequestDataOld);
                await _manufacturingDBContext.OutsourceStepRequestData.AddRangeAsync(outsourceStepRequestDataNew);

                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceStepRequest");
                throw;
            }
        }

        public async Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                outsourceStepRequest.IsDeleted = true;
                var outsourceStepRequestDataOld = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(outsourceStepRequestDataOld);
                await _manufacturingDBContext.SaveChangesAsync();
                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeleteOutsourceStepRequest");
                throw;
            }
        }
    }
}
