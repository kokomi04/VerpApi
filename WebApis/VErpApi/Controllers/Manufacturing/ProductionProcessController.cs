﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionProcessController : VErpBaseController
    {
        private readonly IProductionProcessService _productionProcessService;

        public ProductionProcessController(IProductionProcessService productionProcessService)
        {
            _productionProcessService = productionProcessService;
        }

        [HttpGet]
        [Route("{containerTypeId}/{containerId}")]
        public async Task<ProductionProcessModel> GetProductionProcessByContainerId([FromRoute] EnumContainerType containerTypeId, [FromRoute] int containerId)
        {
            return await _productionProcessService.GetProductionProcessByContainerId(containerTypeId, containerId);
        }

        [HttpGet]
        [Route("ProductionOrder/{productionOrderId}")]
        public async Task<ProductionProcessInfo> GetProductionProcessByScheduleTurn([FromRoute] long productionOrderId)
        {
            return await _productionProcessService.GetProductionProcessByProductionOrder(productionOrderId);
        }

        [HttpGet]
        [Route("productionStep/{productionStepId}")]
        public async Task<ProductionStepModel> GetProductionStepById([FromRoute] long productionStepId)
        {
            return await _productionProcessService.GetProductionStepById(productionStepId);
        }

        [HttpPut]
        [Route("productionStep/{productionStepId}")]
        public async Task<bool> UpdateProductionStepsById([FromRoute] long productionStepId, [FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.UpdateProductionStepById(productionStepId, req);
        }

        [HttpPost]
        [Route("productionStep")]
        public async Task<long> CreateProductionStep([FromBody] ProductionStepInfo req)
        {
            return await _productionProcessService.CreateProductionStep(req);
        }

        [HttpGet]
        [Route("{containerTypeId}/{containerId}/productionStep")]
        public async Task<IList<InternalProductionStepSimpleModel>> GetAllProductionStep([FromRoute] EnumContainerType containerTypeId, [FromRoute] int containerId)
        {
            return await _productionProcessService.GetAllProductionStep(containerTypeId, containerId);
        }

        [HttpPost]
        [Route("productionStepGroup")]
        public async Task<long> CreateProductionStepGroup([FromBody] ProductionStepGroupModel req)
        {
            return await _productionProcessService.CreateProductionStepGroup(req);
        }

        [HttpDelete]
        [Route("productionStep/{productionStepId}")]
        public async Task<bool> DeleteProductionStepById([FromRoute] int productionStepId)
        {
            return await _productionProcessService.DeleteProductionStepById(productionStepId);
        }

        [HttpPost]
        [Route("productionOrder/{productionOrderId}")]
        public async Task<bool> CreateProductionProcess([FromRoute] int productionOrderId)
        {
            return await _productionProcessService.IncludeProductionProcess(productionOrderId);
        }

        //[HttpPut]
        //[Route("productionOrder/{productionOrderId}/process")]
        //public async Task<bool> MergeProductionProcess([FromRoute] int productionOrderId, [FromBody] IList<long> productionStepIds)
        //{
        //    return await _productionProcessService.MergeProductionProcess(productionOrderId, productionStepIds);
        //}

        [HttpPut]
        [Route("productionOrder/{productionOrderId}/step")]
        public async Task<bool> MergeProductionStep([FromRoute] int productionOrderId, [FromBody] IList<long> productionStepIds)
        {
            return await _productionProcessService.MergeProductionStep(productionOrderId, productionStepIds);
        }

        [HttpPost]
        [Route("productionStepRoleClient")]
        public async Task<bool> InsertAndUpdateStepClientData([FromBody] ProductionStepRoleClientModel model)
        {
            return await _productionProcessService.InsertAndUpdatePorductionStepRoleClient(model);
        }

        [HttpGet]
        [Route("productionStepRoleClient/{containerTypeId}/{containerId}")]
        public async Task<string> GetStepClientData([FromRoute] int containerTypeId, [FromRoute] long containerId)
        {
            return await _productionProcessService.GetPorductionStepRoleClient(containerTypeId, containerId);
        }

        [HttpPut]
        [Route("productionStep/updateSortOrder")]
        public async Task<bool> UpdateProductionStepSortOrder([FromBody] IList<ProductionStepSortOrderModel> req)
        {
            return await _productionProcessService.UpdateProductionStepSortOrder(req);
        }

        [HttpPut]
        [Route("{containerTypeId}/{containerId}")]
        public async Task<bool> UpdateProductionProcess([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId, [FromBody] ProductionProcessModel req)
        {
            var rs = await _productionProcessService.UpdateProductionProcess(containerTypeId, containerId, req);
            //if (containerTypeId == EnumContainerType.ProductionOrder)
            //{
            //    await _productionProcessService.UpdateMarkInvalidOutsourcePartRequest(containerId);
            //    await _productionProcessService.UpdateMarkInvalidOutsourceStepRequest(containerId);
            //}
            return rs;
        }

        [HttpPut]
        [Route("{productionOrderId}/dismissUpdateQuantity")]
        public async Task<bool> DismissUpdateQuantity([FromRoute] long productionOrderId)
        {
            var rs = await _productionProcessService.DismissUpdateQuantity(productionOrderId);
            return rs;
        }

        [HttpGet]
        [Route("{productionOrderId}/hasAssignment")]
        public async Task<bool> CheckHasAssignment([FromRoute] long productionOrderId)
        {
            var rs = await _productionProcessService.CheckHasAssignment(productionOrderId);
            return rs;
        }

        [HttpPost]
        [Route("productionStepLinkData/searchByListProductionStepLinkDataId")]
        public async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId([FromBody] List<long> lsProductionStepLinkDataId)
        {
            return await _productionProcessService.GetProductionStepLinkDataByListId(lsProductionStepLinkDataId);
        }

        /// <summary>
        /// Lấy danh sách InOut của 1 nhóm các công đoạn
        /// </summary>
        /// <param name="lsProductionStepId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("productionStepLinkDataRole/getInOutOfListProductionStep")]
        public async Task<IList<ProductionStepLinkDataRoleModel>> GetListStepLinkDataForOutsourceStep(List<long> lsProductionStepId)
        {
            return await _productionProcessService.GetListStepLinkDataForOutsourceStep(lsProductionStepId);
        }

        /// <summary>
        /// Gom nhóm các công đoạn có mối qua hệ với nhau
        /// </summary>
        /// <param name="containerTypeId"></param>
        /// <param name="containerId"></param>
        /// <param name="productionOrderId">Mã lệnh sản xuất</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{containerTypeId}/{containerId}/groupProductionStepToOutsource")]
        public async Task<IList<GroupProductionStepToOutsource>> GroupProductionStepToOutsource([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId, [FromBody] long[] productionOrderId)
        {
            return await _productionProcessService.GroupProductionStepToOutsource(containerTypeId, containerId, productionOrderId);
        }
        /// <summary>
        /// Sét khối lượng công việc cho công đoạn
        /// </summary>
        /// <param name="productionStepWorkload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("productionStep/workload")]
        public async Task<bool> SetProductionStepWorkload([FromBody] IList<ProductionStepWorkload> productionStepWorkload)
        {
            return await _productionProcessService.SetProductionStepWorkload(productionStepWorkload);
        }

        [HttpPost]
        [Route("copy")]
        public async Task<bool> CopyProductionProcess(EnumContainerType containerTypeId, long fromContainerId, long toContainerId)
        {
            return await _productionProcessService.CopyProductionProcess(containerTypeId, fromContainerId, toContainerId);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{containerTypeId}/{containerId}/productionProcessOutsourceStep")]
        public async Task<ProductionProcessOutsourceStep> GetProductionProcessOutsourceStep([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId, [FromBody] long[] productionStepIds)
        {
            return await _productionProcessService.GetProductionProcessOutsourceStep(containerTypeId, containerId, productionStepIds);
        }

        /// <summary>
        /// Trả về các nhóm đầu vào, đầu ra gia công
        /// </summary>
        /// <param name="containerTypeId"></param>
        /// <param name="containerId"></param>
        /// <param name="productionOrderId">Mã lệnh sản xuất</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{containerTypeId}/{containerId}/groupProductionStepInOutToOutsource")]
        public async Task<IList<GroupProductionStepToOutsource>> GroupProductionStepInOutToOutsource([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId, [FromBody] long[] productionOrderId)
        {
            return await _productionProcessService.GroupProductionStepInOutToOutsource(containerTypeId, containerId, productionOrderId);
        }

        /// <summary>
        /// Trả về tất cả các product có trong quy trình kèm số lượng định mức
        /// </summary>
        /// <param name="containerTypeId"></param>
        /// <param name="containerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{containerTypeId}/{containerId}/getAllProductInProductionProcess")]
        public async Task<IList<ProductionStepLinkDataObjectModel>> GetAllProductInProductionProcess([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId)
        {
            return await _productionProcessService.GetAllProductInProductionProcessV2(containerTypeId, containerId);
        }
    }
}
