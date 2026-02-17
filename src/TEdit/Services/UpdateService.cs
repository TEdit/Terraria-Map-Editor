using System;
using System.Threading.Tasks;
using TEdit.ViewModel;
using Velopack;
using Velopack.Sources;

namespace TEdit.Services;

public class UpdateService
{
    private readonly UpdateManager _updateManager;

    public UpdateService(UpdateChannel channel = UpdateChannel.Stable)
    {
        var source = new GithubSource("https://github.com/TEdit/Terraria-Map-Editor", null, false);
        var options = new UpdateOptions();

        var channelName = channel.ToString().ToLowerInvariant();
        if (channel != UpdateChannel.Stable)
        {
            options.ExplicitChannel = channelName;
            options.AllowVersionDowngrade = true;
        }

        _updateManager = new UpdateManager(source, options);
    }

    public bool IsInstalled => _updateManager.IsInstalled;

    public async Task<bool> CheckAndDownloadAsync()
    {
        if (!_updateManager.IsInstalled)
        {
            ErrorLogging.LogInfo("[Update] Not a Velopack install — skipping update check.");
            return false;
        }

        try
        {
            var update = await _updateManager.CheckForUpdatesAsync();
            if (update == null)
            {
                ErrorLogging.LogInfo("[Update] No updates available.");
                return false;
            }

            ErrorLogging.LogInfo($"[Update] Update available: {update.TargetFullRelease.Version}");
            await _updateManager.DownloadUpdatesAsync(update);
            ErrorLogging.LogInfo("[Update] Update downloaded. Waiting for user to restart.");
            return true;
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn($"[Update] Check failed: {ex.Message}");
            ErrorLogging.LogException(ex);
            return false;
        }
    }

    public void ApplyAndRestart()
    {
        if (!_updateManager.IsInstalled) return;

        try
        {
            var update = _updateManager.CheckForUpdatesAsync().GetAwaiter().GetResult();
            if (update != null)
            {
                _updateManager.ApplyUpdatesAndRestart(update);
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn($"[Update] Apply failed: {ex.Message}");
            ErrorLogging.LogException(ex);
        }
    }
}
