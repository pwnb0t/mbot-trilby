using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using mbottrilby.Configuration;
using mbottrilby.Services;

namespace mbottrilby
{
    public partial class SettingsWindow : Window
    {
        private readonly TrilbyEnvironmentCatalogSettings _environments;
        private readonly Func<string> _getSelectedEnvironmentName;
        private readonly Action<string> _setSelectedEnvironmentName;
        private readonly Func<bool> _getOpaqueBackground;
        private readonly Action<bool> _setOpaqueBackground;
        private readonly Func<bool> _getDoNotHideWhenPlayingClip;
        private readonly Action<bool> _setDoNotHideWhenPlayingClip;
        private readonly Func<string, TrilbySessionSettings?> _getSession;
        private readonly Func<string, long?> _getSelectedServerId;
        private readonly Action<string, long?> _setSelectedServerId;
        private readonly Func<string, Task> _signInAsync;
        private readonly Func<string, Task> _signOutAsync;
        private readonly Func<string, Task> _openClipBrowserAsync;
        private readonly Func<TrilbyUpdateStatus> _getUpdateStatus;
        private readonly Func<Task<TrilbyUpdateStatus>> _checkForUpdatesAsync;
        private readonly Func<string, Task<string>> _sendLogsToDeveloperAsync;
        private readonly Action<string> _log;

        public SettingsWindow(
            TrilbyEnvironmentCatalogSettings environments,
            Func<string> getSelectedEnvironmentName,
            Action<string> setSelectedEnvironmentName,
            Func<bool> getOpaqueBackground,
            Action<bool> setOpaqueBackground,
            Func<bool> getDoNotHideWhenPlayingClip,
            Action<bool> setDoNotHideWhenPlayingClip,
            Func<string, TrilbySessionSettings?> getSession,
            Func<string, long?> getSelectedServerId,
            Action<string, long?> setSelectedServerId,
            Func<string, Task> signInAsync,
            Func<string, Task> signOutAsync,
            Func<string, Task> openClipBrowserAsync,
            Func<TrilbyUpdateStatus> getUpdateStatus,
            Func<Task<TrilbyUpdateStatus>> checkForUpdatesAsync,
            Func<string, Task<string>> sendLogsToDeveloperAsync,
            Action<string> log)
        {
            InitializeComponent();
            ApplyWindowIcon();
            _environments = environments;
            _getSelectedEnvironmentName = getSelectedEnvironmentName;
            _setSelectedEnvironmentName = setSelectedEnvironmentName;
            _getOpaqueBackground = getOpaqueBackground;
            _setOpaqueBackground = setOpaqueBackground;
            _getDoNotHideWhenPlayingClip = getDoNotHideWhenPlayingClip;
            _setDoNotHideWhenPlayingClip = setDoNotHideWhenPlayingClip;
            _getSession = getSession;
            _getSelectedServerId = getSelectedServerId;
            _setSelectedServerId = setSelectedServerId;
            _signInAsync = signInAsync;
            _signOutAsync = signOutAsync;
            _openClipBrowserAsync = openClipBrowserAsync;
            _getUpdateStatus = getUpdateStatus;
            _checkForUpdatesAsync = checkForUpdatesAsync;
            _sendLogsToDeveloperAsync = sendLogsToDeveloperAsync;
            _log = log;
            PopulateEnvironmentOptions();
            SelectEnvironment(_getSelectedEnvironmentName());
            RefreshView();
        }

        private void OpaqueBackgroundCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            _setOpaqueBackground(OpaqueBackgroundCheckBox.IsChecked == true);
        }

        private void DoNotHideWhenPlayingClipCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            _setDoNotHideWhenPlayingClip(DoNotHideWhenPlayingClipCheckBox.IsChecked == true);
        }

        private void ApplyWindowIcon()
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "mbot.ico");
            if (!File.Exists(iconPath))
            {
                return;
            }

            try
            {
                Icon = BitmapFrame.Create(new Uri(iconPath, UriKind.Absolute));
            }
            catch
            {
                // Fall back to the default window icon if the file cannot be loaded.
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var environmentName = GetSelectedEnvironmentName();
            try
            {
                SetBusyState(true, $"Starting sign-in for {environmentName}...");
                await _signInAsync(environmentName);
                RefreshView();
            }
            catch (Exception ex)
            {
                _log($"Sign-in failed for {environmentName}: {ex.Message}");
                InfoTextBlock.Text = ex.Message;
            }
            finally
            {
                SetBusyState(false, string.Empty);
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var environmentName = GetSelectedEnvironmentName();
            try
            {
                SetBusyState(true, $"Signing out of {environmentName}...");
                await _signOutAsync(environmentName);
                RefreshView();
            }
            catch (Exception ex)
            {
                _log($"Sign-out failed for {environmentName}: {ex.Message}");
                InfoTextBlock.Text = ex.Message;
            }
            finally
            {
                SetBusyState(false, string.Empty);
            }
        }

        private async void OpenClipBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var environmentName = GetSelectedEnvironmentName();
            try
            {
                SetBusyState(true, $"Opening Clip Browser for {environmentName}...");
                await _openClipBrowserAsync(environmentName);
                RefreshView();
            }
            catch (Exception ex)
            {
                _log($"Open Clip Browser failed for {environmentName}: {ex.Message}");
                InfoTextBlock.Text = ex.Message;
            }
            finally
            {
                SetBusyState(false, string.Empty);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetUpdateBusyState(true, "Checking for updates...");
                await _checkForUpdatesAsync();
                RefreshView();
            }
            catch (Exception ex)
            {
                _log($"Update check failed: {ex.Message}");
                UpdateStatusTextBlock.Text = $"Update check failed: {ex.Message}";
            }
            finally
            {
                SetUpdateBusyState(false, _getUpdateStatus().StatusText);
            }
        }

        private async void SendLogsToDeveloperButton_Click(object sender, RoutedEventArgs e)
        {
            var environmentName = GetSelectedEnvironmentName();
            var confirmation = System.Windows.MessageBox.Show(
                "This will send recent local Trilby logs to the developer for debugging. Continue?",
                "Send Logs to Developer",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (confirmation != MessageBoxResult.OK)
            {
                return;
            }

            try
            {
                SetSupportBusyState(true, "Preparing log bundle...");
                var storedFileName = await _sendLogsToDeveloperAsync(environmentName);
                SupportStatusTextBlock.Text = $"Sent logs successfully: {storedFileName}";
            }
            catch (Exception ex)
            {
                _log($"Send logs failed for {environmentName}: {ex.Message}");
                SupportStatusTextBlock.Text = $"Send logs failed: {ex.Message}";
            }
            finally
            {
                SetSupportBusyState(false, SupportStatusTextBlock.Text);
            }
        }

        private void EnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnvironmentComboBox.SelectedItem is not EnvironmentOption option)
            {
                return;
            }

            var environmentName = option.Name;
            _setSelectedEnvironmentName(environmentName);
            RefreshView();
        }

        private void ServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerComboBox.SelectedItem is not ServerOption option)
            {
                return;
            }

            _setSelectedServerId(GetSelectedEnvironmentName(), option.GuildId > 0 ? option.GuildId : null);
            RefreshView();
        }

        public void RefreshView()
        {
            var environmentName = GetSelectedEnvironmentName();
            var environment = _environments.GetByName(environmentName);
            var session = _getSession(environmentName);
            var updateStatus = _getUpdateStatus();
            BaseUrlTextBlock.Text = $"Base URL: {environment.BaseUrl}";
            VersionTextBlock.Text = $"Current version: {updateStatus.CurrentVersionText}";
            UpdateStatusTextBlock.Text = updateStatus.StatusText;
            CheckForUpdatesButton.IsEnabled = updateStatus.CanCheckForUpdates && !updateStatus.IsBusy;
            OpaqueBackgroundCheckBox.IsChecked = _getOpaqueBackground();
            DoNotHideWhenPlayingClipCheckBox.IsChecked = _getDoNotHideWhenPlayingClip();
            PopulateServerOptions(environmentName, session);
            if (session is null || !session.IsAuthenticated)
            {
                AuthStatusTextBlock.Text = "Not signed in.";
                InfoTextBlock.Text = "Sign in to load your available servers.";
                OverlayHotkeyHintTextBlock.Visibility = Visibility.Collapsed;
                OpenClipBrowserButton.Visibility = Visibility.Collapsed;
                OpenClipBrowserButton.IsEnabled = false;
                SendLogsToDeveloperButton.IsEnabled = false;
                SupportStatusTextBlock.Text = "Sign in to send logs to the developer.";
                SignInButton.IsEnabled = true;
                SignOutButton.IsEnabled = false;
                ServerComboBox.IsEnabled = false;
                return;
            }

            OpenClipBrowserButton.Visibility = Visibility.Visible;
            OpenClipBrowserButton.IsEnabled = true;
            SendLogsToDeveloperButton.IsEnabled = true;
            if (string.IsNullOrWhiteSpace(SupportStatusTextBlock.Text) ||
                string.Equals(SupportStatusTextBlock.Text, "Sign in to send logs to the developer.", StringComparison.Ordinal))
            {
                SupportStatusTextBlock.Text = "Sends recent Trilby logs to the developer for debugging.";
            }
            SignInButton.IsEnabled = true;
            SignOutButton.IsEnabled = true;
            ServerComboBox.IsEnabled = true;
            var selectedServerId = _getSelectedServerId(environmentName);
            var selectedServer = session.Servers.FirstOrDefault(server => server.GuildId == selectedServerId);
            AuthStatusTextBlock.Text = $"Signed in as {session.Username}";
            InfoTextBlock.Text = selectedServer is null
                ? "Choose a server to enable Trilby for this environment."
                : $"Current server: {selectedServer.GuildName} ({selectedServer.GuildId})";
            OverlayHotkeyHintTextBlock.Visibility = Visibility.Visible;
        }

        private string GetSelectedEnvironmentName()
        {
            return EnvironmentComboBox.SelectedItem is EnvironmentOption option
                ? option.Name
                : _environments.GetDefaultEnvironmentName();
        }

        private void SelectEnvironment(string environmentName)
        {
            var options = EnvironmentComboBox.ItemsSource as IReadOnlyList<EnvironmentOption>;
            if (options is null || options.Count == 0)
            {
                EnvironmentComboBox.SelectedItem = null;
                return;
            }

            EnvironmentComboBox.SelectedItem = options.FirstOrDefault(
                option => string.Equals(option.Name, environmentName, StringComparison.OrdinalIgnoreCase))
                ?? options[0];
        }

        private void SetBusyState(bool isBusy, string infoText)
        {
            EnvironmentComboBox.IsEnabled = !isBusy;
            ServerComboBox.IsEnabled = !isBusy;
            OpenClipBrowserButton.IsEnabled = !isBusy && OpenClipBrowserButton.Visibility == Visibility.Visible;
            SignInButton.IsEnabled = !isBusy;
            SignOutButton.IsEnabled = !isBusy;
            InfoTextBlock.Text = infoText;
        }

        private void SetUpdateBusyState(bool isBusy, string statusText)
        {
            CheckForUpdatesButton.IsEnabled = !isBusy && _getUpdateStatus().CanCheckForUpdates;
            UpdateStatusTextBlock.Text = statusText;
        }

        private void SetSupportBusyState(bool isBusy, string statusText)
        {
            SendLogsToDeveloperButton.IsEnabled = !isBusy && (_getSession(GetSelectedEnvironmentName())?.IsAuthenticated ?? false);
            SupportStatusTextBlock.Text = statusText;
        }

        private void PopulateServerOptions(string environmentName, TrilbySessionSettings? session)
        {
            var selectedServerId = _getSelectedServerId(environmentName);
            var options = new List<ServerOption>();
            if (session is not null)
            {
                options.AddRange(session.Servers
                    .OrderBy(server => server.GuildName, StringComparer.OrdinalIgnoreCase)
                    .Select(server => new ServerOption(server.GuildId, server.GuildName ?? server.GuildId.ToString())));
            }

            ServerComboBox.SelectionChanged -= ServerComboBox_SelectionChanged;
            ServerComboBox.ItemsSource = options;
            ServerComboBox.SelectedItem = options.FirstOrDefault(option => option.GuildId == selectedServerId);
            ServerComboBox.SelectionChanged += ServerComboBox_SelectionChanged;
        }

        private void PopulateEnvironmentOptions()
        {
            var options = _environments.GetAvailableEnvironments()
                .Select(entry => new EnvironmentOption(entry.Name, entry.Settings.DisplayName))
                .ToArray();
            EnvironmentComboBox.SelectionChanged -= EnvironmentComboBox_SelectionChanged;
            EnvironmentComboBox.ItemsSource = options;
            EnvironmentComboBox.SelectionChanged += EnvironmentComboBox_SelectionChanged;

            var showEnvironmentPicker = options.Length > 1;
            EnvironmentPanel.Visibility = showEnvironmentPicker ? Visibility.Visible : Visibility.Collapsed;
            EnvironmentRowDefinition.Height = showEnvironmentPicker
                ? GridLength.Auto
                : new GridLength(0);
        }

        private sealed class EnvironmentOption
        {
            public EnvironmentOption(string name, string displayName)
            {
                Name = name;
                DisplayName = displayName;
            }

            public string Name { get; }

            public string DisplayName { get; }
        }

        private sealed class ServerOption
        {
            public ServerOption(long guildId, string displayName)
            {
                GuildId = guildId;
                DisplayName = displayName;
            }

            public long GuildId { get; }

            public string DisplayName { get; }
        }
    }
}
