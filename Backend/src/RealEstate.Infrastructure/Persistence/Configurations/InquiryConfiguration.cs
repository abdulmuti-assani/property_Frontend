using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations
{
    public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
    {
        public void Configure(EntityTypeBuilder<Inquiry> builder)
        {
            builder.ToTable("Inquiries");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.HasOne(i => i.Property)
                .WithMany(p => p.Inquiries)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Buyer)
                .WithMany(u => u.Inquiries)
                .HasForeignKey(i => i.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(i => new { i.BuyerId, i.PropertyId });
        }
    }
}
