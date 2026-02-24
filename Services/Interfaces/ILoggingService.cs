using StargateAPI.Enums;

namespace StargateAPI.Services.Interfaces
{
    public interface ILoggingService
    {
        Task LogAsync(StargateLogSeverityEnum severity, string message, string details, Exception? exception);
        Task LogInfoAsync(string message, string details);
        Task LogWarningAsync(string message, string details);
        Task LogErrorAsync(string message, string details,  Exception? exception);
    }
}
