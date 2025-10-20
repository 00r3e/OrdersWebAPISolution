using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Mapping
{
    public class OrderMapping : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasPrincipalKey(customer => customer.CustomerId)
            .HasForeignKey(order => order.CustomerId);

            //index on CustomerId
            builder.HasIndex(order => order.CustomerId)
                   .HasDatabaseName("IX_Order_CustomerId");
        }
    
    }
}
