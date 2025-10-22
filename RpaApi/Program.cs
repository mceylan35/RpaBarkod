using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Data;
using Shared.Interfaces;
using Shared.Repositories;
using Shared.Services;
using System.Text;
using Shared.DTOs;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MongoDB context
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(
        builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:password@localhost:27017",
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
builder.Services.AddScoped<IAuthenticationService>(sp =>
    new JwtService(
        sp.GetRequiredService<IRpaClientRepository>(),
        builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyHere12345YourSuperSecretKeyHere12345",
        int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "60")
    ));

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyHere12345YourSuperSecretKeyHere12345")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Authentication endpoint
app.MapPost("/api/auth", async (ClientAuthDto authDto, IAuthenticationService authService) =>
{
    try
    {
        var result = await authService.AuthenticateClientAsync(authDto);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("AuthenticateClient")
.WithOpenApi();

// Create job endpoint
app.MapPost("/api/jobs", async (JobRequestDto jobRequest, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        var job = await jobService.CreateJobAsync(jobRequest);
        return Results.Ok(job);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateJob")
.WithOpenApi()
.RequireAuthorization();

// Get pending jobs endpoint
app.MapGet("/api/jobs/pending", async (int limit, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        var jobs = await jobService.GetPendingJobsAsync(limit);
        return Results.Ok(jobs);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetPendingJobs")
.WithOpenApi()
.RequireAuthorization();

// Assign job to client endpoint
app.MapPost("/api/jobs/{jobId}/assign", async (string jobId, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        // Get client ID from claims
        var clientId = httpContext.User.FindFirst("clientId")?.Value;
        if (string.IsNullOrEmpty(clientId))
        {
            return Results.Unauthorized();
        }

        var job = await jobService.AssignJobToClientAsync(jobId, clientId);
        return Results.Ok(job);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("AssignJob")
.WithOpenApi()
.RequireAuthorization();

// Update job result endpoint
app.MapPost("/api/jobs/{jobId}/result", async (string jobId, JobResultDto jobResult, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        jobResult.JobId = jobId;
        await jobService.UpdateJobResultAsync(jobResult);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateJobResult")
.WithOpenApi()
.RequireAuthorization();

// Health check endpoint
app.MapGet("/api/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
})
.WithName("HealthCheck")
.WithOpenApi();

// Get job by ID endpoint
app.MapGet("/api/jobs/{jobId}", async (string jobId, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        var job = await jobService.GetJobByIdAsync(jobId);
        if (job == null)
        {
            return Results.NotFound();
        }
        return Results.Ok(job);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetJobById")
.WithOpenApi()
.RequireAuthorization();

// Get jobs by status endpoint
app.MapGet("/api/jobs/status/{status}", async (string status, IJobService jobService, HttpContext httpContext) =>
{
    try
    {
        if (!Enum.TryParse<JobStatus>(status, true, out var jobStatus))
        {
            return Results.BadRequest(new { error = "Invalid status" });
        }

        IEnumerable<RpaJob> jobs = jobStatus switch
        {
            JobStatus.Pending => await jobService.GetPendingJobsAsync(100),
            JobStatus.InProgress => await jobService.GetInProgressJobsAsync(),
            JobStatus.Expired => await jobService.GetExpiredJobsAsync(),
            _ => new List<RpaJob>()
        };

        return Results.Ok(jobs);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetJobsByStatus")
.WithOpenApi()
.RequireAuthorization();

// Get system statistics endpoint
app.MapGet("/api/stats", async (IJobService jobService, IBarcodeService barcodeService, HttpContext httpContext) =>
{
    try
    {
        var pendingJobs = await jobService.GetPendingJobsAsync(1000);
        var inProgressJobs = await jobService.GetInProgressJobsAsync();
        var expiredJobs = await jobService.GetExpiredJobsAsync();
        var availableBarcodes = await barcodeService.GetAvailableBarcodesAsync(1000);

        return Results.Ok(new
        {
            pendingJobsCount = pendingJobs.Count(),
            inProgressJobsCount = inProgressJobs.Count(),
            expiredJobsCount = expiredJobs.Count(),
            availableBarcodesCount = availableBarcodes.Count(),
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetSystemStats")
.WithOpenApi()
.RequireAuthorization();

app.Run();