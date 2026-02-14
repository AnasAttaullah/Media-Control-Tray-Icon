using System;

namespace Media_Control_Tray_Icon.Services.SessionChangeDetector
{
    internal interface ISessionChangeDetector : IDisposable
    {
        void Start();
    }
}
