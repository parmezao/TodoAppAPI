using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Todo.Domain.Api.Extensions;
using Todo.Domain.Commands;
using Todo.Domain.Entities;
using Todo.Domain.Handlers;
using Todo.Domain.Repositories;

namespace Todo.Api.Controllers
{
    [ApiController]
    [Route("v1/todos")]
    [Authorize]
    [AllowSameSite]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger)
        {
            _logger = logger;
        }

        [Route("alert")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<GenericCommandResult> SendNotification(
            [FromServices] IHttpClientFactory clientFactory,
            [FromBody] PushNotificationCommand pushMessage
        )
        { 
            var client = clientFactory.CreateClient();

            var data = new
            {
                to = pushMessage.Token,
                notification = new
                {
                    body = $"Você tem {pushMessage.Count} tarefa(s) que iniciará(ão) em 10 minutos...",
                    title = $"ToDo - Olá {pushMessage.Name}!",
                    click_action = "https://todo.parmex.com.br",
                    content_available = true,
                    priority = "high"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            request.Headers.TryAddWithoutValidation("Authorization",
                "key=AAAAWuHxpyo:APA91bHqAG2wPLo-pM3dVxQDdNJycmVmwX15EIBWa0h5-BRIZk23PUR0yGIRKbxeszFOZof5yV_aqCmAHrxKs-YkKP_q6g3GUp7Ya9UDy17pAR3jqQ71v_QHYu3lHx2gLc2eYZw0ZpbL");

            var jsonBody = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            request.Content = jsonBody;

            try
            {
                var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new GenericCommandResult(true, "OK", new { });
                }
                else
                {
                    return new GenericCommandResult(false, "Erro", new { statusCode = response.StatusCode });
                }
            }
            catch (Exception ex)
            {
                return new GenericCommandResult(false, ex.Message, new { });
            }
        }

        [Route("alert")]
        [HttpGet]
        [AllowAnonymous]
        public IEnumerable<TodoItem> Alert(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetByPeriod(user, DateTime.Now, DateTime.Now.AddMinutes(10), false); // Período de 10 minutos
        }        

        [Route("")]
        [HttpGet]
        public IEnumerable<TodoItem> GetAll(
           [FromServices] ITodoRepository repository
        )
        {         
           var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;

           var remoteIP = HttpContext.Connection.RemoteIpAddress.ToString();
           _logger.LogInformation($"LogTodoAPI - RemoteIP: {remoteIP}");

           return repository.GetAll(user);            
        }

        [Route("done")]
        [HttpGet]
        public IEnumerable<TodoItem> GetAllDone(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetAllDone(user);
        }

        [Route("undone")]
        [HttpGet]
        public IEnumerable<TodoItem> GetAllUndone(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetAllUndone(user);
        }

        [Route("done/today")]
        [HttpGet]
        public IEnumerable<TodoItem> GetDoneForToday(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetByPeriod(
                user,
                DateTime.Now.Date,
                true
            );
        }

        [Route("undone/today")]
        [HttpGet]
        public IEnumerable<TodoItem> GetInactiveForToday(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetByPeriod(
                user,
                DateTime.Now.Date,
                false
            );
        }

        [Route("done/tomorrow")]
        [HttpGet]
        public IEnumerable<TodoItem> GetDoneForTomorrow(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetByPeriod(
                user,
                DateTime.Now.Date.AddDays(1),
                true
            );
        }

        [Route("undone/tomorrow")]
        [HttpGet]
        public IEnumerable<TodoItem> GetUndoneForTomorrow(
            [FromServices] ITodoRepository repository
        )
        {
            var user = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return repository.GetByPeriod(
                user,
                DateTime.Now.Date.AddDays(1),
                false
            );
        }

        [Route("")]
        [HttpPost]
        public GenericCommandResult Create(
            [FromBody] CreateTodoCommand command,
            [FromServices] TodoHandler handler
        )
        {
            command.User = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return (GenericCommandResult)handler.Handle(command);
        }    

        [Route("")]
        [HttpPut]
        public GenericCommandResult Update(
           [FromBody] UpdateTodoCommand command,
           [FromServices] TodoHandler handler
       )
        {
            command.User = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return (GenericCommandResult)handler.Handle(command);
        }

        [Route("mark-as-done")]
        [HttpPut]
        public GenericCommandResult MarkAsDone(
            [FromBody] MarkTodoAsDoneCommand command,
            [FromServices] TodoHandler handler
        )
        {
            command.User = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return (GenericCommandResult)handler.Handle(command);
        }

        [Route("mark-as-undone")]
        [HttpPut]
        public GenericCommandResult MarkAsUndone(
            [FromBody] MarkTodoAsUndoneCommand command,
            [FromServices] TodoHandler handler
        )
        {
            command.User = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return (GenericCommandResult)handler.Handle(command);
        }

        [Route("")]
        [HttpDelete]
        public GenericCommandResult Delete(
            [FromBody] DeleteTodoCommand command,
            [FromServices] TodoHandler handler
        )
        {
            command.User = User.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            return (GenericCommandResult)handler.Handle(command);
        }
    }
}