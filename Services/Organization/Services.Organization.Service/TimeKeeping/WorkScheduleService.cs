using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Services.Organization.Model.TimeKeeping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.TimeKeeping;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.TimeKeeping
{
    public interface IWorkScheduleService
    {
        Task<int> AddWorkSchedule(WorkScheduleModel model);
        Task<bool> DeleteWorkSchedule(int workScheduleId);
        Task<IList<WorkScheduleModel>> GetListWorkSchedule();
        Task<WorkScheduleModel> GetWorkSchedule(int workScheduleId);
        Task<bool> UpdateWorkSchedule(int workScheduleId, WorkScheduleModel model);
    }

    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly OrganizationDBContext _organizationDBContext;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _workScheduleActivityLog;

        public WorkScheduleService(OrganizationDBContext organizationDBContext, IMapper mapper, IActivityLogService activityLogService)
        {
            _organizationDBContext = organizationDBContext;
            _mapper = mapper;
            _workScheduleActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.WorkSchedule);
        }

        public async Task<int> AddWorkSchedule(WorkScheduleModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {

                var entity = _mapper.Map<WorkSchedule>(model);

                await _organizationDBContext.WorkSchedule.AddAsync(entity);
                await _organizationDBContext.SaveChangesAsync();
                await AddArrangeShift(model.ArrangeShifts, entity.WorkScheduleId);

                await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.CreateWorkSchedule)
                          .MessageResourceFormatDatas(entity.WorkScheduleTitle)
                          .ObjectId(entity.WorkScheduleId)
                          .JsonData(model.JsonSerialize())
                          .CreateLog();

                await trans.CommitAsync();

                return entity.WorkScheduleId;

            }
            catch (System.Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        private async Task AddArrangeShift(IList<ArrangeShiftModel> arrangeShifts, int workScheduleId)
        {
            if (arrangeShifts.Count > 0)
            {
                foreach (var arrangeShift in arrangeShifts)
                {
                    var eArrangeShift = _mapper.Map<ArrangeShift>(arrangeShift);
                    eArrangeShift.WorkScheduleId = workScheduleId;
                    eArrangeShift.ArrangeShiftId = 0;

                    await _organizationDBContext.ArrangeShift.AddAsync(eArrangeShift);

                    if (arrangeShift.Items.Count > 0)
                    {
                        await _organizationDBContext.SaveChangesAsync();

                        var dataSet = _organizationDBContext.Set<ArrangeShiftItem>();
                        await AddEntityWithInner<ArrangeShiftItemModel, ArrangeShiftItem>(dataSet, arrangeShift.Items, new[] { eArrangeShift.ArrangeShiftId });
                    }
                }

                await _organizationDBContext.SaveChangesAsync();
            }
        }

        private async Task AddEntityWithInner<T, E>(DbSet<E> dataSet, IList<T> items, int[] refForeginKeyId, bool ignoreInner = false) where E : class where T : class, IRefForeginKey, IInnerBySelf<T>
        {
            foreach (var item in items)
            {
                item.SetRefForeginKey(refForeginKeyId);

                var eItem = _mapper.Map<E>(item);

                await dataSet.AddAsync(eItem);

                if (item.HasInner())
                    await _organizationDBContext.SaveChangesAsync();

                if (!ignoreInner && item.HasInner())
                {
                    var innerRefForeginKeyId = refForeginKeyId.ToList();
                    innerRefForeginKeyId.Add((_mapper.Map<T>(eItem).GetPrimaryKey()));

                    await AddEntityWithInner<T, E>(dataSet, item.GetInner(), innerRefForeginKeyId.ToArray(), ignoreInner: true);
                }

            }

            await Task.CompletedTask;
        }

        public async Task<bool> UpdateWorkSchedule(int workScheduleId, WorkScheduleModel model)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
                if (workSchedule == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var arrOldArrangeShifts = await _organizationDBContext.ArrangeShift.Where(x => x.WorkScheduleId == workSchedule.WorkScheduleId).ToListAsync();
                var arrOldArrangeShiftItems = await _organizationDBContext.ArrangeShiftItem.Where(i => arrOldArrangeShifts.Select(x => x.ArrangeShiftId).Contains(i.ArrangeShiftId) && !i.ParentArrangeShiftItemId.HasValue).ToListAsync();
                var arrOldInnerArrangeShiftItems = await _organizationDBContext.ArrangeShiftItem.Where(i => arrOldArrangeShifts.Select(x => x.ArrangeShiftId).Contains(i.ArrangeShiftId) && !i.ParentArrangeShiftItemId.HasValue).ToListAsync();

                _organizationDBContext.ArrangeShiftItem.RemoveRange(arrOldInnerArrangeShiftItems);
                _organizationDBContext.ArrangeShiftItem.RemoveRange(arrOldArrangeShiftItems);
                _organizationDBContext.ArrangeShift.RemoveRange(arrOldArrangeShifts);

                await _organizationDBContext.SaveChangesAsync();

                model.WorkScheduleId = workScheduleId;

                _mapper.Map(model, workSchedule);

                await _organizationDBContext.SaveChangesAsync();

                await AddArrangeShift(model.ArrangeShifts, model.WorkScheduleId);

                await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.UpdateWorkSchedule)
                         .MessageResourceFormatDatas(workSchedule.WorkScheduleTitle)
                         .ObjectId(workSchedule.WorkScheduleId)
                         .JsonData(model.JsonSerialize())
                         .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteWorkSchedule(int workScheduleId)
        {
            var trans = await _organizationDBContext.Database.BeginTransactionAsync();
            try
            {
                var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
                if (workSchedule == null)
                    throw new BadRequestException(GeneralCode.ItemNotFound);

                var arrOldArrangeShifts = await _organizationDBContext.ArrangeShift.Where(x => x.WorkScheduleId == workSchedule.WorkScheduleId).ToListAsync();
                var arrOldArrangeShiftItems = await _organizationDBContext.ArrangeShiftItem.Where(i => arrOldArrangeShifts.Select(x => x.ArrangeShiftId).Contains(i.ArrangeShiftId) && !i.ParentArrangeShiftItemId.HasValue).ToListAsync();
                var arrOldInnerArrangeShiftItems = await _organizationDBContext.ArrangeShiftItem.Where(i => arrOldArrangeShifts.Select(x => x.ArrangeShiftId).Contains(i.ArrangeShiftId) && i.ParentArrangeShiftItemId.HasValue).ToListAsync();

                _organizationDBContext.ArrangeShiftItem.RemoveRange(arrOldInnerArrangeShiftItems);
                _organizationDBContext.ArrangeShiftItem.RemoveRange(arrOldArrangeShiftItems);
                _organizationDBContext.ArrangeShift.RemoveRange(arrOldArrangeShifts);

                await _organizationDBContext.SaveChangesAsync();

                workSchedule.IsDeleted = true;

                await _organizationDBContext.SaveChangesAsync();

                await _workScheduleActivityLog.LogBuilder(() => WorkScheduleActivityLogMessage.DeleteWorkSchedule)
                         .MessageResourceFormatDatas(workSchedule.WorkScheduleTitle)
                         .ObjectId(workSchedule.WorkScheduleId)
                         .CreateLog();

                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception)
            {
                await trans.RollbackAsync();
                throw;
            }

        }

        public async Task<WorkScheduleModel> GetWorkSchedule(int workScheduleId)
        {
            var workSchedule = await _organizationDBContext.WorkSchedule.FirstOrDefaultAsync(x => x.WorkScheduleId == workScheduleId);
            if (workSchedule == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var arrangeShifts = await _organizationDBContext.ArrangeShift.Where(x => x.WorkScheduleId == workScheduleId).ProjectTo<ArrangeShiftModel>(_mapper.ConfigurationProvider).ToListAsync();
            var arrangeShiftItems = await _organizationDBContext.ArrangeShiftItem.Where(x => arrangeShifts.Select(x => x.ArrangeShiftId).Contains(x.ArrangeShiftId)).ToListAsync();

            foreach (var shift in arrangeShifts)
            {
                var items = arrangeShiftItems.Where(x => x.ArrangeShiftId == shift.ArrangeShiftId && !x.ParentArrangeShiftItemId.HasValue)
                                             .AsQueryable()
                                             .ProjectTo<ArrangeShiftItemModel>(_mapper.ConfigurationProvider)
                                             .ToList();
                // foreach (var item in items)
                // {
                //     if(arrangeShiftItems.Any(x=>x.ParentArrangeShiftItemId == item.ArrangeShiftItemId))
                //     {
                //         item.InnerItems = arrangeShiftItems.Where(x => x.ParentArrangeShiftItemId.HasValue == true && x.ParentArrangeShiftItemId.GetValueOrDefault() == item.ArrangeShiftItemId)
                //                                            .AsQueryable()
                //                                            .ProjectTo<ArrangeShiftItemModel>(_mapper.ConfigurationProvider)
                //                                            .ToList();
                //     }
                // }
                shift.Items = items;
            }

            var result = _mapper.Map<WorkScheduleModel>(workSchedule);
            result.ArrangeShifts = arrangeShifts;

            return result;
        }

        public async Task<IList<WorkScheduleModel>> GetListWorkSchedule()
        {
            var query = _organizationDBContext.WorkSchedule.AsNoTracking();

            return await query
            .ProjectTo<WorkScheduleModel>(_mapper.ConfigurationProvider)
            .ToListAsync();
        }
    }
}