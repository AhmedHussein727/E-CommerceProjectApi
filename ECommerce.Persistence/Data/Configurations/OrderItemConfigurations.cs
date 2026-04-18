using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Domain.Entities.OrderModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Persistence.Data.Configurations
{
    internal class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(X => X.Price).HasColumnType("decimal(8,2)");

            builder.OwnsOne(
                X => X.Product,
                OE =>
                {
                    OE.Property(X => X.ProductName).HasMaxLength(100);
                    OE.Property(X => X.PictureUrl).HasMaxLength(100);
                }
            );
        }
    }
}
