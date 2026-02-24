using StargateAPI.Business.Dtos;
using StargateAPI.Enums;

namespace StargateAPI.Services.Interfaces
{
    public interface ILoggingService
    {
        public Task LogAsync(StargateLogSeverityEnum severity, string message, string details, Exception? exception);
        public Task LogInfoAsync(string message, string details);
        public Task LogWarningAsync(string message, string details);
        public Task LogErrorAsync(string message, string details,  Exception? exception);
    }
}
