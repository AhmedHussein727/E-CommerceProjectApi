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

namespace ECommerce.Persistence.Data.DataSeed
{
    public class DataIntializer : IDataIntializer
    {
        private readonly StoreDbContext _dbContext;

        public DataIntializer(StoreDbContext dbContext)
        {
            _dbContext = dbContext;
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
                Console.WriteLine($"Error occured during data intialization: {ex}");
            }
        }

        private async Task SeedDataFromJson<T, TKey>(string fileName, DbSet<T> dbset)
            where T : BaseEntity<TKey>
        {
            //C:\Users\Khale\Documents\Desktop\Route Work\C44\Sessions\08-Asp.Net Web API\Online Project\ECommerce.Online.API\ECommerce.Persistence\Data\DataSeed\JsonFiles\brands.json

            var filePath = @"..\ECommerce.Persistence\Data\DataSeed\JsonFiles\" + fileName;

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
                Console.WriteLine($"Error while reading data from Json {ex} ");
            }
        }
    }
}
