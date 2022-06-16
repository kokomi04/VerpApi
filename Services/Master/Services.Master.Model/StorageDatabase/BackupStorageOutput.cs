﻿using System.Collections.Generic;

namespace VErp.Services.Master.Model.StorageDatabase
{
    public class BackupStorageOutput
    {
        public long BackupPoint { get; set; }
        public string Title { get; set; }
        public List<BackupStorageModel> backupStorages { get; set; }
    }

    public class BackupStorageModel
    {
        public int ModuleTypeId { get; set; }
        public long BackupPoint { get; set; }
        public string Title { get; set; }
        public long FileId { get; set; }
        public long BackupDate { get; set; }
        public long RestoreDate { get; set; }
    }
}
