using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace mbottrilby.Services
{
    internal sealed class TrilbyUpdateService
    {
        private const string ReleasesRepositoryUrl = "https://github.com/pwnb0t/mbot-trilby";

        private readonly UpdateManager _updateManager;
        private TrilbyUpdateStatus _status;

        public TrilbyUpdateService()
        {
            _updateManager = new UpdateManager(
                new GithubSource(ReleasesRepositoryUrl, string.Empty, prerelease: false));
            _status = BuildInitialStatus();
        }

        public TrilbyUpdateStatus GetStatus()
        {
            return _status;
        }

        public async Task<TrilbyUpdateStatus> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            if (!_updateManager.IsInstalled)
            {
                _status = _status with
                {
                    StatusText = "Updates are only available in installed Trilby releases.",
                    IsBusy = false,
                    CanCheckForUpdates = false,
                    IsReadyToApply = false
                };
                return _status;
            }

            if (_updateManager.UpdatePendingRestart is not null)
            {
                _status = _status with
                {
                    StatusText = $"Update {_updateManager.UpdatePendingRestart.Version} is ready to install when Trilby exits.",
                    IsBusy = false,
                    CanCheckForUpdates = true,
                    IsReadyToApply = true
                };
                return _status;
            }

            _status = _status with
            {
                StatusText = "Checking for updates...",
                IsBusy = true,
                CanCheckForUpdates = true,
                IsReadyToApply = false
            };

            try
            {
                var updateInfo = await _updateManager.CheckForUpdatesAsync();
                if (updateInfo is null)
                {
                    _status = _status with
                    {
                        StatusText = "You are up to date.",
                        IsBusy = false,
                        CanCheckForUpdates = true,
                        IsReadyToApply = false
                    };
                    return _status;
                }

                _status = _status with
                {
                    StatusText = $"Downloading {updateInfo.TargetFullRelease.Version}...",
                    IsBusy = true,
                    CanCheckForUpdates = true,
                    IsReadyToApply = false
                };

                await _updateManager.DownloadUpdatesAsync(
                    updateInfo,
                    progress =>
                    {
                        _status = _status with
                        {
                            StatusText = $"Downloading {updateInfo.TargetFullRelease.Version}... {progress}%",
                            IsBusy = true
                        };
                    },
                    cancellationToken);

                var pendingUpdate = _updateManager.UpdatePendingRestart ?? updateInfo.TargetFullRelease;
                _status = _status with
                {
                    StatusText = $"Update {pendingUpdate.Version} is ready to install when Trilby exits.",
                    IsBusy = false,
                    CanCheckForUpdates = true,
                    IsReadyToApply = true
                };
                return _status;
            }
            catch (OperationCanceledException)
            {
                _status = _status with
                {
                    StatusText = "Update check canceled.",
                    IsBusy = false,
                    CanCheckForUpdates = true,
                    IsReadyToApply = _updateManager.UpdatePendingRestart is not null
                };
                return _status;
            }
            catch (Exception ex)
            {
                _status = _status with
                {
                    StatusText = $"Update check failed: {ex.Message}",
                    IsBusy = false,
                    CanCheckForUpdates = true,
                    IsReadyToApply = _updateManager.UpdatePendingRestart is not null
                };
                return _status;
            }
        }

        public async Task<bool> PrepareUpdateForExitAsync()
        {
            var pendingUpdate = _updateManager.UpdatePendingRestart;
            if (pendingUpdate is null)
            {
                return false;
            }

            await _updateManager.WaitExitThenApplyUpdatesAsync(
                pendingUpdate,
                silent: false,
                restart: false,
                restartArgs: Array.Empty<string>());
            return true;
        }

        public async Task<bool> PrepareUpdateForImmediateRestartAsync()
        {
            var pendingUpdate = _updateManager.UpdatePendingRestart;
            if (pendingUpdate is null)
            {
                return false;
            }

            await _updateManager.WaitExitThenApplyUpdatesAsync(
                pendingUpdate,
                silent: false,
                restart: true,
                restartArgs: Array.Empty<string>());
            return true;
        }

        private TrilbyUpdateStatus BuildInitialStatus()
        {
            var currentVersion = _updateManager.CurrentVersion?.ToString()
                ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
                ?? "unknown";
            var pendingUpdate = _updateManager.UpdatePendingRestart;
            if (!_updateManager.IsInstalled)
            {
                return new TrilbyUpdateStatus(
                    currentVersion,
                    "Updates are only available in installed Trilby releases.",
                    false,
                    false,
                    false);
            }

            if (pendingUpdate is not null)
            {
                return new TrilbyUpdateStatus(
                    currentVersion,
                    $"Update {pendingUpdate.Version} is ready to install when Trilby exits.",
                    true,
                    false,
                    true);
            }

            return new TrilbyUpdateStatus(
                currentVersion,
                "Check for updates to download the latest Trilby release.",
                true,
                false,
                false);
        }
    }

    public sealed record TrilbyUpdateStatus(
        string CurrentVersionText,
        string StatusText,
        bool CanCheckForUpdates,
        bool IsBusy,
        bool IsReadyToApply);
}
