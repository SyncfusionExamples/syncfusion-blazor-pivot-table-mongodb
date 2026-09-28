# Blazor Pivot Table - MongoDB Database Connection using ASP.NET Core

## Project Overview

This repository demonstrates how to bind MongoDB data to the Syncfusion [Blazor Pivot Table](https://www.syncfusion.com/blazor-components/blazor-pivot-table) through an ASP.NET Core API and the URL Adaptor. The sample uses the MongoDB.Driver provider to load and persist documents from an Orders collection, while the Pivot Table consumes the API through SfDataManager.

The implementation follows the current guide-based pattern where the UI requests data from the server, and the server handles MongoDB access, validation, and CRUD operations. This keeps the component focused on aggregation and editing while the API manages persistence.

### Key Features

- MongoDB-backed data source for a lightweight document store sample
- Syncfusion Blazor Pivot Table with URL Adaptor remote binding
- CRUD operations exposed through API endpoints
- Same-origin relative URLs for read and write requests
- Primary key configuration for drill-through editing
- Problem details middleware and exception handling for API failures

## Repository

Sample location: https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-mongodb

## Prerequisites

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET SDK | 10.0 | Build and run the sample targeting net10.0 |
| Visual Studio 2026 / VS Code | 18.0+ / latest | Development environment with ASP.NET and web workloads |
| MongoDB Community Server | 8.0 or later | Document database engine |
| MongoDB Compass | Latest | GUI for inspecting databases and collections |
| MongoDB.Driver | 3.10.0 | Official MongoDB C# driver for .NET |
| Syncfusion.Blazor.PivotTable | 34.1.33 | Pivot Table UI component |
| Syncfusion.Blazor.Themes | 34.1.33 | Pivot Table styling |
| Newtonsoft.Json | 13.0.4 | JSON serialization for CRUD payloads |

## MongoDB Setup

### 1. Install MongoDB Community Server

Download and install MongoDB Community Server from the official MongoDB download center. The default service listens on `mongodb://localhost:27017`.

### 2. Create the database and collection

Open MongoDB Compass, connect to `mongodb://localhost:27017`, and create the following database and collection used by the sample:

```text
Database:
OrderDB

Collection:
Orders
```

### 3. Insert sample data (optional)

Insert the following sample documents into the `Orders` collection through `Add Data → Insert Document` or the embedded mongosh:

```json
{ "orderId": 1, "customerName": "Toms",   "employeeId": 1, "shipCity": "New York", "freight": 35.30 }
{ "orderId": 2, "customerName": "Ravi",   "employeeId": 2, "shipCity": "London",   "freight": 80.20 }
{ "orderId": 3, "customerName": "Sven",   "employeeId": 1, "shipCity": "Berlin",   "freight": 52.10 }
{ "orderId": 4, "customerName": "Sara",   "employeeId": 3, "shipCity": "Madrid",   "freight": 18.40 }
{ "orderId": 5, "customerName": "Paul",   "employeeId": 2, "shipCity": "Tokyo",    "freight": 64.75 }
```

### 4. Configure the connection string

The application reads the MongoDB connection string and database/collection settings from appsettings.json:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDbSettings": {
    "DatabaseName": "OrderDB",
    "CollectionName": "Orders"
  }
}
```

## Project Structure

| File/Folder | Purpose |
|-------------|---------|
| /Controllers/OrderController.cs | API controller exposing read and CRUD endpoints using MongoDB.Driver |
| /Components/Pages/Home.razor | Pivot Table page with URL Adaptor configuration, editing, and drill-through setup |
| /Program.cs | ASP.NET Core and Syncfusion service registration, middleware, and endpoint mapping |
| /appsettings.json | MongoDB connection string and database/collection configuration |
| /PivotTableMongoDB.csproj | Project file with Syncfusion packages and MongoDB.Driver NuGet references |

## Application Flow

```text
Blazor Pivot Table
        ↓
SfDataManager / UrlAdaptor
        ↓
ASP.NET Core Controller
        ↓
MongoDB.Driver
        ↓
MongoDB Database
```

1. The Pivot Table sends a request through SfDataManager.
2. The URL Adaptor posts the request to the API endpoint.
3. The controller resolves the request through MongoDB.Driver.
4. The response is returned to the client in the format expected by the Pivot Table.

## Updated Implementation Notes

### Program.cs

The current sample uses the following ASP.NET Core configuration:

```csharp
builder.Services.AddSyncfusionBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

### Home.razor

The Pivot Table uses same-origin relative URLs and marks OrderID as the primary key for drill-through editing:

```razor
<SfDataManager Url="/api/Order"
               InsertUrl="/api/Order/Insert"
               UpdateUrl="/api/Order/Update"
               RemoveUrl="/api/Order/Delete"
               Adaptor="Adaptors.UrlAdaptor">
</SfDataManager>
```

```csharp
private void BeginDrillThrough(BeginDrillThroughEventArgs args)
{
    for (int i = 0; i < args.GridObj.Columns.Count; i++)
    {
        if (args.GridObj.Columns[i].Field == "OrderID")
        {
            args.GridObj.Columns[i].IsPrimaryKey = true;
        }
        else
        {
            args.GridObj.Columns[i].Visible = true;
        }
    }
}
```

## CRUD Operations

The sample supports read, insert, update, and delete operations through the URL Adaptor.

| Endpoint | Method | Payload | Description |
|----------|--------|---------|-------------|
| /api/Order | POST | DataManagerRequest | Returns `{ result, count }` for Pivot Table binding |
| /api/Order/Insert | POST | CRUDModel<Order> | Inserts a newly added order and returns the created document |
| /api/Order/Update | POST | CRUDModel<Order> | Updates an existing order document |
| /api/Order/Delete | POST | CRUDModel<Order> | Deletes a order document using the key value |

## Data Model

The Order model used by the controller maps .NET properties to MongoDB document fields through BsonElement attributes, with the MongoDB `_id` exposed as an ObjectId:

```csharp
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
```

## Getting Started

1. Clone or open the repository in Visual Studio or VS Code.
2. Restore packages and build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Start MongoDB Community Server and create the `OrderDB` database and `Orders` collection in MongoDB Compass as described above.
4. Run the application:

   ```bash
   dotnet run --project PivotTableMongoDB
   ```

5. Open the local URL shown in the terminal and verify that the Pivot Table loads and the CRUD endpoints respond correctly.

## Full Documentation

For the complete step-by-step guide, including URL Adaptor configuration, CRUD mappings, and MongoDB connection setup, see the official Syncfusion Blazor Pivot Table documentation: https://help.syncfusion.com/grid-sdk/blazor/pivot-table/getting-started

## Support and Feedback

- For general product questions, visit the [Syncfusion Community Forum](https://www.syncfusion.com/forums) or [Syncfusion Support](https://www.syncfusion.com/support).
- To report an issue specific to this sample, open a GitHub issue in this repository.

## License

This is a Syncfusion sample project provided to demonstrate product usage. Review the [Syncfusion license terms](https://www.syncfusion.com/sales/pricing?category=ui-components) before using Syncfusion components in your own applications.