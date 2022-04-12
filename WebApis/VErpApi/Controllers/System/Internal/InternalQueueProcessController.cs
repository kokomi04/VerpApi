using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Queue.Interfaces;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/InternalQueueProcess")]

    public class InternalQueueProcessController : CrossServiceBaseController
    {
        private readonly IMessageQueuePublisher _messageQueuePublisher;
        private readonly ICurrentContextService _currentContextService;

        public InternalQueueProcessController(IMessageQueuePublisher messageQueuePublisher, ICurrentContextService currentContextService)
        {
            _messageQueuePublisher = messageQueuePublisher;
            _currentContextService = currentContextService;
        }

        [HttpPost("Enqueue")]
        public async Task<bool> Enqueue([FromBody] EnqueueModel model)
        {
            await _messageQueuePublisher.EnqueueAsync(model.QueueName, model.Data, _currentContextService.UserId);
            return true;
        }

        public class EnqueueModel
        {
            public string QueueName { get; set; }
            public string Data { get; set; }
        }
    }

}
