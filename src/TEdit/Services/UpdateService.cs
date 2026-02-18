using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEdit.ViewModel;
using Velopack;
using Velopack.Sources;

namespace TEdit.Services;

public class UpdateService
{
    private const string GithubRepo = "https://github.com/TEdit/Terraria-Map-Editor";

    private readonly List<UpdateManager> _managers = new();

    /// <summary>
    /// Creates update managers for all channels the user's tier includes.
    /// Beta receives beta + stable. Alpha receives alpha + beta + stable.
    /// </summary>
    public UpdateService(UpdateChannel channel = UpdateChannel.Stable)
    {
        // Stable is always included — every tier gets stable releases.
        // CI packs with --channel stable/beta/alpha so ExplicitChannel must match.
        _managers.Add(CreateManager("stable"));

        if (channel >= UpdateChannel.Beta)
            _managers.Add(CreateManager("beta"));

        if (channel >= UpdateChannel.Alpha)
            _managers.Add(CreateManager("alpha"));
    }

    private static UpdateManager CreateManager(string channelName)
    {
        var source = new GithubSource(GithubRepo, null, false);
        var options = new UpdateOptions
        {
            ExplicitChannel = channelName,
            AllowVersionDowngrade = true,
        };

        return new UpdateManager(source, options);
    }

    public bool IsInstalled => _managers[0].IsInstalled;

    /// <summary>
    /// Checks all eligible channels and returns the manager + update info for the newest available version.
    /// </summary>
    private async Task<(UpdateManager manager, UpdateInfo update)?> FindBestUpdateAsync()
    {
        UpdateManager bestManager = null;
        UpdateInfo bestUpdate = null;

        foreach (var mgr in _managers)
        {
            try
            {
                var update = await mgr.CheckForUpdatesAsync();
                if (update == null) continue;

                if (bestUpdate == null || update.TargetFullRelease.Version > bestUpdate.TargetFullRelease.Version)
                {
                    bestManager = mgr;
                    bestUpdate = update;
                }
            }
            catch (Exception ex)
            {
                ErrorLogging.LogDebug($"[Update] Channel check failed: {ex.Message}");
            }
        }

        if (bestManager != null && bestUpdate != null)
            return (bestManager, bestUpdate);

        return null;
    }

    public async Task<bool> CheckOnlyAsync()
    {
        if (!IsInstalled)
        {
            ErrorLogging.LogInfo("[Update] Not a Velopack install — skipping update check.");
            return false;
        }

        try
        {
            var result = await FindBestUpdateAsync();
            if (result == null)
            {
                ErrorLogging.LogInfo("[Update] No updates available.");
                return false;
            }

            ErrorLogging.LogInfo($"[Update] Update available: {result.Value.update.TargetFullRelease.Version}");
            return true;
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn($"[Update] Check failed: {ex.Message}");
            ErrorLogging.LogException(ex);
            return false;
        }
    }

    public async Task<bool> CheckAndDownloadAsync()
    {
        if (!IsInstalled)
        {
            ErrorLogging.LogInfo("[Update] Not a Velopack install — skipping update check.");
            return false;
        }

        try
        {
            var result = await FindBestUpdateAsync();
            if (result == null)
            {
                ErrorLogging.LogInfo("[Update] No updates available.");
                return false;
            }

            var (mgr, update) = result.Value;
            ErrorLogging.LogInfo($"[Update] Update available: {update.TargetFullRelease.Version}");
            await mgr.DownloadUpdatesAsync(update);
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
        if (!IsInstalled) return;

        try
        {
            var result = FindBestUpdateAsync().GetAwaiter().GetResult();
            if (result != null)
            {
                var (mgr, update) = result.Value;
                mgr.ApplyUpdatesAndRestart(update);
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn($"[Update] Apply failed: {ex.Message}");
            ErrorLogging.LogException(ex);
        }
    }
}
