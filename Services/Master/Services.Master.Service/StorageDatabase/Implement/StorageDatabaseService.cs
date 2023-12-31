﻿using ActivityLogDB;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Master.StorageDatabase;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.StorageDatabase;
using static Verp.Resources.Master.StorageDatabase.StorageDatabaseValidationMessage;

namespace VErp.Services.Master.Service.StorageDatabase.Implement
{
    public class StorageDatabaseService : IStorageDatabaseService
    {
        private readonly MasterDBContext _masterContext;
        private readonly ActivityLogDBContext _activityLogDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly ICurrentContextService _currentContextService;
        private readonly IPhysicalFileService _physicalFileService;
        private readonly ISubSystemService _manageVErpModuleService;
        private readonly ObjectActivityLogFacade _storageDatabaseActivityLog;

        public StorageDatabaseService(MasterDBContext masterDBContext,
            ActivityLogDBContext activityLogDBContext,
            IOptions<AppSetting> appSetting,
            IActivityLogService activityLogService,
            ILogger<StorageDatabaseService> logger,
            ICurrentContextService currentContextService,
            IPhysicalFileService physicalFileService,
            ISubSystemService manageVErpModuleService
            )
        {
            _masterContext = masterDBContext;
            _activityLogDBContext = activityLogDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;

            _currentContextService = currentContextService;
            _physicalFileService = physicalFileService;
            _manageVErpModuleService = manageVErpModuleService;

            _storageDatabaseActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.StorageDabase);
        }

        public async Task<bool> BackupStorage(BackupStorageInput backupStorage)
        {
            var backupPoint = DateTime.UtcNow.GetUnix();

            var lsBackupStorage = new List<BackupStorage>();
            foreach (var storage in backupStorage.storages)
            {

                string outDirectory = GenerateOutDirectory(backupPoint, storage.ModuleTypeId.GetStringValue());
                var dbs = _manageVErpModuleService.GetDbByModuleTypeId(storage.ModuleTypeId);
                foreach (string db in dbs)
                {
                    string filePath = $"{outDirectory}/{db.ToLower()}.bak";
                    var sqlDB = $@"BACKUP DATABASE {db} TO DISK='{GetPhysicalFilePath(filePath)}'";
                    await _activityLogDBContext.Database.ExecuteSqlRawAsync(sqlDB);
                    if (!System.IO.File.Exists(GetPhysicalFilePath(filePath)))
                    {
                        throw new BadRequestException(BackupErrorCode.NotFoundFileAfterBackup);
                    }
                }

                string fileZip = $"{storage.ModuleTypeId.GetStringValue()}.zip";

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
                        ModuleTypeId = (int)storage.ModuleTypeId,
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


                    await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.CreateBackup)
                     .MessageResourceFormatDatas(backupStorage.Title)
                     .ObjectId(backupPoint)
                     .JsonData(lsBackupStorage)
                     .CreateLog();
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
            var backupPointRestore = new BackupStorageInput
            {
                Title = "System/Backup before restore",
                storages = Enum.GetValues(typeof(EnumModuleType)).OfType<EnumModuleType>().Select(x => new SubSystemInfo
                {
                    ModuleTypeId = x,
                    Title = x.GetEnumDescription()
                }).ToList(),
            };
            await BackupStorage(backupPointRestore);

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

            await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.RestoreDatabase)
                  .MessageResourceFormatDatas(string.Join(", ", backups.Select(b => b.Title).Distinct().ToArray()))
                  .ObjectId(backupPoint)
                  .JsonData(backups)
                  .CreateLog();

