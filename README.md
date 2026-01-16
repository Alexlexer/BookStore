# BookStore API

Technical test implementation. This project is a REST API built with .NET 10 and SQL Server to manage books, authors, and illustrators.

## Features

- RESTful API for Book management.
- Data persistence using Entity Framework Core and SQL Server.
- Validation logic (ISBN, dates, duplicates).
- Sorting and filtering capabilities.
- Docker support for easy deployment.
- Unit and Integration tests.

## Prerequisites

- Docker Desktop (Recommended)
- OR .NET 10 SDK and a local SQL Server instance.

## How to Run (Docker)

This is the fastest way to run the application without installing dependencies.

1. Open a terminal in the root directory.
2. Run the following command:
   docker-compose up --build
3. Access the application:
   - Swagger UI: http://localhost:8080/swagger
   - API Endpoint: http://localhost:8080/api/books

## How to Run (Manual)

1. Ensure the connection string in `BookStore.Api/appsettings.json` points to your local SQL Server.
2. Apply database migrations:
   dotnet ef database update --project BookStore.Api
3. Start the application:
   dotnet run --project BookStore.Api

## Testing

The solution includes Unit Tests for business logic and Integration Tests for API endpoints. To run all tests:

dotnet test

## Project Structure

- BookStore.Api: Main application, Controllers, Data Access.
- BookStore.Domain: Entities, Enums, DTOs.
- BookStore.UnitTests: Tests for validation logic.
- BookStore.IntegrationTests: End-to-end tests using WebApplicationFactory and In-Memory database.
