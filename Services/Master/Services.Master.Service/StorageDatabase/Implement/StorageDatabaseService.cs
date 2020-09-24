using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.StorageDatabase;

namespace VErp.Services.Master.Service.StorageDatabase.Implement
{
    public class StorageDatabaseService : IStorageDatabaseService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger<StorageDatabaseService> _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IServiceCollection _serviceDescriptors;
        public StorageDatabaseService(MasterDBContext masterDBContext,
            IOptions<AppSetting> appSetting,
            IActivityLogService activityLogService,
            ILogger<StorageDatabaseService> logger,
            IServiceCollection serviceDescriptors)
        {
            _activityLogService = activityLogService;
            _appSetting = appSetting.Value;
            _masterContext = masterDBContext;
            _logger = logger;
            _serviceDescriptors = serviceDescriptors;
        }

        public async Task<bool> BackupStorage(BackupStorageInput backupStorage)
        {
            var backupPoint = DateTime.UtcNow.GetUnix();
            string outDirectory = GenerateOutDirectory(backupPoint);

            var lsBackupStorage = new List<BackupStorage>();
            foreach (var storage in backupStorage.storages)
            {
                string filePath = GetPhysicalFilePath($"{outDirectory}/{storage.DatabaseName.ToLower()}.bak");
                var sqlDB = $@"BACKUP DATABASE {storage.DatabaseName} TO DISK='{filePath}'";

                await _masterContext.Database.ExecuteSqlRawAsync(sqlDB);

                if (!System.IO.File.Exists(filePath))
                {
                    lsBackupStorage.ForEach(x =>
                    {
                        System.IO.File.Delete(x.FilePath);
                    });
                    throw new BadRequestException(BackupErrorCode.NotFoundFileAfterBackup);
                }

                lsBackupStorage.Add(new BackupStorage
                {
                    BackupDate = DateTime.UtcNow,
                    BackupPoint = backupPoint,
                    FileName = $"{storage.DatabaseName.ToLower()}.bak",
                    FilePath = $"{outDirectory}/{storage.DatabaseName.ToLower()}.bak",
                    Title = backupStorage.Title,
                    DatabaseId = storage.DatabaseId
                });
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _masterContext.BackupStorage.AddRangeAsync(lsBackupStorage);
                    await _masterContext.SaveChangesAsync();
                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.StorageDabase, backupPoint, $"Backup database into backup point: {backupPoint}", lsBackupStorage.JsonSerialize());
                }
                catch (Exception ex)
                {
                    trans.TryRollbackTransaction();
                    _logger.LogError(ex, "Backup database");
                    throw;
                }

            }

            return true;
        }

        public async Task<IList<StorageDatabseModel>> GetList()
        {
            var lsDbRegister = new List<string>();
            foreach (var sd in _serviceDescriptors.Where(x => x.ServiceType.IsSubclassOf(typeof(DbContext))))
            {
                lsDbRegister.Add(sd.ServiceType.Name);
            }

            var databases = await GetAllDatabase();

            var results = new List<StorageDatabseModel>();
            databases.ForEach(x =>
            {
                if (lsDbRegister.Any(y => y.Contains(x["name"])))
                {
                    results.Add(new StorageDatabseModel
                    {
                        DatabaseName = x["name"].ToString(),
                        DatabaseId = int.Parse(x["database_id"].ToString()),
                    });
                }
            });
            return results;
        }

        public async Task<bool> RestoreForBackupPoint(long backupPoint)
        {
            var backups = _masterContext.BackupStorage.Where(x => x.BackupPoint == backupPoint).ToList();
            if (backups.Count < 0)
            {
                throw new BadRequestException(BackupErrorCode.NotFoundBackupPoint);
            }
            var databases = await GetAllDatabase();

            foreach (var backup in backups)
            {
                await RestoreDatabase(databases, backup);
                backup.RestoreDate = DateTime.UtcNow;
            }

            await _masterContext.SaveChangesAsync();

            return true;
        }

        private async Task<List<NonCamelCaseDictionary>> GetAllDatabase()
        {
            string sqlDB = @"SELECT name, database_id, create_date
                                FROM sys.databases";
            var data = (await _masterContext.QueryDataTable(sqlDB, Array.Empty<SqlParameter>())).ConvertData();
            return data;
        }

        public async Task<bool> RestoreForBackupPoint(long backupPoint, int databaseId)
        {
            var backup = await _masterContext.BackupStorage.FirstOrDefaultAsync(x => x.BackupPoint == backupPoint && x.DatabaseId == databaseId);
            if (backup == null)
            {
                throw new BadRequestException(BackupErrorCode.NotFoundBackupForDatabase);
            }
            var databases = await GetAllDatabase();
            await RestoreDatabase(databases, backup);

            backup.RestoreDate = DateTime.UtcNow;
            await _masterContext.SaveChangesAsync();

            return true;
        }

        private async Task RestoreDatabase(List<NonCamelCaseDictionary> databases, BackupStorage backup)
        {
            var dbInfo = databases.FirstOrDefault(x => x["database_id"].Equals(backup.DatabaseId.ToString()));
            string sqlDB = $@"RESTORE DATABASE {dbInfo["name"]}  
                                    FROM DISK = '{GetPhysicalFilePath(backup.FilePath)}'";

            await _masterContext.Database.ExecuteSqlRawAsync(sqlDB);
            await _activityLogService.CreateLog(EnumObjectType.StorageDabase, backup.BackupPoint,
                $"Restore database {dbInfo["name"]} from backup point: {backup.BackupPoint}", backup.JsonSerialize());
        }

        private string GenerateOutDirectory(long point)
        {
            var relativeFolder = $"/_backup_/{point}";
            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return obsoluteFolder;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }

        public async Task<IList<BackupStorageOutput>> GetBackupStorages(int databaseId = 0)
        {
            var query = _masterContext.BackupStorage.AsQueryable();
            if (databaseId != 0)
            {
                query = query.Where(x => x.DatabaseId == databaseId);
            }

            var data = query.AsEnumerable().GroupBy(x => new { x.BackupPoint, x.Title })
                .Select(g => new BackupStorageOutput
                {
                    BackupPoint = g.Key.BackupPoint,
                    Title = g.Key.Title,
                    backupStorages = g.Select(gg => new BackupStorageModel
                    {
                        BackupDate = gg.BackupDate.GetUnix(),
                        BackupPoint = gg.BackupPoint,
                        DatabaseId = gg.DatabaseId,
                        FileName = gg.FileName,
                        FilePath = gg.FilePath,
                        RestoreDate = gg.RestoreDate.HasValue ? gg.RestoreDate.GetUnix().Value : 0,
                        Title = gg.Title,
                    }).ToList()
                }).ToList();

            return data;
        }
    }
}
