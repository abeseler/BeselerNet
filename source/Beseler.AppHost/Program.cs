var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("pgUsername");
var password = builder.AddParameter("pgPassword");

var postgres = builder.AddPostgres("postgres", username, password, port: 15432)
    .WithBindMount("../../data", "/docker-entrypoint-initdb.d")
    .WithPgAdmin();

var database = postgres.AddDatabase("Default", "bslr");

var dbMigrator = builder.AddContainer("dbdeploy", "abeseler/dbdeploy")
    .WithEnvironment("Deploy__Command", "update")
    .WithEnvironment("Deploy__StartingFile", "migrations.json")
    .WithEnvironment("Deploy__DatabaseProvider", "postgres")
    .WithEnvironment("Deploy__ConnectionString", $"Host=host.docker.internal;Port=15432;Database=bslr;Username={username.Resource.Value};Password={password.Resource.Value}")
    .WithEnvironment("Deploy__ConnectionAttempts", "10")
    .WithEnvironment("Deploy__ConnectionRetryDelaySeconds", "1")
    .WithEnvironment("Serilog__MinimumLevel__Default", "Debug")
    .WithBindMount("../../data", "/app/Migrations")
    .WaitFor(database);

var apiService = builder.AddProject<Projects.Beseler_ApiService>("ApiService")
    .WithReference(database)
    .WaitForCompletion(dbMigrator);

builder.AddProject<Projects.Beseler_Web>("Web")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
