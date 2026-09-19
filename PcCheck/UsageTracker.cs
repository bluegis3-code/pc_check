namespace PCCheck;
internal sealed class UsageTracker : IDisposable
{
    readonly System.Windows.Forms.Timer timer=new(){Interval=10000};
    DateTime last=DateTime.Now; AppSettings settings; readonly UsageData data;
    readonly SessionRecord session;
    public bool Running {get;private set;}=true;
    public event Action? Updated;
    public UsageTracker(AppSettings s,UsageData d){settings=s;data=d;session=data.StartSession();timer.Tick+=Tick;timer.Start();}
    void Tick(object? sender,EventArgs e){var now=DateTime.Now;var sec=Math.Min(15,(now-last).TotalSeconds);last=now;data.Heartbeat(session);if(Running)data.Add(Native.ActiveProcess(),sec);Updated?.Invoke();}
    public void Pause(){Running=false;} public void Resume(){last=DateTime.Now;Running=true;} public void UpdateSettings(AppSettings s)=>settings=s;
    public void Dispose(){timer.Dispose();data.EndSession(session);}
}
