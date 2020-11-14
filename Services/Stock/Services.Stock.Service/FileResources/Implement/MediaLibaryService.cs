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
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory, $"Directory '{pPath}' not exists.");

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
                ls = ls.Where(x => x.file.Contains(keyWord, StringComparison.CurrentCultureIgnoreCase)).ToList();
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

            if (string.IsNullOrWhiteSpace(directory))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            foreach (var file in formFiles)
            {
                await UploadFile(directory, file);
            }

            return true;
        }

        public async Task<bool> DeleteFiles(IList<string> files)
        {
            foreach (var file in files)
            {
                await DeleteFile(file);
            }

            return true;
        }

        public async Task<bool> DeleteFile(string filePath)
        {
            filePath = GetPhysicalFilePath(GenerateMediaFilePath(filePath));
            if (!File.Exists(filePath))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            File.Delete(filePath);
            return true;
        }

        public async Task<(Stream file, string fileName, string contentType)> GetFileStream(string filePath, bool thumb)
        {
            filePath = GenerateMediaFilePath(filePath);

            if (!File.Exists(GetPhysicalFilePath(filePath)))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            var fInfo = new FileInfo(GetPhysicalFilePath(filePath));

            if (FileExtensionTypes[fInfo.Extension.ToLower()] != EnumFileType.Image)
                return (File.OpenRead(GetPhysicalFilePath(filePath)), fInfo.Name, ContentTypes[fInfo.Extension.ToLower()]);

            var result = thumb switch
            {
                true => GetFileThumb(fInfo),
                _ => (File.OpenRead(GetPhysicalFilePath(filePath)), fInfo.Name, ContentTypes[fInfo.Extension.ToLower()])
            };
            return result;
        }

        public async Task<bool> DeletedDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) 
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            var dPath = GetPhysicalFilePath(GenerateMediaFilePath(directory));
            if (!Directory.Exists(dPath)) throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);
            else if(Directory.GetDirectories(dPath).Length > 0 || Directory.GetFiles(dPath).Length > 0) throw new BadRequestException(MediaLibraryErrorCode.DirectoryNotEmpty);

            Directory.Delete(dPath);

            return true;
        }

        public async Task<bool> CopyDirectory(string pathSource, string pathDest)
        {
            if (pathSource.Equals(pathDest))
                throw new BadRequestException(MediaLibraryErrorCode.GeneralError, "Không thể thực hiện di chuyển vào chính nó");
            if (string.IsNullOrWhiteSpace(pathDest))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            pathSource = GetPhysicalFilePath(GenerateMediaFilePath(pathSource));
            pathDest = GetPhysicalFilePath(GenerateMediaFilePath(pathDest));

            DirectoryInfo source = new DirectoryInfo(pathSource);
            DirectoryInfo dest = new DirectoryInfo(pathDest + "/" + source.Name);

            if (!source.Exists) throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);
            if (dest.Exists) throw new BadRequestException(MediaLibraryErrorCode.SubdirectoryExists);

            CopyDir(source.FullName, dest.FullName);

            return true;
        }

        public async Task<bool> MoveDirectory(string pathSource, string pathDest)
        {
            if (pathSource.Equals(pathDest))
                throw new BadRequestException(MediaLibraryErrorCode.GeneralError, "Không thể thực hiện di chuyển vào chính nó");
            if (string.IsNullOrWhiteSpace(pathSource))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            pathSource = GetPhysicalFilePath(GenerateMediaFilePath(pathSource));
            pathDest = GetPhysicalFilePath(GenerateMediaFilePath(pathDest));

            DirectoryInfo source = new DirectoryInfo(pathSource);
            DirectoryInfo dest = new DirectoryInfo(pathDest + "/" + source.Name);

            if (!source.Exists) throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);
            if (dest.Exists) throw new BadRequestException(MediaLibraryErrorCode.SubdirectoryExists);

            source.MoveTo(dest.FullName);

            return true;
        }

        public async Task<bool> RenameDirectory(string pathSource, string newNameFolder)
        {
            if (string.IsNullOrWhiteSpace(pathSource) || string.IsNullOrWhiteSpace(newNameFolder))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);
            pathSource = GetPhysicalFilePath(GenerateMediaFilePath(pathSource));

            DirectoryInfo source = new DirectoryInfo(pathSource);
            DirectoryInfo dest = new DirectoryInfo(Path.Combine(source.Parent.FullName,newNameFolder));

            if (!source.Exists) throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);
            if (dest.Exists) throw new BadRequestException(MediaLibraryErrorCode.SubdirectoryExists);

            source.MoveTo(dest.FullName);

            return true;
        }

        public async Task<bool> CopyFiles(IList<string> files, string pathSource)
        {
            if (string.IsNullOrWhiteSpace(pathSource))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            foreach(var f in files)
            {
                var s = GetPhysicalFilePath(GenerateMediaFilePath(f));
                var source = new FileInfo(s);

                var d = GetPhysicalFilePath(GenerateMediaFilePath(Path.Combine(pathSource, source.Name)));
                var dest = new FileInfo(d);
                

                if (!source.Exists) throw new BadRequestException(FileErrorCode.FileNotFound);
                if (dest.Directory.Exists) throw new BadRequestException(MediaLibraryErrorCode.SubdirectoryExists);
                if (dest.Exists) throw new BadRequestException(FileErrorCode.FileExists);

                source.CopyTo(dest.FullName);
            }

            return true;
        }

        public async Task<bool> MoveFiles(IList<string> files, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new BadRequestException(MediaLibraryErrorCode.NotFoundDirectory);

            foreach (var f in files)
            {
                var s = GetPhysicalFilePath(GenerateMediaFilePath(f));
                var source = new FileInfo(s);

                var d = GetPhysicalFilePath(GenerateMediaFilePath(Path.Combine(directory, source.Name)));
                var dest = new FileInfo(d);

                if (!source.Exists) throw new BadRequestException(FileErrorCode.FileNotFound);
                if (dest.Directory.Exists) throw new BadRequestException(MediaLibraryErrorCode.SubdirectoryExists);
                if (dest.Exists) throw new BadRequestException(FileErrorCode.FileExists);

                source.MoveTo(dest.FullName);
            }

            return true;
        }

        public async Task<bool> RenameFile(string filePath, string newNameFile)
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(newNameFile))
                throw new BadRequestException(FileErrorCode.FileNotFound);

            filePath = GetPhysicalFilePath(GenerateMediaFilePath(filePath));

            FileInfo source = new FileInfo(filePath);
            FileInfo dest = new FileInfo(Path.Combine(source.Directory.FullName, newNameFile));

            if (!source.Exists) throw new BadRequestException(FileErrorCode.FileNotFound);
            if (dest.Exists) throw new BadRequestException(FileErrorCode.FileExists);

            source.MoveTo(dest.FullName);

            return true;
        }

        #region private
        private void CopyDir(string path, string dest)
        {
            if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);
            foreach (string f in Directory.GetFiles(path))
            {
                FileInfo file = new FileInfo(f);
                if (!System.IO.File.Exists(Path.Combine(dest, file.Name))) System.IO.File.Copy(f, Path.Combine(dest, file.Name));
            }
            foreach (string d in Directory.GetDirectories(path)) CopyDir(d, Path.Combine(dest, new DirectoryInfo(d).Name));
        }
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

            return (File.OpenRead(GetPhysicalFilePath(fileScale)), fileName, "image/jpeg");
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
