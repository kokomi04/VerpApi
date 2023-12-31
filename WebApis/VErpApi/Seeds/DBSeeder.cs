﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Service.Input.Implement;
using VErp.Services.Manafacturing.Service.ProductionProcess;

namespace VErpApi.Seeds
{
    public class DBSeeder
    {
        public static void Seed(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var productionProcessDBSeedService = services.GetRequiredService<IProductionProcessDBSeedService>();
                productionProcessDBSeedService.CreateStockProductionStep().Wait();

            }
        }

        public static void NormalizeData(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var inputPublicConfigService = services.GetRequiredService<IInputPublicConfigSeedService>();
                inputPublicConfigService.ReplacePublicRefTableCode().Wait();

            }
        }
        
    }
}
