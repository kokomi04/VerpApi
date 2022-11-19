using System;
using System.Collections.Generic;
using System.Text;
using Verp.Cache.RedisCache;
using VErp.Commons.Library;

namespace VErp.Infrastructure.ServiceCore.Extensions
{
    public static class ExcelReaderExtensions
    {
        public static void RegisterLongTaskEvent(this ExcelReader reader, LongTaskResourceLock longTask)
        {
            reader.OnBeginReadingExcelRow += (totalRows) =>
            {
                longTask.SetCurrentStep("Đọc tệp excel");
                longTask.SetTotalRows(totalRows);
            };

            reader.OnReadingExcelRow += (int readRows) =>
            {
                longTask.IncProcessedRows();
            };


            reader.OnBeginParseExcelDataToEntity += (totalRows) =>
            {
                longTask.SetCurrentStep("Chuyển dữ liệu từ excel thành đối tượng xử lý");
                longTask.SetTotalRows(totalRows);
            };

            reader.OnParseExcelDataToEntity += (int readRows, object obj) =>
            {
                longTask.IncProcessedRows();
            };
        }
    }
}
