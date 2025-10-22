using Shared.DTOs;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class JobService : IJobService
    {
        private readonly IRpaJobRepository _jobRepository;
        private readonly IProductRepository _productRepository;
        private readonly IBarcodeService _barcodeService;

        public JobService(IRpaJobRepository jobRepository, IProductRepository productRepository, IBarcodeService barcodeService)
        {
            _jobRepository = jobRepository;
            _productRepository = productRepository;
            _barcodeService = barcodeService;
        }

        public async Task<RpaJob> CreateJobAsync(JobRequestDto jobRequest)
        {
            // Get an available barcode
            var barcode = await _barcodeService.GetAvailableBarcodeAsync();
            if (barcode == null)
            {
                throw new InvalidOperationException("No available barcodes");
            }

            // Create product with the barcode
            var product = new Product
            {
                ProductCode = jobRequest.ProductCode,
                ProductName = jobRequest.ProductName,
                Price = jobRequest.Price,
                Barcode = barcode.Code,
                StoreId = jobRequest.StoreId
            };

            // Save product
            await _productRepository.CreateAsync(product);

            // Mark barcode as used
            await _barcodeService.MarkBarcodeAsUsedAsync(barcode.Code, jobRequest.StoreId);

            // Create job
            var job = new RpaJob
            {
                StoreId = jobRequest.StoreId,
                Product = product,
                Status = JobStatus.Pending
            };

            return await _jobRepository.CreateAsync(job);
        }

        public async Task<IEnumerable<RpaJob>> GetPendingJobsAsync(int limit)
        {
            return await _jobRepository.GetPendingJobsAsync(limit);
        }

        public async Task<RpaJob> AssignJobToClientAsync(string jobId, string clientId)
        {
            await _jobRepository.AssignJobToClientAsync(jobId, clientId);
            return await _jobRepository.GetByIdAsync(jobId);
        }

        public async Task UpdateJobResultAsync(JobResultDto jobResult)
        {
            var job = await _jobRepository.GetByIdAsync(jobResult.JobId);
            if (job != null)
            {
                job.Status = jobResult.Status;
                job.ErrorMessage = jobResult.ErrorMessage;
                job.CompletedAt = jobResult.CompletedAt;
                job.UpdatedAt = DateTime.UtcNow;
                
                await _jobRepository.UpdateAsync(job);
            }
        }

        public async Task<IEnumerable<RpaJob>> GetExpiredJobsAsync()
        {
            return await _jobRepository.GetExpiredJobsAsync();
        }

        public async Task<IEnumerable<RpaJob>> GetInProgressJobsAsync()
        {
            return await _jobRepository.GetInProgressJobsAsync();
        }

        public async Task<RpaJob> GetJobByIdAsync(string jobId)
        {
            return await _jobRepository.GetByIdAsync(jobId);
        }
    }
}