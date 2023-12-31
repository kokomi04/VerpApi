﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class ActionButtonExecService : IActionButtonExecService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;


        public ActionButtonExecService(MasterDBContext masterDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<ActionButtonExecService> logger
            , IMapper mapper
            )
        {
            _masterDBContext = masterDBContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<ActionButtonModel>> GetActionButtonsByBillType(EnumObjectType billTypeObjectTypeId, long billTypeObjectId)
        {
            var lst = await (from m in _masterDBContext.ActionButtonBillType
                             join b in _masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                             where b.BillTypeObjectTypeId == (int)billTypeObjectTypeId
                             && m.BillTypeObjectId == billTypeObjectId
                             select b
                          ).ToListAsync();

            return _mapper.Map<IList<ActionButtonModel>>(lst);
        }

        public async Task<ActionButtonModel> ActionButtonExecInfo(int actionButtonId, EnumObjectType billTypeObjectTypeId, long billTypeObjectId)
        {
            var info = await (from m in _masterDBContext.ActionButtonBillType
                              join b in _masterDBContext.ActionButton on m.ActionButtonId equals b.ActionButtonId
                              where b.BillTypeObjectTypeId == (int)billTypeObjectTypeId
                              && m.BillTypeObjectId == billTypeObjectId
                              && m.ActionButtonId == actionButtonId
                              select b
                         ).FirstOrDefaultAsync();

            return _mapper.Map<ActionButtonModel>(info);
        }


    }
}

