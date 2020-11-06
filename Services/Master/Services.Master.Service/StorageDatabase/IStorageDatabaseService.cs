using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Services.Master.Model.StorageDatabase;

namespace VErp.Services.Master.Service.StorageDatabase
{
    public interface IStorageDatabaseService
    {
        Task<IList<BackupStorageOutput>> GetBackupStorages(int? moduleTypeId = null);
        Task<bool> BackupStorage(BackupStorageInput backupStorage);
        Task<bool> RestoreForBackupPoint(long backupPoint);
        Task<bool> RestoreForBackupPoint(long backupPoint, int moduleTypeId);
        
    }
}
