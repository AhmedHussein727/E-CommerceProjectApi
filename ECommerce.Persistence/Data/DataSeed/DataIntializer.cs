using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Domain.Contracts;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Entities.OrderModule;
using ECommerce.Domain.Entities.ProductModule;
using ECommerce.Persistence.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Persistence.Data.DataSeed
{
    public class DataIntializer : IDataIntializer
    {
        private readonly StoreDbContext _dbContext;
        private readonly ILogger<DataIntializer> _logger;

        public DataIntializer(StoreDbContext dbContext, ILogger<DataIntializer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task IntializeAsync()
        {
            try
            {
                var hasProduct = await _dbContext.Products.AnyAsync();
                var hasBrands = await _dbContext.ProductBrands.AnyAsync();
                var hasTypes = await _dbContext.ProductTypes.AnyAsync();
                var hasDeliveryMethods = await _dbContext.Set<DeliveryMethod>().AnyAsync();

                if (hasProduct && hasBrands && hasTypes && hasDeliveryMethods)
                    return;

                if (!hasBrands)
                {
                    await SeedDataFromJson<ProductBrand, int>(
                        "brands.json",
                        _dbContext.ProductBrands
                    );
                }

                if (!hasTypes)
                {
                    await SeedDataFromJson<ProductType, int>("types.json", _dbContext.ProductTypes);
                }

                await _dbContext.SaveChangesAsync();

                if (!hasProduct)
                    await SeedDataFromJson<Product, int>("products.json", _dbContext.Products);

                if (!hasDeliveryMethods)
                    await SeedDataFromJson<DeliveryMethod, int>(
                        "delivery.json",
                        _dbContext.Set<DeliveryMethod>()
                    );

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Catalog seeding failed - the store will start empty");
            }
        }

        private async Task SeedDataFromJson<T, TKey>(string fileName, DbSet<T> dbset)
            where T : BaseEntity<TKey>
        {
            //Resolve against the deployed application directory, not the source tree.
            //Path.Combine also keeps this working on Linux, where a backslash is a
            //legal filename character rather than a separator.
            var filePath = Path.Combine(AppContext.BaseDirectory, "DataSeed", fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Json file not found", filePath);

            try
            {
                // var data = File.ReadAllText(filePath);

                var dataStream = File.OpenRead(filePath);

                var data = await JsonSerializer.DeserializeAsync<List<T>>(
                    dataStream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (data is not null)
                {
                    await dbset.AddRangeAsync(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read seed data from {FileName}", fileName);
            }
        }
    }
}
