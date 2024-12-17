namespace HashKorea.Services;

public interface ILogService
{
    void LogError(string message, string exception, string additionalData);
}
