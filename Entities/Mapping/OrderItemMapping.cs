using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Mapping
{
    public class OrderItemMapping : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(r => r.OrderItemId);
            
            builder.HasOne(r => r.Order)
                   .WithMany(o => o.Items)
                   .HasForeignKey(r => r.OrderId);

            builder.HasOne(i => i.OrderItemReview)
               .WithOne(r => r.OrderItem)
               .HasForeignKey<OrderItemReview>(r => r.OrderItemId)
               .OnDelete(DeleteBehavior.Cascade);

            //index on OrderId
            builder.HasIndex(i => i.OrderId)
                   .HasDatabaseName("IX_OrderItem_OrderId");
        }
    }
}
