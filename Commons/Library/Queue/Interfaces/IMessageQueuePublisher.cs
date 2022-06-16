using System.Threading.Tasks;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Queue.Interfaces
{
    public interface IMessageQueuePublisher
    {
        Task EnqueueAsync(ICurrentContextService context, string queueName, string data, int userId);
    }

}
