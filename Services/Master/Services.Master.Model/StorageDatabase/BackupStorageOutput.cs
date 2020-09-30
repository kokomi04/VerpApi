using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

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
        public int ModuleId { get; set; }
        public long BackupPoint { get; set; }
        public string Title { get; set; }
        public long FileId { get; set; }
        public long BackupDate { get; set; }
        public long RestoreDate { get; set; }
    }
}
