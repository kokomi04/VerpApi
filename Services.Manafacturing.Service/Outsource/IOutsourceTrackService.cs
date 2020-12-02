using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Model.Outsource.Track;

namespace VErp.Services.Manafacturing.Service.Outsource
{
    public interface IOutsourceTrackService
    {
        Task<IList<OutsourceTrackModel>> SearchOutsourceTrackByOutsourceOrder(long outsourceOrderId);
        Task<bool> UpdateOutsourceTrackByOutsourceOrder(long outsourceOrderId, IList<OutsourceTrackModel> req);
    }
}
