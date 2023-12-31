﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/inventory")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IInventoryBillInputService _inventoryBillInputService;
        private readonly IInventoryBillOutputService _inventoryBillOutputService;
        private readonly IFileService _fileService;

        public InventoryController(
            IInventoryService iventoryService,
            IInventoryBillInputService inventoryBillInputService,
            IInventoryBillOutputService inventoryBillOutputService,
            IFileService fileService
            )
        {
            _inventoryService = iventoryService;
            _fileService = fileService;
            _inventoryBillInputService = inventoryBillInputService;
            _inventoryBillOutputService = inventoryBillOutputService;
        }


        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Từ khóa</param>
        /// <param name="customerId">ID khách hàng</param>
        /// <param name="productIds">Danh sách ID mặt hàng</param>        
        /// <param name="stockId">ID kho</param>
        /// <param name="inventoryStatusId">Trạng thái</param>
        /// <param name="type">Loại (1: Nhập kho, 2: Xuất kho)</param>
        /// <param name="beginTime">Từ ngày</param>
        /// <param name="endTime">Đến ngày</param>
        /// <param name="isInputBillCreated">Đã tạo CTGS hay chưa</param>        
        /// <param name="sortBy">Cột sắp xếp</param>
        /// <param name="asc">Tăng dần hay giảm dần</param>
        /// <param name="page">Trang</param>
        /// <param name="size">Số bản ghi/trang</param>
        /// <param name="inventoryActionId">Hành động</param>
        /// <param name="filters">filters</param>
        /// <returns></returns>
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<InventoryListOutput>> Get([FromQuery] string keyword,
            [FromQuery] int? customerId,
            [FromQuery] IList<int> productIds,
            [FromQuery] int stockId,
            [FromQuery] int? inventoryStatusId,
            [FromQuery] EnumInventoryType? type,
            [FromQuery] long? beginTime,
            [FromQuery] long? endTime,
            [FromQuery] bool? isInputBillCreated,
            [FromQuery] string sortBy,
            [FromQuery] bool asc,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] int? inventoryActionId,
            [FromBody] Clause filters = null)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "date";

            return await _inventoryService.GetList(keyword, customerId, productIds, stockId, inventoryStatusId, type, beginTime, endTime, isInputBillCreated, sortBy, asc, page, size, inventoryActionId, filters).ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy thông tin phiếu nhập / xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu</param>
        /// <returns>InventoryOutput</returns>
        [HttpGet]
        [Route("{inventoryId}")]
        public async Task<InventoryOutput> InventoryInfo([FromRoute] long inventoryId)
        {
            return await _inventoryService.InventoryInfo(inventoryId);
        }

        [HttpGet]
        [Route("{inventoryId}/export")]
        public async Task<FileStreamResult> InventoryInfoExport([FromRoute] long inventoryId)
        {
            var (stream, fileName, contentType) = await _inventoryService.InventoryInfoExport(inventoryId);

            return new FileStreamResult(stream, contentType) { FileDownloadName = fileName };

        }

        /// <summary>
        /// Thêm mới phiếu nhập kho
        /// </summary>
        /// <param name="req">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInventoryInput")]
        public async Task<long> AddInventoryInput([FromBody] InventoryInModel req)
        {
            return await _inventoryBillInputService.AddInventoryInput(req);

        }

        /// <summary>
        /// Thêm mới phiếu xuất kho
        /// </summary>
        /// <param name="req">Model InventoryOutput</param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInventoryOutput")]
        public async Task<long> AddInventoryOutput([FromBody] InventoryOutModel req)
        {
            return await _inventoryBillOutputService.AddInventoryOutput(req);

        }


        /// <summary>
        /// Cập nhật phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="req">Model InventoryInput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateInventoryInput/{inventoryId}")]
        public async Task<bool> UpdateInventoryInput([FromRoute] long inventoryId, [FromBody] InventoryInModel req)
        {
            return await _inventoryBillInputService.UpdateInventoryInput(inventoryId, req);
        }


        /// <summary>
        /// Cập nhật phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <param name="req">Model InventoryOutput</param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateInventoryOutput/{inventoryId}")]
        public async Task<bool> UpdateInventoryOutput([FromRoute] long inventoryId, [FromBody] InventoryOutModel req)
        {
            return await _inventoryBillOutputService.UpdateInventoryOutput(inventoryId, req);
        }

        /// <summary>
        /// Xóa phiếu nhập/xuất kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="type">EnumInventoryType: 1-phiếu nhập kho ; 2-phiếu xuất kho</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{inventoryId}")]
        public async Task<bool> Delete([FromRoute] long inventoryId, [FromQuery] EnumInventoryType type)
        {
            switch (type)
            {
                case EnumInventoryType.Input:
                    return await _inventoryBillInputService.DeleteInventoryInput(inventoryId);

                case EnumInventoryType.Output:
                    return await _inventoryBillOutputService.DeleteInventoryOutput(inventoryId);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams);
            }
        }

        /// <summary>
        /// Duyệt phiếu nhập kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <returns></returns>
        [HttpPut]
        [Route("ApproveInventoryInput/{inventoryId}")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> ApproveInventoryInput([FromRoute] long inventoryId)
        {
            return await _inventoryBillInputService.ApproveInventoryInput(inventoryId);
        }


        /// <summary>
        /// Duyệt phiếu xuất kho
        /// </summary>
        /// <param name="inventoryId">Id phiếu nhập/xuất kho</param>
        /// <returns></returns>
        [HttpPut]
        [Route("ApproveInventoryOutput/{inventoryId}")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> ApproveInventoryOutput([FromRoute] long inventoryId)
        {
            return await _inventoryBillOutputService.ApproveInventoryOutput(inventoryId);
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("File/{fileTypeId}")]
        public async Task<long> UploadImage([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.InventoryInput, fileTypeId, string.Empty, file);
        }


        /*

        /// <summary>
        /// Lấy danh sách kiện để xuất kho
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPackageListForExport")]
        public async Task<PageData<PackageOutputModel>> GetPackageListForExport([FromQuery] int productId, [FromQuery] IList<int> productCateIds, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryBillOutputService.GetPackageListForExport(productId: productId, productCateIds,  stockIdList: stockIdList, page: page, size: size);
        }

        */

        /*

        /// <summary>
        /// Xử lý file - Đọc và tạo chứng từ tồn đầu -> tạo phiếu nhập / xuất
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcessOpeningBalance")]
        public async Task<bool> ProcessOpeningBalance([FromBody] InventoryOpeningBalanceModel model)
        {
            if (model == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            var currentUserId = UserId;
            if (model.InventoryTypeId == EnumInventoryType.Input)
                return await _fileProcessDataService.ImportInventoryInputOpeningBalance(currentUserId, model);
            else if (model.InventoryTypeId == EnumInventoryType.Output)
                return await _fileProcessDataService.ImportInventoryOutput(currentUserId, model);
            else
                throw new BadRequestException(GeneralCode.InvalidParams);

        }*/

        [VErpAction(EnumActionType.View)]
        [HttpPost]
        [Route("{inventoryId}/InputGetAffectedPackages")]
        public async Task<IList<CensoredInventoryInputProducts>> InputGetAffectedPackages([FromRoute] int inventoryId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] InventoryInModel req)
        {
            return await _inventoryBillInputService.InputUpdateGetAffectedPackages(inventoryId, fromDate, toDate, req);
        }

        [HttpPut]
        [Route("{inventoryId}/ApprovedInputDataUpdate")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> ApprovedInputDataUpdate([FromRoute] long inventoryId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] ApprovedInputDataSubmitModel req)
        {
            return await _inventoryBillInputService.ApprovedInputDataUpdate(inventoryId, fromDate, toDate, req);
        }

        //[HttpGet]
        //[Route("fieldDataForMapping")]
        //public CategoryNameModel GetInventoryDetailFieldDataForMapping()
        //{
        //    return _inventoryService.GetInventoryDetailFieldDataForMapping();
        //}

        [HttpGet]
        [Route("inputFieldsForMapping")]
        public async Task<CategoryNameModel> InputFieldsForMapping()
        {
            return await _inventoryService.InputFieldsForMapping();
        }


        [HttpPost]
        [Route("inputImportFromMapping")]
        public async Task<bool> InputImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (mapping == null) throw GeneralCode.InvalidParams.BadRequest();
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return await _inventoryService.InventoryInputImport(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("outputFieldsForMapping")]
        public CategoryNameModel OutputFieldsForMapping()
        {
            return _inventoryService.OutputFieldsForMapping();
        }


        [HttpPost]
        [Route("outImportFromMapping")]
        public async Task<bool> OutImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (mapping == null) throw GeneralCode.InvalidParams.BadRequest();
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return await _inventoryService.InventoryOutImport(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("InputFieldsForParse")]
        public async Task<CategoryNameModel> InputFieldsForParse()
        {
            return await _inventoryService.InputFieldsForParse();
        }

        [HttpPost]
        [Route("InputParseExcel")]
        public IAsyncEnumerable<InvInputDetailRowValue> InputParseExcel([FromFormString] ImportExcelMapping mapping, IFormFile file, [FromQuery] int stockId)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return _inventoryService.InputParseExcel(mapping, file.OpenReadStream(), stockId);
        }


        [HttpGet]
        [Route("OutFieldsForParse")]
        public CategoryNameModel OutFieldsForParse()
        {
            return _inventoryService.OutFieldsForParse();
        }

        [HttpPost]
        [Route("OutParseExcel")]
        public IAsyncEnumerable<InvOutDetailRowValue> OutParseExcel([FromFormString] ImportExcelMapping mapping, IFormFile file, [FromQuery] int stockId)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return _inventoryService.OutParseExcel(mapping, file.OpenReadStream(), stockId);
        }


        /// <summary>
        /// Gửi email thông báo duyệt xuất/nhập kho
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="mailCode"></param>
        /// <param name="mailTo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{inventoryId}/notify/sendMail")]
        public async Task<bool> SendMailNotifyCheckAndCensor([FromRoute] long inventoryId, [FromQuery] string mailCode, [FromBody] string[] mailTo)
        {
            return await _inventoryService.SendMailNotifyCensor(inventoryId, mailCode, mailTo).ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{inventoryId}/sentToCensor")]
        public async Task<bool> SentToCensor([FromRoute] long inventoryId, [FromQuery] EnumInventoryType type)
        {
            switch (type)
            {
                case EnumInventoryType.Input:
                    return await _inventoryBillInputService.SentToCensor(inventoryId);

                case EnumInventoryType.Output:
                    return await _inventoryBillOutputService.SentToCensor(inventoryId);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams);
            }
        }

        [HttpPut]
        [Route("{inventoryId}/reject")]
        public async Task<bool> RejectCensored([FromRoute] long inventoryId, [FromQuery] EnumInventoryType type)
        {
            switch (type)
            {
                case EnumInventoryType.Input:
                    return await _inventoryBillInputService.Reject(inventoryId);

                case EnumInventoryType.Output:
                    return await _inventoryBillOutputService.Reject(inventoryId);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams);
            }
        }

    }
}
