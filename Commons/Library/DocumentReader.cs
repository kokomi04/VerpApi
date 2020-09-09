using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Spire.Doc;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library
{
    public class DocumentReader
    {
        public  Document document { get; private set; }

        public DocumentReader(string filePath) : this(new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
        {

        }

        public DocumentReader(Stream fileStream)
        {
            document = new Document(fileStream, FileFormat.Auto);
            fileStream.Close();
        }
    }
}
