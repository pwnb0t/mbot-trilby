using System;
using System.Collections.Generic;
using System.Windows;

namespace ownbotsidekick.Controls
{
    public partial class SearchPanelControl : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty SearchQueryDisplayProperty = DependencyProperty.Register(
            nameof(SearchQueryDisplay),
            typeof(string),
            typeof(SearchPanelControl),
            new PropertyMetadata("Start typing to search...", OnSearchQueryDisplayChanged)
        );

        public static readonly DependencyProperty VisibleClipsProperty = DependencyProperty.Register(
            nameof(VisibleClips),
            typeof(IReadOnlyList<string>),
            typeof(SearchPanelControl),
            new PropertyMetadata(Array.Empty<string>(), OnVisibleClipsChanged)
        );

        public static readonly DependencyProperty NoResultsVisibleProperty = DependencyProperty.Register(
            nameof(NoResultsVisible),
            typeof(bool),
            typeof(SearchPanelControl),
            new PropertyMetadata(false, OnNoResultsVisibleChanged)
        );

        public event EventHandler<string>? ClipSelected;

        public SearchPanelControl()
        {
            InitializeComponent();
        }

        public string SearchQueryDisplay
        {
            get => (string)GetValue(SearchQueryDisplayProperty);
            set => SetValue(SearchQueryDisplayProperty, value);
        }

        public IReadOnlyList<string> VisibleClips
        {
            get => (IReadOnlyList<string>)GetValue(VisibleClipsProperty);
            set => SetValue(VisibleClipsProperty, value);
        }

        public bool NoResultsVisible
        {
            get => (bool)GetValue(NoResultsVisibleProperty);
            set => SetValue(NoResultsVisibleProperty, value);
        }

        private static void OnSearchQueryDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchPanelControl)d;
            control.SearchQueryTextBlock.Text = e.NewValue as string ?? "Start typing to search...";
        }

        private static void OnVisibleClipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchPanelControl)d;
            control.SearchResultsGrid.Children.Clear();
            var clips = e.NewValue as IReadOnlyList<string>;
            if (clips is null)
            {
                return;
            }

            foreach (var trigger in clips)
            {
                var button = new System.Windows.Controls.Button
                {
                    Content = trigger,
                    Style = (Style)control.FindResource("ClipButtonStyle")
                };
                button.Click += (_, _) => control.ClipSelected?.Invoke(control, trigger);
                control.SearchResultsGrid.Children.Add(button);
            }
        }

        private static void OnNoResultsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchPanelControl)d;
            var isVisible = e.NewValue is bool b && b;
            control.NoResultsTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
