using Microsoft.Extensions.Configuration;
using Shared.Data;
using Shared.Models;
using Shared.Repositories;
using Shared.Services;
using Shared.Interfaces;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Data Seeder Started");

        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        try
        {
            // Setup MongoDB context
            var mongoContext = new MongoDbContext(
                configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:password@localhost:27017",
                configuration["DatabaseName"] ?? "raboidrpa_dev");

            // Setup repositories
            var clientRepository = new RpaClientRepository(mongoContext);
            var barcodeService = new BarcodeService(new BarcodeRepository(mongoContext));
            var jobRepository = new RpaJobRepository(mongoContext);
            var productRepository = new ProductRepository(mongoContext);

            // Seed RPA clients
            await SeedRpaClients(clientRepository);

            // Seed barcodes
            await SeedBarcodes(barcodeService);

            // Seed sample products
            await SeedSampleProducts(productRepository);

            // Seed sample jobs
            await SeedSampleJobs(jobRepository, productRepository, barcodeService);

            Console.WriteLine("Data seeding completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task SeedRpaClients(IRpaClientRepository clientRepository)
    {
        Console.WriteLine("Seeding RPA clients...");

        // Create 50 RPA clients as requested
        var clients = new List<RpaClient>();
        for (int i = 1; i <= 50; i++)
        {
            clients.Add(new RpaClient
            {
                ClientId = $"test-client-{i}",
                ClientSecret = $"test-secret-{i}",
                Name = $"Test Client {i}",
                IsActive = true
            });
        }

        foreach (var client in clients)
        {
            // Check if client already exists
            var existingClient = await clientRepository.GetByClientIdAsync(client.ClientId);
            if (existingClient == null)
            {
                await clientRepository.CreateAsync(client);
                Console.WriteLine($"Created client: {client.Name}");
            }
            else
            {
                Console.WriteLine($"Client {client.Name} already exists");
            }
        }
    }

    private static async Task SeedBarcodes(IBarcodeService barcodeService)
    {
        Console.WriteLine("Seeding barcodes...");

        // Seed 1000 barcodes
        await barcodeService.SeedBarcodesAsync(1000);
        Console.WriteLine("Seeded 1000 barcodes");
    }

    private static async Task SeedSampleProducts(IProductRepository productRepository)
    {
        Console.WriteLine("Seeding sample products...");

        var sampleProducts = new List<Product>
        {
            new Product
            {
                ProductCode = "PROD001",
                ProductName = "Laptop Computer",
                Price = 1299.99m,
                Barcode = "1234567890128",
                StoreId = "STORE001"
            },
            new Product
            {
                ProductCode = "PROD002",
                ProductName = "Wireless Mouse",
                Price = 29.99m,
                Barcode = "1234567890135",
                StoreId = "STORE001"
            },
            new Product
            {
                ProductCode = "PROD003",
                ProductName = "Mechanical Keyboard",
                Price = 89.99m,
                Barcode = "1234567890142",
                StoreId = "STORE002"
            },
            new Product
            {
                ProductCode = "PROD004",
                ProductName = "Gaming Monitor",
                Price = 399.99m,
                Barcode = "1234567890159",
                StoreId = "STORE002"
            },
            new Product
            {
                ProductCode = "PROD005",
                ProductName = "USB Cable",
                Price = 9.99m,
                Barcode = "1234567890166",
                StoreId = "STORE003"
            }
        };

        foreach (var product in sampleProducts)
        {
            await productRepository.CreateAsync(product);
            Console.WriteLine($"Created product: {product.ProductName}");
        }
    }

    private static async Task SeedSampleJobs(IRpaJobRepository jobRepository, IProductRepository productRepository, IBarcodeService barcodeService)
    {
        Console.WriteLine("Seeding sample jobs...");

        var storeIds = new[] { "STORE001", "STORE002", "STORE003", "STORE004", "STORE005" };
        var productNames = new[] { "Smartphone", "Tablet", "Headphones", "Charger", "Case", "Screen Protector", "Cable", "Adapter" };
        var random = new Random();

        for (int i = 1; i <= 20; i++)
        {
            try
            {
                // Get an available barcode
                var barcode = await barcodeService.GetAvailableBarcodeAsync();
                if (barcode == null)
                {
                    Console.WriteLine("No available barcodes for job creation");
                    break;
                }

                var storeId = storeIds[random.Next(storeIds.Length)];
                var productName = productNames[random.Next(productNames.Length)];

                // Create product
                var product = new Product
                {
                    ProductCode = $"PROD{1000 + i}",
                    ProductName = $"{productName} {i}",
                    Price = (decimal)(random.NextDouble() * 500 + 10), // Random price between 10-510
                    Barcode = barcode.Code,
                    StoreId = storeId
                };

                await productRepository.CreateAsync(product);

                // Mark barcode as used
                await barcodeService.MarkBarcodeAsUsedAsync(barcode.Code, storeId);

                // Create job
                var job = new RpaJob
                {
                    StoreId = storeId,
                    Product = product,
                    Status = JobStatus.Pending
                };

                await jobRepository.CreateAsync(job);
                Console.WriteLine($"Created job {i} for store {storeId}: {product.ProductName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating job {i}: {ex.Message}");
            }
        }
    }
}