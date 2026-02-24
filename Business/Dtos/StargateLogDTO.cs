namespace StargateAPI.Business.Dtos
{
    public class StargateLogDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int StargateLogSeverityName { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
