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
using VErp.Infrastructure.ServiceCore.Service;
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
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly string _rootFolder = "";

        private readonly IDataProtectionProvider _dataProtectionProvider;
        private static readonly Dictionary<EnumFileType, string[]> ValidFileExtensions = new Dictionary<EnumFileType, string[]>()
        {
            { EnumFileType.Image, new[] { ".jpg", ".jpeg", ".bmp", ".png" } }
        };

        public FileService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IActivityService activityService
            , IDataProtectionProvider dataProtectionProvider
            , IAsyncRunnerService asyncRunnerService
        )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityService = activityService;
            _rootFolder = _appSetting.Configuration.FileUploadFolder.TrimEnd('/').TrimEnd('\\');
            _dataProtectionProvider = dataProtectionProvider;
            _asyncRunnerService = asyncRunnerService;
        }


        public async Task<ServiceResult<FileToDownloadInfo>> GetFileUrl(long fileId, EnumThumbnailSize? thumb)
        {
            var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                return FileErrorCode.FileNotFound;
            }

            return GetFileUrl(fileInfo, thumb, true);
        }

        public async Task<ServiceResult<IList<FileThumbnailInfo>>> GetThumbnails(IList<long> fileIds, EnumThumbnailSize? thumb)
        {
            if (fileIds == null || fileIds.Count == 0)
            {
                return GeneralCode.Success;
            }
            var fileInfos = await _stockContext.File.AsNoTracking().Where(f => fileIds.Contains(f.FileId)).ToListAsync();
            if (fileInfos.Count == 0)
            {
                return FileErrorCode.FileNotFound;
            }

            var lstData = new List<FileThumbnailInfo>();
            foreach (var file in fileInfos)
            {
                var fileInfo = GetFileUrl(file, thumb, false);
                lstData.Add(fileInfo);
            }
            return lstData;
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

            var filePath = GetPhysicalFilePath(relativeFilePath);
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
            var filePath = GetPhysicalFilePath(fileInfo.FilePath);
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

        public async Task<Enum> DeleteFile(long fileId)
        {
            var fileInfo = await _stockContext.File.FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                return FileErrorCode.FileNotFound;
            }

            var beforeJson = fileInfo.JsonSerialize();

            fileInfo.IsDeleted = true;
            await _stockContext.SaveChangesAsync();

            File.Delete(GetPhysicalFilePath(fileInfo.FilePath));

            if (!string.IsNullOrWhiteSpace(fileInfo.SmallThumb))
            {
                File.Delete(GetPhysicalFilePath(fileInfo.SmallThumb));
            }

            if (!string.IsNullOrWhiteSpace(fileInfo.LargeThumb))
            {
                File.Delete(GetPhysicalFilePath(fileInfo.LargeThumb));
            }

            _activityService.CreateActivityAsync(EnumObjectType.File, fileId, $"Xóa file " + Path.GetFileName(fileInfo.FilePath), beforeJson, null);
            return GeneralCode.Success;
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

                        _activityService.CreateActivityAsync(EnumObjectType.File, fileRes.FileId, $"Upload file {fileName}", null, fileRes);

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

        public async Task<Enum> GenerateThumbnail(long fileId)
        {
            try
            {
                var fileInfo = await _stockContext.File.FirstOrDefaultAsync(f => f.FileId == fileId);
                if (fileInfo == null)
                {
                    return FileErrorCode.FileNotFound;
                }

                var filePath = GetPhysicalFilePath(fileInfo.FilePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                var relDirectory = fileInfo.FilePath.Substring(0, fileInfo.FilePath.LastIndexOf('/'));
                relDirectory = "/" + relDirectory.Trim('/') + "/thumbs";

                var smallThumb = relDirectory + "/" + fileName + "_small.jpg";
                var largeThumb = relDirectory + "/" + fileName + "_large.jpg";

                var physicalFolder= GetPhysicalFilePath(relDirectory);
                if (!Directory.Exists(physicalFolder))
                {
                    Directory.CreateDirectory(physicalFolder);
                }

                var sourceStream = new FileStream(filePath, FileMode.Open);

                using (var m = new MemoryStream())
                {
                    await sourceStream.CopyToAsync(m);

                    sourceStream.Close();
                    sourceStream.Dispose();

                    var thumbnailCreator = new ThumbnailSharp.ThumbnailCreator();

                    var streamSmalls = thumbnailCreator.CreateThumbnailBytes(100, m, ThumbnailSharp.Format.Jpeg);
                    await WriteBytesToFile(streamSmalls, GetPhysicalFilePath(smallThumb));

                    var streamLarges = thumbnailCreator.CreateThumbnailBytes(500, m, ThumbnailSharp.Format.Jpeg);
                    await WriteBytesToFile(streamLarges, GetPhysicalFilePath(largeThumb));

                }

                sourceStream.Close();
                sourceStream.Dispose();


                fileInfo.SmallThumb = smallThumb;
                fileInfo.LargeThumb = largeThumb;
                _stockContext.SaveChanges();
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateThumbnail");
                return GeneralCode.InternalError;
            }

        }

        #region private
        private async Task WriteBytesToFile(byte[] bytes, string file)
        {
            var fileStream = new FileStream(file, FileMode.Create);
            await fileStream.WriteAsync(bytes, 0, bytes.Length);
            fileStream.Close();
            fileStream.Dispose();
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

                File.Move(GetPhysicalFilePath(fileInfo.FilePath), GetPhysicalFilePath(filePath));

                try
                {
                    var tmpFolder = fileInfo.FilePath.Substring(0, fileInfo.FilePath.LastIndexOf('/'));
                    Directory.Delete(GetPhysicalFilePath(tmpFolder), true);                    
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
                        fileInfo.FilePath = filePath;
                        fileInfo.FileStatusId = (int)EnumFileStatus.Ok;

                        await _stockContext.SaveChangesAsync();
                        trans.Commit();

                        _activityService.CreateActivityAsync(EnumObjectType.File, objectId, $"Cập nhật file {objectTypeId}", beforeJson, fileInfo);

                        _asyncRunnerService.RunAsync<IFileService>(s => s.GenerateThumbnail(fileInfo.FileId));

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
            while (File.Exists(GetPhysicalFilePath(relativeFilePath)))
            {
                fileName = fNameWithoutExtension + $"({i++})" + ext;
                relativeFilePath = relativeFolder + "/" + fileName;
            }

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/_tmp_/{Guid.NewGuid().ToString()}";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
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


        private FileToDownloadInfo GetFileUrl(FileEnity fileInfo, EnumThumbnailSize? thumb, bool isGetOriginFile)
        {
            var thumbPath = "";
            if (!string.IsNullOrWhiteSpace(fileInfo.SmallThumb))
            {
                if (thumb != null)
                {
                    switch (thumb.Value)
                    {
                        case EnumThumbnailSize.Small:
                            thumbPath = fileInfo.SmallThumb;
                            break;
                        case EnumThumbnailSize.Large:
                            thumbPath = fileInfo.LargeThumb;
                            break;
                    }
                }
            }
            else
            {
                _asyncRunnerService.RunAsync<IFileService>(s => s.GenerateThumbnail(fileInfo.FileId));
            }

            var fileUrl = isGetOriginFile ? GetFileUrl(fileInfo.FileId, fileInfo.FilePath, fileInfo.ContentType) : null;
            var thumbUrl = string.IsNullOrWhiteSpace(thumbPath) ? null : GetFileUrl(fileInfo.FileId, thumbPath, "image/jpeg");
            return new FileToDownloadInfo()
            {
                FileName = fileInfo.FileName,
                FileUrl = fileUrl,
                ThumbnailUrl = thumbUrl,
                FileLength = fileInfo.FileLength ?? 0
            };
        }

        private string GetFileUrl(long fileId, string filePath, string contentType)
        {
            var data = $"{fileId}|{filePath}|{contentType}|{DateTime.UtcNow.GetUnix()}";
            return _appSetting.ServiceUrls.FileService.Endpoint.TrimEnd('/') + "/api/files/preview?fileKey=" + Encrypt(data);
        }
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

        private string GetPhysicalFilePath(string filePath)
        {
            return _rootFolder + filePath;
        }
        #endregion
    }
}
