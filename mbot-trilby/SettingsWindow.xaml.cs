using System;
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
        private readonly Func<string, Task> _signInAsync;
        private readonly Action<string> _signOut;
        private readonly Action<string> _log;

        public SettingsWindow(
            TrilbyEnvironmentCatalogSettings environments,
            Func<string> getSelectedEnvironmentName,
            Action<string> setSelectedEnvironmentName,
            Func<string, TrilbySessionSettings?> getSession,
            Func<string, Task> signInAsync,
            Action<string> signOut,
            Action<string> log)
        {
            InitializeComponent();
            _environments = environments;
            _getSelectedEnvironmentName = getSelectedEnvironmentName;
            _setSelectedEnvironmentName = setSelectedEnvironmentName;
            _getSession = getSession;
            _signInAsync = signInAsync;
            _signOut = signOut;
            _log = log;
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
            if (EnvironmentComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string environmentName)
            {
                return;
            }

            _setSelectedEnvironmentName(environmentName);
            RefreshView();
        }

        public void RefreshView()
        {
            var environmentName = GetSelectedEnvironmentName();
            var environment = _environments.GetByName(environmentName);
            var session = _getSession(environmentName);
            BaseUrlTextBlock.Text = $"Base URL: {environment.BaseUrl}";
            if (session is null || !session.IsAuthenticated)
            {
                AuthStatusTextBlock.Text = "Not signed in.";
                SignOutButton.IsEnabled = false;
            }
            else
            {
                AuthStatusTextBlock.Text = $"Signed in as {session.Username} for {session.GuildName} ({session.GuildId})";
                SignOutButton.IsEnabled = true;
            }
        }

        private string GetSelectedEnvironmentName()
        {
            return EnvironmentComboBox.SelectedItem is ComboBoxItem item && item.Tag is string environmentName
                ? environmentName
                : "dev";
        }

        private void SelectEnvironment(string environmentName)
        {
            foreach (var item in EnvironmentComboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem &&
                    comboBoxItem.Tag is string itemEnvironmentName &&
                    string.Equals(itemEnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase))
                {
                    EnvironmentComboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }

            EnvironmentComboBox.SelectedIndex = 0;
        }

        private void SetBusyState(bool isBusy, string infoText)
        {
            EnvironmentComboBox.IsEnabled = !isBusy;
            SignInButton.IsEnabled = !isBusy;
            SignOutButton.IsEnabled = !isBusy;
            InfoTextBlock.Text = infoText;
        }
    }
}
