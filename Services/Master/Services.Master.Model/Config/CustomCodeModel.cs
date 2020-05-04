using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class CustomCodeModel
    {
        public string CustomCode { get; set; }
        public int LastValue { get; set; }
        public int CustomGenCodeId { get; set; }
    }
}
