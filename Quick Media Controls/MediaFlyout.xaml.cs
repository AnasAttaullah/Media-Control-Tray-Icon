using Quick_Media_Controls.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Quick_Media_Controls
{
    public partial class MediaFlyout : FluentWindow
    {
        private readonly MediaSessionService _sessionManager;
        private bool _IsDragEnabled;

        private string? _cachedThumbnailKey;
        private BitmapImage? _cachedThumbnail;

        public MediaFlyout(MediaSessionService sessionManager)
        {
            ApplicationThemeManager.ApplySystemTheme();
            ApplicationAccentColorManager.ApplySystemAccent();

            _IsDragEnabled = false;
            _sessionManager = sessionManager;

            Left = SystemParameters.WorkArea.Right - 300 - 110;
            Top = SystemParameters.WorkArea.Bottom - 130;

            InitializeComponent();
            UpdateIcons();
        }

        public void UpdateIcons()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(UpdateIcons);
                return;
            }
            if (_sessionManager.CurrentSession == null) return;
            playPauseIcon.Symbol = _sessionManager.IsPlaying() ? SymbolRegular.Pause12 : SymbolRegular.Play12;
        }

        public void ShowFlyout()
        {
            this.Visibility = Visibility.Visible;

            // Force topmost activation workaround for WPF
            this.Topmost = true;
            this.Topmost = false;
            this.Topmost = true;

            this.Activate();
            this.Focus();
            Keyboard.Focus(this);

            UpdateMediaInfo();
        }

        public async void UpdateMediaInfo()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(UpdateMediaInfo);
                return;
            }

            // Skip all work (including async thumbnail I/O) when not visible
            if (Visibility != Visibility.Visible) return;

            if (_sessionManager.CurrentMediaProperties != null)
            {
                if (mediaPlayingGrid.Visibility != Visibility.Visible)
                {
                    mediaPlayingGrid.Visibility = Visibility.Visible;
                    noMediaPlayingGrid.Visibility = Visibility.Collapsed;
                }

                var mediaTitle = _sessionManager.CurrentMediaProperties.Title;
                playingMediaTitle.Text = mediaTitle.Length > 35 ? mediaTitle[..32] + "..." : mediaTitle;
                playingMediaArtist.Text = _sessionManager.CurrentMediaProperties.Artist;

                var thumbnail = await LoadMediaThumbnailAsync(_sessionManager.CurrentMediaProperties.Thumbnail);
                playingMediaThumbnail.Source = thumbnail;
            }
            else
            {
                mediaPlayingGrid.Visibility = Visibility.Collapsed;
                noMediaPlayingGrid.Visibility = Visibility.Visible;
            }
        }

        private async Task<BitmapImage?> LoadMediaThumbnailAsync(Windows.Storage.Streams.IRandomAccessStreamReference? thumbnailRef)
        {
            if (thumbnailRef == null)
                return null;

            var key = $"{_sessionManager.CurrentMediaProperties?.Title}|{_sessionManager.CurrentMediaProperties?.Artist}";
            if (_cachedThumbnailKey == key && _cachedThumbnail != null)
                return _cachedThumbnail;

            try
            {
                using var stream = await thumbnailRef.OpenReadAsync();
                if (stream == null || stream.Size == 0)
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelHeight = 90; // Decode at display size ( image box is 90x90 )

                using var memStream = new MemoryStream();
                await stream.AsStreamForRead().CopyToAsync(memStream);
                memStream.Position = 0;

                bitmap.StreamSource = memStream;
                bitmap.EndInit();
                bitmap.Freeze();

                _cachedThumbnailKey = key;
                _cachedThumbnail = bitmap;
                return bitmap;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load thumbnail: {ex.Message}");
                return null;
            }
        }

        private async void PlayPauseButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _sessionManager.TogglePlayPauseAsync();
        }

        private async void NextButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _sessionManager.SkipNextAsync();
        }

        private async void PreviousButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _sessionManager.SkipPreviousAsync();
        }

        private void GithubMenuItem_Click(object sender, RoutedEventArgs e)
        {
            const string url = "https://github.com/AnasAttaullah/Quick-Media-Controls";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open GitHub link: {ex}");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed && _IsDragEnabled)
                DragMove();
        }

        private void MoveWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _IsDragEnabled = !_IsDragEnabled;
            Cursor = _IsDragEnabled ? Cursors.SizeAll : Cursors.Arrow;
        }

        private void Flyout_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }
    }
}