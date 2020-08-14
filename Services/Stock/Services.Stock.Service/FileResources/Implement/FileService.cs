using ImageMagick;
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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
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
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunnerService;

        private readonly IDataProtectionProvider _dataProtectionProvider;


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

        private static readonly Dictionary<EnumFileType, string[]> FileTypeExtensions = FileExtensionTypes.GroupBy(t => t.Value).ToDictionary(t => t.Key, t => t.Select(v => v.Key).ToArray());


        public FileService(
            StockDBContext stockContext
            , IOptions<AppSetting> appSetting
            , ILogger<FileService> logger
            , IActivityLogService activityLogService
            , IDataProtectionProvider dataProtectionProvider
            , IAsyncRunnerService asyncRunnerService
        )
        {
            _stockContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _dataProtectionProvider = dataProtectionProvider;
            _asyncRunnerService = asyncRunnerService;
        }


        public async Task<FileToDownloadInfo> GetFileUrl(long fileId, EnumThumbnailSize? thumb)
        {
            var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            return GetFileUrl(fileInfo, thumb, true);
        }

        public async Task<IList<FileThumbnailInfo>> GetThumbnails(IList<long> fileIds, EnumThumbnailSize? thumb)
        {
            if (fileIds == null || fileIds.Count == 0)
            {
                return new List<FileThumbnailInfo>();
            }
            var fileInfos = await _stockContext.File.AsNoTracking().Where(f => fileIds.Contains(f.FileId)).ToListAsync();
            if (fileInfos.Count == 0)
            {
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }

            var lstData = new List<FileThumbnailInfo>();
            foreach (var file in fileInfos)
            {
                var fileInfo = GetFileUrl(file, thumb, false);
                lstData.Add(fileInfo);
            }
            return lstData;
        }


        public async Task<(FileEnity info, string physicalPath)> GetFileAndPath(long fileId)
        {
            var fileInfo = await _stockContext.File.AsNoTracking().FirstOrDefaultAsync(f => f.FileId == fileId);
            if (fileInfo == null)
            {
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            var filePath = GetPhysicalFilePath(fileInfo.FilePath);
            try
            {
                return (fileInfo, filePath);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogDebug(ex, $"GetFileAndPath(long fileId={fileId})");
                throw new BadRequestException(FileErrorCode.FileNotFound);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, $"GetFileAndPath(long fileId={fileId})");
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

            await _activityLogService.CreateLog(EnumObjectType.File, fileId, $"Xóa file " + Path.GetFileName(fileInfo.FilePath), beforeJson);
            return GeneralCode.Success;
        }

        public async Task<long> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, string fileName, IFormFile file)
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

            return await Upload(objectTypeId, fileName, file);
        }


        public async Task<long> Upload(EnumObjectType objectTypeId, string fileName, IFormFile file)
        {

            //var fileType = EnumFileType.Image;

            //switch (objectTypeId)
            //{
            //    case EnumObjectType.UserAndEmployee:
            //    case EnumObjectType.BusinessInfo:
            //        fileType = EnumFileType.Image;
            //        break;

            //    case EnumObjectType.PurchasingSuggest:
            //    case EnumObjectType.PurchaseOrder:
            //        fileType = EnumFileType.Document;
            //        break;

            //    default:
            //        return null;
            //}


            var (validate, fileTypeId) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            string filePath = GenerateTempFilePath(file.FileName);

            using (var stream = File.Create(filePath.GetPhysicalFilePath(_appSetting)))
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

                    await _activityLogService.CreateLog(EnumObjectType.File, fileRes.FileId, $"Upload file {fileName}", fileRes.JsonSerialize());

                    return fileRes.FileId;
                }
                catch (Exception ex)
                {
                    trans.TryRollbackTransaction();
                    File.Delete(filePath);
                    _logger.LogError(ex, "Upload");
                    throw;
                }
            }

        }


        public IList<ExcelSheetDataModel> ParseExcel(IFormFile file, string sheetName, int fromRow = 1, int? toRow = null, int? maxrows = null)
        {

            var (validate, fileTypeId) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!new[] { ".xls", ".xlsx" }.Contains(ext))
            {
                throw new BadRequestException(FileErrorCode.InvalidFileExtension);
            }

            var reader = new ExcelReader(file.OpenReadStream());

            return reader.ReadSheets(sheetName, fromRow, toRow, maxrows);
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

                var physicalFolder = GetPhysicalFilePath(relDirectory);
                if (!Directory.Exists(physicalFolder))
                {
                    Directory.CreateDirectory(physicalFolder);
                }

                const int quality = 75;

                using (var image = new MagickImage(filePath))
                {
                    image.Resize((int)(image.Width * 500.0 / image.Height), 500);
                    image.Strip();
                    image.Quality = quality;
                    image.Write(GetPhysicalFilePath(largeThumb));

                    image.Resize((int)(image.Width * 100.0 / image.Height), 100);
                    image.Strip();

                    image.Write(GetPhysicalFilePath(smallThumb));
                }

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

        public async Task<IList<FileToDownloadInfo>> GetListFileUrl(IList<long> fileIds, EnumThumbnailSize? thumb)
        {
            if (fileIds.Count == 0)
                return null;
            var fileList = new List<FileToDownloadInfo>(fileIds.Count);

            var files = await _stockContext.File.AsNoTracking().Where(f => fileIds.Contains(f.FileId)).ToListAsync();
            foreach (var fileInfo in files)
            {
                var fileToDownloadInfo = GetFileUrl(fileInfo, thumb, true);
                fileList.Add(fileToDownloadInfo);
            }
            return fileList;
        }

        #region private

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

                        await _activityLogService.CreateLog(EnumObjectType.File, fileInfo.FileId, $"Cập nhật file {objectTypeId}", fileInfo.JsonSerialize());

                        _asyncRunnerService.RunAsync<IFileService>(s => s.GenerateThumbnail(fileInfo.FileId));

                        return GeneralCode.Success;
                    }
                    catch (Exception ex)
                    {
                        trans.TryRollbackTransaction();
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

            if (!FileExtensionTypes.ContainsKey(ext))
            {
                return (FileErrorCode.InvalidFileType, null);
            }

            //if (!ValidFileExtensions.Values.Any(v => v.Contains(ext))
            //{
            //    return FileErrorCode.InvalidFileExtension;
            //}

            return (GeneralCode.Success, FileExtensionTypes[ext]);
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
                FileId = fileInfo.FileId,
                FileName = fileInfo.FileName,
                FileUrl = fileUrl,
                ThumbnailUrl = thumbUrl,
                FileLength = fileInfo.FileLength ?? 0
            };
        }

        private string GetFileUrl(long fileId, string filePath, string contentType)
        {
            var fileName = Path.GetFileName(filePath);
            var data = $"{fileId}|{filePath}|{contentType}|{DateTime.UtcNow.GetUnix()}";
            return _appSetting.ServiceUrls.FileService.Endpoint.TrimEnd('/') + $"/filestorage/view/{fileName}?fileKey=" + data.EncryptFileKey(_dataProtectionProvider, _appSetting);
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }
        #endregion
    }
}
