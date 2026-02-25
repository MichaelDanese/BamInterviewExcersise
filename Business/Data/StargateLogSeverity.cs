using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("StargateLogSeverity")]
    public class StargateLogSeverity 
    {
        public int StargateLogSeverityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class StargateLogSeverityConfiguration : IEntityTypeConfiguration<StargateLogSeverity>
    {
        public void Configure(EntityTypeBuilder<StargateLogSeverity> builder)
        {
            builder.HasKey(x => x.StargateLogSeverityId);
            builder.Property(x => x.StargateLogSeverityId).ValueGeneratedOnAdd();
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Description).IsRequired();
            builder.HasIndex(x => x.Name);

        }
    }
}
