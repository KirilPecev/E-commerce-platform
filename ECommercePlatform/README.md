# ECommercePlatform

ECommercePlatform is a modular e‑commerce reference application implemented with .NET 10. The solution is built as a set of independent microservices plus an API gateway and includes multiple test projects. This README documents exact project names, ports, Docker Compose configuration, development steps, testing, and troubleshooting.

## Projects (top-level folders and project files)

- ApiGateway/ (ApiGateway.csproj) - API Gateway / entry point
- IdentityService/ (IdentityService.csproj)
- CatalogService/ (CatalogService.csproj)
- InventoryService/ (InventoryService.csproj)
- OrderService/ (OrderService.csproj)
- PaymentService/ (PaymentService.csproj)
- ECommercePlatform/ (ECommercePlatform.csproj) - aggregator / solution project

Test projects (located in repository or under test folders):
- ECommercePlatform.Tests (CatalogService.Tests, OrderService.Tests, InventoryService.Tests, PaymentService.Tests, IdentityService.Tests, Architecture.Tests, etc.)

## Ports (Docker Compose mappings)

The repository includes docker-compose.yml at the repository root. The compose file maps container internal port 8080 for each .NET service to the host ports below:

- api-gateway -> host:5000 -> container:8080
- identity    -> host:5001 -> container:8080
- catalog     -> host:5002 -> container:8080
- inventory   -> host:5003 -> container:8080
- order       -> host:5004 -> container:8080
- payment     -> host:5005 -> container:8080

Other infrastructure ports exposed by compose:
- SQL Server -> 1433
- RabbitMQ -> 5672 (AMQP), 15672 (management UI)
- Redis -> 6379

Container names (configured in docker-compose.yml): sqlserver, rabbitmq, redis, api-gateway, identity, catalog, inventory, order, payment.

Persistent volumes defined in docker-compose.yml: rabbitmq_data, data-protection, sqldata, redis_data.

## Docker Compose (location and notes)

- Path: ./docker-compose.yml (repository root)
- Dockerfiles exist for each service: ApiGateway/Dockerfile, IdentityService/Dockerfile, CatalogService/Dockerfile, InventoryService/Dockerfile, OrderService/Dockerfile, PaymentService/Dockerfile.
- The compose file references ./Common.env for environment variables. Ensure Common.env exists and contains required secrets (SA password, RabbitMQ user, etc.) before starting the stack.

Bring the full environment up:

  docker-compose up --build

To run detached:

  docker-compose up --build -d

To stop and remove containers:

  docker-compose down

To build a single service image locally (example: ApiGateway):

  docker build -f ApiGateway/Dockerfile -t ecommerce/api-gateway:local ./ECommercePlatform

Adjust context and dockerfile path as needed (compose uses context: ./ECommercePlatform).

## Development (run services locally without Docker)

Prerequisites: .NET 10 SDK installed.

1) Restore and build

  pwsh -Command "dotnet restore && dotnet build"

2) Run a single service (example: IdentityService)

  pwsh -Command "dotnet run --project IdentityService/IdentityService.csproj"

3) Run API gateway (example)

  pwsh -Command "dotnet run --project ApiGateway/ApiGateway.csproj"

Notes:
- When running services locally you must provide working backing services (SQL Server, RabbitMQ, Redis) or point connection strings to remote instances. Connection strings and other config are read from appsettings.json and environment variables. See each project's Properties/launchSettings.json and appsettings.json for defaults.
- Example environment variable to set a connection string in PowerShell (replace values accordingly):

  $env:ConnectionStrings__CatalogDb = "Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=yourStrongPassword12!;TrustServerCertificate=True"

Start the dependent infrastructure (SQL, RabbitMQ, Redis) via Docker if you don't have them locally:

  docker-compose up -d data rabbitmq redis

Then run services locally.

## Testing

Run all tests (will discover test projects):

  pwsh -Command "dotnet test"

Or run a specific test project:

  pwsh -Command "dotnet test CatalogService.Tests/CatalogService.Tests.csproj"

## Configuration files

- Each service contains appsettings.json and Properties/launchSettings.json for local development. When running with Docker, docker-compose.yml and Common.env drive configuration and secrets.

Search/modify configuration values in these files when changing ports, connection strings, or external integrations.

## Troubleshooting

- If docker-compose fails due to missing Common.env: copy or create Common.env at the repository root and populate required secrets (SA password, RabbitMQ credentials, Redis settings).
- SQL Server initialization: the compose file mounts a volume for persistent data. If DB connections fail, check the container logs (docker logs sqlserver) and confirm the SA password matches Common.env.
- If a service fails on startup, check its container logs (docker logs <container_name>) or run the project locally to see the exception.

## Contributing

1. Fork the repository and create a branch
2. Add or update tests
3. Submit a pull request with a clear description and changelog

## License

This repository does not include an explicit license file. Add a LICENSE if you plan to publish this project.

---

If you want, I can also add a sample Common.env.template (without secrets) or populate the README with exact launchSettings.json ports for each project. Tell me which you prefer.