using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.VisualDirectory;

namespace VErp.Services.Master.Service.VisualDirectory.Implement
{
    public class MediaLibaryService : IMediaLibaryService

    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private static readonly Dictionary<string, EnumFileType> FileExtensionTypes = new Dictionary<string, EnumFileType>()
        {
            { ".jpg", EnumFileType.Image },
            { ".jpeg", EnumFileType.Image },
            { ".bmp", EnumFileType.Image },
            { ".png"  , EnumFileType.Image },

            { ".mp4" , EnumFileType.Video },
        };

        public MediaLibaryService(IOptions<AppSetting> appSetting, ILogger<MediaLibaryService> logger)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<bool> CreateSubdirectory(string root, string subdirectory)
        {
            var pPath = GenerateMediaFilePath(root);
            if (!Directory.Exists(GetPhysicalFilePath(pPath)))
                throw new BadRequestException(VisualDirectoryErrorCode.SubdirectoryExists, $"Directory '{pPath}' not exists.");

            var sPath = Path.Combine(pPath, subdirectory);

            if (!Directory.Exists(GetPhysicalFilePath(sPath)))
                Directory.CreateDirectory(GetPhysicalFilePath(sPath));

            return true;
        }

        public async Task<DirectoryStructure> GetDirectoryStructure()
        {
            return GetDirectoryStructure(GenerateRootPath());
        }

        public async Task<PageData<VisualFile>> GetVisualFiles(string directory, string keyWord, int page, int size)
        {
            var rootDirectoryPath = $"";
            if (!Directory.Exists(rootDirectoryPath))
                throw new BadRequestException(VisualDirectoryErrorCode.NotFoundVisualDirectory);

            List<VisualFile> ls = null;

            //if (string.IsNullOrWhiteSpace(directory))
            //{
            //    var files = Directory.EnumerateFiles(rootDirectoryPath, "*", SearchOption.AllDirectories).ToList();
            //    ls = files.Select(f => new VisualFile
            //    {
            //        FileName = Path.GetFileName(f),
            //        FilePath = f.Replace(_appSetting.VisualDirectory.PhysicalRootDirectory, "").Replace("\\", "/"),
            //        Address = $"{_appSetting.VisualDirectory.Address}/{f.Replace(_appSetting.VisualDirectory.PhysicalRootDirectory, "").Replace("\\", "/")}"
            //    }).ToList();
            //}
            //else
            //{
            //    var dr = new DirectoryInfo(rootDirectoryPath);
            //    var files = dr.GetFiles().ToList();

            //    ls = files.Select(f => new VisualFile
            //    {
            //        FileName = f.Name,
            //        FilePath = $"{directory}/{f.Name}",
            //        Address = $"{_appSetting.VisualDirectory.Address}/{directory}/{f.Name}"
            //    }).ToList();
            //}

            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                ls = ls.Where(x => x.FileName.Contains(keyWord)).ToList();
            }

            var total = ls.Count;
            return (ls.Skip((page - 1) * size)
                    .Take(size).ToList(), total);
        }

        public async Task<bool> UploadFile(string directory, IFormFile file)
        {
            //var (validate, fileTypeId) = ValidateUploadFile(file);
            //if (!validate.IsSuccess())
            //{
            //    throw new BadRequestException(validate);
            //}
            //string filePath = GetPhysicalFilePath(directory, file.FileName, _appSetting);

            //using (var stream = File.Create(filePath))
            //{
            //    await file.CopyToAsync(stream);
            //}

            return true;
        }

        public async Task<bool> UploadFiles(string directory, IEnumerable<IFormFile> formFiles)
        {
            directory = string.IsNullOrWhiteSpace(directory) ? "" : directory;

            foreach (var file in formFiles)
            {
                await UploadFile(directory, file);
            }

            return true;
        }

        public async Task<bool> DeleteFiles(IList<VisualFile> files)
        {
            //foreach (var file in files)
            //{
            //    File.Delete(GetPhysicalFilePath("", file.FilePath, _appSetting));
            //}

            return true;
        }

        #region private
        private DirectoryStructure GetDirectoryStructure(string rootDirectory)
        {
            var physicalFolderPath = GetPhysicalFilePath(rootDirectory);
            if (!Directory.Exists(physicalFolderPath))
                throw new DirectoryNotFoundException($"Directory '{rootDirectory}' not found.");

            var directoryInfo = new DirectoryInfo(physicalFolderPath);
            var dr = new DirectoryStructure
            {
                RootPath = string.Empty,
                File = Directory.EnumerateFiles(physicalFolderPath, "*", SearchOption.AllDirectories).Count(),
                Name = "Root"
            };

            foreach (var subDirectory in directoryInfo.GetDirectories())
            {
                dr.Folders.Add(GetDirectoryStructure(subDirectory, directoryInfo.FullName));
            }

            return dr;
        }
        private DirectoryStructure GetDirectoryStructure(DirectoryInfo directory, string parent)
        {
            var drStructure = new DirectoryStructure();

            drStructure.File = directory.GetFiles().Count();
            drStructure.Name = directory.Name;
            drStructure.RootPath = directory.FullName.Replace(parent, "").Replace("\\", "/").TrimStart('/').TrimEnd('/');
            drStructure.Folders = new List<DirectoryStructure>();

            foreach (var subDirectory in directory.GetDirectories())
            {
                drStructure.Folders.Add(GetDirectoryStructure(subDirectory, parent));
            }

            return drStructure;
        }

        private string GenerateMediaFilePath(string uploadFileName)
        {
            return GenerateRootPath() + "/" + uploadFileName;
        }

        private string GenerateRootPath()
        {
            var rootFolder = $"/_media_";
            var obsoluteFolder = GetPhysicalFilePath(rootFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return rootFolder;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }

        private (Enum, EnumFileType?) ValidateUploadFile(IFormFile uploadFile)
        {
            if (uploadFile == null || string.IsNullOrWhiteSpace(uploadFile.FileName) || uploadFile.Length == 0)
            {
                return (FileErrorCode.InvalidFile, null);
            }
            //if (uploadFile.Length > _appSetting.Configuration.FileUploadMaxLength)
            //{
            //    return (FileErrorCode.FileSizeExceededLimit, null);
            //}

            var ext = Path.GetExtension(uploadFile.FileName).ToLower();

            if (!FileExtensionTypes.ContainsKey(ext))
            {
                return (FileErrorCode.InvalidFileType, null);
            }

            return (GeneralCode.Success, FileExtensionTypes[ext]);
        }
        #endregion
    }
}
