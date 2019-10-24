using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.FileResources;
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

        private readonly IDataProtectionProvider _dataProtectionProvider;

        public FileService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IActivityService activityService
            , IDataProtectionProvider dataProtectionProvider
        )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _rootFolder = _appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\');
            _dataProtectionProvider = dataProtectionProvider;

        }

      
        public async Task<ServiceResult<FileToDownloadInfo>> GetFileUrl(long fileId)
        {
            var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                return FileErrorCode.FileNotFound;
            }

            var data = $"{fileId}|{fileInfo.FilePath}|{fileInfo.ContentType}|{DateTime.UtcNow.GetUnix()}";
            var fileUrl = _appSetting.ServiceUrls.FileService.Endpoint.TrimEnd('/') + "/api/files/preview?fileKey=" + Encrypt(data);
            return new FileToDownloadInfo()
            {
                FileName = fileInfo.FileName,
                FileUrl = fileUrl,
                FileLength = fileInfo.FileLength??0
            };
        }

        public async Task<ServiceResult<(Stream file, string contentType)>> GetFileStream(string fileKey)
        {
            await Task.CompletedTask;
            var rawString = Decrypt(fileKey);
            var data = rawString.Split('|');

            var fileId = data[0];
            var relativeFilePath = data[1];
            var contentType = data[2];
            var timeUnix = data[3];

            if (long.Parse(timeUnix) < DateTime.UtcNow.AddDays(-1).GetUnix())
            {
                return FileErrorCode.FileUrlExpired;
            }

            var filePath = _rootFolder + relativeFilePath;
            try
            {
                return (File.OpenRead(filePath), contentType);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                return FileErrorCode.FileNotFound;
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                return FileErrorCode.FileNotFound;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(string fileKey={fileKey})");
                throw;
            }
        }

        public async Task<ServiceResult<(FileEnity info, Stream file)>> GetFileStream(long fileId)
        {
            var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                return FileErrorCode.FileNotFound;
            }
            var filePath = _rootFolder + fileInfo.FilePath;
            try
            {
                return (fileInfo, File.OpenRead(filePath));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(long fileId={fileId})");
                return FileErrorCode.FileNotFound;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"GetFileStream(long fileId={fileId})");
                throw;
            }
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
                            ContentType = file.ContentType,
                            FileLength = file.Length,
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
                var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
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

                try
                {
                    Directory.Delete(_rootFolder + fileInfo.FilePath.Substring(0, fileInfo.FilePath.LastIndexOf('/')), true);
                }
                catch (Exception mov)
                {
                    _logger.LogError(mov, "Directory.Delete");
                }


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

        private string Encrypt(string input)
        {
            var protector = _dataProtectionProvider.CreateProtector(_appSetting.FileUrlEncryptPepper);
            return protector.Protect(input);
        }

        private string Decrypt(string cipherText)
        {
            var protector = _dataProtectionProvider.CreateProtector(_appSetting.FileUrlEncryptPepper);
            return protector.Unprotect(cipherText);
        }

    }
}
