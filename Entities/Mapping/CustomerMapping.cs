using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Mapping
{
    public class CustomerMapping : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasMany(cust => cust.Countries)
                .WithMany(country => country.Customers)
                .UsingEntity("CountryCustomer",
                left => left.HasOne(typeof(Country))
                .WithMany()
                .HasForeignKey("CountryId")
                .HasConstraintName("FK_CountryCustomer_Country")
                .OnDelete(DeleteBehavior.Cascade),
                right => right.HasOne(typeof(Customer))
                .WithMany()
                .HasForeignKey("CustomerId")
                .HasConstraintName("FK_CountryCustomer_Customer")
                .OnDelete(DeleteBehavior.Cascade),
                linkBuilder => { 
                    linkBuilder.HasKey("CountryId", "CustomerId");
                    linkBuilder.ToTable("CountryCustomer");
                }
                );
        }
    }
}
