using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Model.FileConfig
{
    public class FileConfigurationModel: IMapFrom<FileConfiguration>
    {
        public long FileMaxLength { get; set; }
    }
}