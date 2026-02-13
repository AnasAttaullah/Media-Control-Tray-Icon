using System;
using System.Security.Policy;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Media_Control_Tray_Icon.Services
{
    public class MediaSessionService : IDisposable
    {
        public GlobalSystemMediaTransportControlsSessionManager? SessionManager { get; private set; }
        public GlobalSystemMediaTransportControlsSession? CurrentSession { get; private set; }
        public GlobalSystemMediaTransportControlsSessionPlaybackInfo? CurrentPlaybackInfo { get; private set; }

        public event EventHandler<GlobalSystemMediaTransportControlsSessionPlaybackInfo>? PlaybackInfoChanged;
        public event EventHandler? MediaPropertiesChanged;
        
        // Methods
        public async Task InitializeAsync()
        {
            SessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync(); 
            CurrentSession = SessionManager.GetCurrentSession();

            if(CurrentSession != null)
            {
                CurrentPlaybackInfo = CurrentSession.GetPlaybackInfo();
                CurrentSession.PlaybackInfoChanged += OnCurrentSession_PlaybackInfoChanged;
                CurrentSession.MediaPropertiesChanged += OnCurrentSession_MediaPropertiesChanged;
            }
        }

        public async Task TogglePlayPauseAsync()
        {
            if(CurrentSession != null)
            {
                await CurrentSession.TryTogglePlayPauseAsync();
            }
        }
        public async Task SkipNextAsync()
        {
            if(CurrentSession != null)
            {
                await CurrentSession.TrySkipNextAsync();
            }
        }
        public async Task SkipPreviousAsync()
        {
            if(CurrentSession != null)
            {
                await CurrentSession.TrySkipPreviousAsync();
            }
        }
        public bool IsPlaying()
        {
            return CurrentPlaybackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }

        // Event Handlers 
        private void OnCurrentSession_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            MediaPropertiesChanged?.Invoke(this, EventArgs.Empty); 
        }

        private void OnCurrentSession_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            CurrentPlaybackInfo = CurrentSession.GetPlaybackInfo();
            if(CurrentPlaybackInfo != null)
            {
                PlaybackInfoChanged?.Invoke(this, CurrentPlaybackInfo);
            }
        }

        // On Dispose
        public void Dispose()
        {
           if(CurrentSession != null)
           {
               CurrentSession.PlaybackInfoChanged -= OnCurrentSession_PlaybackInfoChanged;
               CurrentSession.MediaPropertiesChanged -= OnCurrentSession_MediaPropertiesChanged;
           }
        }
    }
}
