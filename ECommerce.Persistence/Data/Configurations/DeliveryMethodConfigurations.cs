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
    internal class DeliveryMethodConfigurations : IEntityTypeConfiguration<DeliveryMethod>
    {
        public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.Property(X => X.Price).HasColumnType("decimal(8,2)");

            builder.Property(X => X.ShortName).HasMaxLength(50);
            builder.Property(X => X.Description).HasMaxLength(100);
            builder.Property(X => X.DeliveryTime).HasMaxLength(50);
        }
    }
}
