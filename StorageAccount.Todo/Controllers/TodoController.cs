using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pluralight.Todo.Models;
using Pluralight.Todo.Repositories;
using static Pluralight.Todo.Repositories.TodoRepository;

namespace Pluralight.Todo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {

        private readonly ILogger<TodoController> _logger;
        private readonly ITodoRepository _repository;


        public TodoController(ILogger<TodoController> logger, ITodoRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<TodoModel> Get()
        {
            var entities = _repository.GetAllTodo();
            var models = entities.Select(x => new TodoModel
            {
                Id = x.RowKey,
                Group = x.PartitionKey,
                Content = x.Content,
                Due = x.Due,
                Completed = x.Completed,
                Timestamp = x.Timestamp
            });

            return models;
        }

        [HttpGet("{Group}/{Id}")]
        public ActionResult<TodoModel> GetTodoItem(string group, string id)
        {
            var result = _repository.GetTodoById(group, id);

            return new TodoModel {
                Id = id,
                Group = group,
                Content = result.Content,
                Completed = result.Completed,
                Due = result.Due
            };
        }

        [HttpPost("{Group}")]
        public ActionResult<TodoModel> CreateTodoItem(TodoModel item, string group)
        {
            var entity = new TodoEntity
            {
                PartitionKey = group,
                RowKey = Guid.NewGuid().ToString(),
                Completed = item.Completed,
                Content = item.Content,
                Due = item.Due
            };

            _repository.CreateTodoItem(entity);

            item.Id = entity.RowKey;
            item.Group = entity.PartitionKey;

            return CreatedAtAction(nameof(GetTodoItem), new { group = entity.PartitionKey,id = entity.RowKey }, item);
        }

        [HttpPut("{Group}/{Id}")]
        public void Update(TodoModel model, string group, string id)
        {
            _repository.UpdateToDoIntem(new TodoEntity
            {
                PartitionKey = group,
                RowKey = id,
                Completed = model.Completed,
                Content = model.Content,
                Due = model.Due
            });
        }

        [HttpDelete("{Group}/{Id}")]
        public ActionResult<TodoModel> Delete(string group, string id)
        {
            var item = _repository.GetTodoById(group, id);

            _repository.DeleteTodoItem(item);
            return new TodoModel
            {
                Id = id,
                Group = group,
                Content = item.Content,
                Completed = item.Completed,
                Due = item.Due
            };
        }
    }
}
