using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.StorageDatabase
{
    public class BackupStorageInput
    {
        public string Title { get; set; }
        public List<StorageDatabseModel> storages { get; set; }
    }
}
