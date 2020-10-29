using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.VisualDirectory
{
    public class DirectoryStructure
    {

        public DirectoryStructure()
        {
            Folders = new List<DirectoryStructure>();
        }

        public string RootPath { get; set; }
        public string Name { get; set; }
        public int File { get; set; }
        public List<DirectoryStructure> Folders {get;set;}
    }
}
