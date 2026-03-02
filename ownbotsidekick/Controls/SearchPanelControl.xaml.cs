using System;
using System.Collections.Generic;
using System.Windows;

namespace ownbotsidekick.Controls
{
    public partial class SearchPanelControl : System.Windows.Controls.UserControl
    {
        public event EventHandler<string>? ClipSelected;

        public SearchPanelControl()
        {
            InitializeComponent();
        }

        public void UpdateSearchState(string query, IReadOnlyList<string> filteredClipTriggers)
        {
            SearchQueryTextBlock.Text = string.IsNullOrEmpty(query)
                ? "Start typing to search..."
                : query;

            SearchResultsGrid.Children.Clear();
            NoResultsTextBlock.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(query))
            {
                return;
            }

            if (filteredClipTriggers.Count == 0)
            {
                NoResultsTextBlock.Visibility = Visibility.Visible;
                return;
            }

            foreach (var trigger in filteredClipTriggers)
            {
                var button = new System.Windows.Controls.Button
                {
                    Content = trigger,
                    Style = (Style)FindResource("ClipButtonStyle")
                };
                button.Click += (_, _) => ClipSelected?.Invoke(this, trigger);
                SearchResultsGrid.Children.Add(button);
            }
        }
    }
}
