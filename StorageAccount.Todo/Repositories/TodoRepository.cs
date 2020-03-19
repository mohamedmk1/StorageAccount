using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Threading.Tasks;

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
        public TodoRepository()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storageacpluralisght;AccountKey=FoIktP9sACEGjoGhdR7mDn4nGjRMGyAapcTl52VSpYb3NGQtfYyKxTEMcaAmhJQvVx9065rC9ICAXxjkJQHE3Q==;EndpointSuffix=core.windows.net");

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            todoTable = tableClient.GetTableReference("Todo");
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
    }

    public class TodoEntity : TableEntity
    {
        public string Content { get; set; }
        public bool Completed { get; set; }
        public string Due { get; set; }
    }
}
