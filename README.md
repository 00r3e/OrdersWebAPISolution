OrdersWebAPISolution
🧩 Clean Architecture • 🏗️ Repository + Unit of Work • 🚀 ASP.NET Core Web API

🌐 Live Demo

🔗 Public URL: https://orderswebapi20251212133847.azurewebsites.net/swagger

📘 Overview

OrdersWebAPISolution is a clean, modular ASP.NET Core Web API built using best practices:

✔️ Layered architecture

✔️ Repository & Unit-of-Work patterns

✔️ Services for business logic

✔️ Interfaces for all abstractions

✔️ Automated tests

✔️ Easy to extend, maintain, and understand

Perfect for learning backend architecture or using as a production-ready starter template.

📁 Folder Structure
OrdersWebAPISolution.sln
│
├── Entities/                # Domain models  
├── RepositoryContracts/     # Repository interfaces  
├── ServiceContracts/        # Service interfaces  
├── Repositories/            # Data access implementations  
├── Services/                # Business logic  
├── UnitOfWork/              # Unit-of-Work implementation  
├── Orders.WebAPI/           # API layer (controllers, config, DTOs)  
└── Orders.WebAPI.Tests/     # Automated tests  

🛠️ Tech Stack
Layer	Technology
Backend Framework	ASP.NET Core Web API
Language	C#
Architecture	Clean / Layered
Data Patterns	Repository + Unit of Work
Testing	xUnit / NUnit (as applicable)
Tools	Swagger (optional), Dependency Injection
🚀 Getting Started
📌 Prerequisites

Make sure you have installed:

.NET SDK

Visual Studio / VS Code

(Optional) SQL Server, SQLite, or your database of choice

▶️ Run the Project
git clone https://github.com/00r3e/OrdersWebAPISolution.git
cd OrdersWebAPISolution
dotnet restore
dotnet build
cd Orders.WebAPI
dotnet run


Your API will now be live on:

https://localhost:<port>

🔥 API Endpoints (example)

⚠️ Update these according to your actual controller routes.

📄 Orders
Method	Route	Description
GET	/api/orders	Get all orders
GET	/api/orders/{id}	Get order by ID
POST	/api/orders	Create new order
PUT	/api/orders/{id}	Update order
DELETE	/api/orders/{id}	Delete order
🧪 Running Tests
dotnet test


The project includes a dedicated Orders.WebAPI.Tests project for unit testing services and API behaviors.

🧱 Architecture Summary
✔️ Entities

Your domain objects — clean, simple C# classes.

✔️ Repositories

Contain data persistence logic.
Interfaces live in RepositoryContracts, implementations in Repositories.

✔️ Services

Contain all business logic.
Defined in ServiceContracts, implemented in Services.

✔️ Unit of Work

Coordinates all repository operations in a single transactional workflow.

✔️ Controllers

Expose strongly structured, RESTful API endpoints.

📚 Example Request (Create Order)
POST /api/orders
Content-Type: application/json

{
  "customerId": 123,
  "items": [
    { "productId": 456, "quantity": 2 }
  ]
}

🧭 Swagger (Optional Setup)

If you enable Swagger, your API docs appear at:

https://localhost:<port>/swagger


Highly recommended for development & testing.
