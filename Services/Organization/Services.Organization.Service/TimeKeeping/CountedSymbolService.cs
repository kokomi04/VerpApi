using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.OrganizationDB;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface ICountedSymbolService
    {
        Task<int> AddCountedSymbol(CountedSymbolModel model);
        Task<bool> DeleteCountedSymbol(long countedSymbolId);
        Task<CountedSymbolModel> GetCountedSymbol(long countedSymbolId);
        Task<IList<CountedSymbolModel>> GetListCountedSymbol();
        Task<bool> UpdateCountedSymbol(int countedSymbolId, CountedSymbolModel model);
    }

    public class CountedSymbolService : ICountedSymbolService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public CountedSymbolService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<int> AddCountedSymbol(CountedSymbolModel model)
        {
            var entity = _mapper.Map<CountedSymbol>(model);

            await _organizationDBContext.CountedSymbol.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.CountedSymbolId;
        }

        public async Task<bool> UpdateCountedSymbol(int countedSymbolId, CountedSymbolModel model)
        {
            var countedSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.CountedSymbolId = countedSymbolId;
            _mapper.Map(model, countedSymbol);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCountedSymbol(long countedSymbolId)
        {
            var countedSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            countedSymbol.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<CountedSymbolModel> GetCountedSymbol(long countedSymbolId)
        {
            var countedSymbol = await _organizationDBContext.CountedSymbol.FirstOrDefaultAsync(x => x.CountedSymbolId == countedSymbolId);
            if (countedSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<CountedSymbolModel>(countedSymbol);
        }

        public async Task<IList<CountedSymbolModel>> GetListCountedSymbol()
        {
            var query = _organizationDBContext.CountedSymbol.AsNoTracking();

            return await query.Select(x => new CountedSymbolModel
            {
                CountedSymbolId = x.CountedSymbolId,
                IsHide = x.IsHide,
                SymbolCode = x.SymbolCode,
                SymbolDescription = x.SymbolDescription,
                CountedPriority = x.CountedPriority,
                CountedSymbolType = x.CountedSymbolType
            }).ToListAsync();
        }
    }
}