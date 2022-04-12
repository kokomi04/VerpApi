using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.Library.Queue.Interfaces
{
    public interface IMessageQueuePublisher
    {
        Task EnqueueAsync(string queueName, string data, int userId);
    }

}
