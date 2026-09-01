# 🏨 Resort Booking API

A RESTful Resort Booking backend built with **ASP.NET Core Web API**. The application provides APIs for managing villas and amenities, user authentication, JWT-based authorization, image uploads, and database operations using **Entity Framework Core and SQL Server**.

The project is designed with a focus on clean API design, separation of concerns, DTO-based communication, authentication, API versioning, and interactive API documentation using Scalar.

---

## 🚀 Features

* Villa management with CRUD operations
* Villa amenities management
* User registration and login
* JWT-based authentication
* Role-based authorization
* ASP.NET Core Identity
* Entity Framework Core with SQL Server
* DTO-based request and response models
* AutoMapper for object mapping
* Image upload and deletion
* Image validation
* API versioning
* OpenAPI documentation
* Scalar interactive API documentation
* Entity Framework Core migrations
* Database seeding
* Filtering and sorting support
* Asynchronous database operations
* `AsNoTracking()` for read-only queries

---

## 🛠️ Tech Stack

| Technology                    | Purpose                          |
| ----------------------------- | -------------------------------- |
| **C#**                        | Programming language             |
| **.NET 10**                   | Application framework            |
| **ASP.NET Core Web API**      | REST API development             |
| **Entity Framework Core**     | ORM / database access            |
| **SQL Server**                | Relational database              |
| **ASP.NET Core Identity**     | User management                  |
| **JWT Bearer Authentication** | Authentication and authorization |
| **AutoMapper**                | Entity ↔ DTO mapping             |
| **Scalar**                    | Interactive API documentation    |
| **OpenAPI**                   | API specification                |
| **API Versioning**            | Versioned API endpoints          |

---

## 🏗️ Architecture

The application follows a layered approach to separate HTTP handling, business logic, data access, and application models.

```text
                         Client
                           │
                           │ HTTP / JSON
                           ▼
                  ┌──────────────────┐
                  │    Controllers   │
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │     Services     │
                  │                  │
                  │ AuthService      │
                  │ TokenService     │
                  │ ImageService     │
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │    EF Core       │
                  │ ApplicationContext│
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐
                  │    SQL Server    │
                  └──────────────────┘
```

### Main components

**Controllers**

Handle HTTP requests, validate input, invoke application functionality, and return appropriate HTTP responses.

**Services**

Contain reusable application logic such as authentication, JWT token generation, and image management.

**DTOs**

Define the API request and response contracts without directly exposing database entities.

**Models**

Represent the application's domain/database entities.

**Data**

Contains `ApplicationContext`, database configuration, migrations, and database-related extensions.

---

## 🔐 Authentication & Authorization

The API uses **ASP.NET Core Identity** for user management and **JWT Bearer Authentication** for securing protected endpoints.

### Authentication flow

```text
             User
              │
              │ Register
              ▼
      ASP.NET Core Identity
              │
              ▼
        User stored
        in SQL Server
              
              │
              │ Login
              ▼
      Validate credentials
              │
              ▼
        Generate JWT
              │
              ▼
        Return JWT
              │
              ▼
      Client sends token
              
Authorization: Bearer <token>
              │
              ▼
      Protected API endpoint
```

JWT tokens are validated using:

* Signature validation
* Token lifetime validation
* Bearer authentication
* Symmetric security key

---

## 🏡 Villa Management

The Villa API supports CRUD operations for resort villas.

Typical operations include:

```text
GET     /api/villas
GET     /api/villas/{id}
POST    /api/villas
PUT     /api/villas/{id}
DELETE  /api/villas/{id}
```

Villa information includes details such as:

* Name
* Description/details
* Price
* Square footage
* Occupancy
* Image
* Created date
* Updated date

The API also supports filtering and sorting through query parameters where implemented.

---

## 🛎️ Villa Amenities

Amenities are managed separately from villas and maintain a relationship with the corresponding villa.

Example operations:

```text
GET     /api/vill-amenities
GET     /api/vill-amenities/{id}
POST    /api/vill-amenities
PUT     /api/vill-amenities/{id}
DELETE  /api/vill-amenities/{id}
```

The API validates that the associated Villa exists before creating or updating an amenity.

---

## 🖼️ Image Upload

Villa images are handled through a dedicated `IImageService` abstraction and `ImageService` implementation.

The current implementation:

* Accepts `.jpg`, `.jpeg`, and `.png`
* Limits uploads to 5 MB
* Generates unique filenames using `Guid`
* Stores uploaded images under:

```text
wwwroot/images/villas
```

The database stores the corresponding image path/URL.

### Upload flow

```text
Client
  │
  │ Multipart form-data
  ▼
Villa API
  │
  ▼
IImageService
  │
  ├── Validate file
  ├── Validate extension
  ├── Validate file size
  └── Generate unique filename
          │
          ▼
wwwroot/images/villas
```

---

## 🗄️ Database

The application uses **Entity Framework Core with SQL Server**.

Development uses SQL Server LocalDB.

The database contains entities required for:

* Villas
* Villa amenities
* Application users
* ASP.NET Core Identity
* Other application data

Entity Framework Core migrations are included in the repository to track database schema changes.

---

## 🌱 Database Seeding

The application includes initial villa data for development/testing.

Example villas include:

* Royal Villa
* Diamond Villa
* Pool Villa
* Luxury Villa
* Garden Villa

