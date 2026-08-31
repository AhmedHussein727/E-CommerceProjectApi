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
    internal class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(X => X.SubTotal).HasColumnType("decimal(8,2)");

            //OrderItem has no navigation back to Order, so EF infers an optional foreign key
            //and will not cascade. Deleting an order then fails on the FK from OrderItem.
            //An order item has no meaning without its order, so make it required and cascading.
            builder
                .HasMany(X => X.Items)
                .WithOne()
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsOne(
                X => X.Address,
                OE =>
                {
                    OE.Property(X => X.FirstName).HasMaxLength(50);
                    OE.Property(X => X.LastName).HasMaxLength(50);
                    OE.Property(X => X.City).HasMaxLength(50);
                    OE.Property(X => X.Street).HasMaxLength(50);
                    OE.Property(X => X.Country).HasMaxLength(50);
                }
            );
        }
    }
}
