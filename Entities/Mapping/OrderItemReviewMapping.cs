using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Mapping
{
    public class OrderItemReviewMapping : IEntityTypeConfiguration<OrderItemReview>
    {
        public void Configure(EntityTypeBuilder<OrderItemReview> builder)
        {


            builder.HasKey(orderItemReview => orderItemReview.OrderItemId);


            builder.HasOne(orderItemReview => orderItemReview.OrderItem)
                   .WithOne(orderItem => orderItem.OrderItemReview)
                   .HasForeignKey<OrderItemReview>(r => r.OrderItemId)
                   .OnDelete(DeleteBehavior.Restrict); 


            builder.HasOne(orderItemReview => orderItemReview.Customer)
                   .WithMany() 
                   .HasForeignKey(orderItemReview => orderItemReview.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // index on CustomerId
            builder.HasIndex(orderItemReview => orderItemReview.CustomerId)
                   .HasDatabaseName("IX_OrderItemReview_CustomerId");

            //builder.HasKey(orderItemReview => orderItemReview.OrderItemId);
            //builder.HasOne(orderItemReview => orderItemReview.OrderItem)
            //       .WithOne(orderItem => orderItem.OrderItemReview)
            //       .HasForeignKey<OrderItemReview>(r => r.OrderItemId);

            ////index on CustomerId
            //builder.HasIndex(r => r.CustomerId)
            //       .HasDatabaseName("IX_OrderItemReview_CustomerId");

        }
    }
}