The seeding logic prevents the initial villa records from being inserted repeatedly when data already exists.

---

## 🔄 API Versioning

API versioning is implemented using the ASP.NET API Versioning libraries.

The application supports versioned API documentation through OpenAPI and Scalar.

This allows future versions of the API to be introduced without immediately breaking existing clients.

---

## 📚 API Documentation

The API uses **Scalar** as an interactive API documentation interface based on the generated OpenAPI specification.

After running the application locally, open:

```text
https://localhost:<port>/scalar
```

Scalar can be used to:

* Explore endpoints
* View request/response models
* Send API requests
* Test authentication
* Add JWT bearer tokens
* Inspect HTTP responses

### Example

```text
Scalar
   │
   ▼
OpenAPI
   │
   ▼
ASP.NET Core Controllers
   │
   ▼
API
```

---

## 📁 Project Structure

```text
Resort Booking Application/
│
├── ResortBooking.API/
│   │
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── AmenitiesController.cs
│   │   ├── VillaController.cs
│   │   └── ...
│   │
│   ├── Data/
│   │   ├── ApplicationContext.cs
│   │   └── DataExtensions.cs
│   │
│   ├── Dtos/
│   │   ├── CreateVillaDto.cs
│   │   ├── UpdateVillaDto.cs
│   │   ├── VillaDetailsDto.cs
│   │   └── ...
│   │
│   ├── Models/
│   │   ├── Villa.cs
│   │   ├── VillaAmenities.cs
│   │   ├── ApplicationUser.cs
│   │   └── ...
│   │
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── ImageService.cs
│   │   ├── TokenService.cs
│   │   └── IServices/
│   │
│   ├── Migrations/
│   │
│   ├── wwwroot/
│   │   └── images/
│   │       └── villas/
│   │
│   ├── Program.cs
│   ├── appsettings.json
│   └── ResortBooking.API.csproj
│
├── .gitignore
├── README.md
└── Resort Booking Application.slnx
```

---

## ⚙️ Getting Started

### Prerequisites

Install the following:

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* SQL Server or SQL Server LocalDB
* Visual Studio 2022 or VS Code
* Git

---

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/resort-booking-api.git
```

Navigate into the project:

```bash
cd resort-booking-api
```

---

### 2. Restore dependencies

```bash
dotnet restore
```

---

### 3. Configure the database

The default development configuration uses SQL Server LocalDB.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=ResortBooking;TrustServerCertificate=True;Trusted_Connection=True;"
  }
}
```

Update the connection string if your SQL Server configuration is different.

---

### 4. Configure JWT

Configure a development JWT secret using .NET User Secrets or another secure configuration mechanism.

Example:

```bash
dotnet user-secrets init
```

Then:

```bash
dotnet user-secrets set "JwtSettings:Secret" "your-development-secret"
```

Do not commit production secrets, passwords, API keys, or other credentials to source control.

---

### 5. Apply database migrations

If migrations have not already been applied to your local database:

```bash
dotnet ef database update
```

---

### 6. Run the API

```bash
dotnet run
```

The application will start on the configured HTTP/HTTPS development ports.

---

## 🧪 Testing the API

After starting the application, open Scalar:

```text
https://localhost:<port>/scalar
```

From Scalar you can test the available endpoints.

A typical authentication workflow is:

```text
1. Register user
      ↓
2. Login
      ↓
3. Receive JWT
      ↓
4. Authorize using Bearer token
      ↓
5. Access protected endpoints
```

---

## 📌 Example API Response

Example villa response:

```json
{
  "id": 1,
  "name": "Royal Villa",
  "details": "Luxurious villa with stunning ocean views and private beach access.",
  "price": 500,
  "sqft": 2500,
  "occupancy": 6
}
```

Actual response structure may include the application's standard `ApiResponse` wrapper and additional properties.

---

## 🔎 Engineering Practices

The project demonstrates several common ASP.NET Core development practices:

### DTOs

DTOs are used to define API contracts instead of exposing Entity Framework entities directly.

### Dependency Injection

Services and `ApplicationContext` are registered through ASP.NET Core's built-in dependency injection container.

### Asynchronous Programming

Database operations use asynchronous EF Core APIs such as:

```csharp
ToListAsync()
FirstOrDefaultAsync()
FindAsync()
SaveChangesAsync()
```

### Read-only Queries

`AsNoTracking()` is used for read-only queries where entity tracking isn't required.

### AutoMapper

AutoMapper is used to convert between entities and DTOs.

### API Response Wrapper

The API uses a common response structure to provide consistent response information across endpoints.

---

## 🔮 Future Improvements

Potential improvements for future versions include:

* React frontend
* Online deployment
* Cloud-based image storage using Azure Blob Storage
* Global exception handling middleware
* Structured logging
* Automated unit and integration tests
* Docker containerization
* CI/CD pipeline
* Redis caching
* Advanced booking and reservation workflows
* Payment integration
* Email notifications

---

## 🎯 Project Purpose

This project was built to demonstrate practical backend development using the **ASP.NET Core ecosystem**, including REST API development, database access, authentication, authorization, API documentation, and application architecture.

It also serves as the backend foundation for a future React-based resort booking application.

---

## 👨‍💻 Author

**Sai Abhiroop Ravella**

Backend / Software Development Portfolio Project
