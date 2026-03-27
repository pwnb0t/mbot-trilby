using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using mbottrilby.Configuration;
using mbottrilby.Services;

namespace mbottrilby
{
    public partial class SettingsWindow : Window
    {
        private readonly TrilbyEnvironmentCatalogSettings _environments;
        private readonly Func<string> _getSelectedEnvironmentName;
        private readonly Action<string> _setSelectedEnvironmentName;
        private readonly Func<string, TrilbySessionSettings?> _getSession;
        private readonly Func<string, long?> _getSelectedServerId;
        private readonly Action<string, long?> _setSelectedServerId;
        private readonly Func<string, Task> _signInAsync;
        private readonly Action<string> _signOut;
        private readonly Action<string> _log;

        public SettingsWindow(
            TrilbyEnvironmentCatalogSettings environments,
            Func<string> getSelectedEnvironmentName,
            Action<string> setSelectedEnvironmentName,
            Func<string, TrilbySessionSettings?> getSession,
            Func<string, long?> getSelectedServerId,
            Action<string, long?> setSelectedServerId,
            Func<string, Task> signInAsync,
            Action<string> signOut,
            Action<string> log)
        {
            InitializeComponent();
            _environments = environments;
            _getSelectedEnvironmentName = getSelectedEnvironmentName;
            _setSelectedEnvironmentName = setSelectedEnvironmentName;
            _getSession = getSession;
            _getSelectedServerId = getSelectedServerId;
            _setSelectedServerId = setSelectedServerId;
            _signInAsync = signInAsync;
            _signOut = signOut;
            _log = log;
            PopulateEnvironmentOptions();
            SelectEnvironment(_getSelectedEnvironmentName());
            RefreshView();
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

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var environmentName = GetSelectedEnvironmentName();
            _signOut(environmentName);
            RefreshView();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
            BaseUrlTextBlock.Text = $"Base URL: {environment.BaseUrl}";
            PopulateServerOptions(environmentName, session);
            if (session is null || !session.IsAuthenticated)
            {
                AuthStatusTextBlock.Text = "Not signed in.";
                InfoTextBlock.Text = "Sign in to load your available servers.";
                SignOutButton.IsEnabled = false;
                ServerComboBox.IsEnabled = false;
                return;
            }

            SignOutButton.IsEnabled = true;
            ServerComboBox.IsEnabled = true;
            var selectedServerId = _getSelectedServerId(environmentName);
            var selectedServer = session.Servers.FirstOrDefault(server => server.GuildId == selectedServerId);
            AuthStatusTextBlock.Text = $"Signed in as {session.Username}";
            InfoTextBlock.Text = selectedServer is null
                ? "Choose a server to enable Trilby for this environment."
                : $"Current server: {selectedServer.GuildName} ({selectedServer.GuildId})";
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
            SignInButton.IsEnabled = !isBusy;
            SignOutButton.IsEnabled = !isBusy;
            InfoTextBlock.Text = infoText;
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
