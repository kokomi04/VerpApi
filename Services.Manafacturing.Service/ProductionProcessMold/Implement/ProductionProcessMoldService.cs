using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionProcessMold;
using ProductionProcessMoldEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionProcessMold;
using Verp.Resources.Manafacturing.Production.Process;

namespace VErp.Services.Manafacturing.Service.ProductionProcessMold.Implement
{
    public class ProductionProcessMoldService : IProductionProcessMoldService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionProcessMoldService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionProcessMoldService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(Commons.Enums.MasterEnum.EnumObjectType.ProductionProcessMold);
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PageData<ProductionProcessMoldOutput>> Search(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _manufacturingDBContext.ProductionProcessMold.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Title.Contains(keyword));

            if (filters != null)
                query = query.InternalFilter(filters);

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
                query = query.InternalOrderBy(orderByFieldName, asc);

            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ProjectTo<ProductionProcessMoldOutput>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<ICollection<ProductionStepMoldModel>> GetProductionProcessMold(long productionProcessMoldId)
        {
            var process = await _manufacturingDBContext.ProductionProcessMold.FirstOrDefaultAsync(x => x.ProductionProcessMoldId == productionProcessMoldId);
            if (process == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy quy trình mẫu");

            return await _manufacturingDBContext.ProductionStepMold.AsNoTracking()
                .Where(x => x.ProductionProcessMoldId == productionProcessMoldId)
                .ProjectTo<ProductionStepMoldModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public async Task<long> AddProductionProcessMold(ProductionProcessMoldInput model)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                ValidProductionProcessMold(model);

                var process = _mapper.Map<ProductionProcessMoldEntity>(model);

                _manufacturingDBContext.ProductionProcessMold.Add(process);
                await _manufacturingDBContext.SaveChangesAsync();

                var productionSteps = model.ProductionStepMold.ToList();
                productionSteps.ForEach(x => x.ProductionProcessMoldId = process.ProductionProcessMoldId);

                var nProductionSteps = _mapper.Map<IEnumerable<ProductionStepMold>>(productionSteps);
                await _manufacturingDBContext.ProductionStepMold.AddRangeAsync(nProductionSteps);

                await _manufacturingDBContext.SaveChangesAsync();

                // step link
                var mLinks = productionSteps.SelectMany(x => x.ProductionStepMoldLink).ToArray();
                var nLinks = (from l in mLinks
                              join fs in nProductionSteps on l.StepFromId equals fs.StepId
                              join ts in nProductionSteps on l.StepToId equals ts.StepId
                              select new ProductionStepMoldLink
                              {
                                  FromProductionStepMoldId = fs.ProductionStepMoldId,
                                  ToProductionStepMoldId = ts.ProductionStepMoldId
                              }).Distinct();

                await _manufacturingDBContext.ProductionStepMoldLink.AddRangeAsync(nLinks);
                await _manufacturingDBContext.SaveChangesAsync();

                await _objActivityLogFacade.LogBuilder(() => ProductionProcessActivityLogMessage.CreateProcessMold)
                   .MessageResourceFormatDatas(process.Title)
                   .ObjectId(process.ProductionProcessMoldId)
                   .JsonData(model)
                   .CreateLog();

                await trans.CommitAsync();
                return process.ProductionProcessMoldId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("AddProductionProcessMold", ex);

                throw;
            }
        }

