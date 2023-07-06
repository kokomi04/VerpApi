using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class PrintConfigHeaderService : IPrintConfigHeaderService
    {
        private readonly MasterDBContext _masterDBContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public PrintConfigHeaderService(MasterDBContext masterDBContext, ILogger<PrintConfigHeaderService> logger, IMapper mapper)
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
        }


        public async Task<PageData<PrintConfigHeaderViewModel>> Search(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterDBContext.PrintConfigHeader.Where(x => x.IsDeleted == false).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.PrintHeaderName.Contains(keyword) || x.Title.Contains(keyword));
            
            var total = await query.CountAsync();
            var lst = await(size > 0 ? (query.Skip((page - 1) * size)).Take(size) : query)
                .OrderBy(x=>x.SortOrder)
                .ProjectTo<PrintConfigHeaderViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }
        public async Task<PrintConfigHeaderModel> GetHeaderById(int headerId)
        {
            var header = await _masterDBContext.PrintConfigHeader
                    .Where(x => x.IsDeleted == false && x.PrintConfigHeaderId == headerId)
                    .ProjectTo<PrintConfigHeaderModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            if (header == null) throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

            return header;
        }
        public async Task<int> CreateHeader(PrintConfigHeaderModel model)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var entity = _mapper.Map<PrintConfigHeader>(model);

                await _masterDBContext.PrintConfigHeader.AddAsync(entity);
                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return entity.PrintConfigHeaderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePrintConfigHeader");
                throw;
            }
        }
        public async Task<bool> UpdateHeader(int headerId,  PrintConfigHeaderModel model)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var header = await _masterDBContext.PrintConfigHeader.FindAsync(headerId);

                if (header == null) 
                    throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

                _mapper.Map(model, header);

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePrintConfigHeader");
                throw;
            }
        }

        public async Task<bool> DeleteHeader(int headerId)
        {
            await using var trans = await _masterDBContext.Database.BeginTransactionAsync();

            try
            {
                var header = await _masterDBContext.PrintConfigHeader.FindAsync(headerId);

                if (header == null)
                    throw new BadRequestException("Không tìm thấy cấu hình header phiếu in");

                header.IsDeleted = true;

                await _masterDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeletePrintConfigHeader");
                throw;
            }
        }
    }
}
