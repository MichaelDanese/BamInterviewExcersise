using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Services.Interfaces;
using StargateAPI.Enums;
using System;
using System.Runtime.CompilerServices;

namespace StargateAPI.Business.Services
{
    public class DatabaseLoggingService : IDatabaseLoggingService
    {
        private readonly StarbaseContext _starbaseContext;

        public DatabaseLoggingService(StarbaseContext context)
        {
            _starbaseContext = context;
        }

        public async Task LogAsync(StargateLogSeverityEnum severity, string message, string details, Exception? exception)
        {
            switch (severity)
            {
                case StargateLogSeverityEnum.Info:
                    await LogInfoAsync(message, details);
                    break;
                case StargateLogSeverityEnum.Warning:
                    await LogWarningAsync(message, details);
                    break;
                case StargateLogSeverityEnum.Error:
                    await LogErrorAsync(message, details, exception);
                    break;
                default :
                    throw new NotImplementedException();
            }
        }

        public async Task LogErrorAsync(string message, string details, Exception? exception)
        {
            await _starbaseContext.StargateLogs.AddAsync(new StargateLog
            {
                Message = message,
                Details = details,
                Exception = exception?.ToString(),
                StargateLogSeverityId = (int)StargateLogSeverityEnum.Error,
                CreatedOn = GetLogTime()
            });
        }

        public async Task LogInfoAsync(string message, string details)
        {
            await _starbaseContext.StargateLogs.AddAsync(new StargateLog
            {
                Message = message,
                Details = details,
                StargateLogSeverityId = (int)StargateLogSeverityEnum.Info,
                CreatedOn = GetLogTime()
            });
        }

        public async Task LogWarningAsync(string message, string details)
        {
            await _starbaseContext.StargateLogs.AddAsync(new StargateLog
            {
                Message = message,
                Details = details,
                StargateLogSeverityId = (int)StargateLogSeverityEnum.Warning,
                CreatedOn = GetLogTime()
            });
        }

        private DateTime GetLogTime()
        {
            return DateTime.UtcNow;
        }
    }
}
