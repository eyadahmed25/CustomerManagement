# Customer Management API

A professional-grade ASP.NET Core REST API demonstrating enterprise middleware architecture patterns with external service integrations. This project serves as both a learning vehicle and a reusable architectural reference for building production-ready backend APIs.

## Project Overview

The Customer Management API is a complete backend system that showcases how modern middleware applications orchestrate operations across multiple data sources including databases and external REST APIs. The project follows industry-standard three-layer architecture patterns enhanced with a dedicated external services integration layer, demonstrating the exact patterns used in enterprise backend development.

This API manages customer records with full CRUD operations while integrating real-time phone validation through Twilio's Lookup API and automated email notifications through SendGrid's Email API. The architecture demonstrates how service layers orchestrate between internal databases and external third-party services, which is the foundational pattern used in enterprise middleware applications.

## Architecture and Design Principles

The application is built on a four-layer architecture where each layer has a single, well-defined responsibility. This separation of concerns makes the codebase maintainable, testable, and scalable as requirements grow.

The **Repository Layer** (CustomerManagement.Repository) handles all database communication through stored procedures and Dapper ORM. This layer abstracts database operations behind interfaces, providing a clean contract for data access without exposing SQL implementation details to higher layers. The repository speaks only in terms of domain models and has no knowledge of HTTP, business rules, or data transfer objects.

The **Services Layer** (CustomerManagement.Services) contains all business logic and orchestration. This is the brain of the application where validation rules are enforced, data transformations occur, and operations across multiple data sources are coordinated. The services layer depends on both the repository layer for database operations and the external services layer for third-party API calls, treating them as equivalent data sources abstracted behind interfaces.

The **External Services Layer** (CustomerManagement.ExternalServices) handles all communication with external REST APIs including Twilio and SendGrid. This layer mirrors the repository layer's structure but for HTTP API calls instead of database operations. It manages HTTP client configuration, authentication, JSON serialization, and error handling for external service integrations. Each external service is abstracted behind an interface, making the implementations swappable and testable.

The **API Layer** (CustomerManagement.API) exposes HTTP endpoints following RESTful conventions. Controllers handle routing, status codes, and content negotiation but contain no business logic. The API layer depends only on the services layer through interfaces, remaining completely decoupled from implementation details of how data is retrieved or where it comes from.

This architecture follows the dependency inversion principle where high-level modules depend on abstractions rather than concrete implementations. Every layer registers its services with the dependency injection container through extension methods, making dependencies explicit and enabling flexible configuration during application startup.

## Technology Stack

The application is built with .NET 8.0 LTS using modern C# language features including nullable reference types, required properties, and async/await patterns throughout. The API layer uses ASP.NET Core Web API with Swagger/OpenAPI documentation generated through Swashbuckle for interactive endpoint testing and documentation.

Data access is implemented with Dapper as a lightweight, high-performance ORM that executes stored procedures and maps results to domain models. The database is SQL Server with all operations going through stored procedures rather than inline SQL, providing performance benefits through query plan caching and enabling database administrators to optimize queries independently from application code.

External service integration uses HttpClient with the factory pattern provided by Microsoft.Extensions.Http, avoiding socket exhaustion issues and enabling proper HttpClient lifecycle management. Object-to-object mapping between DTOs and domain models uses AutoMapper for internal transformations, while external service data transformations are done manually to maintain clear boundaries between systems.

Configuration management uses the Options pattern with strongly-typed classes bound from appsettings.json, providing compile-time safety and clean separation of configuration from code. Dependency injection is implemented throughout using Microsoft's built-in container, with all services registered through extension methods that follow a consistent pattern across layers.

## Initial Implementation (Foundation)

The initial implementation established the foundational three-layer architecture with complete CRUD operations for customer management. The database schema includes a Customer table with fields for personal information including name, nationality, email, phone, date of birth, blood group, and salary. The table uses soft deletes with an IsActive flag rather than physically removing records, and includes system-managed fields like CustomerID as a GUID primary key and CreatedDate for audit tracking.

