using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Identity;

namespace RealEstate.Infrastructure.Persistence.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("UserProfiles");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(255);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(255);

            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.Phone).HasMaxLength(20);
            builder.Property(u => u.Role).IsRequired().HasMaxLength(20);
            builder.Property(u => u.Address).HasMaxLength(500);
            builder.Property(u => u.ProfilePicUrl).HasMaxLength(1000);

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<User>(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
