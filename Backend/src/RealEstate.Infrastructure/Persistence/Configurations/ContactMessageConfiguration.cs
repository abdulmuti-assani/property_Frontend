using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations
{
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> builder)
        {
            builder.ToTable("ContactMessages");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(255);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
            builder.Property(c => c.Phone).HasMaxLength(20);
            builder.Property(c => c.Message).IsRequired().HasMaxLength(4000);
            builder.Property(c => c.Role).HasMaxLength(20);
        }
    }
}
