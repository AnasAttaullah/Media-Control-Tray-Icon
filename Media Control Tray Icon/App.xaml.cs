using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media.Control;
using Wpf.Ui.Controls;
using Wpf.Ui.Tray.Controls;

namespace Media_Control_Tray_Icon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private NotifyIcon trayIcon;
        private ImageSource? pauseDarkIcon;
        private ImageSource? playDarkIcon;

        public GlobalSystemMediaTransportControlsSessionManager? MediaSessionManager { get; set; }
        public GlobalSystemMediaTransportControlsSession? CurrentSession { get; set; }
        public GlobalSystemMediaTransportControlsSessionPlaybackInfo PlaybackInfo { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            trayIcon = (NotifyIcon)FindResource("trayIcon");

            pauseDarkIcon = LoadTrayIcon("icons/pauseDark.ico");
            playDarkIcon = LoadTrayIcon("icons/playDark.ico");

            try
            {
                MediaSessionManager = await InitializeMediaSessionAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Startup Error");
                Shutdown();
            }

            CurrentSession = MediaSessionManager.GetCurrentSession();
            PlaybackInfo = CurrentSession.GetPlaybackInfo();

            // Events
            trayIcon.LeftClick += TrayIcon_LeftClickAsync;
            trayIcon.LeftDoubleClick += TrayIcon_LeftDoubleClickAsync;
            trayIcon.RightClick += TrayIcon_RightClick;
            CurrentSession.PlaybackInfoChanged += CurrentSession_PlaybackInfoChanged;
            CurrentSession.MediaPropertiesChanged += CurrentSession_MediaPropertiesChanged;

            // Registering the TrayIcon
            if (MainWindow is not null)
            {
                MainWindow.Loaded += MainWindow_Loaded;
            }
            else
            {
                Dispatcher.BeginInvoke(RegisterTrayIcon, DispatcherPriority.ApplicationIdle);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon?.Dispose();
            base.OnExit(e);
        }

        // METHODS
        private async Task<GlobalSystemMediaTransportControlsSessionManager> InitializeMediaSessionAsync()
        {
            return await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        }

        private void RegisterTrayIcon()
        {
            if (!trayIcon.IsRegistered)
            {
                trayIcon.Register();
            }
        }

        private static ImageSource LoadTrayIcon(string relativePath)
        {
            var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
            var image = BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            image.Freeze();
            return image;
        }

        private void UpdateTrayIcon()
        {
            Dispatcher.Invoke(() =>
            {
                trayIcon.Icon = PlaybackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing
                    ? pauseDarkIcon
                    : playDarkIcon;
            });
        }

        // EVENT HANDLERS

        private async void TrayIcon_LeftClickAsync([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            await CurrentSession.TryTogglePlayPauseAsync();
        }

        private async void TrayIcon_LeftDoubleClickAsync([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            await CurrentSession.TrySkipNextAsync();
        }

        private void TrayIcon_RightClick([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Right mouse click");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterTrayIcon();
        }

        private void CurrentSession_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            // update the details on the popup
        }

        private void CurrentSession_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            PlaybackInfo = CurrentSession.GetPlaybackInfo();
            UpdateTrayIcon();
            Debug.WriteLine(PlaybackInfo.PlaybackStatus.ToString());
        }
    }
}