Five stored procedures handle all database operations following a consistent naming pattern. The GetAll procedure retrieves customers with optional filtering by active status and returns results ordered by creation date. GetById retrieves a single customer by their unique identifier. Create inserts new customer records and returns the complete record including database-generated values. Update modifies existing customer information after verifying the customer exists. Delete performs a soft delete by setting IsActive to false rather than removing the record permanently.

The repository layer implements the repository pattern with an interface defining the contract and a concrete implementation using Dapper to execute stored procedures. Each repository method is asynchronous, returns appropriate types wrapped in Task, and handles the creation of SQL connections using connection strings provided through dependency injection.

The services layer enforces business validation rules including email format validation using regular expressions, duplicate email checking by querying all customers, age validation requiring customers to be at least eighteen years old, and nationality validation ensuring the field is not empty. The service layer uses AutoMapper to transform between AddCustomerDTO for input, Customer domain models for internal processing, and CustomerDTO for output. System-managed fields like CustomerID, IsActive, and CreatedDate are set by the service layer rather than accepting values from external clients.

The API layer exposes five RESTful endpoints following standard HTTP conventions. GET requests to the collection endpoint return all customers with optional query parameters for filtering. GET requests with an ID path parameter return a single customer or 404 if not found. POST requests to the collection endpoint create new customers and return 201 Created with the resource location. PUT requests with an ID update existing customers and return the updated resource or 404 if not found. DELETE requests with an ID perform soft deletes and return 204 No Content on success.

## Phase 1: External API Integration

Phase 1 transformed the application from a database-only CRUD API into a sophisticated middleware orchestrator that coordinates operations across multiple external systems. This phase demonstrates the real-world pattern where backend APIs integrate with third-party services to enhance functionality and validate data in real-time.

### Twilio Phone Validation Integration

The Twilio integration adds real-time phone number validation using Twilio's Lookup API, ensuring that only legitimate phone numbers are stored in the customer database. When a customer is created or updated with a phone number, the system calls Twilio's API to verify the number exists in the global phone network and is properly formatted.

The TwilioClient implementation uses HTTP Basic Authentication with Account SID and Auth Token credentials provided through configuration. The client makes HTTP GET requests to Twilio's Lookup endpoint with phone numbers in E.164 international format, deserializes JSON responses to extract validation results, and maps Twilio's response structure to internal PhoneValidationResponse DTOs. The implementation includes comprehensive error handling for network failures, API errors, and JSON parsing issues.

Phone validation is integrated into the customer creation and update workflows in the CustomerService. Before saving a customer to the database, the service calls the Twilio client to validate the provided phone number. If validation fails, the service throws an ArgumentException with a descriptive error message, preventing the customer from being created with invalid data. This fail-fast approach ensures data quality and provides immediate feedback to API clients about why their request was rejected.

The phone validation is optional, meaning customers can be created without providing a phone number, but if a phone number is provided it must pass validation. This design accommodates business scenarios where phone numbers are useful but not strictly required while still maintaining data quality when they are provided.

### SendGrid Email Notification Integration

The SendGrid integration adds automated welcome email notifications sent to customers immediately after their accounts are successfully created. Using SendGrid's Email API, the system sends professionally formatted emails with both plain text and HTML content to ensure compatibility across all email clients.

The SendGridClient implementation uses Bearer token authentication with an API key provided through configuration. The client constructs HTTP POST requests to SendGrid's mail send endpoint with JSON payloads matching SendGrid's required structure, including personalization data, sender information, subject lines, and email content. The implementation transforms internal EmailRequest DTOs into SendGrid's specific JSON format, demonstrating how middleware applications map between different API contracts.

Two methods are provided in the ISendGridClient interface showing different levels of abstraction. SendWelcomeEmailAsync is a high-level convenience method that accepts only an email address and first name, automatically constructing a pre-formatted welcome email with a fixed template. SendEmailAsync is a lower-level flexible method that accepts a complete EmailRequest, allowing custom subjects, bodies, and content for different email types in future enhancements.

