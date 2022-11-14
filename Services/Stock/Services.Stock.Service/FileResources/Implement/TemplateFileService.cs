using ImageMagick;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Commons.ObjectExtensions.Extensions;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Model.FileResources;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;
using StockDBContext = VErp.Infrastructure.EF.StockDB.StockDBContext;

namespace VErp.Services.Stock.Service.FileResources.Implement
{
    public class TemplateFileService: ITemplateFileService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunnerService;

        private readonly IDataProtectionProvider _dataProtectionProvider;

        private const string DOCUMENT_TEMPLATE_FOLDER = Utils.DOCUMENT_TEMPLATE_FOLDER;

        private static readonly Dictionary<string, EnumFileType> FileExtensionTypesNotAccepted = new Dictionary<string, EnumFileType>()
        {
            { ".sh", EnumFileType.Other },
            { ".cmd", EnumFileType.Other },
            { ".bash", EnumFileType.Other },
            { ".exe"  , EnumFileType.Other },

            { ".asp" , EnumFileType.Other },
            { ".dll" , EnumFileType.Other },
            { ".ahx", EnumFileType.Other },
            { ".apx", EnumFileType.Other },
            { ".ini", EnumFileType.Other },

            { ".cs" , EnumFileType.Other },
            { ".py" , EnumFileType.Other },
            { ".h", EnumFileType.Other },
            { ".cpp", EnumFileType.Other },
            { ".jar", EnumFileType.Other },
            { ".java", EnumFileType.Other },
            { ".js", EnumFileType.Other },
            { ".ts", EnumFileType.Other },
        };

        private static readonly Dictionary<string, EnumFileType> FileExtensionTypes = new Dictionary<string, EnumFileType>()
        {
            { ".jpg", EnumFileType.Image },
            { ".jpeg", EnumFileType.Image },
            { ".bmp", EnumFileType.Image },
            { ".png"  , EnumFileType.Image },

            { ".doc" , EnumFileType.Document },
            { ".pdf" , EnumFileType.Document },
            { ".docx", EnumFileType.Document },
            { ".xls", EnumFileType.Document },
            { ".xlsx" , EnumFileType.Document },
            { ".csv" , EnumFileType.Document },
        };

        private static readonly Dictionary<string, EnumFileType> ImageFileExtensionTypes = new Dictionary<string, EnumFileType>()
        {
            { ".jpg", EnumFileType.Image },
            { ".jpeg", EnumFileType.Image },
            { ".bmp", EnumFileType.Image },
            { ".png"  , EnumFileType.Image },
        };



        private static readonly Dictionary<EnumFileType, string[]> FileTypeExtensions = FileExtensionTypes.GroupBy(t => t.Value).ToDictionary(t => t.Key, t => t.Select(v => v.Key).ToArray());


        public TemplateFileService(
            IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IActivityLogService activityLogService
            , IDataProtectionProvider dataProtectionProvider
            , IAsyncRunnerService asyncRunnerService
        )
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _dataProtectionProvider = dataProtectionProvider;
            _asyncRunnerService = asyncRunnerService;
        }


        public IList<TemplateFileToDownloadInfo> GetFilesUrls(IList<string> filePaths)
        {
            if (filePaths == null || filePaths.Count == 0)
            {
                return new List<TemplateFileToDownloadInfo>();
            }
            return filePaths.Select(f => GenerateFileDownloadInfo(f)).ToList();
        }

        private TemplateFileToDownloadInfo GenerateFileDownloadInfo(string filePath)
        {
            var fileUrl = GenerateFileUrl(filePath);
            return new TemplateFileToDownloadInfo()
            {
                FileName = Path.GetFileName(filePath),
                FileUrl = fileUrl,
            };
        }


        public async Task<string> Upload(EnumFileType fileTypeId, IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!FileTypeExtensions.ContainsKey(fileTypeId))
            {
                throw new BadRequestException(FileErrorCode.InvalidFileType);
            }

            if (!FileTypeExtensions[fileTypeId].Contains(ext))
            {
                throw new BadRequestException(FileErrorCode.InvalidFileExtension);
            }


            var (validate, _) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            string filePath = GenerateTempFilePath(file.FileName);

            using (var stream = File.Create(filePath.GetPhysicalFilePath(_appSetting)))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }


        #region private


        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/{DOCUMENT_TEMPLATE_FOLDER}/{DateTime.Now.ToString("yy/MM/dd")}/{Guid.NewGuid()}";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = relativeFolder.GetPhysicalFilePath(_appSetting);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private (Enum, EnumFileType?) ValidateUploadFile(IFormFile uploadFile)
        {
            if (uploadFile == null || string.IsNullOrWhiteSpace(uploadFile.FileName) || uploadFile.Length == 0)
            {
                return (FileErrorCode.InvalidFile, null);
            }
            if (uploadFile.Length > _appSetting.Configuration.FileUploadMaxLength)
            {
                return (FileErrorCode.FileSizeExceededLimit, null);
            }

            var ext = Path.GetExtension(uploadFile.FileName).ToLower();

            if (FileExtensionTypesNotAccepted.ContainsKey(ext))
            {
                return (FileErrorCode.InvalidFileType, null);
            }

            //if (!ValidFileExtensions.Values.Any(v => v.Contains(ext))
            //{
            //    return FileErrorCode.InvalidFileExtension;
            //}

            return (GeneralCode.Success, FileExtensionTypes.ContainsKey(ext) ? FileExtensionTypes[ext] : EnumFileType.Other);
        }



        public string GenerateFileUrl(string filePath)
        {
            var fileName = Path.GetFileName(filePath).NormalizeAsUrlRouteParam();// Path.GetFileName(filePath).Replace('?', ' ').Replace('#', ' ').Replace(" ", "");
            var data = $"{filePath}|{DateTime.UtcNow.GetUnix()}";
            return _appSetting.ServiceUrls.FileService.Endpoint.TrimEnd('/') + $"/filestorage/template/{fileName}?fileKey=" + data.EncryptFileKey(_dataProtectionProvider, _appSetting);
        }
     

        #endregion
    }
}
