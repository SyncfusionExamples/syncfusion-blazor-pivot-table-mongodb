using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace PivotTableMongoDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> ordersCollection;

        public OrderController(IConfiguration configuration)
        {
            string connectionString =
                configuration.GetConnectionString("MongoDB")
                ?? throw new InvalidOperationException(
                    "The MongoDB connection string is not configured.");

            string databaseName =
                configuration["MongoDbSettings:DatabaseName"]
                ?? "OrderDB";

            string collectionName =
                configuration["MongoDbSettings:CollectionName"]
                ?? "Orders";

            MongoClient client = new(connectionString);
            IMongoDatabase database = client.GetDatabase(databaseName);

            ordersCollection =
                database.GetCollection<Order>(collectionName);
        }

        [HttpPost]
        public object Post([FromBody] DataManagerRequest request)
        {
            _ = request;

            List<Order> dataSource = ordersCollection
                .Find(_ => true)
                .SortBy(x => x.OrderID)
                .ToList();

            return new
            {
                result = dataSource,
                count = dataSource.Count
            };
        }

        [HttpPost("Insert")]
        public IActionResult Insert([FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || string.IsNullOrWhiteSpace(order.CustomerName)
                || !order.EmployeeID.HasValue)
            {
                return ValidationProblem(
                    "CustomerName and EmployeeID are required.");
            }

            Order? lastRecord = ordersCollection
                .Find(_ => true)
                .SortByDescending(x => x.OrderID)
                .FirstOrDefault();

            order.OrderID = lastRecord?.OrderID + 1 ?? 1;

            ordersCollection.InsertOne(order);

            return Ok(order);
        }

        [HttpPost("Update")]
        public IActionResult Update([FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || !order.OrderID.HasValue
                || string.IsNullOrWhiteSpace(order.CustomerName)
                || !order.EmployeeID.HasValue)
            {
                return ValidationProblem(
                    "OrderID, CustomerName and EmployeeID are required.");
            }

            UpdateDefinition<Order> update =
                Builders<Order>.Update
                    .Set(x => x.CustomerName, order.CustomerName)
                    .Set(x => x.EmployeeID, order.EmployeeID)
                    .Set(x => x.Freight, order.Freight)
                    .Set(x => x.ShipCity, order.ShipCity);

            UpdateResult result =
                ordersCollection.UpdateOne(
                    x => x.OrderID == order.OrderID,
                    update);

            return result.MatchedCount == 0
                ? NotFound()
                : Ok(order);
        }

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] CRUDModel<Order> value)
        {
            if (!int.TryParse(
                value.Key?.ToString(),
                out int orderId))
            {
                return ValidationProblem(
                    "A numeric order key is required.");
            }

            DeleteResult result =
                ordersCollection.DeleteOne(
                    x => x.OrderID == orderId);

            return result.DeletedCount == 0
                ? NotFound()
                : NoContent();
        }

        public class Order
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? Id { get; set; }

            [BsonElement("orderId")]
            public int? OrderID { get; set; }

            [BsonElement("customerName")]
            public string? CustomerName { get; set; }

            [BsonElement("employeeId")]
            public int? EmployeeID { get; set; }

            [BsonElement("freight")]
            public double? Freight { get; set; }

            [BsonElement("shipCity")]
            public string? ShipCity { get; set; }
        }

        public class CRUDModel<T> where T : class
        {
            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("keyColumn")]
            public string? KeyColumn { get; set; }

            [JsonPropertyName("key")]
            public object? Key { get; set; }

            [JsonPropertyName("value")]
            public T? Value { get; set; }

            [JsonPropertyName("added")]
            public List<T>? Added { get; set; }

            [JsonPropertyName("changed")]
            public List<T>? Changed { get; set; }

            [JsonPropertyName("deleted")]
            public List<T>? Deleted { get; set; }

            [JsonPropertyName("params")]
            public IDictionary<string, object>? Params { get; set; }
        }
    }
}
