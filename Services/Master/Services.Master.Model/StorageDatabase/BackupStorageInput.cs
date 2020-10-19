using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Master.Model.StorageDatabase
{
    public class BackupStorageInput
    {
        public string Title { get; set; }
        public List<ProductModuleInfo> storages { get; set; }
    }
}
