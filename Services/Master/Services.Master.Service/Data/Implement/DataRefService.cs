using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Services.Master.Model.Data;

namespace VErp.Services.Master.Service.Data.Implement
{
    public class DataRefService : IDataRefService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterContext;
        private readonly ICurrentContextService _currentContextService;

        public DataRefService(MasterDBContext masterContext
            , ILogger<DataRefService> logger
            , IMapper mapper
            , ICurrentContextService currentContextService
            )
        {
            _logger = logger;
            _masterContext = masterContext;
            _mapper = mapper;
            _currentContextService = currentContextService;
        }

        public async Task<IList<DataRefModel>> GetDataRef(EnumObjectType objectTypeId, long? id, string code)
        {
            var sqlParams = new[] {
                new SqlParameter("@Id",id) ,
                new SqlParameter("@Code",code) ,
                new SqlParameter("@TypeId",(int)objectTypeId) ,
            };
            return await _masterContext.QueryList<DataRefModel>("asp_ObjectGetRef", sqlParams, CommandType.StoredProcedure);
        }
    }
}
