using Checklist.Application.Interfaces;

namespace Checklist.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime CurrentUtcDateTime => DateTime.UtcNow;
}