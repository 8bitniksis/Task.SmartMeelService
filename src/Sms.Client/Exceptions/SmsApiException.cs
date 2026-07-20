namespace Sms.Client.Exceptions;

public sealed class SmsApiException : Exception
{
    public SmsApiException(string message)
        : base(message)
    {
    }
}