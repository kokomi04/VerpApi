using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
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
            if (_organizationDBContext.OvertimeLevel.Any(x => x.OrdinalNumber == model.OrdinalNumber))
                throw new BadRequestException(GeneralCode.InvalidParams, $"Đã có mức tăng ca {model.OrdinalNumber} trong hệ thống");

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

            if (countedSymbol.OrdinalNumber != model.OrdinalNumber && _organizationDBContext.OvertimeLevel.Any(x => x.OrdinalNumber == model.OrdinalNumber))
                throw new BadRequestException(GeneralCode.InvalidParams, $"Đã có mức tăng ca {model.OrdinalNumber} trong hệ thống");

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
            var query = _organizationDBContext.OvertimeLevel.AsNoTracking();

            return await query.Select(x => new OvertimeLevelModel
            {
                OvertimeLevelId = x.OvertimeLevelId,
                OrdinalNumber = x.OrdinalNumber,
                OvertimeRate = x.OvertimeRate
            }).ToListAsync();
        }
    }
}