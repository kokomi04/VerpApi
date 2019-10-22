using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using FileEnity = VErp.Infrastructure.EF.StockDB.File;
using StockDBContext = VErp.Infrastructure.EF.StockDB.StockDBContext;

namespace VErp.Services.Stock.Service.FileResources.Implement
{
    public class FileService : IFileService
    {
        private readonly StockDBContext _stockContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityService _activityService;

        private readonly string _rootFolder = "";


        public FileService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IActivityService activityService
        )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _rootFolder = _appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\');

        }


        public async Task<ServiceResult<long>> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file)
        {

            try
            {
                var validate = ValidateUploadFile(fileTypeId, file);
                if (!validate.IsSuccess())
                {
                    return validate;
                }

                string filePath = GenerateTempFilePath(file.FileName);

                using (var stream = File.Create(_rootFolder + filePath))
                {
                    await file.CopyToAsync(stream);
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = file.FileName;
                }

                using (var trans = await _stockContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var fileRes = new FileEnity
                        {
                            FileTypeId = (int)fileTypeId,
                            FilePath = filePath,
                            FileName = fileName,
                            ObjectTypeId = (int)objectTypeId,
                            ObjectId = null,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            FileStatusId = (int)EnumFileStatus.Temp,
                            IsDeleted = false
                        };

                        await _stockContext.File.AddAsync(fileRes);
                        await _stockContext.SaveChangesAsync();
                        trans.Commit();

                        await _activityService.CreateActivity(EnumObjectType.File, fileRes.FileId, $"Upload file {fileName}", null, fileRes);

                        return fileRes.FileId;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        File.Delete(filePath);
                        _logger.LogError(ex, "Upload");
                        return GeneralCode.InternalError;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Upload");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> FileAssignToObject(EnumObjectType objectTypeId, long objectId, long fileId)
        {

            try
            {
                var fileInfo = await _stockContext.File.FirstOrDefaultAsync(f => f.FileId == fileId);
                if (fileInfo == null)
                {
                    return FileErrorCode.FileNotFound;
                }

                if (fileInfo.FileStatusId != (int)EnumFileStatus.Temp)
                {
                    return FileErrorCode.InvalidFileStatus;
                }

                if (fileInfo.ObjectTypeId != (int)objectTypeId)
                {
                    return FileErrorCode.InvalidObjectType;
                }

                string filePath = GenerateFilePathWithObject(objectTypeId, objectId, Path.GetFileName(fileInfo.FilePath));

                File.Move(_rootFolder + fileInfo.FilePath, _rootFolder + filePath);

                Directory.Delete(_rootFolder + fileInfo.FilePath.Substring(fileInfo.FilePath.LastIndexOf('/')), true);

                var beforeJson = fileInfo.JsonSerialize();

                using (var trans = await _stockContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        fileInfo.UpdatedDatetimeUtc = DateTime.UtcNow;
                        fileInfo.ObjectId = objectId;
                        fileInfo.FileStatusId = (int)EnumFileStatus.Ok;

                        await _stockContext.SaveChangesAsync();
                        trans.Commit();

                        await _activityService.CreateActivity(EnumObjectType.File, objectId, $"Cập nhật file {objectTypeId}", beforeJson, fileInfo);

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        File.Delete(filePath);
                        _logger.LogError(ex, "FileAssignToObject");
                        return GeneralCode.InternalError;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FileAssignToObject");
                return GeneralCode.InternalError;
            }
        }

        private string GenerateFilePathWithObject(EnumObjectType objectTypeId, long objectId, string uploadFileName)
        {
            var relativeFolder = $"/{objectTypeId.ToString()}/{objectId}";
            var fNameWithoutExtension = Path.GetFileNameWithoutExtension(uploadFileName);
            var ext = Path.GetExtension(uploadFileName);

            var fileName = fNameWithoutExtension + ext;
            var relativeFilePath = relativeFolder + "/" + fileName;

            int i = 1;
            while (File.Exists(_rootFolder + relativeFilePath))
            {
                fileName = fNameWithoutExtension + $"({i++})" + ext;
                relativeFilePath = relativeFolder + "/" + fileName;
            }

            var obsoluteFolder = _rootFolder + relativeFolder;
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/_tmp_/{Guid.NewGuid().ToString()}";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = _rootFolder + relativeFolder;
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private Enum ValidateUploadFile(EnumFileType fileTypeId, IFormFile uploadFile)
        {
            if (uploadFile == null || string.IsNullOrWhiteSpace(uploadFile.FileName) || uploadFile.Length == 0)
            {
                return FileErrorCode.InvalidFile;
            }
            if (uploadFile.Length > _appSetting.Configuration.FileUploadMaxLength)
            {
                return FileErrorCode.FileSizeExceededLimit;
            }

            var ext = Path.GetExtension(uploadFile.FileName).ToLower();

            if (!ValidFileExtensions.ContainsKey(fileTypeId))
            {
                return FileErrorCode.InvalidFileType;
            }

            if (!ValidFileExtensions[fileTypeId].Contains(ext))
            {
                return FileErrorCode.InvalidFileExtension;
            }

            return GeneralCode.Success;
        }

        private static readonly Dictionary<EnumFileType, string[]> ValidFileExtensions = new Dictionary<EnumFileType, string[]>()
        {
            { EnumFileType.Image, new[] { ".jpg", ".jpeg", ".bmp", ".png" } }
        };
    }
}
