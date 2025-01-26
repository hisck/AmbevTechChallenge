using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
    {
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            builder.HasKey(si => si.Id);

            builder.Property(si => si.Id)
                   .HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

            builder.Property(si => si.SaleId)
                   .IsRequired();

            builder.Property(si => si.ProductId)
                   .IsRequired();

            builder.Property(si => si.ProductName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(si => si.UnitPrice)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(si => si.Quantity)
                   .IsRequired();

            builder.Property(si => si.Discount)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(si => si.TotalAmount)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(si => si.IsCancelled)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.ToTable("SaleItems");
        }
    }
}
