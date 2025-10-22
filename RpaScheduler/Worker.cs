using Microsoft.Extensions.Options;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;
using Shared.Repositories;
using Shared.Services;

namespace RpaScheduler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RPA Scheduler Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scope for dependency injection
                using var scope = _serviceProvider.CreateScope();
                
                // Get required services
                var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                var barcodeService = scope.ServiceProvider.GetRequiredService<IBarcodeService>();
                
                // Seed barcodes if needed (only once)
                await SeedBarcodesIfNeeded(barcodeService, scope);
                
                // Handle expired jobs
                await HandleExpiredJobs(jobService, scope);
                
                _logger.LogInformation("Scheduler iteration completed at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during scheduler iteration");
            }
            
            // Wait for 30 seconds before next iteration
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task SeedBarcodesIfNeeded(IBarcodeService barcodeService, IServiceScope scope)
    {
        // This is a simplified check. In a real application, you might want to check 
        // the actual count of available barcodes in the database
        try
        {
            var availableBarcodes = await barcodeService.GetAvailableBarcodesAsync(1);
            if (!availableBarcodes.Any())
            {
                _logger.LogInformation("Seeding barcodes...");
                await barcodeService.SeedBarcodesAsync(1000); // Seed 1000 barcodes
                _logger.LogInformation("Barcodes seeded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding barcodes");
        }
    }

    private async Task HandleExpiredJobs(IJobService jobService, IServiceScope scope)
    {
        try
        {
            var expiredJobs = await jobService.GetExpiredJobsAsync();
            _logger.LogInformation("Found {Count} expired jobs", expiredJobs.Count());
            
            foreach (var job in expiredJobs)
            {
                _logger.LogInformation("Handling expired job: {JobId}, Retry count: {RetryCount}/{MaxRetries}", 
                    job.Id, job.RetryCount, job.MaxRetries);
                
                // Check if job has exceeded max retries
                if (job.RetryCount >= job.MaxRetries)
                {
                    // Mark job as failed permanently
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = $"Job failed after {job.MaxRetries} retries";
                    job.CompletedAt = DateTime.UtcNow;
                    job.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogWarning("Job {JobId} marked as failed after {MaxRetries} retries", 
                        job.Id, job.MaxRetries);
                }
                else
                {
                    // Reset the job status to pending for retry
                    job.Status = JobStatus.Pending;
                    job.ClientId = null;
                    job.StartedAt = null;
                    job.RetryCount++;
                    job.ErrorMessage = $"Job expired - retry {job.RetryCount}/{job.MaxRetries}";
                    job.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Expired job {JobId} reset to pending for retry {RetryCount}/{MaxRetries}", 
                        job.Id, job.RetryCount, job.MaxRetries);
                }
                
                // Update job in repository
                var jobRepository = scope.ServiceProvider.GetRequiredService<IRpaJobRepository>();
                await jobRepository.UpdateAsync(job);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling expired jobs");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RPA Scheduler Worker stopped at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }
}