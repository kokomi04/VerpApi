﻿using System.Collections.Generic;
using VErp.Commons.Enums.StandardEnum;

namespace VErp.Services.Stock.Model.FileResources
{
    public class GetThumbnailsInput
    {
        public IList<long> FileIds { get; set; }
        public EnumThumbnailSize? ThumbnailSize { get; set; }
    }
}
