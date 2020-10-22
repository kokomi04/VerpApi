using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.VisualDirectory
{
    public class DirectoryStructure
    {
        public string RootPath { get; set; }
        public string Name { get; set; }
        public int CountFile { get; set; }
        public List<DirectoryStructure> Childs {get;set;}
    }
}
