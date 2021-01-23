using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Inventory.OpeningBalance;
using VErp.Services.Stock.Model.Package;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Inventory
{
    [Route("api/inventory")]
    public class InventoryController : VErpBaseController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IFileService _fileService;
        private readonly IFileProcessDataService _fileProcessDataService;

        public InventoryController(IInventoryService iventoryService, IFileService fileService, IFileProcessDataService fileProcessDataService)
        {
            _inventoryService = iventoryService;
            _fileService = fileService;
            _fileProcessDataService = fileProcessDataService;
        }

        /// <summary>
        /// Lấy danh sách phiếu nhập / xuất kho
        /// </summary>
        /// <param name="keyword">Tìm kiếm trong Mã phiếu, mã SP, tên SP, tên người gủi/nhận, tên Obj liên quan RefObjectCode</param>
        /// <param name="stockId">Id kho</param>
        /// <param name="isApproved"></param>
        /// <param name="type">Loại InventoryTypeId: 1 nhập ; 2 : xuất kho theo MasterEnum.EnumInventory</param>        
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="isExistedInputBill"></param>
        /// <param name="mappingFunctionKeys"></param>
        /// <param name="sortBy">sort by column (default: date) </param>
        /// <param name="asc">true/false (default: false. It mean sort desc)</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<InventoryOutput>> Get([FromQuery] string keyword, [FromQuery] int? customerId, [FromQuery] string accountancyAccountNumber, [FromQuery] int stockId, [FromQuery] bool? isApproved, [FromQuery] EnumInventoryType? type, [FromQuery] long? beginTime, [FromQuery] long? endTime, [FromQuery] bool? isExistedInputBill, [FromQuery] IList<string> mappingFunctionKeys, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = "date";

            return await _inventoryService.GetList(keyword, customerId, accountancyAccountNumber, stockId, isApproved, type, beginTime, endTime, isExistedInputBill, mappingFunctionKeys, sortBy, asc, page, size).ConfigureAwait(true);
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
            return await _inventoryService.AddInventoryInput(req);

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
            return await _inventoryService.AddInventoryOutput(req);

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
            return await _inventoryService.UpdateInventoryInput(inventoryId, req);
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
            return await _inventoryService.UpdateInventoryOutput(inventoryId, req);
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
                    return await _inventoryService.DeleteInventoryInput(inventoryId);

                case EnumInventoryType.Output:
                    return await _inventoryService.DeleteInventoryOutput(inventoryId);
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
            return await _inventoryService.ApproveInventoryInput(inventoryId);
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
            return await _inventoryService.ApproveInventoryOutput(inventoryId);
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

        /// <summary>
        /// Lấy danh sách sản phẩm để nhập kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductListForImport")]
        public async Task<PageData<ProductListOutput>> GetProductListForImport([FromQuery] string keyword, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetProductListForImport(keyword: keyword, stockIdList: stockIdList, page: page, size: size);
        }


        /// <summary>
        /// Lấy danh sách sản phẩm để xuất kho
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="stockIdList"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductListForExport")]
        public async Task<PageData<ProductListOutput>> GetProductListForExport([FromQuery] string keyword, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetProductListForExport(keyword: keyword, stockIdList: stockIdList, page: page, size: size);
        }

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
        public async Task<PageData<PackageOutputModel>> GetPackageListForExport([FromQuery] int productId, [FromQuery] IList<int> stockIdList, [FromQuery] int page, [FromQuery] int size)
        {
            return await _inventoryService.GetPackageListForExport(productId: productId, stockIdList: stockIdList, page: page, size: size);
        }



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
            if (model.Type == EnumInventoryType.Input)
                return await _fileProcessDataService.ImportInventoryInputOpeningBalance(currentUserId, model);
            else if (model.Type == EnumInventoryType.Output)
                return await _fileProcessDataService.ImportInventoryOutput(currentUserId, model);
            else
                throw new BadRequestException(GeneralCode.InvalidParams);

        }

        [VErpAction(EnumActionType.View)]
        [HttpPost]
        [Route("{inventoryId}/InputGetAffectedPackages")]
        public async Task<IList<CensoredInventoryInputProducts>> InputGetAffectedPackages([FromRoute] int inventoryId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] InventoryInModel req)
        {
            return await _inventoryService.InputUpdateGetAffectedPackages(inventoryId, fromDate, toDate, req);
        }

        [HttpPut]
        [Route("{inventoryId}/ApprovedInputDataUpdate")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> ApprovedInputDataUpdate([FromRoute] long inventoryId, [FromQuery] long fromDate, [FromQuery] long toDate, [FromBody] ApprovedInputDataSubmitModel req)
        {
            return await _inventoryService.ApprovedInputDataUpdate(inventoryId, fromDate, toDate, req);
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetInventoryDetailFieldDataForMapping()
        {
            return _inventoryService.GetInventoryDetailFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<long> ImportFromMapping([FromForm] string mapping, [FromForm] string info, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _inventoryService.InventoryImport(JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream(), JsonConvert.DeserializeObject<InventoryOpeningBalanceModel>(info)).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{inventoryTypeId}/FieldsForParse")]
        public CategoryNameModel FieldsForParse([FromRoute] EnumInventoryType inventoryTypeId)
        {
            return _inventoryService.FieldsForParse(inventoryTypeId);
        }

        [HttpPost]
        [Route("{inventoryTypeId}/ParseExcel")]
        public IAsyncEnumerable<InventoryDetailRowValue> InputExcelParse([FromRoute] EnumInventoryType inventoryTypeId, [FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return _inventoryService.ParseExcel(JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream(), inventoryTypeId);
        }

    }
}
