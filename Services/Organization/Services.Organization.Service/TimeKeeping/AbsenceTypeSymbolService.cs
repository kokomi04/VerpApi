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
    public interface IAbsenceTypeSymbolService
    {
        Task<int> AddAbsenceTypeSymbol(AbsenceTypeSymbolModel model);
        Task<bool> DeleteAbsenceTypeSymbol(long absenceTypeSymbolId);
        Task<AbsenceTypeSymbolModel> GetAbsenceTypeSymbol(long absenceTypeSymbolId);
        Task<IList<AbsenceTypeSymbolModel>> GetListAbsenceTypeSymbol();
        Task<bool> UpdateAbsenceTypeSymbol(int absenceTypeSymbolId, AbsenceTypeSymbolModel model);
    }

    public class AbsenceTypeSymbolService : IAbsenceTypeSymbolService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;

        public AbsenceTypeSymbolService(OrganizationDBContext organizationDBContext, IMapper mapper)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
        }

        public async Task<int> AddAbsenceTypeSymbol(AbsenceTypeSymbolModel model)
        {
            var entity = _mapper.Map<AbsenceTypeSymbol>(model);

            await _organizationDBContext.AbsenceTypeSymbol.AddAsync(entity);
            await _organizationDBContext.SaveChangesAsync();

            return entity.AbsenceTypeSymbolId;
        }

        public async Task<bool> UpdateAbsenceTypeSymbol(int absenceTypeSymbolId, AbsenceTypeSymbolModel model)
        {
            var absenceTypeSymbol = await _organizationDBContext.AbsenceTypeSymbol.FirstOrDefaultAsync(x => x.AbsenceTypeSymbolId == absenceTypeSymbolId);
            if (absenceTypeSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            model.AbsenceTypeSymbolId = absenceTypeSymbolId;
            _mapper.Map(model, absenceTypeSymbol);

            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAbsenceTypeSymbol(long absenceTypeSymbolId)
        {
            var absenceTypeSymbol = await _organizationDBContext.AbsenceTypeSymbol.FirstOrDefaultAsync(x => x.AbsenceTypeSymbolId == absenceTypeSymbolId);
            if (absenceTypeSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            absenceTypeSymbol.IsDeleted = true;
            await _organizationDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<AbsenceTypeSymbolModel> GetAbsenceTypeSymbol(long absenceTypeSymbolId)
        {
            var absenceTypeSymbol = await _organizationDBContext.AbsenceTypeSymbol.FirstOrDefaultAsync(x => x.AbsenceTypeSymbolId == absenceTypeSymbolId);
            if (absenceTypeSymbol == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return _mapper.Map<AbsenceTypeSymbolModel>(absenceTypeSymbol);
        }

        public async Task<IList<AbsenceTypeSymbolModel>> GetListAbsenceTypeSymbol()
        {
            var query = _organizationDBContext.AbsenceTypeSymbol.AsNoTracking();

            return await query.Select(x => new AbsenceTypeSymbolModel
            {
                AbsenceTypeSymbolId = x.AbsenceTypeSymbolId,
                IsCounted = x.IsCounted,
                IsUsed = x.IsUsed,
                SymbolCode = x.SymbolCode,
                TypeSymbolCode = x.TypeSymbolCode,
                TypeSymbolDescription = x.TypeSymbolDescription
            }).ToListAsync();
        }
    }
}