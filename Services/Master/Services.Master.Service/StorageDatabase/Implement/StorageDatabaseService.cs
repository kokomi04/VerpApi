using Elasticsearch.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
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
        private readonly ICurrentContextService _currentContextService;
        private readonly IPhysicalFileService _physicalFileService;
        private readonly ISubSystemService _manageVErpModuleService;

        public StorageDatabaseService(MasterDBContext masterDBContext,
            IOptions<AppSetting> appSetting,
            IActivityLogService activityLogService,
            ILogger<StorageDatabaseService> logger,
            ICurrentContextService currentContextService,
            IPhysicalFileService physicalFileService,
            ISubSystemService manageVErpModuleService)
        {
            _activityLogService = activityLogService;
            _appSetting = appSetting.Value;
            _masterContext = masterDBContext;
            _logger = logger;
            _currentContextService = currentContextService;
            _physicalFileService = physicalFileService;
            _manageVErpModuleService = manageVErpModuleService;
        }

        public async Task<bool> BackupStorage(BackupStorageInput backupStorage)
        {
            var backupPoint = DateTime.UtcNow.GetUnix();

            var lsBackupStorage = new List<BackupStorage>();
            foreach (var storage in backupStorage.storages)
            {

                string outDirectory = GenerateOutDirectory(backupPoint,storage.SubSystemId.GetStringValue());
                var dbs = await _manageVErpModuleService.GetDbBySubSystemId(storage.SubSystemId);
                foreach (string db in dbs)
                {
                    string filePath = $"{outDirectory}/{db.ToLower()}.bak";
#if !DEBUG
                    var sqlDB = $@"BACKUP DATABASE {db} TO DISK='{GetPhysicalFilePath(filePath)}'";
                    await _masterContext.Database.ExecuteSqlRawAsync(sqlDB);
                    if (!System.IO.File.Exists(GetPhysicalFilePath(filePath)))
                    {
                        throw new BadRequestException(BackupErrorCode.NotFoundFileAfterBackup);
                    }
#endif
                }

                string fileZip = $"{storage.SubSystemId.GetStringValue()}.zip";

                if (ZipDirectory(GetPhysicalFilePath(outDirectory), fileZip))
                {
                    var fileId = await _physicalFileService.SaveSimpleFileInfo(EnumObjectType.StorageDabase, new SimpleFileInfo
                    {
                        FileTypeId = (int)EnumFileType.Other,
                        FilePath = $"{outDirectory}/{fileZip}",
                        FileName = fileZip,
                        ContentType = "application/zip",
                        FileLength = 0,
                    });

                    lsBackupStorage.Add(new BackupStorage
                    {
                        BackupDate = DateTime.UtcNow,
                        BackupPoint = backupPoint,
                        Title = backupStorage.Title,
                        ModuleId = (int)storage.SubSystemId,
                        IsDeleted = false,
                        FileId = fileId,
                        CreatedByUserId = _currentContextService.UserId,
                        UpdatedByUserId = _currentContextService.UserId,
                    });
                }
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

        public async Task<bool> RestoreForBackupPoint(long backupPoint)
        {
            var backups = _masterContext.BackupStorage.Where(x => x.BackupPoint == backupPoint).ToList();
            if (backups.Count < 0)
            {
                throw new BadRequestException(BackupErrorCode.NotFoundBackupPoint);
            }

            foreach (var backup in backups)
            {
                await RestoreDatabase(backup);

                backup.RestoreDate = DateTime.UtcNow;
                backup.UpdatedByUserId = _currentContextService.UserId;
            }

            await _masterContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreForBackupPoint(long backupPoint, int moduleId)
        {
            var backup = await _masterContext.BackupStorage.FirstOrDefaultAsync(x => x.BackupPoint == backupPoint && x.ModuleId == moduleId);
            if (backup == null)
            {
                throw new BadRequestException(BackupErrorCode.NotFoundBackupForDatabase);
            }
            await RestoreDatabase(backup);

            backup.RestoreDate = DateTime.UtcNow;
            backup.UpdatedByUserId = _currentContextService.UserId;
            await _masterContext.SaveChangesAsync();

            return true;
        }

        private async Task RestoreDatabase(BackupStorage backup)
        {
            var fileInfo = await _physicalFileService.GetSimpleFileInfo(backup.FileId);

            if (fileInfo == null)
                throw new BadRequestException(FileErrorCode.FileNotFound, $"Không tìm thông tin file backup");
            var dbs = await _manageVErpModuleService.GetDbBySubSystemId((EnumModuleType)backup.ModuleId);
            foreach(string db in dbs)
            {
                string outDirectory = Path.GetDirectoryName(fileInfo.FilePath);
                string filePath = GetPhysicalFilePath($"{outDirectory}/{db}.bak");
#if !DEBUG
                string sqlDB = $@"RESTORE DATABASE {db}  
                                    FROM DISK = '{filePath}'";
                await _masterContext.Database.ExecuteSqlRawAsync(sqlDB);
#endif
                await _activityLogService.CreateLog(EnumObjectType.StorageDabase, backup.BackupPoint,
                $"Restore database {db} of module {backup.ModuleId} from backup point: {backup.BackupPoint}", backup.JsonSerialize());
            }
        }

        private string GenerateOutDirectory(long backupPoint, string moduleName)
        {
            var relativeFolder = $"/_backup_/{backupPoint}/{moduleName}";

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFolder;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            filePath = filePath.Replace('\\', '/');

            while (filePath.StartsWith('.') || filePath.StartsWith('/'))
            {
                filePath = filePath.TrimStart('/').TrimStart('.');
            }

            return _appSetting.BackupStorage.FileBackupFolder.TrimEnd('/').TrimEnd('\\') + "/" + filePath;
        }

        private bool ZipDirectory(string folderInput, string fileName)
        {
            string filePath = GetPhysicalFilePath(fileName);
            ZipFile.CreateFromDirectory(folderInput, filePath);
            File.Move(filePath, $"{folderInput}/{fileName}");
            return true;
        }


        public async Task<IList<BackupStorageOutput>> GetBackupStorages(int moduleId = 0)
        {
            var query = _masterContext.BackupStorage.AsQueryable();
            if (moduleId != 0)
            {
                query = query.Where(x => x.ModuleId == moduleId);
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
                        ModuleId = gg.ModuleId,
                        FileId = gg.FileId,
                        RestoreDate = gg.RestoreDate.HasValue ? gg.RestoreDate.GetUnix().Value : 0,
                        Title = gg.Title,
                    }).ToList()
                })
                .OrderByDescending(x => x.BackupPoint).ToList();

            return data;
        }
    }
}
