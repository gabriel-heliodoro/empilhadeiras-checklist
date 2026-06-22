namespace Checklist.Application.Common;

public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message)
    {
    }
}