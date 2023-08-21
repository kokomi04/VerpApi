using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OpenXmlPowerTools;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IOvertimeLevelService
    {
        Task<int> AddOvertimeLevel(OvertimeLevelModel model);
        Task<bool> DeleteOvertimeLevel(long countedSymbolId);
        Task<IList<OvertimeLevelModel>> GetListOvertimeLevel();
        Task<OvertimeLevelModel> GetOvertimeLevel(long countedSymbolId);
        Task<bool> UpdateOvertimeLevel(int countedSymbolId, OvertimeLevelModel model);
        Task<bool> UpdateOvertimeLevelSortOrder(IList<OvertimeLevelModel> model);
    }

    public class OvertimeLevelService : IOvertimeLevelService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public OvertimeLevelService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<int> AddOvertimeLevel(OvertimeLevelModel model)
        {
            if (await _organizationDBContext.OvertimeLevel.AnyAsync(a => a.OvertimeCode == model.OvertimeCode))
                throw new BadRequestException(GeneralCode.InvalidParams, "Ký hiệu mức tăng ca đã tồn tại");

            var maxSortOrder = await _organizationDBContext.OvertimeLevel.MaxAsync(m => m.SortOrder);

            await UpdateSortOrder(maxSortOrder + 1, model.SortOrder);

            var entity = _mapper.Map<OvertimeLevel>(model);

            await _organizationDBContext.OvertimeLevel.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.OvertimeLevelId;
        }

        public async Task<bool> UpdateOvertimeLevel(int countedSymbolId, OvertimeLevelModel model)
        {
            var countedSymbol = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            if (countedSymbol.OvertimeCode != model.OvertimeCode && await _organizationDBContext.OvertimeLevel.AnyAsync(a => a.OvertimeCode == model.OvertimeCode))
                throw new BadRequestException(GeneralCode.InvalidParams, "Ký hiệu mức tăng ca đã tồn tại");

            await UpdateSortOrder(countedSymbol.SortOrder, model.SortOrder);

            model.OvertimeLevelId = countedSymbolId;
            _mapper.Map(model, countedSymbol);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteOvertimeLevel(long countedSymbolId)
        {
            var countedSymbol = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            await UpdateSortOrder(countedSymbol.SortOrder, null);

            countedSymbol.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<OvertimeLevelModel> GetOvertimeLevel(long countedSymbolId)
        {
            var countedSymbol = await _organizationDBContext.OvertimeLevel.FirstOrDefaultAsync(x => x.OvertimeLevelId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<OvertimeLevelModel>(countedSymbol);
        }

        public async Task<IList<OvertimeLevelModel>> GetListOvertimeLevel()
        {
            return await _organizationDBContext.OvertimeLevel
                .OrderBy(o => o.SortOrder)
                .ProjectTo<OvertimeLevelModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync();
        }

        public async Task<bool> UpdateOvertimeLevelSortOrder(IList<OvertimeLevelModel> model)
        {
            var overtimeLevelIds = model.Select(model => model.OvertimeLevelId).ToList();

            var entities = await _organizationDBContext.OvertimeLevel.Where(x => overtimeLevelIds.Contains(x.OvertimeLevelId)).ToListAsync();
            if(!entities.Any())
                throw new BadRequestException(GeneralCode.ItemNotFound);

            foreach (var ov in model)
            {
                entities.ForEach(e =>
                {
                    if(e.OvertimeLevelId == ov.OvertimeLevelId)
                        e.SortOrder = ov.SortOrder;
                });
            }

            await _organizationDBContext.SaveChangesAsync();
            return true;
        }

        private async Task UpdateSortOrder(int entitySortOrder, int? modelSortOrder)
        {
            if (entitySortOrder > modelSortOrder)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder >= modelSortOrder && x.SortOrder < entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder++);
            }

            if (entitySortOrder < modelSortOrder)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder <= modelSortOrder && x.SortOrder > entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder--);
            }

            if (!modelSortOrder.HasValue)
            {
                var behindOvertimeLevels = await _organizationDBContext.OvertimeLevel.Where(x => x.SortOrder > entitySortOrder).ToListAsync();
                if (behindOvertimeLevels.Any())
                    behindOvertimeLevels.ForEach(x => x.SortOrder--);
            }
        }
    }
}