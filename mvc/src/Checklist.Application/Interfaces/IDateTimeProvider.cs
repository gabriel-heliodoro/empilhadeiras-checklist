namespace Checklist.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime CurrentUtcDateTime { get; }
}
