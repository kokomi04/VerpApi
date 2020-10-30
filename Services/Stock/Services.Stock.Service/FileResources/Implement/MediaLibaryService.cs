using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Service.FileResources;

namespace VErp.Services.Stock.Service.FileResources.Implement
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

            //{ ".doc" , EnumFileType.Document },
            //{ ".pdf" , EnumFileType.Document },
            //{ ".docx", EnumFileType.Document },
            //{ ".xls", EnumFileType.Document },
            //{ ".xlsx" , EnumFileType.Document },
            //{ ".csv" , EnumFileType.Document },
        };

        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>()
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".bmp", "image/bmp" },
            { ".png"  , "image/png" },

            { ".doc" , "application/msword" },
            { ".pdf" , "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls","application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".csv", "text/csv" },
        };

        private static readonly Dictionary<EnumFileType, string[]> FileTypeExtensions = FileExtensionTypes.GroupBy(t => t.Value).ToDictionary(t => t.Key, t => t.Select(v => v.Key).ToArray());


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
            var rootDirectory = GenerateRootPath();
            var ls = new List<VisualFile>();

            if (string.IsNullOrWhiteSpace(directory))
            {
                var files = Directory.EnumerateFiles(GetPhysicalFilePath(rootDirectory), "*", SearchOption.AllDirectories).ToList();

                foreach (var f in files)
                {
                    var info = new FileInfo(f);
                    ls.Add(new VisualFile
                    {
                        file = info.Name,
                        path = f.Replace("\\", "/").Replace(GetPhysicalFilePath(rootDirectory), "").TrimStart('/').TrimEnd('/'),
                        ext = info.Extension,
                        size = info.Length,
                        time = info.LastWriteTime.GetUnix()
                    });
                }
            }
            else
            {
                var dr = new DirectoryInfo(GetPhysicalFilePath(rootDirectory + "/" + directory));
                var files = dr.GetFiles().ToList();

                ls = files.Select(f => new VisualFile
                {
                    file = f.Name,
                    path = $"{directory}/{f.Name}",
                    ext = f.Extension,
                    size = f.Length,
                    time = f.LastWriteTime.GetUnix()
                }).ToList();
            }

            if (!string.IsNullOrWhiteSpace(keyWord))
            {
                ls = ls.Where(x => x.file.Contains(keyWord)).ToList();
            }

            var total = ls.Count;
            return (ls.Skip((page - 1) * size)
                    .Take(size).ToList(), total);
        }

        public async Task<bool> UploadFile(string directory, IFormFile file)
        {

            var (validate, fileTypeId) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            string filePath = GenerateMediaFilePath($"{directory}/{file.FileName}");

            using (var stream = File.Create(GetPhysicalFilePath(filePath)))
            {
                await file.CopyToAsync(stream);
            }

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
            foreach (var file in files)
            {
                var filePath = GenerateMediaFilePath(file.path);
                File.Delete(GetPhysicalFilePath(filePath));
            }

            return true;
        }

        public async Task<(Stream file, string fileName, string contentType)> GetFileStream(string filePath, bool thumb)
        {
            filePath = GenerateMediaFilePath(filePath);

            if (!File.Exists(GetPhysicalFilePath(filePath)))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            var fInfo = new FileInfo(GetPhysicalFilePath(filePath));

            if (FileExtensionTypes[fInfo.Extension] != EnumFileType.Image)
                return (File.OpenRead(GetPhysicalFilePath(filePath)), fInfo.Name, ContentTypes[fInfo.Extension]);

            var result = thumb switch
            {
                true => GetFileThumb(fInfo),
                _ => (File.OpenRead(GetPhysicalFilePath(filePath)), fInfo.Name, ContentTypes[fInfo.Extension])
            };
            return result;
        }

        #region private
        private (Stream file, string fileName, string contentType) GetFileThumb(FileInfo file)
        {
            const int quality = 75;

            var fileName = Path.GetFileNameWithoutExtension(file.Name) + $"_thumb.jpg";

            var fileScale = GenerateTempFilePath(fileName);
            if (File.Exists(GetPhysicalFilePath(fileScale)))
                return (File.OpenRead(GetPhysicalFilePath(fileScale)), fileName, null);

            using (var image = new MagickImage(file))
            {
                image.Resize((int)(image.Width * 50 / image.Height), 50);
                image.Strip();
                image.Quality = quality;
                image.Write(GetPhysicalFilePath(fileScale));
            }

            return (File.OpenRead(GetPhysicalFilePath(fileScale)), fileName, null);
        }

        private DirectoryStructure GetDirectoryStructure(string rootDirectory)
        {
            var physicalFolderPath = GetPhysicalFilePath(rootDirectory);
            if (!Directory.Exists(physicalFolderPath))
                throw new DirectoryNotFoundException($"Directory '{rootDirectory}' not found.");

            var directoryInfo = new DirectoryInfo(physicalFolderPath);
            var dr = new DirectoryStructure
            {
                path = string.Empty,
                file = Directory.EnumerateFiles(physicalFolderPath, "*", SearchOption.AllDirectories).Count(),
                name = "Root"
            };

            foreach (var subDirectory in directoryInfo.GetDirectories())
            {
                dr.subdirectories.Add(GetDirectoryStructure(subDirectory, directoryInfo.FullName));
            }

            return dr;
        }
        private DirectoryStructure GetDirectoryStructure(DirectoryInfo directory, string parent)
        {
            var drStructure = new DirectoryStructure();

            drStructure.file = directory.GetFiles().Count();
            drStructure.name = directory.Name;
            drStructure.path = directory.FullName.Replace(parent, "").Replace("\\", "/").TrimStart('/').TrimEnd('/');
            drStructure.subdirectories = new List<DirectoryStructure>();

            foreach (var subDirectory in directory.GetDirectories())
            {
                drStructure.subdirectories.Add(GetDirectoryStructure(subDirectory, parent));
            }

            return drStructure;
        }

        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/_tmp_/media_thumb";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
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
            if (uploadFile.Length > _appSetting.Configuration.FileUploadMaxLength)
            {
                return (FileErrorCode.FileSizeExceededLimit, null);
            }

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
