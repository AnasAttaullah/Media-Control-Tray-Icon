using Media_Control_Tray_Icon.Services;
using System;
using System.Diagnostics;
using System.Drawing;
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
        private ImageSource pauseDarkIcon;
        private ImageSource playDarkIcon;

        private MediaSessionService _mediaService;
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            trayIcon = (NotifyIcon)FindResource("trayIcon");

            pauseDarkIcon = LoadTrayIcon("icons/pauseDark.ico");
            playDarkIcon = LoadTrayIcon("icons/playDark.ico");

            try
            {
                _mediaService = new MediaSessionService();
                await _mediaService.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Startup Error");
                Shutdown();
            }


            // Events
            trayIcon.LeftClick += TrayIcon_LeftClickAsync;
            trayIcon.LeftDoubleClick += TrayIcon_LeftDoubleClickAsync;
            trayIcon.RightClick += TrayIcon_RightClick;
            _mediaService.PlaybackInfoChanged += MediaService_PlaybackInfoChanged;
            _mediaService.MediaPropertiesChanged += MediaService_MediaPropertiesChanged;

            // Registering the TrayIcon
            if (MainWindow is not null)
            {
                MainWindow.Loaded += MainWindow_Loaded;
            }
            else
            {
                Dispatcher.BeginInvoke(RegisterTrayIcon, DispatcherPriority.ApplicationIdle);
            }
            UpdateTrayIcon();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mediaService.Dispose();
            trayIcon?.Dispose();
            base.OnExit(e);
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
                trayIcon.Icon = _mediaService.IsPlaying() ? pauseDarkIcon : playDarkIcon;
            });
        }

        // EVENT HANDLERS

        private async void TrayIcon_LeftClickAsync([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            await _mediaService.TogglePlayPauseAsync();
        }

        private async void TrayIcon_LeftDoubleClickAsync([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            await _mediaService.SkipNextAsync();
        }
         
        private void TrayIcon_RightClick([System.Diagnostics.CodeAnalysis.NotNull] NotifyIcon sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Right mouse click");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterTrayIcon();
        }
        private void MediaService_MediaPropertiesChanged(object? sender, EventArgs e)
        {
            // update the details on the popup
        }

        private void MediaService_PlaybackInfoChanged(object? sender, Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackInfo e)
        {
            UpdateTrayIcon();
            Debug.WriteLine(e.PlaybackStatus.ToString());
        }
    }
}
