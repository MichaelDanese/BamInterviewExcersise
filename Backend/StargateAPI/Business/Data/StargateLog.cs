using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("StargateLog")]
    public class StargateLog
    {
        public int StargateLogId { get; set; }
        public int StargateLogSeverityId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public DateTime CreatedOn {  get; set; }

        public virtual StargateLogSeverity StargateLogSeverity { get; set; }

    }

    public class StargateLogConfiguration : IEntityTypeConfiguration<StargateLog>
    {
        public void Configure(EntityTypeBuilder<StargateLog> builder)
        {
            builder.HasKey(x => x.StargateLogId);
            builder.Property(x => x.StargateLogId).ValueGeneratedOnAdd();
            builder.Property(x => x.Message).IsRequired();
            builder.Property(x => x.Details).IsRequired();
            builder.HasIndex(x => x.CreatedOn);

        }
    }
}
