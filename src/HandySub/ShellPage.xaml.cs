using HandySub.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsUI.ViewModel;
using System;

namespace HandySub.Pages
{
    public sealed partial class ShellPage : Page
    {
        internal static ShellPage Instance { get; set; }
        public ShellViewModel ViewModel { get; } = new ShellViewModel();
        public ShellPage()
        {
            this.InitializeComponent();
            Instance = this;
            ViewModel.InitializeNavigation(shellFrame, navigationView)
                .WithSettingsPage(typeof(SettingsPage))
                .WithDefaultPage(typeof(SubscenePage))
                .WithKeyboardAccelerator(KeyboardAccelerators);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded();

            if (Helper.Settings.IsSoundEnabled)
            {
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
                if (Helper.Settings.IsSpatialSoundEnabled)
                {
                    ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
                }
            }
            FirstStartup();
        }

        private void navigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            ViewModel.OnItemInvoked(args);
        }

        #region TeachingTip
        public void SetEnableNavView(bool isEnabled)
        {
            navigationView.IsEnabled = isEnabled;
        }

        private async void FirstStartup()
        {
            if (Helper.Settings.IsFirstRun)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Step by step guide",
                    Content = "Since this is your first use, please take a few minutes and follow the steps with us. You must complete this guide to use the app",
                    PrimaryButtonText = "Let's go",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = Content.XamlRoot
                };
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    navigationView.IsEnabled = false;
                    tip1.IsOpen = true;
                }
                else
                {
                    Helper.Settings.IsFirstRun = false;
                }
            }
        }
        private void tip1_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip1.IsOpen = false;
            tip2.IsOpen = true;
        }

        private void tip2_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip2.IsOpen = false;
            SubscenePage.Instance.ShowTip1();
        }
        #endregion

    }
}
