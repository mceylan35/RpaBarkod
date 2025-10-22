using Newtonsoft.Json;
using Shared.DTOs;
using Shared.Models;
using System.Net.Http.Headers;
using System.Text;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static string? authToken;
    private static readonly string baseUrl = "http://localhost:5121"; // Updated API URL
    private static string[] args = new string[0];

    static async Task Main(string[] mainArgs)
    {
        args = mainArgs; // Store args for use in other methods
        Console.WriteLine("RPA Mock Client Started");

        try
        {
            // Authenticate client
            await AuthenticateClient();

            if (string.IsNullOrEmpty(authToken))
            {
                Console.WriteLine("Authentication failed. Exiting.");
                return;
            }

            Console.WriteLine("Client authenticated successfully.");

            // Run the client loop
            await RunClientLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task AuthenticateClient()
    {
        try
        {
            // Get client ID from command line arguments or use default
            var clientId = args.Length > 0 ? args[0] : "test-client-1";
            var clientSecret = $"test-secret-{clientId.Split('-').Last()}";
            
            Console.WriteLine($"Authenticating as client: {clientId}");
            
            var authDto = new ClientAuthDto
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var json = JsonConvert.SerializeObject(authDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/auth", content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonConvert.DeserializeObject<AuthResponseDto>(responseContent);
                authToken = authResponse?.Token;

                if (!string.IsNullOrEmpty(authToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }
            }
            else
            {
                Console.WriteLine($"Authentication failed with status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during authentication: {ex.Message}");
        }
    }

    private static async Task RunClientLoop()
    {
        int consecutiveErrors = 0;
        const int maxConsecutiveErrors = 5;
        
        while (true)
        {
            try
            {
                // Check if we need to re-authenticate
                if (await IsTokenExpired())
                {
                    Console.WriteLine("Token expired, re-authenticating...");
                    await AuthenticateClient();
                    if (string.IsNullOrEmpty(authToken))
                    {
                        Console.WriteLine("Re-authentication failed. Exiting.");
                        return;
                    }
                }

                // Get pending jobs
                var jobs = await GetPendingJobs(5); // Get up to 5 pending jobs

                if (jobs != null && jobs.Any())
                {
                    Console.WriteLine($"Found {jobs.Count()} pending jobs. Processing...");
                    consecutiveErrors = 0; // Reset error counter on success

                    foreach (var job in jobs)
                    {
                        await ProcessJobWithRetry(job);
                    }
                }
                else
                {
                    Console.WriteLine("No pending jobs found. Waiting...");
                }

                // Wait before next iteration
                await Task.Delay(10000); // 10 seconds
            }
            catch (Exception ex)
            {
                consecutiveErrors++;
                Console.WriteLine($"Error in client loop (attempt {consecutiveErrors}/{maxConsecutiveErrors}): {ex.Message}");
                
                if (consecutiveErrors >= maxConsecutiveErrors)
                {
                    Console.WriteLine("Too many consecutive errors. Exiting client.");
                    return;
                }
                
                // Exponential backoff
                var delay = Math.Min(30000, 5000 * (int)Math.Pow(2, consecutiveErrors - 1));
                await Task.Delay(delay);
            }
        }
    }

    private static async Task<IEnumerable<RpaJob>?> GetPendingJobs(int limit)
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/api/jobs/pending?limit={limit}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jobs = JsonConvert.DeserializeObject<IEnumerable<RpaJob>>(content);
                return jobs;
            }
            else
            {
                Console.WriteLine($"Failed to get pending jobs: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting pending jobs: {ex.Message}");
            return null;
        }
    }

    private static async Task ProcessJobWithRetry(RpaJob job)
    {
        const int maxRetries = 3;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                await ProcessJob(job);
                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Error processing job {job.Id} (attempt {retryCount}/{maxRetries}): {ex.Message}");
                
                if (retryCount >= maxRetries)
                {
                    Console.WriteLine($"Job {job.Id} failed after {maxRetries} attempts");
                    await ReportJobFailure(job.Id, $"Failed after {maxRetries} attempts: {ex.Message}");
                }
                else
                {
                    // Wait before retry with exponential backoff
                    var delay = 2000 * (int)Math.Pow(2, retryCount - 1);
                    Console.WriteLine($"Retrying job {job.Id} in {delay}ms...");
                    await Task.Delay(delay);
                }
            }
        }
    }

    private static async Task ProcessJob(RpaJob job)
    {
        Console.WriteLine($"Processing job {job.Id} for store {job.StoreId}");

        // Assign job to this client
        var assignResponse = await client.PostAsync($"{baseUrl}/api/jobs/{job.Id}/assign", null);
        if (!assignResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to assign job {job.Id}: {assignResponse.StatusCode}");
        }

        // Simulate work (Windows app interaction)
        Console.WriteLine($"Simulating work for job {job.Id}...");
        await Task.Delay(new Random().Next(5000, 15000)); // Simulate 5-15 seconds of work

        // Simulate random success/failure
        var success = new Random().Next(0, 100) < 90; // 90% success rate

        var jobResult = new JobResultDto
        {
            JobId = job.Id,
            Status = success ? JobStatus.Completed : JobStatus.Failed,
            CompletedAt = DateTime.UtcNow,
            ErrorMessage = success ? null : "Simulated failure"
        };

        var json = JsonConvert.SerializeObject(jobResult);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var resultResponse = await client.PostAsync($"{baseUrl}/api/jobs/{job.Id}/result", content);
        if (!resultResponse.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update job result for {job.Id}: {resultResponse.StatusCode}");
        }

        Console.WriteLine($"Job {job.Id} completed with status: {jobResult.Status}");
    }

    private static async Task ReportJobFailure(string jobId, string errorMessage)
    {
        try
        {
            var jobResult = new JobResultDto
            {
                JobId = jobId,
                Status = JobStatus.Failed,
                CompletedAt = DateTime.UtcNow,
                ErrorMessage = errorMessage
            };

            var json = JsonConvert.SerializeObject(jobResult);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await client.PostAsync($"{baseUrl}/api/jobs/{jobId}/result", content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to report job failure: {ex.Message}");
        }
    }

    private static async Task<bool> IsTokenExpired()
    {
        try
        {
            // Try to make a simple API call to check if token is still valid
            var response = await client.GetAsync($"{baseUrl}/api/health");
            return !response.IsSuccessStatusCode;
        }
        catch
        {
            return true; // Assume expired if we can't check
        }
    }
}