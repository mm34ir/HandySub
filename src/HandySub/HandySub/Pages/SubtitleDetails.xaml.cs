using HandySub.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace HandySub.Pages
{
    public sealed partial class SubtitleDetails : Page
    {
        string before = string.Empty;
        string after = string.Empty;
        public SubtitleDetails()
        {
            this.InitializeComponent();
        }

        #region OpenFile

        private async Task<string> OpenSubtitle()
        {
            var main = MainWindow.Instance;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(main);
            
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".srt");
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return file.Path;
            }
            return null;
        }

        #endregion

        private async void btnSub1_Click(object sender, RoutedEventArgs e)
        {
            var path = await OpenSubtitle();
            if (path != null)
            {
                var srtModel = SrtHelper.ParseSrt(path);
                var first = srtModel.FirstOrDefault();
                var last = srtModel.LastOrDefault();
                txtBeginTime1.Text = $"Begin Time: {first.BeginHour}:{first.BeginMintue}:{first.BeginSecond}:{first.BeginMSecond}";
                txtEndTime1.Text = $"End Time: {last.EndHour}:{last.EndMintue}:{last.EndSecond}:{last.EndMSecond}";
                before = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(before) && !string.IsNullOrEmpty(after))
                {
                    sidebySideDiff.RenderDiff(before, after, ElementTheme.Default);
                }
            }
        }
        private async void btnSub2_Click(object sender, RoutedEventArgs e)
        {
            var path = await OpenSubtitle();
            if (path != null)
            {
                var srtModel = SrtHelper.ParseSrt(path);
                var first = srtModel.FirstOrDefault();
                var last = srtModel.LastOrDefault();
                txtBeginTime2.Text = $"Begin Time: {first.BeginHour}:{first.BeginMintue}:{first.BeginSecond}:{first.BeginMSecond}";
                txtEndTime2.Text = $"End Time: {last.EndHour}:{last.EndMintue}:{last.EndSecond}:{last.EndMSecond}";

                after = File.ReadAllText(path);
                ShowDiff();
            }
        }

        private void ShowDiff()
        {
            if (!string.IsNullOrEmpty(before) && !string.IsNullOrEmpty(after))
            {
                if (Helper.Settings.ApplicationTheme == ElementTheme.Dark)
                {
                    sidebySideDiff.RenderDiff(before, after, ElementTheme.Dark);
                }
                else
                {
                    sidebySideDiff.RenderDiff(before, after, ElementTheme.Light);
                }
            }
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ShowDiff();
        }

        #region Drog and Drop
        private void StackPanel_Left_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Drop First Subtitle Here";
        }
        private void StackPanel_Right_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Drop Second Subtitle Here";
        }
        private async void StackPanel_Left_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && (await e.DataView.GetStorageItemsAsync()).Count == 1)
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var storageFile = items[0] as StorageFile;
                if (storageFile.FileType.Contains("txt") || storageFile.FileType.Contains("srt"))
                {
                    before = await FileIO.ReadTextAsync(storageFile);
                    ShowDiff();
                }
            }
        }

        private async void StackPanel_Right_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && (await e.DataView.GetStorageItemsAsync()).Count == 1)
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var storageFile = items[0] as StorageFile;
                if (storageFile.FileType.Contains("txt") || storageFile.FileType.Contains("srt"))
                {
                    before = await FileIO.ReadTextAsync(storageFile);
                    ShowDiff();
                }
            }
        }
        #endregion
    }
}
