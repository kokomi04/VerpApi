﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Services.Master.Model.StorageDatabase;
using VErp.Services.Master.Service.StorageDatabase;

namespace VErpApi.Controllers.System
{
    [Route("api/StorageDatabase")]
    [ApiController]
    public class StorageDatabaseController : ControllerBase
    {
        private readonly IStorageDatabaseService _storageDbService;

        public StorageDatabaseController(IStorageDatabaseService storageDatabaseService)
        {
            _storageDbService = storageDatabaseService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<StorageDatabseModel>> GetList()
        {
            return await _storageDbService.GetList();
        }

        [HttpGet]
        [Route("backupPoints/{databaseId}")]
        public async Task<IList<BackupStorageOutput>> GetBackupStorages([FromRoute]int databaseId)
        {
            return await _storageDbService.GetBackupStorages(databaseId);
        }

        [HttpGet]
        [Route("backupPoints")]
        public async Task<IList<BackupStorageOutput>> GetBackupStorages()
        {
            return await _storageDbService.GetBackupStorages();
        }

        [HttpPost]
        [Route("backup")]
        public async Task<bool> BackupDatabase([FromBody]BackupStorageInput  storageModel)
        {
            return await _storageDbService.BackupStorage(storageModel);
        }

        [HttpPost]
        [Route("restore/{backupPoint}/{databaseId}")]
        public async Task<bool> RestoreForBackupPoint([FromRoute] long backupPoint, [FromRoute] int databaseId)
        {
            return await _storageDbService.RestoreForBackupPoint(backupPoint, databaseId);
        }

        [HttpPost]
        [Route("restore/{backupPoint}")]
        public async Task<bool> RestoreForBackupPoint([FromRoute] long backupPoint)
        {
            return await _storageDbService.RestoreForBackupPoint(backupPoint);
        }
    }
}
