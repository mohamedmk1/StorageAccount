using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pluralight.Todo.Models;
using Pluralight.Todo.Repositories;
using StorageAccount.Todo.Core;
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
            _logger.LogInformation(LoggingEvents.ListItems, "Listing all items");

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
            _logger.LogInformation(LoggingEvents.GetItem,"Get todo item information group {group}, id {id}", group, id);
            TodoEntity item;

            item = _repository.GetTodoById(group, id);

            try
            {
                if (item == null)
                {
                    throw new Exception("Todo item is not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(LoggingEvents.GetItemNotFound, ex.Message,"GetTodoItem( {group}, {id}) NotFound", group, id);
                return NotFound();
            }

            return new TodoModel
            {
                Id = id,
                Group = group,
                Content = item.Content,
                Completed = item.Completed,
                Due = item.Due
            };
        }

        [HttpPost("{Group}")]
        public ActionResult<TodoModel> CreateTodoItem(TodoModel item, string group)
        {
            if (item == null || string.IsNullOrEmpty(item.Content))
            {
                _logger.LogInformation(LoggingEvents.InsertItem, "Empty item");
                return BadRequest();
            }

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

            _logger.LogInformation(LoggingEvents.InsertItem, "Item {id} created in {group}", entity.RowKey, group);

            return CreatedAtAction(nameof(GetTodoItem), new { group = entity.PartitionKey,id = entity.RowKey }, item);
        }

        [HttpPut("{Group}/{Id}")]
        public IActionResult Update(TodoModel model, string group, string id)
        {
            if(model == null || model.Id != id || string.IsNullOrEmpty(model.Content))
            {
                _logger.LogInformation(LoggingEvents.UpdateItem, "Empty item");
                return BadRequest();
            }

            var item = _repository.GetTodoById(group, id);
            if (item == null)
            {
                _logger.LogInformation(LoggingEvents.UpdateItem, "Item {Id} in {Group} partition not exist", id, group);
                return NotFound();
            }
           

            _repository.UpdateToDoIntem(new TodoEntity
            {
                PartitionKey = group,
                RowKey = id,
                Completed = model.Completed,
                Content = model.Content,
                Due = model.Due
            });

            _logger.LogInformation(LoggingEvents.UpdateItem, "Item {Id} in {Group} partition Updated", id, group);

            return new NoContentResult();
        }

        [HttpDelete("{Group}/{Id}")]
        public IActionResult Delete(string group, string id)
        {
            var item = _repository.GetTodoById(group, id);
            if (item == null)
            {
                _logger.LogInformation(LoggingEvents.DeleteItem, "Delete Group: {Group}, Id: {Id} Not Found", group, id);
                return NotFound();
            }

            _repository.DeleteTodoItem(item);

            _logger.LogInformation(LoggingEvents.DeleteItem, "Delete Group: {Group}, Id: {Id}", group, id);

            return new NoContentResult();
        }
    }
}