            return true;
        }

        public async Task<bool> RestoreForBackupPoint(long backupPoint, int moduleTypeId)
        {
            var backup = await _masterContext.BackupStorage.FirstOrDefaultAsync(x => x.BackupPoint == backupPoint && x.ModuleTypeId == moduleTypeId);
            if (backup == null)
            {
                throw new BadRequestException(BackupErrorCode.NotFoundBackupForDatabase);
            }
            var backupPointRestore = new BackupStorageInput
            {
                Title = $"System/Backup {((EnumModuleType)backup.ModuleTypeId).GetEnumDescription()} before restore",
                storages = new List<SubSystemInfo> { new SubSystemInfo { ModuleTypeId = (EnumModuleType)backup.ModuleTypeId } },
            };

            await BackupStorage(backupPointRestore);
            await RestoreDatabase(backup);

            backup.RestoreDate = DateTime.UtcNow;
            backup.UpdatedByUserId = _currentContextService.UserId;
            await _masterContext.SaveChangesAsync();

            await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.RestoreDatabaseModule)
                .MessageResourceFormatDatas((EnumModuleType)moduleTypeId, backup.Title)
                .ObjectId(backupPoint)
                .JsonData(backup)
                .CreateLog();

            return true;
        }

        private async Task RestoreDatabase(BackupStorage backup)
        {
            var fileInfo = await _physicalFileService.GetSimpleFileInfo(backup.FileId);

            if (fileInfo == null)
                throw BackupFileNotFound.BadRequest(FileErrorCode.FileNotFound);

            var dbs = _manageVErpModuleService.GetDbByModuleTypeId((EnumModuleType)backup.ModuleTypeId);
            foreach (string db in dbs)
            {
                string outDirectory = Path.GetDirectoryName(fileInfo.FilePath);
                string filePath = GetPhysicalFilePath($"{outDirectory}/{db}.bak");

                await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.RestoreStarted)
                    .MessageResourceFormatDatas(db, (EnumModuleType)backup.ModuleTypeId, backup.Title)
                    .ObjectId(backup.BackupPoint)
                    .JsonData(backup)
                    .CreateLog();

                DbContext dbContext = _activityLogDBContext;

                try
                {
                    dbContext.Database.SetCommandTimeout(new TimeSpan(1, 0, 0));
                    await dbContext.Database.ExecuteSqlRawAsync($"Alter Database {db} SET SINGLE_USER With ROLLBACK AFTER 30");
                    await dbContext.Database.ExecuteSqlRawAsync($"RESTORE DATABASE {db} FROM DISK = '{filePath}' WITH REPLACE");
                }
                catch (Exception)
                {
                    await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.RestoreFail)
                      .MessageResourceFormatDatas(db, (EnumModuleType)backup.ModuleTypeId, backup.Title)
                      .ObjectId(backup.BackupPoint)
                      .JsonData(backup)
                      .CreateLog();

                    throw;
                }
                finally
                {
                    await dbContext.Database.ExecuteSqlRawAsync($"Alter Database {db}  SET MULTI_USER");
                }

                await _storageDatabaseActivityLog.LogBuilder(() => StorageDatabaseActivityLogMessage.RestoreSuccess)
                    .MessageResourceFormatDatas(db, (EnumModuleType)backup.ModuleTypeId, backup.Title)
                    .ObjectId(backup.BackupPoint)
                    .JsonData(backup)
                    .CreateLog();
            }
        }

        private string GenerateOutDirectory(long backupPoint, string moduleType)
        {
            var relativeFolder = $"/_backup_/{backupPoint}/{moduleType}";

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


        public async Task<IList<BackupStorageOutput>> GetBackupStorages(int? moduleTypeId)
        {
            var query = _masterContext.BackupStorage.AsQueryable();
            if (moduleTypeId.HasValue)
            {
                query = query.Where(x => x.ModuleTypeId == moduleTypeId);
            }

            var data = (await query.ToListAsync())
                .GroupBy(x => new { x.BackupPoint, x.Title })
                .Select(g => new BackupStorageOutput
                {
                    BackupPoint = g.Key.BackupPoint,
                    Title = g.Key.Title,
                    backupStorages = g.Select(gg => new BackupStorageModel
                    {
                        BackupDate = gg.BackupDate.GetUnix(),
                        BackupPoint = gg.BackupPoint,
                        ModuleTypeId = gg.ModuleTypeId,
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
