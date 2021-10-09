using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Windows.System;
using HandySub.DiffViewer;
using Microsoft.UI;
using HandySub.Common;
using CommunityToolkit.WinUI;
using SettingsUI.Helpers;

namespace HandySub.UserControls
{
    public sealed partial class SideBySideDiffViewer : UserControl, ISideBySideDiffViewer
    {
        public event EventHandler OnCloseEvent;

        private readonly RichTextBlockDiffRenderer _diffRenderer;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly ScrollViewerSynchronizer _scrollSynchronizer;

        public SideBySideDiffViewer()
        {
            InitializeComponent();

            _scrollSynchronizer = new ScrollViewerSynchronizer(new List<ScrollViewer> { LeftScroller, RightScroller });

            _diffRenderer = new RichTextBlockDiffRenderer();

            var color = Application.Current.Resources["SystemAccentColor"];
            var accent = GeneralHelper.GetColorFromHex(color.ToString());

            LeftTextBlock.SelectionHighlightColor = new SolidColorBrush(accent);
            RightTextBlock.SelectionHighlightColor = new SolidColorBrush(accent);

            LeftTextBlockBorder.PointerWheelChanged += LeftTextBlockBorder_PointerWheelChanged;
            RightTextBlockBorder.PointerWheelChanged += RightTextBlockBorder_PointerWheelChanged;

            DismissButton.Click += DismissButton_OnClick;
            Loaded += SideBySideDiffViewer_Loaded;
        }

        public void Dispose()
        {
            StopRenderingAndClearCache();

            DismissButton.Click -= DismissButton_OnClick;
            Loaded -= SideBySideDiffViewer_Loaded;

            LeftTextBlockBorder.PointerWheelChanged -= LeftTextBlockBorder_PointerWheelChanged;
            RightTextBlockBorder.PointerWheelChanged -= RightTextBlockBorder_PointerWheelChanged;

            _scrollSynchronizer.Dispose();
        }

        private void SideBySideDiffViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }

        private void ChangeVerticalScrollingBasedOnMouseInput(PointerRoutedEventArgs args)
        {
            var mouseWheelDelta = args.GetCurrentPoint(this).Properties.MouseWheelDelta;
            RightScroller.ChangeView(null, RightScroller.VerticalOffset + (-1 * mouseWheelDelta), null, false);
        }

        // Ctrl + Shift + Wheel -> horizontal scrolling
        private void ChangeHorizontalScrollingBasedOnMouseInput(PointerRoutedEventArgs args)
        {
            var mouseWheelDelta = args.GetCurrentPoint(this).Properties.MouseWheelDelta;
            RightScroller.ChangeView(RightScroller.HorizontalOffset + (-1 * mouseWheelDelta), null, null, false);
        }
       
        public void Focus()
        {
            RightTextBlock.Focus(FocusState.Programmatic);
        }

        public void StopRenderingAndClearCache()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            LeftTextBlock.TextHighlighters.Clear();
            LeftTextBlock.Blocks.Clear();
            RightTextBlock.TextHighlighters.Clear();
            RightTextBlock.Blocks.Clear();
        }

        public void RenderDiff(string left, string right, ElementTheme theme)
        {
            StopRenderingAndClearCache();

            var foregroundBrush = (theme == ElementTheme.Dark)
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Colors.Black);

            var diffContext = _diffRenderer.GenerateDiffViewData(left, right, foregroundBrush);
            var leftContext = diffContext.Item1;
            var rightContext = diffContext.Item2;
            var leftHighlighters = leftContext.GetTextHighlighters();
            var rightHighlighters = rightContext.GetTextHighlighters();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(async () =>
            {
                var leftCount = leftContext.Blocks.Count;
                var rightCount = rightContext.Blocks.Count;

                var leftStartIndex = 0;
                var rightStartIndex = 0;
                var threshold = 1;

                while (true)
                {
                    Thread.Sleep(10);
                    if (leftStartIndex < leftCount)
                    {
                        var end = leftStartIndex + threshold;
                        if (end >= leftCount) end = leftCount;
                        var start = leftStartIndex;
                        await DispatcherQueue.EnqueueAsync(() => {
                            for (int x = start; x < end; x++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) return;
                                LeftTextBlock.Blocks.Add(leftContext.Blocks[x]);
                            }
                        }, Microsoft.UI.Dispatching.DispatcherQueuePriority.High);
                    }

                    if (rightStartIndex < rightCount)
                    {
                        var end = rightStartIndex + threshold;
                        if (end >= rightCount) end = rightCount;
                        var start = rightStartIndex;
                        await DispatcherQueue.EnqueueAsync(() => {
                            for (int x = start; x < end; x++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) return;
                                RightTextBlock.Blocks.Add(rightContext.Blocks[x]);
                            }
                        }, Microsoft.UI.Dispatching.DispatcherQueuePriority.High);
                    }

                    leftStartIndex += threshold;
                    rightStartIndex += threshold;
                    threshold *= 5;

                    if (leftStartIndex >= leftCount && rightStartIndex >= rightCount)
                    {
                        break;
                    }
                }
            }, cancellationTokenSource.Token);

            Task.Factory.StartNew(async () =>
            {
                var leftCount = leftHighlighters.Count;
                var rightCount = rightHighlighters.Count;

                var leftStartIndex = 0;
                var rightStartIndex = 0;
                var threshold = 5;

                while (true)
                {
                    Thread.Sleep(10);
                    if (leftStartIndex < leftCount)
                    {
                        var end = leftStartIndex + threshold;
                        if (end >= leftCount) end = leftCount;
                        var start = leftStartIndex;
                        await DispatcherQueue.EnqueueAsync(() => {
                            for (int x = start; x < end; x++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) return;
                                LeftTextBlock.TextHighlighters.Add(leftHighlighters[x]);
                            }
                        }, Microsoft.UI.Dispatching.DispatcherQueuePriority.High);
                    }

                    if (rightStartIndex < rightCount)
                    {
                        var end = rightStartIndex + threshold;
                        if (end >= rightCount) end = rightCount;
                        var start = rightStartIndex;
                        await DispatcherQueue.EnqueueAsync(() => {
                            for (int x = start; x < end; x++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested) return;
                                RightTextBlock.TextHighlighters.Add(rightHighlighters[x]);
                            }
                        }, Microsoft.UI.Dispatching.DispatcherQueuePriority.High);
                    }

                    leftStartIndex += threshold;
                    rightStartIndex += threshold;
                    threshold *= 5;

                    if (leftStartIndex >= leftCount && rightStartIndex >= rightCount)
                    {
                        break;
                    }
                }
            }, cancellationTokenSource.Token);

            _cancellationTokenSource = cancellationTokenSource;
        }

        private void DismissButton_OnClick(object sender, RoutedEventArgs e)
        {
            StopRenderingAndClearCache();
            OnCloseEvent?.Invoke(this, EventArgs.Empty);
        }

        private void LeftTextBlockBorder_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // Always handle it so that left ScrollViewer won't pick up the event
            e.Handled = true;
        }

        private void RightTextBlockBorder_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            // Always handle it so that right ScrollViewer won't pick up the event
            e.Handled = true;
        }
    }
}