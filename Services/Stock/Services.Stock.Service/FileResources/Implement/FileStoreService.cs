﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Stock.Service.FileResources.Implement
{
    public class FileStoreService : IFileStoreService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly string _rootFolder = "";

        private readonly IDataProtectionProvider _dataProtectionProvider;


        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>()
        {
            { ".doc" , "application/msword" },
            { ".pdf" , "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".jpg", "image/jpg" },
            { ".jpeg", "image/jpg" },
            { ".bmp", "image/bmp" },
            { ".png", "image/png" },
        };

        public FileStoreService(
            IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IDataProtectionProvider dataProtectionProvider
            , IAsyncRunnerService asyncRunnerService
        )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _rootFolder = _appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\');
            _dataProtectionProvider = dataProtectionProvider;
            _asyncRunnerService = asyncRunnerService;
        }

        public async Task<(Stream file, string contentType)> GetFileStream(string fileKey)
        {
            await Task.CompletedTask;
            var rawString = fileKey.DecryptFileKey(_dataProtectionProvider, _appSetting);
            var data = rawString.Split('|');

            // var fileId = data[0];
            var relativeFilePath = data[1];
            var contentType = data[2];
            var timeUnix = data[3];

            if (long.Parse(timeUnix) < DateTime.UtcNow.AddDays(-1).GetUnix())
            {
                throw new BadRequestException(FileErrorCode.FileUrlExpired);
            }

            var filePath = relativeFilePath.GetPhysicalFilePath(_appSetting);
            try
            {
                return (File.OpenRead(filePath), contentType);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                throw;
            }
        }


        public async Task<(Stream file, string contentType)> GetTemplateStream(string fileKey)
        {
            await Task.CompletedTask;
            var rawString = fileKey.DecryptFileKey(_dataProtectionProvider, _appSetting);
            var data = rawString.Split('|');

            // var fileId = data[0];
            var relativeFilePath = data[0];
            var timeUnix = data[1];

            if (long.Parse(timeUnix) < DateTime.UtcNow.AddDays(-1).GetUnix())
            {
                throw new BadRequestException(FileErrorCode.FileUrlExpired);
            }

            var filePath = relativeFilePath.GetPhysicalFilePath(_appSetting);
            try
            {
                var ext = Path.GetExtension(filePath)?.ToLower();
                var contentType = "";
                if (ContentTypes.ContainsKey(ext))
                {
                    contentType = ContentTypes[ext];
                }
                else
                {
                    contentType = "application/octet-stream";
                }
                return (File.OpenRead(filePath), contentType);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetTemplateStream(string fileKey={fileKey})");
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetTemplateStream(string fileKey={fileKey})");
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"GetTemplateStream(string fileKey={fileKey})");
                throw;
            }
        }

    }
}