        public async Task<bool> UpdateProductionProcessMold(long productionProcessMoldId, ProductionProcessMoldInput model)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var process = await _manufacturingDBContext.ProductionProcessMold.FirstOrDefaultAsync(x => x.ProductionProcessMoldId == productionProcessMoldId);
                if (process == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy quy trình mẫu");

                ValidProductionProcessMold(model);

                var productionSteps = await _manufacturingDBContext.ProductionStepMold
                   .Where(x => x.ProductionProcessMoldId == productionProcessMoldId)
                   .ToListAsync();

                var linkOlds = await _manufacturingDBContext.ProductionStepMoldLink
                    .Where(l => productionSteps.Select(x => x.ProductionStepMoldId).Contains(l.FromProductionStepMoldId))
                    .ToListAsync();

                _mapper.Map(model, process); // Cập nhật title

                foreach (var step in productionSteps)
                {
                    var mStep = model.ProductionStepMold.FirstOrDefault(x => x.ProductionStepMoldId == step.ProductionStepMoldId);
                    if (mStep != null)
                        _mapper.Map(mStep, step);
                    else step.IsDeleted = true;
                }

                var nProductionSteps = _mapper.Map<List<ProductionStepMold>>(model.ProductionStepMold.Where(x => x.ProductionStepMoldId <= 0).ToArray());
                nProductionSteps.ForEach(x => x.ProductionProcessMoldId = productionProcessMoldId);

                await _manufacturingDBContext.ProductionStepMold.AddRangeAsync(nProductionSteps);

                await _manufacturingDBContext.SaveChangesAsync();

                // step link
                productionSteps.AddRange(nProductionSteps);
                var mLinks = model.ProductionStepMold.SelectMany(x => x.ProductionStepMoldLink).ToArray();
                var nLinks = (from l in mLinks
                              join fs in productionSteps on l.StepFromId equals fs.StepId
                              join ts in productionSteps on l.StepToId equals ts.StepId
                              select new ProductionStepMoldLink
                              {
                                  FromProductionStepMoldId = fs.ProductionStepMoldId,
                                  ToProductionStepMoldId = ts.ProductionStepMoldId
                              }).Distinct();

                _manufacturingDBContext.ProductionStepMoldLink.RemoveRange(linkOlds.Except(nLinks, new ProductionStepMoldLinkComparer()));

                await _manufacturingDBContext.ProductionStepMoldLink.AddRangeAsync(nLinks.Except(linkOlds, new ProductionStepMoldLinkComparer()));

                await _manufacturingDBContext.SaveChangesAsync();

                await _objActivityLogFacade.LogBuilder(() => ProductionProcessActivityLogMessage.UpdateProcessMold)
                   .MessageResourceFormatDatas(model.Title)
                   .ObjectId(productionProcessMoldId)
                   .JsonData(model)
                   .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("UpdateProductionProcessMold", ex);

                throw;
            }
        }

        public async Task<bool> DeleteProductionProcessMold(long productionProcessMoldId)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var process = await _manufacturingDBContext.ProductionProcessMold.FirstOrDefaultAsync(x => x.ProductionProcessMoldId == productionProcessMoldId);
                if (process == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy quy trình mẫu");

                var productionSteps = await _manufacturingDBContext.ProductionStepMold.Where(x => x.ProductionProcessMoldId == productionProcessMoldId)
                    .ToListAsync();

                process.IsDeleted = true;

                productionSteps.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await _objActivityLogFacade.LogBuilder(() => ProductionProcessActivityLogMessage.DeleteProcessMold)
                   .MessageResourceFormatDatas(process.Title)
                   .ObjectId(productionProcessMoldId)
                   .JsonData(process)
                   .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError("DeleteProductionProcessMold", ex);

                throw;
            }
        }

        private void ValidProductionProcessMold(ProductionProcessMoldInput model)
        {
            if (model.ProductionStepMold.GroupBy(x => x.StepId).Where(x => x.Count() > 1).Count() > 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện 2 công đoạn giống nhau trong quy trình mẫu");

            if (model.ProductionStepMold.Where(x => x.IsFinish == true).Count() != 1
                || model.ProductionStepMold.Where(x => !(x.ProductionStepMoldLink != null && x.ProductionStepMoldLink.Count() > 0) && x.IsFinish == true).Count() != 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Chưa thiết lập hoặc xuất hiện nhiều hơn 1 công đoạn cuối cùng trong quy trình mẫu");

        }
    }
}
