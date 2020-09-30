using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Master.Model.StorageDatabase;

namespace VErp.Services.Master.Service.StorageDatabase
{
    public interface IStorageDatabaseService
    {
        Task<IList<StorageDatabseModel>> GetList();
        Task<IList<BackupStorageOutput>> GetBackupStorages(int moduleId = 0);
        Task<bool> BackupStorage(BackupStorageInput backupStorage);
        Task<bool> RestoreForBackupPoint(long backupPoint);
        Task<bool> RestoreForBackupPoint(long backupPoint, int moduleId);
        
    }
}
