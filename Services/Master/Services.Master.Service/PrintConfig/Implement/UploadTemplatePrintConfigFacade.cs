using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;

namespace VErp.Services.Master.Service.PrintConfig.Implement
{
    public class UploadTemplatePrintConfigFacade
    {
        private static readonly Dictionary<string, EnumFileType> FileExtensionTypes = new Dictionary<string, EnumFileType>()
        {
            { ".doc" , EnumFileType.Document },
            { ".docx", EnumFileType.Document },
        };

        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>()
        {
            { ".doc" , "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        };

        private static readonly Dictionary<EnumFileType, string[]> FileTypeExtensions = FileExtensionTypes.GroupBy(t => t.Value).ToDictionary(k => k.Key, v => v.Select(e => e.Key).ToArray());

        private const string FOLDER_DOCUMENT = "_document_template_";

        private AppSetting _appSetting;

        public UploadTemplatePrintConfigFacade SetAppSetting(AppSetting appSetting)
        {
            _appSetting = appSetting;
            return this;
        }

        public FileStream GetFileStream(string filePath)
        {
            return File.OpenRead(GetPhysicalFilePath(filePath));
        }

        public bool ExistsFile(string filePath)
        {
            return File.Exists(GetPhysicalFilePath(filePath));
        }

        public async Task DeleteFile(string fielPath)
        {
            if (File.Exists(GetPhysicalFilePath(fielPath)))
            {
                File.Delete(GetPhysicalFilePath(fielPath));
            }

            await Task.CompletedTask;
        }

        public async Task<string> CopyFile(string fileName, string sourceFile)
        {
            string filePath = "";
            if (File.Exists(GetPhysicalFilePath(sourceFile)))
            {
                filePath = GenerateTempFilePath(fileName);

                File.Copy(GetPhysicalFilePath(sourceFile), GetPhysicalFilePath(filePath));
            }

            return await Task.FromResult(filePath);
        }

        public async Task<SimpleFileInfo> Upload(EnumObjectType objectTypeId, EnumFileType fileTypeId, IFormFile file)
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

            return await Upload(objectTypeId, file);
        }

        public async Task<SimpleFileInfo> Upload(EnumObjectType objectTypeId, IFormFile file)
        {
            var (validate, fileTypeId) = ValidateUploadFile(file);
            if (!validate.IsSuccess())
                throw new BadRequestException(validate);

            string filePath = GenerateTempFilePath(file.FileName);

            using (var stream = File.Create(GetPhysicalFilePath(filePath)))
            {
                await file.CopyToAsync(stream);
            }

            return new SimpleFileInfo
            {
                FileName = file.FileName,
                FilePath = filePath,
                FileTypeId = (int)fileTypeId,
                ContentType = file.ContentType,
                FileLength = file.Length
            };
        }


        private string GenerateTempFilePath(string uploadFileName)
        {
            var relativeFolder = $"/{FOLDER_DOCUMENT }/{Guid.NewGuid().ToString()}";
            var relativeFilePath = relativeFolder + "/" + uploadFileName;

            var obsoluteFolder = GetPhysicalFilePath(relativeFolder);
            if (!Directory.Exists(obsoluteFolder))
                Directory.CreateDirectory(obsoluteFolder);

            return relativeFilePath;
        }

        private string GetPhysicalFilePath(string filePath)
        {
            return filePath.GetPhysicalFilePath(_appSetting);
        }

        private (Enum, EnumFileType?) ValidateUploadFile(IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length == 0 || string.IsNullOrWhiteSpace(uploadFile.FileName))
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
    }
}
