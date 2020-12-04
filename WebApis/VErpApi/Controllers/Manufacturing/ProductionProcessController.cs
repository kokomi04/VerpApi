using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionProcess;
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
        [Route("ScheduleTurn/{scheduleTurnId}")]
        public async Task<ProductionProcessInfo> GetProductionProcessByScheduleTurn([FromRoute] long scheduleTurnId)
        {
            return await _productionProcessService.GetProductionProcessByScheduleTurn(scheduleTurnId);
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

        [HttpPut]
        [Route("productionOrder/{productionOrderId}/process")]
        public async Task<bool> MergeProductionProcess([FromRoute] int productionOrderId, [FromBody] IList<long> productionStepIds)
        {
            return await _productionProcessService.MergeProductionProcess(productionOrderId, productionStepIds);
        }

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
        public async Task<bool> UpdateProductionStepSortOrder([FromBody] IList<PorductionStepSortOrderModel> req)
        {
            return await _productionProcessService.UpdateProductionStepSortOrder(req);
        }

        [HttpPut]
        [Route("{containerTypeId}/{containerId}")]
        public async Task<bool> UpdateProductionProcess([FromRoute] EnumContainerType containerTypeId, [FromRoute] long containerId, [FromBody] ProductionProcessModel req)
        {
            return await _productionProcessService.UpdateProductionProcess(containerTypeId, containerId, req);
        }

        [HttpPost]
        [Route("productionStepLinkData/searchByListProductionStepLinkDataId")]
        public async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId([FromBody] List<long> lsProductionStepLinkDataId)
        {
            return await _productionProcessService.GetProductionStepLinkDataByListId(lsProductionStepLinkDataId);
        }

        [HttpPost]
        [Route("productionStepLinkDataRole/getInOutOfListProductionStep")]
        public async Task<IList<ProductionStepLinkDataRoleModel>> GetListStepLinkDataForOutsourceStep(List<long> lsProductionStepId)
        {
           return await _productionProcessService.GetListStepLinkDataForOutsourceStep(lsProductionStepId);
        }
    }
}
