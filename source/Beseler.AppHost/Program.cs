var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("pgUsername");
var password = builder.AddParameter("pgPassword");

var postgres = builder.AddPostgres("postgres", username, password, port: 15432)
    .WithBindMount("../../data", "/docker-entrypoint-initdb.d");
var database = postgres.AddDatabase("Default", "bslr");

builder.AddContainer("dbdeploy", "abeseler/dbdeploy")
    .WithEnvironment("Deploy__Command", "update")
    .WithEnvironment("Deploy__StartingFile", "migrations.json")
    .WithEnvironment("Deploy__DatabaseProvider", "postgres")
    .WithEnvironment("Deploy__ConnectionString", $"Host=host.docker.internal;Port=15432;Database=bslr;Username={username.Resource.Value};Password={password.Resource.Value}")
    .WithEnvironment("Serilog__MinimumLevel__Default", "Debug")
    .WithBindMount("../../data", "/app/Migrations");

var apiService = builder.AddProject<Projects.Beseler_ApiService>("BeselerApi")
    .WithReference(database);

builder.AddProject<Projects.Beseler_Web>("BeselerWeb")
    .WithReference(apiService);

builder.Build().Run();
