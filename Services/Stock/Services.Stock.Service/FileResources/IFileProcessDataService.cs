﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using EFCore.BulkExtensions;
using VErp.Services.Stock.Model.Inventory;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErp.Services.Stock.Service.FileResources
{
    public interface IFileProcessDataService
    {
        /// <summary>
        /// Nhập dữ liệu khách hàng đối tác
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        Task<Enum> ImportCustomerData(int currentUserId, long fileId);

        /// <summary>
        /// Nhập dữ liệu tồn kho (nhập kho) đầu kỳ
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ServiceResult> ImportInventoryInputOpeningBalance(int currentUserId, InventoryOpeningBalanceModel model);

        /// <summary>
        /// Nhập dữ liệu xuất kho đầu kỳ
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ServiceResult> ImportInventoryOutput(int currentUserId, InventoryOpeningBalanceModel model);
    }
}
