using RpaScheduler;
using Shared.Data;
using Shared.Interfaces;
using Shared.Repositories;
using Shared.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add MongoDB context
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(
        builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017",
        builder.Configuration["DatabaseName"] ?? "raboidrpa"
    ));

// Add repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBarcodeRepository, BarcodeRepository>();
builder.Services.AddScoped<IRpaJobRepository, RpaJobRepository>();
builder.Services.AddScoped<IRpaClientRepository, RpaClientRepository>();

// Add services
builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<IJobService, JobService>();

// Add the worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();