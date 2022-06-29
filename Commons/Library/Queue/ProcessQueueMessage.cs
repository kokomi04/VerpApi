using System;
using VErp.Commons.GlobalObject;

namespace VErp.Commons.Library.Queue
{
    public class ProcessQueueMessage<T>
    {
        public string QueueName { get; set; }
        public T Data { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }

        public ICurrentContextService Context { get; set; }

    }

}
