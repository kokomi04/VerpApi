﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductController : CrossServiceBaseController
    {
        private readonly IProductService _productService;
        public InternalProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<ProductListOutput>>> Search([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int[] productTypeIds = null, [FromQuery] int[] productCateIds = null)
        {
            return await _productService.GetList(keyword, productTypeIds, productCateIds, page, size);
        }


        [HttpGet]
        [Route("{productId}")]
        public async Task<ServiceResult<ProductModel>> GetProduct([FromRoute] int productId)
        {
            return await _productService.ProductInfo(productId);
        }
    }
}