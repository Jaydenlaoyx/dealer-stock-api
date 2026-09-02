# Dealer Stock API

A C# web API for managing vehicle inventory across multiple car dealers.

The API allows authenticated dealers to manage their own vehicle stock while ensuring that they cannot access or modify inventory belonging to other dealers.

## Tech Stack

* .NET 8
* C#
* FastEndpoints
* Dapper
* SQLite
* JWT Authentication
* Swagger / OpenAPI

## Features

* Authenticate dealers using JWT
* Add cars to dealer inventory
* Remove cars from dealer inventory
* Retrieve a car by ID
* List cars and current stock levels
* Update car stock levels
* Search cars by make and/or model
* Validate incoming requests
* Prevent access to another dealer's inventory
* Prevent duplicate vehicle records within the same dealer inventory

## Getting Started

### Prerequisites

The .NET 8 SDK is required to run the application.

Verify your installed .NET version with:

```bash
dotnet --version
```

### Clone the Repository

```bash
git clone https://github.com/Jaydenlaoyx/dealer-stock-api
cd DealerStockApi
```

### Restore Dependencies

```bash
dotnet restore
```

### Run the Application

```bash
dotnet run
```

The application will initialize the SQLite database when it starts.

Once the application is running, open the Swagger URL displayed in the terminal to explore and test the API.

## Test Dealers

Two dealers are seeded into the database for testing:

| Dealer           | Username  | Password      |
| ---------------- | --------- | ------------- |
| Melbourne Motors | `dealer1` | `password123` |
| City Cars        | `dealer2` | `password123` |

Passwords are stored as hashes in the database rather than as plain text.

## Authentication

Most API endpoints require authentication.

Authenticate using:

```http
POST /api/auth/login
```

Example request:

```json
{
  "username": "dealer1",
  "password": "password123"
}
```

A successful request returns a JWT:

```json
{
  "token": "<JWT>",
  "dealerName": "Melbourne Motors"
}
```

The returned token should be supplied as a Bearer token when accessing protected endpoints.

## API Endpoints

| Method | Endpoint               | Description                                           |
| ------ | ---------------------- | ----------------------------------------------------- |
| POST   | `/api/auth/login`      | Authenticate a dealer and obtain a JWT                |
| GET    | `/api/auth/me`         | Get information about the authenticated dealer        |
| GET    | `/api/cars`            | List the authenticated dealer's cars and stock levels |
| GET    | `/api/cars/{id}`       | Retrieve a car by ID                                  |
| POST   | `/api/cars`            | Add a car to the authenticated dealer's inventory     |
| PUT    | `/api/cars/{id}/stock` | Update a car's stock level                            |
| DELETE | `/api/cars/{id}`       | Remove a car from inventory                           |
| GET    | `/api/cars/search`     | Search inventory by make and/or model                 |
| GET    | `/api/health`          | Check API health                                      |

### Search Example

Search by make:

```http
GET /api/cars/search?make=Audi
```

Search by model:

```http
GET /api/cars/search?model=A4
```

Search using both:

```http
GET /api/cars/search?make=Audi&model=A4
```

Searches are case-insensitive and support partial matching.

## Dealer Data Isolation

Each authenticated dealer is identified using the `DealerId` claim stored in their JWT.

The API does not accept a dealer ID from the client when performing inventory operations. Instead, the dealer ID is obtained from the authenticated user's token.

Database queries involving dealer-owned inventory include the authenticated dealer ID. For example:

```sql
SELECT
    Id,
    Make,
    Model,
    Year,
    StockLevel
FROM Cars
WHERE Id = @Id
  AND DealerId = @DealerId;
```

This prevents one dealer from accessing or modifying another dealer's inventory.

If a dealer attempts to access a car belonging to another dealer, the API returns `404 Not Found` rather than exposing whether the resource exists.

## Validation and Error Handling

Incoming requests are validated before they are processed.

Examples of validation include:

* Make and model are required when adding a car
* Make and model have maximum lengths
* Vehicle year must be within a sensible range
* Stock level cannot be negative
* A search must contain at least a make or model

The API uses appropriate HTTP status codes, including:

| Status             | Meaning                                                  |
| ------------------ | -------------------------------------------------------- |
| `200 OK`           | Request completed successfully                           |
| `201 Created`      | Car successfully created                                 |
| `204 No Content`   | Car successfully deleted                                 |
| `400 Bad Request`  | Request validation failed                                |
| `401 Unauthorized` | Authentication is required or failed                     |
| `404 Not Found`    | Requested car was not found for the authenticated dealer |
| `409 Conflict`     | A matching car already exists in the dealer's inventory  |

## Inventory Model

Each car record represents a make, model and year combination within a dealer's inventory.

For example:

```text
Make: Audi
Model: A4
Year: 2018
Stock Level: 5
```

`StockLevel` represents the number of vehicles of that type currently held by the dealer.

A dealer cannot create multiple inventory records with the same make, model and year combination. Different dealers can independently stock the same vehicle.

## Database

The application uses SQLite so that the project can be run and reviewed locally without requiring an external database server.

The database contains two main tables:

### Dealers

* `Id`
* `Name`
* `Username`
* `PasswordHash`

### Cars

* `Id`
* `DealerId`
* `Make`
* `Model`
* `Year`
* `StockLevel`

`DealerId` associates each car with its owning dealer.

Database access is implemented using Dapper and explicit SQL queries rather than an ORM.

## Project Structure

```text
DealerStockApi/
├── Data/
│   ├── DatabaseConnectionFactory.cs
│   └── DatabaseInitializer.cs
├── Extensions/
│   └── ClaimsPrincipalExtensions.cs
├── Features/
│   ├── Auth/
│   │   ├── Login/
│   │   └── Me/
│   ├── Cars/
│   │   ├── AddCar/
│   │   ├── DeleteCar/
│   │   ├── GetCar/
│   │   ├── ListCars/
│   │   ├── SearchCars/
│   │   └── UpdateStock/
│   └── Health/
├── Models/
│   ├── Car.cs
│   └── Dealer.cs
├── Program.cs
├── appsettings.json
└── dealerstock.db
```

The project uses a feature-based structure so that the request, response, validation and endpoint logic for each API operation remain grouped together.

## Security Notes

The JWT signing key included in `appsettings.json` is intended only for this development/take-home project.

In a production application, secrets such as signing keys should not be committed to source control and should instead be provided through environment variables or an appropriate secrets-management system.

The seeded dealer credentials are also intended for demonstration and testing purposes only.

## Design Decisions

The implementation intentionally keeps the architecture relatively simple.

FastEndpoints is used for defining API endpoints and request validation, while Dapper is used for explicit SQL database access as required by the assessment.

Dealer ownership is enforced at the SQL query level using the authenticated dealer's ID. This ensures that inventory operations are scoped to the current dealer.

SQLite is used to make the application straightforward for a reviewer to clone and run without additional database setup.

Automated integration tests were not included in the initial implementation due to the focused scope of the assessment. The API's authentication, validation, CRUD operations and cross-dealer isolation scenarios were manually verified during development.