Email sending is implemented as a fire-and-forget operation using Task.Run, meaning the customer creation workflow initiates the email send but does not wait for it to complete before returning the response. This design prevents slow email delivery or SendGrid service delays from impacting API response times. The email operation runs in a background task with comprehensive exception handling to ensure email failures do not crash the application or cause customer creation to fail.

### Service Layer Orchestration

The CustomerService now demonstrates sophisticated middleware orchestration by coordinating operations across three completely different systems. When creating a customer, the service executes a carefully ordered sequence of operations. First, it performs local validation checks like email format validation that require no external calls. Second, if a phone number was provided, it calls the Twilio client to validate the number exists in the phone network, throwing an exception immediately if validation fails. Third, it queries the database through the repository to check for duplicate email addresses. Fourth, it uses AutoMapper to transform the input DTO to a domain model and sets system-managed fields. Fifth, it calls the repository to save the customer to the database. Sixth, it initiates a background task to send a welcome email through SendGrid. Finally, it transforms the created customer to an output DTO and returns it.

This orchestration demonstrates how the service layer treats database operations and external API calls equivalently. Both ICustomerRepo and ITwilioClient are dependencies injected through the constructor, both hide implementation details behind interfaces, and both return data that the service uses to make business decisions. The service layer contains the intelligence to coordinate these different data sources appropriately based on business requirements.

Error handling is implemented at multiple levels with different strategies depending on the operation's criticality. Phone validation errors and database errors are synchronous and cause the entire operation to fail with appropriate exception types. Email sending errors are caught in the background task and logged but do not affect the success of customer creation since email notification is a nice-to-have side effect rather than a critical operation.

### Configuration and Security

All external service credentials are managed through the Options pattern with strongly-typed configuration classes. TwilioSettings contains properties for AccountSid, AuthToken, and BaseUrl that are bound from the Twilio section in appsettings.json. SendGridSettings contains properties for ApiKey, FromEmail, FromName, and BaseUrl bound from the SendGrid section.

The main appsettings.json file contains placeholder values showing what configuration is required without including actual credentials. A separate appsettings.Development.json file contains real API keys for local development and is excluded from source control through gitignore to prevent credential leakage. This configuration hierarchy allows environment-specific settings to override base values, following the standard pattern used across .NET applications.

HttpClient instances are configured using the factory pattern through AddHttpClient registration, which manages client lifecycles properly and enables named clients with different configurations for different services. Authentication headers are set as default headers in client constructors, so every request automatically includes proper credentials without requiring manual header management in individual method calls.

## API Endpoints

The API exposes five endpoints for customer management, all documented through Swagger UI available at the root URL when running in development mode.

**GET /api/customers** retrieves all customers with an optional query parameter for filtering by active status. The endpoint returns 200 OK with an array of CustomerDTO objects or 404 Not Found if no customers exist matching the criteria. The default behavior shows only active customers unless explicitly requested otherwise.

**GET /api/customers/{id}** retrieves a single customer by their unique identifier. The endpoint returns 200 OK with the CustomerDTO if found or 404 Not Found with a descriptive message if the customer does not exist. The ID must be a valid GUID format.

**POST /api/customers** creates a new customer with phone validation and email notification. The request body must contain an AddCustomerDTO with required fields including FirstName, LastName, Nationality, and Email. Optional fields include Phone, DateOfBirth, BloodGroup, and Salary. If a phone number is provided, it must pass Twilio validation or the request fails with 400 Bad Request. On success, returns 201 Created with the Location header pointing to the new customer resource and the complete CustomerDTO in the response body. A welcome email is sent asynchronously after successful creation.

**PUT /api/customers/{id}** updates an existing customer with phone validation. The request body contains an AddCustomerDTO with the updated values. The endpoint validates the phone number if provided, checks for email conflicts with other customers, and returns 200 OK with the updated CustomerDTO on success. Returns 404 Not Found if the customer does not exist or 400 Bad Request if validation fails.

**DELETE /api/customers/{id}** performs a soft delete by setting the customer's IsActive flag to false. The customer remains in the database for audit purposes but is excluded from default queries. Returns 204 No Content on success or 404 Not Found if the customer does not exist.

## Setup and Configuration

