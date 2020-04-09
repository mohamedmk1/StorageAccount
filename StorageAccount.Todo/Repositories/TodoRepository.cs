using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Pluralight.Todo.Repositories
{
    public interface ITodoRepository
    {
        IEnumerable<TodoEntity> GetAllTodo();
        void CreateTodoItem(TodoEntity item);
        void UpdateToDoIntem(TodoEntity item);
        void DeleteTodoItem(TodoEntity item);
        TodoEntity GetTodoById(string group, string id);
    }

    public class TodoRepository: ITodoRepository
    {

        private CloudTable todoTable = null;
        private readonly IConfiguration _configuration;

        public TodoRepository(IConfiguration configuration)
        {
            _configuration = configuration;

            todoTable = GetTodoTableRefernce();
        }

        public IEnumerable<TodoEntity> GetAllTodo()
        {
            var query = new TableQuery<TodoEntity>();

            var entities = todoTable.ExecuteQuery(query);

            return entities;
        }

        public void CreateTodoItem(TodoEntity entity)
        {
            var operation = TableOperation.Insert(entity);

            todoTable.Execute(operation);
        }

        public void UpdateToDoIntem(TodoEntity entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);

            todoTable.Execute(operation);
        }

        public void DeleteTodoItem(TodoEntity entity)
        {
            var operation = TableOperation.Delete(entity);

            todoTable.Execute(operation);
        }

        public  TodoEntity GetTodoById(string partitionKey, string rowKey)
        {
            var operation = TableOperation.Retrieve<TodoEntity>(partitionKey, rowKey);

            var result = todoTable.Execute(operation);

            return result.Result as TodoEntity;
        }

        private CloudTable GetTodoTableRefernce()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.GetSection("ConnectionString").Value);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            return tableClient.GetTableReference("Todo");
        }
    }

    public class TodoEntity : TableEntity
    {
        public string Content { get; set; }
        public bool Completed { get; set; }
        public string Due { get; set; }
    }
}
