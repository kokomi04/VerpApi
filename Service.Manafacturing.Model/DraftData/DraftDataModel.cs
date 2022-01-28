using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Commons.Library;
using DraftDataEntity = VErp.Infrastructure.EF.ManufacturingDB.DraftData;
namespace VErp.Services.Manafacturing.Model.DraftData
{
    public class DraftDataModel : IMapFrom<DraftDataEntity>
    {
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string Data { get; set; }

        public DraftDataModel()
        {
        }
    }

}
