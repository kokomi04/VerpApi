using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using System.Linq;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config.Implement
{
    public class BarcodeConfigService : IBarcodeConfigService
    {

        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public BarcodeConfigService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<BarcodeConfigService> logger
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }
        public async Task<int> AddBarcodeConfig(BarcodeConfigModel data)
        {
            var (status, config) = ExtractBarcodeConfig(data);

            if (!status.IsSuccess())
            {
                throw new BadRequestException(status);
            }

            //if (data.IsActived)
            //{
            //    var activedModel = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeStandardId == (int)data.BarcodeStandardId && c.IsActived);
            //    if (activedModel != null)
            //    {
            //        return BarcodeConfigErrorCode.OnlyAllowOneBarcodeConfigActivedAtTheSameTime;
            //    }
            //}
            var model = new BarcodeConfig()
            {
                Name = data.Name.Trim(),
                BarcodeStandardId = (int)data.BarcodeStandardId,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsActived = data.IsActived,
                IsDeleted = false,
                ConfigurationJson = config
            };

            await _masterContext.BarcodeConfig.AddAsync(model);

            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.BarcodeConfig, model.BarcodeConfigId, $"Thêm mới cấu hình barcode {model.Name}", data.JsonSerialize());

            return model.BarcodeConfigId;
        }

        public async Task<bool> DeleteBarcodeConfig(int barcodeConfigId)
        {
            var model = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeConfigId == barcodeConfigId);
            if (model == null)
            {
                throw new BadRequestException(BarcodeConfigErrorCode.BarcodeNotFound);
            }


            model.IsDeleted = true;
            model.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.BarcodeConfig, model.BarcodeConfigId, $"Xóa cấu hình barcode {model.Name}", model.JsonSerialize());
            return true;
        }

        public async Task<string> Make(int barcodeConfigId)
        {


            var activedConfig = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeConfigId == barcodeConfigId);
            if (activedConfig == null)
            {
                throw new BadRequestException(BarcodeConfigErrorCode.BarcodeNotFound);
            }

            if (!activedConfig.IsActived)
            {
                throw new BadRequestException(BarcodeConfigErrorCode.BarcodeConfigHasBeenDisabled);
            }

            var model = ExtractBarcodeModel(activedConfig);

            var barcode = string.Empty;

            switch (model.BarcodeStandardId)
            {
                case EnumBarcodeStandard.EAN_13:
                    var g = new BarcodeGenerate()
                    {
                        GeneratedDatetimeUtc = DateTime.UtcNow,
                        IsUsed = true
                    };
                    var config = _masterContext.BarcodeGenerate.Add(g);

                    await _masterContext.SaveChangesAsync();

                    var ean = model.Ean13;
                    barcode = $"{ean.CountryCode}{ean.CompanyCode}";
                    var n = 12 - barcode.Length;
                    var pCode = g.BarcodeGenerateId.ToString($"D{n}");
                    if (pCode.Length > n)
                        pCode = pCode.Substring(pCode.Length - n);
                    barcode = $"{ean.CountryCode}{ean.CompanyCode}{pCode}";
                    var total = 0;
                    for (var i = barcode.Length - 1; i >= 0; i--)
                    {
                        total += Convert.ToInt32(barcode[i]) * (i % 2 == 0 ? 3 : 1);
                    }
                    var num = total % 10;
                    return $"{barcode}{(num == 0 ? 0 : 10 - num)}";
                default:
                    throw new BadRequestException(BarcodeConfigErrorCode.BarcodeStandardNotSupportedYet);
            }
        }


        public async Task<BarcodeConfigModel> GetInfo(int barcodeConfigId)
        {
            var model = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeConfigId == barcodeConfigId);
            if (model == null)
            {
                throw new BadRequestException(BarcodeConfigErrorCode.BarcodeNotFound);
            }

            return ExtractBarcodeModel(model);
        }

        public async Task<PageData<BarcodeConfigListOutput>> GetList(string keyword, int page, int size)
        {
            var query = (from c in _masterContext.BarcodeConfig select c);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from c in query
                        where c.Name.Contains(keyword)
                        select c;
            }

            var lst = (
                from c in query
                select new BarcodeConfigListOutput()
                {
                    BarcodeConfigId = c.BarcodeConfigId,
                    BarcodeStandardId = (EnumBarcodeStandard)c.BarcodeStandardId,
                    IsActived = c.IsActived,
                    Name = c.Name
                }
                );
            var total = await lst.CountAsync();
            var pageData = size > 0 ? lst.OrderBy(c => c.Name).Skip((page - 1) * size).Take(size) : lst.OrderBy(c => c.Name);

            return (await pageData.ToListAsync(), total);
        }

        public async Task<IList<BarcodeConfigListOutput>> GetListActived()
        {
            return await (
                from c in _masterContext.BarcodeConfig
                where c.IsActived
                select new BarcodeConfigListOutput()
                {
                    BarcodeConfigId = c.BarcodeConfigId,
                    BarcodeStandardId = (EnumBarcodeStandard)c.BarcodeStandardId,
                    IsActived = c.IsActived,
                    Name = c.Name
                }
                ).ToListAsync();
        }
        public async Task<bool> UpdateBarcodeConfig(int barcodeConfigId, BarcodeConfigModel data)
        {
            var (status, config) = ExtractBarcodeConfig(data);

            if (!status.IsSuccess())
            {
                throw new BadRequestException(status);
            }

            var model = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeConfigId == barcodeConfigId);
            if (model == null)
            {
                throw new BadRequestException(BarcodeConfigErrorCode.BarcodeNotFound);

            }

            //if (data.IsActived)
            //{
            //    var activedModel = await _masterContext.BarcodeConfig.FirstOrDefaultAsync(c => c.BarcodeStandardId == (int)data.BarcodeStandardId && c.IsActived && c.BarcodeConfigId != barcodeConfigId);
            //    if (activedModel != null)
            //    {
            //        return BarcodeConfigErrorCode.OnlyAllowOneBarcodeConfigActivedAtTheSameTime;
            //    }
            //}


            model.Name = data.Name;
            model.IsActived = data.IsActived;
            model.ConfigurationJson = config;
            model.UpdatedDatetimeUtc = DateTime.UtcNow;
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.BarcodeConfig, model.BarcodeConfigId, $"Cập nhật cấu hình barcode {data.Name}", model.JsonSerialize());

            return true;
        }


        private (Enum, string) ExtractBarcodeConfig(BarcodeConfigModel data)
        {
            var config = "";
            switch (data.BarcodeStandardId)
            {
                case EnumBarcodeStandard.EAN_8:
                    config = data.Ean8.JsonSerialize();
                    break;

                case EnumBarcodeStandard.EAN_13:
                    config = data.Ean13.JsonSerialize();
                    break;
                default:
                    throw new NotSupportedException($"BarcodeStandard {data.BarcodeStandardId} not supported yet");
            }

            if (string.IsNullOrWhiteSpace(config))
            {
                return (GeneralCode.InvalidParams, null);
            }
            return (GeneralCode.Success, config);
        }

        private BarcodeConfigModel ExtractBarcodeModel(BarcodeConfig model)
        {
            var config = model.ConfigurationJson;
            var barcodeStandardId = (EnumBarcodeStandard)model.BarcodeStandardId;
            var data = new BarcodeConfigModel();

            data.BarcodeStandardId = barcodeStandardId;
            data.IsActived = model.IsActived;
            data.Name = model.Name;

            switch (barcodeStandardId)
            {
                case EnumBarcodeStandard.EAN_8:
                    data.Ean8 = model.ConfigurationJson.JsonDeserialize<BarcodeConfigEan8>();
                    break;

                case EnumBarcodeStandard.EAN_13:
                    data.Ean13 = model.ConfigurationJson.JsonDeserialize<BarcodeConfigEan13>();
                    break;
                default:
                    throw new NotSupportedException($"BarcodeStandard {data.BarcodeStandardId} not supported yet");
            }

            return data;
        }
    }
}
