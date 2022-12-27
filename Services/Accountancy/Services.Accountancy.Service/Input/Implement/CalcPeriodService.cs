using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class CalcPeriodPrivateService : CalcPeriodServiceBase, ICalcPeriodPrivateService
    {
        public CalcPeriodPrivateService(AccountancyDBPrivateContext accountancyDBContext) : base(accountancyDBContext)
        {
        }
    }
    public class CalcPeriodPublicService : CalcPeriodServiceBase, ICalcPeriodPublicService
    {
        public CalcPeriodPublicService(AccountancyDBPublicContext accountancyDBContext) : base(accountancyDBContext)
        {
        }
    }

    public abstract class CalcPeriodServiceBase : ICalcPeriodServiceBase
    {

        private readonly AccountancyDBContext _accountancyDBContext;

        public CalcPeriodServiceBase(AccountancyDBContext accountancyDBContext)
        {
            _accountancyDBContext = accountancyDBContext;
        }

        public async Task<PageData<CalcPeriodListModel>> GetList(EnumCalcPeriodType calcPeriodTypeId, string keyword, long? fromDate, long? toDate, int page, int? size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountancyDBContext.CalcPeriod.Where(c => c.CalcPeriodTypeId == (int)calcPeriodTypeId);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.Title.Contains(keyword) || c.Description.Contains(keyword));
            }

            if (fromDate.HasValue)
            {
                var date = fromDate.Value.UnixToDateTime();
                query = query.Where(c => c.FromDate >= date);
            }

            if (toDate.HasValue)
            {
                var date = toDate.Value.UnixToDateTime();
                query = query.Where(c => c.FromDate <= date);
            }

            var total = await query.CountAsync();
            query = query.OrderByDescending(c => c.FromDate).ThenByDescending(c => c.CreatedDatetimeUtc);
            if (size > 0)
            {
                query = query.Skip((page - 1) * size.Value).Take(size.Value);
            }
            var pagedData = query
                .Select(c => new
                {
                    c.CalcPeriodId,
                    c.Title,
                    c.Description,
                    c.FilterHash,
                    c.FromDate,
                    c.ToDate,
                    c.CreatedByUserId,
                    c.CreatedDatetimeUtc
                })
                .ToList()
                .Select(c => new CalcPeriodListModel()
                {


                    CalcPeriodId = c.CalcPeriodId,
                    Title = c.Title,
                    Description = c.Description,
                    FilterHash = c.FilterHash,
                    FromDate = c.FromDate.GetUnix(),
                    ToDate = c.ToDate.GetUnix(),
                    CreatedByUserId = c.CreatedByUserId,
                    CreatedDatetimeUtc = c.CreatedDatetimeUtc.GetUnix()
                }).ToList();

            return (pagedData, total);
        }

        public async Task<CalcPeriodDetailModel> GetInfo(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId)
        {
            var info = await _accountancyDBContext.CalcPeriod.FirstOrDefaultAsync(c => c.CalcPeriodTypeId == (int)calcPeriodTypeId && c.CalcPeriodId == calcPeriodId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }

            var data = new CalcPeriodDetailModel
            {
                CalcPeriodId = info.CalcPeriodId,
                Title = info.Title,
                Description = info.Description,
                FilterHash = info.FilterHash,
                FromDate = info.FromDate.GetUnix(),
                ToDate = info.ToDate.GetUnix(),
                CreatedByUserId = info.CreatedByUserId,
                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                FilterData = info.FilterData,
                Data = info.Data
            };

            return data;
        }

        public async Task<CalcPeriodView<TFilter, TOutput>> CalcPeriodInfo<TFilter, TOutput>(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId)
        {
            var info = await GetInfo(calcPeriodTypeId, calcPeriodId);
            return new CalcPeriodView<TFilter, TOutput>()
            {
                CalcPeriodInfo = info,
                FilterData = info.FilterData.JsonDeserialize<TFilter>(),
                OutputData = info.Data.JsonDeserialize<TOutput>()
            };
        }

        public async Task<bool> Delete(EnumCalcPeriodType calcPeriodTypeId, long calcPeriodId)
        {
            var info = await _accountancyDBContext.CalcPeriod.FirstOrDefaultAsync(c => c.CalcPeriodTypeId == (int)calcPeriodTypeId && c.CalcPeriodId == calcPeriodId);
            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound);
            }

            info.IsDeleted = true;

            await _accountancyDBContext.SaveChangesAsync();
            return true;
        }


        public async Task<long> Create(EnumCalcPeriodType calcPeriodTypeId, string title, string description, long? fromDate, long? toDate, IFilterHashData filterData, object data)
        {
            var info = new CalcPeriod
            {
                Title = title,
                Description = description,
                CalcPeriodTypeId = (int)calcPeriodTypeId,
                FilterData = filterData.JsonSerialize(),
                FilterHash = filterData?.GetHashString(),
                FromDate = fromDate.UnixToDateTime(),
                ToDate = toDate.UnixToDateTime(),

                Data = data?.JsonSerialize()
            };
            await _accountancyDBContext.CalcPeriod.AddAsync(info);
            await _accountancyDBContext.SaveChangesAsync();
            return info.CalcPeriodId;
        }

    }
}
