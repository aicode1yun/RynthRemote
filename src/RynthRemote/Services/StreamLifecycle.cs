namespace RynthRemote.Services;

/// <summary>
/// Bridges MAUI app-resume to the live-view page. iOS drops a long-lived MJPEG connection when the
/// app backgrounds; on foreground we raise this so the page bumps a stream "epoch" and the &lt;img&gt;
/// reconnects (instead of showing a dead/tiny frame).
/// </summary>
public static class StreamLifecycle
{
    public static event Action? Resumed;
    public static void RaiseResumed() => Resumed?.Invoke();

    /// Raised when the app backgrounds/resigns active. The live-view page uses this to release any
    /// held movement key — otherwise a d-pad button pressed when the app is backgrounded would never
    /// see its pointer-up and the character would keep moving.
    public static event Action? Suspended;
    public static void RaiseSuspended() => Suspended?.Invoke();
}
