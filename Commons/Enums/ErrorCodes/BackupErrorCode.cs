using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.Enums.ErrorCodes
{
    public enum BackupErrorCode
    {
        NotFoundFileAfterBackup = 1,
        NotFoundBackupPoint = 2,
        NotFoundBackupForDatabase = 3
    }
}
