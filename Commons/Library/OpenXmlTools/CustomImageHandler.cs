﻿using OpenXmlPowerTools;
using OpenXmlPowerTools.OpenXMLWordprocessingMLToHtmlConverter;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;

namespace VErp.Commons.Library.OpenXmlTools
{
    public class CustomImageHandler : IImageHandler
    {
        public CustomImageHandler(string imageDirectoryName)
        {
            ImageDirectoryName = imageDirectoryName;
            ImageCounter = 0;
        }

        public string ImageDirectoryName { get; }
        public int ImageCounter { get; private set; }

        public XElement TransformImage(ImageInfo imageInfo)
        {
            var localDirInfo = new DirectoryInfo(ImageDirectoryName);
            if (!localDirInfo.Exists)
            {
                localDirInfo.Create();
            }

            ++ImageCounter;
            var extension = imageInfo.ContentType.Split('/')[1].ToLower();
            ImageFormat imageFormat = null;
            if (extension == "png")
            {
                imageFormat = ImageFormat.Png;
            }
            else if (extension == "gif")
            {
                imageFormat = ImageFormat.Gif;
            }
            else if (extension == "bmp")
            {
                imageFormat = ImageFormat.Bmp;
            }
            else if (extension == "jpeg")
            {
                imageFormat = ImageFormat.Jpeg;
            }
            else if (extension == "tiff")
            {
                // Convert tiff to gif.
                extension = "gif";
                imageFormat = ImageFormat.Gif;
            }
            else if (extension == "x-wmf")
            {
                extension = "wmf";
                imageFormat = ImageFormat.Wmf;
            }

            // If the image format isn't one that we expect, ignore it,
            // and don't return markup for the link.
            if (imageFormat == null)
            {
                return null;
            }

            var imageFileName = ImageDirectoryName + "/image" + ImageCounter.ToString() + "." + extension;
            try
            {
                imageInfo.Bitmap.Save(imageFileName, imageFormat);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                return null;
            }
            var imageSource = localDirInfo.Name + "/image" + ImageCounter.ToString() + "." + extension;

            // This is code commentstring
            // base64 = null;
            //try
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        imageInfo.Bitmap.Save(ms, imageFormat);
            //        var ba = ms.ToArray();
            //        base64 = System.Convert.ToBase64String(ba);
            //    }
            //}
            //catch (System.Runtime.InteropServices.ExternalException)
            //{
            //    return null;
            //}

            //ImageFormat format = imageInfo.Bitmap.RawFormat;
            //ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
            //string mimeType = codec.MimeType;

            //string imageSource = string.Format("data:{0};base64,{1}", mimeType, base64);

            var img = new XElement(Xhtml.img, new XAttribute(NoNamespace.src, imageSource), imageInfo.ImgStyleAttribute, imageInfo.AltText != null ? new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
            return img;
        }
    }
}
