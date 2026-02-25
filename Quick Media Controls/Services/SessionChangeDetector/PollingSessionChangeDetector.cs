using System;
using System.Diagnostics;
using System.Threading;
using Windows.Media.Control;

namespace Quick_Media_Controls.Services.SessionChangeDetector
{
    /// <summary>
    /// Windows 10 session change detector using polling strategy
    /// </summary>

    internal class PollingSessionChangeDetector : ISessionChangeDetector
    {
        private readonly GlobalSystemMediaTransportControlsSessionManager _sessionManager;
        private readonly Action<GlobalSystemMediaTransportControlsSession?> _onSessionChanged;
        private Timer? _pollTimer;

        public PollingSessionChangeDetector(GlobalSystemMediaTransportControlsSessionManager sessionManager , Action<GlobalSystemMediaTransportControlsSession?> onSessionChanged)
        {
            _sessionManager = sessionManager;
            _onSessionChanged = onSessionChanged;
        }

        public void Start()
        {
            _pollTimer = new Timer(CheckForSessionChange, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void CheckForSessionChange(object? state)
        {
            try
            {
                var newSession = _sessionManager?.GetCurrentSession();
                _onSessionChanged.Invoke(newSession);
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error in polling session change: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _pollTimer.Dispose();
        }

    }
}
