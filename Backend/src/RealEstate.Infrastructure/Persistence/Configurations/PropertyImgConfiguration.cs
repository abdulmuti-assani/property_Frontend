using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstate.Domain.Entities;

namespace RealEstate.Infrastructure.Persistence.Configurations
{
    public class PropertyImgConfiguration : IEntityTypeConfiguration<PropertyImg>
    {
        public void Configure(EntityTypeBuilder<PropertyImg> builder)
        {
            builder.ToTable("PropertyImages");
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ImgUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(i => i.Property)
                .WithMany(p => p.PropertyImgs)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
