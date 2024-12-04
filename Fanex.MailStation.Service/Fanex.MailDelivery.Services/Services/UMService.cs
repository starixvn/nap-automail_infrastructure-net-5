using Fanex.MailDelivery.Core.Models;
using ONEbook.UM.Services;
using System;
using System.Threading.Tasks;

namespace Fanex.MailDelivery.Services.Services;

public interface UMService
{
    Task<UMInfoModel> GetUMInfo(int retryCount = 0);

    Task WaitForUMOff();
}

public class UMServiceImpl(
    LogService logService,
    IMaintenanceService um) : UMService, IDisposable
{
    private const int UMGMT = 8;
    private const int RetryIntervalInSeconds = 10;

    public async Task<UMInfoModel> GetUMInfo(int retryCount = 0)
    {
        UMInfoModel result = new UMInfoModel();
        bool isFailed = false;

        try
        {
            if (um.IsUnderMaintenance())
            {
                result.From = Convert.ToDateTime(um.GetUnderMaintenanceTime().From);
                result.To = Convert.ToDateTime(um.GetUnderMaintenanceTime().To);

                result.From = result.From.AddHours(UMGMT * -1); //Convert to UTC
                result.To = result.To.AddHours(UMGMT * -1);
                result.IsUnderMaintenanceTime = um.IsUnderMaintenance();
            }
            else
            {
                result = new UMInfoModel();
            }
        }
        catch (Exception ex)
        {
            logService.LogError($"Cannot get UM info -> Error: {ex.InnerException?.Message ?? ex.Message}", ex);
            isFailed = true;
            result = new UMInfoModel();
        }

        if (isFailed && retryCount == 0)
        {
            retryCount++;

            await Task.Delay(TimeSpan.FromSeconds(RetryIntervalInSeconds));

            result = await GetUMInfo(retryCount);
        }

        return result;
    }

    public async Task WaitForUMOff()
    {
        var umInfo = await GetUMInfo();
        bool isUM = umInfo != null && umInfo.IsUnderMaintenanceTime;

        if (isUM)
        {
            var afterUMTime = new DateTime(umInfo.To.Ticks, DateTimeKind.Utc).AddMinutes(1);
            TimeSpan waitingTime = afterUMTime - DateTime.UtcNow;

            if (waitingTime.TotalSeconds > 0)
            {
                logService.LogInfo($"Waiting for UM off in {(int)waitingTime.TotalMinutes} minutes before continue...");

                await Task.Delay(waitingTime);
            }
        }
    }

    #region IDisposable Support

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    ~UMServiceImpl()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}