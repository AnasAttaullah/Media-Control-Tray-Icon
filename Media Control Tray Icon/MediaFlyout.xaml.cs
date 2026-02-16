using Media_Control_Tray_Icon.Services;
using System;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Media_Control_Tray_Icon
{
    /// <summary>
    /// Interaction logic for MediaFlyout.xaml
    /// </summary>
    public partial class MediaFlyout : FluentWindow
    {
        private readonly MediaSessionService _sessionManager;
        public MediaFlyout(MediaSessionService sessionManager)
        {
            ApplicationThemeManager.ApplySystemTheme();
            ApplicationAccentColorManager.ApplySystemAccent();
            _sessionManager = sessionManager;
            Left = SystemParameters.WorkArea.Right - 300 - 110;
            Top = SystemParameters.WorkArea.Bottom - 130;
            InitializeComponent();
            UpdateIcon();
        }

        public void UpdateIcon()
        {
            if(_sessionManager.CurrentSession == null)
            {
                //PlaybackButtonsGrid.IsEnabled = false;
                return;
            }
            playPauseIcon.Symbol = (_sessionManager.IsPlaying()) ? SymbolRegular.Pause12 : SymbolRegular.Play12;
        }



        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private async void PreviousButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _sessionManager.SkipPreviousAsync();
        }
        private async void PlayPauseButton_ClickAsync(object sender, RoutedEventArgs e)
        {
           await _sessionManager.TogglePlayPauseAsync();
        }
        private async void NextButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            await _sessionManager.SkipNextAsync();
        }

        private void Flyout_Deactivated(object sender, EventArgs e)
        {
                Hide();
             }



        internal void showFlyout()
        {

            // Make visible first
            this.Visibility = Visibility.Visible;

            // Temporarily toggle Topmost to force Windows to treat it as foreground
            this.Topmost = true;
            this.Topmost = false;
            this.Topmost = true;

            // Activate + Focus
            this.Activate();
            this.Focus();

            Keyboard.Focus(this);

        }

        //private void FluentWindow_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    //Hide(); // I think by commeting this the sudden UI flyout Close is not happening anymore 
        //}
    }
}
