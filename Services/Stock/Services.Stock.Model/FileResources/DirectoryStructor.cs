using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.FileResources
{
    public class DirectoryStructure
    {

        public DirectoryStructure()
        {
            subdirectories = new List<DirectoryStructure>();
        }

        public string path { get; set; }
        public string name { get; set; }
        public int file { get; set; }
        public List<DirectoryStructure> subdirectories {get;set;}
    }
}