To run the application locally, you need Visual Studio 2022 or later with the ASP.NET and web development workload installed. The application targets .NET 8.0 LTS which must be installed on your development machine. You also need SQL Server or SQL Server Express for the database, and accounts with Twilio and SendGrid to obtain API credentials for external service integration.

Start by cloning the repository and opening the solution file in Visual Studio. The solution contains four projects that should all load automatically. Create a new database in SQL Server Management Studio named CustomerManagementDB and execute the table creation script followed by all five stored procedure scripts found in the project documentation to set up the database schema.

Create an appsettings.Development.json file in the CustomerManagement.API project if it does not already exist. This file should contain your Twilio credentials including AccountSid and AuthToken, your SendGrid credentials including ApiKey, FromEmail, and FromName, and your SQL Server connection string. Make sure this file is in your gitignore to prevent committing credentials to source control.

Build the solution to restore NuGet packages and verify all projects compile successfully. Set CustomerManagement.API as the startup project and run the application. The browser should open to the Swagger UI where you can test all endpoints interactively. Create test customers with valid phone numbers to see the complete flow including Twilio validation and SendGrid email delivery.

## Project Structure

The solution is organized into four projects with clear separation of concerns and explicit dependency relationships through project references.

CustomerManagement.API is the entry point containing controllers that expose HTTP endpoints, Program.cs that configures the application and dependency injection, and appsettings files for configuration management. This project references the Services and ExternalServices projects.

CustomerManagement.Services contains business logic and orchestration with DTOs defining input and output shapes, service interfaces and implementations that coordinate operations, AutoMapper profiles for DTO transformations, and dependency injection registration. This project references both Repository and ExternalServices projects.

CustomerManagement.Repository handles database operations with domain models that match database table structures, repository interfaces and implementations using Dapper, connection string configuration, and dependency injection registration. This project has no references to other application projects.

CustomerManagement.ExternalServices manages external API integrations with client interfaces and implementations for Twilio and SendGrid, DTOs organized by service in separate subfolders, configuration classes for API credentials, and dependency injection registration with HttpClient factory configuration. This project has no references to other application projects.

## Key Learning Outcomes

This project demonstrates professional backend development practices that transfer directly to enterprise middleware applications. The four-layer architecture with clear separation of concerns shows how to structure applications that remain maintainable as complexity grows. The interface-based design throughout enables testing with mock implementations and allows swapping implementations without changing dependent code.

The project shows how dependency injection works across multiple layers with services registered through extension methods that follow consistent patterns. The use of the Options pattern for configuration demonstrates how to manage credentials securely and support environment-specific settings without hardcoding values.

The external service integrations demonstrate the complete lifecycle of REST API integration including HttpClient configuration with factory pattern, authentication header management, JSON serialization and deserialization, error handling for network failures and API errors, and mapping between external API contracts and internal data structures.

The service layer orchestration shows how to coordinate operations across databases and external APIs, treating them as equivalent data sources abstracted behind interfaces. The fire-and-forget pattern for email notifications demonstrates how to handle side effects that should not block main operation flows. The validation patterns including fail-fast with exceptions and comprehensive error messaging show how to maintain data quality while providing useful feedback.

## Future Enhancements

Planned enhancements include authentication and authorization using JWT tokens to secure endpoints, comprehensive unit and integration testing with mock implementations of external services, structured logging with correlation IDs for request tracing, pagination and filtering for the GetAll endpoint to handle large datasets efficiently, and additional entities with relationships to demonstrate one-to-many and many-to-many patterns.

The architecture is designed to accommodate these enhancements without requiring fundamental restructuring. The layered design, interface abstractions, and dependency injection foundation provide flexibility to add new features while maintaining the separation of concerns that makes the codebase maintainable.

## Contributing

This project serves as a learning reference and architectural template. The patterns demonstrated here are intentionally generic and reusable across different business domains. When building similar APIs, use this project as a starting point and adapt the entity models, business rules, and validation logic to match your specific requirements while maintaining the same architectural patterns.

## License

This project is created for educational purposes and demonstrates industry-standard patterns for building professional backend APIs with external service integrations.
