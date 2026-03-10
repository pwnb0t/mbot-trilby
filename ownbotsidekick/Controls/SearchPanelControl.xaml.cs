using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ownbotsidekick.Search;
using ownbotsidekick.Services;

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
            typeof(IReadOnlyList<ClipSearchResult>),
            typeof(SearchPanelControl),
            new PropertyMetadata(Array.Empty<ClipSearchResult>(), OnVisibleClipsChanged)
        );

        public static readonly DependencyProperty NoResultsVisibleProperty = DependencyProperty.Register(
            nameof(NoResultsVisible),
            typeof(bool),
            typeof(SearchPanelControl),
            new PropertyMetadata(false, OnNoResultsVisibleChanged)
        );

        public event EventHandler<string>? ClipSelected;
        public event EventHandler<bool>? ClipDragStateChanged;

        public SearchPanelControl()
        {
            InitializeComponent();
        }

        public string SearchQueryDisplay
        {
            get => (string)GetValue(SearchQueryDisplayProperty);
            set => SetValue(SearchQueryDisplayProperty, value);
        }

        public IReadOnlyList<ClipSearchResult> VisibleClips
        {
            get => (IReadOnlyList<ClipSearchResult>)GetValue(VisibleClipsProperty);
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
            var clips = e.NewValue as IReadOnlyList<ClipSearchResult>;
            if (clips is null)
            {
                return;
            }

            foreach (var clip in clips)
            {
                var button = new System.Windows.Controls.Button
                {
                    Content = CreateHighlightedContent(clip),
                    Style = (Style)control.FindResource("ClipButtonStyle"),
                    Tag = clip.Trigger
                };
                button.Click += (_, _) => control.ClipSelected?.Invoke(control, clip.Trigger);
                button.PreviewMouseMove += control.SearchResultButton_PreviewMouseMove;
                control.SearchResultsGrid.Children.Add(button);
            }
        }

        private static void OnNoResultsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SearchPanelControl)d;
            var isVisible = e.NewValue is bool b && b;
            control.NoResultsTextBlock.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private static System.Windows.Controls.TextBlock CreateHighlightedContent(ClipSearchResult clip)
        {
            var textBlock = new System.Windows.Controls.TextBlock();
            var accentBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("AccentBrush");
            foreach (var segment in clip.Segments)
            {
                var run = new Run(segment.Text);
                if (segment.IsMatch)
                {
                    run.FontWeight = FontWeights.SemiBold;
                    run.Foreground = accentBrush;
                }

                textBlock.Inlines.Add(run);
            }

            return textBlock;
        }

        private void SearchResultButton_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (
                sender is not System.Windows.Controls.Button button ||
                button.Tag is not string trigger ||
                string.IsNullOrWhiteSpace(trigger)
            )
            {
                return;
            }

            ClipDragStateChanged?.Invoke(this, true);
            try
            {
                var dragData = new System.Windows.DataObject(QuickPlayDragDrop.ClipTriggerFormat, trigger);
                DragDrop.DoDragDrop(button, dragData, System.Windows.DragDropEffects.Copy);
            }
            finally
            {
                ClipDragStateChanged?.Invoke(this, false);
            }
        }
    }
}
