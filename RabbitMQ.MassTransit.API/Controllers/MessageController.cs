using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.MassTransit.API.Model;
using RabbitMQ.MassTransit.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.MassTransit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _serviceFactory;

        public MessageController(AppDbContext context, IServiceScopeFactory serviceFactory)
        {
            _context = context;
            _serviceFactory = serviceFactory;
        }

        [HttpPost("MassTransit")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> MassTransit(MessageBox request)
        {
            _context.MessageBoxs.Add(request);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] string message)
        {
            using (IServiceScope scope = _serviceFactory.CreateScope())
            {
                IPublishEndpoint publisher = scope.ServiceProvider.GetService<IPublishEndpoint>();

                await publisher.Publish<IMessage>(new
                {
                    Message = message
                });
            }

            return Ok("Message sent successfully");
        }

        [HttpGet]
        public async Task<ActionResult<List<MessageBox>>> Get()
        {
            List<MessageBox> result = await _context.MessageBoxs.ToListAsync();

            if (!result.Any())
                return NotFound("Messages not found");

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MessageBox>> Get(int id)
        {
            MessageBox result = await _context.MessageBoxs.FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
                return NotFound("Message not found");

            return Ok(result);
        }
    }
}
